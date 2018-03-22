using System.Collections.Generic;
using System.Linq;
using NeuralNetworkLibrary.ArchiveSerialization;
using NeuralNetworkLibrary.NNLayers;
using NeuralNetworkLibrary.NNNeurons;

namespace NeuralNetworkLibrary.NeuralNetwork
{
    // Neural Network class
    public sealed class NeuralNetwork : IArchiveSerialization
    {
        private uint _mcBackprops; // counter used in connection with Weight sanity check
        public double MEtaLearningRate;
        public double MEtaLearningRatePrevious;
        public NnLayerList MLayers;

        public NeuralNetwork()
        {
            MEtaLearningRate = .001; // arbitrary, so that brand-new NNs can be serialized with a non-ridiculous number
            _mcBackprops = 0;
            MLayers = new NnLayerList();
        }

        public void Serialize(Archive ar)
        {
            if (ar.IsStoring())
            {
                // TODO: add storing code here
                ar.Write(MEtaLearningRate);
                ar.Write(MLayers.Count);
                foreach (var lit in MLayers)
                    lit.Serialize(ar);
            }
            else
            {
                // TODO: add loading code here

                // ReSharper disable once InlineOutVariableDeclaration
                double eta;
                ar.Read(out eta);
                MEtaLearningRate = eta; // two-step storage is needed since m_etaLearningRate is "volatile"

                // ReSharper disable once InlineOutVariableDeclaration
                int nLayers;
                var pLayer = (NnLayer) null;

                ar.Read(out nLayers);
                MLayers.Clear();
                MLayers = new NnLayerList(nLayers);
                for (var ii = 0; ii < nLayers; ii++)
                {
                    pLayer = new NnLayer("", pLayer);

                    MLayers.Add(pLayer);
                    pLayer.Serialize(ar);
                }
            }
        }

        public void Calculate(double[] inputVector, int iCount,
            double[] outputVector /* =NULL */, int oCount /* =0 */,
            NnNeuronOutputsList pNeuronOutputs /* =NULL */)
        {
            var lit = MLayers.First();
            // first layer is imput layer: directly set outputs of all of its neurons
            // to the input vector
            if (MLayers.Count > 1)
            {
                var count = 0;
                if (iCount != lit.MNeurons.Count)
                    return;
                foreach (var nit in lit.MNeurons)
                    if (count < iCount)
                    {
                        nit.Output = inputVector[count];
                        count++;
                    }
            }
            //caculate output of next layers
            for (var i = 1; i < MLayers.Count; i++)
                MLayers[i].Calculate();

            // load up output vector with results

            if (outputVector != null)
            {
                lit = MLayers[MLayers.Count - 1];

                for (var ii = 0; ii < oCount; ii++)
                    outputVector[ii] = lit.MNeurons[ii].Output;
            }

            // load up neuron output values with results
            if (pNeuronOutputs == null) return;
            {
                // check for first time use (re-use is expected)
                pNeuronOutputs.Clear();
                // it's empty, so allocate memory for its use
                pNeuronOutputs.Capacity = MLayers.Count;
                foreach (var nnlit in MLayers)
                {
                    var layerOut = new NnNeuronOutputs(nnlit.MNeurons.Count);
                    for (var ii = 0; ii < nnlit.MNeurons.Count; ++ii)
                        layerOut.Add(ii);
                    pNeuronOutputs.Add(layerOut);
                }
            }
        }

