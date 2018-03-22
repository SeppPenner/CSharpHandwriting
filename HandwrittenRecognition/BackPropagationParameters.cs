namespace HandwrittenRecogniration
{
    public struct BackPropagationParameters
    {
        public uint MAfterEvery;
        public double MEtaDecay;
        public double MInitialEta;
        public double MMinimumEta;

        // ReSharper disable once NotAccessedField.Global
        public string MStrInitialEtaMessage;

        // ReSharper disable once NotAccessedField.Global
        public string MStrStartingPatternNum;

        public uint MStartingPattern;
        public uint McNumThreads;
        public bool MbDistortPatterns;
        public double MEstimatedCurrentMse;
    }
}