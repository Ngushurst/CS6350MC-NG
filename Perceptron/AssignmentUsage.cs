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


            //start testing here


            PerceptronLearner NormalPerceptron = new PerceptronLearner(10, TrainBank, 1, 1500, PerceptronLearner.PType.Normal);

            NormalPerceptron.PerceptionFull();

            Console.WriteLine("Training error Normal = " + NormalPerceptron.GetError(TrainBank));
            Console.WriteLine("Testing error Normal  = " + NormalPerceptron.GetError(TestBank));


            PerceptronLearner VotedPerceptron = new PerceptronLearner(10, TrainBank, 1, 1500, PerceptronLearner.PType.Voted);

            VotedPerceptron.PerceptionFull();

            Console.WriteLine("Training error Voted = " + VotedPerceptron.GetError(TrainBank));
            Console.WriteLine("Testing error Voted  = " + VotedPerceptron.GetError(TestBank));



            PerceptronLearner AveragedPerceptron = new PerceptronLearner(10, TrainBank, 1, 1500, PerceptronLearner.PType.Averaged);

            AveragedPerceptron.PerceptionFull();

            Console.WriteLine("Training error Averaged = " + AveragedPerceptron.GetError(TrainBank));
            Console.WriteLine("Testing error Averaged  = " + AveragedPerceptron.GetError(TestBank));



            PerceptronLearner MarginPerceptron = new PerceptronLearner(10, TrainBank, 1, 1500, PerceptronLearner.PType.Margin, 6);

            MarginPerceptron.PerceptionFull();

            Console.WriteLine("Training error Margin = " + MarginPerceptron.GetError(TrainBank));
            Console.WriteLine("Testing error Margin  = " + MarginPerceptron.GetError(TestBank));

            Console.Read();
        }
    }
}
