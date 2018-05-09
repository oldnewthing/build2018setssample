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
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Books
{
    public class BookViewModel
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public Uri ImageUri { get; set; }
    }

    public partial class App : Application
    {
        public static readonly string ProtocolScheme = "ms-build2018-sample-books";

        public static List<BookViewModel> Books = new List<BookViewModel>()
        {
            // public domain: https://www.flickr.com/photos/158283218@N03/38610164014/
            new BookViewModel
            {
                Id = "spaceneedle",
                Title = "Space Needle",
                ImageUri = new Uri("https://farm5.staticflickr.com/4732/38610164014_dc173b41e5_z.jpg"),
            },

            // public domain: https://www.flickr.com/photos/81547818@N07/35600655871/
            new BookViewModel
            {
                Id = "fremonttroll",
                Title = "Fremont Troll",
                ImageUri = new Uri("https://farm5.staticflickr.com/4087/35600655871_629183cddd_z.jpg"),
            },

            // public domain: https://www.flickr.com/photos/volvob12b/12046810643/
            new BookViewModel
            {
                Id = "stanleypark",
                Title = "Stanley Park",
                ImageUri = new Uri("https://farm4.staticflickr.com/3686/12046810643_83bb3b5752_z.jpg"),
            },

            // public domain: https://www.flickr.com/photos/roland/100896/
            new BookViewModel
            {
                Id = "canadaplace",
                Title = "Canada Place",
                ImageUri = new Uri("https://farm1.staticflickr.com/1/100896_d63e3e8038_z.jpg"),
            },
        };

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var args = e.Args;

            Window window = null;
            if (args.Length == 1 && Uri.TryCreate(args[0], UriKind.Absolute, out var uri) && uri.Scheme == ProtocolScheme)
            {
                var pageName = uri.AbsolutePath;
                if (pageName == "book")
                {
                    window = new BookViewer(uri.Query);
                }
            }

            if (window == null)
            {
                window = new MainWindow();
            }

            window.Show();
        }
    }

}
