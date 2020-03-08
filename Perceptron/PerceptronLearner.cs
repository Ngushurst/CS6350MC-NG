using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using General_Tools;

namespace Perceptron
{


    /// <summary>
    /// A class implementing the percepton machine learning algorithm. While it is very similar to gradient decent, my understanding
    /// is that it is notably faster without sacrificing accuracy. Something like that. Furthermore, the Perceptron algorithm does not
    /// try to find a line of best fit, but a line that linearly separates the data. As such, items are either "above" or "below" the line.
    /// </summary>
    public class PerceptronLearner
    {
        int Epoch;
        int MaxEpoch;
        int RandomSeed;
        double bias;
        double LearningRate;
        double[] weight;



        PType Type;

        int numWeights;
        double[] averagedWeight; //for averaged perceptron (add all the weights, and then take)

        int currentAcurracy;
        List<int> accuracy; //number of correct guesses per vote
        List<double[]> weights; //for voted perceptron

        int marginErrors;
        double marginAllowed; //for margin perceptron


        List<Case> Data;
        
        public PerceptronLearner(int MaxEpochs, List<Case> Data, double LearningRate, int ShuffleSeed, PType type)
        {
            MaxEpoch = MaxEpochs;
            this.Data = Data;
            this.LearningRate = LearningRate;

            Type = type;

            bias = 0;

            weight = new double[Data[0].AttributeVals.Length - 1]; //We have the number of weights equal to the number of non final attributes.
                                                                   //I really should have made the case class have an answer field. That would make things so much easier now.

            RandomSeed = ShuffleSeed;


            switch (Type) //set up parameters specific to the type
            {
                case PType.Normal: //Nothing. It's normal
                    break;
                case PType.Voted: //Initialize
                    currentAcurracy = 0;
                    accuracy = new List<int>();
                    accuracy.Add(0); //first item is zero. The rest are adjusted at some point
                    weights = new List<double[]>();
                    weights.Add(weight); //toss in the first weight
                    break;
                case PType.Averaged:
                    numWeights = 0;
                    averagedWeight = new double[Data[0].AttributeVals.Length]; //also includes bias as the last term
                    break;
                case PType.Margin:
                    marginAllowed = 1; //default to 1
                    marginErrors = 0;
                    break;
            }
        }
        public PerceptronLearner(int MaxEpochs, List<Case> Data, double LearningRate, int ShuffleSeed, PType type, double margin)
        {
            MaxEpoch = MaxEpochs;
            this.Data = Data;
            this.LearningRate = LearningRate;

            Type = type;

            bias = 0;

            weight = new double[Data[0].AttributeVals.Length - 1]; //We have the number of weights equal to the number of non final attributes.
                                                                   //I really should have made the case class have an answer field. That would make things so much easier now.

            RandomSeed = ShuffleSeed;


            switch (Type) //set up parameters specific to the type
            {
                case PType.Normal: //Nothing. It's normal
                    break;
                case PType.Voted: //Initialize
                    currentAcurracy = 0;
                    accuracy = new List<int>();
                    accuracy.Add(0); //first item is zero. The rest are adjusted at some point
                    weights = new List<double[]>();
                    weights.Add(weight); //toss in the first weight
                    break;
                case PType.Averaged:
                    numWeights = 0;
                    averagedWeight = new double[Data[0].AttributeVals.Length]; //also includes bias as the last term
                    break;
                case PType.Margin:
                    marginAllowed = margin;
                    marginErrors = 0;
                    break;
            }
        }

        /// <summary>
        /// Runs the perceptron algorithm with MaxEpoch epochs (1 epoch touches on every item in the data).
        /// </summary>
        public void PerceptionFull()
        {
            for (; Epoch < MaxEpoch;)
            {
                ShuffleData(); //shuffle the data before accessing
                //access each item of the data "in order" (it's shuffled) and update the weight on an error
                 SingleEpoch();

            }
        }

        /// <summary>
        /// Runs the perceptron through a single epoch
        /// </summary>
        public void SingleEpoch()
        {
            ShuffleData(); //shuffle the data before accessing
            for (int i = 0; i < Data.Count(); i++)
            {//access each item of the data "in order" (it's shuffled) and update the weight on an error
                double margin;
                bool Prediction = TestCase(Data[i], out margin); // predicted true/false for current case
                bool value = Data[i].AttributeVals.Last() == 1;
                if (!((Prediction && value) || (!Prediction && !value)))
                {   // True and True        or   False and false
                    //the prediction was wrong. Update the weights

                    switch (Type)
                    {
                        case PType.Normal: //Nothing. It's normal
                            break;
                        case PType.Voted:
                            accuracy.Add(currentAcurracy);
                            currentAcurracy = 1;
                            weights.Add(weight.ToArray());
                            break;
                        case PType.Averaged:
                            //add the weight
                            addWeight();
                            break;
                        case PType.Margin:
                            //do nothing. It's an error, but that's because it's actually wrong
                            break;
                    }


                    Update(Data[i], margin);
                }
                else //it was a hit. Lower the variation a bit.
                {
                    switch (Type)
                    {
                        case PType.Normal: //Nothing. It's normal
                            break;
                        case PType.Voted:
                            currentAcurracy++;//accuracy is higher 
                            break;
                        case PType.Averaged:
                            //add the weight anyway
                            addWeight();
                            break;
                        case PType.Margin:
                            if (margin > marginAllowed)
                            {
                                Update(Data[i], margin);
                                marginErrors++;
                            }
                            break;
                    }
                    //UpdateLearningRate();
                }
                switch (Type)
                {
                    case PType.Normal: //Nothing. It's normal
                        break;
                    case PType.Voted: //Initialize
                        
                        break;
                    case PType.Averaged:
                        addWeight();
                        break;
                    case PType.Margin:
                        if (marginErrors > Data.Count / 3) //if more than 33% the data is beyond the margin, assume that the margin is not possible
                        {
                            marginAllowed = marginAllowed / 2; //cut the margin allowed in half
                        }
                        marginErrors = 0; //reset margin errors
                        break;
                }

            }

            Console.WriteLine("Completed Epoch #" + (Epoch + 1));
            Epoch++;
        }

