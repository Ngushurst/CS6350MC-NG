using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using General_Tools;

namespace ID3_Algorithm
{
    class AssignmentDetails
    {
        //filepath used to get to the testing data folder from the current spot.
        public static string TestPath = @"..\..\..\..\TestingData";
        public static bool BuildCarTrees = true;
        public static bool BuildBankTrees = true;
        public static bool BuildBankTreeNormal = true;
        public static bool BuildBankMissingVals = true;


        public static void Main()
        {
            // ========= Part 1 ============= //

            if (BuildCarTrees)
            {
                //This is the car example.
                List<DAttribute> attributeCars = new List<DAttribute>(7);
                //while I could auto detect this, it's much easier to read the trees if I name the DataAttributes ahead of time
                //below data descriptions come from data-desc.txt, located near the data for this training data.
                string[] AVariants = new string[] { "vhigh", "high", "med", "low" }; //array of attribute variants to pass in to an attribute


                attributeCars.Add(new DAttribute("buying", 0, new List<string>(AVariants), DAttribute.Type.Categorical, false));
                attributeCars.Add(new DAttribute("maint", 1, new List<string>(AVariants), DAttribute.Type.Categorical, false));

                AVariants = new string[] { "2", "3", "4", "5more" };
                attributeCars.Add(new DAttribute("doors", 2, new List<string>(AVariants), DAttribute.Type.Categorical, false));
                AVariants = new string[] { "2", "4", "more" };
                attributeCars.Add(new DAttribute("persons", 3, new List<string>(AVariants), DAttribute.Type.Categorical, false));
                AVariants = new string[] { "small", "med", "big" };
                attributeCars.Add(new DAttribute("lug_boot", 4, new List<string>(AVariants), DAttribute.Type.Categorical, false));
                AVariants = new string[] { "low", "med", "high" };
                attributeCars.Add(new DAttribute("safety", 5, new List<string>(AVariants), DAttribute.Type.Categorical, false));
                AVariants = new string[] { "unacc", "acc", "good", "vgood" };
                attributeCars.Add(new DAttribute("label", 6, new List<string>(AVariants), DAttribute.Type.Categorical, true));

                
                List<Case> TrainCars = DRT.ParseCSV(attributeCars.ToArray(), TestPath + @"\car\train.csv");
                List<Case> TestCars = DRT.ParseCSV(attributeCars.ToArray(), TestPath + @"\car\test.csv");

                StringBuilder TreeLayout = new StringBuilder();

                for (int depth = 1; depth < 7; depth++)
                {
                    ID3_Node Tree = ID3Tools.ID3(attributeCars, TrainCars, depth, ID3Tools.EntropyCalucalation.IG);
                    //add the tree to the string builder and prepare to write it to a file.

                    Double TrainError = ID3Tools.FindTestError(TrainCars, attributeCars, Tree);
                    Double TestError = ID3Tools.FindTestError(TestCars, attributeCars, Tree);

                    TreeLayout.Append("Information Gain Cars, Max Depth of " + depth + ". Test Error = " + TestError + ". TrainError = " + TrainError + " \n \n" + Tree.PrintTree(attributeCars.ToArray()) + "\n ----------------------------------------------------------------- \n");
                    Console.WriteLine("Finished an IG Tree");
                }

                for (int depth = 1; depth < 7; depth++)
                {
                    ID3_Node Tree = ID3Tools.ID3(attributeCars, TrainCars, depth, ID3Tools.EntropyCalucalation.GI);
                    //add the tree to the string builder and prepare to write it to a file.

                    Double TrainError = ID3Tools.FindTestError(TrainCars, attributeCars, Tree);
                    Double TestError = ID3Tools.FindTestError(TestCars, attributeCars, Tree);

                    TreeLayout.Append("Gini Index Cars, Max Depth of " + depth + ". Test Error = " + TestError + ". TrainError = " + TrainError + " \n \n" + Tree.PrintTree(attributeCars.ToArray()) + "\n ----------------------------------------------------------------- \n");
                    Console.WriteLine("Finished a GI Tree");
                }

                for (int depth = 1; depth < 7; depth++)
                {
                    ID3_Node Tree = ID3Tools.ID3(attributeCars, TrainCars, depth, ID3Tools.EntropyCalucalation.ME);
                    //add the tree to the string builder and prepare to write it to a file.

                    Double TrainError = ID3Tools.FindTestError(TrainCars, attributeCars, Tree);
                    Double TestError = ID3Tools.FindTestError(TestCars, attributeCars, Tree);

                    TreeLayout.Append("Majority Error Cars, Max Depth of " + depth + ". Test Error = " + TestError + ". TrainError = " + TrainError + " \n \n" + Tree.PrintTree(attributeCars.ToArray()) + "\n ----------------------------------------------------------------- \n");
                    Console.WriteLine("Finished an ME Tree");
                }

                Console.WriteLine("Writing all results to DecisionTree/TestingData/RunResults/ResultsCars.txt");
                System.IO.File.WriteAllText(TestPath+ @"/RunResults/ResultsCars.txt", TreeLayout.ToString());
            }

            // ========= Part 2 ============= //
            // bank information
            if (BuildBankTrees)
            {
                List<DAttribute> attributeBank = new List<DAttribute>(7);
                //Once again, could auto detect, but doing so makes the data harder to read. Furthermore, autodetecting doesn't work for filling in missing values.
                //below data descriptions come from data-desc.txt, located near the data for this training data.

                string[] AVariants;

                //age being numeric means that the actual variants will be figured out at run time. The variant will be overwritten when we pull in the testing data.
                attributeBank.Add(new DAttribute("age", 0, null, DAttribute.Type.BinaryNumeric, false));
                AVariants = new string[] {"admin.","unknown","unemployed","management","housemaid","entrepreneur","student",
                                       "blue-collar","self-employed","retired","technician","services" };
                attributeBank.Add(new DAttribute("job", 1, new List<string>(AVariants), DAttribute.Type.Categorical, false));
                AVariants = new string[] { "married", "divorced", "single" };
                attributeBank.Add(new DAttribute("marital", 2, new List<string>(AVariants), DAttribute.Type.Categorical, false));
                AVariants = new string[] { "unknown", "secondary", "primary", "tertiary" };
                attributeBank.Add(new DAttribute("education", 3, new List<string>(AVariants), DAttribute.Type.Categorical, false));
                AVariants = new string[] { "yes", "no" };
                attributeBank.Add(new DAttribute("default", 4, new List<string>(AVariants), DAttribute.Type.Categorical, false));

                attributeBank.Add(new DAttribute("balance", 5, null, DAttribute.Type.BinaryNumeric, false));
                AVariants = new string[] { "yes", "no" };
                attributeBank.Add(new DAttribute("housing", 6, new List<string>(AVariants), DAttribute.Type.Categorical, false));
                AVariants = new string[] { "yes", "no" };
                attributeBank.Add(new DAttribute("loan", 7, new List<string>(AVariants), DAttribute.Type.Categorical, false));
                AVariants = new string[] { "unknown", "telephone", "cellular" };
                attributeBank.Add(new DAttribute("contact", 8, new List<string>(AVariants), DAttribute.Type.Categorical, false));

                attributeBank.Add(new DAttribute("day", 9, null, DAttribute.Type.BinaryNumeric, false));
                AVariants = new string[] { "jan", "feb", "mar", "apr", "may", "jun", "jul", "aug", "sep", "oct", "nov", "dec" };
                attributeBank.Add(new DAttribute("month", 10, new List<string>(AVariants), DAttribute.Type.Categorical, false));

                attributeBank.Add(new DAttribute("duration", 11, null, DAttribute.Type.BinaryNumeric, false));

                attributeBank.Add(new DAttribute("campaign", 12, null, DAttribute.Type.BinaryNumeric, false));

                attributeBank.Add(new DAttribute("pdays", 13, null, DAttribute.Type.BinaryNumeric, false));

                attributeBank.Add(new DAttribute("previous", 14, null, DAttribute.Type.BinaryNumeric, false));
                AVariants = new string[] { "unknown", "other", "failure", "success" }; //If unknown needs to be filled in, remove it from this list.
                attributeBank.Add(new DAttribute("poutcome", 15, new List<string>(AVariants), DAttribute.Type.Categorical, false));
                AVariants = new string[] { "yes", "no" };
                attributeBank.Add(new DAttribute("result", 16, new List<string>(AVariants), DAttribute.Type.Categorical, true));

                if (BuildBankTreeNormal)
                {
                    List<Case> TrainBank = DRT.ParseCSV(attributeBank.ToArray(), TestPath + @"\bank\train.csv", true);
                    List<Case> TestBank = DRT.ParseCSV(attributeBank.ToArray(), TestPath + @"\bank\test.csv", false);

                    StringBuilder TreeLayout = new StringBuilder();

                    for (int depth = 1; depth < 17; depth++)
                    {
                        ID3_Node Tree = ID3Tools.ID3(attributeBank, TrainBank, depth, ID3Tools.EntropyCalucalation.IG);
                        //add the tree to the string builder and prepare to write it to a file.

                        Double TrainError = ID3Tools.FindTestError(TrainBank, attributeBank, Tree);
                        Double TestError = ID3Tools.FindTestError(TestBank, attributeBank, Tree);

                        TreeLayout.Append("Information Gain Bank, Max Depth of " + depth + ". Test Error = " + TestError + ". TrainError = " + TrainError + " \n \n" + Tree.PrintTree(attributeBank.ToArray()) + "\n ----------------------------------------------------------------- \n");
                        Console.WriteLine("Finished an IG Tree");
                    }

                    for (int depth = 1; depth < 17; depth++)
                    {
                        ID3_Node Tree = ID3Tools.ID3(attributeBank, TrainBank, depth, ID3Tools.EntropyCalucalation.GI);
                        //add the tree to the string builder and prepare to write it to a file.                        

                        Double TrainError = ID3Tools.FindTestError(TrainBank, attributeBank, Tree);
                        Double TestError = ID3Tools.FindTestError(TestBank, attributeBank, Tree);

                        TreeLayout.Append("Gini Index Bank, Max Depth of " + depth + ". Test Error = " + TestError + ". TrainError = " + TrainError + " \n \n" + Tree.PrintTree(attributeBank.ToArray()) + "\n ----------------------------------------------------------------- \n");
                        Console.WriteLine("Finished a GI Tree");
                    }

                    for (int depth = 1; depth < 17; depth++)
                    {
                        ID3_Node Tree = ID3Tools.ID3(attributeBank, TrainBank, depth, ID3Tools.EntropyCalucalation.ME);
                        //add the tree to the string builder and prepare to write it to a file.                        

                        Double TrainError = ID3Tools.FindTestError(TrainBank, attributeBank, Tree);
                        Double TestError = ID3Tools.FindTestError(TestBank, attributeBank, Tree);

                        TreeLayout.Append("Majority Error Bank, Max Depth of " + depth + ". Test Error = " + TestError + ". TrainError = " + TrainError + " \n \n" + Tree.PrintTree(attributeBank.ToArray()) + "\n ----------------------------------------------------------------- \n");
                        Console.WriteLine("Finished an ME Tree");
                    }

                    Console.WriteLine("Writing all results to DecisionTree/TestingData/RunResults/ResultsBankNormal.txt");
                    System.IO.File.WriteAllText(TestPath + @"/RunResults/ResultsBankNormal.txt", TreeLayout.ToString());
                }
                if (BuildBankMissingVals)
                {
                    //In this case, the "unknown" values in poutcome
                    attributeBank[15] = new DAttribute("poutcome", 15, new List<string>(new string[] { "unknown", "other", "failure", "success" }), DAttribute.Type.Categorical, false);

                    //Now we rebuild all the datasets, which will have elements filled in by the majority elements.
                    List<Case> TrainBank = DRT.ParseCSV(attributeBank.ToArray(), TestPath + @"\bank\train.csv", true);
                    List<Case> TestBank = DRT.ParseCSV(attributeBank.ToArray(), TestPath + @"\bank\test.csv", false);

                    StringBuilder TreeLayout = new StringBuilder();

                    for (int depth = 1; depth < 17; depth++)
                    {
                        ID3_Node Tree = ID3Tools.ID3(attributeBank, TrainBank, depth, ID3Tools.EntropyCalucalation.IG);
                        //add the tree to the string builder and prepare to write it to a file.

                        Double TrainError = ID3Tools.FindTestError(TrainBank, attributeBank, Tree);
                        Double TestError = ID3Tools.FindTestError(TestBank, attributeBank, Tree);

                        TreeLayout.Append("Information Gain Bank, Max Depth of " + depth + ". Test Error = " + TestError + ". TrainError = " + TrainError + " \n \n"  + Tree.PrintTree(attributeBank.ToArray()) + "\n ----------------------------------------------------------------- \n");
                        Console.WriteLine("Finished an IG Tree");
                    }

                    for (int depth = 1; depth < 17; depth++)
                    {
                        ID3_Node Tree = ID3Tools.ID3(attributeBank, TrainBank, depth, ID3Tools.EntropyCalucalation.GI);
                        //add the tree to the string builder and prepare to write it to a file.                        

                        Double TrainError = ID3Tools.FindTestError(TrainBank, attributeBank, Tree);
                        Double TestError = ID3Tools.FindTestError(TestBank, attributeBank, Tree);

                        TreeLayout.Append("Gini Index Bank, Max Depth of " + depth + ". Test Error = " + TestError + ". TrainError = " + TrainError + " \n \n" + Tree.PrintTree(attributeBank.ToArray()) + "\n ----------------------------------------------------------------- \n");
                        Console.WriteLine("Finished a GI Tree");
                    }

                    for (int depth = 1; depth < 17; depth++)
                    {
                        ID3_Node Tree = ID3Tools.ID3(attributeBank, TrainBank, depth, ID3Tools.EntropyCalucalation.ME);
                        //add the tree to the string builder and prepare to write it to a file.                        

                        Double TrainError = ID3Tools.FindTestError(TrainBank, attributeBank, Tree);
                        Double TestError = ID3Tools.FindTestError(TestBank, attributeBank, Tree);

                        TreeLayout.Append("Majority Error Bank, Max Depth of " + depth + ". Test Error = " + TestError + ". TrainError = " + TrainError + " \n \n" + Tree.PrintTree(attributeBank.ToArray()) + "\n ----------------------------------------------------------------- \n");
                        Console.WriteLine("Finished an ME Tree");
                    }

                    Console.WriteLine("Writing all results to DecisionTree/TestingData/RunResults/ResultsBankMissingVals.txt");
                    System.IO.File.WriteAllText(TestPath + @"/RunResults/ResultsBankMissingVals.txt", TreeLayout.ToString());
                }
            }
        }
    }
}
