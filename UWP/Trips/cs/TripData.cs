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
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace Trips
{
    public class TripViewModel
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public ImageSource ImageSource { get; set; }
        public Uri ImageSourceUri { get; set; }
        public string Arriving { get; set; }
        public Uri ArrivingUri { get; set; }
        public string Departing { get; set; }
        public Uri DepartingUri { get; set; }
        public List<string> TodDoList { get; set; }
        public bool IsNewTrip { get; set; }
    }

    public class TripData
    {
        public static List<TripViewModel> AllTrips { get; } = new List<TripViewModel>()
        {
            new TripViewModel
            {
                Id = "1",
                Title = "Seattle",
                Description = "Microsoft Build 2018",
                // Source: https://www.publicdomainpictures.net/en/view-image.php?image=209131
                ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/Seattle-Skyline.jpg")),
                ImageSourceUri = new Uri("https://www.publicdomainpictures.net/pictures/210000/velka/seattle-skyline-14904764209Wo.jpg"),
                Arriving = "Alaska 11, arriving 8:28pm",
                ArrivingUri = new Uri("https://flightaware.com/live/flight/ASA11"),
                Departing = "Alaska 8, departing 7:38am",
                DepartingUri = new Uri("https://flightaware.com/live/flight/ASA8"),
                TodDoList = new List<string>()
                {
                    "Pack suitcase",
                    "Attend keynote",
                    "Go to the Sets talk",
                    "Get Raymond's autograph",
                    "Visit the Fremont Troll",
                },
            },
            new TripViewModel {
                Id = "2",
                Title = "London", Description = "Summer trip",
                // Source: https://www.publicdomainpictures.net/en/view-image.php?image=221094
                ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/London-Cityscape.jpg")),
                ImageSourceUri = new Uri("https://www.publicdomainpictures.net/pictures/230000/velka/london-cityscape.jpg"),
                Arriving = "Virgin Atlantic 4, 6:25am",
                ArrivingUri = new Uri("https://flightaware.com/live/flight/VIR4"),
                Departing = "Virgin Atlantic 137, 11:50am",
                DepartingUri = new Uri("https://flightaware.com/live/flight/VIR137"),
                TodDoList = new List<string>()
                {
                    "Tower of London",
                    "House of Parliament",
                    "Greenwich Observatory",
                    "London Eye",
                    "Buckingham Palace",
                    "Victoria and Albert Museum",
                },
            },
            new TripViewModel
            {
                IsNewTrip = true,
            }
        };

        public static List<TripViewModel> FeaturedDestinations { get; } = new List<TripViewModel>
        {
            new TripViewModel
            {
                Title = "Vancouver",
                //Source: https://www.publicdomainpictures.net/en/view-image.php?image=215874
                ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/Vancouver-Waterfront.jpg")),
            },
            new TripViewModel
            {
                Title = "Thailand",
                // Source: https://publicdomainpictures.net/en/view-image.php?image=101876
                ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/Thailand-BlueLagoon.jpg")),
            },
        };

        public static TripViewModel FromId(string id)
        {
            return AllTrips.FirstOrDefault(t => t.Id == id);
        }
    }
}