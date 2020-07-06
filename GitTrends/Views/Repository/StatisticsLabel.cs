﻿using Xamarin.Forms;
using Xamarin.Forms.Markup;

namespace GitTrends
{
    class StatisticsLabel : Label
    {
        public const int StatiscsFontSize = 12;

        public StatisticsLabel(in string textColorThemeName)
        {
            FontSize = StatiscsFontSize;

            HorizontalOptions = LayoutOptions.FillAndExpand;

            HorizontalTextAlignment = TextAlignment.Start;
            VerticalTextAlignment = TextAlignment.End;

            LineBreakMode = LineBreakMode.TailTruncation;

            this.DynamicResource(TextColorProperty, textColorThemeName);
        }
    }
}
