using System.Windows.Forms;
using System.Drawing;

namespace Quartz.Services
{
    internal class BlackContextMenuRenderer : ToolStripProfessionalRenderer
    {
        public BlackContextMenuRenderer() : base(new BlackTable()) { }
    }

    public class BlackTable : ProfessionalColorTable
    {
        public override Color MenuBorder
        {
            get { return Color.FromArgb(64, 64, 64); }
        }
        public override Color ImageMarginGradientBegin
        {
            get { return Color.Black; }
        }
        public override Color ImageMarginGradientMiddle
        {
            get { return Color.Black; }
        }
        public override Color ImageMarginGradientEnd
        {
            get { return Color.Black; }
        }
        public override Color MenuItemBorder
        {
            get { return Color.FromArgb(18, 18, 18); }
        }
        public override Color MenuItemSelected
        {
            get { return Color.FromArgb(18, 18, 18); }
        }
        public override Color SeparatorDark
        {
            get { return Color.FromArgb(128, 128, 128); }
        }
    }
}