        public void Backpropagate(double[] actualOutput, double[] desiredOutput, int count,
            NnNeuronOutputsList pMemorizedNeuronOutputs)
        {
            // backpropagates through the neural net

            if (MLayers.Count >= 2 == false) // there must be at least two layers in the net
                return;
            if (actualOutput == null || desiredOutput == null || count >= 256)
                return;


            // check if it's time for a weight sanity check

            _mcBackprops++;

            if (_mcBackprops % 10000 == 0)
                PeriodicWeightSanityCheck();


            // proceed from the last layer to the first, iteratively
            // We calculate the last layer separately, and first, since it provides the needed derviative
            // (i.e., dErr_wrt_dXnm1) for the previous layers

            // nomenclature:
            //
            // Err is output error of the entire neural net
            // Xn is the output vector on the n-th layer
            // Xnm1 is the output vector of the previous layer
            // Wn is the vector of weights of the n-th layer
            // Yn is the activation value of the n-th layer, i.e., the weighted sum of inputs BEFORE the squashing function is applied
            // F is the squashing function: Xn = F(Yn)
            // F' is the derivative of the squashing function
            //   Conveniently, for F = tanh, then F'(Yn) = 1 - Xn^2, i.e., the derivative can be calculated from the output, without knowledge of the input

            var iSize = MLayers.Count;
            var dErrWrtDXlast = new DErrorsList(MLayers[MLayers.Count - 1].MNeurons.Count);
            var differentials = new List<DErrorsList>(iSize);

            int ii;

            // start the process by calculating dErr_wrt_dXn for the last layer.
            // for the standard MSE Err function (i.e., 0.5*sumof( (actual-target)^2 ), this differential is simply
            // the difference between the target and the actual

            for (ii = 0; ii < MLayers[MLayers.Count - 1].MNeurons.Count; ++ii)
                dErrWrtDXlast.Add(actualOutput[ii] - desiredOutput[ii]);


            // store Xlast and reserve memory for the remaining vectors stored in differentials


            for (ii = 0; ii < iSize - 1; ii++)
            {
                var mDifferential = new DErrorsList(MLayers[ii].MNeurons.Count);
                for (var kk = 0; kk < MLayers[ii].MNeurons.Count; kk++)
                    mDifferential.Add(0.0);
                differentials.Add(mDifferential);
            }
            differentials.Add(dErrWrtDXlast); // last one
            // now iterate through all layers including the last but excluding the first, and ask each of
            // them to backpropagate error and adjust their weights, and to return the differential
            // dErr_wrt_dXnm1 for use as the input value of dErr_wrt_dXn for the next iterated layer

            var bMemorized = pMemorizedNeuronOutputs != null;
            for (var jj = iSize - 1; jj > 0; jj--)
                if (bMemorized)
                    MLayers[jj].Backpropagate(differentials[jj], differentials[jj - 1],
                        pMemorizedNeuronOutputs[jj], pMemorizedNeuronOutputs[jj - 1], MEtaLearningRate);
                else
                    MLayers[jj].Backpropagate(differentials[jj], differentials[jj - 1],
                        null, null, MEtaLearningRate);


            differentials.Clear();
        }

        public void EraseHessianInformation()
        {
            foreach (var lit in MLayers)
                lit.EraseHessianInformation();
        }

        public void DivideHessianInformationBy(double divisor)
        {
            // controls each layer to divide its current diagonal Hessian info by a common divisor. 
            // A check is also made to ensure that each Hessian is strictly zero-positive

            foreach (var lit in MLayers)
                lit.DivideHessianInformationBy(divisor);
        }

        public void BackpropagateSecondDervatives(double[] actualOutputVector, double[] targetOutputVector, uint count)
        {
            // calculates the second dervatives (for diagonal Hessian) and backpropagates
            // them through neural net


            if (MLayers.Count < 2) return;

            if (actualOutputVector == null || targetOutputVector == null || count >= 256)
                return;

            // we use nearly the same nomenclature as above (e.g., "dErr_wrt_dXnm1") even though everything here
            // is actually second derivatives and not first derivatives, since otherwise the ASCII would 
            // become too confusing.  To emphasize that these are second derivatives, we insert a "2"
            // such as "d2Err_wrt_dXnm1".  We don't insert the second "2" that's conventional for designating
            // second derivatives"

            var iSize = MLayers.Count;
            var neuronCount = MLayers[MLayers.Count - 1].MNeurons.Count;
            var d2ErrWrtDXlast = new DErrorsList(neuronCount);
            var differentials = new List<DErrorsList>(iSize);


            // start the process by calculating the second derivative dErr_wrt_dXn for the last layer.
            // for the standard MSE Err function (i.e., 0.5*sumof( (actual-target)^2 ), this differential is 
            // exactly one

            var lit = MLayers.Last(); // point to last layer

            for (var ii = 0; ii < lit.MNeurons.Count; ii++)
                d2ErrWrtDXlast.Add(1.0);

            // store Xlast and reserve memory for the remaining vectors stored in differentials


            for (var ii = 0; ii < iSize - 1; ii++)
            {
                var mDifferential = new DErrorsList(MLayers[ii].MNeurons.Count);
                for (var kk = 0; kk < MLayers[ii].MNeurons.Count; kk++)
                    mDifferential.Add(0.0);
                differentials.Add(mDifferential);
            }

            differentials.Add(d2ErrWrtDXlast); // last one

            // now iterate through all layers including the last but excluding the first, starting from
            // the last, and ask each of
            // them to backpropagate the second derviative and accumulate the diagonal Hessian, and also to
            // return the second dervative
            // d2Err_wrt_dXnm1 for use as the input value of dErr_wrt_dXn for the next iterated layer (which
            // is the previous layer spatially)

            for (var ii = iSize - 1; ii > 0; ii--)
                MLayers[ii].BackpropagateSecondDerivatives(differentials[ii], differentials[ii - 1]);

            differentials.Clear();
        }

        private void PeriodicWeightSanityCheck()
        {
            // fucntion that simply goes through all weights, and tests them against an arbitrary
            // "reasonable" upper limit.  If the upper limit is exceeded, a warning is displayed

            foreach (var lit in MLayers)
                lit.PeriodicWeightSanityCheck();
        }
    }
}