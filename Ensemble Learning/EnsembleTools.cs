using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using General_Tools;
using ID3_Algorithm;

namespace EnsembleLearning
{
    public class EnsembleTools
    {

        public static EnsembleLearner AdaBoost(int numTrees, List<Case> data, List<DAttribute> attributes)
        {
            List<Case> dataCopy = data.ToList(); //copy the data so that we're not editing the source data weights.
            ID3_Node[] FullLearner = new ID3_Node[numTrees];
            double[] votes = new double[numTrees];
            //each index gets a decision stump and a corresponding wieght based on its accuracy

            Case.NormalizeWeights(dataCopy);

            for (int i = 0; i < numTrees; i++)
            { //generate that many trees
                ID3_Node current = ID3Tools.ID3(attributes, dataCopy, 1, ID3Tools.EntropyCalucalation.IG);
                double error = ID3Tools.FindTestError(dataCopy, attributes, current);

                if (error < .5) //learner is better than random chance (ID3 should ensure that)
                {
                    double vote = .5 * Math.Log((1 - error) / error);
                    //adjust weight of all labels. This does work done when calculating error. Can possibly code to avoid repeats, but that's effort.
                    foreach(Case c in dataCopy)
                    {
                        int treeResult = ID3Tools.TestWithTree(c, current);
                        if (treeResult == c.AttributeVals.Last())
                        {
                            double newWeight = c.getWeight() * Math.Pow(Math.E, -vote);
                            c.setWeight(newWeight); //on correct, set weight to weight^-vote
                        }
                        else
                        {
                            double newWeight = c.getWeight() * Math.Pow(Math.E, vote);
                            c.setWeight(newWeight); //on incorrect, set weight to weight^vote
                        }

                    }

                    Case.NormalizeWeights(dataCopy);

                    FullLearner[i] = current;
                    votes[i] = vote;
                }
                else
                {
                    throw new Exception("Something went terribly wrong and the weak learner is worse than guessing when considering the current weights.");
                }

            }
            return new EnsembleLearner(FullLearner, votes);
        }

        /// <summary>
        /// A basic Bootstrap AGGregation algorithm. Creates a specified number of full decision trees based on subsets a specified size.
        /// </summary>
        public static EnsembleLearner Bagging(int numTrees, int subSize, bool AllowDuplicates, int RNGseed, List<Case> data, List<DAttribute> attributes)
        {
            Random Gen = new Random(RNGseed);
            ID3_Node[] Trees = new ID3_Node[numTrees];
            List<Case> subset;
            for (int i = 0; i < numTrees; i++)
            {
                subset = GetRandomSubset(!AllowDuplicates, subSize, Gen, data); //pick some random items to use for this current tree
                Trees[i] = ID3Tools.ID3(attributes, subset, int.MaxValue, ID3Tools.EntropyCalucalation.IG); //specified to use information gain
            }// make another tree.

            return new EnsembleLearner(Trees); //output
        }

        /// <summary>
        /// Like Bagging, but also imposes limits on which attributes the decision tree is allowed to use at each level.
        /// </summary>
        public static EnsembleLearner RandomForest(int numTrees, int subSize, bool AllowDuplicates, int RNGseed, int subAttSize, List<Case> data, List<DAttribute> attributes)
        {
            Random Gen = new Random(RNGseed);
            ID3_Node[] Trees = new ID3_Node[numTrees];
            List<Case> subset;
            for (int i = 0; i < numTrees; i++)
            {
                subset = GetRandomSubset(!AllowDuplicates, subSize, Gen, data); //pick some random items to use for this current tree
                Trees[i] = ID3Tools.ID3(attributes, subset, Gen, subAttSize, int.MaxValue, ID3Tools.EntropyCalucalation.IG); //specified to use information gain
            }// make another tree.

            return new EnsembleLearner(Trees); //output
        }


