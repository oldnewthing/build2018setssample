//*********************************************************
//
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
// IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
// PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//*********************************************************

using AdaptiveCards;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Windows.ApplicationModel.UserActivities;
using Windows.Foundation;
using Windows.Foundation.Metadata;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Shell;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

namespace Trips
{
    public sealed partial class TripPage : Page, INotifyPropertyChanged
    {
        public TripViewModel Trip { get; set; }
        public TripPage()
        {
            this.InitializeComponent();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private SystemNavigationManager systemNavigationManager;
        private UserActivitySession activitySession = null;
        private UserActivityRequestManager activityRequestManager = UserActivityRequestManager.GetForCurrentView();
        private int previousSelectedIndex = -1;

        static string GetDecoderField(WwwFormUrlDecoder decoder, string key)
        {
            try
            {
                return decoder.GetFirstValueByName(key);
            }
            catch (Exception)
            {
                return String.Empty;
            }
        }
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            // Handle the events from the Back button, but do not show it.
            systemNavigationManager = SystemNavigationManager.GetForCurrentView();
            systemNavigationManager.BackRequested += OnBackRequested;

            // Parse the URI query parameters. They tell us what to load.
            var decoder = new WwwFormUrlDecoder((string)e.Parameter);
            Trip = TripData.FromId(GetDecoderField(decoder, "id"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Trip"));

            if (Trip == null)
            {
                // Invalid trip.
                return;
            }

            if (!int.TryParse(GetDecoderField(decoder, "todo"), out previousSelectedIndex))
            {
                previousSelectedIndex = -1;
            }

            // If applicable, perform a connected animation to make the page transition smoother.
            var animationService = ConnectedAnimationService.GetForCurrentView();
            var animation = animationService.GetAnimation("drillin");
            if (animation != null)
            {
                animation.TryStart(HeroGrid);
            }

            // Update the title of the view to match the trip the user is looking at.
            ApplicationView.GetForCurrentView().Title = Trip.Title;

            // Generate a UserActivity that says that the user is looking at this trip.
            var channel = UserActivityChannel.GetDefault();
            string activityId = $"trip?id={Trip.Id}";
            var activity = await channel.GetOrCreateUserActivityAsync(activityId);

            // The system uses this URI to resume the activity.
            activity.ActivationUri = new Uri($"{App.ProtocolScheme}:{activityId}");

            // Describe the activity.
            activity.VisualElements.DisplayText = Trip.Title;
            activity.VisualElements.Description = Trip.Description;

            // Build the adaptive card JSON with the helper classes in the NuGet package.
            // You are welcome to generate your JSON using any library you like.
            var card = new AdaptiveCard();
            card.BackgroundImage = Trip.ImageSourceUri;
            card.Body.Add(new AdaptiveTextBlock(Trip.Title) { Size = AdaptiveTextSize.Large, Weight = AdaptiveTextWeight.Bolder });
            card.Body.Add(new AdaptiveTextBlock(Trip.Description));
            var adaptiveCardJson = card.ToJson();

            // Turn the JSON into an adaptive card and set it on the activity.
            activity.VisualElements.Content = AdaptiveCardBuilder.CreateAdaptiveCardFromJson(adaptiveCardJson);

            // Save it to the activity feed.
            await activity.SaveAsync();

            // Start a session. This tels the system know that the user is engaged in the activity right now.
            this.activitySession = activity.CreateSession();

            // Subscribe to the UserActivityRequested event if the system supports it.
            if (ApiInformation.IsEventPresent(typeof(UserActivityRequestManager).FullName, "UserActivityRequested"))
            {
                activityRequestManager = UserActivityRequestManager.GetForCurrentView();
                activityRequestManager.UserActivityRequested += OnUserActivityRequested;
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            // Disconnect from the UserActivityRequested event if applicable.
            if (activityRequestManager != null)
            {
                activityRequestManager.UserActivityRequested -= OnUserActivityRequested;
            }

            // Disposing the session tells the system that the user is no longer engaged in the activity.
            activitySession?.Dispose();

            systemNavigationManager.BackRequested -= OnBackRequested;
        }

        void OnUserActivityRequested(UserActivityRequestManager sender, UserActivityRequestedEventArgs e)
        {
            // The system raises the UserActivityRequested event to request that the
            // app generate an activity that describes what the user is doing right now.
            // This activity is used to restore the app as part of a Set.

            using (e.GetDeferral())
            {
                // Determine which trip and to-do item the user is currently working on.
                var description = Trip.Description;
                var index = ToDoListView.SelectedIndex;
                if (index >= 0)
                {
                    description = ToDoListView.SelectedItem.ToString();
                }

                // Generate a UserActivity that says that the user is looking at
                // a particular to-do item on this trip.
                string activityId = $"trip?id={Trip.Id}&todo={index}";
                var activity = new UserActivity(activityId);

                // The system uses this URI to resume the activity.
                activity.ActivationUri = new Uri($"{App.ProtocolScheme}:{activityId}");

                // Describe the activity.
                activity.VisualElements.DisplayText = Trip.Title;
                activity.VisualElements.Description = description;

                // Build the adaptive card JSON with the helper classes in the NuGet package.
                // You are welcome to generate your JSON using any library you like.
                var card = new AdaptiveCard();
                card.BackgroundImage = Trip.ImageSourceUri;
                card.Body.Add(new AdaptiveTextBlock(Trip.Title) { Size = AdaptiveTextSize.Large, Weight = AdaptiveTextWeight.Bolder });
                card.Body.Add(new AdaptiveTextBlock(description));
                var adaptiveCardJson = card.ToJson();

                // Turn the JSON into an adaptive card and set it on the activity.
                activity.VisualElements.Content = AdaptiveCardBuilder.CreateAdaptiveCardFromJson(adaptiveCardJson);

                // Respond to the request.
                e.Request.SetUserActivity(activity);
            }
        }

        void OnPageLoaded()
        {
            if (Trip == null)
            {
                // We don't have a valid trip. Go back to the home page.
                // (We may have been sent here by an old activity.)
                MainPage.RootFrame.Navigate(typeof(MainPage));
            }

            // Restore the previously selected item and bring it into view.
            if (previousSelectedIndex >= 0 && previousSelectedIndex < ToDoListView.Items.Count)
            {
                ToDoListView.SelectedIndex = previousSelectedIndex;
                ToDoListView.ScrollIntoView(ToDoListView.SelectedItem);
            }
        }

        void GoBack()
        {
            var animationService = ConnectedAnimationService.GetForCurrentView();
            animationService.PrepareToAnimate("drillout", HeroGrid);

            MainPage.RootFrame.Navigate(typeof(MainPage), Trip.Id, new SuppressNavigationTransitionInfo());
        }

        void OnBackRequested(object sender, BackRequestedEventArgs e)
        {
            GoBack();
            e.Handled = true;
        }

        async void LaunchFlightUri(Hyperlink sender, HyperlinkClickEventArgs e)
        {
            var parent = (FrameworkElement)VisualTreeHelper.GetParent(sender);
            var uri = (Uri)parent.Tag;
            if (uri != null)
            {
                var options = new LauncherOptions();
                // Set the grouping preference if running on a system that supports it.
                if (ApiInformation.IsPropertyPresent(typeof(LauncherOptions).FullName, "GroupingPreference"))
                {
                    options.GroupingPreference = ViewGrouping.WithSource;
                }
                await Launcher.LaunchUriAsync(uri, options);
            }
        }

        async void LaunchAd()
        {
            var uri = new Uri("https://microsoft.com/surface");
            var options = new LauncherOptions();
            // Set the grouping preference if running on a system that supports it.
            if (ApiInformation.IsPropertyPresent(typeof(LauncherOptions).FullName, "GroupingPreference"))
            {
                options.GroupingPreference = ViewGrouping.Separate;
            }
            await Launcher.LaunchUriAsync(uri, options);
        }
    }
}
