using GitTrends.Mobile.Common;
using GitTrends.Mobile.Common.Constants;
using Xamarin.Forms;
using Xamarin.Forms.Markup;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;
using static Xamarin.Forms.Markup.Factories;
using vm = GitTrends.ReferringSitesViewModel;

namespace GitTrends
{
    partial class ReferringSitesPage
    {
        void Build() => Content = 
            RelativeLayout (
                ReferringSitesWithRefresh
                .Constrain () .X (0) .Y (titleRowHeight) 
                              .Width (p => p.Width) .Height (p => p.Height - titleRowHeight),

                iOS ? TitleShadow
                .Constrain () .X (0) .Y (0) 
                              .Width (p => p.Width) .Height (titleRowHeight) : null,

                iOS ? TitleText
                .Constrain () .X (10) .Y (0) : null,

                iOS ? CloseButton
                .Constrain () .X (p => p.Width - (closeButton?.GetWidth(p) ?? 0) - 10) .Y (0) 
                              .Width (p => closeButton?.GetWidth(p) ?? 0) : null,

                storeRatingRequest
                .Constrain () .X (0) .Y (p => p.Height - storeRatingRequest.GetHeight(p)) 
                              .Width (p => p.Width)
            );

        RefreshView ReferringSitesWithRefresh => new RefreshView {
            AutomationId = ReferringSitesPageAutomationIds.RefreshView,
            CommandParameter = (repository.OwnerLogin, repository.Name, repository.Url, refreshViewCancelltionTokenSource.Token),
            Content = ReferringSites
        }  .DynamicResource (RefreshView.RefreshColorProperty, nameof(BaseTheme.PullToRefreshColor))
           .Assign (out refreshView)
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