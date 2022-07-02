using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading;
using System.IO;
using System.Drawing.Imaging;
using System.Windows.Threading;
using System.Diagnostics;

using AForge.Video;
using AForge.Video.DirectShow;

using BottleBusiness.RandomForest;
using BottleBusiness.BottleBusiness;

using Alturos.Yolo;
using Alturos.Yolo.Model;

using NumSharp;

using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using BottleVisionApp.BottleBusiness;

namespace BottleVisionApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        // Classes and variable for AForge Camera Access
        public ObservableCollection<FilterInfo> VideoDevices { get; set; }
        public FilterInfo CurrentDevice { get; set; }
        private IVideoSource _videoSource;

        // Initialize Object Detection with Yolov3
        public static YoloWrapper yoloWrapper = new YoloWrapper("yolov3.cfg", "yolov3.weights", "coco.names");

        // Using Stopwatch to make the application robust against 
        // - false detection in one frame
        // - someone putting the bottle back in place
        public static Stopwatch stopwatch = new Stopwatch();
        public static bool timerIsRunning = false;

        // Is used to save every frame of the camera but only process the last one taken
        public static List<Bitmap> frames = new List<Bitmap>(); // use Circular Buffer instead

        // lets the user decide if the boundary boxes are shown
        public static bool addBoxes = false;

        // Initializes shop before business processes start
        public static bool shopIsInitialized = false;
        public static Business business;

        // this thread runs the detection process parallel to the application
        public static Thread detection_thread;

        // dimension of the tensor needed to feed ONNX
        public static int[] dimensions = new int[] { 1, 192, 192, 3 };

        // Initialize options for ONNX session
        public static string pred = "";
        public static string onnxModelPath = @"C:\Users\bayan2\Desktop\PythonCode\model.onnx";
        public static SessionOptions options = new SessionOptions();
        public static InferenceSession session;

        // Initialize new Status Windows
        public static Status statusWindow = new Status();

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            GetVideoDevices();
            this.Closing += MainWindow_Closing;

            // Set Inference Session and Graph optimization level for options (onnx)
            options.GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_EXTENDED;
            session = new InferenceSession(onnxModelPath, options);
        }

        /// <summary>
        /// Start Button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></paradm>
        private void StartBtn_Click(object sender, RoutedEventArgs e)
        {
            StartCamera();
            //Detect and process image parallel in another thread
            detection_thread = new Thread(LoopDetection);
            detection_thread.Start();
        }

        /// <summary>
        /// Stop Button    
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StopBtn_Click(object sender, RoutedEventArgs e)
        {
            StopCamera();
        }

        /// <summary>
        /// Loads camera frames to application and saves it in List
        /// https://github.com/mesta1/AForge-examples
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void Video_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            try
            {
                // Add frames in Array
                BitmapImage bi;
                using var bitmap = (Bitmap)eventArgs.Frame.Clone();
                bi = ToBitmapImage(bitmap);
                
                // save all frames in List
                frames.Add((Bitmap)bitmap.Clone());
                bitmap.Dispose();
            
                bi.Freeze(); // avoid cross thread operations and prevents leaks
                Dispatcher.BeginInvoke(new ThreadStart(delegate {
                    videoPlayer.Source = bi;
                }));
            }
            catch (Exception exc)
            {
                MessageBox.Show("Error on _videoSource_NewFrame:\n" + exc.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StopCamera();
            }
        }

        /// <summary>
        /// Start detection and processing 
        /// </summary>
        private void StartDetection()
        {
            try
            {
                // Get last image saved in the list(actual frame)
                BitmapImage processedImage;
                Image videoPlayerImage= (Image)frames[frames.Count() - 1].Clone();
                byte[] videoPlayerByte = ImageToByte(videoPlayerImage);
                frames.Clear();

                List<YoloItem> items = yoloWrapper.Detect(videoPlayerByte).ToList();
                List<int> detectedBottles = ListOfBottles(videoPlayerImage, items);

                if (shopIsInitialized)
                {
                    UpdateInventory(detectedBottles);
                }
                else
                {
                    InitializeBusiness(detectedBottles);
                }

                // adds boxes if checkbox is clicked
                if (addBoxes)
                {
                    Image img = new ImageProcessor().AddBox(videoPlayerImage, items);
                    processedImage = ToBitmapImage((Bitmap)img);
                }
                else
                {
                    processedImage = ToBitmapImage((Bitmap)videoPlayerImage);
                }

                // show image
                processedImage.Freeze(); // avoid cross thread operations and prevents leaks
                Dispatcher.BeginInvoke(new ThreadStart(delegate {
                    detectionPlayer.Source = processedImage;
                    UpdateDataGrid();
                }));
            }
            catch (Exception)
            {
                // no exception handeling yet. 
            }
        }

        /// <summary>
        /// Converts Image to byte array
        /// https://stackoverflow.com/questions/7350679/convert-a-bitmap-into-a-byte-array
        /// </summary>
        /// <param name="img"></param>
        /// <returns>byte array from image</returns>
        private byte[] ImageToByte(Image img)
        {
            ImageConverter converter = new ImageConverter();
            return (byte[])converter.ConvertTo(img, typeof(byte[]));
        }

        /// <summary>
        /// Updates the Inventory based on detection data.
        /// Uses stopwatch to be sure that changes really happened.
        /// </summary>
        /// <param name="detectedBottles"></param>
        private void UpdateInventory(List<int> detectedBottles)
        {
            bool amountOfBottlesChanged = !business.IsSameAmount(detectedBottles);
            if (amountOfBottlesChanged)
            {
                if (!timerIsRunning)
                {
                    stopwatch.Start();
                    timerIsRunning = true;
                }
            }

            if (stopwatch.ElapsedMilliseconds >= 6000)
            {
                CheckAmountOfBottlesAndUpdate(amountOfBottlesChanged, detectedBottles);
                stopwatch.Reset();
                timerIsRunning = false;
            }
        }

        /// <summary>
        /// Checks if the amount of bottles changed and update storages
        /// </summary>
        /// <param name="amountOfBottlesChanged"></param>
        /// <param name="detectedBottles"></param>
        private void CheckAmountOfBottlesAndUpdate(bool amountOfBottlesChanged, List<int> detectedBottles)
        {
            if (amountOfBottlesChanged)
            {
                business.UpdateStorages(detectedBottles);
                AlertIfNumOfBottlesLowThread();
            }
        }

        /// <summary>
        /// Show Alert if number fo bottles is too low in any storage
        /// </summary>
        private void AlertIfNumOfBottlesLowThread()
        {
            Dispatcher.BeginInvoke(new ThreadStart(delegate
            {
                AlertIfNumOfBottlesLow();
            }));
        }

        /// <summary>
        /// Updates and shows the number of sold bottles in the new opened window
        /// </summary>
        private void UpdateStatus()
        {
            Dispatcher.BeginInvoke(new ThreadStart(delegate
            {
                statusWindow.status.Items.Clear();
                try
                {
                    List<BottleType> bottleTypes = business.GetBottleList();
                    bottleTypes.RemoveAll(bottleType => business.GetOverallSoldBottles(bottleType).Equals("0"));
                    bottleTypes.ForEach(bottleType => statusWindow.status.Items.Add(new StatusData
                    {
                        type = bottleType.ToString(),
                        amount = business.GetOverallSoldBottles(bottleType),
                        price = business.GetBottlePrice(bottleType)
                    }));
                    statusWindow.income.Text = business.GetBudgetMsg();
                    statusWindow.Show();
                }
                catch(Exception)
                {
                    MessageBox.Show("Please start the application before opening the status");
                }
            }));
        }

        /// <summary>
        /// Initialize Business
        /// </summary>
        /// <param name="detectedBottles"></param>
        private void InitializeBusiness(List<int> detectedBottles)
        {
            Dispatcher.BeginInvoke(new ThreadStart(delegate
            {
                // This information should come from the data base
                // it will make the code more flexible => changes in code needed!
                List<int> initWarehouse = new List<int>{10, 10, 10};
                business = new Business(detectedBottles, initWarehouse);
            }));
            shopIsInitialized = true;
        }

        /// <summary>
        /// Show a Message Box to the user if one of the storages 
        /// has no bottles of any bottle type
        /// </summary>
        private void AlertIfNumOfBottlesLow()
        {
            if (business.NeedBottlesInShop())
            {
                MessageBox.Show("Please fill up the bottles in the Shop.");
            }

            if (business.NeedBottlesInWarehouse())
            {
                MessageBox.Show("Please fill up the bottles in the Warehouse.");
            }
        }

        /// <summary>
        /// Get Video devices
        /// https://github.com/mesta1/AForge-examples
        /// </summary>
        private void GetVideoDevices()
        {
            VideoDevices = new ObservableCollection<FilterInfo>();
            foreach (FilterInfo filterInfo in new FilterInfoCollection(FilterCategory.VideoInputDevice))
            {
                VideoDevices.Add(filterInfo);
            }
            if (VideoDevices.Any())
            {
                CurrentDevice = VideoDevices[0];
            }
            else
            {
                MessageBox.Show("No video sources found", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Start camera
        /// https://github.com/mesta1/AForge-examples
        /// </summary>
        private void StartCamera()
        {
            if (CurrentDevice != null)
            {
                _videoSource = new VideoCaptureDevice(CurrentDevice.MonikerString);
                _videoSource.NewFrame += Video_NewFrame;
                _videoSource.Start();
            }
        }

        /// <summary>
        /// Stop Camera
        /// https://github.com/mesta1/AForge-examples
        /// </summary>
        private void StopCamera()
        {
            if (_videoSource != null && _videoSource.IsRunning)
            {
                _videoSource.SignalToStop();
                _videoSource.NewFrame -= new NewFrameEventHandler(Video_NewFrame);
            }
        }

        /// <summary>
        /// Close Main Window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StopCamera();
        }

        /// <summary>
        /// Get information if the user wants to see the boxes around the detected bottles
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)checkBox.IsChecked)
            {
                addBoxes = true;
            }
            else
            {
                addBoxes = false;
            }
        }

        /// <summary>
        /// Converts Bitmap to BitmapImage
        /// https://stackoverflow.com/questions/6484357/converting-bitmapimage-to-bitmap-and-vice-versa
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        private BitmapImage ToBitmapImage(Bitmap bitmap)
        {
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            MemoryStream ms = new MemoryStream();
            bitmap.Save(ms, ImageFormat.Bmp);
            ms.Seek(0, SeekOrigin.Begin);
            bi.StreamSource = ms;
            bi.EndInit();
            return bi;
        }

        /// <summary>
        /// Extracts from Image and List of YoloItems
        /// an int array of the amount of each type of bottle
        /// </summary>
        /// <param name="image"></param>
        /// <param name="items"></param>
        /// <returns></returns>
        private List<int> ListOfBottles(Image image, List<YoloItem> items)
        {
            //Initialize Dictionary: Keys are bottle types, all values are set 0
            Dictionary<BottleType, int> detectedBottles = new Dictionary<BottleType, int>();
            foreach (BottleType bottleType in (BottleType[])Enum.GetValues(typeof(BottleType)))
            {
                detectedBottles.Add(bottleType, 0);
            }

            foreach (YoloItem item in items)
            {
                if (item.Type.Equals("bottle"))
                {
                    //Extract bottle
                    var rect = new Rectangle(item.X, item.Y, item.Width, item.Height);
                    Image bottle = ((Bitmap)image).Clone(rect, image.PixelFormat);

                    // Predict bottle type
                    int prediction = GetOnnxPrediction(bottle);

                    // Add 1 to the counter of the detected bottle
                    detectedBottles[(BottleType)prediction]++;
                }
            }
            return detectedBottles.Values.ToList();
        }

        /// <summary>
        /// Run ONNX model and get the index of the predicted bottle type 
        /// https://github.com/microsoft/onnxruntime/blob/master/docs/CSharp_API.md
        /// </summary>
        /// <param name="bottle"></param>
        /// <returns></returns>
        private int GetOnnxPrediction(Image bottle)
        {
            // Resize bottle
            Bitmap resizedBottle = Resize(bottle, 192, 192);

            // Convert Image to Array for onnx model
            NDArray imageArray = resizedBottle.ToNDArray(flat: true); // may be simplified...
            float[] tensorArray = ImageArrayToTensorArray(imageArray);

            // Build Container for running the session
            var inputMeta = session.InputMetadata;
            var container = new List<NamedOnnxValue>();
            foreach (var name in inputMeta.Keys)
            {
                var tensor = new DenseTensor<float>(tensorArray, dimensions);
                container.Add(NamedOnnxValue.CreateFromTensor<float>(name, tensor));
            }

            // Run the inference
            using (var results = session.Run(container))
            {
                // get prediction                           
                foreach (var r in results)
                {
                    pred = r.AsTensor<float>().GetArrayString();
                }
            }
            return PredictionFromString(pred);
        }

        /// <summary>
        /// Convert string array (from onnx prediction) to float array 
        /// and extract index of highest value .
        /// </summary>
        /// <param name="pred"></param>
        /// <returns>Index of max value</returns>
        private int PredictionFromString(string pred)
        {
            string[] predArray = ExtractStringListFromString(pred);
            float[] predArrayFloat = new float[predArray.Length];
            for (int i = 0; i < predArray.Length; i++)
            {
                predArrayFloat[i] = float.Parse(predArray[i]);
            }
            return predArrayFloat.ToList().IndexOf(predArrayFloat.Max());
        }

        /// <summary>
        /// Extracts a string list from the ONNX string prediction
        /// </summary>
        /// <param name="pred"></param>
        /// <returns>string array</returns>
        private string[] ExtractStringListFromString(string pred)
        {
            // change to stringbuilder => changes needed!
            pred = pred.Trim('{');
            pred = pred.Trim('}');
            pred = pred.Trim();
            pred = pred.Trim('{');
            pred = pred.Trim('}');
            return pred.Split(',');
        }

        /// <summary>
        /// Converts an image NDArray to a float array which is needed to make the tensor
        /// The NDArray has instead of the RGB format a BGR format which is changed in the code
        /// Can be simplified -> instead loading as byte and not as NDArray -> NumSharp.Bitmap can be deleted
        /// </summary>
        /// <param name="imageArray"></param>
        /// <returns></returns>
        private float[] ImageArrayToTensorArray(NDArray imageArray)
        {
            List<float> arr = new List<float>();
            int j = 1;
            foreach (byte i in imageArray)
            {
                if (j % 4 != 0)
                {
                    arr.Add((float)(i / 255.0));
                }
                j++;
            }
            float[] fl = arr.ToArray();

            // switch R and B value places
            int width = dimensions[1];
            int height = dimensions[2];

            for (int l = 0; l < width * height; l++)
            {
                float placeholder = fl[0 + 3 * l];
                fl[0 + 3 * l] = fl[2 + 3 * l];
                fl[2 + 3 * l] = placeholder;
            }
            return fl;
        }

        /// <summary>
        /// Loop Detection and Processing
        /// </summary>
        private void LoopDetection()
        {
            while(true)
            {
                StartDetection();
                Thread.Sleep(10);
            }
        }

        /// <summary>
        /// Datagrid class for Shop/Warehouse
        /// </summary>
        private class DataGridBottle
        {
            public string bottleType { get; set; }
            public string inShop { get; set; }
            public string inWarehouse { get; set; }
        }

        /// <summary>
        /// Datagrid class for Status
        /// </summary>
        private class StatusData
        {
            public string type { get; set; }
            public string amount { get; set; }
            public string price { get; set; }
        }

        /// <summary>
        /// Update amount of bottles in the shop/warehouse in the datagrid
        /// https://www.youtube.com/watch?v=dOZYOnFb56Q
        /// </summary>
        private void UpdateDataGrid()
        {
            datagrid.Items.Clear();
            business.GetBottleList()
                .ForEach(bottleType => datagrid.Items.Add(new DataGridBottle
                {
                    bottleType = bottleType.ToString(),
                    inShop = business.GetAmountInShop(bottleType).ToString(),
                    inWarehouse = business.GetAmountInWarehouse(bottleType).ToString()
                }));
        }

        /// <summary>
        /// http://stackoverflow.com/questions/11137979/image-resizing-using-c-sharp
        /// Resizes Image to given size
        /// </summary>
        /// <param name="image"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        private Bitmap Resize(Image image, int width, int height)
        {

            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
               
                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }
            return destImage;
        }

        private void status1_Click(object sender, RoutedEventArgs e)
        {
            UpdateStatus();
        }
    }
}
