﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using GitTrends.Shared;
using ImageCircle.Forms.Plugin.Abstractions;
using Sharpnado.MaterialFrame;
using Xamarin.Forms;
using Xamarin.Forms.Markup;
using static GitTrends.XamarinFormsService;
using static Xamarin.Forms.Markup.GridRowsColumns;

namespace GitTrends
{
    abstract class BaseRepositoryDataTemplate : DataTemplate
    {
        const int _statsColumnSize = 40;
        const double _statisticsRowHeight = StatisticsLabel.StatiscsFontSize + 4;
        const double _emojiColumnSize = _statisticsRowHeight;
        readonly static bool _isSmallScreen = ScreenWidth <= 360;
        readonly static double _circleImageHeight = _isSmallScreen ? 52 : 62;

        protected BaseRepositoryDataTemplate(IEnumerable<View> parentDataTemplateChildren) : base(() => new CardView(parentDataTemplateChildren))
        {

        }

        protected enum Row { Title, Description, DescriptionPadding, Separator, SeparatorPadding, Statistics }
        protected enum Column { Avatar, AvatarPadding, Trending, Emoji1, Statistic1, Emoji2, Statistic2, Emoji3, Statistic3 }

        public static int TopPadding { get; } = Device.RuntimePlatform is Device.Android ? 4 : 8;
        public static int BottomPadding { get; } = Device.RuntimePlatform is Device.Android ? 12 : 16;

        class CardView : Grid
        {
            public CardView(in IEnumerable<View> parentDataTemplateChildren)
            {
                RowSpacing = 0;
                RowDefinitions = Rows.Define(
                    (CardViewRow.TopPadding, AbsoluteGridLength(TopPadding)),
                    (CardViewRow.Card, StarGridLength(1)),
                    (CardViewRow.BottomPadding, AbsoluteGridLength(BottomPadding)));

                ColumnDefinitions = Columns.Define(
                    (CardViewColumn.LeftPadding, AbsoluteGridLength(16)),
                    (CardViewColumn.Card, StarGridLength(1)),
                    (CardViewColumn.RightPadding, AbsoluteGridLength(16)));

                Children.Add(new CardViewFrame(parentDataTemplateChildren).Row(CardViewRow.Card).Column(CardViewColumn.Card));

                SetDynamicResource(BackgroundColorProperty, nameof(BaseTheme.PageBackgroundColor));
            }

            enum CardViewRow { TopPadding, Card, BottomPadding }
            enum CardViewColumn { LeftPadding, Card, RightPadding }

            class CardViewFrame : MaterialFrame
            {
                public CardViewFrame(in IEnumerable<View> parentDataTemplateChildren)
                {
                    Padding = new Thickness(16, 16, 12, 8);
                    CornerRadius = 4;
                    HasShadow = false;
                    Elevation = 4;

                    Content = new ContentGrid(parentDataTemplateChildren);

                    SetDynamicResource(MaterialThemeProperty, nameof(BaseTheme.MaterialFrameTheme));
                }

                class ContentGrid : Grid
                {
                    public ContentGrid(in IEnumerable<View> parentDataTemplateChildren)
                    {
                        this.FillExpand();

                        RowDefinitions = Rows.Define(
                            (Row.Title, AbsoluteGridLength(25)),
                            (Row.Description, AbsoluteGridLength(40)),
                            (Row.DescriptionPadding, AbsoluteGridLength(4)),
                            (Row.Separator, AbsoluteGridLength(1)),
                            (Row.SeparatorPadding, AbsoluteGridLength(4)),
                            (Row.Statistics, AbsoluteGridLength(_statisticsRowHeight)));

                        ColumnDefinitions = Columns.Define(
                            (Column.Avatar, AbsoluteGridLength(_circleImageHeight)),
                            (Column.AvatarPadding, AbsoluteGridLength(_isSmallScreen ? 8 : 16)),
                            (Column.Trending, StarGridLength(1)),
                            (Column.Emoji1, AbsoluteGridLength(_emojiColumnSize)),
                            (Column.Statistic1, AbsoluteGridLength(_statsColumnSize)),
                            (Column.Emoji2, AbsoluteGridLength(_emojiColumnSize)),
                            (Column.Statistic2, AbsoluteGridLength(_statsColumnSize)),
                            (Column.Emoji3, AbsoluteGridLength(_emojiColumnSize)),
                            (Column.Statistic3, AbsoluteGridLength(_statsColumnSize)));

                        Children.Add(new AvatarImage()
                                        .Row(Row.Title).Column(Column.Avatar).RowSpan(2)
                                        .Bind(Image.SourceProperty, nameof(Repository.OwnerAvatarUrl)));

                        Children.Add(new NameLabel()
                                        .Row(Row.Title).Column(Column.Trending).ColumnSpan(7)
                                        .Bind(Label.TextProperty, nameof(Repository.Name)));

                        Children.Add(new DescriptionLabel()
                                        .Row(Row.Description).Column(Column.Trending).ColumnSpan(7)
                                        .Bind(Label.TextProperty, nameof(Repository.Description)));

                        Children.Add(new Separator()
                                        .Row(Row.Separator).Column(Column.Trending).ColumnSpan(7));

                        //On large screens, display TrendingImage in the same column as the repository name
                        Children.Add(new LargeScreenTrendingImage().Assign(out LargeScreenTrendingImage largeScreenTrendingImage)
                                        .Row(Row.SeparatorPadding).Column(Column.Trending).RowSpan(2));

                        //On smaller screens, display TrendingImage under the Avatar
                        Children.Add(new SmallScreenTrendingImage(largeScreenTrendingImage)
                                        .Row(Row.SeparatorPadding).Column(Column.Avatar).RowSpan(2).ColumnSpan(3));

                        foreach (var child in parentDataTemplateChildren)
                        {
                            Children.Add(child);
                        }
                    }

