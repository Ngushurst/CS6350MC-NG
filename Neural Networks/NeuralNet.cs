using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using General_Tools;

namespace Neural_Networks
{
    public class NeuralNet
    {
        Neuron Root;
        Neuron[][] Network;
        double BaseLearningRate;
        double CurrentLearningRate;
        int Width;
        int Layers;

        int RSeed;
        
        /// <summary>
        /// Creates a neural net that will predict for cases with width predictors and hiddenlayers hidden layers (root and inputs not counted as layers).
        /// </summary>
        /// <param name="width"></param>
        /// <param name="hiddenLayers"></param>
        public NeuralNet(double LearningRate, int RandomSeed, int numInputs, int width, int hiddenLayers)
        {
            this.BaseLearningRate = LearningRate;
            CurrentLearningRate = LearningRate;
            RSeed = RandomSeed;

            Width = width + 1; //accounting for bias values. The bias values are at Network[X, 0].
            Layers = hiddenLayers + 1; //accounting for input layer

            Network = new Neuron[Layers][]; //one extra layer for inputs and one extra on width for bias.
            Root = new Neuron(0, Width);
            //=======================================================================
            //set up stuff directly beneath the root
            Network[hiddenLayers] = new Neuron[Width];
            for (int j = 1; j < Width; j++)
            {
                Network[hiddenLayers][j] = new Neuron(1, Width); //these neurons only have one output, but a normal number of inputs
            }

            for (int i = 2; i<hiddenLayers; i++) // typical hidden layer items
            {
                Network[i] = new Neuron[Width];
                for (int j = 1; j < Width; j++)
                {
                    Network[i][j] = new Neuron(width,Width); //recieve input from all, including bias. Output to all on next layer, exluding bias. (x-1,x)
                }
            }

            //set up layer above input layer. It might have an abnormal number of inputs, but has the full number of neurons and outputs
            Network[1] = new Neuron[Width];
            for (int j = 1; j < Width; j++)
            {
                Network[1][j] = new Neuron(width, numInputs);
            }

            //Finally, set up all of the bias neurons. These have full outputs and no inputs
            for (int i = 1; i < hiddenLayers; i++)
            {
                Network[i][0] = new Neuron(width, 0);
            }
            Network[hiddenLayers][0] = new Neuron(1, 0); //the last one only has one output (points at the root)

            //set up input layer. It has fewer neurons than the rest of the layers.
            Network[0] = new Neuron[numInputs];
            for (int j = 0; j < numInputs; j++) 
            {
                Network[0][j] = new Neuron(width, 0); //these neurons ARE the inputs, and have none, but a normal number of outputs
            }

            //=======================================================================
            //All neurons built. Now need to set up edges. Building from the top down.

            Edge[] edges = new Edge[Width];
            for (int j = 0; j < Width; j++) //set up the edges array
            {
                edges[j] = new Edge();
                edges[j].updateComingFrom(Network[hiddenLayers][j]); //adjust all edges to point different neurons on the top layer
            }

            Root.addIncomingEdges(edges,0); //the root will be the only output for the top neurons. This also sets the outgoing edges at the other ends.

            //start from the top of the hidden layers, count down from there, but don't try to add inputs to the input layer (stop after "1")
            for (int i = hiddenLayers; i > 1; i--)
            {
                int minus = i - 1;
                for (int j = 0; j < Width; j++) //set up the edges array
                {
                    edges[j].updateComingFrom(Network[minus][j]); //adjust all edges to point different neurons on the next layer down
                }
                for (int j = 1; j < Width; j++) //now that the edges are set, add the incoming edges to each of the neurons on the current layer
                { //of course, the bias has no input, so skip over it in this loop. (we still count in in the other one though)
                    Network[i][j].addIncomingEdges(edges, j - 1); //add the edges to both ends of each
                }
            }

            //since the input layer is weird, account for that.
            edges = new Edge[numInputs];
            for (int j = 0; j < numInputs; j++) // set up the edges
            {
                edges[j] = new Edge();
                edges[j].updateComingFrom(Network[0][j]); //adjust so they point at the inputs
            }
            for (int j = 1; j < Width; j++) //now that the edges are set, add the incoming edges to each of the neurons on the current layer
            { //of course, the bias has no input, so skip over it in this loop. (we still count in in the other one though)
                Network[1][j].addIncomingEdges(edges, j - 1); //add the edges to both ends of each
            }

            //=======================================================================
            //Lastly, we set all the bias terms hold 1 as their value

            for (int i = 1; i<Layers; i++) //there is no bias term on the input layer
            {
                Network[i][0].setValue(1); //set all bias terms to be 1. We will never change these values (but we will change the outgoing weights)
            }

            //And that's that
        }

