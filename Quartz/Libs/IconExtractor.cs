using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.IconLib;
using System.Windows;
using System.Drawing;
using System.Windows.Media.Media3D;
using System.IO;

namespace Quartz.Libs
{
    internal class IconExtractor
    {
        public static Icon ExtractIconSize(string path, System.Drawing.Size size)
        {
            MultiIcon multiIcon = new MultiIcon();
            multiIcon.Load(path);

            foreach (SingleIcon singleIcon in multiIcon)
            {
                foreach (IconImage iconImage in singleIcon)
                {
                    Icon icon = iconImage.Icon;

                    if (icon.Size == size)
                    {
                        return icon;
                    }
                }
            }

            return null;
        }

    }
}
