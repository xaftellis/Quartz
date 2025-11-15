using System.Windows.Forms;
using System.Drawing;

namespace Quartz.Services
{
    internal class DarkContextMenuRenderer : ToolStripProfessionalRenderer
    {
        public DarkContextMenuRenderer() : base(new DarkTable()) { }
    }

    public class DarkTable : ProfessionalColorTable
    {
        public override Color MenuBorder
        {
            get { return Color.FromArgb(88, 88, 88); }
        }
        public override Color ImageMarginGradientBegin
        {
            get { return Color.FromArgb(35, 35, 35); }
        }
        public override Color ImageMarginGradientMiddle
        {
            get { return Color.FromArgb(35, 35, 35); }
        }
        public override Color ImageMarginGradientEnd
        {
            get { return Color.FromArgb(35, 35, 35); }
        }
        public override Color MenuItemBorder
        {
            get { return Color.FromArgb(64, 64, 64); }
        }
        public override Color MenuItemSelected
        {
            get { return Color.FromArgb(64, 64, 64); }
        }
        public override Color SeparatorDark
        {
            get { return Color.FromArgb(88, 88, 88); }
        }
    }
}
