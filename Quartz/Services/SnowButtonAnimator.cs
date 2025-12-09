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
        private const int MaxFrames = 98;

        static SnowButtonAnimator()
        {
            timer.Tick += Timer_Tick;
        }

        /// <summary>
        /// Start animating this button. If a button with the same Name on the same Form
        /// is already animated, this does nothing.
        /// </summary>
        public static void Animate(Button btn)
        {
            if (btn == null) return;

            Form form = btn.FindForm();

            // check duplicates: same Name + same Form
            foreach (var b in buttons)
            {
                if (b.Name == btn.Name && b.FindForm() == form)
                    return; // already animating same-name on same form
            }

            buttons.Add(btn);
            frames[btn] = 1;

            // remove from lists when disposed
            btn.Disposed += (s, e) => RemoveButton(btn);
            // also remove when parent changes to null (removed from form/controls)
            btn.ParentChanged += (s, e) =>
            {
                if (btn.Parent == null)
                    RemoveButton(btn);
            };

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

        private static void RemoveButton(Button btn)
        {
            if (btn == null) return;

            if (buttons.Contains(btn))
                buttons.Remove(btn);

            if (frames.ContainsKey(btn))
                frames.Remove(btn);

            if (buttons.Count == 0 && timer.Enabled)
                timer.Stop();
        }

        /// <summary>
        /// Optional helper to stop and clear all animations.
        /// </summary>
        public static void StopAll()
        {
            timer.Stop();
            buttons.Clear();
            frames.Clear();
        }
    }
}
