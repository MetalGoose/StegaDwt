namespace StegaDwt.DWT.WaveletType
{
    internal class Rbio55 : ICustomWavelet
    {
        public float[] LowPassDecimationFilter { get; }

        public float[] HighPassDecimationFilter { get; }

        public Rbio55()
        {
            LowPassDecimationFilter = new float[12]
            {
                0.0f,
                0.013456709459118716f,
                -0.002694966880111507f,
                -0.13670658466432914f,
                -0.09350469740093886f,
                0.47680326579848425f,
                0.8995061097486484f,
                0.47680326579848425f,
                -0.09350469740093886f,
                -0.13670658466432914f,
                -0.002694966880111507f,
                0.013456709459118716f,
            };
            HighPassDecimationFilter = new float[12]
            {
                0.0f,
                0.03968708834740544f,
                -0.007948108637240322f,
                -0.05446378846823691f,
                -0.34560528195603346f,
                0.7366601814282105f,
                -0.34560528195603346f,
                -0.05446378846823691f,
                -0.007948108637240322f,
                0.03968708834740544f,
                0.0f,
                0.0f
            };
        }
    }
}
