using System;
using System.Threading;
using NeuralNetworkLibrary.Activation_Functions;
using NeuralNetworkLibrary.ArchiveSerialization;
using NeuralNetworkLibrary.NeuralNetwork;
using NeuralNetworkLibrary.NNConnections;
using NeuralNetworkLibrary.NNNeurons;
using NeuralNetworkLibrary.NNWeights;

namespace NeuralNetworkLibrary.NNLayers
{
    // Layer class
    public sealed class NnLayer : IArchiveSerialization
    {
        private readonly NnLayer _mpPrevLayer;
        private string _label;

        private bool _mBFloatingPointWarning
            ; // flag for one-time warning (per layer) about potential floating point overflow

        // ReSharper disable once NotAccessedField.Local
        private SigmoidFunction _mSigmoid;
        public NnNeuronList MNeurons;
        public NnWeightList MWeights;

        // ReSharper disable once UnusedMember.Global
        public NnLayer()
        {
            _label = "";
            _mpPrevLayer = null;
            _mSigmoid = new SigmoidFunction();
            MWeights = new NnWeightList();
            MNeurons = new NnNeuronList();
            Initialize();
        }

        public NnLayer(string str, NnLayer pPrev /* =NULL */)
        {
            _label = str;
            _mpPrevLayer = pPrev;
            _mSigmoid = new SigmoidFunction();
            MWeights = new NnWeightList();
            MNeurons = new NnNeuronList();
        }

        private void Initialize()
        {
            MWeights.Clear();
            MNeurons.Clear();
            _mBFloatingPointWarning = false;
        }

        public void Calculate()
        {
            double dSum = 0;
            foreach (var nit in MNeurons)
            {
                foreach (var cit in nit.MConnections)
                    if (cit == nit.MConnections[0])
                        dSum = MWeights[(int) cit.WeightIndex].Value;
                    else
                        dSum += MWeights[(int) cit.WeightIndex].Value *
                                _mpPrevLayer.MNeurons[(int) cit.NeuronIndex].Output;

                nit.Output = SigmoidFunction.Sigmoid(dSum);
            }
        }

        /////////////
        public void Backpropagate(DErrorsList dErrWrtDXn /* in */,
            DErrorsList dErrWrtDXnm1 /* out */,
            NnNeuronOutputs thisLayerOutput, // memorized values of this layer's output
            NnNeuronOutputs prevLayerOutput, // memorized values of previous layer's output
            double etaLearningRate)
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
                var dErrWrtDYn = new DErrorsList(MNeurons.Count);
                //
                //	std::vector< double > dErr_wrt_dWn( m_Weights.size(), 0.0 );  // important to initialize to zero
                //////////////////////////////////////////////////
                //
                ///// DESIGN TRADEOFF: REVIEW !!
                // We would prefer (for ease of coding) to use STL vector for the array "dErr_wrt_dWn", which is the 
                // differential of the current pattern's error wrt weights in the layer.  However, for layers with
                // many weights, such as fully-connected layers, there are also many weights.  The STL vector
                // class's allocator is remarkably stupid when allocating large memory chunks, and causes a remarkable 
                // number of page faults, with a consequent slowing of the application's overall execution time.

                // To fix this, I tried using a plain-old C array, by new'ing the needed space from the heap, and 
                // delete[]'ing it at the end of the function.  However, this caused the same number of page-fault
                // errors, and did not improve performance.

                // So I tried a plain-old C array allocated on the stack (i.e., not the heap).  Of course I could not
                // write a statement like 
                //    double dErr_wrt_dWn[ m_Weights.size() ];
                // since the compiler insists upon a compile-time known constant value for the size of the array.  
                // To avoid this requirement, I used the _alloca function, to allocate memory on the stack.
                // The downside of this is excessive stack usage, and there might be stack overflow probelms.  That's why
                // this comment is labeled "REVIEW"
                var dErrWrtDWn = new double[MWeights.Count];
                for (ii = 0; ii < MWeights.Count; ii++)
                    dErrWrtDWn[ii] = 0.0;

                var bMemorized = thisLayerOutput != null && prevLayerOutput != null;
                // calculate dErr_wrt_dYn = F'(Yn) * dErr_wrt_Xn

