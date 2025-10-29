using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace EasyTabs
{
    public static class ContextMenuProvider
    {
        public static TitleBarTabs _parentForm;
        public static TitleBarTab _clickedTab;
            
        public static ContextMenuStrip _contextMenuStripNormal;
        public static ContextMenuStrip _contextMenuStripTab;
    }
}
