using System.Collections.Generic;

namespace NeuralNetworkLibrary.NNNeurons
{
    public class NnNeuronOutputsList : List<NnNeuronOutputs>
    {
        public NnNeuronOutputsList()
        {
        }

        // ReSharper disable once UnusedMember.Global
        public NnNeuronOutputsList(int capacity)
            : base(capacity)
        {
        }

        // ReSharper disable once UnusedMember.Global
        public NnNeuronOutputsList(IEnumerable<NnNeuronOutputs> collection)
            : base(collection)
        {
        }
    }
}