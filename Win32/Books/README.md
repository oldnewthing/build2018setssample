# Books sample

Items of note:

* This application is packaged to simplify deployment. In particular,
    we use the `uap:Protocol` element in the `Package.appxmanifest` to
    do the work of registering a protocol handler upon install and
    unregistering it upon uninstall.

* Both the `MainWindow.xaml.cs and `BookViewer.xaml.cs` files 
    use a COM interop helper to obtain the `ApplicationViewTitleBar object.
    The colors set on this object are shown in the tab.

* Both the `MainWindow.xaml.cs` and `BookViewer.xaml.cs` create a `UserActivity`
    when the user visits the page. Applications can choose any string for
    their `activityId`. For simplicity,
    this sample app uses the URI path and query as the `activityId`.
    Observe that the code to create the activity is the same as in the UWP app.

* The sample uses the AdaptiveCards NuGet package to generate adaptive card JSON.
    You are welcome to use any JSON library (or no library at all!)
    to generate your adaptive cards.

* Instead of `CreateSession`,
    Win32 apps must call `CreateSessionForWindow` so that the system can associate
    the activity with the correct window.

* The sample checks whether functionality new to version 1803 and to Windows
    Insider builds is present in the system.

* The `OnUserActivityRequested` method in the `BookViewer.xaml.cs` file
    generates a user activity which represents the current scroll position
    of the document. That position is is encoded in the URI,
    and when the app is activated with that URI, the `Application_Startup` method
    in `App.xaml.cs` first parses the URI to determine which window to create,
    and the `BookViewer` constructor
    parses the query portion of the URI to extract the book and optional
    scroll position. Restoring the scroll position is done in
    the `Window_SourceInitialized` method.

* Right-clicking a book from the main page brings up a menu that gives
    the option of opening the book in a new tab or in a new window.
    The `BookViewer` constructor remembers the desired grouping behavior and applies
    it in the `Window_SourceInitialized` method.

* Right-clicking the word "Library" on the the main page brings up a menu that gives
    the option of opening Notepad in a new tab or in a new window.
    The `DoShellExecute` method saves this grouping behavior to the
    `GroupingPreferenceSite` so that when the `ShellExecuteEx` function
    asks if the app wishes to customize the grouping behavior, the
    `GroupingPreferencesSite` can do so.
