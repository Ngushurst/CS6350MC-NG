using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using General_Tools;

namespace Perceptron
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

            Attributes[4] = new DAttribute("Genuine", 1, new List<String>(new String[]{ "0", "1" }), DAttribute.Type.Categorical, false);

            List<Case> TrainBank = DRT.ParseCSV(Attributes, TestPath + @"\bank-note\bank-note\train.csv", false);
            List<Case> TestBank = DRT.ParseCSV(Attributes, TestPath + @"\bank-note\bank-note\test.csv", false);

            StringBuilder output = new StringBuilder();

            //start testing here


            PerceptronLearner NormalPerceptron = new PerceptronLearner(10, TrainBank, 1, 1500, PerceptronLearner.PType.Normal);
            output.Append("NormalPerceptron \nTrain,Test\n");

            for (int i = 1; i < 11; i++)
            {
                NormalPerceptron.SingleEpoch(); //do an epoch then test it
                double trainError = NormalPerceptron.GetError(TrainBank);
                double testError = NormalPerceptron.GetError(TestBank);
                Console.WriteLine("Training error Normal Epoch# " + i +" = " + trainError);
                Console.WriteLine("Testing error Normal Epoch# " + i + " = " + testError);

                output.Append(trainError + "," + testError + "\n");
            }

            PerceptronLearner VotedPerceptron = new PerceptronLearner(10, TrainBank, 1, 1500, PerceptronLearner.PType.Voted);
            output.Append("\nVotedPerceptron \nTrain,Test\n");
            for (int i = 1; i < 11; i++)
            {
                VotedPerceptron.SingleEpoch();
                double trainError = VotedPerceptron.GetError(TrainBank);
                double testError = VotedPerceptron.GetError(TestBank);
                Console.WriteLine("Training error Voted Epoch# " + i + " = " + trainError);
                Console.WriteLine("Testing error Voted Epoch# " + i + " = " + testError);
                output.Append(trainError + "," + testError + "\n");
            }



            PerceptronLearner AveragedPerceptron = new PerceptronLearner(10, TrainBank, 1, 1500, PerceptronLearner.PType.Averaged);
            output.Append("\nAveragedPerceptron \nTrain,Test\n");
            for (int i = 1; i < 11; i++)
            {
                AveragedPerceptron.SingleEpoch();
                double trainError = AveragedPerceptron.GetError(TrainBank);
                double testError = AveragedPerceptron.GetError(TestBank);
                Console.WriteLine("Training error Averaged Epoch# " + i + " = " + trainError);
                Console.WriteLine("Testing error Averaged Epoch# " + i + " = " + testError);

                output.Append(trainError + "," + testError + "\n");
            }



            PerceptronLearner MarginPerceptron = new PerceptronLearner(10, TrainBank, 1, 1500, PerceptronLearner.PType.Margin, 6);
            output.Append("\nMarginPerceptron \nTrain,Test\n");
            for (int i = 1; i < 11; i++)
            {
                MarginPerceptron.SingleEpoch();
                double trainError = MarginPerceptron.GetError(TrainBank);
                double testError = MarginPerceptron.GetError(TestBank);
                Console.WriteLine("Training error Margin Epoch# " + i + " = " + trainError);
                Console.WriteLine("Testing error Margin Epoch# " + i + " = " + testError);
                output.Append(trainError + "," + testError + "\n");
            }

            Console.WriteLine("\n\n\n\n\n\n Writing all results to TestingData/RunResults/Perceptron.csv");
            System.IO.File.WriteAllText(TestPath + @"/RunResults/Perceptron.csv", output.ToString());

            Console.Read();


        }
    }
}