                for (ii = 0; ii < MNeurons.Count; ii++)
                {
                    output = bMemorized ? thisLayerOutput[ii] : MNeurons[ii].Output;

                    dErrWrtDYn.Add(SigmoidFunction.Dsigmoid(output) * dErrWrtDXn[ii]);
                }

                // calculate dErr_wrt_Wn = Xnm1 * dErr_wrt_Yn
                // For each neuron in this layer, go through the list of connections from the prior layer, and
                // update the differential for the corresponding weight

                ii = 0;
                foreach (var nit in MNeurons)
                {
                    foreach (var cit in nit.MConnections)
                    {
                        kk = cit.NeuronIndex;
                        if (kk == 0xffffffff)
                            output = 1.0; // this is the bias weight
                        else
                            output = bMemorized ? prevLayerOutput[(int) kk] : _mpPrevLayer.MNeurons[(int) kk].Output;
                        dErrWrtDWn[cit.WeightIndex] += dErrWrtDYn[ii] * output;
                    }

                    ii++;
                }
                // calculate dErr_wrt_Xnm1 = Wn * dErr_wrt_dYn, which is needed as the input value of
                // dErr_wrt_Xn for backpropagation of the next (i.e., previous) layer
                // For each neuron in this layer

                ii = 0;
                foreach (var nit in MNeurons)
                {
                    foreach (var cit in nit.MConnections)
                    {
                        kk = cit.NeuronIndex;
                        if (kk == 0xffffffff) continue;
                        // we exclude ULONG_MAX, which signifies the phantom bias neuron with
                        // constant output of "1", since we cannot train the bias neuron

                        var nIndex = (int) kk;
                        dErrWrtDXnm1[nIndex] += dErrWrtDYn[ii] * MWeights[(int) cit.WeightIndex].Value;
                    }

                    ii++; // ii tracks the neuron iterator
                }
                // finally, update the weights of this layer neuron using dErr_wrt_dW and the learning rate eta
                // Use an atomic compare-and-exchange operation, which means that another thread might be in 
                // the process of backpropagation and the weights might have shifted slightly
                const double dMicron = 0.10;
                for (jj = 0; jj < MWeights.Count; ++jj)
                {
                    var divisor = MWeights[jj].DiagHessian + dMicron;

                    // the following code has been rendered unnecessary, since the value of the Hessian has been
                    // verified when it was created, so as to ensure that it is strictly
                    // zero-positve.  Thus, it is impossible for the diagHessian to be less than zero,
                    // and it is impossible for the divisor to be less than dMicron
                    /*
                    if ( divisor < dMicron )  
                    {
                    // it should not be possible to reach here, since everything in the second derviative equations 
                    // is strictly zero-positive, and thus "divisor" should definitely be as large as MICRON.
		
                      ASSERT( divisor >= dMicron );
                      divisor = 1.0 ;  // this will limit the size of the update to the same as the size of gloabal eta
                      }
                    */
                    var epsilon = etaLearningRate / divisor;
                    var oldValue = MWeights[jj].Value;
                    var newValue = oldValue - epsilon * dErrWrtDWn[jj];
                    while (Math.Abs(oldValue - Interlocked.CompareExchange(
                                        ref MWeights[jj].Value,
                                        newValue, oldValue)) > 0.00000000000000000001)
                    {
                        // another thread must have modified the weight.

                        // Obtain its new value, adjust it, and try again

                        oldValue = MWeights[jj].Value;
                        newValue = oldValue - epsilon * dErrWrtDWn[jj];
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }


        public void PeriodicWeightSanityCheck()
        {
            // called periodically by the neural net, to request a check on the "reasonableness" of the 
            // weights.  The warning message is given only once per layer


            foreach (var wit in MWeights)
            {
                var val = Math.Abs(wit.Value);

                if (val > 100.0 && _mBFloatingPointWarning == false)
                    _mBFloatingPointWarning = true;
            }
        }


        public void EraseHessianInformation()
        {
            // goes through all the weights associated with this layer, and sets each of their
            // diagHessian value to zero

            foreach (var wit in MWeights)
                wit.DiagHessian = 0.0;
        }

        public void DivideHessianInformationBy(double divisor)
        {
            // goes through all the weights associated with this layer, and divides each of their
            // diagHessian value by the indicated divisor


            foreach (var wit in MWeights)
            {
                var dTemp = wit.DiagHessian;

                if (dTemp < 0.0)
                    dTemp = 0.0;

                wit.DiagHessian = dTemp / divisor;
            }
        }

        public void BackpropagateSecondDerivatives(DErrorsList d2ErrWrtDXn /* in */,
            DErrorsList d2ErrWrtDXnm1 /* out */)
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
            double dTemp;

            var d2ErrWrtDYn = new DErrorsList(MNeurons.Count);
            //
            // std::vector< double > d2Err_wrt_dWn( m_Weights.size(), 0.0 );  // important to initialize to zero
            //////////////////////////////////////////////////
            //
            ///// DESIGN TRADEOFF: REVIEW !!
            //
            // Note that the reasoning of this comment is identical to that in the NNLayer::Backpropagate() 
            // function, from which the instant BackpropagateSecondDerivatives() function is derived from
            //
            // We would prefer (for ease of coding) to use STL vector for the array "d2Err_wrt_dWn", which is the 
            // second differential of the current pattern's error wrt weights in the layer.  However, for layers with
            // many weights, such as fully-connected layers, there are also many weights.  The STL vector
            // class's allocator is remarkably stupid when allocating large memory chunks, and causes a remarkable 
            // number of page faults, with a consequent slowing of the application's overall execution time.

            // To fix this, I tried using a plain-old C array, by new'ing the needed space from the heap, and 
            // delete[]'ing it at the end of the function.  However, this caused the same number of page-fault
            // errors, and did not improve performance.

            // So I tried a plain-old C array allocated on the stack (i.e., not the heap).  Of course I could not
            // write a statement like 
            //    double d2Err_wrt_dWn[ m_Weights.size() ];
            // since the compiler insists upon a compile-time known constant value for the size of the array.  
            // To avoid this requirement, I used the _alloca function, to allocate memory on the stack.
            // The downside of this is excessive stack usage, and there might be stack overflow probelms.  That's why
            // this comment is labeled "REVIEW"

            var d2ErrWrtDWn = new double[MWeights.Count];
            for (ii = 0; ii < MWeights.Count; ii++)
                d2ErrWrtDWn[ii] = 0.0;
            // calculate d2Err_wrt_dYn = ( F'(Yn) )^2 * dErr_wrt_Xn (where dErr_wrt_Xn is actually a second derivative )

            for (ii = 0; ii < MNeurons.Count; ii++)
            {
                output = MNeurons[ii].Output;
                dTemp = SigmoidFunction.Dsigmoid(output);
                d2ErrWrtDYn.Add(d2ErrWrtDXn[ii] * dTemp * dTemp);
            }
            // calculate d2Err_wrt_Wn = ( Xnm1 )^2 * d2Err_wrt_Yn (where dE2rr_wrt_Yn is actually a second derivative)
            // For each neuron in this layer, go through the list of connections from the prior layer, and
            // update the differential for the corresponding weight

            ii = 0;
            foreach (var nit in MNeurons)
            {
                foreach (var cit in nit.MConnections)
                    try
                    {
                        kk = cit.NeuronIndex;
                        output = kk == 0xffffffff ? 1.0 : _mpPrevLayer.MNeurons[(int) kk].Output;

                        ////////////	ASSERT( (*cit).WeightIndex < d2Err_wrt_dWn.size() );  // since after changing d2Err_wrt_dWn to a C-style array, the size() function this won't work
                        //d2Err_wrt_dWn[cit.WeightIndex] += d2Err_wrt_dYn[ii] * output * output;
                        d2ErrWrtDWn[cit.WeightIndex] = d2ErrWrtDYn[ii] * output * output;
                    }
                    catch (Exception)
                    {
                        // ignored
                    }

                ii++;
            }
            // calculate d2Err_wrt_Xnm1 = ( Wn )^2 * d2Err_wrt_dYn (where d2Err_wrt_dYn is a second derivative not a first).
            // d2Err_wrt_Xnm1 is needed as the input value of
            // d2Err_wrt_Xn for backpropagation of second derivatives for the next (i.e., previous spatially) layer
            // For each neuron in this layer

            ii = 0;
            foreach (var nit in MNeurons)
            {
                foreach (var cit in nit.MConnections)
                    try
                    {
                        kk = cit.NeuronIndex;
                        if (kk == 0xffffffff) continue;
                        // we exclude ULONG_MAX, which signifies the phantom bias neuron with
                        // constant output of "1", since we cannot train the bias neuron

                        var nIndex = (int) kk;
                        dTemp = MWeights[(int) cit.WeightIndex].Value;
                        d2ErrWrtDXnm1[nIndex] += d2ErrWrtDYn[ii] * dTemp * dTemp;
                    }
                    catch (Exception)
                    {
                        return;
                    }

                ii++; // ii tracks the neuron iterator
            }

            // finally, update the diagonal Hessians for the weights of this layer neuron using dErr_wrt_dW.
            // By design, this function (and its iteration over many (approx 500 patterns) is called while a 
            // single thread has locked the nueral network, so there is no possibility that another
            // thread might change the value of the Hessian.  Nevertheless, since it's easy to do, we
            // use an atomic compare-and-exchange operation, which means that another thread might be in 
            // the process of backpropagation of second derivatives and the Hessians might have shifted slightly

            for (jj = 0; jj < MWeights.Count; jj++)
            {
                var oldValue = MWeights[jj].DiagHessian;
                var newValue = oldValue + d2ErrWrtDWn[jj];
                MWeights[jj].DiagHessian = newValue;
            }
        }

