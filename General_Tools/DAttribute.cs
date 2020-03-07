using System;
using System.Collections.Generic;
using System.Text;

namespace General_Tools
{

    /// <summary>
    /// An attribute by which to differentiate a case of data. Each attribute has a name, array of differentiable forms it may take (Variants),
    /// and will note if it represents a numerical range or final tag. The range of a numeric attribute must be defined when examining the training data.
    /// </summary>
    public class DAttribute
    {
        public readonly String Name;
        public readonly int ID; //ID refers to the position in the order of the data's representation
        private List<String> Variants;
        public readonly Type AttType; 
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
        public DAttribute(String name, int id, List<String> variants, Type type, bool final)
        {
            Name = name;
            ID = id;
            Variants = variants;
            AttType = type;
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
        /// If true, this attribute is a binary numeric one and represents two ranges of numbers (has two )
        /// </summary>
        /// <returns></returns>
        public bool IsBNumeric()
        {
            return AttType == Type.BinaryNumeric;
        }

        /// <summary>
        /// Use to define values for a binary numeric attribute, since it needs to be set after looking over all the data.
        /// </summary>
        /// <param name="newVars"></param>
        public void setNumericVariants(int median)
        {
            if (AttType == Type.BinaryNumeric)
            {
                this.median = median;
                List<String> newVars = new List<string>(2);
                newVars.Add("<" + median);
                newVars.Add(">=" + median);
                Variants = newVars;
            }
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

        /// <summary>
        /// Given an array of attributes and an ID, returns the attribute with ID or Null if not found.
        /// </summary>
        /// <param name="attributes"></param>
        /// <returns></returns>
        public static DAttribute findByID(DAttribute[] attributes, int ID)
        {
            for (int i = 0; i < attributes.Length; i++)
            {
                if (attributes[i].ID == ID)
                {
                    return attributes[i];
                }
            }

            return null;
        }

        public String[] GetVariants()
        {
            return Variants.ToArray();
        }

        /// <summary>
        /// Given a string representing an attribute variant, returns a number corresponding to the variant. It makes storage a little less repetetive
        /// since it lets us store all the variant types as strings in the attributes without repeating them in the individual cases. Loses a little information
        /// when dealing with numerical ranges, though it will still record the relevant range of the number.
        /// In the final case of a purely numeric attribute, returns the the number input to the function.
        /// </summary>
        /// <param name="variant"></param>
        /// <returns></returns>
        public double GetVarID(String variant)
        {
            switch (AttType)
            {
                case Type.Categorical:
                    for (int i = 0; i < Variants.Count; i++) //a categorical attribute
                    {
                        if (variant.Equals(Variants[i]))
                        { //found a match
                            return i; //return the list index
                        }
                    }
                    break;
                case Type.BinaryNumeric:
                    if (int.Parse(variant) < median)
                    { //first option
                        return 0;
                    }
                    else
                    {//greater than or equal
                        return 1;
                    }
                case Type.Numeric:
                    return double.Parse(variant);
                    //throw new NotImplementedException("Pure numeric attributes do not have variable ID's");
            }

            return -1; //something went horribly wrong
        }

        /// <summary>
        /// An enum referring to the type of attribute, seperating numeric attributes from categorical ones, so one and so forth.
        /// </summary>
        public enum Type
        {
            BinaryNumeric,
            Categorical,
            Numeric
        }
    }

}
