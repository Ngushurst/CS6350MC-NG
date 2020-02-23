using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using General_Tools;


namespace ID3_Algorithm
{

    /// <summary>
    /// Primary class defining static methods by which to build and test decision trees.
    /// </summary>
    public class ID3Tools
    {
        /// <summary>
        /// Creates the root of a decision tree and recursively builds the rest.
        /// </summary>
        public static ID3_Node ID3(List<DAttribute> attributes, List<Case> data, int DepthRemaining, EntropyCalucalation calc)
        {
            if (DepthRemaining > -1 && attributes.Count > 1 && data.Count > 0) // Check more layers are allowed and if there are usable attributes remaining, and that there's at least one data point
            {
                //The next line assumes that the last attribute is always the final one, and the only final one.
                //DataAttributes that are final are marked, so you should be able to change that behavior very easily.
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
                    List<DAttribute> UpdatedAttributes = attributes.ToList(); //Copy the list and remove the winning attribute (dividing by it again wouldn't be helpful.)
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
        /// Finds the error of the input tree according to the test labels.
        /// </summary>
        /// <param name="TestFilepath"></param>
        /// <param name=""></param>
        /// <returns></returns>
        public static double FindTestError(List<Case> TestCases, List<DAttribute> attributes, ID3_Node Tree)
        {
            double RightAnswers = 0;

            foreach(Case C in TestCases)
            {
                if(C.AttributeVals.Last() == TestWithTree(C, Tree))
                {
                    RightAnswers += C.getWeight(); //add the case's weight (usually one)
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
        public static double[] GetLabelDistribution(List<Case> Data, DAttribute attribute)
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
                output[AVal]+= c.getWeight(); //increment the corresponding attribute variant by the case's weight (summing number of hits for each)
            }

            for (int i = 0; i<numVars; i++) //divide each by count to get the relative proportion of the label as oppsed to the count.
            {
                output[i] = output[i] / Data.Count;
            }

            return output;
        }

        /// <summary>
        /// Three different methods to calculate the entropy of a set. There is information gain (IG), gini index (GI), and majority error (ME).
        /// Information gain is for each label, sum -= percentage of label * NaturalLog(Percentage of label).
        /// Gini index is, for each attribute, sum = 1, sum -= percentage of label ^ 2. 
        /// Majority error is 1 - percentage of set composed by majority element.
        /// 
        /// Each calculation will give a slightly different answer, though none is considered to be dominant. Try using all three and see which
        /// performs best.
        /// </summary>
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

        /// <summary>
        /// Given an array of the same attributes used to build a tree containing the current node, list the node's information. As a note, the
        /// attributes don't need to be ordered.
        /// </summary>
        /// <param name="attributes"></param>
        /// <returns></returns>
        public String WriteToString(DAttribute[] attributes)
        {
            String output = "";
            int depth = this.depth();

            for(int i = 0; i < depth; i++)
            {
                output += "\t";
            }

            //find the attribute by its ID and print out the relevant information.
            output += DAttribute.findByID(attributes, this.AttributeID).Name + " " + depth + "\t";

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
        public String PrintTree(DAttribute[] attributes)
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

}
