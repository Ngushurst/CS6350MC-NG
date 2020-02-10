using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ID3_Algorithm_NG
{

    /// <summary>
    /// Primary class defining static methods by which to build and test decision trees.
    /// </summary>
    public class ID3Tools
    {
        /// <summary>
        /// Creates the root of a decision tree and recursively builds the rest.
        /// </summary>
        public static ID3_Node ID3(List<Attribute> attributes, List<Case> data, int DepthRemaining, EntropyCalucalation calc)
        {
            if (DepthRemaining > -1 && attributes.Count > 1 && data.Count > 0) // Check more layers are allowed and if there are usable attributes remaining, and that there's at least one data point
            {
                //The next line assumes that the last attribute is always the final one, and the only final one.
                //Attributes that are final are marked, so you should be able to change that behavior very easily.
                double[] proportions = GetLabelDistribution(data, attributes.Last());
                double entropy = CalculateEntropy(proportions, calc);
                if (entropy == 0) //set only has one output label
                {
                    int result = 0; //default
                    for (int i = 0; i < proportions.Length; i++)
                    {//track down the item that makes up the whole set
                        if (proportions[i] > 0) //found the only one that's greater than 0
                        {
                            result = i;
                            break;
                        }
                    }
                    return ID3_Node.Leaf(attributes.Last().ID, result); //Entire data set is of the same label. Tree is a single leaf node. Return a leaf node.
                }
                else
                { //find the attribute that results in the lowest entropy and divide on that.
                    List<Case>[] LowestEntropySets = null; //default
                    double LowestEntropy = double.PositiveInfinity; //max value by default.
                    int BestAttNum = 0; //default
                    int BestAttID = -1;

                    for (int a = 0; a < attributes.Count-1; a++) //check all attributes, but the last (that's the label, and we don't want to judge by that.)
                    {
                        int numVars = attributes[a].numVariants();
                        int aID = attributes[a].ID; //refers to the position in a case's data to find the desired attribute value.
                        List<Case>[] CurrentSets = new List<Case>[numVars];

                        for (int i = 0; i < numVars; i++) //initialize all the lists
                        {
                            CurrentSets[i] = new List<Case>();
                        }

                        foreach (Case c in data)// populate lists by dividing by attribute value
                        {
                            CurrentSets[c.AttributeVals[aID]].Add(c); //add the case to a list pertaining to its attribute value
                        }

                        //now that the data is split, calculate each set's entropy, weight it, and recombine it all

                        double sumEntropy = 0;

                        foreach (List<Case> set in CurrentSets)
                        {
                            //We're doing the same entropy calculation as the one at the top. We need to know the distribution of outputs.
                            double weight = (double)(set.Count) / (double) data.Count; //percentage of data represented by 'set'
                            if (set.Count > 0)
                            {
                                double[] distribution = GetLabelDistribution(set, attributes.Last());
                                sumEntropy += weight * CalculateEntropy(distribution, calc); //weighted entropy value
                            }
                        }

                        if (sumEntropy < LowestEntropy) //Lowest entropy so far
                        {// record keep track of the sets created and attribute number
                            LowestEntropy = sumEntropy; //if equal, favor the pre-existing value
                            LowestEntropySets = CurrentSets;
                            BestAttNum = a;
                            BestAttID = aID;
                        }

                        //check all of them. \_:)_/
                    }

                    List<ID3_Node> Children = new List<ID3_Node>();
                    List<Attribute>UpdatedAttributes = attributes.ToList(); //Copy the list and remove the winning attribute (dividing by it again wouldn't be helpful.)
                    UpdatedAttributes.RemoveAt(BestAttNum);
                    //create children recursively by use of the ID3 algorithm
                    for (int i = 0; i < LowestEntropySets.Length; i++)
                    {
                        //Notice how we're copying the list before we pass it in. Things get weird if you don't do that.
                        ID3_Node child = ID3(UpdatedAttributes.ToList(), LowestEntropySets[i], DepthRemaining - 1, calc);
                        //if (!ReferenceEquals(child, null)) //if the child is not null, add it to the list
                        //{// if we let the children be null, then it maintains order in the child array.
                            Children.Add(child);
                        //}
                    }

                    //point null children to real children. In effect, the tree guesses where to go in the event that values not present in the training data show up.
                    List<int> nullChildren = new List<int>();
                    int nonNullChild = 0;

                    for (int i = 0; i<Children.Count; i++)
                    {
                        if(Children[i] == null)
                        {
                            nullChildren.Add(i);
                        }
                        else
                        {
                            nonNullChild = i;
                        }
                    }
                    
                    foreach(int i in nullChildren)
                    {
                        Children[i] = Children[nonNullChild];
                    }

                    ID3_Node output = new ID3_Node(BestAttID);

                    if (nullChildren.Count == Children.Count) //All children are null
                    {
                        double[] proportion = GetLabelDistribution(data, attributes.Last());
                        double max = 0;
                        int mode = -1; //number representing most common Final label in the current dataset
                        for (int i = 0; i<proportion.Length; i++)
                        {
                            if (proportion[i] > max) {
                                max = proportion[i];
                                mode = i;
                            }
                        }
                        return ID3_Node.Leaf(attributes.Last().ID, mode);
                    }


                    else{ //add children to 
                        output.addChildren(Children.ToArray());
                    }
                    return output;
                }
            }
            //Only reach this spot if the program builds the tree beyond the depth limit. Return null.
            return null;
        }

        /// <summary>
        /// Parse CSV with a pregenerated list of attributes, except that this one will define numeric attributes based on the data if true is passed in as the third parameter
        /// </summary>
        /// <param name="Attributes"></param>
        /// <param name="Filepath"></param>
        /// <param name="defineNumericAtt"></param>
        /// <returns></returns>
        public static List<Case> ParseCSV(Attribute[] Attributes, String Filepath, bool defineNumericAtt)
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


            List<Case> data = new List<Case>(rawCases.Length);
            List<int> unidentifiedID = new List<int>(); //stores the ID's of all the unidentified cases

            if (defineNumericAtt)
            {
                //look for numeric attributes and find the medians for them.
                foreach (Attribute att in Attributes)
                {
                    if (att.IsNumeric())
                    {
                        List<int> NumValues = new List<int>(input.Length);
                        foreach (String[] vals in rawCases)
                        {
                            NumValues.Add(int.Parse(vals[att.ID])); //take the corresponding value for the attribute and put it into the list
                        }
                        //now that we have all the values, find the median.
                        NumValues.Sort();
                        int median = NumValues[NumValues.Count / 2]; //go halfway through to get the median.
                        att.setNumericVariants(median); //generate the two variants for the numeric attribute
                    }
                }
            }
            for (int i = 0; i < rawCases.Length; i++)
            {
                String[] current = rawCases[i];
                int[] caseVars = new int[current.Length]; //array to hold all the relevant values in the case


                if (current.Length == 0)
                { //blank data likely at the start or end
                    continue;
                }

                for (int j = 0; j < current.Length; j++)
                {
                    int varID = Attributes[j].GetVarID(current[j]);
                    if (varID == -1) //unrecognized attribute value. Assumed to be unknown and will be replaced by a fractional count
                    {
                        unidentifiedID.Add(i); //toss a reference to it onto this stack. Will add multiple copies of the same number for multiple missing values
                    }
                    caseVars[j] = varID; //record the variant number by the attribute


                }

                data.Add(new Case(i, caseVars)); //add the case to the list
            }//repeat until we run out of data.


            if (unidentifiedID.Count > 0)
            {//If there are items that have unidentified attribute values, replace them with fractional counts.
                if (false) //Create fractional cases to fill in missing values.
                {
                    return FractionalValueFilling(unidentifiedID, data, Attributes);
                }
                else if (true) //Default to filling missing values with the majority value.
                {
                    return MajorityValueFilling(unidentifiedID, data, Attributes);
                }
            }

            //return data
            return data;
        }

        /// <summary>
        /// Given a list of Case IDs that have missing attribute values, the list of cases, and the attributes that the cases are described by, fill
        /// in the missing values by taking fractional counts (dividing one missing value into all values based on the weight in the data). On a side note,
        /// this does not work when cases are missing more than one value.
        /// </summary>
        /// <param name="unidentifiedID"></param>
        /// <param name="data"></param>
        /// <param name="Attributes"></param>
        /// <returns></returns>
        private static List<Case> FractionalValueFilling(List<int> unidentifiedID, List<Case> data, Attribute[] Attributes)
        {
            List<Case> FractionalCases = new List<Case>();

            List<int> CasesToDelete = new List<int>(); //a list of cases to remove once we've built all of their replacements

            int lastID = -1; //an integer to make sure that we don't try to operate on the same item multiple times in the case of it missing values. \_:)_/ This is buggy, but I don't think it's a problem for this assignment

            for (int a = 0; a < Attributes.Length - 2; a++) //go through all the attributes (not including final ones) and check for missing values
            {
                List<int> CasesToReplicate = new List<int>(); //holds the list of case ID's to replicate on the current attribute

                for (int IDnum = 0; IDnum < unidentifiedID.Count; IDnum++) //collect all missing values
                {
                    if (unidentifiedID[IDnum] != lastID && data[unidentifiedID[IDnum]].AttributeVals[a] == -1) //attribute is undefined, and we haven't already maked this one
                    {
                        CasesToReplicate.Add(unidentifiedID[IDnum]); //record so we can replicate it later
                        CasesToDelete.Add(unidentifiedID[IDnum]); //Put away the index and hold on to it later
                        unidentifiedID.RemoveAt(IDnum); //We've found a missing value, so remove it
                    }
                }

                // found all cases with missing information. Now to tally up the percentage of all the variants
                double[] Weights = GetLabelDistribution(data, Attributes[a]); // the distribution works for any attribute. Apologies for the poor labeling


                // I am deeply sorry about this loop. It is very, very hard to read.
                // In summary, it goes through all of the designated cases to replicate them, copies their attributes while varying the unknown one,
                // and then uses that copied array of attributes to make a new Case with a weight notated by the position of i in weights.
                foreach (int Fc in CasesToReplicate)
                {
                    for (int i = 0; i < Weights.Length; i++) //there is one weight for each attribute variant
                    {
                        int[] newVals = new int[Attributes.Length]; //create a replacement array
                        for (int j = 0; j < Attributes.Length; j++)
                        {
                            if (j == a) //if the item is known to be unkown, fill it in with the current variant (i)
                            {
                                newVals[j] = i;
                            }
                            else
                            {
                                newVals[j] = data[Fc].AttributeVals[j]; //copy the item if the item was previous defined.
                            }
                        }
                        FractionalCases.Add(new Case(Fc, newVals, Weights[i])); //add the copy to fractional cases.
                    }
                }

            }
            //Now fractional cases holds all of the new weighted cases.
            //Cases to delete holds the id's of all of the cases that need to be replaced.

            CasesToDelete.Sort(); //sort the cases to delete from least to greatest.
            int lastRemoved = -1; //keep track of the last ID that was removed
            int numRemoved = 0;
            for (int i = 0; i < CasesToDelete.Count; i++)
            {
                if (lastRemoved == CasesToDelete[i]) //if we've already removed this one. Try again on the next one.
                {
                    continue;
                }
                data.RemoveAt(CasesToDelete[i] - numRemoved); //since the size will go down as the cases go up, we need to shrink the ID accordingly.
                numRemoved++; //removed another one
                lastRemoved = CasesToDelete[i]; //Keep track of what removed was last
            }

            //now that we've gotten rid of all the old stuff, dump in all the fractional cases.
            data.AddRange(FractionalCases);
            return data;
        }

        /// <summary>
        /// Given a list of Case IDs that have missing attribute values, the list of cases, and the attributes that the cases are described by, fill
        /// in the missing values by replacing them with the most common value for that attribute. Works when cases are missing multiple values.
        /// </summary>
        /// <param name="unidentifiedID"></param>
        /// <param name="data"></param>
        /// <param name="Attributes"></param>
        /// <returns></returns>
        private static List<Case> MajorityValueFilling(List<int> unidentifiedID, List<Case> data, Attribute[] Attributes)
        {
            int lastID = -1; //an integer to make sure that we don't try to operate on the same item multiple times in the case of it missing multiple values. \_:)_/ This is buggy, but I don't think it's a problem for this assignment

            for (int a = 0; a < Attributes.Length - 2; a++) //go through all the attributes (not including final ones) and check for missing values
            {
                List<int> CasesToFill = new List<int>(); //holds the list of case ID's to fill on the current attribute

                for (int IDnum = 0; IDnum < unidentifiedID.Count; IDnum++) //collect all missing values
                {
                    if (unidentifiedID[IDnum] != lastID && data[unidentifiedID[IDnum]].AttributeVals[a] == -1) //attribute is undefined, and we haven't already maked this one
                    {
                        CasesToFill.Add(unidentifiedID[IDnum]); //record so we can replicate it later
                        unidentifiedID.RemoveAt(IDnum); //We've found a missing value, so remove it
                    }
                }

                // found all cases with missing information. Now to tally up the percentage of all the variants
                double[] Weights = GetLabelDistribution(data, Attributes[a]); // the distribution works for any attribute. Apologies for the poor labeling

                int MajorityElement = 0; //figure out which variant is the most common (highest weight)
                double highest = 0;
                for (int i = 0; i < Weights.Length; i++)
                {
                    if(highest < Weights[i])
                    {
                        highest = Weights[i];
                        MajorityElement = i;
                    }
                }
                // I am deeply sorry about this loop. It is very, very hard to read.
                // In summary, it goes through all of the designated cases to replicate them, copies their attributes while varying the unknown one,
                // and then uses that copied array of attributes to make a new Case with a weight notated by the position of i in weights.
                foreach (int Fc in CasesToFill)
                {
                    int[] newVals = new int[Attributes.Length]; //create a replacement array
                    for (int j = 0; j < Attributes.Length; j++)
                    {
                        if (j == a) //if the item is known to be unkown, fill it in with the most common item overall.
                        {
                            newVals[j] = MajorityElement; 
                        }
                        else
                        {
                            newVals[j] = data[Fc].AttributeVals[j]; //copy the item if the item was previous defined.
                        }
                    }
                    data[Fc] = new Case(Fc, newVals); //replace the old case with a case that's more complete
                }

            }
            //return data now that all of the incomplete data has been replaced.

            return data;
        }

        /// <summary>
        /// Builds a List of cases with a pregennerated list of attributes. Will not define Numeric attributes with the data.
        /// </summary>
        /// <returns></returns>
        public static List<Case> ParseCSV(Attribute[] Attributes, String Filepath)
        {
            //will not define numeric attributes
            return ParseCSV(Attributes, Filepath, false);
        }

        /// <summary>
        /// Parses a CSV and tries to automatically detect and build attributes. Returns a list of cases and populates the given list of attributes.
        /// </summary>
        /// <returns></returns>
        public static List<Case> ParseCSV(String Filepath, out List<Attribute> attributes)
        {
            //read all lines of the CSV file
            String[] rawCases = System.IO.File.ReadAllLines(Filepath);

            if(rawCases.Length == 0)
            {
                throw new MissingFieldException("File must have at least one data point");
            }

            String[] example = rawCases[0].Split(',');
            attributes = new List<Attribute>(example.Length); //initialize attributes

            for (int i = 0; i<example.Length; i++) //check the first item to find the number of attributes
            { //create attributes and intialize their lists of variants. Populate attributes
                if (i < example.Length - 1)
                {
                    attributes.Add(new Attribute("Var" + i, i, new List<String>(), false, false));
                }
                else
                {//last attribute assumed by default to be final
                    attributes.Add(new Attribute("Var" + i, i, new List<String>(), false, true));
                }

            }
            List<Case> data = new List<Case>(rawCases.Length);
            for (int i = 0; i < rawCases.Length; i++)
            {
                String[] current = rawCases[i].Split(',');
                int[] caseVars = new int[current.Length]; //array to hold all the relevant values in the case


                if (current.Length == 0)
                { //blank data likely at the start or end
                    continue;
                }

                for(int j = 0; j<current.Length; j++)
                {
                    attributes[j].addVariant(current[j]); //record attribute
                    caseVars[j] = attributes[j].GetVarID(current[j]); //record the variant number by the attribute now that it's in
                }

                data.Add(new Case(i, caseVars)); //add the case to the list
            }//repeat until we run out of data.

            return data;
        }

        /// <summary>
        /// Finds the error of the input tree according to the test labels.
        /// </summary>
        /// <param name="TestFilepath"></param>
        /// <param name=""></param>
        /// <returns></returns>
        public static double FindTestError(List<Case> TestCases, List<Attribute> attributes, ID3_Node Tree)
        {
            double RightAnswers = 0;

            foreach(Case C in TestCases)
            {
                if(C.AttributeVals.Last() == TestWithTree(C, Tree))
                {
                    RightAnswers += C.Weight; //add the case's weight (usually one)
                }
            }

           return 1.0 - (RightAnswers/(double) TestCases.Count);
        }


        /// <summary>
        /// Given a Case by which to test with a ID3_Node representing a decision tree, find the tree's label variant number for the Case and return it.
        /// </summary>
        /// <returns></returns>
        public static int TestWithTree(Case test, ID3_Node Tree)
        {
            if (ReferenceEquals(Tree.getChildren(), null))
                return Tree.Value;
            /*
            if(Tree.getChildren()[test.AttributeVals[Tree.AttributeID]] == null) //if the child is null pick another value that is valid. (example not present in training data)
            {
                if(Tree.getChildren().Length == test.AttributeVals[Tree.AttributeID])
            }*/
            
            return TestWithTree(test, Tree.getChildren()[test.AttributeVals[Tree.AttributeID]]);
        }

        //Here are the three variations on the entropy calculations
            private static double InformationGain(double[] numLabelType)
            {
                double sum = 0.0;
                for(int i = 0; i< numLabelType.Length; i++)
                { //Entropy is the sum of all i*ln(i), where i is the proportion of a given label.
                    if (numLabelType[i] == 0) continue; //skip as ln(0) * 0 = 0 * infinity. Still zero, but the algorithm doesn't know that.
                    sum -= numLabelType[i] * Math.Log(numLabelType[i]);
                }

                return sum;
            }

            private static double GiniIndex(double[] numLabelType)
            {//entropy = 1 - sum((p_i)^2)
                double total = 1.0; //starts at one and goes down from there.
                for (int i = 0; i < numLabelType.Length; i++)
                { //Entropy is the sum of all i*ln(i), where i is the proportion of a given label.
                    total -= Math.Pow(numLabelType[i],2);
                }

                return total;
            }

            private static double MajorityError(double[] numLabelType) //note to self: can unwravel loop by one degree to allow higher optimization.
            { //majority error = 0 + (nonMajority examples / total examples), or better :  1 - (Majority examples / total examples)
                double majorityProportion = 0;
                for (int i = 0; i < numLabelType.Length; i++)
                {
                    majorityProportion = Math.Max(majorityProportion, numLabelType[i]); //labels already in proportions. Find highest.
                }
                return 1 - majorityProportion;
            }

        /// <summary>
        /// Given an array of the number of occurences of each label, calculate entopy using the method indicated by EntropyCalculation.
        /// Throws an InvalidOperationException if given an invalid EntropyCalcuation.
        /// </summary>
        /// <param name="numLabelType"></param>
        /// <returns>Well it should return a number between zero and one, but the methods don't seem to work that way. However,
        /// high entropy is still high while zero is still the minimum, so it still works well enough.</returns>
        public static double CalculateEntropy(double[] numLabelType, EntropyCalucalation CalcType)
        { //Jackson suggested I use an enum. It's defined at the bottom of the class if you're wondering. I like the look of it.
            switch (CalcType)
            {
                case EntropyCalucalation.IG:
                    return InformationGain(numLabelType);
                case EntropyCalucalation.GI:
                    return GiniIndex(numLabelType);
                case EntropyCalucalation.ME:
                    return MajorityError(numLabelType);
            }

            //No valid calcuation type detected.
            throw new InvalidOperationException("CalculateEntropy() must have a designated EntroypyCalculation.");
        }

        /// <summary>
        /// Calculates the Final label (output, found in data as #attributeNum) purity for each variant of a dataset and returns it.
        /// </summary>
        /// <param name="Data"></param>
        /// <returns></returns>
        public static double[] GetLabelDistribution(List<Case> Data, Attribute attribute)
        {
            int numVars = attribute.numVariants();
            double[] output = new double[numVars];

            foreach (Case c in Data)
            {
                int AVal = c.AttributeVals[attribute.ID]; // the varID of the attribute value held by C
                if (AVal <= -1)
                {
                    continue; //value is undefined. proceed to the next value
                }
                output[AVal]+= c.Weight; //increment the corresponding attribute variant by the case's weight (summing number of hits for each)
            }

            for (int i = 0; i<numVars; i++) //divide each by count to get the relative proportion of the label as oppsed to the count.
            {
                output[i] = output[i] / Data.Count;
            }

            return output;
        }

        public enum EntropyCalucalation
        {
            IG,
            GI,
            ME
        }
    }


    /// <summary>
    /// A node inside an ID3 decision tree. Main body nodes have a parent, an int referring to an attribute on which to split the data,
    /// and children. Leaf nodes have a null children field, and relevant value for their final label attribute (usually accept/reject).
    /// </summary>
    public class ID3_Node
    {
        private ID3_Node Parent;
        private ID3_Node[] Children;
        //int Depth;
        public readonly int AttributeID; //Int referring to where the attribute was in the original training set attribute order. (Order read in)
        public int Value; // Notates which Final value is represented by this node if it is a leaf note. -1 by default (invalid).

        public ID3_Node(int attributeid)
        {
            AttributeID = attributeid;
        }

        /// <summary>
        /// Uses input params to generate a leaf node where FinalAttributeID points to a final attribue and FinalValue refers to said attribute's variant. 
        /// </summary>
        /// <returns></returns>
        public static ID3_Node Leaf(int FinalAttributeID, int FinalValue)
        {
            ID3_Node output = new ID3_Node(FinalAttributeID);
            output.Value = FinalValue;

            return output;
        }

        /// <summary>
        /// Gets the depth of the given node in the tree.
        /// </summary>
        /// <returns></returns>
        public int depth()
        {
            if (Object.ReferenceEquals(Parent, null))
            {
                return 0; //depth zero at the root
            }
            return Parent.depth() + 1; //the depth of any node is one more than its parent's
        }

        /// <summary>
        /// Records an array of input childnodes and sets current node as their parent. Will overwrite current child nodes.
        /// </summary>
        /// <param name="children"></param>
        public void addChildren(ID3_Node[] children)
        {
            foreach(ID3_Node c in children)
            {
                c.Parent = this; //set child's parent to current node.
            }
            Children = children;
        }

        public ID3_Node[] getChildren()
        {
            return Children;
        }

        public String WriteToString(Attribute[] attributes)
        {
            String output = "";
            int depth = this.depth();

            for(int i = 0; i < depth; i++)
            {
                output += "\t";
            }

            output += attributes[AttributeID].Name + " " + depth + "\t";

            if(Children == null)
            {
                output += "FinalLabel == " + attributes[AttributeID].GetVariants()[Value];
            }

            output += "\n";
            return output;
        }

        /// <summary>
        /// Prints information about the current node and all nodes below it.
        /// </summary>
        /// <returns></returns>
        public String PrintTree(Attribute[] attributes)
        {
            StringBuilder output = new StringBuilder();
            output.Append(this.WriteToString(attributes));
            if (!ReferenceEquals(this.Children, null))
            {
                foreach (ID3_Node N in Children)
                {
                    output.Append(N.PrintTree(attributes));
                }
            }
            
            return output.ToString();
        }
    }

    /// <summary>
    /// An attribute by which to differentiate a case of data. Each attribute has a name, array of differentiable forms it may take (Variants),
    /// and will note if it represents a numerical range or final tag. The range of a numeric attribute must be defined when examining the training data.
    /// </summary>
    public class Attribute
    {
        public readonly String Name;
        public readonly int ID; //ID refers to the position in the order of the data's representation
        private List<String> Variants;
        private bool Numeric;
        private int median; //for numeric attributes only. Variants are greater than or equal, or less than the median.
        private bool Final; // If true, this attribute represents a result as opposed to a distinguishing feature.

        /// <summary>
        /// Build a new attribute with all input fields. As a note, it's fine to pass in null for the variants of a numeric attribute
        /// as the program must find the median of the training data set to define that (done in parse CSV when you pass in the pre-made attributes)
        /// </summary>
        /// <param name="name"></param>
        /// <param name="id"></param>
        /// <param name="variants"></param>
        /// <param name="numeric"></param>
        /// <param name="final"></param>
        public Attribute(String name, int id, List<String> variants, bool numeric, bool final)
        {
            Name = name;
            ID = id;
            Variants = variants;
            Numeric = numeric;
            Final = final;
        }

        /// <summary>
        /// If true, this attribute represents a final tag, meaning that it is defined by the data and cannot be used to distinguish cases in the tree.
        /// Such attributes are often the desired output, such as knowing whether an item is valid or invalid.
        /// </summary>
        /// <returns></returns>
        public bool IsFinal()
        {
            return Final;
        }

        /// <summary>
        /// If true, this attribute represents two ranges of numbers (has two )
        /// </summary>
        /// <returns></returns>
        public bool IsNumeric()
        {
            return Numeric;
        }

        /// <summary>
        /// Mostly for use regarding a numeric attribute, since it needs to be set after looking over all the data.
        /// </summary>
        /// <param name="newVars"></param>
        public void setNumericVariants(int median)
        {
            this.median = median;
            List<String> newVars = new List<string>(2);
            newVars.Add("<" + median);
            newVars.Add(">=" + median);
            Variants = newVars;
        }

        /// <summary>
        /// Takes in a string representing a variant of the current attribute and tries to add it to the current list. Used in simple autodetection,
        /// and thus is not used for numeric data or data with missing parameters.
        /// </summary>
        /// <param name="variant">A string representing a potentially new variant of this attribute</param>
        /// <returns>True if the variant was able to be addded, or false if the variant was already contained.</returns>
        public bool addVariant(String variant)
        {

            if (!Variants.Contains(variant)) //new variant not contained
            {
                Variants.Add(variant);
                return true;
            }
            return false;
        }

        public int numVariants()
        {
            return Variants.Count;
        }

        public String[] GetVariants()
        {
            return Variants.ToArray();
        }

        /// <summary>
        /// Given a string representing an attribute variant, returns a number corresponding to the variant. It makes storage a little less repetetive
        /// since it lets us store all the variant types as strings in the attributes without repeating them in the individual cases. Loses a little information
        /// when dealing with numerical ranges, though it will still record the relevant range of the number.
        /// </summary>
        /// <param name="variant"></param>
        /// <returns></returns>
        public int GetVarID(String variant)
        {
            if (this.Numeric) //a binary numeric attribute that is either below, or above/equal to the median.
            {
                if (int.Parse(variant) < median)
                { //first option
                    return 0;
                }
                else
                {//greater than or equal
                    return 1;
                }
            }

            for (int i =0; i<Variants.Count; i++) //a categorical attribute
            {
                if (variant.Equals(Variants[i])){ //found a match
                    return i; //return the list index
                }
            }
            //input variant not found
            return -1;
        }
    }

    /// <summary>
    /// A Case representing one event in the data. Each case has an array of values pertaining to its corresponding variant in each attribute.
    /// Cases also have a weight representing their fractional value in the case of a missing attribute.
    /// Beware: There are no systems checking to see if cases are consistent across the board, nor anything to enforce an order in the attributes
    /// or trees. The attributes are assumed to be ordered the same as they are in the training data, minus the final label (can be blank).
    /// </summary>
    public class Case
    {
        public readonly int ID;
        public readonly int[] AttributeVals; //Attribute vals are important to know, and don't need to be changed after creation
        public readonly double Weight;
        //public readonly String[] AttributeOrder;

        /// <summary>
        /// Creates a new Case identified by an input id to contain input attribute values. The data is not self describing, and 
        /// requires a similarly ordered list of attributes (ordered by ID) to make sense of it.
        /// </summary>
        public Case(int id, int[] attributevals/*, String[] order*/)
        {
            ID = id;
            AttributeVals = attributevals;
            Weight = 1;
            //AttributeOrder = order;
        }

        /// <summary>
        /// Creates a new Case identified by an input id to contain input attribute values. Also records a double representing weight
        /// to act as part of the case of an orginal case lacking a value.
        /// </summary>
        public Case(int id, int[] attributevals/*, String[] order*/, double weight)
        {
            ID = id;
            AttributeVals = attributevals;
            Weight = weight;
            //AttributeOrder = order;
        }

    }
}
