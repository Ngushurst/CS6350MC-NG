using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using ID3_Algorithm_NG;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {
        public string TestPath = @"..\..\..\..\..\TestingData";

        [TestMethod]
        public void TestAutoDetect()
        {
            
            List<Attribute> attributes = new List<Attribute>();
            //read in a simple file with 7 examples with 5 attributes
            List<Case> TestData = ID3Tools.ParseCSV(TestPath + @"\simple\simple1.txt", out attributes);

            Assert.AreEqual(7, TestData.Count);
            Assert.AreEqual(5, attributes.Count);
        }

        [TestMethod]
        public void TestSimple()
        {
            //Initialize the attributes beforehand to make it more readable when debugging

            List<Attribute> attributes = new List<Attribute>(5);
            string[] alphabet = new string[2];
            alphabet[0] = "0"; alphabet[1] = "1";
            attributes.Add(new Attribute("X_1", 0, new List<string>(alphabet), false, false));
            attributes.Add(new Attribute("X_2", 1, new List<string>(alphabet), false, false));
            attributes.Add(new Attribute("X_3", 2, new List<string>(alphabet), false, false));
            attributes.Add(new Attribute("X_4", 3, new List<string>(alphabet), false, false));
            attributes.Add(new Attribute("VarFinal", 4, new List<string>(alphabet), false, true));

            List<Case> TestData = ID3Tools.ParseCSV(attributes.ToArray(), TestPath + @"\simple\simple1.txt");


            ID3_Node Tree = ID3Tools.ID3(attributes, TestData, 999, ID3Tools.EntropyCalucalation.IG);

            System.Console.WriteLine(Tree.PrintTree(attributes.ToArray()));

            Assert.AreEqual(0, ID3Tools.TestWithTree(TestData[6], Tree));
        }

        [TestMethod]
        public void TestDepthLimit()
        {
            List<Attribute> attributes = new List<Attribute>(5);
            string[] alphabet = new string[2];
            alphabet[0] = "0"; alphabet[1] = "1";
            attributes.Add(new Attribute("X_1", 0, new List<string>(alphabet), false, false));
            attributes.Add(new Attribute("X_2", 1, new List<string>(alphabet), false, false));
            attributes.Add(new Attribute("X_3", 2, new List<string>(alphabet), false, false));
            attributes.Add(new Attribute("X_4", 3, new List<string>(alphabet), false, false));
            attributes.Add(new Attribute("VarFinal", 4, new List<string>(alphabet), false, true));

            List<Case> TestData = ID3Tools.ParseCSV(attributes.ToArray(), TestPath + @"\simple\simple1.txt");


            ID3_Node Tree = ID3Tools.ID3(attributes, TestData, 1, ID3Tools.EntropyCalucalation.IG);
            //it works

            System.Console.WriteLine(Tree.PrintTree(attributes.ToArray()));
            int i = 0; // a line of code on which to wait afterwards.
        }
    }
}
