using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace EasyTabs
{
    public interface IContextMenuPresenter
    {
        void Show(Control source, System.Drawing.Point location);
        void Close();
        bool Visible { get; }
        bool IsDisposed { get; }
    }
    public static class ContextMenuProvider
    {
        // The custom presenter owns mouse input while open. Low-level tab hooks
        // must not interpret that same input behind an owned popup.
        public static Func<bool> IsOwnedMenuOpen { get; set; }
        internal static bool MenuOwnsInput => IsOwnedMenuOpen?.Invoke() == true;
        public static TitleBarTabs _parentForm;
        public static TitleBarTab _clickedTab;
            
        public static IContextMenuPresenter _contextMenuStripNormal;
        public static IContextMenuPresenter _contextMenuStripTab;
    }
}
