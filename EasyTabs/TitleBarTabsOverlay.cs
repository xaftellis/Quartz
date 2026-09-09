using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Win32Interop.Enums;
using Win32Interop.Methods;
using Win32Interop.Structs;
using Timer = System.Timers.Timer;

namespace EasyTabs
{
	/// <summary>
	/// Borderless overlay window that is moved with and rendered on top of the non-client area of a  <see cref="TitleBarTabs" /> instance that's responsible
	/// for rendering the actual tab content and responding to click events for those tabs.
	/// </summary>
	public class TitleBarTabsOverlay : Form
	{
		private const int DwmwaTransitionsForcedDisabled = 3;

		// A lone tab uses the parent's native caption move loop. Only its owner checks
		// merge targets while Windows handles movement, restore-from-maximized and snap.
		private static volatile TitleBarTabsOverlay _singleTabDragOwner;
		private TitleBarTabs _singleTabDropTarget;
		private Point _singleTabDragStart;
		private Point _singleTabDropPoint;
		private bool _singleTabMergeHidden;

		[DllImport("user32.dll", EntryPoint = "SendMessageW")]
		private static extern IntPtr SendWindowDragMessage(IntPtr window, int message, IntPtr wParam, IntPtr lParam);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool ReleaseCapture();

		private System.Windows.Forms.Timer _loadingAnimationTimer;

		private void UpdateLoadingAnimation()
		{
			if (_loadingAnimationTimer == null) return;
			_loadingAnimationTimer.Enabled = !IsDisposed && !Disposing && !_parentForm.IsDisposed &&
				!_parentForm.Disposing && _parentForm.Visible &&
				_parentForm.WindowState != FormWindowState.Minimized &&
				(_parentForm.Tabs.Any(tab => tab.IsLoading && !tab.Content.IsDisposed) ||
					(_parentForm.TabRenderer != null && _parentForm.TabRenderer.IsLayoutAnimating));
		}

		private void LoadingAnimation_Tick(object sender, EventArgs e)
		{
			UpdateLoadingAnimation();
			// Share the loading timer with layout easing. Position-only animation
			// reuses tab backgrounds and the timer stops once both kinds are idle.
			if (_loadingAnimationTimer.Enabled) Render();
		}

