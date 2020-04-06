﻿using System;
using System.Collections.Generic;
using System.Text;

namespace General_Tools
{
    /// <summary>
    /// A Case representing one event in the data. Each case has an array of values pertaining to its corresponding variant in each attribute.
    /// Cases also have a weight representing their fractional value in the case of a missing attribute.
    /// Beware: There are no systems checking to see if cases are consistent across the board, nor anything to enforce an order in the attributes
    /// or trees. The attributes are assumed to be ordered the same as they are in the training data, minus the final label (can be blank).
    /// </summary>
    public class Case
    {
        public readonly int ID;
        public readonly double[] AttributeVals; //Attribute vals are important to know, and don't need to be changed after creation
        private double Weight;
        //public readonly String[] AttributeOrder;

        /// <summary>
        /// Creates a new Case identified by an input id to contain input attribute values. The data is not self describing, and 
        /// requires a similarly ordered list of attributes (ordered by ID) to make sense of it.
        /// </summary>
        public Case(int id, double[] attributevals/*, String[] order*/)
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
        public Case(int id, double[] attributevals/*, String[] order*/, double weight)
        {
            ID = id;
            AttributeVals = attributevals;
            Weight = weight;
            //AttributeOrder = order;
        }

        public void setWeight(double weight)
        {
            Weight = weight;
        }

        public double getWeight()
        {
            return Weight;
        }


        /// <summary>
        /// Converts all the weights in the list of cases to be a relative percentage of the whole weight (which will then sum to 1).
        /// </summary>
        public static void NormalizeWeights(List<Case> data)
        {
            double sumWeight = 0;
            foreach (Case c in data)
            {
                sumWeight += c.getWeight();
            }

            for (int i = 0; i < data.Count; i++)
            {
                data[i].setWeight(data[i].getWeight() / sumWeight); //every weight now equal to self/totalWeight (converted to percentage summing to 1)
            }
        }

        /// <summary>
        /// Takes a list of cases and shuffles it randomly, based on the input random seed.
        /// The methods Shuffle and Swap are copied from user "Shital Sha"'s implentation of the Fisher-Yates shuffle on stack overflow.
        /// The direct link is: https://stackoverflow.com/questions/273313/randomize-a-listt
        /// </summary>
        public static void Shuffle(List<Case> data, int RandomSeed)
        {
            Random rand = new Random(RandomSeed);

            for (var i = data.Count; i > 0; i--)
                Swap(data, 0, rand.Next(0, i));
        }

        private static void Swap(List<Case> data, int i, int j)
        {
            var temp = data[i];
            data[i] = data[j];
            data[j] = temp;
        }

        /// <summary>
        /// Converts all x in a collumn to Y in the given list of cases
        /// </summary>
        public static void ColXtoY(List<Case> data, int colNum, double x, double y)
        {
            foreach(Case c in data)
            {
                if(c.AttributeVals[colNum] == x)
                {
                    c.AttributeVals[colNum] = y;
                }
            }
        }
    }
}
