using System.Collections.Generic;

namespace NeuralNetworkLibrary.NNNeurons
{
    public class NnNeuronOutputs : List<double>
    {
        // ReSharper disable once UnusedMember.Global
        public NnNeuronOutputs()
        {
        }

        public NnNeuronOutputs(int capacity)
            : base(capacity)
        {
        }

        // ReSharper disable once UnusedMember.Global
        public NnNeuronOutputs(IEnumerable<double> collection)
            : base(collection)
        {
        }
    }
}