		/// <summary>Releases the window's shared animation timer.</summary>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_loadingAnimationTimer?.Dispose();
				_loadingAnimationTimer = null;
			}
			base.Dispose(disposing);
		}

		[DllImport("dwmapi.dll")]
		private static extern int DwmSetWindowAttribute(IntPtr windowHandle, int attribute, ref int attributeValue, int attributeSize);

		private static void SetWindowTransitionsEnabled(Form window, bool enabled)
		{
			int transitionsDisabled = enabled ? 0 : 1;
			DwmSetWindowAttribute(window.Handle, DwmwaTransitionsForcedDisabled, ref transitionsDisabled, sizeof(int));
		}

        protected Timer showTooltipTimer;

		/// <summary>All of the parent forms and their overlays so that we don't create duplicate overlays across the application domain.</summary>
		protected static Dictionary<TitleBarTabs, TitleBarTabsOverlay> _parents = new Dictionary<TitleBarTabs, TitleBarTabsOverlay>();

		/// <summary>Tab that has been torn off from this window and is being dragged.</summary>
		protected static TitleBarTab _tornTab;

		/// <summary>Live window containing <see cref="_tornTab" /> while it is being dragged.</summary>
		protected static TitleBarTabs _tornTabWindow;

		/// <summary>Overlay that owns the current cross-window tab drag.</summary>
		protected static TitleBarTabsOverlay _tornTabDragOwner;

		/// <summary>Cursor offset inside <see cref="_tornTabWindow" /> while dragging.</summary>
		protected static Point _tornTabWindowCursorOffset;

		/// <summary>Latest horizontal cursor position waiting to be applied to the live torn-tab window.</summary>
		protected static int _latestTornTabCursorX;

		/// <summary>Latest vertical cursor position waiting to be applied to the live torn-tab window.</summary>
		protected static int _latestTornTabCursorY;

		/// <summary>Prevents mouse moves from queuing faster than the UI thread can display them.</summary>
		protected static int _tornTabMoveQueued;

		/// <summary>
		/// Flag used in <see cref="WndProc" /> and <see cref="MouseHookCallback" /> to track whether the user was click/dragging when a particular event
		/// occurred.
		/// </summary>
		protected static bool _wasDragging = false;

		/// <summary>Flag indicating whether or not <see cref="_hookproc" /> has been installed as a hook.</summary>
		protected static bool _hookProcInstalled;

		/// <summary>Semaphore to control access to <see cref="_tornTab" />.</summary>
		protected static object _tornTabLock = new object();

        protected static uint _doubleClickInterval = User32.GetDoubleClickTime();

        /// <summary>Flag indicating whether or not the underlying window is active.</summary>
        protected bool _active = false;

		/// <summary>Flag indicating whether we should draw the titlebar background (i.e. we are in a non-Aero environment).</summary>
		protected bool _aeroEnabled = false;

		/// <summary>
		/// When a tab is torn from the window, this is where we store the areas on all open windows where tabs can be dropped to combine the tab with that
		/// window.
		/// </summary>
		protected Tuple<TitleBarTabs, Rectangle>[] _dropAreas = null;

		/// <summary>Pointer to the low-level mouse hook callback (<see cref="MouseHookCallback" />).</summary>
		protected IntPtr _hookId;

		/// <summary>Delegate of <see cref="MouseHookCallback" />; declared as a member variable to keep it from being garbage collected.</summary>
		protected HOOKPROC _hookproc = null;

		/// <summary>Index of the tab, if any, whose close button is being hovered over.</summary>
		protected int _isOverCloseButtonForTab = -1;

        protected bool _isOverSizingBox = false;

        protected bool _isOverAddButton = true;

		/// <summary>Queue of mouse events reported by <see cref="_hookproc" /> that need to be processed.</summary>
		protected BlockingCollection<MouseEvent> _mouseEvents = new BlockingCollection<MouseEvent>();

		/// <summary>Consumer thread for processing events in <see cref="_mouseEvents" />.</summary>
		protected Thread _mouseEventsThread = null;

		/// <summary>Parent form for the overlay.</summary>
		protected TitleBarTabs _parentForm;

        protected long _lastLeftButtonClickTicks = 0;

		protected bool _firstClick = true;
		protected Point[] _lastTwoClickCoordinates = new Point[2];

		protected bool _parentFormClosing = false;

		/// <summary>Blank default constructor to ensure that the overlays are only initialized through <see cref="GetInstance" />.</summary>
		protected TitleBarTabsOverlay()
		{
		}

		/// <summary>Creates the overlay window and attaches it to <paramref name="parentForm" />.</summary>
		/// <param name="parentForm">Parent form that the overlay should be rendered on top of.</param>
		protected TitleBarTabsOverlay(TitleBarTabs parentForm)
		{
			_parentForm = parentForm;
			_loadingAnimationTimer = new System.Windows.Forms.Timer { Interval = 16 };
			_loadingAnimationTimer.Tick += LoadingAnimation_Tick;

			// We don't want this window visible in the taskbar
			ShowInTaskbar = false;
			FormBorderStyle = FormBorderStyle.SizableToolWindow;
			MinimizeBox = false;
			MaximizeBox = false;
			_aeroEnabled = _parentForm.IsCompositionEnabled;

			Show(_parentForm);
			AttachHandlers();

			showTooltipTimer = new Timer
			{
				AutoReset = false
			};

			showTooltipTimer.Elapsed += ShowTooltipTimer_Elapsed;
		}

		/// <summary>
		/// Makes sure that the window is created with an <see cref="WS_EX.WS_EX_LAYERED" /> flag set so that it can be alpha-blended properly with the content (
		/// <see cref="_parentForm" />) underneath the overlay.
		/// </summary>
		protected override CreateParams CreateParams
		{
			get
			{
				CreateParams createParams = base.CreateParams;
				createParams.ExStyle |= (int) (WS_EX.WS_EX_LAYERED | WS_EX.WS_EX_NOACTIVATE);

				return createParams;
			}
		}

		/// <summary>Primary color for the titlebar background.</summary>
		protected Color TitleBarColor
		{
			get
			{
				if (Application.RenderWithVisualStyles && Environment.OSVersion.Version.Major >= 6)
				{
					return _active
						? SystemColors.GradientActiveCaption
						: SystemColors.GradientInactiveCaption;
				}

				return _active
					? SystemColors.ActiveCaption
					: SystemColors.InactiveCaption;
			}
		}

		/// <summary>Type of theme being used by the OS to render the desktop.</summary>
		protected DisplayType DisplayType
		{
			get
			{
				if (_aeroEnabled)
				{
					return DisplayType.Aero;
				}

				if (Application.RenderWithVisualStyles && Environment.OSVersion.Version.Major >= 6)
				{
					return DisplayType.Basic;
				}

				return DisplayType.Classic;
			}
		}

		/// <summary>Gradient color for the titlebar background.</summary>
		protected Color TitleBarGradientColor
		{
			get
			{
				return _active
					? SystemInformation.IsTitleBarGradientEnabled
						? SystemColors.GradientActiveCaption
						: SystemColors.ActiveCaption
					: SystemInformation.IsTitleBarGradientEnabled
						? SystemColors.GradientInactiveCaption
						: SystemColors.InactiveCaption;
			}
		}

		/// <summary>Screen area in which tabs can be dragged to and dropped for this window.</summary>
		public Rectangle TabDropArea
		{
			get
			{
				RECT windowRectangle;
				User32.GetWindowRect(_parentForm.Handle, out windowRectangle);
				Rectangle tabDragArea = _parentForm.TabRenderer == null
					? Rectangle.Empty
					: _parentForm.TabRenderer.MaxTabArea;
				int left = tabDragArea.Width > 0
					? tabDragArea.Left
					: windowRectangle.left + SystemInformation.HorizontalResizeBorderThickness;
				int width = tabDragArea.Width > 0
					? tabDragArea.Width
					: ClientRectangle.Width;

				return new Rectangle(
					left, windowRectangle.top + SystemInformation.VerticalResizeBorderThickness,
					width, _parentForm.NonClientAreaHeight - SystemInformation.VerticalResizeBorderThickness);
			}
		}

		/// <summary>Retrieves or creates the overlay for <paramref name="parentForm" />.</summary>
		/// <param name="parentForm">Parent form that we are to create the overlay for.</param>
		/// <returns>Newly-created or previously existing overlay for <paramref name="parentForm" />.</returns>
		public static TitleBarTabsOverlay GetInstance(TitleBarTabs parentForm)
		{
			if (!_parents.ContainsKey(parentForm))
			{
				_parents.Add(parentForm, new TitleBarTabsOverlay(parentForm));
			}

			return _parents[parentForm];
		}

		/// <summary>
		/// Attaches the various event handlers to <see cref="_parentForm" /> so that the overlay is moved in synchronization to
		/// <see cref="_parentForm" />.
		/// </summary>
		protected void AttachHandlers()
		{
            FormClosing += TitleBarTabsOverlay_FormClosing;

			_parentForm.FormClosing += _parentForm_FormClosing;
			_parentForm.Disposed += _parentForm_Disposed;
			_parentForm.Deactivate += _parentForm_Deactivate;
			_parentForm.Activated += _parentForm_Activated;
			_parentForm.SizeChanged += _parentForm_Refresh;
			_parentForm.Shown += _parentForm_Refresh;
			_parentForm.VisibleChanged += _parentForm_Refresh;
			_parentForm.Move += _parentForm_Refresh;
			_parentForm.SystemColorsChanged += _parentForm_SystemColorsChanged;

			if (_hookproc == null)
			{
				// Spin up a consumer thread to process mouse events from _mouseEvents
				_mouseEventsThread = new Thread(InterpretMouseEvents)
				{
					Name = "Low level mouse hooks processing thread"
				};
				_mouseEventsThread.Priority = ThreadPriority.Highest;
				_mouseEventsThread.Start();

				using (Process curProcess = Process.GetCurrentProcess())
				{
					using (ProcessModule curModule = curProcess.MainModule)
					{
						// Install the low level mouse hook that will put events into _mouseEvents
						_hookproc = MouseHookCallback;
						_hookId = User32.SetWindowsHookEx(WH.WH_MOUSE_LL, _hookproc, Kernel32.GetModuleHandle(curModule.ModuleName), 0);
					}
				}
			}
		}

        private void TitleBarTabsOverlay_FormClosing(object sender, FormClosingEventArgs e)
        {
			if (!_parentFormClosing)
            {
				e.Cancel = true;
				_parentFormClosing = true;
				_parentForm.Close();
            }
        }

        /// <summary>
        /// Event handler that is called when <see cref="_parentForm" /> is in the process of closing.  This uninstalls <see cref="_hookproc" /> from the low-
        /// level hooks list and stops the consumer thread that processes those events.
        /// </summary>
        /// <param name="sender">Object from which this event originated, <see cref="_parentForm" /> in this case.</param>
        /// <param name="e">Arguments associated with this event.</param>
        private void _parentForm_FormClosing(object sender, CancelEventArgs e)
		{
			if (e.Cancel)
			{
				_parentFormClosing = false;
				return;
			}

            TitleBarTabs form = (TitleBarTabs) sender;

			if (form == null)
			{
				return;
			}

			_parentFormClosing = true;

			if (_parents.ContainsKey(form))
			{
				_parents.Remove(form);
			}

			// Uninstall the mouse hook
			User32.UnhookWindowsHookEx(_hookId);

			// Kill the mouse events processing thread
			_mouseEvents.CompleteAdding();
			_mouseEventsThread.Abort();
		}

		private void HideTooltip()
		{
			showTooltipTimer.Stop();

			if (_parentForm.InvokeRequired)
			{
				_parentForm.Invoke(new Action(() =>
				{
					_parentForm.Tooltip.Hide(_parentForm);
				}));
			}

			else
			{
				_parentForm.Tooltip.Hide(_parentForm);
			}
		}

		private void ShowTooltip(TitleBarTabs tabsForm, string caption)
		{
			Point tooltipLocation = new Point(Cursor.Position.X + 7, Cursor.Position.Y + 55);
			tabsForm.Tooltip.Show(caption, tabsForm, tabsForm.PointToClient(tooltipLocation), tabsForm.Tooltip.AutoPopDelay);
		}

		private void ShowTooltipTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			if (!_parentForm.ShowTooltips)
			{
				return;
			}

			Point relativeCursorPosition = GetRelativeCursorPosition(Cursor.Position);
			TitleBarTab hoverTab = _parentForm.TabRenderer.OverTab(_parentForm.Tabs, relativeCursorPosition);

			if (hoverTab != null)
			{
				TitleBarTabs hoverTabForm = hoverTab.Parent;

				if (hoverTabForm.InvokeRequired)
				{
					hoverTabForm.Invoke(new Action(() =>
					{
						ShowTooltip(hoverTabForm, hoverTab.Caption);
					}));
				}

				else
				{
					ShowTooltip(hoverTabForm, hoverTab.Caption);
				}
			}
		}

		private void StartTooltipTimer()
		{
			if (!_parentForm.ShowTooltips)
			{
				return;
			}

			Point relativeCursorPosition = GetRelativeCursorPosition(Cursor.Position);
			TitleBarTab hoverTab = _parentForm.TabRenderer.OverTab(_parentForm.Tabs, relativeCursorPosition);

			if (hoverTab != null)
			{
				showTooltipTimer.Interval = hoverTab.Parent.Tooltip.AutomaticDelay;
				showTooltipTimer.Start();
			}
		}

		private bool CanDragSingleTabWindow(TitleBarTab tab, Point relativeCursor)
		{
			return tab != null && _parentForm.Tabs.Count == 1 && _tornTab == null &&
				_singleTabDragOwner == null &&
				!_parentForm.TabRenderer.IsOverCloseButton(tab, relativeCursor) &&
				!_parentForm.TabRenderer.IsOverAddButton(relativeCursor);
		}

		private void DragSingleTabWindow(TitleBarTab tab, Point cursorPosition)
		{
			HideTooltip();
			_parentForm.TabRenderer.IsTabRepositioning = false;
			_singleTabDragStart = cursorPosition;
			_singleTabDropTarget = null;
			_singleTabDragOwner = this;
			try
			{
				ReleaseCapture();
				int packedPoint = (cursorPosition.X & 0xffff) | ((cursorPosition.Y & 0xffff) << 16);
				SendWindowDragMessage(_parentForm.Handle, (int)WM.WM_NCLBUTTONDOWN,
					new IntPtr((int)HT.HTCAPTION), new IntPtr(packedPoint));
				_singleTabDragOwner = null;

				// Chromium also exits the native move loop before attaching to another strip.
				TitleBarTabs target = _singleTabDropTarget;
				_singleTabDropTarget = null;
				if (target == null || target.IsDisposed || target.Disposing || !target.Visible ||
					target.WindowState == FormWindowState.Minimized || target.Tabs.Count == 0 ||
					_parentForm.IsDisposed || _parentForm.Tabs.Count != 1 || !_parentForm.Tabs.Contains(tab))
					return;

				// The target must deselect its current tab when this one is inserted.
				tab.Active = false;
				tab.ClearSubscriptions();
				_parentForm.Tabs.Remove(tab);
				TitleBarTabsOverlay targetOverlay = target._overlay;
				target.TabRenderer.CombineTab(tab, targetOverlay.GetRelativeCursorPosition(_singleTabDropPoint));
				target.TabRenderer.Overlay_MouseDown(targetOverlay,
					new MouseEventArgs(MouseButtons.Left, 1, _singleTabDropPoint.X, _singleTabDropPoint.Y, 0));
				if ((Control.MouseButtons & MouseButtons.Left) == 0)
					target.TabRenderer.Overlay_MouseUp(targetOverlay,
						new MouseEventArgs(MouseButtons.Left, 1, _singleTabDropPoint.X, _singleTabDropPoint.Y, 0));
				target.Activate();
				_parentForm.Close();
			}
			finally
			{
				_singleTabDragOwner = null;
				// A target can disappear while the move loop exits, or closing can be cancelled.
				// In either case, do not leave a surviving source window hidden.
				if (_singleTabMergeHidden && !_parentForm.IsDisposed && !IsDisposed)
				{
					_parentForm.Show();
					if (!Visible) Show(_parentForm);
					SetWindowTransitionsEnabled(_parentForm, true);
					SetWindowTransitionsEnabled(this, true);
				}
				_singleTabMergeHidden = false;
			}
		}

		private void CheckSingleTabWindowDrop(Point cursorPosition)
		{
			if (_singleTabDragOwner != this || _singleTabDropTarget != null ||
				_parentForm.IsDisposed || _parentForm.Tabs.Count != 1)
				return;
			Size dragSize = SystemInformation.DragSize;
			if (Math.Abs(cursorPosition.X - _singleTabDragStart.X) < Math.Max(1, dragSize.Width / 2) &&
				Math.Abs(cursorPosition.Y - _singleTabDragStart.Y) < Math.Max(1, dragSize.Height / 2))
				return;
			_wasDragging = true;
			var context = _parentForm.ApplicationContext;
			if (context == null) return;
			TitleBarTabs target = context.OpenWindows.FirstOrDefault(window => window != _parentForm &&
				!window.IsDisposed && !window.Disposing && window.Visible &&
				window.WindowState != FormWindowState.Minimized && window.Tabs.Count > 0 &&
				window.TabDropArea.Contains(cursorPosition));
			if (target == null) return;
			_singleTabDropTarget = target;
			_singleTabDropPoint = cursorPosition;
			// Ending the Windows move loop can restore the source to its starting bounds.
			// Hide both native windows first, as Chromium does, so that restore and the
			// temporarily empty source are never presented during the handoff.
			_singleTabMergeHidden = true;
			SetWindowTransitionsEnabled(_parentForm, false);
			SetWindowTransitionsEnabled(this, false);
			Hide();
			_parentForm.Hide();
			// WM_CANCELMODE ends the move loop; the live tab is transferred after it returns.
			SendWindowDragMessage(_parentForm.Handle, 0x001F, IntPtr.Zero, IntPtr.Zero);
		}

		/// <summary>Moves a tab into a real window as soon as it leaves its current tab strip.</summary>
		private void CreateLiveTornTabWindow(Point cursorPosition)
		{
			TitleBarTab tab = _parentForm.SelectedTab;
			if (tab == null)
			{
				return;
			}

			Rectangle sourceTabArea = tab.Area;
			int sourceTabAreaWidth = Math.Max(1, _parentForm.TabRenderer.MaxTabArea.Width);
			Rectangle sourceDropArea = TabDropArea;
			bool preserveTabOffset = cursorPosition.X >= sourceDropArea.Left && cursorPosition.X < sourceDropArea.Right;

			Point relativeCursorPosition = GetRelativeCursorPosition(cursorPosition);
			Point dragClickOffset = _parentForm.TabRenderer.TabDragClickOffset ?? new Point(
				relativeCursorPosition.X - sourceTabArea.Left,
				relativeCursorPosition.Y - sourceTabArea.Top);
			Point cursorOffsetWithinTab = new Point(
				Math.Max(0, Math.Min(dragClickOffset.X, sourceTabArea.Width)),
				Math.Max(0, Math.Min(dragClickOffset.Y, sourceTabArea.Height)));

			Rectangle sourceBounds = _parentForm.WindowState == FormWindowState.Normal
				? _parentForm.Bounds
				: _parentForm.RestoreBounds;

			TitleBarTabs newWindow = (TitleBarTabs) Activator.CreateInstance(_parentForm.GetType());
			newWindow.StartPosition = FormStartPosition.Manual;
			newWindow.WindowState = FormWindowState.Normal;

			int width = Math.Max(newWindow.MinimumSize.Width, sourceBounds.Width);
			int height = Math.Max(newWindow.MinimumSize.Height, sourceBounds.Height);
			if (width <= 0 || height <= 0)
			{
				width = Math.Max(newWindow.MinimumSize.Width, _parentForm.Width);
				height = Math.Max(newWindow.MinimumSize.Height, _parentForm.Height);
			}

			_tornTabWindowCursorOffset = new Point(
				Math.Max(0, Math.Min(cursorPosition.X - _parentForm.Left, Math.Max(0, width - 100))),
				Math.Max(0, Math.Min(cursorPosition.Y - _parentForm.Top, _parentForm.NonClientAreaHeight)));
			Rectangle draggedWindowBounds = new Rectangle(
				cursorPosition.X - _tornTabWindowCursorOffset.X,
				cursorPosition.Y - _tornTabWindowCursorOffset.Y,
				width,
				height);
			newWindow.Bounds = draggedWindowBounds;

			_tornTab = tab;
			_tornTabWindow = newWindow;
			_tornTabDragOwner = this;
			Interlocked.Exchange(ref _tornTabMoveQueued, 0);

			SetWindowTransitionsEnabled(newWindow, false);
			_parentForm.ApplicationContext.OpenWindow(newWindow);
			newWindow.Show();
			newWindow.WindowState = FormWindowState.Normal;
			newWindow.Bounds = draggedWindowBounds;

			tab.ClearSubscriptions();
			_parentForm.SelectedTabIndex = _parentForm.SelectedTabIndex == _parentForm.Tabs.Count - 1
				? _parentForm.SelectedTabIndex - 1
				: _parentForm.SelectedTabIndex + 1;
			_parentForm.Tabs.Remove(tab);

			tab.Parent = newWindow;
			newWindow.Tabs.Add(tab);
			newWindow.SelectedTabIndex = 0;
			newWindow.ResizeTabContents();
			newWindow.RedrawTabs();

			int normalTabX = tab.Area.Left;
			int targetTabAreaWidth = Math.Max(1, newWindow.TabRenderer.MaxTabArea.Width);
			int detachedTabWidth = Math.Max(1, Math.Min(sourceTabArea.Width, targetTabAreaWidth));
			int detachedTabX = normalTabX;

			// Chrome keeps the current tab offset when detaching above or below the strip,
			// but starts at the beginning when detaching from either side.
			if (preserveTabOffset)
			{
				detachedTabX = sourceTabArea.Left;
				if (targetTabAreaWidth < sourceTabAreaWidth)
				{
					int sourceLeadingSpace = Math.Max(0, sourceTabArea.Left - normalTabX);
					detachedTabX = normalTabX + Convert.ToInt32(
						Math.Round(sourceLeadingSpace * (targetTabAreaWidth / Convert.ToDouble(sourceTabAreaWidth))));
				}

				int maximumTabX = normalTabX + Math.Max(0, targetTabAreaWidth - detachedTabWidth);
				detachedTabX = Math.Max(normalTabX, Math.Min(detachedTabX, maximumTabX));
			}

			int detachedTabCursorOffsetX = Math.Min(cursorOffsetWithinTab.X, detachedTabWidth);
			newWindow.TabRenderer.BeginDetachedWindowDrag(detachedTabX, detachedTabWidth);
			_tornTabWindowCursorOffset = new Point(
				(newWindow._overlay.Left - newWindow.Left) + detachedTabX + detachedTabCursorOffsetX,
				(newWindow._overlay.Top - newWindow.Top) + tab.Area.Top + cursorOffsetWithinTab.Y);
			MoveLiveTornTabWindow(cursorPosition);
			newWindow.RedrawTabs();

			if (_parentForm.Tabs.Count == 0)
			{
				_parentForm.Hide();
			}

			_dropAreas = (from window in _parentForm.ApplicationContext.OpenWindows
						  where window != newWindow && window.Tabs.Count > 0
						  select new Tuple<TitleBarTabs, Rectangle>(window, window.TabDropArea)).ToArray();
		}

		/// <summary>Keeps the live torn-tab window underneath the cursor.</summary>
		private void MoveLiveTornTabWindow(Point cursorPosition)
		{
			if (_tornTabWindow == null || _tornTabWindow.IsDisposed)
			{
				return;
			}

			_tornTabWindow.Location = new Point(
				cursorPosition.X - _tornTabWindowCursorOffset.X,
				cursorPosition.Y - _tornTabWindowCursorOffset.Y);
		}

		/// <summary>Moves the live window asynchronously while discarding superseded mouse positions.</summary>
		private void QueueLiveTornTabWindowMove(Point cursorPosition)
		{
			Interlocked.Exchange(ref _latestTornTabCursorX, cursorPosition.X);
			Interlocked.Exchange(ref _latestTornTabCursorY, cursorPosition.Y);

			if (Interlocked.CompareExchange(ref _tornTabMoveQueued, 1, 0) != 0)
			{
				return;
			}

			TitleBarTabs window = _tornTabWindow;
			if (window == null || window.IsDisposed || !window.IsHandleCreated)
			{
				Interlocked.Exchange(ref _tornTabMoveQueued, 0);
				return;
			}

			try
			{
				window.BeginInvoke(new Action(ProcessQueuedTornTabWindowMove));
			}
			catch (InvalidOperationException)
			{
				Interlocked.Exchange(ref _tornTabMoveQueued, 0);
			}
		}

		/// <summary>Applies one current cursor position and schedules another only if it changed meanwhile.</summary>
		private void ProcessQueuedTornTabWindowMove()
		{
			int cursorX = Interlocked.CompareExchange(ref _latestTornTabCursorX, 0, 0);
			int cursorY = Interlocked.CompareExchange(ref _latestTornTabCursorY, 0, 0);

			MoveLiveTornTabWindow(new Point(cursorX, cursorY));
			Interlocked.Exchange(ref _tornTabMoveQueued, 0);

			int latestX = Interlocked.CompareExchange(ref _latestTornTabCursorX, 0, 0);
			int latestY = Interlocked.CompareExchange(ref _latestTornTabCursorY, 0, 0);
			if (_tornTab != null && (latestX != cursorX || latestY != cursorY))
			{
				QueueLiveTornTabWindowMove(new Point(latestX, latestY));
			}
		}

		/// <summary>Consumer method that processes mouse events in <see cref="_mouseEvents" /> that are recorded by <see cref="MouseHookCallback" />.</summary>
		protected void InterpretMouseEvents()
		{
			foreach (MouseEvent mouseEvent in _mouseEvents.GetConsumingEnumerable())
			{
				int nCode = mouseEvent.nCode;
				IntPtr wParam = mouseEvent.wParam;

				if (_singleTabDragOwner != null)
				{
					if (_singleTabDragOwner == this && nCode >= 0 && (int)wParam == (int)WM.WM_MOUSEMOVE)
					{
						Point nativeCursor = Cursor.Position;
						Invoke(new Action(() => CheckSingleTabWindowDrop(nativeCursor)));
					}
					continue;
				}

				if (nCode >= 0 && (int) WM.WM_MOUSEMOVE == (int) wParam)
				{
					// Hook points are physical pixels. Use the same DPI-virtualized screen
					// coordinates as mouse-down events, overlay bounds and tab drop areas.
					Point cursorPosition = Cursor.Position;
					bool reRender = _parentForm.TabRenderer.RequiresHoverRedraw(GetRelativeCursorPosition(cursorPosition));

					if (_tornTab != null && _tornTabDragOwner != this)
					{
						continue;
					}

					if (_tornTab != null && _dropAreas != null)
					{
						QueueLiveTornTabWindowMove(cursorPosition);

						// ReSharper disable ForCanBeConvertedToForeach
						for (int i = 0; i < _dropAreas.Length; i++)
						// ReSharper restore ForCanBeConvertedToForeach
						{
							// If the cursor is within the drop area, combine the tab for the window that belongs to that drop area
							if (_dropAreas[i].Item2.Contains(cursorPosition))
							{
								TitleBarTab tabToCombine = null;

								lock (_tornTabLock)
								{
									if (_tornTab != null)
									{
										tabToCombine = _tornTab;
										_tornTab = null;
									}
								}

								if (tabToCombine != null)
								{
									TitleBarTabs targetWindow = _dropAreas[i].Item1;
									TitleBarTabs tornTabWindow = _tornTabWindow;
									_tornTabWindow = null;
									_tornTabDragOwner = null;
									_dropAreas = null;
									Interlocked.Exchange(ref _tornTabMoveQueued, 0);

									// In all cases where we need to affect the UI, we call Invoke so that those changes are made on the main UI thread since
									// we are on a separate processing thread in this case
									Invoke(
										new Action(
											() =>
											{
												tabToCombine.ClearSubscriptions();
												if (tornTabWindow != null && !tornTabWindow.IsDisposed)
												{
													tornTabWindow.Tabs.Remove(tabToCombine);
												}

												targetWindow.TabRenderer.CombineTab(tabToCombine, cursorPosition);

												if (tornTabWindow != null && !tornTabWindow.IsDisposed)
												{
													tornTabWindow.Close();
												}

												if (_parentForm.Tabs.Count == 0)
												{
													_parentForm.Close();
												}
											}));

									break;
								}
							}
						}

						// A live cross-window drag has its own move path. Do not also run the old
						// window's synchronous hover and redraw path for the same mouse event.
						continue;
					}

					else if (!_parentForm.TabRenderer.IsTabRepositioning)
					{
						HideTooltip();
						StartTooltipTimer();

                        Point relativeCursorPosition = GetRelativeCursorPosition(cursorPosition);

                        // If we were over a close button previously, check to see if the cursor is still over that tab's
                        // close button; if not, re-render
                        if (_isOverCloseButtonForTab != -1 &&
							(_isOverCloseButtonForTab >= _parentForm.Tabs.Count ||
							!_parentForm.TabRenderer.IsOverCloseButton(_parentForm.Tabs[_isOverCloseButtonForTab], relativeCursorPosition)))
						{
							reRender = true;
							_isOverCloseButtonForTab = -1;
						}

						// Otherwise, see if any tabs' close button is being hovered over
						else
						{
                            // ReSharper disable ForCanBeConvertedToForeach
                            for (int i = 0; i < _parentForm.Tabs.Count; i++)
							// ReSharper restore ForCanBeConvertedToForeach
							{
								if (_parentForm.TabRenderer.IsOverCloseButton(_parentForm.Tabs[i], relativeCursorPosition))
								{
									_isOverCloseButtonForTab = i;
									reRender = true;

									break;
								}
							}
						}

                        if (_isOverCloseButtonForTab == -1 && _parentForm.TabRenderer.RendersEntireTitleBar)
                        {
                            if (_parentForm.TabRenderer.IsOverSizingBox(relativeCursorPosition))
                            {
                                _isOverSizingBox = true;
                                reRender = true;
                            }

                            else if (_isOverSizingBox)
                            {
                                _isOverSizingBox = false;
                                reRender = true;
                            }
                        }

                        if (_parentForm.TabRenderer.IsOverAddButton(relativeCursorPosition))
                        {
                            _isOverAddButton = true;
                            reRender = true;
                        }

                        else if (_isOverAddButton)
                        {
                            _isOverAddButton = false;
                            reRender = true;
                        }
                    }

					else
					{
						Invoke(
							new Action(
								() =>
								{
									_wasDragging = true;

									// When determining if a tab has been torn from the window while dragging, we take the drop area for this window and inflate it by the
									// TabTearDragDistance setting
									Rectangle dragArea = TabDropArea;
									dragArea.Inflate(_parentForm.TabRenderer.TabTearDragDistance, _parentForm.TabRenderer.TabTearDragDistance);

									// If the cursor is outside the tear area, tear it away from the current window
									if (!dragArea.Contains(cursorPosition) && _tornTab == null)
									{
										lock (_tornTabLock)
										{
									if (_tornTab == null)
									{
										_parentForm.TabRenderer.IsTabRepositioning = false;
										CreateLiveTornTabWindow(cursorPosition);
									}
								}
							}
								}));
					}

					Invoke(new Action(() => OnMouseMove(new MouseEventArgs(MouseButtons.None, 0, cursorPosition.X, cursorPosition.Y, 0))));

					if (_parentForm.TabRenderer.IsTabRepositioning)
					{
						reRender = true;
					}

					if (reRender)
					{
						Invoke(new Action(() => Render(cursorPosition, true)));
					}
				}

                else if (nCode >= 0 && (int) WM.WM_LBUTTONDBLCLK == (int) wParam)
                {
					if (DesktopBounds.Contains(_lastTwoClickCoordinates[0]) && DesktopBounds.Contains(_lastTwoClickCoordinates[1]))
					{
						Invoke(new Action(() =>
						{
							_parentForm.WindowState = _parentForm.WindowState == FormWindowState.Maximized
							? FormWindowState.Normal
							: FormWindowState.Maximized;
						}));
					}
                }

				else if (nCode >= 0 && (int) WM.WM_LBUTTONDOWN == (int) wParam)
				{
					if (!_firstClick)
                    {
						_lastTwoClickCoordinates[1] = _lastTwoClickCoordinates[0];
                    }

					_lastTwoClickCoordinates[0] = Cursor.Position;

					_firstClick = false;
					_wasDragging = false;
				}

				else if (nCode >= 0 && (int) WM.WM_LBUTTONUP == (int) wParam)
				{
					if (_tornTab != null && _tornTabDragOwner != this)
					{
						continue;
					}

					// The torn tab is already in a live window; releasing the mouse simply finishes the drag.
					if (_tornTab != null)
					{
						TitleBarTab tabToRelease = null;

						lock (_tornTabLock)
						{
							if (_tornTab != null)
							{
								tabToRelease = _tornTab;
								_tornTab = null;
							}
						}

						if (tabToRelease != null)
						{
							TitleBarTabs releasedWindow = _tornTabWindow;
							_tornTabWindow = null;
							_tornTabDragOwner = null;
							_dropAreas = null;
							Interlocked.Exchange(ref _tornTabMoveQueued, 0);

							Invoke(
								new Action(
									() =>
									{
										if (releasedWindow != null && !releasedWindow.IsDisposed)
										{
											releasedWindow.TabRenderer.EndDetachedWindowDrag();
											releasedWindow.Activate();
											releasedWindow.RedrawTabs();
											SetWindowTransitionsEnabled(releasedWindow, true);
										}

										if (_parentForm.Tabs.Count == 0)
										{
											_parentForm.Close();
										}
									}));
						}
					}

					Invoke(new Action(() => OnMouseUp(new MouseEventArgs(MouseButtons.Left, 1, Cursor.Position.X, Cursor.Position.Y, 0))));
				}
			}
		}

		/// <summary>Hook callback to process <see cref="WM.WM_MOUSEMOVE" /> messages to highlight/un-highlight the close button on each tab.</summary>
		/// <param name="nCode">The message being received.</param>
		/// <param name="wParam">Additional information about the message.</param>
		/// <param name="lParam">Additional information about the message.</param>
		/// <returns>A zero value if the procedure processes the message; a nonzero value if the procedure ignores the message.</returns>
		protected IntPtr MouseHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
		{
			MouseEvent mouseEvent = new MouseEvent
			{
				nCode = nCode,
				wParam = wParam,
				lParam = lParam
			};

			if (nCode >= 0 && (int) WM.WM_MOUSEMOVE == (int) wParam)
			{
				mouseEvent.MouseData = (MSLLHOOKSTRUCT) Marshal.PtrToStructure(lParam, typeof (MSLLHOOKSTRUCT));
			}

			// Rendering a drag can take longer than Windows' mouse sampling interval.
			// Keep at most one move waiting so stale positions are not replayed later.
			if ((int) WM.WM_MOUSEMOVE != (int) wParam || _mouseEvents.Count == 0)
			{
				_mouseEvents.Add(mouseEvent);
			}

            if (nCode >= 0 && (int) WM.WM_LBUTTONDOWN == (int) wParam)
            {
                long currentTicks = DateTime.Now.Ticks;

                if (_lastLeftButtonClickTicks > 0 && currentTicks - _lastLeftButtonClickTicks < _doubleClickInterval * 10000)
                {
                    _mouseEvents.Add(new MouseEvent
                    {
                        nCode = nCode,
                        wParam = new IntPtr((int) WM.WM_LBUTTONDBLCLK),
                        lParam = lParam
                    });
                }

                _lastLeftButtonClickTicks = currentTicks;
            }

			return User32.CallNextHookEx(_hookId, nCode, wParam, lParam);
		}

		/// <summary>Draws the titlebar background behind the tabs if Aero glass is not enabled.</summary>
		/// <param name="graphics">Graphics context with which to draw the background.</param>
		protected virtual void DrawTitleBarBackground(Graphics graphics)
		{
			if (DisplayType == DisplayType.Aero)
			{
				return;
			}

			Rectangle fillArea;

			if (DisplayType == DisplayType.Basic)
			{
				fillArea = new Rectangle(
					new Point(
						1, Top == 0
							? SystemInformation.CaptionHeight - 1
							: (SystemInformation.CaptionHeight + SystemInformation.VerticalResizeBorderThickness) - (Top - _parentForm.Top) - 1),
					new Size(Width - 2, _parentForm.Padding.Top));
			}

			else
			{
				fillArea = new Rectangle(new Point(1, 0), new Size(Width - 2, Height - 1));
			}

			if (fillArea.Height <= 0)
			{
				return;
			}

			// Adjust the margin so that the gradient stops immediately prior to the control box in the titlebar
			int rightMargin = 3;

			if (_parentForm.ControlBox && _parentForm.MinimizeBox)
			{
				rightMargin += SystemInformation.CaptionButtonSize.Width;
			}

			if (_parentForm.ControlBox && _parentForm.MaximizeBox)
			{
				rightMargin += SystemInformation.CaptionButtonSize.Width;
			}

			if (_parentForm.ControlBox)
			{
				rightMargin += SystemInformation.CaptionButtonSize.Width;
			}

			using (LinearGradientBrush gradient = new LinearGradientBrush(
				new Point(24, 0), new Point(fillArea.Width - rightMargin + 1, 0), TitleBarColor, TitleBarGradientColor))
			using (SolidBrush backgroundBrush = new SolidBrush(TitleBarColor))
			using (SolidBrush gradientBrush = new SolidBrush(TitleBarGradientColor))
			using (BufferedGraphics bufferedGraphics = BufferedGraphicsManager.Current.Allocate(graphics, fillArea))
			{
				bufferedGraphics.Graphics.FillRectangle(backgroundBrush, fillArea);
				bufferedGraphics.Graphics.FillRectangle(
					gradientBrush,
					new Rectangle(new Point(fillArea.Location.X + fillArea.Width - rightMargin, fillArea.Location.Y), new Size(rightMargin, fillArea.Height)));
				bufferedGraphics.Graphics.FillRectangle(
					gradient, new Rectangle(fillArea.Location, new Size(fillArea.Width - rightMargin, fillArea.Height)));
				bufferedGraphics.Graphics.FillRectangle(backgroundBrush, new Rectangle(fillArea.Location, new Size(24, fillArea.Height)));

				bufferedGraphics.Render(graphics);
			}
		}

		/// <summary>
		/// Event handler that is called when <see cref="_parentForm" />'s <see cref="Control.SystemColorsChanged" /> event is fired which re-renders
		/// the tabs.
		/// </summary>
		/// <param name="sender">Object from which the event originated.</param>
		/// <param name="e">Arguments associated with the event.</param>
		private void _parentForm_SystemColorsChanged(object sender, EventArgs e)
		{
			_aeroEnabled = _parentForm.IsCompositionEnabled;
			OnPosition();
		}

		/// <summary>
		/// Event handler that is called when <see cref="_parentForm" />'s <see cref="Control.SizeChanged" />, <see cref="Control.VisibleChanged" />, or
		/// <see cref="Control.Move" /> events are fired which re-renders the tabs.
		/// </summary>
		/// <param name="sender">Object from which the event originated.</param>
		/// <param name="e">Arguments associated with the event.</param>
		private void _parentForm_Refresh(object sender, EventArgs e)
		{
			UpdateLoadingAnimation();
			if (_parentForm.WindowState == FormWindowState.Minimized)
			{
				Visible = false;
			}

			else
			{
				OnPosition();
			}
		}

		/// <summary>Sets the position of the overlay window to match that of <see cref="_parentForm" /> so that it moves in tandem with it.</summary>
		protected void OnPosition()
		{
			if (!IsDisposed)
			{
				// 92 is SM_CXPADDEDBORDER, which returns the amount of extra border padding around captioned windows
				int borderPadding = DisplayType == DisplayType.Classic
					? 0
					: User32.GetSystemMetrics(92);

				// If the form is in a non-maximized state, we position the tabs below the minimize/maximize/close
				// buttons
				Top = _parentForm.Top + (DisplayType == DisplayType.Classic
					? SystemInformation.VerticalResizeBorderThickness
					: _parentForm.WindowState == FormWindowState.Maximized
						? SystemInformation.VerticalResizeBorderThickness + borderPadding
						: _parentForm.TabRenderer.RendersEntireTitleBar 
                            ? _parentForm.TabRenderer.IsWindows10
								? SystemInformation.BorderSize.Width
								: 0
							: borderPadding);
				Left = _parentForm.Left + SystemInformation.HorizontalResizeBorderThickness - (_parentForm.TabRenderer.IsWindows10 ? 0 : SystemInformation.BorderSize.Width) + borderPadding;
				Width = _parentForm.Width - ((SystemInformation.VerticalResizeBorderThickness + borderPadding) * 2) + (_parentForm.TabRenderer.IsWindows10 ? 0 : (SystemInformation.BorderSize.Width * 2));
				Height = _parentForm.TabRenderer.TabHeight + (DisplayType == DisplayType.Classic && _parentForm.WindowState != FormWindowState.Maximized && !_parentForm.TabRenderer.RendersEntireTitleBar
					? SystemInformation.CaptionButtonSize.Height
					: _parentForm.TabRenderer.IsWindows10
						? -1 * SystemInformation.BorderSize.Width
						: _parentForm.WindowState != FormWindowState.Maximized
							? borderPadding
							: 0);

				Render();
			}
		}

		/// <summary>
		/// Renders the tabs and then calls <see cref="User32.UpdateLayeredWindow" /> to blend the tab content with the underlying window (
		/// <see cref="_parentForm" />).
		/// </summary>
		/// <param name="forceRedraw">Flag indicating whether a full render should be forced.</param>
		public void Render(bool forceRedraw = false)
		{
			Render(Cursor.Position, forceRedraw);
		}

		/// <summary>
		/// Renders the tabs and then calls <see cref="User32.UpdateLayeredWindow" /> to blend the tab content with the underlying window (
		/// <see cref="_parentForm" />).
		/// </summary>
		/// <param name="cursorPosition">Current position of the cursor.</param>
		/// <param name="forceRedraw">Flag indicating whether a full render should be forced.</param>
		public void Render(Point cursorPosition, bool forceRedraw = false)
		{
			UpdateLoadingAnimation();
			if (!IsDisposed && _parentForm.TabRenderer != null && _parentForm.WindowState != FormWindowState.Minimized && _parentForm.ClientRectangle.Width > 0)
			{
				cursorPosition = GetRelativeCursorPosition(cursorPosition);

				using (Bitmap bitmap = new Bitmap(Width, Height, PixelFormat.Format32bppArgb))
				{
					using (Graphics graphics = Graphics.FromImage(bitmap))
					{
						DrawTitleBarBackground(graphics);

						// Since classic mode themes draw over the *entire* titlebar, not just the area immediately behind the tabs, we have to offset the tabs
						// when rendering in the window
						Point offset = _parentForm.WindowState != FormWindowState.Maximized && DisplayType == DisplayType.Classic && !_parentForm.TabRenderer.RendersEntireTitleBar
							? new Point(0, SystemInformation.CaptionButtonSize.Height)
							: _parentForm.WindowState != FormWindowState.Maximized && !_parentForm.TabRenderer.RendersEntireTitleBar
                                ? new Point(0, SystemInformation.VerticalResizeBorderThickness - SystemInformation.BorderSize.Height)
								: new Point(0, 0);

						// Render the tabs into the bitmap
						_parentForm.TabRenderer.Render(_parentForm.Tabs, graphics, offset, cursorPosition, forceRedraw);
						UpdateLoadingAnimation();

						// Cut out a hole in the background so that the control box on the underlying window can be shown
						if (DisplayType == DisplayType.Classic && (_parentForm.ControlBox || _parentForm.MaximizeBox || _parentForm.MinimizeBox))
						{
							int boxWidth = 0;

							if (_parentForm.ControlBox)
							{
								boxWidth += SystemInformation.CaptionButtonSize.Width;
							}

							if (_parentForm.MinimizeBox)
							{
								boxWidth += SystemInformation.CaptionButtonSize.Width;
							}

							if (_parentForm.MaximizeBox)
							{
								boxWidth += SystemInformation.CaptionButtonSize.Width;
							}

							CompositingMode oldCompositingMode = graphics.CompositingMode;

							graphics.CompositingMode = CompositingMode.SourceCopy;
							graphics.FillRectangle(
								Brushes.Transparent, Width - boxWidth, 0, boxWidth, SystemInformation.CaptionButtonSize.Height);
							graphics.CompositingMode = oldCompositingMode;
						}

						IntPtr screenDc = User32.GetDC(IntPtr.Zero);
						IntPtr memDc = Gdi32.CreateCompatibleDC(screenDc);
						IntPtr oldBitmap = IntPtr.Zero;
						IntPtr bitmapHandle = IntPtr.Zero;

						try
						{
							// Copy the contents of the bitmap into memDc
							bitmapHandle = bitmap.GetHbitmap(Color.FromArgb(0));
							oldBitmap = Gdi32.SelectObject(memDc, bitmapHandle);

							SIZE size = new SIZE
							{
								cx = bitmap.Width,
								cy = bitmap.Height
							};

							POINT pointSource = new POINT
							{
								x = 0,
								y = 0
							};
							POINT topPos = new POINT
							{
								x = Left,
								y = Top
							};
							BLENDFUNCTION blend = new BLENDFUNCTION
							{
								// We want to blend the bitmap's content with the screen content under it
								BlendOp = Convert.ToByte((int) AC.AC_SRC_OVER),
								BlendFlags = 0,
								// Follow the parent forms' opacity level
								SourceConstantAlpha = (byte)(_parentForm.Opacity * 255),
								// We use the bitmap's alpha channel for blending instead of a pre-defined transparency key
								AlphaFormat = Convert.ToByte((int) AC.AC_SRC_ALPHA)
							};

							// Blend the tab content with the underlying content
							if (!User32.UpdateLayeredWindow(
								Handle, screenDc, ref topPos, ref size, memDc, ref pointSource, 0, ref blend, ULW.ULW_ALPHA))
							{
								int error = Marshal.GetLastWin32Error();
								throw new Win32Exception(error, "Error while calling UpdateLayeredWindow().");
							}
						}

						// Clean up after ourselves
						finally
						{
							User32.ReleaseDC(IntPtr.Zero, screenDc);

							if (bitmapHandle != IntPtr.Zero)
							{
								Gdi32.SelectObject(memDc, oldBitmap);
								Gdi32.DeleteObject(bitmapHandle);
							}

							Gdi32.DeleteDC(memDc);
						}
					}
				}
			}
		}

		/// <summary>Gets the relative location of the cursor within the overlay.</summary>
		/// <param name="cursorPosition">Cursor position that represents the absolute position of the cursor on the screen.</param>
		/// <returns>The relative location of the cursor within the overlay.</returns>
		public Point GetRelativeCursorPosition(Point cursorPosition)
		{
			return new Point(cursorPosition.X - Location.X, cursorPosition.Y - Location.Y);
		}

		/// <summary>Overrides the message pump for the window so that we can respond to click events on the tabs themselves.</summary>
		/// <param name="m">Message received by the pump.</param>
		protected override void WndProc(ref Message m)
		{
            // Detect any sort of mouse click
            if (m.Msg == (int)WM.WM_LBUTTONDOWN ||
                m.Msg == (int)WM.WM_LBUTTONUP ||
                m.Msg == (int)WM.WM_LBUTTONDBLCLK ||
                m.Msg == (int)WM.WM_NCLBUTTONDOWN ||
                m.Msg == (int)WM.WM_NCLBUTTONUP ||
                m.Msg == (int)WM.WM_MBUTTONDOWN ||
                m.Msg == (int)WM.WM_MBUTTONUP ||
                m.Msg == (int)WM.WM_NCMBUTTONDOWN ||
                m.Msg == (int)WM.WM_NCMBUTTONUP ||
                m.Msg == (int)WM.WM_RBUTTONDOWN ||
                m.Msg == (int)WM.WM_RBUTTONUP ||
                m.Msg == (int)WM.WM_NCRBUTTONDOWN ||
                m.Msg == (int)WM.WM_NCRBUTTONUP)
            {
				if(!_active)
				{
					_parentForm.Activate();
				}
            }

            switch ((WM) m.Msg)
			{
				case WM.WM_SYSCOMMAND:
					if (m.WParam == new IntPtr(0xF030) || m.WParam == new IntPtr(0xF120) || m.WParam == new IntPtr(0xF020))
					{
						_parentForm.ForwardMessage(ref m);
					}

					else
                    {
						base.WndProc(ref m);
                    }

					break;

				case WM.WM_NCLBUTTONDOWN:
				case WM.WM_LBUTTONDOWN:
					Point relativeCursorPosition = GetRelativeCursorPosition(Cursor.Position);
					_parentForm.TabRenderer.ButtonPointerDown(relativeCursorPosition);

					// If we were over a tab, set the capture state for the window so that we'll actually receive a WM_LBUTTONUP message
					if (_parentForm.TabRenderer.OverTab(_parentForm.Tabs, relativeCursorPosition) == null &&
						!_parentForm.TabRenderer.IsOverAddButton(relativeCursorPosition))
					{
						_parentForm.ForwardMessage(ref m);
					}

					else
					{
						// When the user clicks a mouse button, save the tab that the user was over so we can respond properly when the mouse button is released
						TitleBarTab clickedTab = _parentForm.TabRenderer.OverTab(_parentForm.Tabs, relativeCursorPosition);

						if (clickedTab != null)
						{
							if (CanDragSingleTabWindow(clickedTab, relativeCursorPosition))
							{
								DragSingleTabWindow(clickedTab, Cursor.Position);
								return;
							}

							// If the user clicked the close button, remove the tab from the list
							if (!_parentForm.TabRenderer.IsOverCloseButton(clickedTab, relativeCursorPosition))
							{
								_parentForm.ResizeTabContents(clickedTab);
								_parentForm.SelectedTabIndex = _parentForm.Tabs.IndexOf(clickedTab);

								Render();
							}

							OnMouseDown(new MouseEventArgs(MouseButtons.Left, 1, Cursor.Position.X, Cursor.Position.Y, 0));
						}

						_parentForm.Activate();
					}

					break;

				case WM.WM_LBUTTONDBLCLK:
					_parentForm.ForwardMessage(ref m);
					break;

				// We always return HTCAPTION for the hit test message so that the underlying window doesn't have its focus removed
				case WM.WM_NCHITTEST:
					m.Result = new IntPtr((int) _parentForm.TabRenderer.NonClientHitTest(m, GetRelativeCursorPosition(Cursor.Position)));
					break;

				case WM.WM_LBUTTONUP:
				case WM.WM_NCLBUTTONUP:
				case WM.WM_MBUTTONUP:
				case WM.WM_NCMBUTTONUP:
					Point relativeCursorPosition2 = GetRelativeCursorPosition(Cursor.Position);

					if (_parentForm.TabRenderer.OverTab(_parentForm.Tabs, relativeCursorPosition2) == null &&
						!_parentForm.TabRenderer.IsOverAddButton(relativeCursorPosition2))
					{
						_parentForm.ForwardMessage(ref m);
					}

					else
					{
						// When the user clicks a mouse button, save the tab that the user was over so we can respond properly when the mouse button is released
						TitleBarTab clickedTab = _parentForm.TabRenderer.OverTab(_parentForm.Tabs, relativeCursorPosition2);

						if (clickedTab != null)
						{
							// If the user clicks the middle button/scroll wheel over a tab, close it
							if ((WM) m.Msg == WM.WM_MBUTTONUP || (WM) m.Msg == WM.WM_NCMBUTTONUP)
							{
								clickedTab.Content.Close();
								Render();
							}

							else
							{
								// If the user clicked the close button, remove the tab from the list
								if (_parentForm.TabRenderer.IsOverCloseButton(clickedTab, relativeCursorPosition2))
								{
									clickedTab.Content.Close();
									Render();
								}

								else
								{
									_parentForm.OnTabClicked(
										new TitleBarTabEventArgs
										{
											Tab = clickedTab,
											TabIndex = _parentForm.SelectedTabIndex,
											Action = TabControlAction.Selected,
											WasDragging = _wasDragging
										});
								}
							}
						}

						// Otherwise, if the user clicked the add button, call CreateTab to add a new tab to the list and select it
						else if (_parentForm.TabRenderer.IsOverAddButton(relativeCursorPosition2))
						{
							_parentForm.AddNewTab();
						}

						if ((WM) m.Msg == WM.WM_LBUTTONUP || (WM) m.Msg == WM.WM_NCLBUTTONUP)
						{
							OnMouseUp(new MouseEventArgs(MouseButtons.Left, 1, Cursor.Position.X, Cursor.Position.Y, 0));
						}
					}

					break;

				case WM.WM_NCRBUTTONDOWN:
				case WM.WM_RBUTTONDOWN:
					Point _relativeCursorPosition = GetRelativeCursorPosition(Cursor.Position);
					TitleBarTab _clickedTab = _parentForm.TabRenderer.OverTab(_parentForm.Tabs, _relativeCursorPosition);

					if (_clickedTab != null)
					{
						if (!_parentForm.TabRenderer.IsOverCloseButton(_clickedTab, _relativeCursorPosition))
						{
                            ContextMenuProvider._parentForm = _parentForm;
							ContextMenuProvider._clickedTab = _clickedTab;

                            Point cursorPos = Control.MousePosition;
                            ContextMenuProvider._contextMenuStripTab.Show(cursorPos);
                        }
					}
					else
					{
						if (!_parentForm.TabRenderer.IsOverAddButton(_relativeCursorPosition))
						{
                            ContextMenuProvider._parentForm = _parentForm;
                            ContextMenuProvider._clickedTab = _clickedTab;

                            Point cursorPos = Control.MousePosition;
                            ContextMenuProvider._contextMenuStripNormal.Show(cursorPos);                     
						}
                    }

					break;

                default:
					base.WndProc(ref m);
					break;
			}
		}

		/// <summary>Event handler that is called when <see cref="_parentForm" />'s <see cref="Form.Activated" /> event is fired.</summary>
		/// <param name="sender">Object from which this event originated.</param>
		/// <param name="e">Arguments associated with the event.</param>
		private void _parentForm_Activated(object sender, EventArgs e)
		{
			_active = true;
			Render();
		}

		/// <summary>Event handler that is called when <see cref="_parentForm" />'s <see cref="Form.Deactivate" /> event is fired.</summary>
		/// <param name="sender">Object from which this event originated.</param>
		/// <param name="e">Arguments associated with the event.</param>
		private void _parentForm_Deactivate(object sender, EventArgs e)
		{
			_active = false;
			Render();
		}

		/// <summary>Event handler that is called when <see cref="_parentForm" />'s <see cref="Component.Disposed" /> event is fired.</summary>
		/// <param name="sender">Object from which this event originated.</param>
		/// <param name="e">Arguments associated with the event.</param>
		private void _parentForm_Disposed(object sender, EventArgs e)
		{
			_loadingAnimationTimer?.Dispose();
			_loadingAnimationTimer = null;
		}

		/// <summary>
		/// Contains information on mouse events captured by <see cref="TitleBarTabsOverlay.MouseHookCallback" /> and processed by
		/// <see cref="TitleBarTabsOverlay.InterpretMouseEvents" />.
		/// </summary>
		protected class MouseEvent
		{
			/// <summary>Code for the event.</summary>
			// ReSharper disable InconsistentNaming
			public int nCode
			{
				get;
				set;
			}

			/// <summary>wParam value associated with the event.</summary>
			public IntPtr wParam
			{
				get;
				set;
			}

			/// <summary>lParam value associated with the event.</summary>
			public IntPtr lParam
			{
				get;
				set;
			}

			// ReSharper restore InconsistentNaming

			/// <summary>Data associated with the mouse event.</summary>
			public MSLLHOOKSTRUCT? MouseData
			{
				get;
				set;
			}
		}

		private void InitializeComponent()
		{
			this.SuspendLayout();
			// 
			// TitleBarTabsOverlay
			// 
			this.ClientSize = new System.Drawing.Size(284, 261);
			this.Name = "TitleBarTabsOverlay";
			this.ResumeLayout(false);

		}
	}
}
