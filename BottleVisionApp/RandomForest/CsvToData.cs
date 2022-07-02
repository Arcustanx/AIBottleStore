using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

/// <summary>
/// This Class extracts data and label from a .csv file
/// </summary>

namespace BottleBusiness.RandomForest
{
    class CsvDataMaker
    {
        public List<double> label = new List<double>();
        public List<List<double>> values = new List<List<double>>();

        public CsvDataMaker(String csvFileName)
        {
            // save csv data in list
            List<string> features = new List<string>();
            using (var reader = new StreamReader(@csvFileName))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var value = line.Split(';');

                    features.Add(value[0]);
                }
            }
            // remove first row -> features
            features.RemoveAt(0);

            // convert every row in csv to a double list
            foreach (String str in features)
            {
                List<string> listOfStrings = str.Split(',').ToList();
                listOfStrings.RemoveAt(0);
                List<double> dataRow = listOfStrings.Select(x => double.Parse(x)).ToList();
                this.values.Add(dataRow);
            }

            // init last value (= label) of each list
            label = values.Select(x => x[x.Count() - 1]).ToList();

            // remove first (index) and last (label) of list
            foreach(List<double> num in values)
            {
                num.RemoveAt(0);
                num.RemoveAt(num.Count() - 1);
            }
        }
    }
}
