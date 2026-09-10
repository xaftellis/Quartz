EasyTabs
========

EasyTabs is a library that you can add to your .NET WinForms applications (WPF support coming eventually) in order to render a list of tabs in the title bar of the application, similar to Chrome, Firefox, Edge, etc.  Instead of inheriting from `System.Windows.Forms.Form`, you inherit from `EasyTabs.TitleBarTabs` and set the tab renderer that you wish to use:

```cs
using EasyTabs;

namespace YourNamespace
{
    public partial class YourApp : TitleBarTabs
    {
        public TestApp()
        {
            InitializeComponent();

            // Enable or disable viewing tabs through the taskbar
            AeroPeekEnabled = true;
            // Set the tab rendering engine that you wish to use
            TabRenderer = new ChromeTabRenderer(this);
        }
        
        // ...
    }
}
```

The base class takes care of the grunt work of rendering the tabs, responding to clicks to activate/close/add/etc. while you simply add `TitleBarTab` objects to the `Tabs` collection.  `TitleBarTab` objects expect their `Content` property to be set to a `Form` object that represents the contents for the tab.  You can design these forms in Visual Studio the same as you would any other application; the `Title` and `Icon` properties are used to display the tab itself.

The vector `ChromiumTabRenderer` enables the two-stage waiting/loading animation by default. To use the modern single spinner, set this option when configuring EasyTabs:

```cs
TabRenderer = new ChromiumTabRenderer(this)
{
    ShowWaitingAnimation = false
};
```

Content implementing `ITabLoadingPhase` supplies the waiting state; existing `ITabLoadingState` implementations still work. `ITabFaviconState` identifies the default placeholder. The favicon transition uses [modern Chromium's reversible animation](https://chromium.googlesource.com/chromium/src/+/9130e7a5778e8a5e29cbb36b0c3bf3aec6fdb5cf/chrome/browser/ui/views/tabs/tab/tab_icon.cc); the optional waiting stage uses [Chromium's implementation immediately before its October 2024 removal](https://chromium.googlesource.com/chromium/src/+/e8b04254875e840f401e669680ae05005895d71c/ui/gfx/paint_throbber.cc).

The library comes with a renderer for Chrome-like tabs (`ChromeTabRenderer`), but you can implement your own by creating a class inheriting from `BaseTabRenderer`.  The TestApp directory contains the project for a tabbed web browser test application that you can use as a starting point for implementing your own functionality.

You can include this functionality in your project via NuGet:

    PM> Install-Package EasyTabs

This project is licensed under the terms of the [BSD license](BSD.txt)
