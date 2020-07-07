using GitTrends.Mobile.Common;
using GitTrends.Mobile.Common.Constants;
using Xamarin.Forms;
using Xamarin.Forms.Markup;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;
using static Xamarin.Forms.Constraint;
using static Xamarin.Forms.Markup.Factories;
using vm = GitTrends.ReferringSitesViewModel;

namespace GitTrends
{
    partial class ReferringSitesPage
    {
        void Build() => Content = 
            RelativeLayout (
                ReferringSitesWithRefresh .Constrain (
                    Constant (0),
                    Constant (titleRowHeight),
                    RelativeToParent (parent => parent.Width),
                    RelativeToParent (parent => parent.Height - titleRowHeight)),

                iOS ? TitleShadow .Constrain (
                    Constant (0),
                    Constant (0),
                    RelativeToParent (parent => parent.Width),
                    Constant (titleRowHeight))
                    : null,

                iOS ? TitleText .Constrain (
                    Constant (10),
                    Constant (0))
                    : null,

                iOS ? CloseButton .Constrain (
                    RelativeToParent (parent => parent.Width - (closeButton?.GetWidth(parent) ?? 0) - 10),
                    Constant (0),
                    RelativeToParent (parent => closeButton?.GetWidth(parent) ?? 0))
                    : null,

                storeRatingRequest .Constrain (
                    Constant (0),
                    RelativeToParent (parent => parent.Height - storeRatingRequest.GetHeight(parent)),
                    RelativeToParent (parent => parent.Width))
            );

        RefreshView ReferringSitesWithRefresh => new RefreshView {
            AutomationId = ReferringSitesPageAutomationIds.RefreshView,
            CommandParameter = (repository.OwnerLogin, repository.Name, repository.Url, refreshViewCancelltionTokenSource.Token),
            Content = ReferringSites
        }  .DynamicResource (RefreshView.RefreshColorProperty, nameof(BaseTheme.PullToRefreshColor))
           .Assign (out _refreshView)
           .Bind (RefreshView.CommandProperty, nameof(vm.RefreshCommand))
           .Bind (RefreshView.IsRefreshingProperty, nameof(vm.IsRefreshing));

        CollectionView ReferringSites => new CollectionView {
            AutomationId = ReferringSitesPageAutomationIds.CollectionView,
            BackgroundColor = Color.Transparent,
            ItemTemplate = new ReferringSitesDataTemplate(),
            SelectionMode = SelectionMode.Single,
            ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Vertical),

            //Set iOS Header to `new BoxView { HeightRequest = titleRowHeight + titleTopMargin }` following this bug fix: https://github.com/xamarin/Xamarin.Forms/issues/9879
            Header = iOS ? null : new BoxView { HeightRequest = ReferringSitesDataTemplate.BottomPadding },
            Footer = iOS ? null : new BoxView { HeightRequest = ReferringSitesDataTemplate.TopPadding },
            EmptyView = new EmptyDataView ("EmptyReferringSitesList", ReferringSitesPageAutomationIds.EmptyDataView)
                .Bind (IsVisibleProperty, nameof(vm.IsEmptyDataViewEnabled))
                .Bind (EmptyDataView.TitleProperty, nameof(vm.EmptyDataViewTitle))
                .Bind (EmptyDataView.DescriptionProperty, nameof(vm.EmptyDataViewDescription))
        }  .Invoke (collectionView => collectionView.SelectionChanged += HandleCollectionViewSelectionChanged)
           .Bind (nameof(vm.MobileReferringSitesList));

        BoxView TitleShadow => new BoxView { }
            .DynamicResource (BackgroundColorProperty, nameof(BaseTheme.CardSurfaceColor))
            .Invoke (titleShadow => { if (IsLightTheme(themeService.Preference)) titleShadow.On<iOS>()
                .SetIsShadowEnabled (true)
                .SetShadowColor (Color.Gray)
                .SetShadowOffset (new Size(0, 1))
                .SetShadowOpacity (0.5)
                .SetShadowRadius (4); });

        Label TitleText => new Label { 
            Text = PageTitles.ReferringSitesPage
        }  .Font (family: FontFamilyConstants.RobotoMedium, size: 30)
           .DynamicResource (Label.TextColorProperty, nameof(BaseTheme.TextColor))
           .Center () .Margins (top: titleTopMargin) .TextCenterVertical ();

        Button CloseButton => new Button {
            Text = ReferringSitesPageConstants.CloseButtonText,
            AutomationId = ReferringSitesPageAutomationIds.CloseButton
        }  .Font (family: FontFamilyConstants.RobotoRegular)
           .DynamicResources (
               (Button.TextColorProperty, nameof(BaseTheme.CloseButtonTextColor)), 
               (BackgroundColorProperty , nameof(BaseTheme.CloseButtonBackgroundColor)))
           .Assign (out closeButton)
           .Invoke (closeButton => closeButton.Clicked += HandleCloseButtonClicked)
           .End () .CenterVertical () .Margins (top: titleTopMargin) .Height (titleRowHeight * 3 / 5) .Padding (5, 0);
    }
}