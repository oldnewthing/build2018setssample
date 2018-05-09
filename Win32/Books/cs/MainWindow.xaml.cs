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
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Windows.ApplicationModel.UserActivities;
using Windows.UI;
using Windows.UI.Shell;
using Windows.UI.ViewManagement;

using static Books.DwmInterop;

namespace Books
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        IntPtr windowHandle = IntPtr.Zero;
        UserActivitySession session;
        public List<BookViewModel> Books => App.Books;

        private async void Window_SourceInitialized(object sender, EventArgs e)
        {
            windowHandle = (new System.Windows.Interop.WindowInteropHelper(this)).Handle;

            // Title bar colors.
            ApplicationViewTitleBar titleBar = ComInterop.GetTitleBarForWindow(windowHandle);
            if (titleBar != null)
            {
                titleBar.BackgroundColor = Color.FromArgb(255, 54, 60, 116);
                titleBar.ForegroundColor = Color.FromArgb(255, 232, 211, 162);
            }

            // Generate a UserActivity that says that
            // the user is looking at the library.
            var channel = UserActivityChannel.GetDefault();
            string activityId = "home";
            var activity = await channel.GetOrCreateUserActivityAsync(activityId);

            // Describe the activity.
            activity.ActivationUri = new Uri($"{App.ProtocolScheme}:{activityId}");
            activity.VisualElements.DisplayText = "Library";
            activity.VisualElements.Description = "4 books";

            var card = new AdaptiveCard();
            // Photo by Mikhail Pavstyuk (https://unsplash.com/photos/EKy2OTRPXdw) free for commercial use.
            card.BackgroundImage = new Uri("https://images.unsplash.com/photo-1423592707957-3b212afa6733");
            card.Body.Add(new AdaptiveTextBlock("Library") { Size = AdaptiveTextSize.Large, Weight = AdaptiveTextWeight.Bolder });
            card.Body.Add(new AdaptiveTextBlock("4 books"));
            activity.VisualElements.Content = AdaptiveCardBuilder.CreateAdaptiveCardFromJson(card.ToJson());

            // Save it to the channel.
            await activity.SaveAsync();

            // Start a session.
            session = activity.CreateSessionForWindow(windowHandle);
        }

        private void Window_Closed(object sender, object e)
        {
            session?.Dispose();
        }

        public static RoutedCommand OpenInNewTab = new RoutedCommand();

        private void OpenViewer(object sender, IntPtr associatedWindow)
        {
            var source = (ListView)sender;
            var item = (BookViewModel)source.SelectedItem;
            if (item != null)
            {
                var viewer = new BookViewer($"id={item.Id}", true, associatedWindow);
                viewer.Show();
            }
        }

        private void OpenInNewTab_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            OpenViewer(sender, windowHandle);
        }

        public static RoutedCommand OpenInNewWindow = new RoutedCommand();

        private void OpenInNewWindow_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            OpenViewer(sender, IntPtr.Zero);
        }

        void Library_OnContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            var source = (FrameworkElement)sender;
            if (DwmGetUnmetTabRequirements(windowHandle, out var unmetRequirements) == 0 &&
                unmetRequirements == DWM_TAB_WINDOW_REQUIREMENTS.DWMTWR_NONE)
            {
                source.ContextMenu = (ContextMenu)FindResource("SetsContextMenu");
            }
            else
            {
                source.ContextMenu = (ContextMenu)FindResource("NoSetsContextMenu");
            }
        }

        void OpenNotepadInNewTab(object sender, RoutedEventArgs e)
        {
            DoShellExecute("notepad.exe", DWMTGP_TAB_WITH_ASSOCIATED_WINDOW);
        }

        void OpenNotepadInNewWindow(object sender, RoutedEventArgs e)
        {
            DoShellExecute("notepad.exe", DWMTGP_NEW_TAB_GROUP);
        }

        class GroupingPreferenceSite : IServiceProvider, ILaunchUIContextProvider
        {
            public int GroupingPreference { get; set; }
            public IntPtr AssociatedWindow { get; set; }

            public int QueryService(Guid serviceId, Guid riid, out IntPtr result)
            {
                if (serviceId == typeof(ILaunchUIContextProvider).GUID)
                {
                    using (var thisComPtr = new ComPtr(this))
                    {
                        return Marshal.QueryInterface(thisComPtr.IntPtr, ref riid, out result);
                    }
                }
                result = IntPtr.Zero;
                return unchecked((int)0x80004005); // E_FAIL
            }

            public void UpdateContext(ILaunchUIContext context)
            {
                context.SetTabGroupingPreference(GroupingPreference);
                context.SetAssociatedWindow(AssociatedWindow);
            }
        }

        void DoShellExecute(string file, int groupingPreference)
        {
            var site = new GroupingPreferenceSite()
            {
                GroupingPreference = groupingPreference,
                AssociatedWindow = windowHandle
            };

            using (var siteComPtr = new ComPtr(site))
            {
                var info = new ShellInterop.SHELLEXECUTEINFO();
                info.StructureSize = Marshal.SizeOf<ShellInterop.SHELLEXECUTEINFO>();
                info.WindowHandle = windowHandle;
                info.ShowCommand = ShellInterop.SW_SHOWNORMAL;
                info.File = file;
                info.Mask = ShellInterop.SEE_MASK_FLAG_HINST_IS_SITE;
                info.Site = siteComPtr.IntPtr;

                ShellInterop.ShellExecuteEx(ref info);
            }
        }
    }
}