                    class AvatarImage : CircleImage
                    {
                        public AvatarImage()
                        {
                            this.Center();

                            HeightRequest = _circleImageHeight;
                            WidthRequest = _circleImageHeight;

                            BorderThickness = 1;

                            SetDynamicResource(BorderColorProperty, nameof(BaseTheme.SeparatorColor));
                            SetDynamicResource(ErrorPlaceholderProperty, nameof(BaseTheme.DefaultProfileImageSource));
                            SetDynamicResource(LoadingPlaceholderProperty, nameof(BaseTheme.DefaultProfileImageSource));
                        }
                    }

                    class NameLabel : PrimaryColorLabel
                    {
                        public NameLabel() : base(20)
                        {
                            LineBreakMode = LineBreakMode.TailTruncation;
                            HorizontalOptions = LayoutOptions.FillAndExpand;
                            FontFamily = FontFamilyConstants.RobotoBold;
                        }
                    }

                    class DescriptionLabel : PrimaryColorLabel
                    {
                        public DescriptionLabel() : base(14)
                        {
                            MaxLines = 2;
                            LineHeight = 1.16;
                            FontFamily = FontFamilyConstants.RobotoRegular;
                        }
                    }

                    abstract class PrimaryColorLabel : Label
                    {
                        protected PrimaryColorLabel(in double fontSize)
                        {
                            FontSize = fontSize;
                            LineBreakMode = LineBreakMode.TailTruncation;
                            HorizontalTextAlignment = TextAlignment.Start;
                            VerticalTextAlignment = TextAlignment.Start;

                            SetDynamicResource(TextColorProperty, nameof(BaseTheme.PrimaryTextColor));
                        }
                    }

                    class Separator : BoxView
                    {
                        public Separator() => SetDynamicResource(ColorProperty, nameof(BaseTheme.SeparatorColor));
                    }

                    class LargeScreenTrendingImage : TrendingImage
                    {
                        public LargeScreenTrendingImage()
                        {
                            SetBinding(IsVisibleProperty, new MultiBinding
                            {
                                Converter = new IsVisibleConverter(largeScreenTrendingImageWidth => largeScreenTrendingImageWidth >= SvgWidthRequest),
                                Bindings =
                                {
                                    new Binding(nameof(Repository.IsTrending)),
                                    new Binding(nameof(Width), source: this)
                                }
                            });
                        }
                    }

                    class SmallScreenTrendingImage : TrendingImage
                    {
                        public SmallScreenTrendingImage(LargeScreenTrendingImage largeScreenTrendingImage)
                        {
                            SetBinding(IsVisibleProperty, new MultiBinding
                            {
                                Converter = new IsVisibleConverter(largeScreenTrendingImageWidth => largeScreenTrendingImageWidth < SvgWidthRequest),
                                Bindings =
                                {
                                    new Binding(nameof(Repository.IsTrending)),
                                    new Binding(nameof(Width), source: largeScreenTrendingImage)
                                }
                            });
                        }
                    }

                    abstract class TrendingImage : StatisticsSvgImage
                    {
                        public const double SvgWidthRequest = 62;
                        public const double SvgHeightRequest = 16;

                        public TrendingImage() : base("trending_tag.svg", nameof(BaseTheme.CardTrendingStatsColor), SvgWidthRequest, SvgHeightRequest)
                        {
                            HorizontalOptions = LayoutOptions.Start;
                            VerticalOptions = LayoutOptions.End;

                        }

                        protected class IsVisibleConverter : IMultiValueConverter
                        {
                            readonly Func<double, bool> _isWidthValid;

                            public IsVisibleConverter(Func<double, bool> isWidthValid) => _isWidthValid = isWidthValid;

                            public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
                            {
                                if (values is null || !values.Any())
                                    return false;

                                if (values[0] is bool isTrending && isTrending is true
                                    && values[1] is double width)
                                {
                                    // When `Width is -1`, Xamarin.Forms hasn't inflated the View
                                    // Allow Xamarin.Forms to inflate the view, then validate its Width
                                    if (width is -1 || _isWidthValid(width))
                                        return true;

                                    return false;
                                }

                                return false;
                            }

                            public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
                            {
                                throw new NotImplementedException();
                            }
                        }
                    }
                }
            }
        }
    }
}
