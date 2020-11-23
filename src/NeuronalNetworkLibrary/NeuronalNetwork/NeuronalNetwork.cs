// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NeuronalNetwork.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   The neuronal network class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NeuronalNetworkLibrary.NeuronalNetwork
{
    using System.Collections.Generic;
    using System.Linq;

    using NeuronalNetworkLibrary.ArchiveSerialization;
    using NeuronalNetworkLibrary.NeuronalNetworkLayers;
    using NeuronalNetworkLibrary.NeuronalNetworkNeurons;

    /// <inheritdoc cref="IArchiveSerialization"/>
    /// <summary>
    /// The neuronal network class.
    /// </summary>
    /// <seealso cref="IArchiveSerialization"/>
    public sealed class NeuronalNetwork : IArchiveSerialization
    {
        /// <summary>
        /// The number of back propagation steps (Counter used in connection with weight sanity check).
        /// </summary>
        private uint numberOfBackPropagationSteps;

        /// <summary>
        /// Initializes a new instance of the <see cref="NeuronalNetwork"/> class.
        /// </summary>
        public NeuronalNetwork()
        {
            // Arbitrary, so that brand-new neuronal networks can be serialized with a non-ridiculous number
            this.EtaLearningRate = .001;
            this.numberOfBackPropagationSteps = 0;
            this.LayersList = new NeuronalNetworkLayerList();
        }

        /// <summary>
        /// Gets or sets the ETA learning rate.
        /// </summary>
        public double EtaLearningRate { get; set; }

        /// <summary>
        /// Gets or sets the previous ETA learning rate.
        /// </summary>
        public double PreviousEtaLearningRate { get; set; }

        /// <summary>
        /// Gets or sets the layers list.
        /// </summary>
        public NeuronalNetworkLayerList LayersList { get; set; }

        /// <inheritdoc cref="IArchiveSerialization"/>
        /// <summary>
        /// Serializes the archive.
        /// </summary>
        /// <param name="archive">The archive.</param>
        /// <seealso cref="IArchiveSerialization"/>
        public void Serialize(Archive archive)
        {
            if (archive.IsStoring())
            {
                archive.Write(this.EtaLearningRate);
                archive.Write(this.LayersList.Count);

                foreach (var layer in this.LayersList)
                {
                    layer.Serialize(archive);
                }
            }
            else
            {
                archive.Read(out double eta);

                // Two step storage is needed since EtaLearningRate is "volatile"
                this.EtaLearningRate = eta;
                var layer = (NeuronalNetworkLayer)null;
                archive.Read(out int numberOfLayers);
                this.LayersList.Clear();
                this.LayersList = new NeuronalNetworkLayerList(numberOfLayers);

                for (var ii = 0; ii < numberOfLayers; ii++)
                {
                    layer = new NeuronalNetworkLayer(string.Empty, layer);
                    this.LayersList.Add(layer);
                    layer.Serialize(archive);
                }
            }
        }

        /// <summary>
        /// Calculates the neuronal network.
        /// </summary>
        /// <param name="inputVector">The input vector.</param>
        /// <param name="inputCount">The input count.</param>
        /// <param name="outputVector">The output vector.</param>
        /// <param name="outputCount">The output count.</param>
        /// <param name="outputList">The output list.</param>
        public void Calculate(double[] inputVector, int inputCount, double[] outputVector, int outputCount, NeuronalNetworkNeuronOutputsList outputList)
        {
            var list = this.LayersList.First();

            // First layer is the input layer: Directly set outputs of all of its neurons
            // to the input vector
            if (this.LayersList.Count > 1)
            {
                var count = 0;

                if (inputCount != list.Neurons.Count)
                {
                    return;
                }

                // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
                foreach (var neurons in list.Neurons)
                {
                    if (count >= inputCount)
                    {
                        continue;
                    }

                    neurons.Output = inputVector[count];
                    count++;
                }
            }

            // Calculate the output of the next layers
            for (var i = 1; i < this.LayersList.Count; i++)
            {
                this.LayersList[i].Calculate();
            }

            // Loads up output vector with results
            if (outputVector != null)
            {
                list = this.LayersList[^1];

                for (var ii = 0; ii < outputCount; ii++)
                {
                    outputVector[ii] = list.Neurons[ii].Output;
                }
            }

            // Loads up neuron output values with results
            if (outputList == null) return;
            {
                // Check for first time use (re-use is expected)
                outputList.Clear();

                // It's empty, so allocate memory for its use
                outputList.Capacity = this.LayersList.Count;
                foreach (var layer in this.LayersList)
                {
                    var layerOut = new NeuronalNetworkNeuronOutputs(layer.Neurons.Count);

                    for (var ii = 0; ii < layer.Neurons.Count; ++ii)
                    {
                        layerOut.Add(ii);
                    }

                    outputList.Add(layerOut);
                }
            }
        }

        /// <summary>
        /// Back propagates the neuronal network.
        /// </summary>
        /// <param name="actualOutput">The actual output.</param>
        /// <param name="desiredOutput">The desired output.</param>
        /// <param name="count">The count.</param>
        /// <param name="memorizedNeuronOutputs">The memorized neuron outputs.</param>
        public void BackPropagate(double[] actualOutput, double[] desiredOutput, int count, NeuronalNetworkNeuronOutputsList memorizedNeuronOutputs)
        {
            // Back propagates through the neuronal network

            // There must be at least two layers in the network
            if (this.LayersList.Count >= 2 == false)
            {
                return;
            }

            if (actualOutput == null || desiredOutput == null || count >= 256)
            {
                return;
            }

            // Check if it's time for a weight sanity check
            this.numberOfBackPropagationSteps++;

            if (this.numberOfBackPropagationSteps % 10000 == 0)
            {
                this.PeriodicWeightSanityCheck();
            }

            // Proceed from the last layer to the first, iteratively
            // We calculate the last layer separately, and first, since it provides the needed derviative
            // (i.e., dErr_wrt_dXnm1) for the previous layers

            // Nomenclature:
            //
            // Err is output error of the entire neural net
            // Xn is the output vector on the n-th layer
            // Xnm1 is the output vector of the previous layer
            // Wn is the vector of weights of the n-th layer
            // Yn is the activation value of the n-th layer, i.e., the weighted sum of inputs BEFORE the squashing function is applied
            // F is the squashing function: Xn = F(Yn)
            // F' is the derivative of the squashing function
            //
            // Conveniently, for F = tanh, then F'(Yn) = 1 - Xn^2, i.e., the derivative can be calculated from the output, without knowledge of the input
            var layersCount = this.LayersList.Count;
            var errorsList = new ErrorsList(this.LayersList[^1].Neurons.Count);
            var differentials = new List<ErrorsList>(layersCount);

            int ii;

            // Start the process by calculating dErr_wrt_dXn for the last layer.
            // for the standard MSE Err function (i.e., 0.5*sumof( (actual-target)^2 ), this differential is simply
            // the difference between the target and the actual
            for (ii = 0; ii < this.LayersList[^1].Neurons.Count; ++ii)
            {
                errorsList.Add(actualOutput[ii] - desiredOutput[ii]);
            }

            // store Xlast and reserve memory for the remaining vectors stored in differentials
            for (ii = 0; ii < layersCount - 1; ii++)
            {
                var differential = new ErrorsList(this.LayersList[ii].Neurons.Count);

                for (var kk = 0; kk < this.LayersList[ii].Neurons.Count; kk++)
                {
                    differential.Add(0.0);
                }

                differentials.Add(differential);
            }

            // Last one
            differentials.Add(errorsList);

            // Now iterate through all layers including the last but excluding the first, and ask each of
            // them to back propagate error and adjust their weights, and to return the differential
            // dErr_wrt_dXnm1 for use as the input value of dErr_wrt_dXn for the next iterated layer
            var memorized = memorizedNeuronOutputs != null;

            for (var jj = layersCount - 1; jj > 0; jj--)
            {
                if (memorized)
                {
                    this.LayersList[jj].BackPropagate(
                        differentials[jj],
                        differentials[jj - 1],
                        memorizedNeuronOutputs[jj],
                        memorizedNeuronOutputs[jj - 1],
                        this.EtaLearningRate);
                }
                else
                {
                    this.LayersList[jj].BackPropagate(
                        differentials[jj],
                        differentials[jj - 1],
                        null,
                        null,
                        this.EtaLearningRate);
                }
            }

            differentials.Clear();
        }

        /// <summary>
        /// Erase the Hessian information.
        /// </summary>
        public void EraseHessianInformation()
        {
            foreach (var layer in this.LayersList)
            {
                layer.EraseHessianInformation();
            }
        }

        /// <summary>
        /// Divides the Hessian information by the divisor.
        /// </summary>
        /// <param name="divisor">The divisor.</param>
        public void DivideHessianInformationBy(double divisor)
        {
            // controls each layer to divide its current diagonal Hessian info by a common divisor. 
            // A check is also made to ensure that each Hessian is strictly zero-positive
            foreach (var layer in this.LayersList)
            {
                layer.DivideHessianInformationBy(divisor);
            }
        }

        /// <summary>
        /// Back propagates the second derivatives.
        /// </summary>
        /// <param name="actualOutputVector">The actual output vector.</param>
        /// <param name="targetOutputVector">The target output vector.</param>
        /// <param name="count">The count.</param>
        public void BackPropagateSecondDerivatives(double[] actualOutputVector, double[] targetOutputVector, uint count)
        {
            // calculates the second derivatives (for diagonal Hessian) and back propagates
            // them through the neuronal network
            if (this.LayersList.Count < 2)
            {
                return;
            }

            if (actualOutputVector == null || targetOutputVector == null || count >= 256)
            {
                return;
            }

            // we use nearly the same nomenclature as above (e.g., "dErr_wrt_dXnm1") even though everything here
            // is actually second derivatives and not first derivatives, since otherwise the ASCII would 
            // become too confusing. To emphasize that these are second derivatives, we insert a "2"
            // such as "d2Err_wrt_dXnm1". We don't insert the second "2" that's conventional for designating
            // second derivatives"
            var layersCount = this.LayersList.Count;
            var neuronCount = this.LayersList[^1].Neurons.Count;
            var errorsList = new ErrorsList(neuronCount);
            var differentials = new List<ErrorsList>(layersCount);

            // Start the process by calculating the second derivative dErr_wrt_dXn for the last layer.
            // for the standard MSE Err function (i.e., 0.5*sumof( (actual-target)^2 ), this differential is 
            // exactly one
            // Point to the last layer
            var layer = this.LayersList.Last();

            for (var ii = 0; ii < layer.Neurons.Count; ii++)
            {
                errorsList.Add(1.0);
            }

            // store Xlast and reserve memory for the remaining vectors stored in differentials
            for (var ii = 0; ii < layersCount - 1; ii++)
            {
                var differential = new ErrorsList(this.LayersList[ii].Neurons.Count);

                for (var kk = 0; kk < this.LayersList[ii].Neurons.Count; kk++)
                {
                    differential.Add(0.0);
                }

                differentials.Add(differential);
            }

            // Last one
            differentials.Add(errorsList); 

            // Now iterate through all layers including the last but excluding the first, starting from
            // the last, and ask each of them to back propagate the second derviative and accumulate
            // the diagonal Hessian, and also to return the second derivative
            // d2Err_wrt_dXnm1 for use as the input value of dErr_wrt_dXn for the next iterated layer (which
            // is the previous layer spatially)
            for (var ii = layersCount - 1; ii > 0; ii--)
            {
                this.LayersList[ii].BackPropagateSecondDerivatives(differentials[ii], differentials[ii - 1]);
            }

            differentials.Clear();
        }

        /// <summary>
        /// Does a sanity check for the weights.
        /// </summary>
        private void PeriodicWeightSanityCheck()
        {
            // Function that simply goes through all weights, and tests them against an arbitrary
            // "reasonable" upper limit. If the upper limit is exceeded, a warning is displayed
            foreach (var layer in this.LayersList)
            {
                layer.PeriodicWeightSanityCheck();
            }
        }
    }
}