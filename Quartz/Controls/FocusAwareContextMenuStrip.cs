using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace Quartz.Controls
{
    // WebView2's native child windows do not participate in the WinForms menu
    // message filter. Give an open menu focus so clicking back into an already
    // focused page still produces a native focus transition.
    public class FocusAwareContextMenuStrip : ContextMenuStrip
    {
        public FocusAwareContextMenuStrip() { }
        public FocusAwareContextMenuStrip(IContainer container) : base(container) { }

        protected override void OnOpening(CancelEventArgs e)
        {
            // Opening handlers populate dynamic menus. Style the final items,
            // including newly created submenus, before the popup is measured.
            base.OnOpening(e);
            if (!e.Cancel) ChromiumMenuStyle.Apply(this);
        }

        protected override void OnOpened(EventArgs e)
        {
            base.OnOpened(e);
            if (Visible && !IsDisposed) Focus();
        }
    }
}
