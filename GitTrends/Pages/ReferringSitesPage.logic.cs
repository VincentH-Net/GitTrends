using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GitTrends.Mobile.Common;
using GitTrends.Mobile.Common.Constants;
using GitTrends.Shared;
using Xamarin.Essentials.Interfaces;
using Xamarin.Forms;

namespace GitTrends
{
    partial class ReferringSitesPage : BaseContentPage<ReferringSitesViewModel>
    {
        readonly StoreRatingRequestView storeRatingRequest = new StoreRatingRequestView();
        readonly CancellationTokenSource refreshViewCancelltionTokenSource = new CancellationTokenSource();

        readonly Repository repository;
        readonly ThemeService themeService;
        readonly ReviewService reviewService;
        readonly DeepLinkingService deepLinkingService;

        const int titleTopMargin = 10;
        readonly bool iOS = Device.RuntimePlatform is Device.iOS;
        readonly int titleRowHeight = Device.RuntimePlatform is Device.iOS ? 50 : 0;
        RefreshView? refreshView;
        Button? closeButton;

        public ReferringSitesPage(DeepLinkingService deepLinkingService,
                                  ReferringSitesViewModel referringSitesViewModel,
                                  Repository repository,
                                  IAnalyticsService analyticsService,
                                  ThemeService themeService,
                                  ReviewService reviewService,
                                  IMainThread mainThread) : base(referringSitesViewModel, analyticsService, mainThread)
        {
            Title = PageTitles.ReferringSitesPage;

            this.repository = repository;
            this.themeService = themeService;
            this.reviewService = reviewService;
            this.deepLinkingService = deepLinkingService;

            ViewModel.PullToRefreshFailed += HandlePullToRefreshFailed;
            reviewService.ReviewCompleted += HandleReviewCompleted;

            Build(); 
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (refreshView?.Content is CollectionView collectionView
                && collectionView.ItemsSource.IsNullOrEmpty())
            {
                refreshView.IsRefreshing = true;
                reviewService.TryRequestReviewPrompt();
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            refreshViewCancelltionTokenSource.Cancel();
            storeRatingRequest.IsVisible = false;
        }

        static bool IsLightTheme(PreferredTheme preferredTheme) => preferredTheme is PreferredTheme.Light || preferredTheme is PreferredTheme.Default && Xamarin.Forms.Application.Current.RequestedTheme is OSAppTheme.Light;

        async void HandleCollectionViewSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var collectionView = (CollectionView)sender;
            collectionView.SelectedItem = null;

            if (e?.CurrentSelection.FirstOrDefault() is ReferringSiteModel referingSite
                && referingSite.IsReferrerUriValid
                && referingSite.ReferrerUri != null)
            {
                AnalyticsService.Track("Referring Site Tapped", new Dictionary<string, string>
                {
                    { nameof(ReferringSiteModel) + nameof(ReferringSiteModel.Referrer), referingSite.Referrer },
                    { nameof(ReferringSiteModel) + nameof(ReferringSiteModel.ReferrerUri), referingSite.ReferrerUri.ToString() }
                });

                await deepLinkingService.OpenBrowser(referingSite.ReferrerUri);
            }
        }

        void HandlePullToRefreshFailed(object sender, PullToRefreshFailedEventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                if (Xamarin.Forms.Application.Current.MainPage.Navigation.ModalStack.LastOrDefault() is ReferringSitesPage
                    || Xamarin.Forms.Application.Current.MainPage.Navigation.NavigationStack.Last() is ReferringSitesPage)
                {
                    if (e.Accept is null)
                    {
                        await DisplayAlert(e.Title, e.Message, e.Cancel);
                    }
                    else
                    {
                        var isAccepted = await DisplayAlert(e.Title, e.Message, e.Accept, e.Cancel);
                        if (isAccepted)
                            await deepLinkingService.OpenBrowser(GitHubConstants.GitHubRateLimitingDocs);
                    }
                }
            });
        }

        void HandleReviewCompleted(object sender, ReviewRequest e) => MainThread.BeginInvokeOnMainThread(async () =>
        {
            const int animationDuration = 300;

            await Task.WhenAll(storeRatingRequest.TranslateTo(0, storeRatingRequest.Height, animationDuration),
                               storeRatingRequest.ScaleTo(0, animationDuration));

            storeRatingRequest.IsVisible = false;
            storeRatingRequest.Scale = 1;
            storeRatingRequest.TranslationY = 0;
        });

        async void HandleCloseButtonClicked(object sender, EventArgs e) => await Navigation.PopModalAsync();
    }
}