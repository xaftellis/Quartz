using System.Windows.Forms;
using System.Drawing;
using System.Windows.Media.Animation;

namespace Quartz.Services
{
    internal class AquaContextMenuRenderer : ToolStripProfessionalRenderer
    {
        public AquaContextMenuRenderer() : base(new AquaTable()) { }
    }

    public class AquaTable : ProfessionalColorTable
    {
        public override Color MenuBorder
        {
            get { return Color.Blue; }
        }
        public override Color ImageMarginGradientBegin
        {
            get { return Color.Blue; }
        }
        public override Color ImageMarginGradientMiddle
        {
            get { return Color.Blue; }
        }
        public override Color ImageMarginGradientEnd
        {
            get { return Color.Blue; }
        }
        public override Color MenuItemBorder
        {
            get { return Color.FromArgb(0, 64, 255); }
        }
        public override Color MenuItemSelected
        {
            get { return Color.FromArgb(0, 64, 255); }
        }
        public override Color SeparatorDark
        {
            get { return Color.Aqua; }
        }
    }
}
