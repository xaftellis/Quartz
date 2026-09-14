using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Quartz.Services
{
    public static class SnowButtonAnimator
    {
        private static readonly Timer timer = new Timer { Interval = 30 };
        // list of buttons being animated
        private static readonly List<Button> buttons = new List<Button>();
        // frame index per button
        private static readonly Dictionary<Button, int> frames = new Dictionary<Button, int>();
        private static readonly Dictionary<Button, Image> originalImages = new Dictionary<Button, Image>();
        private const int MaxFrames = 98;

        static SnowButtonAnimator()
        {
            timer.Tick += Timer_Tick;
        }

        /// <summary>
        /// Start animating this button. Repeated calls for the same button do nothing.
        /// </summary>
        public static void Animate(Button btn)
        {
            if (btn == null) return;

            if (buttons.Contains(btn)) return;
            buttons.Add(btn);
            frames[btn] = 1;
            originalImages[btn] = btn.BackgroundImage;
            btn.Disposed += ButtonDisposed;
            btn.ParentChanged += ButtonParentChanged;

            if (!timer.Enabled)
                timer.Start();
        }

        private static void Timer_Tick(object sender, EventArgs e)
        {
            // iterate over a copy to allow safe removal during iteration
            var copy = new List<Button>(buttons);

            foreach (var btn in copy)
            {
                // skip invalid buttons
                if (btn == null || btn.IsDisposed || btn.Parent == null)
                {
                    RemoveButton(btn);
                    continue;
                }

                int frame = frames.TryGetValue(btn, out var f) ? f : 1;
                string resourceName = $"frame_{frame}";

                var prop = typeof(Properties.Resources).GetProperty(resourceName,
                    BindingFlags.Static | BindingFlags.Public);

                if (prop != null)
                {
                    var img = prop.GetValue(null) as Image;
                    // timer runs on UI thread, so direct assignment is safe
                    btn.BackgroundImage = img;
                }

                frame++;
                if (frame > MaxFrames) frame = 1;
                frames[btn] = frame;
            }

            // stop timer automatically if nothing left
            if (buttons.Count == 0 && timer.Enabled)
                timer.Stop();
        }

        private static void ButtonDisposed(object sender, EventArgs e) => RemoveButton((Button)sender);
        private static void ButtonParentChanged(object sender, EventArgs e)
        {
            var button = (Button)sender;
            if (button.Parent == null) RemoveButton(button);
        }

        public static void Stop(Button button) => RemoveButton(button);

        private static void RemoveButton(Button btn)
        {
            if (btn == null || !buttons.Remove(btn)) return;
            frames.Remove(btn);
            btn.Disposed -= ButtonDisposed;
            btn.ParentChanged -= ButtonParentChanged;
            if (originalImages.TryGetValue(btn, out Image image) && !btn.IsDisposed)
                btn.BackgroundImage = image;
            originalImages.Remove(btn);
            if (buttons.Count == 0) timer.Stop();
        }

        public static void StopAll()
        {
            foreach (var button in buttons.ToArray()) RemoveButton(button);
        }
    }
}
