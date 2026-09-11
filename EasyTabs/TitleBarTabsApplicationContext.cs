using System.Collections.Generic;
using System;
using System.Linq;
using System.Windows.Forms;

namespace EasyTabs
{
	/// <summary>
	/// Application context to use when starting a <see cref="TitleBarTabs" /> application via <see cref="Application.Run(ApplicationContext)" />.  Used to
	/// track open windows so that the entire application doesn't quit when the first-opened window is closed.
	/// </summary>
	public class TitleBarTabsApplicationContext : ApplicationContext
	{
		/// <summary>List of all opened windows.</summary>
		protected List<TitleBarTabs> _openWindows = new List<TitleBarTabs>();
		private readonly List<TitleBarTabs> _activationOrder = new List<TitleBarTabs>();

		/// <summary>A snapshot of open windows, most recently activated first.</summary>
		public IEnumerable<TitleBarTabs> OpenWindowsByActivation => _activationOrder.ToArray();

		/// <summary>List of all opened windows.</summary>
		public List<TitleBarTabs> OpenWindows
		{
			get
			{
				return _openWindows;
			}
		}

		/// <summary>Constructor; takes the initial window to display and, if it's not closing, opens it and shows it.</summary>
		/// <param name="initialFormInstance">Initial window to display.</param>
		public void Start(TitleBarTabs initialFormInstance)
		{
			if (initialFormInstance.IsClosing)
			{
				ExitThread();
			}

			else
			{
				OpenWindow(initialFormInstance);
				initialFormInstance.Show();
			}
		}

		/// <summary>
		/// Adds <paramref name="window" /> to <see cref="_openWindows" /> and attaches event handlers to its <see cref="Form.FormClosed" /> event to keep track
		/// of it.
		/// </summary>
		/// <param name="window">Window that we're opening.</param>
		public void OpenWindow(TitleBarTabs window)
		{
			if (!_openWindows.Contains(window))
			{
				window.ApplicationContext = this;

				_openWindows.Add(window);
				_activationOrder.Insert(0, window);
				window.Activated += Window_Activated;
				window.FormClosed += window_FormClosed;
				window.Disposed += Window_Disposed;
			}
		}

		/// <summary>
		/// Handler method that's called when an item in <see cref="_openWindows" /> has its <see cref="Form.FormClosed" /> event invoked.  Removes the window
		/// from <see cref="_openWindows" /> and, if there are no more windows open, calls <see cref="ApplicationContext.ExitThread" />.
		/// </summary>
		/// <param name="sender">Object from which this event originated.</param>
		/// <param name="e">Arguments associated with the event.</param>
		protected void window_FormClosed(object sender, FormClosedEventArgs e)
		{
			RemoveWindow((TitleBarTabs)sender);
		}

		private void Window_Activated(object sender, EventArgs e)
		{
			var window = (TitleBarTabs)sender;
			_activationOrder.Remove(window);
			_activationOrder.Insert(0, window);
		}

		private void Window_Disposed(object sender, EventArgs e) => RemoveWindow((TitleBarTabs)sender);

		private void RemoveWindow(TitleBarTabs window)
		{
			if (!_openWindows.Remove(window)) return;
			_activationOrder.Remove(window);
			window.Activated -= Window_Activated;
			window.FormClosed -= window_FormClosed;
			window.Disposed -= Window_Disposed;

			if (_openWindows.Count == 0)
			{
				ExitThread();
			}
		}
	}
}
