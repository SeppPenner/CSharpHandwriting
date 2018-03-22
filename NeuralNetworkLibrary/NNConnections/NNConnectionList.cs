using System.Collections.Generic;
using NeuralNetworkLibrary.ArchiveSerialization;

namespace NeuralNetworkLibrary.NNConnections
{
    public class NnConnectionList : List<NnConnection>, IArchiveSerialization
    {
        public NnConnectionList()
        {
        }

        public NnConnectionList(int capacity)
            : base(capacity)
        {
        }

        // ReSharper disable once UnusedMember.Global
        public NnConnectionList(IEnumerable<NnConnection> collection)
            : base(collection)
        {
        }

        public void Serialize(Archive ar)
        {
        }
    }
}