        /// <summary>
        /// Returns the prediction of a Case C. Returns -1 or 1 when binary is true.
        /// </summary>
        /// <param name="C"></param>
        /// <returns></returns>
        public double Predict(Case C, bool Binary)
        {
            for (int j = 0; j<Network[0].Length; j++) //set up inputs layer to hold values in the case
            {
                Network[0][j].setValue(C.AttributeVals[j]);
            }
            //now that that's done, we can calculate the values of all spots in the hidden layers (skips input layer)

            for(int i = 1; i<Layers; i++)
                for(int j = 1; j<Width; j++) //skip bias on all layers (always equals 1)
                {
                    Network[i][j].SumIncomingValues(); //calculate value for all neurons on current layer
                    Network[i][j].ActionFunction(); //Activate all neurons (as you do for all hidden layer neurons)
                }

            //All hidden layer items are calculated.
            Root.SumIncomingValues(); //do the root. Do not activate the root (want an actual value out of it)
            if(Binary)
            {
                if (Math.Sign(Root.value) <= 0)
                {
                    return -1;
                }
                else
                    return 1;
            }
            return Root.value;
        }

        /// <summary>
        /// Updates edges assuming a boolean classifier using the input Case C
        /// </summary>
        public void updateEdges(Case c, int epochNum, int numCases)
        {
            double y = Predict(c,true);//Will either be -1, 0, or 1 depending on what the value actually is. Ensures network is updated
            //We'll use square loss here. This is where you'd put other loss functions by the way.
            //Square loss derivative is Y - Y' (actual - expected). Function is 1/2 * (y-y')^2
            double LDerivative = y - c.AttributeVals.Last();

            if (LDerivative != 0) //if there was a miss of any degree
            {
                foreach (Edge e in Root.EdgesIn) //the top layer edges are slighly different
                {
                    e.CalculateDerivative(LDerivative);
                }

                //do the rest of the network
                for (int i = Layers - 1; i > 0; i--) //start at the top and go down from there. Derivatives at lower levels based on higher ones
                {
                    for (int j = 1; j < Width; j++) //do for all neurons on each layer
                    {
                        //and for every incoming edge in each neuron
                        foreach (Edge e in Network[i][j].EdgesIn)
                        {
                            e.CalculateDerivative();
                        }
                    }
                }

                //Now that all the derivatives are done, recalculate edge weights.

                foreach (Edge e in Root.EdgesIn) //the top layer edges are slighly different
                {
                    e.weight -= CurrentLearningRate * e.derivative; //subtract the learning rate * the derivative.
                }

                for (int i = Layers - 1; i > 0; i--) //start at the top and go down from there. Derivatives at lower levels based on higher ones
                {
                    for (int j = 1; j < Width; j++) //do for all neurons on each layer
                    {
                        //and for every incoming edge in each neuron
                        foreach (Edge e in Network[i][j].EdgesIn)
                        {
                            e.weight -= CurrentLearningRate * e.derivative; //subtract the learning rate * the derivative.
                        }
                    }
                }


                CurrentLearningRate = BaseLearningRate/(1+ (BaseLearningRate/(double)numCases)*epochNum); //Readjust learning rate. (complex. \_:)_/)
            }
        }


        /// <summary>
        /// Runs the input number of epochs using the input training set.
        /// </summary>
        /// <param name="numEpochs"></param>
        public void runEpochs(int numEpochs, List<Case> data)
        {
            for(int i = 1; i < numEpochs+1; i++) //starting at zero would screw with the learning rate.
            {
                Case.Shuffle(data, RSeed); //always shuffle it the same way. I'll consider adding an input random seed later
                foreach (Case c in data)
                {
                    updateEdges(c, i, data.Count);
                }
            }
        }

        /// <summary>
        /// Given a list of cases, returns the percentage of cases that the Neural Net classifies incorrectly
        /// </summary>
        /// <param name="cases"></param>
        /// <returns></returns>
        public double getError(List<Case> cases)
        {
            int errors = 0;

            foreach(Case c in cases)
            {
                if (Predict(c, true) != c.AttributeVals.Last())
                    errors++;
            }

            return ((double)errors)/cases.Count;
        }
    }


    /// <summary>
    /// A neuron containing a value and edges connected to it, split into incoming and outgoing edges. These groups will be Null if they are empty.
    /// </summary>
    public class Neuron
    {
        public double value;
        public Edge[] EdgesOut;
        public Edge[] EdgesIn;


        /// <summary>
        /// Sets up a neuron with empty/Null arrays and a zero value. Null arrays are for the instances when their size would be zero.
        /// </summary>
        /// <param name="Outgoing"></param>
        /// <param name="Incoming"></param>
        public Neuron(int Outgoing, int Incoming)
        {
            if (Outgoing != 0) // if non zero, set up an array
            {
                EdgesOut = new Edge[Outgoing];
            }
            else  //if zero, set to null
                EdgesOut = null;

            if (Incoming != 0) // if non zero, set up an array
            {
                EdgesIn = new Edge[Incoming];
            }
            else  //if zero, set to null
                EdgesIn = null;


        }

