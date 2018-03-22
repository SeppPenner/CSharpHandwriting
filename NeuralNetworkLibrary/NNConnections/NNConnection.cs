using NeuralNetworkLibrary.ArchiveSerialization;

namespace NeuralNetworkLibrary.NNConnections
{
    // Connection class

    public class NnConnection : IArchiveSerialization
    {
        public uint NeuronIndex;
        public uint WeightIndex;

        public NnConnection()
        {
            NeuronIndex = 0xffffffff;
            WeightIndex = 0xffffffff;
        }

        public NnConnection(uint iNeuron, uint iWeight)
        {
            NeuronIndex = iNeuron;
            WeightIndex = iWeight;
        }

        public void Serialize(Archive ar)
        {
        }
    }
}