using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;

namespace Quartz.Libs
{
    internal class MouseTopDetector
    {
        private Timer mouseTimer;
        private bool isMouseAtTop = false;
        private int enterThreshold;  // Threshold for entering the top
        private int leaveThreshold;  // Threshold for leaving the top

        // Define custom events
        public event EventHandler MouseEnteredTop;
        public event EventHandler MouseLeftTop;

        // Constructor with enter and leave thresholds
        public MouseTopDetector(int enterThreshold = 10, int leaveThreshold = 250)
        {
            this.enterThreshold = enterThreshold;  // Default enter threshold = 10 pixels
            this.leaveThreshold = leaveThreshold;  // Default leave threshold = 50 pixels

            // Create and configure the timer
            mouseTimer = new Timer();
            mouseTimer.Interval = 100; // Check every 100ms
            mouseTimer.Tick += MouseTimer_Tick;
            mouseTimer.Start();
        }

        private void MouseTimer_Tick(object sender, EventArgs e)
        {
            // Get the current mouse position relative to the screen
            Point mousePosition = Cursor.Position;

            // If the mouse is near the very top (within enterThreshold pixels)
            if (mousePosition.Y < enterThreshold)
            {
                // If the mouse enters the top and it's not already detected
                if (!isMouseAtTop)
                {
                    isMouseAtTop = true;
                    OnMouseEnteredTop(); // Trigger Mouse Enter Top event
                }
            }
            else if (mousePosition.Y >= leaveThreshold)
            {
                // If the mouse moves away from the top area (after entering)
                if (isMouseAtTop)
                {
                    isMouseAtTop = false;
                    OnMouseLeftTop(); // Trigger Mouse Left Top event
                }
            }
        }

        // Trigger the MouseEnteredTop event
        protected virtual void OnMouseEnteredTop()
        {
            MouseEnteredTop?.Invoke(this, EventArgs.Empty); // Fire the event
        }

        // Trigger the MouseLeftTop event
        protected virtual void OnMouseLeftTop()
        {
            MouseLeftTop?.Invoke(this, EventArgs.Empty); // Fire the event
        }

        // Stop the timer when the detector is no longer needed
        public void Stop()
        {
            mouseTimer.Stop();
        }
    }
}