        // ReSharper disable once NotAccessedField.Local
        public void Serialize(Archive ar)
        {
            if (ar.IsStoring())
            {
                // TODO: add storing code here

                ar.Write(_label);
                //ar.WriteString(_T("\r\n"));  // ar.ReadString will look for \r\n when loading from the archive
                ar.Write(MNeurons.Count);
                ar.Write(MWeights.Count);


                foreach (var nit in MNeurons)
                {
                    ar.Write(nit.Label);
                    ar.Write(nit.MConnections.Count);

                    foreach (var cit in nit.MConnections)
                    {
                        ar.Write(cit.NeuronIndex);
                        ar.Write(cit.WeightIndex);
                    }
                }

                foreach (var wit in MWeights)
                {
                    ar.Write(wit.Label);
                    ar.Write(wit.Value);
                }
            }
            else
            {
                // TODO: add loading code here

                // ReSharper disable once InlineOutVariableDeclaration
                string str;
                //Read Layter's label
                ar.Read(out str);
                _label = str;

                // ReSharper disable InlineOutVariableDeclaration
                int iNumNeurons, iNumWeights;

                //Read No of Neuron, Weight
                ar.Read(out iNumNeurons);
                ar.Read(out iNumWeights);
                if (iNumNeurons == 0) return;
                //clear neuron list and weight list.
                MNeurons.Clear();
                MNeurons = new NnNeuronList(iNumNeurons);
                MWeights.Clear();
                MWeights = new NnWeightList(iNumWeights);

                int ii;
                int jj;
                for (ii = 0; ii < iNumNeurons; ii++)
                {
                    //ar.Read Neuron's label
                    ar.Read(out str);
                    //Read Neuron's Connection number
                    int iNumConnections;
                    ar.Read(out iNumConnections);
                    var pNeuron = new NnNeuron(str, iNumConnections) {Label = str};
                    MNeurons.Add(pNeuron);
                    for (jj = 0; jj < iNumConnections; jj++)
                    {
                        var conn = new NnConnection();
                        ar.Read(out conn.NeuronIndex);
                        ar.Read(out conn.WeightIndex);
                        pNeuron.AddConnection(conn);
                    }
                }

                for (jj = 0; jj < iNumWeights; jj++)
                {
                    ar.Read(out str);
                    double value;
                    ar.Read(out value);

                    var pWeight = new NnWeight(str, value);
                    MWeights.Add(pWeight);
                }
            }
        }
    }
}