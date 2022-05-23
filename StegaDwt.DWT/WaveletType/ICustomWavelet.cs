using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StegaDwt.DWT.WaveletType
{
    internal interface ICustomWavelet
    {
        float[] LowPassDecimationFilter { get; }

        float[] HighPassDecimationFilter { get; }
    }
}