        /// <summary>
        /// Generates a random subset of the input data that can have duplicate cases (based on input) of an input size. The randomness is
        /// controlled by the input random number generator, and allows this function to operate off of the same generator even in different calls.
        /// </summary>
        public static List<Case> GetRandomSubset(bool withoutReplacement, int numSamples, Random RNG, List<Case> data)
        {
            List<Case> dataCopy = data;
            List<Case> output = new List<Case>(numSamples);
            if (withoutReplacement)
            {
                dataCopy = data.ToList(); //will be editing if this statement is hit, so we'd better copy to avoid changing data itself.
            }

            for(int i = 0; i<numSamples; i++)
            {
                int index = RNG.Next(dataCopy.Count);
                output.Add(dataCopy[index]);

                if (withoutReplacement) //no duplicates allowed if true. Remove item to prevent duplicates.
                {
                    dataCopy.RemoveAt(index);
                }
            }
            return output;
        }
    }


    /// <summary>
    /// A class containing an array of ID3 decision trees of some variety and, in the case of boosting, weighted votes for the trees.
    /// </summary>
    public class EnsembleLearner
    {
        public readonly ID3_Node[] Trees;
        public readonly bool WeightedVotes;
        private double[] VoteWeights;

        /// <summary>
        /// An ensemble learner with trees of uniform weight.
        /// </summary>
        public EnsembleLearner(ID3_Node[] trees)
        {
            Trees = trees;
            VoteWeights = new double[trees.Length];
            double normalWeight = 1.0 / (double)Trees.Length;
            for(int i = 0; i < trees.Length; i++)
            {
                VoteWeights[i] = normalWeight; //set all weights to one over the number of items
            }
            WeightedVotes = false;
        }

        /// <summary>
        /// An ensemble learner with trees of non-uniform weights.
        /// </summary>
        public EnsembleLearner(ID3_Node[] trees, double[] weights)
        {
            Trees = trees;
            VoteWeights = weights;
            WeightedVotes = true;
        }

        /// <summary>
        /// Tests the case c using classification (majority vote) against the ensemble learner and assumes
        /// that its last attribute is the target label. Also requires the target attribute so then it knows
        /// how many variants there are. Unlike the ID3 node, this is a full
        /// learner or something, so it actually contains its own testing functions.
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public int TestEnsembleClassificaiton(Case c, DAttribute target)
        {
            double[] voting = new double[target.numVariants()];

            for(int i = 0; i < VoteWeights.Length; i++)
            {
                int currentResult = ID3Tools.TestWithTree(c, Trees[i]);
                voting[currentResult] += VoteWeights[i]; //add the tree's voting power to the bucket for its answer
            }

            //find the majority vote in the voting pool

            int max = -1;
            double highest = -1;
            for(int i = 0; i<target.numVariants(); i++)
            {
                if (voting[i] > highest)
                {
                    max = i;
                    highest = voting[i];
                }
            }

            //max should contain the winning variant number for the attribute.

            return max;
        }
        
        /// <summary>
        /// Finds the error of the current ensemble learner when using the given data set.
        /// </summary>
        public double TestEnsembleClassMass(List<Case> TestCases, List<DAttribute> attributes)
        {
            double error = 0;
            foreach(Case c in TestCases)
            {
                if(c.AttributeVals.Last() != TestEnsembleClassificaiton(c, attributes.Last()))
                {//prediction does not match reality. add one to error
                    error++;
                }
            }

            error = error / (double)TestCases.Count;
            return error;
        }

        public void PrintLearner(DAttribute[] attributes)
        {
            StringBuilder output = new StringBuilder();

            output.Append("Here is a list of all of the contained trees and their corresponding weights.\n\n");
            double sumWeight = 0;

            foreach(double weight in VoteWeights)
            {
                sumWeight += weight;
            }

            output.Append("The sum of all weight values is: " + sumWeight + ". \n \n");


            for(int i = 0; i<Trees.Length; i++)
            {
                output.Append("---------------------------------------------------------------------------------------------------------\n\n");
                output.Append("Weight = " + VoteWeights[i] + "\n");
                output.Append(Trees[i].PrintTree(attributes));
            }
        }

        /// <summary>
        /// Converts all the weights in the learner to be a relative percentage of the whole weight (which will then sum to 1).
        /// </summary>
        private void NormalizeWeights()
        {
            double sumWeight = 0;
            foreach (double weight in VoteWeights)
            {
                sumWeight += weight;
            }
            
            for(int i = 0; i < VoteWeights.Length; i++)
            {
                VoteWeights[i] = VoteWeights[i] / sumWeight; //every weight now equal to self/totalWeight (converted to percentage summing to 1)
            }
        }

    }
}
