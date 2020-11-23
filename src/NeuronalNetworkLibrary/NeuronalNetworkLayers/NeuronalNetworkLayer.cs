// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NeuronalNetworkLayer.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   The neuronal network layer class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NeuronalNetworkLibrary.NeuronalNetworkLayers
{
    using System;
    using System.Threading;

    using NeuronalNetworkLibrary.Activation_Functions;
    using NeuronalNetworkLibrary.ArchiveSerialization;
    using NeuronalNetworkLibrary.NeuronalNetwork;
    using NeuronalNetworkLibrary.NeuronalNetworkConnections;
    using NeuronalNetworkLibrary.NeuronalNetworkNeurons;
    using NeuronalNetworkLibrary.NeuronalNetworkWeights;

    /// <inheritdoc cref="IArchiveSerialization"/>
    /// <summary>
    /// The neuronal network layer class.
    /// </summary>
    /// <seealso cref="IArchiveSerialization"/>
    // ReSharper disable ArrangeRedundantParentheses
    public sealed class NeuronalNetworkLayer : IArchiveSerialization
    {
        /// <summary>
        /// The previous layer.
        /// </summary>
        private readonly NeuronalNetworkLayer previousLayer;

        /// <summary>
        /// The label.
        /// </summary>
        private string label;

        /// <summary>
        /// A value indicating whether a floating point overflow warning is shown or not.
        /// </summary>
        private bool floatingPointWarning;

        /// <summary>
        /// The Sigmoid function.
        /// </summary>
        // ReSharper disable once NotAccessedField.Local
        private SigmoidFunction sigmoidFunction;

        /// <summary>
        /// Initializes a new instance of the <see cref="NeuronalNetworkLayer"/> class.
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public NeuronalNetworkLayer()
        {
            this.label = string.Empty;
            this.previousLayer = null;
            this.sigmoidFunction = new SigmoidFunction();
            this.Weights = new NeuronalNetworkWeightList();
            this.Neurons = new NeuronalNetworkNeuronList();
            this.Initialize();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NeuronalNetworkLayer"/> class.
        /// </summary>
        /// <param name="label">The label.</param>
        /// <param name="previousLayer">The previous layer.</param>
        public NeuronalNetworkLayer(string label, NeuronalNetworkLayer previousLayer)
        {
            this.label = label;
            this.previousLayer = previousLayer;
            this.sigmoidFunction = new SigmoidFunction();
            this.Weights = new NeuronalNetworkWeightList();
            this.Neurons = new NeuronalNetworkNeuronList();
        }

        /// <summary>
        /// Gets or sets the neurons.
        /// </summary>
        public NeuronalNetworkNeuronList Neurons { get; set; }

        /// <summary>
        /// Gets or sets the weights.
        /// </summary>
        public NeuronalNetworkWeightList Weights { get; set; }

        /// <summary>
        /// Calculates the layer data.
        /// </summary>
        public void Calculate()
        {
            double sum = 0;

            foreach (var nit in this.Neurons)
            {
                foreach (var cit in nit.Connections)
                {
                    if (cit == nit.Connections[0])
                    {
                        sum = this.Weights[(int)cit.WeightIndex].Value;
                    }
                    else
                    {
                        sum += this.Weights[(int)cit.WeightIndex].Value
                               * this.previousLayer.Neurons[(int)cit.NeuronIndex].Output;
                    }
                }

                nit.Output = SigmoidFunction.Sigmoid(sum);
            }
        }

        /// <summary>
        /// Back propagates the neuronal network layer.
        /// </summary>
        /// <param name="errorList">The error list.</param>
        /// <param name="previousErrorList">The previous error list.</param>
        /// <param name="thisLayerOutput">The values of this layer.</param>
        /// <param name="previousLayerOutput">The values of the previous layer.</param>
        /// <param name="etaLearningRate">The ETA learning rate.</param>
        public void BackPropagate(ErrorsList errorList, ErrorsList previousErrorList, NeuronalNetworkNeuronOutputs thisLayerOutput, NeuronalNetworkNeuronOutputs previousLayerOutput, double etaLearningRate)
        {
            // nomenclature (repeated from NeuralNetwork class):
            //
            // Err is output error of the entire neural net
            // Xn is the output vector on the n-th layer
            // Xnm1 is the output vector of the previous layer
            // Wn is the vector of weights of the n-th layer
            // Yn is the activation value of the n-th layer, i.e., the weighted sum of inputs BEFORE the squashing function is applied
            // F is the squashing function: Xn = F(Yn)
            // F' is the derivative of the squashing function
            //   Conveniently, for F = tanh, then F'(Yn) = 1 - Xn^2, i.e., the derivative can be calculated from the output, without knowledge of the input
            try
            {
                int ii, jj;
                uint kk;
                double output;
                var neuronsErrorList = new ErrorsList(this.Neurons.Count);
                var weightsErrorList = new double[this.Weights.Count];

                for (ii = 0; ii < this.Weights.Count; ii++)
                {
                    weightsErrorList[ii] = 0.0;
                }

                var memorized = thisLayerOutput != null && previousLayerOutput != null;
                
                // Calculate dErr_wrt_dYn = F'(Yn) * dErr_wrt_Xn
                for (ii = 0; ii < this.Neurons.Count; ii++)
                {
                    output = memorized ? thisLayerOutput[ii] : this.Neurons[ii].Output;
                    neuronsErrorList.Add(SigmoidFunction.DeSigmoid(output) * errorList[ii]);
                }

                // Calculate dErr_wrt_Wn = Xnm1 * dErr_wrt_Yn
                // For each neuron in this layer, go through the list of connections from the prior layer, and
                // update the differential for the corresponding weight
                ii = 0;

                foreach (var neuron in this.Neurons)
                {
                    foreach (var connection in neuron.Connections)
                    {
                        kk = connection.NeuronIndex;
                        if (kk == 0xffffffff)
                        {
                            // This is the bias weight
                            output = 1.0;
                        }
                        else
                        {
                            output = memorized
                                         ? previousLayerOutput[(int)kk]
                                         : this.previousLayer.Neurons[(int)kk].Output;
                        }

                        weightsErrorList[connection.WeightIndex] += neuronsErrorList[ii] * output;
                    }

                    ii++;
                }

                // Calculate dErr_wrt_Xnm1 = Wn * dErr_wrt_dYn, which is needed as the input value of
                // dErr_wrt_Xn for back propagation of the next (i.e., previous) layer
                // For each neuron in this layer
                ii = 0;

                foreach (var neuron in this.Neurons)
                {
                    foreach (var connection in neuron.Connections)
                    {
                        kk = connection.NeuronIndex;

                        // We exclude ULONG_MAX, which signifies the phantom bias neuron with
                        // constant output of "1", since we cannot train the bias neuron
                        if (kk == 0xffffffff)
                        {
                            continue;
                        }

                        var index = (int)kk;
                        previousErrorList[index] += neuronsErrorList[ii] * this.Weights[(int)connection.WeightIndex].Value;
                    }

                    // ii tracks the neuron iterator
                    ii++;
                }

                // Finally, update the weights of this layer neuron using dErr_wrt_dW and the learning rate eta
                // Use an atomic compare-and-exchange operation, which means that another thread might be in 
                // the process of back propagation and the weights might have shifted slightly
                const double Micron = 0.10;

                for (jj = 0; jj < this.Weights.Count; ++jj)
                {
                    var divisor = this.Weights[jj].DiagonalHessian + Micron;

                    // The following code has been rendered unnecessary, since the value of the Hessian has been
                    // verified when it was created, so as to ensure that it is strictly
                    // zero-positive. Thus, it is impossible for the diagHessian to be less than zero,
                    // and it is impossible for the divisor to be less than micron
                    var epsilon = etaLearningRate / divisor;
                    var oldValue = this.Weights[jj].Value;
                    var newValue = oldValue - (epsilon * weightsErrorList[jj]);
                    var currentWeightValue = this.Weights[jj].Value;

                    while (Math.Abs(oldValue - Interlocked.CompareExchange(ref currentWeightValue, newValue, oldValue)) > 0.00000000000000000001)
                    {
                        // Another thread must have modified the weight.
                        // Obtain its new value, adjust it, and try again
                        oldValue = this.Weights[jj].Value;
                        newValue = oldValue - (epsilon * weightsErrorList[jj]);
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        /// <summary>
        /// Does a sanity check on the weights.
        /// </summary>
        public void PeriodicWeightSanityCheck()
        {
            // Called periodically by the neuronal network, to request a check on the "reasonableness" of the 
            // weights. The warning message is given only once per layer
            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            foreach (var weight in this.Weights)
            {
                var val = Math.Abs(weight.Value);

                if (val > 100.0 && this.floatingPointWarning == false)
                {
                    this.floatingPointWarning = true;
                }
            }
        }

        /// <summary>
        /// Erases the Hessian information.
        /// </summary>
        public void EraseHessianInformation()
        {
            // Goes through all the weights associated with this layer, and sets each of their
            // diagHessian value to zero
            foreach (var weight in this.Weights)
            {
                weight.DiagonalHessian = 0.0;
            }
        }

        /// <summary>
        /// Divides the Hessian information by a divisor.
        /// </summary>
        /// <param name="divisor">The divisor.</param>
        public void DivideHessianInformationBy(double divisor)
        {
            // Goes through all the weights associated with this layer, and divides each of their
            // diagHessian value by the indicated divisor
            foreach (var weight in this.Weights)
            {
                var tempValue = weight.DiagonalHessian;

                if (tempValue < 0.0)
                {
                    tempValue = 0.0;
                }

                weight.DiagonalHessian = tempValue / divisor;
            }
        }

        /// <summary>
        /// Back propagates and send the derivatives.
        /// </summary>
        /// <param name="errorList">The error list of the current layer.</param>
        /// <param name="previousErrorList">The error list of the previous layer.</param>
        public void BackPropagateSecondDerivatives(ErrorsList errorList, ErrorsList previousErrorList)
        {
            // nomenclature (repeated from NeuralNetwork class)
            // NOTE: even though we are addressing SECOND derivatives ( and not first derivatives),
            // we use nearly the same notation as if there were first derivatives, since otherwise the
            // ASCII look would be confusing.  We add one "2" but not two "2's", such as "d2Err_wrt_dXn",
            // to give a gentle emphasis that we are using second derivatives
            //
            // Err is output error of the entire neural net
            // Xn is the output vector on the n-th layer
            // Xnm1 is the output vector of the previous layer
            // Wn is the vector of weights of the n-th layer
            // Yn is the activation value of the n-th layer, i.e., the weighted sum of inputs BEFORE the squashing function is applied
            // F is the squashing function: Xn = F(Yn)
            // F' is the derivative of the squashing function
            //   Conveniently, for F = tanh, then F'(Yn) = 1 - Xn^2, i.e., the derivative can be calculated from the output, without knowledge of the input 
            int ii, jj;
            uint kk;
            double output;
            double tempValue;

            var neuronsErrorList = new ErrorsList(this.Neurons.Count);
            var weightsErrorList = new double[this.Weights.Count];

            for (ii = 0; ii < this.Weights.Count; ii++)
            {
                weightsErrorList[ii] = 0.0;
            }

            // Calculate d2Err_wrt_dYn = ( F'(Yn) )^2 * dErr_wrt_Xn (where dErr_wrt_Xn is actually a second derivative )
            for (ii = 0; ii < this.Neurons.Count; ii++)
            {
                output = this.Neurons[ii].Output;
                tempValue = SigmoidFunction.DeSigmoid(output);
                neuronsErrorList.Add(errorList[ii] * tempValue * tempValue);
            }

            // Calculate d2Err_wrt_Wn = ( Xnm1 )^2 * d2Err_wrt_Yn (where dE2rr_wrt_Yn is actually a second derivative)
            // For each neuron in this layer, go through the list of connections from the prior layer, and
            // update the differential for the corresponding weight
            ii = 0;

            foreach (var neuron in this.Neurons)
            {
                foreach (var connection in neuron.Connections)
                {
                    try
                    {
                        kk = connection.NeuronIndex;
                        output = kk == 0xffffffff ? 1.0 : this.previousLayer.Neurons[(int)kk].Output;
                        weightsErrorList[connection.WeightIndex] = neuronsErrorList[ii] * output * output;
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }

                ii++;
            }

            // Calculate d2Err_wrt_Xnm1 = ( Wn )^2 * d2Err_wrt_dYn (where d2Err_wrt_dYn is a second derivative not a first).
            // d2Err_wrt_Xnm1 is needed as the input value of
            // d2Err_wrt_Xn for back propagation of second derivatives for the next (i.e., previous spatially) layer
            // For each neuron in this layer
            ii = 0;

            foreach (var neuron in this.Neurons)
            {
                foreach (var connection in neuron.Connections)
                {
                    try
                    {
                        kk = connection.NeuronIndex;

                        // We exclude ULONG_MAX, which signifies the phantom bias neuron with
                        // constant output of "1", since we cannot train the bias neuron
                        if (kk == 0xffffffff)
                        {
                            continue;
                        }
                        
                        var index = (int)kk;
                        tempValue = this.Weights[(int)connection.WeightIndex].Value;
                        previousErrorList[index] += neuronsErrorList[ii] * tempValue * tempValue;
                    }
                    catch (Exception)
                    {
                        return;
                    }
                }

                // ii tracks the neuron iterator
                ii++; 
            }

            // Finally, update the diagonal Hessians for the weights of this layer neuron using dErr_wrt_dW.
            // By design, this function (and its iteration over many (approx 500 patterns) is called while a 
            // single thread has locked the neuronal network, so there is no possibility that another
            // thread might change the value of the Hessian.  Nevertheless, since it's easy to do, we
            // use an atomic compare-and-exchange operation, which means that another thread might be in 
            // the process of back propagation of second derivatives and the Hessians might have shifted slightly
            for (jj = 0; jj < this.Weights.Count; jj++)
            {
                var oldValue = this.Weights[jj].DiagonalHessian;
                var newValue = oldValue + weightsErrorList[jj];
                this.Weights[jj].DiagonalHessian = newValue;
            }
        }

        /// <inheritdoc cref="IArchiveSerialization"/>
        /// <summary>
        /// Serializes the archive.
        /// </summary>
        /// <param name="archive">The archive.</param>
        /// <seealso cref="IArchiveSerialization"/>
        // ReSharper disable once NotAccessedField.Local
        public void Serialize(Archive archive)
        {
            if (archive.IsStoring())
            {
                archive.Write(this.label);
                archive.Write(this.Neurons.Count);
                archive.Write(this.Weights.Count);

                foreach (var neuron in this.Neurons)
                {
                    archive.Write(neuron.Label);
                    archive.Write(neuron.Connections.Count);

                    foreach (var cit in neuron.Connections)
                    {
                        archive.Write(cit.NeuronIndex);
                        archive.Write(cit.WeightIndex);
                    }
                }

                foreach (var wit in this.Weights)
                {
                    archive.Write(wit.Label);
                    archive.Write(wit.Value);
                }
            }
            else
            {
                // Read the label
                archive.Read(out string localLabel);
                this.label = localLabel;

                // Read numbers of neurons and weights
                archive.Read(out int numberOfNeurons);
                archive.Read(out int numberOfWeights);

                if (numberOfNeurons == 0)
                {
                    return;
                }

                // Clear neuron list and weight list.
                this.Neurons.Clear();
                this.Neurons = new NeuronalNetworkNeuronList(numberOfNeurons);
                this.Weights.Clear();
                this.Weights = new NeuronalNetworkWeightList(numberOfWeights);

                int ii;
                int jj;

                for (ii = 0; ii < numberOfNeurons; ii++)
                {
                    // Read the neuron's label
                    archive.Read(out localLabel);

                    // Read the neuron's connection number
                    archive.Read(out int numberOfConnections);
                    var neuron = new NeuronalNetworkNeuron(localLabel, numberOfConnections) { Label = localLabel };
                    this.Neurons.Add(neuron);

                    for (jj = 0; jj < numberOfConnections; jj++)
                    {
                        var connection = new NeuronalNetworkConnection();

                        archive.Read(out uint neuronIndex);
                        archive.Read(out uint weightIndex);

                        connection.NeuronIndex = neuronIndex;
                        connection.WeightIndex = weightIndex;
                        neuron.AddConnection(connection);
                    }
                }

                for (jj = 0; jj < numberOfWeights; jj++)
                {
                    archive.Read(out localLabel);
                    archive.Read(out double value);
                    var weight = new NeuronalNetworkWeight(localLabel, value);
                    this.Weights.Add(weight);
                }
            }
        }

        /// <summary>
        /// Initializes the layer.
        /// </summary>
        private void Initialize()
        {
            this.Weights.Clear();
            this.Neurons.Clear();
            this.floatingPointWarning = false;
        }
    }
}