using System;
using System.Threading;
using Autofac;
using BackgroundTasks;
using GitTrends.iOS;
using UIKit;
using Xamarin.Forms;

[assembly: Dependency(typeof(BackgroundFetchService_iOS))]
namespace GitTrends.iOS
{
    public class BackgroundFetchService_iOS : IBackgroundFetchService
    {
        //Disable this until BGTask SIGSEV is resolve: https://github.com/xamarin/xamarin-macios/issues/7456
#pragma warning disable CS0162 // Unreachable code detected
        public void Register()
        {
            const double twelveHoursInSeconds = 12 * 60 * 60;
            //Workaround for BGTask SIGSEV: https://github.com/xamarin/xamarin-macios/issues/7456
            UIApplication.SharedApplication.SetMinimumBackgroundFetchInterval(twelveHoursInSeconds);
            return;

            var isNotfyTrendingRepositoriesSuccessful = BGTaskScheduler.Shared.Register(BackgroundFetchService.NotifyTrendingRepositoriesIdentifier, null, task => NotifyTrendingRepositoriesBackgroundTask(task));
            var isCleanupDatabaseSuccessful = BGTaskScheduler.Shared.Register(BackgroundFetchService.CleanUpDatabaseIdentifier, null, task => CleanUpDatabaseBackgroundTask(task));

            using var scope = ContainerService.Container.BeginLifetimeScope();
            var analyticsService = scope.Resolve<AnalyticsService>();

            if (!isNotfyTrendingRepositoriesSuccessful)
                analyticsService.Track($"Register Notify Trending Repositories Failed");

            if (!isCleanupDatabaseSuccessful)
                analyticsService.Track($"Register Cleanup Database Failed");
        }

        //Disable this until BGTask SIGSEV is resolve: https://github.com/xamarin/xamarin-macios/issues/7456
        public void Scehdule()
        {
            return;

            var notifyTrendingRepositoriesRequest = new BGProcessingTaskRequest(BackgroundFetchService.NotifyTrendingRepositoriesIdentifier)
            {
                RequiresNetworkConnectivity = true,
                RequiresExternalPower = true
            };

            var cleanupDatabaseRequest = new BGProcessingTaskRequest(BackgroundFetchService.CleanUpDatabaseIdentifier);

            var isNotifyTrendingRepositoriesSuccessful = BGTaskScheduler.Shared.Submit(notifyTrendingRepositoriesRequest, out var notifyTrendingRepositoriesError);
            var isCleanupDatabaseSuccessful = BGTaskScheduler.Shared.Submit(cleanupDatabaseRequest, out var cleanUpDataBaseError);

            using var scope = ContainerService.Container.BeginLifetimeScope();
            var anayticsService = scope.Resolve<AnalyticsService>();

            if (!isNotifyTrendingRepositoriesSuccessful)
                anayticsService.Report(new ArgumentException(notifyTrendingRepositoriesError.LocalizedDescription));

            if (!isCleanupDatabaseSuccessful)
                anayticsService.Report(new ArgumentException(cleanUpDataBaseError.LocalizedDescription));
        }
#pragma warning restore CS0162 // Unreachable code detected

        async void NotifyTrendingRepositoriesBackgroundTask(BGTask task)
        {
            var backgroudTaskCancellationTokenSource = new CancellationTokenSource();
            task.ExpirationHandler = backgroudTaskCancellationTokenSource.Cancel;

            using var scope = ContainerService.Container.BeginLifetimeScope();
            var isSuccessful = await scope.Resolve<BackgroundFetchService>().NotifyTrendingRepositories(backgroudTaskCancellationTokenSource.Token).ConfigureAwait(false);

            task.SetTaskCompleted(isSuccessful);
        }

        async void CleanUpDatabaseBackgroundTask(BGTask task)
        {
            var backgroudTaskCancellationTokenSource = new CancellationTokenSource();
            task.ExpirationHandler = backgroudTaskCancellationTokenSource.Cancel;

            using var scope = ContainerService.Container.BeginLifetimeScope();

            try
            {
                await scope.Resolve<BackgroundFetchService>().CleanUpDatabase().ConfigureAwait(false);
                task.SetTaskCompleted(true);
            }
            catch (Exception e)
            {
                scope.Resolve<AnalyticsService>().Report(e);
                task.SetTaskCompleted(false);
            }
        }
    }
}
