using NeuralNetworkLibrary.ArchiveSerialization;

namespace NeuralNetworkLibrary.NNWeights
{
    // Weight class
    public sealed class NnWeight : IArchiveSerialization
    {
        public readonly string Label;
        public double DiagHessian;
        public double Value;

        // ReSharper disable once UnusedMember.Global
        public NnWeight()
        {
            Label = "";
            Value = 0.0;
            DiagHessian = 0.0;
        }

        public NnWeight(string str, double val = 0.0)
        {
            Label = str;
            Value = val;
            DiagHessian = 0.0;
        }

        public void Serialize(Archive ar)
        {
        }

        // ReSharper disable once UnusedMember.Local
        private void Initialize()
        {
        }
    }
}