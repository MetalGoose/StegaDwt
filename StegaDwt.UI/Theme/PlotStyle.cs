using ScottPlot.Drawing;
using ScottPlot.Styles;
using System.Drawing;

namespace StegaDwt.UI.Theme
{
    internal class PlotStyle : IStyle
    {
        public virtual Color FigureBackgroundColor => Color.Transparent;

        public virtual Color DataBackgroundColor => Color.White;

        public virtual Color GridLineColor => ColorTranslator.FromHtml("#efefef");

        public virtual Color FrameColor => Color.White;

        public virtual Color TitleFontColor => Color.White;

        public virtual Color AxisLabelColor => Color.White;

        public virtual Color TickLabelColor => Color.White;

        public virtual Color TickMajorColor => ColorTranslator.FromHtml("#00e699");

        public virtual Color TickMinorColor => Color.White;

        public virtual string TitleFontName => InstalledFont.Default();

        public virtual string AxisLabelFontName => InstalledFont.Default();

        public virtual string TickLabelFontName => InstalledFont.Default();
    }
}
