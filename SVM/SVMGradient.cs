using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using General_Tools;

namespace SVM
{
    public class SVMGradient
    {
        private static double C;
        private static double BaseLR;
        private static double CurrentLR;
        private static double LRUpdate;
        private static int RSeed; //used for random number generator when shuffling data
        private static List<Case> Training;

        private static double[] weight;
        private static double bias;

        public static int epochsCompleted;
        private static int dataLocation; //current location in training data

        /// <summary>
        /// Sets up a new support vector machine with the input parameters.
        /// </summary>
        /// <param name="c">A hyper parameter used to alter the relative weight of slack (allowance for values that break into the margin)</param>
        /// <param name="LearningRate">A hyper parameter that influences how fast the weight and bias may change</param>
        /// <param name="LearningRateUpdater">A hyper parameter that influences how the learning rate changes between epochs</param>
        /// <param name="RandomSeed">An int to use as the seed to any random generators needed (just pick a number, any will do)</param>
        /// <param name="training">A set of training data</param>
        public SVMGradient(double c, double LearningRate, double LearningRateUpdater, int RandomSeed, List<Case> training)
        {
            dataLocation = 0;
            epochsCompleted = 0;

            weight = new double[training.First().AttributeVals.Length-1]; //weight needs a spot for all input values in the data, minus the label
            bias = 0;

            C = c;
            BaseLR = LearningRate;
            CurrentLR = LearningRate;
            LRUpdate = LearningRateUpdater;
            RSeed = RandomSeed;
            Training = training;
        }


        /// <summary>
        /// Runs a full epoch of SVM primal sub gradient decent (a full run through the training data), updating the weight and bias. Uses LRFormNum to determine
        /// how to update the learning rate.
        /// </summary>
        public void PGradientEpoch(int LRFormNum)
        {
            Case.Shuffle(Training, RSeed); //shuffle the data
            for(;dataLocation<Training.Count; dataLocation++)
            {
                PGradientStep();
            }
            //completed epoch. Reset data location, update the learning rate, and return
            dataLocation = 0;
            UpdateLearnningRate(LRFormNum);
            return;
        }

        /// <summary>
        /// Runs a single step of SVM primal sub-gradient decent using the next item in the data, updating the weight and bias.
        /// </summary>
        private void PGradientStep()
        {
            Case c = Training[dataLocation];
            double expected = c.AttributeVals.Last();

            double prediction = bias;
            for (int i = 0; i < weight.Length; i++) //sum of weighted attributes
            {
                prediction += weight[i] * c.AttributeVals[i];
            }

            prediction = expected * prediction; //will be positive on correct prediction (multiplying by expected value)


            // we diminish the weight to comparatively increase the weight of future adjustments. Do it regardless of hit or miss.
            double LRCompliment = 1 - CurrentLR;
            
            for (int i = 0; i < weight.Length; i++)
            {
                weight[i] *= LRCompliment;
            }

            if (prediction < 1) // expected * prediction will be positive on a correct prediction. predictions greater than 1 will be a safe distance from the margin
            {//yeesh, my head is really starting to fail.
                //W = LR * C * N * Expected * ValueVector
                
                double multiplier = CurrentLR * C * Training.Count * expected;

                for (int i = 0; i < weight.Length; i++)
                {
                    weight[i] += c.AttributeVals[i] * multiplier;
                }

                //bias += 1 - prediction; // I don't really know how the bias is calculated, so I'll just roll with this.
                                        // As such, the bias serves as the sum of the slack.
            }
        }

        /// <summary>
        /// Unimplemented due to a lack of time and energy.
        /// </summary>
        public void DGradientEpoch()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Unimplemented due to a lack of time and energy.
        /// </summary>
        private static void DGradientStep()
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Updates the learning rate to a smaller number. variant == 0 for NewLR = LR / (1 + LR * T / D). variant == 1 for NewLR = LR/ (1 + T).
        /// </summary>
        private void UpdateLearnningRate(int variant)
        {
            //NewLR = LR / (1 + LR * T / D).LR->Base Learning Rate. T->Epochs Completed.D->Learning Rate adjustment var.
            if (variant == 0)
            {
                double divisor = 1 + (BaseLR / LRUpdate) * (double)epochsCompleted;

                CurrentLR = BaseLR / divisor;
            }

            // Updates the learning rate to a smaller number. NewLR = LR/ (1 + T). LR -> Base Learning Rate. T -> Epochs Completed.
            else if (variant == 1)
            {
                CurrentLR = LRUpdate / (1.0 + (double)epochsCompleted);
            }

        }


        /// <summary>
        /// Uses the wieght and bias to predict the training data to get the accuracy. Returns proprotion of errors.
        /// </summary>
        /// <returns></returns>
        public double getTrainingError()
        {
            return getTestError(Training);
        }

        /// <summary>
        /// tests the given set using the current weight and bias and returns a number between zero and 1 representing the error.
        /// </summary>
        /// <returns></returns>
        public double getTestError(List<Case> TestSet)
        {
            double errors = 0;
            foreach(Case c in TestSet)
            {
                if (!test(c))
                {
                    errors++;
                }
            }
            return errors/(double)TestSet.Count; //return proportion of errors in set.
        }

        /// <summary>
        /// tests a single case using the current weight and bias.
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        private bool test(Case c)
        {
            double value = bias;
            for(int i = 0; i < weight.Length; i++) //bias plus sum of weighted attributes
            {
                value += weight[i] * c.AttributeVals[i];
            }
            return value >= 0; //if positive, return true. Else return false.
        }

        public double[] getWeight()
        {
            return weight.ToArray();
        }

        public double getBias()
        {
            return bias;
        }
    }
}
