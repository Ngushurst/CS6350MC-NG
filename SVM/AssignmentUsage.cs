using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using General_Tools;

namespace SVM
{
    class AssignmentUsage
    {
        public static string TestPath = @"..\..\TestingData";

        public static void Main()
        {

            //Attributes for the data
            DAttribute[] Attributes = new DAttribute[5];

            Attributes[0] = new DAttribute("Varaince", 1, null, DAttribute.Type.Numeric, false);
            Attributes[1] = new DAttribute("Skew", 1, null, DAttribute.Type.Numeric, false);
            Attributes[2] = new DAttribute("Curtosis", 1, null, DAttribute.Type.Numeric, false);
            Attributes[3] = new DAttribute("Entropy", 1, null, DAttribute.Type.Numeric, false);

            Attributes[4] = new DAttribute("Genuine", 1, new List<String>(new String[] { "0", "1" }), DAttribute.Type.Categorical, false);

            List<Case> TrainBank = DRT.ParseCSV(Attributes, TestPath + @"\bank-note\bank-note\train.csv", false);
            List<Case> TestBank = DRT.ParseCSV(Attributes, TestPath + @"\bank-note\bank-note\test.csv", false);

            //Convert output to -1,1 as opposed to 0,1

            Case.ColXtoY(TrainBank, 4, 0, -1);
            Case.ColXtoY(TestBank, 4, 0, -1);

            StringBuilder output = new StringBuilder();

            //begin testing

            
            double[] C = new double[]{ 100.0 / 873.0, 500.0 / 873.0, 700.0 / 873.0 };

            Console.WriteLine("Starting primal Subgradient Decent with C = { 100/873, 500/873, 700/873 }");
            Console.WriteLine("=====================================================================================");

            Console.WriteLine("\nUsing NewLR = LR / (1 + LR * T / D) for learning rate. \n");

            //set up parameters
            double LearningRate = .2;
            double LearningAdjust = .75;
            int Seed = 1500;
            //report
            Console.WriteLine("\tBase Learning Rate = " + LearningRate);
            Console.WriteLine("\tNum epochs (T) = 100");
            Console.WriteLine("\tLearning Rate Adjustment = " + LearningAdjust);

            SVMGradient current; //set up the variable for the SVM for the all the tests
            for (int j = 0; j < 3; j++)
            {
                current = new SVMGradient(C[j], LearningRate, LearningAdjust, Seed, TrainBank);
                Console.WriteLine("\nCreated new primal learner with C = " + C[j]);


                for (int i = 0; i < 100; i++)
                {
                    if (i % 10 == 9)
                    {
                        Console.WriteLine("\tCompleted " + (i + 1) + " Epochs.");
                        Console.WriteLine("\tTraining error = " + current.getTrainingError());
                    }
                    current.PGradientEpoch(1); //do 100 epochs

                }

                Console.WriteLine("\n\tTraining error = " + current.getTrainingError());
                Console.WriteLine("\tTesting error  = " + current.getTestError(TestBank));
                double[] weight = current.getWeight();
                Console.Write("\tWeight = { " + weight[0]);
                for (int i = 1; i < weight.Length; i++)
                {
                    Console.Write(", " + weight[i]);
                }
                Console.Write("}\n");
                Console.WriteLine("\tBias = " + current.getBias());
            }

            Console.WriteLine("-------------------------------------------------------------------------------------\n");

            Console.WriteLine("Using NewLR = LR/ (1 + T) for learning rate.\n");
            Console.WriteLine("\tBase Learning Rate = " + LearningRate);
            Console.WriteLine("\tNum epochs (T) = 100");
            Console.WriteLine("\tLearning Rate Adjustment = " + LearningAdjust);

            for (int j = 0; j < 3; j++)
            {
                current = new SVMGradient(C[j], LearningRate, LearningAdjust, Seed, TrainBank);

                Console.WriteLine("\nCreated new primal learner with C = " + C[j]);

                for (int i = 0; i < 100; i++)
                {
                    if (i % 10 == 9)
                    {
                        Console.WriteLine("\tCompleted " + (i+1) + " Epochs.");
                        Console.WriteLine("\tTraining error = " + current.getTrainingError());
                    }
                    current.PGradientEpoch(1); //do 100 epochs

                }

                Console.WriteLine("\n\tTraining error = " + current.getTrainingError());
                Console.WriteLine("\tTesting error  = " + current.getTestError(TestBank));
                double[] weight = current.getWeight();
                Console.Write("\tWeight = { " + weight[0]);
                for (int i = 1; i < weight.Length; i++)
                {
                    Console.Write(", " + weight[i]);
                }
                Console.Write("}\n");
                Console.WriteLine("\tBias = " + current.getBias());
            }
                //let the user read the stuff on screen.
                Console.WriteLine("\n\n\nFinished execution. Hit any key to exit.");

            Console.Read();

        }
    }
}
