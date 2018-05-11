<!---
  samplefwlink: https://aka.ms/build2018setssample
--->

# Windows Sets sample from Build 2018

This repo contains sample programs demonstrated at Build 2018.

These samples include code which adapts the program's behavior
to Windows 10 versions 1709 ("Fall Creators Update"),
1803 ("April 2018 Update"), and the 17666 Windows Insider build.

The samples require [Visual Studio 15.7](https://aka.ms/build2018-vsideannounce)
to build and run.

# Solutions in this repo

* UWP\Trips\cs\Trips.sln: Solution file for a UWP C# application.
    See the [README](UWP/Trips/cs/README.md) for more information.
* Win32\Books\Books.sln: Solution file for a Win32 C# application.
    See the [README](Win32/Books/README.md) for more information.

# Other resources

* [AdaptiveCards.io](adaptivecards.io)
* [AdaptiveCards NuGet package](https://www.nuget.org/packages/AdaptiveCards)
* [Guidelines for tile and asset icons](https://docs.microsoft.com/en-us/windows/uwp/design/shell/tiles-and-notifications/app-assets):
    If you follow the `altform-unplated` guidance, the icon for your UWP app
    will appear in the Taskbar and in the Tab without a solid backplate.
* [Create a multi-instance Universal Windows App](https://docs.microsoft.com/en-us/windows/uwp/launch-resume/multi-instance-uwp).


# Code of Conduct

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information, see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/)
or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.
