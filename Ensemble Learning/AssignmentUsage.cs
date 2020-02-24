using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ID3_Algorithm;
using General_Tools;


namespace EnsembleLearning
{
    class AssignmentUsage
    {
        public static string TestPath = @"..\..\TestingData";

        public static int RNGseed = 1500; //seed used to make all random number generators when using bagging methods.
        public static int NumIterations = 1001; //number of iterations plus 1 (normally 1001)

        public static bool UseBoost = true;
        public static bool UseBag = true;
        public static bool UseBagBias = true;
        public static bool UseRandTrees = true;

        public static void Main()
        {
            List<DAttribute> attributeBank = new List<DAttribute>(7);
            //Once again, could auto detect, but doing so makes the data harder to read. Furthermore, autodetecting doesn't work for filling in missing values.
            //below data descriptions come from data-desc.txt, located near the data for this training data.

            string[] AVariants;

            //age being numeric means that the actual variants will be figured out at run time. The variant will be overwritten when we pull in the testing data.
            attributeBank.Add(new DAttribute("age", 0, null, true, false));
            AVariants = new string[] {"admin.","unknown","unemployed","management","housemaid","entrepreneur","student",
                                       "blue-collar","self-employed","retired","technician","services" };
            attributeBank.Add(new DAttribute("job", 1, new List<string>(AVariants), false, false));
            AVariants = new string[] { "married", "divorced", "single" };
            attributeBank.Add(new DAttribute("marital", 2, new List<string>(AVariants), false, false));
            AVariants = new string[] { "unknown", "secondary", "primary", "tertiary" };
            attributeBank.Add(new DAttribute("education", 3, new List<string>(AVariants), false, false));
            AVariants = new string[] { "yes", "no" };
            attributeBank.Add(new DAttribute("default", 4, new List<string>(AVariants), false, false));

            attributeBank.Add(new DAttribute("balance", 5, null, true, false));
            AVariants = new string[] { "yes", "no" };
            attributeBank.Add(new DAttribute("housing", 6, new List<string>(AVariants), false, false));
            AVariants = new string[] { "yes", "no" };
            attributeBank.Add(new DAttribute("loan", 7, new List<string>(AVariants), false, false));
            AVariants = new string[] { "unknown", "telephone", "cellular" };
            attributeBank.Add(new DAttribute("contact", 8, new List<string>(AVariants), false, false));

            attributeBank.Add(new DAttribute("day", 9, null, true, false));
            AVariants = new string[] { "jan", "feb", "mar", "apr", "may", "jun", "jul", "aug", "sep", "oct", "nov", "dec" };
            attributeBank.Add(new DAttribute("month", 10, new List<string>(AVariants), false, false));

            attributeBank.Add(new DAttribute("duration", 11, null, true, false));

            attributeBank.Add(new DAttribute("campaign", 12, null, true, false));

            attributeBank.Add(new DAttribute("pdays", 13, null, true, false));

            attributeBank.Add(new DAttribute("previous", 14, null, true, false));
            AVariants = new string[] { "unknown", "other", "failure", "success" }; //If unknown needs to be filled in, remove it from this list.
            attributeBank.Add(new DAttribute("poutcome", 15, new List<string>(AVariants), false, false));
            AVariants = new string[] { "yes", "no" };
            attributeBank.Add(new DAttribute("result", 16, new List<string>(AVariants), false, true));



            List<Case> TrainBank = DRT.ParseCSV(attributeBank.ToArray(), TestPath + @"\bank\train.csv", true);
            List<Case> TestBank = DRT.ParseCSV(attributeBank.ToArray(), TestPath + @"\bank\test.csv", false);

            

            if (UseBoost)
            {
                StringBuilder output = new StringBuilder();
                StringBuilder outputTree = new StringBuilder();

                output.Append("T(rees),Training Error,Testing Error\n"); //going to generate a csv file the ensemble learner's performance

                EnsembleLearner current = null; //initialize. Doesn't matter what to
                for (int i = 1; i < NumIterations; i++)//Assignment specifies 1000 iterations
                {
                    current = EnsembleTools.AdaBoost(i, TrainBank, attributeBank);

                    double TrainingError = current.TestEnsembleClassMass(TrainBank, attributeBank);
                    double TestingError = current.TestEnsembleClassMass(TestBank, attributeBank);


                    Console.WriteLine("Built an AdaBoost Learner with " + i + " Trees.");
                    output.Append(i + "," + TrainingError + "," + TestingError + "\n"); //write a new line for the CSV file
                }

                StringBuilder output2 = new StringBuilder();
                outputTree.Append("Tree#,Training Error,Testing Error\n");
                for (int i = 0; i<NumIterations -1; i++)
                {
                    ID3_Node node = current.Trees[i];

                    double TrainingError = ID3Tools.FindTestError(TrainBank, attributeBank, node);
                    double TestingError = ID3Tools.FindTestError(TestBank, attributeBank, node);

                    int index = i + 1;
                    outputTree.Append(index + "," + TrainingError + "," + TestingError + "\n"); //write a new line for the CSV file
                }

                Console.WriteLine("Writing all results to Ensemble\\ Learning/TestingData/RunResults/ResultsBankBoost.csv");
                System.IO.File.WriteAllText(TestPath + @"/RunResults/ResultsBankBoost.csv", output.ToString());
                System.IO.File.WriteAllText(TestPath + @"/RunResults/ResultsBankBoostTrees.csv", outputTree.ToString());
            }

            if (UseBag)
            {
                StringBuilder output = new StringBuilder();
                StringBuilder outputTree = new StringBuilder();

                output.Append("T(rees),Training Error,Testing Error\n"); //going to generate a csv file the ensemble learner's performance

                EnsembleLearner current = null; //initialize. Doesn't matter what to
                for (int i = 1; i < NumIterations ; i++)//Assignment specifies 1000 iterations
                {
                    current = EnsembleTools.Bagging(i, TrainBank.Count, true, RNGseed, TrainBank, attributeBank);

                    double TrainingError = current.TestEnsembleClassMass(TrainBank, attributeBank);
                    double TestingError = current.TestEnsembleClassMass(TestBank, attributeBank);

                    Console.WriteLine("Built a Bagged Learner with " + i + " Trees.");

                    output.Append(i + "," + TrainingError + "," + TestingError + "\n"); //write a new line for the CSV file
                }

                StringBuilder output2 = new StringBuilder();
                outputTree.Append("Tree#,Training Error,Testing Error\n");
                for (int i = 0; i < NumIterations -1; i++)
                {
                    ID3_Node node = current.Trees[i];

                    double TrainingError = ID3Tools.FindTestError(TrainBank, attributeBank, node);
                    double TestingError = ID3Tools.FindTestError(TestBank, attributeBank, node);

                    int index = i + 1;
                    outputTree.Append(index + "," + TrainingError + "," + TestingError + "\n"); //write a new line for the CSV file
                }

                Console.WriteLine("Writing all results to Ensemble\\ Learning/TestingData/RunResults/ResultsBankBag.csv");
                System.IO.File.WriteAllText(TestPath + @"/RunResults/ResultsBankBag.csv", output.ToString());
                System.IO.File.WriteAllText(TestPath + @"/RunResults/ResultsBankBagTrees.csv", outputTree.ToString());
            }

            if (UseBagBias)
            {
                Random Gen = new Random(RNGseed);

                double averageResult = 0;
                foreach(Case c in TrainBank)
                {
                    averageResult += c.AttributeVals[16]; //add target label value
                }
                averageResult = averageResult / (double)TrainBank.Count;

                double AverageTreeVariance = 0;
                double AverageBagVariance = 0;

                double AverageTreeBias = 0;
                double AverageBagBias = 0;

                StringBuilder output = new StringBuilder();
                output.Append("TreeBias,EnsBias,TreeVar,EnsVar\n");

                for (int i = 1; i<101; i++)
                {
                    

                    List<Case> Sample = EnsembleTools.GetRandomSubset(true, 1000, Gen, TrainBank); //Generate samples without replacement
                    EnsembleLearner current = EnsembleTools.Bagging(1000, 1000, true, RNGseed, Sample, attributeBank);//Generate samples allowing duplicates
                    //Calculate bias first

                    double Bias = 0;//tree
                    foreach (Case c in TrainBank)
                    {
                        // (1 - prediction) ^ 2
                        if (ID3Tools.TestWithTree(c, current.Trees[0]) != c.AttributeVals[16]) //Incorrect guess
                            Bias += 1;
                    }
                    Bias = Bias / (double) TrainBank.Count;
                    output.Append( Bias+",");
                    AverageTreeBias += Bias;

                    Bias = 0;//Ensemble
                    foreach (Case c in TrainBank)
                    {
                        // (1 - prediction) ^ 2
                        if (current.TestEnsembleClassificaiton(c, attributeBank[16]) != c.AttributeVals[16]) //Incorrect guess
                            Bias += 1;
                    }
                    Bias = Bias / (double)TrainBank.Count;
                    AverageBagBias += Bias;
                    output.Append(Bias + ",");

                    //now variance
                    double Variance = 0;//tree
                    foreach (Case c in TrainBank)
                    {
                        Variance += ID3Tools.TestWithTree(c, current.Trees[0]) - averageResult; //add target label value
                    }
                    Variance = Math.Pow(Variance / (double)(TrainBank.Count), 2 );

                    AverageTreeVariance += Variance;
                    output.Append(Variance + ",");


                    Variance = 0;//ensemble
                    foreach (Case c in TrainBank)
                    {
                        Variance += current.TestEnsembleClassificaiton(c, attributeBank[16]) - averageResult; //add target label value
                    }
                    Variance = Math.Pow(Variance / (double)(TrainBank.Count), 2);

                    AverageBagVariance += Variance;
                    output.Append(Variance + "\n");

                    Console.WriteLine("Completed Bias and Variance calculations for Bagged Learner number " + i);
                }

                AverageTreeVariance = AverageTreeVariance / 100;
                AverageTreeBias = AverageTreeBias / 100;
                AverageBagVariance = AverageBagVariance / 100;
                AverageBagBias = AverageBagBias / 100;

                output.Append("FinalVals\n" + AverageTreeBias + "," + AverageBagBias + "," + AverageTreeVariance + "," + AverageBagVariance);
                Console.WriteLine();
                System.IO.File.WriteAllText(TestPath + @"/RunResults/ResultsBankBagAnalysis.csv", output.ToString());

            }

            if (UseRandTrees)
            {
                for (int numAttributes = 2; numAttributes < 7; numAttributes += 2)
                {
                    StringBuilder output = new StringBuilder();
                    StringBuilder output2 = new StringBuilder();

                    output.Append("T(rees),Training Error,Testing Error\n"); //going to generate a csv file the ensemble learner's performance

                    EnsembleLearner current = null; //initialize. Doesn't matter what to
                    for (int i = 1; i < NumIterations; i++)//Assignment specifies 1000 iterations
                    {
                        current = EnsembleTools.RandomForest(i, TrainBank.Count, true, RNGseed, numAttributes, TrainBank, attributeBank);

                        double TrainingError = current.TestEnsembleClassMass(TrainBank, attributeBank);
                        double TestingError = current.TestEnsembleClassMass(TestBank, attributeBank);

                        Console.WriteLine("Built a Random Forest Learner with " + i + " Trees.");

                        output.Append(i + "," + TrainingError + "," + TestingError + "\n"); //write a new line for the CSV file
                    }

                    output2.Append("Tree#,Training Error,Testing Error\n");
                    for (int i = 0; i < NumIterations -1; i++)
                    {
                        ID3_Node node = current.Trees[i];

                        double TrainingError = ID3Tools.FindTestError(TrainBank, attributeBank, node);
                        double TestingError = ID3Tools.FindTestError(TestBank, attributeBank, node);

                        int index = i + 1;
                        output2.Append(index + "," + TrainingError + "," + TestingError + "\n"); //write a new line for the CSV file
                    }

                    Console.WriteLine("Writing all results to Ensemble\\ Learning/TestingData/RunResults/ResultsBankRForest.csv");
                    System.IO.File.WriteAllText(TestPath + @"/RunResults/ResultsBankRForest" + numAttributes + ".csv", output.ToString());
                    System.IO.File.WriteAllText(TestPath + @"/RunResults/ResultsBankRForest" + numAttributes + "Trees.csv", output2 .ToString());
                }
            }

        }

    }
}
