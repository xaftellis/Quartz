using Svg;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Win32Interop.Enums;

namespace EasyTabs
{
    public class WindowsSizingBoxes : IDisposable
    {
        protected TitleBarTabs _parentWindow;
        protected Image _minimizeImage = null;
        protected Image _restoreImage = null;
        protected Image _maximizeImage = null;
        protected Image _closeImage = null;
        protected Image _closeHighlightImage = null;
        protected Brush _minimizeMaximizeButtonHighlight = new SolidBrush(Color.FromArgb(27, Color.Black));
        protected Brush _closeButtonHighlight = new SolidBrush(Color.FromArgb(232, 17, 35));
        protected Rectangle _minimizeButtonArea = new Rectangle(0, 0, 45, 29);
        protected Rectangle _maximizeRestoreButtonArea = new Rectangle(45, 0, 45, 29);
        protected Rectangle _closeButtonArea = new Rectangle(90, 0, 45, 29);

        public float Scale { get; set; } = 1;
        private int Pixel(float value) => (int)Math.Round(value * Scale);

        public WindowsSizingBoxes(TitleBarTabs parentWindow)
        {
            _parentWindow = parentWindow;
            _minimizeImage = LoadSvg(Encoding.UTF8.GetString(Resources.Minimize), 10, 10);
            _restoreImage = LoadSvg(Encoding.UTF8.GetString(Resources.Restore), 10, 10);
            _maximizeImage = LoadSvg(Encoding.UTF8.GetString(Resources.Maximize), 10, 10);
            _closeImage = LoadSvg(Encoding.UTF8.GetString(Resources.Close), 10, 10);
            _closeHighlightImage = LoadSvg(Encoding.UTF8.GetString(Resources.CloseHighlight), 10, 10);
        }

        protected Image LoadSvg(string svgXml, int width, int height)
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(svgXml);

            return SvgDocument.Open(xmlDocument).Draw(width, height);
        }

        public int Width
        {
            get
            {
                return Pixel(45) * 3;
            }
        }

        public bool Contains(Point cursor)
        {
            return _minimizeButtonArea.Contains(cursor) || _maximizeRestoreButtonArea.Contains(cursor) || _closeButtonArea.Contains(cursor);
        }

        public void Render(Graphics graphicsContext, Point cursor)
        {
            int right = _parentWindow.ClientRectangle.Width;
            bool closeButtonHighlighted = false;
            
            int buttonWidth = Pixel(45);
            _minimizeButtonArea = new Rectangle(right - buttonWidth * 3, 0, buttonWidth, Pixel(29));
            _maximizeRestoreButtonArea = new Rectangle(right - buttonWidth * 2, 0, buttonWidth, Pixel(29));
            _closeButtonArea = new Rectangle(right - buttonWidth, 0, buttonWidth, Pixel(29));

            if (_minimizeButtonArea.Contains(cursor))
            {
                graphicsContext.FillRectangle(_minimizeMaximizeButtonHighlight, _minimizeButtonArea);
            }

            else if (_maximizeRestoreButtonArea.Contains(cursor))
            {
                graphicsContext.FillRectangle(_minimizeMaximizeButtonHighlight, _maximizeRestoreButtonArea);
            }

            else if (_closeButtonArea.Contains(cursor))
            {
                graphicsContext.FillRectangle(_closeButtonHighlight, _closeButtonArea);
                closeButtonHighlighted = true;
            }

            graphicsContext.DrawImage(closeButtonHighlighted ? _closeHighlightImage : _closeImage, _closeButtonArea.X + Pixel(17), Pixel(9), Pixel(10), Pixel(10));
            graphicsContext.DrawImage(_parentWindow.WindowState == FormWindowState.Maximized ? _restoreImage : _maximizeImage, _maximizeRestoreButtonArea.X + Pixel(17), Pixel(9), Pixel(10), Pixel(10));
            graphicsContext.DrawImage(_minimizeImage, _minimizeButtonArea.X + Pixel(17), Pixel(9), Pixel(10), Pixel(10));
        }

        public void Dispose()
        {
            _minimizeImage?.Dispose(); _restoreImage?.Dispose(); _maximizeImage?.Dispose();
            _closeImage?.Dispose(); _closeHighlightImage?.Dispose();
            _minimizeMaximizeButtonHighlight?.Dispose(); _closeButtonHighlight?.Dispose();
        }

        public HT NonClientHitTest(Point cursor)
        {
            if (_minimizeButtonArea.Contains(cursor))
            {
                return HT.HTMINBUTTON;
            }

            else if (_maximizeRestoreButtonArea.Contains(cursor))
            {
                return HT.HTMAXBUTTON;
            }

            else if (_closeButtonArea.Contains(cursor))
            {
                return HT.HTCLOSE;
            }

            return HT.HTNOWHERE;
        }
    }
}
