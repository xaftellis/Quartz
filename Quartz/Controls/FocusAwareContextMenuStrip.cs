using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace Quartz.Controls
{
    public class FocusAwareContextMenuStrip : ContextMenuStrip
    {
        public FocusAwareContextMenuStrip() { }

        public FocusAwareContextMenuStrip(IContainer container) : base(container) { }

        protected override void OnOpened(EventArgs e)
        {
            base.OnOpened(e);
            // Give a subsequent page click a native focus transition even if the
            // WebView was already focused when this menu was opened.
            if (!IsDisposed && !Disposing && Visible)
                Focus();
        }
    }
}
