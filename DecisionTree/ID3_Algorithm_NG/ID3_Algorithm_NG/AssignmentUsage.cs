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

        public static void Main()
        {
            // ========= Part 1 ============= //

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

            
            List<Case> DataCars = ID3Tools.ParseCSV(attributeCars.ToArray(), TestPath + @"\car\train.csv");
            List<Case> TestCars = ID3Tools.ParseCSV(attributeCars.ToArray(), TestPath + @"\car\test.csv");

            StringBuilder TreeLayout = new StringBuilder();


            for(int depth = 1; depth<7; depth++)
            {
                ID3_Node Tree = ID3Tools.ID3(attributeCars, DataCars, depth, ID3Tools.EntropyCalucalation.IG);
                //add the tree to the string builder and prepare to write it to a file.

                Double TrainError = ID3Tools.FindTestError(DataCars, attributeCars, Tree);
                Double TestError = ID3Tools.FindTestError(TestCars, attributeCars, Tree);

                TreeLayout.Append("Information Gain Cars, Max Depth of " + depth + ". Test Error = "+ TestError + ". TrainError = " + TrainError +" \n \n" /*+ Tree.PrintTree(attributeCars.ToArray())*/ + "\n ----------------------------------------------------------------- \n");
                Console.WriteLine("Finished an IG Tree");
            }

            for (int depth = 1; depth < 7; depth++)
            {
                ID3_Node Tree = ID3Tools.ID3(attributeCars, DataCars, depth, ID3Tools.EntropyCalucalation.GI);
                //add the tree to the string builder and prepare to write it to a file.

                Double TrainError = ID3Tools.FindTestError(DataCars, attributeCars, Tree);
                Double TestError = ID3Tools.FindTestError(TestCars, attributeCars, Tree);

                TreeLayout.Append("Gini Index Cars, Max Depth of " + depth + ". Test Error = " + TestError + ". TrainError = " + TrainError + " \n \n" /*+ Tree.PrintTree(attributeCars.ToArray())*/ + "\n ----------------------------------------------------------------- \n");
                Console.WriteLine("Finished a GI Tree");
            }
            
            for (int depth = 1; depth < 7; depth++)
            {
                ID3_Node Tree = ID3Tools.ID3(attributeCars, DataCars, depth, ID3Tools.EntropyCalucalation.ME);
                //add the tree to the string builder and prepare to write it to a file.

                Double TrainError = ID3Tools.FindTestError(DataCars, attributeCars, Tree);
                Double TestError = ID3Tools.FindTestError(TestCars, attributeCars, Tree);

                TreeLayout.Append("Majority Error Cars, Max Depth of " + depth + ". Test Error = " + TestError + ". TrainError = " + TrainError + " \n \n" /*+ Tree.PrintTree(attributeCars.ToArray())*/ + "\n ----------------------------------------------------------------- \n");
                Console.WriteLine("Finished an ME Tree");
            }

            Console.WriteLine("Writing all results to ResultsCars.txt");
            System.IO.File.WriteAllText(@"ResultsCars.txt", TreeLayout.ToString());

            // ========= Part 2 ============= //
            // bank information
        }
    }
}
