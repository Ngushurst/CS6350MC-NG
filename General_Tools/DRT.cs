using System;
using System.Collections.Generic;

namespace General_Tools
{
    /// <summary>
    /// A group of methods that can read data in CSV files to set up lists of cases for use in other algorithms.
    /// </summary>
    public class DRT
    {
        /// <summary>
        /// Parse CSV with a pregenerated list of attributes, except that this one will define numeric attributes based on the data if true is passed in as the third parameter
        /// </summary>
        /// <param name="Attributes"></param>
        /// <param name="Filepath"></param>
        /// <param name="defineNumericAtt"></param>
        /// <returns></returns>
        public static List<Case> ParseCSV(DAttribute[] Attributes, String Filepath, bool defineNumericAtt)
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
                foreach (DAttribute att in Attributes)
                {
                    if (att.AttType == DAttribute.Type.BinaryNumeric)
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
                double[] caseVars = new double[current.Length]; //array to hold all the relevant values in the case


                if (current.Length == 0)
                { //blank data likely at the start or end
                    continue;
                }

                for (int j = 0; j < current.Length; j++)
                {
                    if (Attributes[j].AttType != DAttribute.Type.Numeric)
                    {
                        double varID = Attributes[j].GetVarID(current[j]);
                        if (varID == -1) //unrecognized attribute value. Assumed to be unknown and will be replaced by a fractional count
                        {
                            unidentifiedID.Add(i); //toss a reference to it onto this stack. Will add multiple copies of the same number for multiple missing values
                        }
                        caseVars[j] = varID; //record the variant number by the attribute

                    }

                    else //pure numeric attribute
                    {
                        caseVars[j] = double.Parse(current[j]);
                    }
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
        private static List<Case> FractionalValueFilling(List<int> unidentifiedID, List<Case> data, DAttribute[] Attributes)
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
                        double[] newVals = new double[Attributes.Length]; //create a replacement array
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
        private static List<Case> MajorityValueFilling(List<int> unidentifiedID, List<Case> data, DAttribute[] Attributes)
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
                    if (highest < Weights[i])
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
                    double[] newVals = new double[Attributes.Length]; //create a replacement array
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
        public static List<Case> ParseCSV(DAttribute[] Attributes, String Filepath)
        {
            //will not define numeric attributes
            return ParseCSV(Attributes, Filepath, false);
        }

        /// <summary>
        /// Parses a CSV and tries to automatically detect and build attributes. Returns a list of cases and populates the given list of attributes.
        /// </summary>
        /// <returns></returns>
        public static List<Case> ParseCSV(String Filepath, out List<DAttribute> attributes)
        {
            //read all lines of the CSV file
            String[] rawCases = System.IO.File.ReadAllLines(Filepath);

            if (rawCases.Length == 0)
            {
                throw new MissingFieldException("File must have at least one data point");
            }

            String[] example = rawCases[0].Split(',');
            attributes = new List<DAttribute>(example.Length); //initialize attributes

            for (int i = 0; i < example.Length; i++) //check the first item to find the number of attributes
            { //create attributes and intialize their lists of variants. Populate attributes
                if (i < example.Length - 1)
                {
                    attributes.Add(new DAttribute("Var" + i, i, new List<String>(), DAttribute.Type.Categorical, false));
                }
                else
                {//last attribute assumed by default to be final
                    attributes.Add(new DAttribute("Var" + i, i, new List<String>(), DAttribute.Type.Categorical, true));
                }

            }
            List<Case> data = new List<Case>(rawCases.Length);
            for (int i = 0; i < rawCases.Length; i++)
            {
                String[] current = rawCases[i].Split(',');
                double[] caseVars = new double[current.Length]; //array to hold all the relevant values in the case


                if (current.Length == 0)
                { //blank data likely at the start or end
                    continue;
                }

                for (int j = 0; j < current.Length; j++)
                {
                    attributes[j].addVariant(current[j]); //record attribute
                    caseVars[j] = attributes[j].GetVarID(current[j]); //record the variant number by the attribute now that it's in
                }

                data.Add(new Case(i, caseVars)); //add the case to the list
            }//repeat until we run out of data.

            return data;
        }


        /// <summary>
        /// Calculates the Final label (output, found in data as #attributeNum) purity for each variant of a dataset and returns it.
        /// Doesn't work on pure numeric attributes.
        /// </summary>
        /// <param name="Data"></param>
        /// <returns></returns>
        private static double[] GetLabelDistribution(List<Case> Data, DAttribute attribute)
        {
            if(attribute.AttType == DAttribute.Type.Numeric)
            {
                throw new Exception("Failure in DRT.GetLabelDistribution() - Cannot get the label distribution of a purely numeric attribute.");
            }

            int numVars = attribute.numVariants();
            double[] output = new double[numVars];

            foreach (Case c in Data)
            {
                int AVal = (int) c.AttributeVals[attribute.ID]; // the varID of the attribute value held by C. Treat it as an integer.
                if (AVal <= -1)
                {
                    continue; //value is undefined. proceed to the next value
                }
                output[AVal] += c.getWeight(); //increment the corresponding attribute variant by the case's weight (summing number of hits for each)
            }

            for (int i = 0; i < numVars; i++) //divide each by count to get the relative proportion of the label as oppsed to the count.
            {
                output[i] = output[i] / Data.Count;
            }

            return output;
        }

    }
}
