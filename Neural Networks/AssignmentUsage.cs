using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using General_Tools;

namespace Neural_Networks
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

            //StringBuilder output = new StringBuilder();

            //begin testing


            int[] NumNeurons = new int[] { 5, 10, 25, 50, 100 };

            Console.WriteLine("Testing 3 layer neural nets with X neurons per layer = { 5, 10, 25, 50, 100 }");
            Console.WriteLine("=====================================================================================");

            Console.WriteLine("\nUsing NewLR = Base LR / (1 + Base LR * T / D) for learning rate. \n");

            //set up parameters
            double LearningRate = 1; //arbitrary number
            int Seed = 1500;
            //report
            Console.WriteLine("\tBase Learning Rate = " + LearningRate);
            Console.WriteLine("\tNum epochs (T) = 100");

            NeuralNet current; //set up the variable for the SVM for the all the tests
            for (int j = 0; j < NumNeurons.Length; j++)
            {
                current = new NeuralNet(LearningRate, Seed, 4, NumNeurons[j], 2); //two hidden layers + 1 output, always
                Console.WriteLine("\nCreated new three layer Neural Net with " + NumNeurons[j] + " Neurons per layer.");


                for (int i = 0; i < 20; i++)
                {
                    current.runEpochs(10,TrainBank); //do 100 epochs
                    Console.WriteLine("\tCompleted " + (i+1)*10 + " Epochs.");
                    Console.WriteLine("\tTraining error at " + (i+1)*10 + " epochs = " + current.getError(TrainBank));
                }

                Console.WriteLine("\n\tFinal Training error = " + current.getError(TrainBank));
                Console.WriteLine("\tTesting error  \t= " + current.getError(TestBank));

            }
            /*
            Console.WriteLine("-------------------------------------------------------------------------------------\n");

            Console.WriteLine("Using NewLR = LR/ (1 + T) for learning rate.\n");
            Console.WriteLine("\tBase Learning Rate = " + LearningRate);
            Console.WriteLine("\tNum epochs (T) = 100");

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
            */
            //let the user read the stuff on screen.
            Console.WriteLine("\n\n\nFinished execution. Hit any key to exit.");

            Console.Read();

        }
    }
}
