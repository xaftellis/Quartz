using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;

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

		private delegate bool EnumWindowCallback(IntPtr window, IntPtr parameter);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool EnumWindows(EnumWindowCallback callback, IntPtr parameter);

		private TabFrameScheduler _loadingAnimationTimer;
		private readonly LayeredWindowBuffer _surface = new LayeredWindowBuffer();
		private bool _renderPending, _forceRenderPending, _rendering;

		// Called on the UI thread. Mouse events share the animation clock instead
		// of presenting another full frame between every pair of timer ticks.
		internal void RequestRender(bool forceRedraw = false)
		{
			_renderPending = true;
			_forceRenderPending |= forceRedraw;
			UpdateLoadingAnimation();
		}

		private void UpdateLoadingAnimation()
		{
			if (_loadingAnimationTimer == null) return;
			_loadingAnimationTimer.Enabled = !IsDisposed && !Disposing && !_parentForm.IsDisposed &&
				!_parentForm.Disposing && (_parentForm.Visible || _tornTabDragOwner == this) &&
				_parentForm.WindowState != FormWindowState.Minimized &&
				(_renderPending || _mouseMovePending || _parentForm.Tabs.Any(tab => tab.IsLoading && !tab.Content.IsDisposed) ||
					(_parentForm.TabRenderer != null && _parentForm.TabRenderer.IsLayoutAnimating));
		}

		private void LoadingAnimation_Tick(object sender, EventArgs e)
		{
			ProcessPendingMouseMove();
			UpdateLoadingAnimation();
			// Render samples the latest pointer once for the shared frame. The timer
			// stops when loading, animations and pending mouse feedback are all idle.
			if (_loadingAnimationTimer != null && _loadingAnimationTimer.Enabled) Render();
		}

		/// <summary>Releases the shared frame timer and native drawing surface.</summary>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_loadingAnimationTimer?.Dispose();
				_loadingAnimationTimer = null;
				StopMouseInput();
				showTooltipTimer?.Dispose();
				_surface.Dispose();
			}
			base.Dispose(disposing);
		}

		[DllImport("dwmapi.dll")]
		private static extern int DwmSetWindowAttribute(IntPtr windowHandle, int attribute, ref int attributeValue, int attributeSize);

		[DllImport("dwmapi.dll")]
		private static extern int DwmGetWindowAttribute(IntPtr windowHandle, int attribute, out int attributeValue, int attributeSize);

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

		private bool _tornTabWindowReady;

		/// <summary>Overlay that owns the current cross-window tab drag.</summary>
		protected static TitleBarTabsOverlay _tornTabDragOwner;

		/// <summary>Cursor offset inside <see cref="_tornTabWindow" /> while dragging.</summary>
		protected static Point _tornTabWindowCursorOffset;

		private static PointF _tornTabGrabRatio;

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

		/// <summary>Pointer to the low-level mouse hook callback (<see cref="MouseHookCallback" />).</summary>
		protected IntPtr _hookId;

		/// <summary>Delegate of <see cref="MouseHookCallback" />; declared as a member variable to keep it from being garbage collected.</summary>
		protected HOOKPROC _hookproc = null;

		/// <summary>Index of the tab, if any, whose close button is being hovered over.</summary>
		protected int _isOverCloseButtonForTab = -1;

        protected bool _isOverSizingBox = false;

        protected bool _isOverAddButton = true;

		private const int MouseInputMessage = 0x8000 + 72;
		private readonly Queue<MouseEvent> _mouseEvents = new Queue<MouseEvent>();
		private bool _mouseInputQueued, _mouseMovePending, _mouseInside;
		private Point _latestMousePosition;
		private TitleBarTab _tooltipTab;

		[DllImport("user32.dll", EntryPoint = "PostMessageW")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool PostInputMessage(IntPtr window, int message, IntPtr wParam, IntPtr lParam);

		/// <summary>Parent form for the overlay.</summary>
		protected TitleBarTabs _parentForm;

        protected long _lastLeftButtonClickTicks = 0;

		private Point _lastCaptionClickPosition;

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

			// We don't want this window visible in the taskbar
			ShowInTaskbar = false;
			FormBorderStyle = FormBorderStyle.SizableToolWindow;
			MinimizeBox = false;
			MaximizeBox = false;
			_aeroEnabled = _parentForm.IsCompositionEnabled;

			Show(_parentForm);
			_loadingAnimationTimer = new TabFrameScheduler(Handle);
			AttachHandlers();

			showTooltipTimer = new Timer
			{
				AutoReset = false,
				SynchronizingObject = this
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

			StopMouseInput();
		}

		private void StopMouseInput()
		{
			if (_hookId != IntPtr.Zero) User32.UnhookWindowsHookEx(_hookId);
			_hookId = IntPtr.Zero;
			_mouseEvents.Clear();
			_mouseMovePending = false;
			if (_parentForm != null) _parents.Remove(_parentForm);
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
			if (IsDisposed || Disposing || _parentForm.IsDisposed || !_parentForm.ShowTooltips)
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
			Point tabClickOffset = GetRelativeCursorPosition(cursorPosition);
			tabClickOffset.Offset(-tab.Area.Left, -tab.Area.Top);
			PointF grabRatio = BaseTabRenderer.GetTabDragGrabRatio(tabClickOffset, tab.Area.Size);
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
					_parentForm.IsDisposed || _parentForm.Tabs.Count != 1 || !_parentForm.Tabs.Contains(tab) ||
					FindTabDropTarget(_singleTabDropPoint, _parentForm) != target)
					return;

				// The target must deselect its current tab when this one is inserted.
				tab.Active = false;
				tab.ClearSubscriptions();
				_parentForm.Tabs.Remove(tab);
				TitleBarTabsOverlay targetOverlay = target._overlay;
				target.TabRenderer.CombineTab(tab, targetOverlay.GetRelativeCursorPosition(_singleTabDropPoint), grabRatio);
				// Establish the full-size dragged tab before mouse-up can trigger an opening animation.
				targetOverlay.Render(_singleTabDropPoint);
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
			TitleBarTabs target = FindTabDropTarget(cursorPosition, _parentForm);
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

		private TitleBarTabs FindTabDropTarget(Point cursorPosition, TitleBarTabs draggedWindow)
		{
			var context = _parentForm.ApplicationContext;
			if (context == null || draggedWindow == null || !draggedWindow.IsHandleCreated) return null;
			// Avoid scanning desktop windows unless a live tab strip is at the pointer.
			if (!context.OpenWindows.Any(window => window != draggedWindow && !window.IsDisposed && !window.Disposing &&
				window.IsHandleCreated && window.Visible && window.WindowState != FormWindowState.Minimized &&
				window.Tabs.Count > 0 && window._overlay != null && window.TabDropArea.Contains(cursorPosition))) return null;

			IntPtr ignoredWindow = draggedWindow.Handle;
			TitleBarTabsOverlay draggedOverlay = draggedWindow._overlay;
			IntPtr ignoredOverlay = draggedOverlay != null && draggedOverlay.IsHandleCreated ? draggedOverlay.Handle : IntPtr.Zero;
			IntPtr frontWindow = IntPtr.Zero;
			// EnumWindows walks front to back, including other applications. Ignore only
			// the dragged window and its overlay; the source remains a possible target
			// or obstruction. A foreground page must also block tab strips behind it.
			EnumWindows((window, unused) =>
			{
				if (window == ignoredWindow || window == ignoredOverlay || !User32.IsWindowVisible(window) || User32.IsIconic(window)) return true;
				RECT bounds;
				// GetWindowRect uses the same DPI-virtualized screen coordinates as Cursor.Position.
				if (!User32.GetWindowRect(window, out bounds) || cursorPosition.X < bounds.left || cursorPosition.X >= bounds.right ||
					cursorPosition.Y < bounds.top || cursorPosition.Y >= bounds.bottom) return true;
				int cloaked;
				if (DwmGetWindowAttribute(window, 14 /* DWMWA_CLOAKED */, out cloaked, sizeof(int)) == 0 && cloaked != 0) return true;
				// Click-through layered helpers do not obscure a drop target for input.
				int clickThrough = (int)(WS_EX.WS_EX_LAYERED | WS_EX.WS_EX_TRANSPARENT);
				if ((User32.GetWindowLong(window, -20 /* GWL_EXSTYLE */) & clickThrough) == clickThrough) return true;
				frontWindow = window;
				return false;
			}, IntPtr.Zero);

			foreach (TitleBarTabs window in context.OpenWindows)
			{
				if (window == draggedWindow || window.IsDisposed || window.Disposing || !window.IsHandleCreated ||
					!window.Visible || window.WindowState == FormWindowState.Minimized || window.Tabs.Count == 0) continue;
				TitleBarTabsOverlay overlay = window._overlay;
				if (window.Handle == frontWindow || (overlay != null && overlay.IsHandleCreated && overlay.Handle == frontWindow))
					return overlay != null && window.TabDropArea.Contains(cursorPosition) ? window : null;
			}
			return null;
		}

		/// <summary>Moves a tab into a real window as soon as it leaves its current tab strip.</summary>
		private void CreateLiveTornTabWindow(Point cursorPosition)
		{
			TitleBarTab tab = _parentForm.SelectedTab;
			if (tab == null)
			{
				return;
			}
			_tornTabWindowReady = false;

			Rectangle sourceTabArea = tab.Area;
			int sourceTabAreaWidth = Math.Max(1, _parentForm.TabRenderer.MaxTabArea.Width);
			Rectangle sourceDropArea = TabDropArea;
			bool preserveTabOffset = cursorPosition.X >= sourceDropArea.Left && cursorPosition.X < sourceDropArea.Right;

			Point relativeCursorPosition = GetRelativeCursorPosition(cursorPosition);
			_tornTabGrabRatio = _parentForm.TabRenderer.TabDragGrabRatio ?? BaseTabRenderer.GetTabDragGrabRatio(
				new Point(relativeCursorPosition.X - sourceTabArea.Left, relativeCursorPosition.Y - sourceTabArea.Top),
				sourceTabArea.Size);

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
			// The detach anchor below needs the new window's actual layout now.
			newWindow._overlay.Render();

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

			newWindow.TabRenderer.BeginDetachedWindowDrag(detachedTabX, detachedTabWidth, _tornTabGrabRatio);
			Point cursorOffsetWithinTab = newWindow.TabRenderer.TabDragClickOffset.Value;
			_tornTabWindowCursorOffset = new Point(
				(newWindow._overlay.Left - newWindow.Left) + detachedTabX + cursorOffsetWithinTab.X,
				(newWindow._overlay.Top - newWindow.Top) + tab.Area.Top + cursorOffsetWithinTab.Y);
			MoveLiveTornTabWindow(cursorPosition);
			newWindow.RedrawTabs();

			if (_parentForm.Tabs.Count == 0)
			{
				_parentForm.Hide();
			}

			// Source-tab selection and reparenting must finish before the dragged window takes focus.
			newWindow.Activate();
			_tornTabWindowReady = true;
		}

		/// <summary>Keeps the live torn-tab window underneath the cursor.</summary>
		private void MoveLiveTornTabWindow(Point cursorPosition)
		{
			if (_tornTabWindow == null || _tornTabWindow.IsDisposed)
			{
				return;
			}

			// Raise while moving, without repeatedly changing focus or making the window always-on-top.
			User32.SetWindowPos(_tornTabWindow.Handle, IntPtr.Zero,
				cursorPosition.X - _tornTabWindowCursorOffset.X,
				cursorPosition.Y - _tornTabWindowCursorOffset.Y, 0, 0, SWP.SWP_NOSIZE | SWP.SWP_NOACTIVATE);
		}

        // Mouse moves are sampled once per frame on the UI thread. Button events
        // retain their original coordinates and are dispatched without waiting for
        // the next animation tick. There is no worker/UI Invoke round trip.
        private void ProcessPendingMouseMove()
        {
            if (!_mouseMovePending || IsDisposed || _parentForm.IsDisposed || _parentForm.TabRenderer == null) return;
            _mouseMovePending = false;
            Point cursor = _latestMousePosition;
            if (_singleTabDragOwner != null)
            {
                if (_singleTabDragOwner == this) CheckSingleTabWindowDrop(cursor);
                return;
            }
            if (_tornTab != null && _tornTabDragOwner != this) return;
            if (_tornTab != null)
            {
                // Showing/reparenting can dispatch messages before the detached
                // window's tab and pointer offset have finished initialization.
                if (!_tornTabWindowReady) return;
                MoveLiveTornTabWindow(cursor);
                TitleBarTabs target = FindTabDropTarget(cursor, _tornTabWindow);
                if (target != null)
                {
                    TitleBarTab tab = _tornTab;
                    TitleBarTabs tornWindow = _tornTabWindow;
                    _tornTab = null;
                    _tornTabWindow = null;
                    _tornTabDragOwner = null;
                    _tornTabWindowReady = false;
                    tab.Active = false;
                    tab.ClearSubscriptions();
                    if (tornWindow != null && !tornWindow.IsDisposed) tornWindow.Tabs.Remove(tab);
                    target.TabRenderer.CombineTab(tab, target._overlay.GetRelativeCursorPosition(cursor), _tornTabGrabRatio);
                    target._overlay.Render(cursor);
                    target.Activate();
                    if (tornWindow != null && !tornWindow.IsDisposed) tornWindow.Close();
                    if (_parentForm.Tabs.Count == 0) _parentForm.Close();
                }
                return;
            }

            BaseTabRenderer renderer = _parentForm.TabRenderer;
            Point relative = GetRelativeCursorPosition(cursor);
            _mouseInside = DesktopBounds.Contains(cursor);
            OnMouseMove(new MouseEventArgs(MouseButtons.None, 0, cursor.X, cursor.Y, 0));
            if (renderer.IsTabRepositioning)
            {
                _wasDragging = true;
                HideTooltip();
                _tooltipTab = null;
                Rectangle dragArea = TabDropArea;
                dragArea.Inflate(renderer.TabTearDragDistance, renderer.TabTearDragDistance);
                if (!dragArea.Contains(cursor) && _tornTab == null)
                {
                    renderer.IsTabRepositioning = false;
                    CreateLiveTornTabWindow(cursor);
                    return;
                }
                RequestRender();
                return;
            }

            TitleBarTab hovered = renderer.OverTab(_parentForm.Tabs, relative);
            if (hovered != _tooltipTab)
            {
                HideTooltip();
                _tooltipTab = hovered;
                StartTooltipTimer();
            }
            int closeIndex = hovered != null && renderer.IsOverCloseButton(hovered, relative)
                ? _parentForm.Tabs.IndexOf(hovered) : -1;
            bool sizing = renderer.RendersEntireTitleBar && renderer.IsOverSizingBox(relative);
            bool add = renderer.IsOverAddButton(relative);
            bool redraw = renderer.RequiresHoverRedraw(relative) || closeIndex != _isOverCloseButtonForTab ||
                sizing || sizing != _isOverSizingBox || add != _isOverAddButton;
            _isOverCloseButtonForTab = closeIndex;
            _isOverSizingBox = sizing;
            _isOverAddButton = add;
            if (redraw) RequestRender();
        }

        /// <summary>Dispatches queued button events on the overlay's UI thread.</summary>
        protected void InterpretMouseEvents()
        {
            _mouseInputQueued = false;
            while (_mouseEvents.Count > 0 && !IsDisposed && !_parentForm.IsDisposed)
            {
                MouseEvent input = _mouseEvents.Dequeue();
                // Finish the drag at the release position, even if its last move
                // arrived between frames. Never replay the current cursor for an
                // older queued click.
                if (_mouseMovePending)
                {
                    _latestMousePosition = input.Position;
                    ProcessPendingMouseMove();
                }
                if (_singleTabDragOwner != null) continue;
                if ((WM)input.wParam.ToInt32() == WM.WM_LBUTTONDOWN)
                {
                    _wasDragging = false;
                }
                else if ((WM)input.wParam.ToInt32() == WM.WM_LBUTTONDBLCLK)
                {
                    if (!_wasDragging && _parentForm.MaximizeBox && IsCaptionPoint(input.Position))
                        _parentForm.WindowState = _parentForm.WindowState == FormWindowState.Maximized
                            ? FormWindowState.Normal : FormWindowState.Maximized;
                }
                else if ((WM)input.wParam.ToInt32() == WM.WM_LBUTTONUP)
                {
                    if (_parentForm.TabRenderer.IsTabRepositioning) Render(input.Position);
                    if (_tornTab != null && _tornTabDragOwner != this) continue;
                    if (_tornTab != null)
                    {
                        TitleBarTabs released = _tornTabWindow;
                        MoveLiveTornTabWindow(input.Position);
                        _tornTab = null;
                        _tornTabWindow = null;
                        _tornTabDragOwner = null;
                        _tornTabWindowReady = false;
                        if (released != null && !released.IsDisposed)
                        {
                            released.TabRenderer.EndDetachedWindowDrag();
                            released.Activate();
                            released.RedrawTabs();
                            SetWindowTransitionsEnabled(released, true);
                        }
                        if (_parentForm.Tabs.Count == 0) _parentForm.Close();
                    }
                    if (!IsDisposed) OnMouseUp(new MouseEventArgs(MouseButtons.Left, 1, input.Position.X, input.Position.Y, 0));
                }
            }
        }

        private bool IsCaptionPoint(Point screenPosition)
        {
            if (!DesktopBounds.Contains(screenPosition)) return false;
            Point relative = GetRelativeCursorPosition(screenPosition);
            BaseTabRenderer renderer = _parentForm.TabRenderer;
            return renderer.OverTab(_parentForm.Tabs, relative) == null &&
                !renderer.IsOverAddButton(relative) && !renderer.IsOverSizingBox(relative);
        }

		/// <summary>Hook callback to process <see cref="WM.WM_MOUSEMOVE" /> messages to highlight/un-highlight the close button on each tab.</summary>
		/// <param name="nCode">The message being received.</param>
		/// <param name="wParam">Additional information about the message.</param>
		/// <param name="lParam">Additional information about the message.</param>
		/// <returns>A zero value if the procedure processes the message; a nonzero value if the procedure ignores the message.</returns>
        protected IntPtr MouseHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && !IsDisposed && !Disposing && !_parentFormClosing && _parentForm.TabRenderer != null)
            {
                WM message = (WM)wParam.ToInt32();
                if (message == WM.WM_MOUSEMOVE)
                {
                    // Cursor.Position uses the host's DPI coordinate system.
                    Point cursor = Cursor.Position;
                    if (_mouseInside || DesktopBounds.Contains(cursor) || _parentForm.TabRenderer.IsTabRepositioning ||
                        (_parentForm.TabRenderer.TabDragClickOffset.HasValue) || _singleTabDragOwner == this || _tornTabDragOwner == this)
                    {
                        _latestMousePosition = cursor;
                        _mouseMovePending = true;
                        UpdateLoadingAnimation();
                    }
                }
                else if (message == WM.WM_LBUTTONDOWN || message == WM.WM_LBUTTONUP)
                {
                    Point position = Cursor.Position;
                    _mouseEvents.Enqueue(new MouseEvent { nCode = nCode, wParam = wParam, Position = position });
                    if (message == WM.WM_LBUTTONDOWN)
                    {
                        long ticks = DateTime.Now.Ticks;
                        // Classify before a click can add/remove a tab and expose
                        // caption space underneath it. Both clicks must be blank.
                        bool caption = _singleTabDragOwner == null && _tornTab == null && IsCaptionPoint(position);
                        Size doubleClickSize = SystemInformation.DoubleClickSize;
                        var doubleClickArea = new Rectangle(
                            _lastCaptionClickPosition.X - doubleClickSize.Width / 2,
                            _lastCaptionClickPosition.Y - doubleClickSize.Height / 2,
                            doubleClickSize.Width, doubleClickSize.Height);
                        if (caption && _lastLeftButtonClickTicks > 0 &&
                            ticks - _lastLeftButtonClickTicks < _doubleClickInterval * 10000 && doubleClickArea.Contains(position))
                        {
                            _mouseEvents.Enqueue(new MouseEvent { nCode = nCode, wParam = new IntPtr((int)WM.WM_LBUTTONDBLCLK), Position = position });
                            _lastLeftButtonClickTicks = 0;
                        }
                        else _lastLeftButtonClickTicks = caption ? ticks : 0;
                        _lastCaptionClickPosition = position;
                    }
                    if (!_mouseInputQueued)
                        _mouseInputQueued = PostInputMessage(Handle, MouseInputMessage, IntPtr.Zero, IntPtr.Zero);
                }
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
			if (!IsDisposed && _parentForm.TabRenderer != null)
			{
				// 92 is SM_CXPADDEDBORDER, which returns the amount of extra border padding around captioned windows
				int borderPadding = DisplayType == DisplayType.Classic
					? 0
					: User32.GetSystemMetrics(92);

				// If the form is in a non-maximized state, we position the tabs below the minimize/maximize/close
				// buttons
				int top = _parentForm.Top + (DisplayType == DisplayType.Classic
					? SystemInformation.VerticalResizeBorderThickness
					: _parentForm.WindowState == FormWindowState.Maximized
						? SystemInformation.VerticalResizeBorderThickness + borderPadding
						: _parentForm.TabRenderer.RendersEntireTitleBar 
                            ? _parentForm.TabRenderer.IsWindows10
								? SystemInformation.BorderSize.Width
								: 0
							: borderPadding);
				int left = _parentForm.Left + SystemInformation.HorizontalResizeBorderThickness - (_parentForm.TabRenderer.IsWindows10 ? 0 : SystemInformation.BorderSize.Width) + borderPadding;
				int width = _parentForm.Width - ((SystemInformation.VerticalResizeBorderThickness + borderPadding) * 2) + (_parentForm.TabRenderer.IsWindows10 ? 0 : (SystemInformation.BorderSize.Width * 2));
				int height = _parentForm.TabRenderer.TabHeight + (DisplayType == DisplayType.Classic && _parentForm.WindowState != FormWindowState.Maximized && !_parentForm.TabRenderer.RendersEntireTitleBar
					? SystemInformation.CaptionButtonSize.Height
					: _parentForm.TabRenderer.IsWindows10
						? -1 * SystemInformation.BorderSize.Width
						: _parentForm.WindowState != FormWindowState.Maximized
							? borderPadding
							: 0);

				bool resized = Width != width || Height != height;
				_parentForm.TabRenderer.OffsetWindowPosition(left - Left, top - Top);
				SetBounds(left, top, width, height);
				if (resized || _surface.Bitmap == null) Render();
				else RequestRender();
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
			// Layout/model callbacks may request a repaint during rendering. Defer
			// those requests so they cannot resize or overwrite the shared surface.
			if (_rendering)
			{
				_renderPending = true;
				_forceRenderPending |= forceRedraw;
				return;
			}
			if (IsDisposed || Disposing || _parentForm.TabRenderer == null ||
				_parentForm.WindowState == FormWindowState.Minimized || _parentForm.ClientRectangle.Width <= 0 || Width <= 0 || Height <= 0)
			{
				UpdateLoadingAnimation();
				return;
			}

			_rendering = true;
			forceRedraw |= _forceRenderPending;
			_renderPending = _forceRenderPending = false;
			try
			{
				cursorPosition = GetRelativeCursorPosition(cursorPosition);
				_surface.EnsureSize(Width, Height);
				Graphics graphics = _surface.Graphics;
				GraphicsState state = graphics.Save();
				try
				{
					graphics.Clear(Color.Transparent);
					DrawTitleBarBackground(graphics);

					// Preserve the existing offsets for classic and partial-titlebar renderers.
					Point offset = _parentForm.WindowState != FormWindowState.Maximized && DisplayType == DisplayType.Classic && !_parentForm.TabRenderer.RendersEntireTitleBar
						? new Point(0, SystemInformation.CaptionButtonSize.Height)
						: _parentForm.WindowState != FormWindowState.Maximized && !_parentForm.TabRenderer.RendersEntireTitleBar
							? new Point(0, SystemInformation.VerticalResizeBorderThickness - SystemInformation.BorderSize.Height)
							: Point.Empty;
					_parentForm.TabRenderer.Render(_parentForm.Tabs, graphics, offset, cursorPosition, forceRedraw);

					// Retain the transparent hole for the underlying classic control box.
					if (DisplayType == DisplayType.Classic && (_parentForm.ControlBox || _parentForm.MaximizeBox || _parentForm.MinimizeBox))
					{
						int boxes = (_parentForm.ControlBox ? 1 : 0) + (_parentForm.MinimizeBox ? 1 : 0) + (_parentForm.MaximizeBox ? 1 : 0);
						int boxWidth = boxes * SystemInformation.CaptionButtonSize.Width;
						graphics.CompositingMode = CompositingMode.SourceCopy;
						graphics.FillRectangle(Brushes.Transparent, Width - boxWidth, 0, boxWidth, SystemInformation.CaptionButtonSize.Height);
					}
				}
				finally { graphics.Restore(state); }

				// GDI+ draws directly into the selected premultiplied DIB. Finish its
				// writes before Windows reads it; no GetHbitmap allocation/copy is needed.
				graphics.Flush(FlushIntention.Sync);
				SIZE size = new SIZE { cx = _surface.Bitmap.Width, cy = _surface.Bitmap.Height };
				POINT pointSource = new POINT { x = 0, y = 0 };
				POINT topPos = new POINT { x = Left, y = Top };
				BLENDFUNCTION blend = new BLENDFUNCTION
				{
					BlendOp = Convert.ToByte((int)AC.AC_SRC_OVER),
					BlendFlags = 0,
					SourceConstantAlpha = (byte)(_parentForm.Opacity * 255),
					AlphaFormat = Convert.ToByte((int)AC.AC_SRC_ALPHA)
				};
				if (!User32.UpdateLayeredWindow(Handle, IntPtr.Zero, ref topPos, ref size,
					_surface.DeviceContext, ref pointSource, 0, ref blend, ULW.ULW_ALPHA))
					throw new Win32Exception(Marshal.GetLastWin32Error(), "Error while calling UpdateLayeredWindow().");
			}
			finally
			{
				_rendering = false;
				UpdateLoadingAnimation();
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
			if (m.Msg == TabFrameScheduler.Message)
			{
				_loadingAnimationTimer?.Acknowledge();
				if (_loadingAnimationTimer != null) LoadingAnimation_Tick(this, EventArgs.Empty);
				return;
			}
			if (m.Msg == MouseInputMessage)
			{
				InterpretMouseEvents();
				return;
			}
			if ((m.Msg == (int)WM.WM_LBUTTONUP || m.Msg == (int)WM.WM_NCLBUTTONUP) && _mouseMovePending)
			{
				_latestMousePosition = Cursor.Position;
				ProcessPendingMouseMove();
				if (_parentForm.TabRenderer.IsTabRepositioning) Render(_latestMousePosition);
			}
            // Detect any sort of mouse click
            if (m.Msg == (int)WM.WM_LBUTTONDOWN ||
                m.Msg == (int)WM.WM_LBUTTONUP ||
                m.Msg == (int)WM.WM_LBUTTONDBLCLK ||
                m.Msg == (int)WM.WM_NCLBUTTONDBLCLK ||
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
				case WM.WM_NCLBUTTONDBLCLK:
                    // Caption toggling is handled by the filtered hook events.
                    // Keep the second press as a normal tab/button press instead
                    // of allowing Windows to maximize through HTCAPTION.
                    m.Msg = m.Msg == (int)WM.WM_NCLBUTTONDBLCLK
                        ? (int)WM.WM_NCLBUTTONDOWN : (int)WM.WM_LBUTTONDOWN;
                    goto case WM.WM_LBUTTONDOWN;

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
			/// <summary>DPI-adjusted screen coordinates when the event was received.</summary>
			public Point Position { get; set; }
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
