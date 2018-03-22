using NeuralNetworkLibrary.ArchiveSerialization;
using NeuralNetworkLibrary.NNConnections;

namespace NeuralNetworkLibrary.NNNeurons
{
    // Neuron class
    public sealed class NnNeuron : IArchiveSerialization
    {
        public readonly NnConnectionList MConnections;
        public string Label;
        public double Output;

        // ReSharper disable once UnusedMember.Global
        public NnNeuron()
        {
            Initialize();
            Label = "";
            Output = 0.0;
            MConnections = new NnConnectionList();
        }

        public NnNeuron(string str)
        {
            Label = str;
            Output = 0.0;
            MConnections = new NnConnectionList();
            Initialize();
        }

        public NnNeuron(string str, int icount)
        {
            Label = str;
            Output = 0.0;
            MConnections = new NnConnectionList(icount);
            Initialize();
        }

        public void Serialize(Archive ar)
        {
        }

        public void AddConnection(uint iNeuron, uint iWeight)
        {
            var conn = new NnConnection(iNeuron, iWeight);
            MConnections.Add(conn);
        }

        public void AddConnection(NnConnection conn)
        {
            MConnections.Add(conn);
        }

        private static void Initialize()
        {
        }
    }
}