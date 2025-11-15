using System.Windows.Forms;
using System.Drawing;

namespace Quartz.Services
{
    internal class WhiteContextMenuRenderer : ToolStripProfessionalRenderer
    {
        public WhiteContextMenuRenderer() : base(new WhiteTable()) { }
    }

    public class WhiteTable : ProfessionalColorTable
    {
        public override Color MenuBorder
        {
            get { return Color.FromArgb(219, 220, 221);  }
        }
        public override Color MenuItemBorder
        {
            get { return Color.FromArgb(242, 242, 242); }
        }
        public override Color ImageMarginGradientBegin
        {
            get { return Color.White; }
        }
        public override Color ImageMarginGradientMiddle
        {
            get { return Color.White; }
        }

        public override Color ImageMarginGradientEnd
        {
            get { return Color.White; }
        }

        public override Color MenuItemSelected
        {
            get { return Color.FromArgb(242, 242, 242); }
        }

        public override Color SeparatorDark
        {
            get { return Color.FromArgb(219, 220, 221); }
        }
    }
}
