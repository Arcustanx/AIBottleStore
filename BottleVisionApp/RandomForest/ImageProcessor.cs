using Alturos.Yolo.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

/// <summary>
/// The Training File Contains 3 Files (Cola, Fanta, Water)
/// Each file has Images
/// The Images get resized, the RGB values are saved in a double array
/// Create a List of List of RGB values to feed the Random Forest Model
/// </summary>
namespace BottleBusiness.RandomForest
{
    class ImageProcessor
    {
        public List<List<double>> trainingData = new List<List<double>>();
        public List<double> label = new List<double>();

        public ImageProcessor()
        {
            trainingData = null;
            label = null;
        }
        public ImageProcessor(String trainingFile)
        {
            // saves all the folders in the directory
            string[] filesindirectory = System.IO.Directory.GetDirectories(trainingFile, "*", System.IO.SearchOption.AllDirectories);
            int fileLabel = 0;
            foreach (string searchFolder in filesindirectory)
            {
                var filters = new String[] { "jpg", "jpeg" };
                var files = GetFilesFrom(searchFolder, filters, false);

                // goes through each image in a folde, resizes it and saves rgb into List
                foreach (string imgFile in files)
                {
                    Bitmap img = (Bitmap)Image.FromFile(imgFile);
                    List<double> imageFeatures = getListFromProcessedImage(img);
                    trainingData.Add(imageFeatures);
                    label.Add(fileLabel);
                }
                fileLabel++;
            }
        }

        private String[] GetFilesFrom(String searchFolder, String[] filters, bool isRecursive)
        {
            List<String> filesFound = new List<String>();
            var searchOption = isRecursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            foreach (var filter in filters)
            {
                filesFound.AddRange(Directory.GetFiles(searchFolder, String.Format("*.{0}", filter), searchOption));
            }
            return filesFound.ToArray();
        }

        /// <summary>
        /// Resized Image
        /// </summary>
        /// <param name="img"></param>
        /// <returns>List of RGB values of resized Image</returns>
        public List<double> getListFromProcessedImage(Bitmap img)
        {
            Bitmap resized = new Bitmap(img, new Size(11, 26));
            List<double> imageFeatures = new List<double>();
            for (int y = 0; y < resized.Height; y++)
            {
                for (int x = 0; x < resized.Width; x++)
                {
                    Color color = img.GetPixel(x, y);
                    imageFeatures.Add(color.R / 255.0);
                    imageFeatures.Add(color.G / 255.0);
                    imageFeatures.Add(color.B / 255.0);
                }
            }
            return imageFeatures;
        }

        public Image AddBox(Image image, List<YoloItem> items)
        {
            Image img = (Image)image.Clone();
            Graphics graphics = Graphics.FromImage(img);
            var font = new Font("Arial", 12, FontStyle.Regular);
            var brush = new SolidBrush(Color.Blue);
            foreach (var item in items)
            {
                if (item.Type.Equals("bottle"))
                {
                    var x = item.X;
                    var y = item.Y;
                    var width = item.Width;
                    var height = item.Height;

                    var rect = new Rectangle(x, y, width, height);
                    var pen = new Pen(Color.LightGreen, 3);

                    var point = new Point(x, y);

                    graphics.DrawRectangle(pen, rect);
                    graphics.DrawString(item.Type, font, brush, point);
                    //graphics.DrawString(item.Type + ": " + item.Confidence, font, brush, point);
                }

            }
            return img;
        }
    }
}