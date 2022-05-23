namespace StegaDwt.DWT
{
    /// <summary>
    /// Contains the results of signal transformation and encoding
    /// </summary>
    public struct DwtTransformInfo
    {
        public int DecompLvl;

        public float[] ResultSamples;

        /// <summary>
        /// Signal detail at the specified level
        /// </summary>
        public float[] Details;

        /// <summary>
        /// Signal approximation at the specified level
        /// </summary>
        public float[] Approx;
    }
}
