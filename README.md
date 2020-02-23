# CS6350MC-NG
This is a machine learning library developed in C# by Noah Kane Gushurst for CS6350 at the University of Utah

## Decision Tree

------------


Contained within the C# namespace "ID3_Algorithm_NG" are four major classes: ID3_Node, Attribute, Case, and ID3_Tools.

### ID3_Node

------------


A class representing a single node within a decision tree.

```csharp
ID3_Node Parent
ID3_Node[] Children
int attributeID
int Value
```

All nodes will have an int referring to an attribute on which to split the data.
Additionally:
- The root node will have a null parent field
- Main body nodes will have a parent and an array of nodes representing children.
- Leaf nodes will have a parent null children field, and a relevant integer pertaining to the final value of their attribute.

ID3_Nodes have two main methods that they can use to give a user more information:
##### String WriteToString(Attribute[] attributes)
Given a complete array of the same attributes used to build the decision tree (and hence nodes), print the name of the attribute the node splits the data on and its value should the node be a leaf node.


##### String PrintTree(Attribute[] attributes)
Prints information about the current node and all nodes below it by recursively calling   WriteToString(attributes) .

### Attribute

------------


A class used to represent a single attribute that helps to define cases in the data.

```csharp
String Name;
int ID;
List<String> Variants;
bool Numeric;
int median;
bool Final;

```
##### bool isFinal()
Returns true if the current attribute is a final attribute, meaning that it is meant to be predicted by the other attributes, and will not be used in predictions in the learning algorithm.

##### bool isNumeric()
Returns true if the current attribute is a numeric attribute, meaning that its value will be determined by a range as opposed to by matching. As of now, numeric attributes are determined to either be above or below the median value in the training data.

##### void setNumericVariants(int median)
A method used to set the median value for a numeric attribute. Does nothing if the attribute is not numeric.

##### bool addVariant(String variant)
Tries to add in the new variant input into the attribute. If the variant is not contained, add it to the attribute and return true. Otherwise, return false.

##### static Attribute findByID(Attribute[] attributes, int ID)
Given an array of attributes, returns the attribute with the input ID. An attribute's ID often refers to the way the data is ordered. Hence, an attribute with an ID of 0 generally points at the first item used to describe the data.

##### String[] getVariants()
Returns a list of strings representing the different variations of the current attribute.  Data using this attribute will either match if represented by strings, or correspond to an index in the array if represented by an integer.

##### int numVariants()
Returns the number of variants that exist for the current attribute.

### Case

------------


An individual case in the data used to contain differing values for the attributes. The weight may or may not be used depending on the exact algorithm, and is one by default.
```csharp
        readonly int ID;
        readonly int[] AttributeVals;
        double Weight;
```

##### void setWeight(double weight)
Sets the weight to the input double.

##### double getWeight()
Returns the current weight of the attribute.

### ID3_Tools

------------


The ID3 algorithm and methods to enable and support its use.

##### static ID3_Node ID3(List< Attribute> attributes, List< Case> data, int DepthRemaining, EntropyCalucalation calc)
Utilizing the given lists of attributes and cases, a depth limit, and desired method of entropy calculation, recurively creates a decision tree and returns its root node.

##### static List< Case> parseCSV(Attribute[] Attributes, String Filepath, bool defineNumericAtt)

Given a file path to the csv file, reads and encodes the data as cases according to the input attributes. If defineNumericAtt is input as true, the method will also find the median of the numeric attributes and define them using the data in the file at Filepath. Missing and invalid attribute values will be filled in by the most common values for that given attribute across the data set.

##### static List< Case> ParseCSV(String Filepath, out List< Attribute> attributes)
Given a file path and a place to put a list of attributes, reads the data and autodetects attributes. The attributes will not be named (so use the other variation of this method if you want descriptive attributes), will be assumed to be categorical (all disinctly different by case), and the last item in each row of data will be assumed to be the target label.

##### static double FindTestError(List< Case> TestCases, List< Attribute> attributes, ID3_Node Tree)
Given a list of cases to test, the attributes used to define the set, and a tree to test the cases by, return a double between one and zero representing the percentage of cases that the tree predicts correctly.

##### static int TestWithTree(Case test, ID3_Node Tree)
Given a case to test and a tree to test it against, returns the predicted attribute variant number of the target label. (If you have the attribute handy, you can get its variants as an array and use this integer as an index to get the corresponding string)


## Ensemble learners come next
and likely a reorganizing of all the classes here.
