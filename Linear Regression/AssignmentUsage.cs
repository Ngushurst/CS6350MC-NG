using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linear_Regression
{
    class AssignmentUsage
    {
        public static string TestPath = @"..\..\TestingData";
        public static double limit = Math.Pow(10, -6); //when the output of a gradient decent operation is less than Limit, stop the program and write to file


        public static void Main()
        {
            List<Reach> ConcreteTrain = readConcreteCSV(TestPath + @"/Concrete/slump_train.csv"); //53 items
            List<Reach> ConcreteTest = readConcreteCSV(TestPath + @"/Concrete/slump_test.csv"); //50 items

            //Batch Gradient Decent
            if (true)
            {
                GradientDecent Batch = new GradientDecent(ConcreteTrain);
                StringBuilder output = new StringBuilder();
                output.Append("Num, Weight,-,-,-,-,-,-,LearnRate,TrainError\n");

                int itCount = 0;
                double limitTest = double.PositiveInfinity;

                while (limitTest > limit)
                {
                    output.Append(itCount + ",");
                    itCount++; //increment
                    limitTest = Batch.Batch(); //update weight and get value to test against limit

                    double[] CurrentWeight = Batch.getWeight();

                    for(int i = 0; i < 7; i++) //write current weight to stringbuilder
                    {
                        output.Append(CurrentWeight[i] + ",");
                    }
                    //slap on learning rate and error. Go do it again
                    output.Append(Batch.getLearningRate() + "," + Batch.getError() + "\n");

                    //report to user
                    Console.WriteLine("Finished one iteration of Batch Gradient Decent with a training error of " + Batch.getError());
                }

                //found a convergent weight vector

                output.Append("Final\n,");

                double[] FinalWeight = Batch.getWeight();
                for (int i = 0; i < 7; i++) //write current weight to stringbuilder
                {
                    output.Append(FinalWeight[i] + ",");
                }


                output.Append(Batch.getLearningRate() + "," + Batch.CalculateAverageError(ConcreteTest)); //slap on test error

                Console.WriteLine("Writing all results to Linear Regression/TestingData/RunResults/ResultsBatch.csv\n\n\n");
                System.IO.File.WriteAllText(TestPath + @"/RunResults/ResultBatch.csv", output.ToString());

            }
            //Stochastic Gradient Decent
            if (true)
            {
                GradientDecent Stochastic = new GradientDecent(ConcreteTrain);
                StringBuilder output = new StringBuilder();
                output.Append("Num, Weight,-,-,-,-,-,-,LearnRate,TrainError\n");

                int itCount = 0;
                double limitTest = double.PositiveInfinity;

                while (limitTest > limit)
                {
                    output.Append(itCount + ",");
                    itCount++; //increment
                    limitTest = Stochastic.Stochastic(); //update weight and get value to test against limit

                    double[] CurrentWeight = Stochastic.getWeight();

                    for (int i = 0; i < 7; i++) //write current weight to stringbuilder
                    {
                        output.Append(CurrentWeight[i] + ",");
                    }
                    //slap on learning rate and error. Go do it again
                    output.Append(Stochastic.getLearningRate() + "," + Stochastic.getError() + "\n");

                    //report to user
                    Console.WriteLine("Finished one iteration of Stochastic Gradient Decent with a training error of " + Stochastic.getError());
                }

                //found a convergent weight vector

                output.Append("Final\n,");

                double[] FinalWeight = Stochastic.getWeight();
                for (int i = 0; i < 7; i++) //write current weight to stringbuilder
                {
                    output.Append(FinalWeight[i] + ",");
                }


                output.Append(Stochastic.getLearningRate() + "," + Stochastic.CalculateAverageError(ConcreteTest)); //slap on test error

                Console.WriteLine("Writing all results to Linear Regression/TestingData/RunResults/ResultsStochastic.csv");
                System.IO.File.WriteAllText(TestPath + @"/RunResults/ResultStochastic.csv", output.ToString());
            }
        }



        /// <summary>
        /// 7 predictors starting at the second item, and then the goal value. A quick and drity function for reading the concrete examples and testing for SLUMP values.
        /// </summary>
        private static List<Reach> readConcreteCSV(String Filepath)
        {
            //read all lines of the CSV file
            String[] input = System.IO.File.ReadAllLines(Filepath);
            if (input.Length == 0)
            {
                throw new MissingFieldException("File must have at least one data point");
            }

            String[][] rawCases = new String[input.Length][];

            for (int i = 0; i < input.Length; i++)
            {
                rawCases[i] = input[i].Split(','); //split all the strings and store them in raw cases
            }

            List<Reach> output = new List<Reach>();

            for(int i = 0; i < rawCases.Length; i++)
            {
                double[] attVals = new double[7];

                for(int j = 0; j < 7; j++)
                {
                    attVals[j] = Double.Parse(rawCases[i][j + 1]);
                }

                output.Add(new Reach(attVals, Double.Parse(rawCases[i][8])));
            }

            return output;
        }

    }
}
