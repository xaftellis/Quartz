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
        public static TitleBarTabs _parentForm;
        public static TitleBarTab _clickedTab;
            
        public static IContextMenuPresenter _contextMenuStripNormal;
        public static IContextMenuPresenter _contextMenuStripTab;
    }
}
