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
using System.Collections.Generic;
using Windows.ApplicationModel.UserActivities;
using Windows.UI.Shell;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

namespace Trips
{
    public sealed partial class MainPage : Page
    {
        public static Frame RootFrame => Window.Current.Content as Frame;

        string defaultId = null;
        UserActivitySession activitySession = null;

        public MainPage()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            // Remember which trip we came from, so we can perform a connected animation.
            defaultId = e.Parameter as string;

            // Clear any trip title.
            ApplicationView.GetForCurrentView().Title = String.Empty;

            // Generate a UserActivity that says that
            // the user is looking at the trips overview.
            var channel = UserActivityChannel.GetDefault();
            string activityId = "home";
            var activity = await channel.GetOrCreateUserActivityAsync(activityId);

            // Describe the activity.
            activity.ActivationUri = new Uri($"{App.ProtocolScheme}:{activityId}");
            activity.VisualElements.DisplayText = "Trips Overview";

            var card = new AdaptiveCard();
            // Source: https://www.publicdomainpictures.net/en/view-image.php?image=248425
            card.BackgroundImageString = "https://www.publicdomainpictures.net/pictures/250000/velka/passport-a-map-and-money.jpg";
            card.Body.Add(new AdaptiveTextBlock("Trips Overview") { Size = AdaptiveTextSize.Large, Weight = AdaptiveTextWeight.Bolder });
            card.Body.Add(new AdaptiveTextBlock("2 trips"));
            activity.VisualElements.Content = AdaptiveCardBuilder.CreateAdaptiveCardFromJson(card.ToJson());

            // Save it to the channel.
            await activity.SaveAsync();

            // Start a session.
            activitySession = activity.CreateSession();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            activitySession?.Dispose();
        }

        async void TripsLoaded(object sender, RoutedEventArgs e)
        {
            var list = (ListViewBase)sender;
            var item = TripData.FromId(defaultId);
            if (item != null)
            {
                // Perform a connected animation to make the page transition smoother.
                list.ScrollIntoView(item);
                var animation = ConnectedAnimationService.GetForCurrentView().GetAnimation("drillout");
                if (animation != null)
                {
                    await list.TryStartConnectedAnimationAsync(animation, item, "MainTile");
                }
            }
        }

        public List<TripViewModel> Trips => TripData.AllTrips;
        public List<TripViewModel> FeaturedDestinations => TripData.FeaturedDestinations;

        void TripItemClicked(object sender, ItemClickEventArgs e)
        {
            var item = e.ClickedItem as TripViewModel;
            if (item?.Id == null)
            {
                return;
            }
            var list = (ListViewBase)sender;
            list.PrepareConnectedAnimation("drillin", e.ClickedItem, "MainTile");
            RootFrame.Navigate(typeof(TripPage), $"id={item.Id}", new SuppressNavigationTransitionInfo());
        }
    }

    public class TripTemplateSelector : DataTemplateSelector
    {
        public DataTemplate TripTemplate { get; set; }
        public DataTemplate NewTripTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            var trip = (TripViewModel)item;
            return trip.IsNewTrip ? NewTripTemplate : TripTemplate;
        }
    }
}