        /// <summary>
        /// Takes a group of edges and assumes that they are incoming. Copies the entire list and populates the Neuron's Array.
        /// Afterwards, adds the same edges to their outgoing Neurons. Don't do this until you've calculated the layer before it.
        /// </summary>
        /// <param name="incoming">An array of incoming edges</param>
        /// <param name="index">An integer pertaining to the index of a neuron in a layer. Skip the bias and use zero to represent the next neuron
        /// and count up from there.</param>
        public void addIncomingEdges(Edge[] incoming, int index)
        {
            for(int i = 0; i < incoming.Length; i++)
            {
                EdgesIn[i] = new Edge(incoming[i]);
                EdgesIn[i].updateGoingTo(this);// set edge to point to current neuron
                EdgesIn[i].ComingFrom.addOutgoingEdge(EdgesIn[i], index); //add the updated edge to the neuron
            }
        }

        /// <summary>
        ///  adds the input edge to the current neuron at index i (used in the edges out array). Helper method to addIncomingEdges
        /// </summary>
        private void addOutgoingEdge(Edge e, int index)
        {
            EdgesOut[index] = e; //don't copy the edge. It serves as a bridge, though this would still probably work even if you did.
        }

        /// <summary>
        /// Activation function's too dry. Action sounds cooler. Anyhow, this method performs the sigmoid activation by default.
        /// Sigmoid activation function A(X) = 1/ (1+e^(-x)). As a note the function num does nothing, but this is where you'd add more
        /// activation functions if you wanted them.
        /// </summary>
        public void ActionFunction(/*int functionNum*/)
        {
            double val = 1 + Math.Exp(-value);
            value = 1 / val; //Update value to = 1/(1+e^oldValue)
        }

        /// <summary>
        /// Calculates the sigmoid activation function of the input value. As a reminder the function is A(X) = 1/ (1+e^(-x)). Once again, this is where
        /// you'd add the function number and have a switch statement for the various activation functions.
        /// </summary>
        public static double ActionFunction(double value/*, int functionNum*/)
        {
            
            double val = 1 + Math.Exp(-value);
            return (1 / val); //Update value to = 1/(1+e^oldValue)
        }
        /// <summary>
        /// A nodes value is the sum of all of the incoming weights times their starting neuron's values.
        /// </summary>
        public void SumIncomingValues()
        {
            //mutiply all incoming edges by their starting neuron's value (the other end of the edge)
            double sum = 0;
            foreach(Edge e in EdgesIn)
            {
                sum += e.ComingFrom.value * e.weight;
            }
            value = sum;
        }

        public void setValue(double newValue)
        {
            value = newValue;
        }
    }

    /// <summary>
    /// An object representing the edge between two neurons. Each edge has a weight, and later a derivative for calculating loss.
    /// </summary>
    public class Edge
    {
        public Neuron GoingTo;
        public Neuron ComingFrom;
        public double weight;
        public double derivative;

        /// <summary>
        /// Creates a new edge will a null start and destination, and zero weight and derivative.
        /// </summary>
        public Edge()
        {
            weight = 0;
            derivative = 0;
            GoingTo = null;
            ComingFrom = null;
        }

        /// <summary>
        /// Copies the input edge (not including the derivative, which is set to zero)
        /// </summary>
        /// <param name="copy"></param>
        public Edge(Edge copy)
        {
            weight = copy.weight;
            GoingTo = copy.GoingTo;
            ComingFrom = copy.ComingFrom;
            derivative = 0;
        }

        /// <summary>
        /// Calculates the derivative of the current edge Going to the Root, hence there are no other edges to base the derivative on.
        /// Furthermore, these derivatives should be done in layers, starting from the root, and proceeding down to the inputs.
        /// </summary>
        public void CalculateDerivative(double LossDerivative)
        {
            derivative = LossDerivative * ComingFrom.value; //the derivative of these is the loss derivative time the previous Neruon's value
        }

        /// <summary>
        /// Calculates the derivative based off of the outgoing edges from the neuron this edge points to. That neuron is cannot be the Root.
        /// Furthermore, these derivatives should be done in layers, starting from the root, and proceeding down to the inputs.
        /// </summary>
        public void CalculateDerivative()
        {
            double sum = 0;
            foreach(Edge e in GoingTo.EdgesOut)
            {
                sum += e.derivative * e.weight; //e.derivative * e.weight
            }
            //finally multiply by the partial derivative of the next neuron is respect to this edge.

            derivative = sum * (1 - GoingTo.value) * ComingFrom.value; //multiply by previous neuron,  GoingTo.value / GoingTo.value, and 1 - GoingTo.value
            //and that's that
        }

        public void updateWeight(double newWeight)
        {
            weight = newWeight;
        }

        public void updateDerivative(double newDerivative)
        {
            derivative = newDerivative;
        }

        public void updateGoingTo(Neuron n)
        {
            GoingTo = n;
        }

        public void updateComingFrom(Neuron n)
        {
            ComingFrom = n;
        }
    }

    
}
