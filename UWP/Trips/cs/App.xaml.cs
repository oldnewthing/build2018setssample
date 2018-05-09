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

using System;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Trips
{
    sealed partial class App : Application
    {
        public static readonly string ProtocolScheme = "ms-build2018-sample-trips";

        public App()
        {
            this.InitializeComponent();
        }

        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {

#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                this.DebugSettings.EnableFrameRateCounter = false;
            }
#endif

            Frame rootFrame = CreateRootFrame();

            if (rootFrame.Content == null)
            {
                rootFrame.Navigate(typeof(MainPage), e.Arguments);
            }

            Window.Current.Activate();
        }

        protected override void OnActivated(IActivatedEventArgs args)
        {
            if (args.Kind == ActivationKind.Protocol)
            {
                var protocolArgs = (ProtocolActivatedEventArgs)args;

                var rootFrame = CreateRootFrame();

                // Figure out which page to load into the frame.
                Type pageType = null;
                var page = protocolArgs.Uri.AbsolutePath;
                switch (page)
                {
                    default:
                    case "home":
                        pageType = typeof(MainPage);
                        break;

                    case "trip":
                        pageType = typeof(TripPage);
                        break;
                }

                rootFrame.Navigate(pageType, protocolArgs.Uri.Query);

                Window.Current.Activate();
            }
        }

        Frame CreateRootFrame()
        {
            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content.
            if (rootFrame == null)
            {
                // Update title bar branding colors.
                // These colors are also used for the tab.
                var titleBar = ApplicationView.GetForCurrentView().TitleBar;
                var brandingBackground = Color.FromArgb(0xFF, 0, 130, 153);
                var brandingBackgroundHover = Color.FromArgb(255, 51, 224, 255);
                var brandingBackgroundPressed = Color.FromArgb(255, 0, 65, 77);
                var brandingForeground = Colors.White;
                titleBar.ForegroundColor = brandingForeground;
                titleBar.BackgroundColor = brandingBackground;
                titleBar.ButtonForegroundColor = brandingForeground;
                titleBar.ButtonBackgroundColor = brandingBackground;
                titleBar.ButtonHoverForegroundColor = brandingForeground;
                titleBar.ButtonHoverBackgroundColor = brandingBackgroundHover;
                titleBar.ButtonPressedForegroundColor = brandingForeground;
                titleBar.ButtonPressedBackgroundColor = brandingBackgroundPressed;

                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                // Set the default language
                rootFrame.Language = Windows.Globalization.ApplicationLanguages.Languages[0];

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }
            return rootFrame;
        }
    }
}
