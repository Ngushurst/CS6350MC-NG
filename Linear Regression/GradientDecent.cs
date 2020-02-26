using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linear_Regression
{

    /// <summary>
    /// A class used to iteratively report weight vectors based on an input data set. Data must be composed fully of numbers, and not made 
    /// of typical cases (by this library's standards).
    /// </summary>
    public class GradientDecent
    {
        double[] Weight; //That's all a gradient decent learner really is.
        double LearningRate = 1; //should be 1 by default
        double LearningRateDecrease = 1.05; // learningRate = learningRate/LearningRateDecrease. 1.05 seems to be the best for Batch. 1.005 appears to be "good" for Stochastic
        double PreviousError = double.PositiveInfinity;
        Random StochasticIndex = new Random(777); //using a random number generator to get the stochastic items
        List<Reach> TrainingData;


        /// <summary>
        /// Gradient decent learner initialized the given weight vector
        /// </summary>
        public GradientDecent(double[] weight, List<Reach> Train)
        {
            Weight = weight.ToArray();
            TrainingData = Train.ToList();
        }


        /// <summary>
        /// Gradient decent learner initialized to contain the zero vector with dimentions equal to the data's attributes (don't include the target attribute in your count)
        /// Returns the norm (length) of the difference of the new weight vector minus the previous length vector.
        /// </summary>
        public GradientDecent(List<Reach> Train)
        {
            Weight = new double[Train[0].attributeVals.Length];
            for (int i = 0; i < Train[0].attributeVals.Length; i++) //initialize all doubles to zero
            {
                Weight[i] = 0.0;
            }

            TrainingData = Train.ToList();
        }

        public GradientDecent(List<Reach> Train, double[] Learn)
        {
            Weight = new double[Train[0].attributeVals.Length];
            for (int i = 0; i < Train[0].attributeVals.Length; i++) //initialize all doubles to zero
            {
                Weight[i] = 0.0;
            }

            LearningRate = Learn[0];

            LearningRateDecrease = Learn[1];

            TrainingData = Train.ToList();
        }


        /// <summary>
        /// Updates the weight vector
        /// </summary>
        public double Batch()
        {
            double[] newWeight = GD(Weight, TrainingData, LearningRate);

            double output = WeightVectorDifference(newWeight, Weight);

            Weight = newWeight;

            upDateLR();

            return output;
        }

        /// <summary>
        /// Same as batch gradient decent but uses one item from the data for each calculation.
        /// </summary>
        public double Stochastic()
        {
            List<Reach> nextItem = new List<Reach>(1);
            nextItem.Add(TrainingData[StochasticIndex.Next(TrainingData.Count)]); //put in the next item

            double[] newWeight = GD(Weight, nextItem, LearningRate);

            double output = WeightVectorDifference(newWeight, Weight);

            Weight = newWeight;

            upDateLR();
            /*
            if(StochasticIndex == TrainingData.Count - 1)
            {
                StochasticIndex = 0;
            }
            else
            {
                StochasticIndex++;
            }
            */
            return output;
        }

        /// <summary>
        /// Returns ||W_1 - W_2||, or the length of the difference of the two vectors. Smaller values mean a smaller change in weight.
        /// </summary>
        private static double WeightVectorDifference(double[] W_1, double[]W_2)
        {
            double[] temp = new double[W_1.Length];

            for(int i = 0; i < W_1.Length; i++) //take the difference of the vectors
            {
                temp[i] = W_1[i] - W_2[i];
            }

            //use pythagrian theorem
            double squareSum = 0;
            for (int i = 0; i < temp.Length; i++) //take the difference of the vectors
            {
                squareSum += Math.Pow(temp[i],2); //square each item and toss it on
            }

            return Math.Sqrt(squareSum); //finish by taking the square root

        }

        /// <summary>
        /// Gradient Decent function.
        /// </summary>
        private static double[] GD(double[] weight, List<Reach> data, double LearningRate)
        {
            double[] newWeight = new double[weight.Length];

            for(int i = 0; i <weight.Length; i++) //initialize all doubles to zero
            {
                newWeight[i] = 0.0;
            }

            foreach(Reach item in data)
            {
                double error = CalculateError(item, weight);
                if (error != 0)
                {
                    error = error / Math.Abs(error); //error is either 1 or -1
                }
                for(int i = 0; i < weight.Length; i++)
                {
                    newWeight[i] += error * item.attributeVals[i]; //sum of error times value of each piece of data
                }
            }
            
            for (int i = 0; i < weight.Length; i++)
            {
                newWeight[i] = newWeight[i]/ data.Count; //average all weights
            }

            for (int i = 0; i < weight.Length; i++)
            { //negate sum, multiply by learning rate and original wieght, and then subtract from original weight to get new weight.'
                newWeight[i] = weight[i] - (LearningRate * (-newWeight[i]));
            }

            return newWeight;
        }


        private static double CalculateError(Reach data, double[] weight)
        {
            double prediction = 0;

            for(int i = 0; i < data.attributeVals.Length; i++)
            {
                prediction += data.attributeVals[i] * weight[i]; //add data times its weight
            }

            return data.answer - prediction;
        }


        /// <summary>
        /// Updates the learning rate and training error
        /// </summary>
        private void upDateLR()
        {
            double error = Math.Abs(CalculateAverageError(TrainingData)); //get new training error
            if (!(error < PreviousError)) //passed mid-point. Cut learning rate in half.
            {
                LearningRate = LearningRate/LearningRateDecrease; //smaller decrements in learning rate seem to improve accuracy
            }

            PreviousError = error;
        }

        /// <summary>
        /// Error is also known as cost. Calculates the average error per data point in a given list of data.
        /// </summary>
        public double CalculateAverageError(List<Reach> data)
        {
            double totalError = 0;

            foreach(Reach item in data)
            {
                totalError += CalculateError(item, Weight);
            }

            return totalError / data.Count;
        }

        public double getLearningRate()
        {
            return LearningRate;
        }

        public double[] getWeight()
        {
            double[] output = Weight.ToArray();
            return output;
        }

        public double getError()
        {
            return PreviousError;
        }
    }

    public class Reach
    {
        public readonly double[] attributeVals;

        public readonly double answer;

        public Reach(double[] attributes, double expectedValue)
        {
            attributeVals = attributes.ToArray(); //copy rather than reference
            answer = expectedValue;
        }

    }
}
