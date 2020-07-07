using System.Collections;
using Xamarin.Forms;

namespace GitTrends
{
    static class MarkupExtensions
    {
        public static GridLength StarGridLength(double value) => new GridLength(value, GridUnitType.Star);
        public static GridLength StarGridLength(int value) => StarGridLength((double)value);

        public static GridLength AbsoluteGridLength(double value) => new GridLength(value, GridUnitType.Absolute);
        public static GridLength AbsoluteGridLength(int value) => AbsoluteGridLength((double)value);

        public static double GetWidth(this View view, in RelativeLayout parent) => view.Measure(parent.Width, parent.Height).Request.Width;
        public static double GetHeight(this View view, in RelativeLayout parent) => view.Measure(parent.Width, parent.Height).Request.Height;

        public static bool IsNullOrEmpty(this IEnumerable? enumerable) => !enumerable?.GetEnumerator().MoveNext() ?? true;

        public static double Width(this RelativeLayout parent, View? child) => child?.GetWidth(parent) ?? 0;
        public static double Height(this RelativeLayout parent, View? child) => child?.GetHeight(parent) ?? 0;
    }
}
