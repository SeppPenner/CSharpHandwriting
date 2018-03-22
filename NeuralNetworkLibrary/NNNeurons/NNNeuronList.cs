using System.Collections.Generic;
using NeuralNetworkLibrary.ArchiveSerialization;

namespace NeuralNetworkLibrary.NNNeurons
{
    public class NnNeuronList : List<NnNeuron>, IArchiveSerialization
    {
        public NnNeuronList()
        {
        }

        public NnNeuronList(int capacity)
            : base(capacity)
        {
        }

        // ReSharper disable once UnusedMember.Global
        public NnNeuronList(IEnumerable<NnNeuron> collection)
            : base(collection)
        {
        }

        public void Serialize(Archive ar)
        {
        }
    }
}