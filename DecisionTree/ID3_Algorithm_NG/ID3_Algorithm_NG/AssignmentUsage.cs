using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ID3_Algorithm_NG
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
                List<Attribute> attributeCars = new List<Attribute>(7);
                //while I could auto detect this, it's much easier to read the trees if I name the Attributes ahead of time
                //below data descriptions come from data-desc.txt, located near the data for this training data.
                string[] AVariants = new string[] { "vhigh", "high", "med", "low" }; //array of attribute variants to pass in to an attribute


                attributeCars.Add(new Attribute("buying", 0, new List<string>(AVariants), false, false));
                attributeCars.Add(new Attribute("maint", 1, new List<string>(AVariants), false, false));

                AVariants = new string[] { "2", "3", "4", "5more" };
                attributeCars.Add(new Attribute("doors", 2, new List<string>(AVariants), false, false));
                AVariants = new string[] { "2", "4", "more" };
                attributeCars.Add(new Attribute("persons", 3, new List<string>(AVariants), false, false));
                AVariants = new string[] { "small", "med", "big" };
                attributeCars.Add(new Attribute("lug_boot", 4, new List<string>(AVariants), false, false));
                AVariants = new string[] { "low", "med", "high" };
                attributeCars.Add(new Attribute("safety", 5, new List<string>(AVariants), false, false));
                AVariants = new string[] { "unacc", "acc", "good", "vgood" };
                attributeCars.Add(new Attribute("label", 6, new List<string>(AVariants), false, true));

                
                List<Case> TrainCars = ID3Tools.ParseCSV(attributeCars.ToArray(), TestPath + @"\car\train.csv");
                List<Case> TestCars = ID3Tools.ParseCSV(attributeCars.ToArray(), TestPath + @"\car\test.csv");

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
                List<Attribute> attributeBank = new List<Attribute>(7);
                //Once again, could auto detect, but doing so makes the data harder to read. Furthermore, autodetecting doesn't work for filling in missing values.
                //below data descriptions come from data-desc.txt, located near the data for this training data.

                string[] AVariants;

                //age being numeric means that the actual variants will be figured out at run time. The variant will be overwritten when we pull in the testing data.
                attributeBank.Add(new Attribute("age", 0, null, true, false));
                AVariants = new string[] {"admin.","unknown","unemployed","management","housemaid","entrepreneur","student",
                                       "blue-collar","self-employed","retired","technician","services" };
                attributeBank.Add(new Attribute("job", 1, new List<string>(AVariants), false, false));
                AVariants = new string[] { "married", "divorced", "single" };
                attributeBank.Add(new Attribute("marital", 2, new List<string>(AVariants), false, false));
                AVariants = new string[] { "unknown", "secondary", "primary", "tertiary" };
                attributeBank.Add(new Attribute("education", 3, new List<string>(AVariants), false, false));
                AVariants = new string[] { "yes", "no" };
                attributeBank.Add(new Attribute("default", 4, new List<string>(AVariants), false, false));

                attributeBank.Add(new Attribute("balance", 5, null, true, false));
                AVariants = new string[] { "yes", "no" };
                attributeBank.Add(new Attribute("housing", 6, new List<string>(AVariants), false, false));
                AVariants = new string[] { "yes", "no" };
                attributeBank.Add(new Attribute("loan", 7, new List<string>(AVariants), false, false));
                AVariants = new string[] { "unknown", "telephone", "cellular" };
                attributeBank.Add(new Attribute("contact", 8, new List<string>(AVariants), false, false));

                attributeBank.Add(new Attribute("day", 9, null, true, false));
                AVariants = new string[] { "jan", "feb", "mar", "apr", "may", "jun", "jul", "aug", "sep", "oct", "nov", "dec" };
                attributeBank.Add(new Attribute("month", 10, new List<string>(AVariants), false, false));

                attributeBank.Add(new Attribute("duration", 11, null, true, false));

                attributeBank.Add(new Attribute("campaign", 12, null, true, false));
                
                attributeBank.Add(new Attribute("pdays", 13, null, true, false));

                attributeBank.Add(new Attribute("previous", 14, null, true, false));
                AVariants = new string[] { "unknown", "other", "failure", "success" }; //If unknown needs to be filled in, remove it from this list.
                attributeBank.Add(new Attribute("poutcome", 15, new List<string>(AVariants), false, false));
                AVariants = new string[] { "yes", "no" };
                attributeBank.Add(new Attribute("result", 16, new List<string>(AVariants), false, true));

                if (BuildBankTreeNormal)
                {
                    List<Case> TrainBank = ID3Tools.ParseCSV(attributeBank.ToArray(), TestPath + @"\bank\train.csv", true);
                    List<Case> TestBank = ID3Tools.ParseCSV(attributeBank.ToArray(), TestPath + @"\bank\test.csv", false);

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
                    attributeBank[15] = new Attribute("poutcome", 15, new List<string>(new string[] { "unknown", "other", "failure", "success" }), false, false);

                    //Now we rebuild all the datasets, which will have elements filled in by the majority elements.
                    List<Case> TrainBank = ID3Tools.ParseCSV(attributeBank.ToArray(), TestPath + @"\bank\train.csv", true);
                    List<Case> TestBank = ID3Tools.ParseCSV(attributeBank.ToArray(), TestPath + @"\bank\test.csv", false);

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
