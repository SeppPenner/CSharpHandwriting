using System.Collections.Generic;

namespace NeuralNetworkLibrary.NeuralNetwork
{
    public class DErrorsList : List<double>
    {
        // ReSharper disable once UnusedMember.Global
        public DErrorsList()
        {
        }

        public DErrorsList(int capacity)
            : base(capacity)
        {
        }

        // ReSharper disable once UnusedMember.Global
        public DErrorsList(IEnumerable<double> collection)
            : base(collection)
        {
        }
    }
}