namespace NeuralNetworkLibrary.DataFiles
{
    /// <summary>
    ///     Image Pattern Class
    /// </summary>
    public class ImagePattern
    {
        public byte NLabel;
        public byte[] PPattern = new byte[MyDefinitions.GcImageSize * MyDefinitions.GcImageSize];
    }
}