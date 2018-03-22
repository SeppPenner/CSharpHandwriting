using System.Collections.Generic;
using NeuralNetworkLibrary.ArchiveSerialization;

namespace NeuralNetworkLibrary.NNWeights
{
    public class NnWeightList : List<NnWeight>, IArchiveSerialization
    {
        public NnWeightList()
        {
        }

        public NnWeightList(int capacity)
            : base(capacity)
        {
        }

        // ReSharper disable once UnusedMember.Global
        public NnWeightList(IEnumerable<NnWeight> collection)
            : base(collection)
        {
        }

        public void Serialize(Archive ar)
        {
        }
    }
}