using System.Collections.Generic;
using NeuralNetworkLibrary.ArchiveSerialization;

namespace NeuralNetworkLibrary.NNLayers
{
    public class NnLayerList : List<NnLayer>, IArchiveSerialization
    {
        public NnLayerList()
        {
        }

        public NnLayerList(int capacity)
            : base(capacity)
        {
        }

        // ReSharper disable once UnusedMember.Global
        public NnLayerList(IEnumerable<NnLayer> collection)
            : base(collection)
        {
        }

        public void Serialize(Archive ar)
        {
        }
    }
}