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
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Windows.ApplicationModel.UserActivities;
using Windows.UI.Shell;
using Windows.UI.ViewManagement;

using static Books.DwmInterop;

using Color = Windows.UI.Color;

namespace Books
{
    /// <summary>
    /// Interaction logic for BookViewer.xaml
    /// </summary>
    public partial class BookViewer : Window
    {
        string BookId;
        bool WantsCustomGroupingBehavior;
        IntPtr AssociatedWindow;
        double PreviousScrollProgress = 0.0;
        public BookViewModel Book { get; private set; }
        UserActivitySession Session;
        UserActivityRequestManager userActivityRequestManager = null;

        ScrollViewer FindScrollViewer(DependencyObject root)
        {
            while (!(root is ScrollViewer))
            {
                if (VisualTreeHelper.GetChildrenCount(root) == 0)
                {
                    return null;
                }
                root = VisualTreeHelper.GetChild(root, 0);
            }
            return (ScrollViewer)root;
        }

        public BookViewer(string queryString) : this(queryString, false, IntPtr.Zero)
        {
        }

        public BookViewer(string queryString, bool customGroupingBehavior, IntPtr associatedWindow)
        {
            var query = System.Web.HttpUtility.ParseQueryString(queryString);
            Book = App.Books.FirstOrDefault(book => book.Id == query["id"]);
            if (Book == null)
            {
                // Invalid book. Just show the first book in the library.
                Book = App.Books.First();
            }
            BookId = Book.Id;

            double.TryParse(query["pos"], NumberStyles.Float, CultureInfo.InvariantCulture, out PreviousScrollProgress);

            WantsCustomGroupingBehavior = customGroupingBehavior;
            AssociatedWindow = associatedWindow;
            InitializeComponent();
            this.DataContext = this;
        }

        private async void Window_SourceInitialized(object sender, EventArgs e)
        {
            Title = Book.Title;

            var scrollViewer = FindScrollViewer(ReaderElement);
            scrollViewer.ScrollToVerticalOffset(PreviousScrollProgress * scrollViewer.ScrollableHeight);

            var windowHandle = (new System.Windows.Interop.WindowInteropHelper(this)).Handle;

            // Set custom tab colors.
            ApplicationViewTitleBar titleBar = ComInterop.GetTitleBarForWindow(windowHandle);
            if (titleBar != null)
            {
                titleBar.BackgroundColor = Color.FromArgb(255, 54, 60, 116);
                titleBar.ForegroundColor = Color.FromArgb(255, 232, 211, 162);
            }

            // Apply grouping behavior.
            if (!WantsCustomGroupingBehavior)
            {
                // Allow caller-specified default grouping to happen.
            }
            else if (AssociatedWindow == IntPtr.Zero)
            {
                // Open in new group.
                int preference = DWMTGP_NEW_TAB_GROUP;
                DwmSetWindowAttribute(windowHandle, DWMWA_TAB_GROUPING_PREFERENCE, ref preference, Marshal.SizeOf<int>());
            }
            else
            {
                // Join an existing group.
                int preference = DWMTGP_TAB_WITH_ASSOCIATED_WINDOW;
                DwmSetWindowAttribute(windowHandle, DWMWA_TAB_GROUPING_PREFERENCE, ref preference, Marshal.SizeOf<int>());

                DwmSetWindowAttribute(windowHandle, DWMWA_ASSOCIATED_WINDOW, ref AssociatedWindow, IntPtr.Size);
            }

            // Generate a UserActivity that says that
            // the user is reading this book.
            var channel = UserActivityChannel.GetDefault();
            var activity = await channel.GetOrCreateUserActivityAsync(BookId);
            activity.ActivationUri = new Uri($"{App.ProtocolScheme}:{BookId}");
            activity.VisualElements.DisplayText = Book.Title;

            var card = new AdaptiveCard();
            card.BackgroundImage = Book.ImageUri;
            card.Body.Add(new AdaptiveTextBlock(Book.Title) { Size = AdaptiveTextSize.Large, Weight = AdaptiveTextWeight.Bolder });
            activity.VisualElements.Content = AdaptiveCardBuilder.CreateAdaptiveCardFromJson(card.ToJson());

            await activity.SaveAsync();
            Session = activity.CreateSessionForWindow(windowHandle);

            // Listen for user activity requests.
            userActivityRequestManager = ComInterop.GetUserActivityRequestManagerForWindow(windowHandle);
            if (userActivityRequestManager != null)
            {
                userActivityRequestManager.UserActivityRequested += OnUserActivityRequested;
            }
        }

        private void Window_Closed(object sender, object e)
        {
            if (userActivityRequestManager != null)
            {
                userActivityRequestManager.UserActivityRequested -= OnUserActivityRequested;
            }
            Session?.Dispose();
        }

        void OnUserActivityRequested(UserActivityRequestManager sender, UserActivityRequestedEventArgs e)
        {
            using (e.GetDeferral())
            {
                // Generate a user activity that remembers the scroll position.
                var scrollViewer = FindScrollViewer(ReaderElement);
                var scrollProgress = scrollViewer.ScrollableHeight > 0 ? scrollViewer.ContentVerticalOffset / scrollViewer.ScrollableHeight : 0.0;

                // Create user activity session for this window.
                var activityId = FormattableString.Invariant($"book?id={BookId}&pos={scrollProgress}");
                var activity = new UserActivity(activityId);
                activity.ActivationUri = new Uri($"{App.ProtocolScheme}:{activityId}");
                activity.VisualElements.DisplayText = Book.Title;
                activity.VisualElements.Description = $"{(int)(scrollProgress * 100)}% complete";

                var card = new AdaptiveCard();
                card.BackgroundImage = Book.ImageUri;
                card.Body.Add(new AdaptiveTextBlock(Book.Title) { Size = AdaptiveTextSize.Large, Weight = AdaptiveTextWeight.Bolder });
                activity.VisualElements.Content = AdaptiveCardBuilder.CreateAdaptiveCardFromJson(card.ToJson());

                e.Request.SetUserActivity(activity);
            }
        }
    }
}
