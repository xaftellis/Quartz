using EasyTabs;
using Quartz.Controls;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

internal static partial class PinnedTabsTests
{
    private sealed class WindowMoveMenu : TabContextMenu
    {
        internal Window CreatedWindow;
        internal TitleBarTabs Activated;
        protected override TitleBarTabs CreateMoveWindow(TitleBarTabs source)
        {
            CreatedWindow = new Window();
            source.ApplicationContext.OpenWindow(CreatedWindow);
            return CreatedWindow;
        }
        protected override void ActivateMoveWindow(TitleBarTabs destination) => Activated = destination;
    }

    private static ToolStripMenuItem OpenMoveMenu(TabContextMenu menu, Window window, TitleBarTab tab)
    {
        ContextMenuProvider._parentForm = window;
        ContextMenuProvider._clickedTab = tab;
        var args = new CancelEventArgs();
        typeof(TabContextMenu).GetMethod("DefaultContextMenu_Opening", BindingFlags.Instance | BindingFlags.NonPublic)
            .Invoke(menu, new object[] { menu, args });
        Check(!args.Cancel, "A valid tab opens its context menu.");
        return menu.Items.OfType<ToolStripMenuItem>().Single(item => item.Text.StartsWith("Move tab to "));
    }

    private static void WindowTransfers()
    {
        foreach (bool pinned in new[] { false, true })
        foreach (bool active in new[] { false, true })
        using (var context = new TitleBarTabsApplicationContext())
        using (var source = new Window())
        using (var target = new Window())
        {
            context.OpenWindow(source); context.OpenWindow(target);
            var left = source.Add("Left");
            var moving = source.Add("Moving", pinned);
            var right = source.Add("Right");
            source.SelectedTab = active ? moving : right;
            target.Add("Target pin", true); var previous = target.Add("Target normal");
            target.SelectedTab = previous;
            var content = moving.Content;
            IntPtr handle = content.Handle;
            object state = content.Tag = new object();
            moving.IsLoading = true;
            int closed = 0, renamed = 0;
            moving.Closing += (s, e) => closed++;
            moving.TextChanged += (s, e) => renamed++;
            Check(source.MoveTabToWindow(moving, target), "Move active/background, pinned/normal tabs.");
            Check(moving.Parent == target && content.Parent == target && target.Tabs.Contains(moving) &&
                !source.Tabs.Contains(moving), "Window, content parent and collection ownership move together.");
            Check(content.Handle == handle && content.Tag == state && !content.IsDisposed && moving.IsLoading &&
                moving.IsPinned == pinned && closed == 0, "Transfer preserves the live page, native handle, loading and pin state.");
            Check(target.SelectedTab == moving && !previous.Active, "The moved tab becomes the destination's only selection.");
            Check(source.SelectedTab == (active ? (pinned ? left : right) : right), "Source selects a neighbor only when moving its active tab.");
            Check(target.Order == (pinned ? "Target pin,Moving,Target normal" : "Target pin,Target normal,Moving"),
                "Destination insertion respects the pinned boundary.");
            Check(content.Width == target.ClientSize.Width, "Transferred content fits the destination.");
            content.Text = "Renamed";
            Check(renamed == 1, "Client title subscriptions survive transfer.");
            Check(target.MoveTabToWindow(moving, source) && source.MoveTabToWindow(moving, target), "Repeated transfers work.");
            var handlers = (Delegate)typeof(TitleBarTab).GetField("Closing", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(moving);
            Check(handlers.GetInvocationList().Count(handler => handler.Target == target) == 1 &&
                handlers.GetInvocationList().All(handler => handler.Target != source), "Repeated transfers leave exactly one destination close subscription.");
            int sourceCount = source.Tabs.Count;
            moving.Content.Close();
            Check(closed == 1 && content.IsDisposed && !target.Tabs.Contains(moving) && source.Tabs.Count == sourceCount,
                "Closing a moved tab closes once and only changes its current window.");
        }

        using (var context = new TitleBarTabsApplicationContext())
        using (var source = new Window())
        using (var target = new Window())
        using (var unrelated = new Window())
        {
            context.OpenWindow(source); context.OpenWindow(target);
            var tab = source.Add("Last", true, cancelClose: true);
            target.Add("Target");
            Check(!source.MoveTabToWindow(tab, source) && !source.MoveTabToWindow(tab, unrelated), "Self and unregistered destinations are rejected.");
            target.IsClosing = true;
            Check(!source.MoveTabToWindow(tab, target), "Closing destinations are rejected without detaching the tab.");
            target.IsClosing = false;
            source.ExitOnLastTabClose = true;
            Check(source.MoveTabToWindow(tab, target), "The last tab can move into an existing window.");
            Check(source.IsDisposed && !context.OpenWindows.Contains(source) && context.OpenWindows.Contains(target) &&
                !tab.Content.IsDisposed, "Empty source closes after transfer without disposing the moved page or exiting the application.");
            tab.Content.Close();
            Check(!tab.Content.IsDisposed && target.Tabs.Contains(tab), "Page close cancellation still works after transfer.");
            target.Dispose();
            Check(!context.OpenWindows.Any() && !context.OpenWindowsByActivation.Any(), "Disposal removes windows from both lists.");
        }
    }

    private static void WindowMoveMenus()
    {
        using (var context = new TitleBarTabsApplicationContext())
        using (var source = new Window())
        using (var first = new Window())
        using (var second = new Window())
        using (var menu = new WindowMoveMenu())
        {
            context.OpenWindow(source);
            var moving = source.Add("Moving");
            var root = OpenMoveMenu(menu, source, moving);
            Check(root.Text == "Move tab to new window" && !root.Enabled && !root.HasDropDownItems,
                "One window with one tab shows a disabled direct command.");
            source.Add("Stay");
            root = OpenMoveMenu(menu, source, moving);
            Check(root.Enabled && !root.HasDropDownItems, "A second tab enables moving into a new window.");
            root.PerformClick();
            Check(menu.CreatedWindow != null && menu.CreatedWindow.Tabs.Single() == moving && menu.Activated == menu.CreatedWindow &&
                moving.Parent == menu.CreatedWindow && source.Tabs.Count == 1, "The direct command transfers the clicked tab and activates a new window.");
            menu.CreatedWindow.Dispose();

            var remaining = source.Tabs.Single();
            first.Add("R&D"); first.Add("Background");
            second.Add("Second");
            context.OpenWindow(first); context.OpenWindow(second);
            root = OpenMoveMenu(menu, source, remaining);
            Check(root.Text == "Move tab to another window" && root.Enabled && root.DropDownItems.Count == 4,
                "Other windows turn the command into a destination submenu.");
            Check(root.DropDownItems[0].Text == "New window" && !root.DropDownItems[0].Enabled &&
                root.DropDownItems[1] is ToolStripSeparator, "New window is first and disabled for the last source tab.");
            Check(root.DropDownItems[2].Text == "Second" && root.DropDownItems[3].Text == "R&&D and 1 other tab",
                "Window labels include the active title, literal ampersands and other-tab count.");
            typeof(Form).GetMethod("OnActivated", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(first, new object[] { EventArgs.Empty });
            root = OpenMoveMenu(menu, source, remaining);
            Check(root.DropDownItems[2].Text == "R&&D and 1 other tab", "Window list follows most recent activation.");
            Check(context.OpenWindows.SequenceEqual(new[] { source, first, second }), "Activation does not reorder the application's original window list.");

            var staleTarget = (ToolStripMenuItem)root.DropDownItems[2];
            first.Dispose();
            staleTarget.PerformClick();
            Check(remaining.Parent == source && source.Tabs.Contains(remaining), "A destination closed while the menu was open is a safe no-op.");
            second.SelectedTab.Caption = new string('W', 250) + "😀";
            root = OpenMoveMenu(menu, source, remaining);
            Check(root.DropDownItems.Count == 3 && root.DropDownItems[2].Text.EndsWith("…") &&
                root.DropDownItems[2].Text.Length < 100, "Reopening removes closed destinations and elides long labels.");

            second.WindowState = FormWindowState.Minimized;
            root = OpenMoveMenu(menu, source, remaining);
            Check(root.DropDownItems.Count == 3 && root.DropDownItems[2].Enabled, "Minimized windows remain available destinations.");
            root.DropDownItems[2].PerformClick();
            Check(remaining.Parent == second && second.SelectedTab == remaining && menu.Activated == second,
                "Existing-window menu command moves and selects the clicked tab.");
        }
        ContextMenuProvider._parentForm = null; ContextMenuProvider._clickedTab = null;
    }
}
