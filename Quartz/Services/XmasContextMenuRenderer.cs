using System.Windows.Forms;
using System.Drawing;

namespace Quartz.Services
{
    internal class XmasContextMenuRenderer : ToolStripProfessionalRenderer
    {
        public XmasContextMenuRenderer() : base(new XmasTable()) { }
    }

    public class XmasTable : ProfessionalColorTable
    {
        public override Color MenuBorder
        {
            get { return Color.Lime; }
        }
        public override Color ImageMarginGradientBegin
        {
            get { return Color.Red; }
        }
        public override Color ImageMarginGradientMiddle
        {
            get { return Color.Red; }
        }
        public override Color ImageMarginGradientEnd
        {
            get { return Color.Red; }
        }
        public override Color MenuItemBorder
        {
            get { return Color.Lime; }
        }
        public override Color MenuItemSelected
        {
            get { return Color.Red; }
        }
        public override Color SeparatorDark
        {
            get { return Color.Lime; }
        }
    }
}
