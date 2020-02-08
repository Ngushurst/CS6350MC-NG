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
            if (DepthRemaining > -1) // Check more layers are allowed.
            {
                //The next line assumes that the last attribute is always the final one, and the only final one.
                //Attributes that are final are marked, so you should be able to change that behavior very easily.
                double[] proportions = GetLabelDistribution(data, attributes.Last(), attributes.Count - 1);
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
                    return ID3_Node.Leaf(attributes.Count - 1, result); //Entire data set is of the same label. Tree is a single leaf node. Return a leaf node.
                }
                else
                { //find the attribute that results in the lowest entropy and divide on that.
                    List<Case>[] LowestEntropySets = null; //default
                    double LowestEntropy = double.PositiveInfinity; //max value by default.
                    int BestAttNum = 0; //default
                    int BestAttID = -1;

                    for (int a = 0; a < attributes.Count; a++)
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
                            double weight = set.Count / data.Count; //percentage of data represented by 'set'
                            double[] distribution = GetLabelDistribution(set, attributes.Last(), attributes.Count - 1);
                            sumEntropy += weight * CalculateEntropy(proportions, calc); //weighted entropy value
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
                    attributes.RemoveAt(BestAttNum);
                    //create children recursively by use of the ID3 algorithm
                    for (int i = 0; i < LowestEntropySets.Length; i++)
                    {
                        ID3_Node child = ID3(attributes.ToList(), LowestEntropySets[i], DepthRemaining - 1, calc);
                        if (!ReferenceEquals(child, null)) //if the child is not null, add it to the list
                        {
                            Children.Add(child);
                        }
                    }


                    ID3_Node output = new ID3_Node(BestAttNum);

                    if (Children.Count == 0) //this node is required to be a leaf by the depth limit
                    {
                        double[] proportion = GetLabelDistribution(data, attributes.Last(), data[0].AttributeVals.Length);
                        double max = 0;
                        int mode = -1; //number representing most common Final label in the current dataset
                        for (int i = 0; i<proportion.Length; i++)
                        {
                            if (proportion[i] > max) {
                                max = proportion[i];
                                mode = i;
                            }
                        }
                        return ID3_Node.Leaf(attributes.Count-1, mode);
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

        public static List<Case> ParseCSV(Attribute[] Attributes, String filename)
        {

            return null;
        }

        /// <summary>
        /// Parses a CSV and tries to automatically detect and build attributes
        /// </summary>
        /// <returns></returns>
        public static List<Case> ParseCSV(String Filename)
        {
            return null;
        }


        /// <summary>
        /// Given a Case by which to test with a ID3_Node representing a decision tree, find the tree's label variant number for the Case and return it.
        /// </summary>
        /// <returns></returns>
        public static int TestWithTree(Case test, ID3_Node Tree)
        {
            if (ReferenceEquals(Tree.getChildren(), null))
                return Tree.Value;
            return TestWithTree(test, Tree.getChildren()[test.AttributeVals[Tree.AttributeID]]);
        }

        //Here are the three variations on the entropy calculations
            private static double InformationGain(double[] numLabelType)
            {
                double sum = 0.0;
                for(int i = 0; i< numLabelType.Length; i++)
                { //Entropy is the sum of all i*ln(i), where i is the proportion of a given label.
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
                { //Entropy is the sum of all i*ln(i), where i is the proportion of a given label.
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
        {
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
        public static double[] GetLabelDistribution(List<Case> Data, Attribute Final, int attributeNum)
        {
            int numVars = Final.numVariants();
            double[] output = new double[numVars];

            foreach (Case c in Data)
            {
                output[c.AttributeVals[attributeNum]]++; //increment the corresponding attribute variant (summing number of hits for each)
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
    }

    /// <summary>
    /// An attribute by which to differentiate a case of data. Each attribute has a name, array of differentiable forms it may take (Variants),
    /// and will note if it represents a numerical range or final tag.
    /// </summary>
    public class Attribute
    {
        public readonly String Name;
        public readonly int ID; //ID refers to the position in the order of the data's representation
        private List<String> Variants;
        private bool Numerical_Range;
        private bool Final; // If true, this attribute represents a result as opposed to a distinguishing feature.

        /// <summary>
        /// Build a new attribute with all input fields.
        /// </summary>
        Attribute(String name, int id, List<String> variants, bool numericalrange, bool final)
        {
            Name = name;
            ID = id;
            Variants = variants;
            Numerical_Range = numericalrange;
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
        /// If true, this attribute represents different ranges of numbers. Ex: 0-10, I>10 
        /// </summary>
        /// <returns></returns>
        public bool IsRange()
        {
            return Numerical_Range;
        }

        /// <summary>
        /// Takes in a string representing a variant of the current attribute and tries to add it to the current list.
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


        /// <summary>
        /// Given a string representing an attribute variant, returns a number corresponding to the variant. It makes storage a little less repetetive
        /// since it lets us store all the variant types as strings in the attributes without repeating them in the individual cases. Loses a little information
        /// when dealing with numerical ranges, though it will still record the relevant range of the number.
        /// </summary>
        /// <param name="variant"></param>
        /// <returns></returns>
        public int GetVarID(String variant)
        {
            for(int i =0; i<Variants.Count; i++)
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
        /// Creates a new Case identified by an input id to contain input attribute values 
        /// </summary>
        Case(int id, int[] attributevals, String[] order)
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
        Case(int id, int[] attributevals, String[] order, double weight)
        {
            ID = id;
            AttributeVals = attributevals;
            Weight = weight;
            //AttributeOrder = order;
        }

    }
}
