/*
using BottleBusiness.RandomForest;
using SharpLearning.Containers.Matrices;
using SharpLearning.RandomForest.Learners;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

/// <summary>
/// Class takes training file, trains the Random Forest Model and saves it in the given file
/// </summary>
namespace BottleVisionApp.RandomForest
{
    class RandomForest
    {

        //Initialize Classification Model
        //Extract features and labels from Training Folder
        public RandomForest(int numberOfTrees, string trainingFile, string saveFileForModel)
        {
            ImageProcessor imgProcessor = new ImageProcessor(trainingFile);
            List<List<double>> data = imgProcessor.trainingData;
            List<double> label = imgProcessor.label;

            // data to F64Matrix
            F64Matrix observations = new F64Matrix(data.SelectMany(x => x).ToArray(), data.Count(), data[0].Count());
            double[] targets = label.ToArray();
            // Create a random forest learner for classification with 100 trees
            var learner = new ClassificationRandomForestLearner(trees: numberOfTrees);

            // train and save the model
            var model = learner.Learn(observations, targets);
            model.Save(() => new StreamWriter(saveFileForModel));
        }
    }
}
*/