        /// <summary>
        /// Adds the weight to the Averaged weight variable and increments the number of weights
        /// </summary>
        public void addWeight()
        {
            numWeights++;
            for(int i = 0; i<weight.Length; i++)
            {
                averagedWeight[i] += weight[i];
            }

            averagedWeight[averagedWeight.Length - 1] += bias;
        }

        /// <summary>
        /// Updates the weight vector and bias on an error using a single case and the current set of weights.
        /// </summary>
        private void Update(Case C, double margin)
        {
            if (C.AttributeVals.Last() == 1)
            {
                bias += 1;
            }
            else
            {
                bias -= 1;
            }
            double sign;
            if(margin < 0)
            {
                sign = -1;
            }
            else
            {
                sign = 1;
            }
            

            for (int i = 0; i < weight.Length; i++)
            {
                weight[i] = weight[i] - (sign * LearningRate * C.AttributeVals[i]);
            }
        }


        /// <summary>
        /// Tests a single case using the current weight vector and returns its "distance" to the vector. Treat a positive value as true and a negative
        /// value as false. The distance can be used later to calculate the margin if needed.
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        private double TestSingleCase(Case c)
        {
            double sum = bias;
            double weightLength = 0.0;

            for(int i = 0; i < weight.Length; i++)
            {
                sum += c.AttributeVals[i] * weight[i];
                weightLength = weight[i] * weight[i];
            }

            //distance = (Weight^Transpose * Case) / weight length
            if(weightLength == 0)
            {
                return 0;
            }

            return sum / Math.Sqrt(weightLength);
            
        }

        /// <summary>
        /// Returns either positive or negative based on where the case falls compared to the line/plane notated by weight.
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public bool TestCase(Case c)
        {
            double d = TestSingleCase(c);

            return d > 0;

        }

        /// <summary>
        /// Returns either positive or negative based on where the case falls compared to the line/plane notated by weight.
        /// This method also returns the margin generated to find the answer in the input double.
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        private bool TestCase(Case c, out double margin)
        {
            margin = TestSingleCase(c);

            return margin > 0;
        }

        /// <summary>
        /// Shuffles the data in the data array and then sets the next random seed.
        /// </summary>
        private void ShuffleData()
        {
            Case.Shuffle(Data, RandomSeed);

            RandomSeed = new Random(RandomSeed).Next(); //uses the seed to get the next seed.
        }

        /// <summary>
        /// Gets the error of a dataset.
        /// </summary>
        /// <param name="test"></param>
        /// <returns></returns>
        public double GetError(List<Case> test)
        {
            double sumError = 0;

            Case.NormalizeWeights(test);

            switch (Type)
            {
                case PType.Normal: //Nothing. It's normal
                    
                    for (int i = 0; i < test.Count; i++)
                    {
                        bool Prediction = TestCase(test[i]);
                        bool value = Data[i].AttributeVals.Last() == 1;
                        if (!((Prediction && value) || (!Prediction && !value)))
                        {
                            sumError += test[i].getWeight();
                        }
                    }
                    break;

                case PType.Voted: //calculate for each vote


                    for (int i = 0; i < test.Count; i++)
                    {
                        int zero = 0;
                        int one = 0;
                        bool value = Data[i].AttributeVals.Last() == 1;
                        bool Prediction;
                        for (int j = 0; j < weights.Count; j++)
                        {
                            double sum = 0;
                            for (int k = 0; k < weight.Length; k++)
                            {
                                sum += Data[i].AttributeVals[k] * weights[j][k];
                            }
                            if (sum < 0)
                            {
                                zero += accuracy[j];
                            }
                            else
                            {
                                one += accuracy[j];
                            }

                        }
                        if (one > zero)
                            Prediction = true;
                        else
                            Prediction = false;
                        if (!((Prediction && value) || (!Prediction && !value)))
                        {
                            sumError += test[i].getWeight();
                        }
                    }
                    break;
                case PType.Averaged:
                    double[] tempWeight = new double[weight.Length];
                    double tempBias = bias; 
                    for (int i = 0; i<weight.Length; i++)
                    {
                        tempWeight[i] = weight[i]; //copy it
                        weight[i] = averagedWeight[i] / (double)numWeights;
                    }

                    bias = averagedWeight.Last() / (double)numWeights;

                    for (int i = 0; i < test.Count; i++)
                    {
                        bool Prediction = TestCase(test[i]);
                        bool value = Data[i].AttributeVals.Last() == 1;
                        if (!((Prediction && value) || (!Prediction && !value)))
                        {
                            sumError += test[i].getWeight();
                        }
                    }

                    //restore old settings
                    weight = tempWeight;
                    bias = tempBias;

                    break;
                case PType.Margin:
                    //do normally
                    for (int i = 0; i < test.Count; i++)
                    {
                        bool Prediction = TestCase(test[i]);
                        bool value = Data[i].AttributeVals.Last() == 1;
                        if (!((Prediction && value) || (!Prediction && !value)))
                        {
                            sumError += test[i].getWeight();
                        }
                    }
                    break;
            }


            return sumError;
        }

        public enum PType
        {
            Normal,
            Voted,
            Margin,
            Averaged
        }

    }
}
