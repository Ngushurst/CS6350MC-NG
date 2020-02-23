using System;
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
        public readonly int[] AttributeVals; //Attribute vals are important to know, and don't need to be changed after creation
        private double Weight;
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

        public void setWeight(double weight)
        {
            Weight = weight;
        }

        public double getWeight()
        {
            return Weight;
        }
    }
}
