using Quartz.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Quartz
{
    public partial class Settings
    {
        // This file only arranges the page. The original event handlers in Settings.cs
        // still read, save and apply each setting.
        private Panel _settingsHeader;
        private Panel _settingsBody;
        private FlowLayoutPanel _settingsNavigation;
        private FlowLayoutPanel _settingsCards;
        private TextBox _settingsSearch;
        private Label _settingsSubtitle;
        private Label _emptySearchLabel;
        private Panel _timeMachinePanel;
        private readonly List<FlowLayoutPanel> _cards = new List<FlowLayoutPanel>();
        private readonly Dictionary<Control, string> _cardSections = new Dictionary<Control, string>();
        private readonly Dictionary<Control, string> _cardSearchText = new Dictionary<Control, string>();
        private readonly List<Label> _secondaryLabels = new List<Label>();
        private readonly Font _settingsBodyFont = new Font("Segoe UI", 10F);
        private readonly Font _settingsTitleFont = new Font("Segoe UI", 25F, FontStyle.Bold);
        private readonly Font _settingsSectionFont = new Font("Segoe UI", 12F, FontStyle.Bold);
        private readonly Font _settingsSmallFont = new Font("Segoe UI", 9F);
        private string _selectedSettingsSection = "General";
        private bool _arrangingSettings;
        private bool _settingsLoaded;
        private Color _pageColor;
        private Color _cardColor;
        private Color _textColor;
        private Color _mutedColor;
        private Color _accentColor;
        private Color _selectedColor;
        private Color _borderColor;

        private int SettingsPixels(int value)
        {
            return (int)Math.Round(value * DeviceDpi / 96F);
        }

        private void BuildSettingsLayout()
        {
            SuspendLayout();
            tabControl1.Visible = false;
            Font = _settingsBodyFont;
            MinimumSize = Size.Empty;
            FormBorderStyle = FormBorderStyle.Sizable;
            MaximizeBox = true;
            ClientSize = new Size(1060, 720);

            _settingsHeader = new Panel();
            _settingsHeader.Dock = DockStyle.Top;
            _settingsHeader.Height = SettingsPixels(124);

            Label title = new Label();
            title.Text = "Settings";
            title.Font = _settingsTitleFont;
            title.AutoSize = true;
            title.Location = new Point(SettingsPixels(28), SettingsPixels(18));
            _settingsHeader.Controls.Add(title);

            _settingsSubtitle = new Label();
            _settingsSubtitle.Text = "Make Quartz feel like you.";
            _settingsSubtitle.AutoSize = true;
            _settingsSubtitle.Location = new Point(SettingsPixels(31), SettingsPixels(75));
            _secondaryLabels.Add(_settingsSubtitle);
            _settingsHeader.Controls.Add(_settingsSubtitle);

            Panel searchPanel = new Panel();
            searchPanel.Dock = DockStyle.Right;
            searchPanel.Width = SettingsPixels(286);
            searchPanel.Padding = new Padding(SettingsPixels(12), SettingsPixels(27), SettingsPixels(28), 0);
            Label searchLabel = new Label();
            searchLabel.Text = "Search settings";
            searchLabel.Dock = DockStyle.Top;
            searchLabel.Height = SettingsPixels(25);
            searchLabel.Font = _settingsSmallFont;
            _secondaryLabels.Add(searchLabel);
            _settingsSearch = new TextBox();
            _settingsSearch.Name = "txtSearchSettings";
            _settingsSearch.AccessibleName = "Search settings";
            _settingsSearch.BorderStyle = BorderStyle.FixedSingle;
            _settingsSearch.Dock = DockStyle.Top;
            _settingsSearch.TextChanged += SettingsSearch_TextChanged;
            searchPanel.Controls.Add(_settingsSearch);
            searchPanel.Controls.Add(searchLabel);
            _settingsHeader.Controls.Add(searchPanel);

            _settingsBody = new Panel();
            _settingsBody.Dock = DockStyle.Fill;
            _settingsNavigation = new FlowLayoutPanel();
            _settingsNavigation.AutoScroll = true;
            _settingsNavigation.Padding = new Padding(SettingsPixels(16), SettingsPixels(8), SettingsPixels(12), 0);
            _settingsNavigation.Dock = DockStyle.Left;
            _settingsNavigation.Width = SettingsPixels(190);
            _settingsNavigation.FlowDirection = FlowDirection.TopDown;
            _settingsNavigation.WrapContents = false;

            string[] sections = { "General", "Appearance", "Privacy", "Performance", "Extras", "About" };
            foreach (string section in sections)
            {
                Button button = new Button();
                button.Text = section;
                button.Tag = section;
                button.AccessibleName = section + " settings";
                button.FlatStyle = FlatStyle.Flat;
                button.FlatAppearance.BorderSize = 0;
                button.TextAlign = ContentAlignment.MiddleLeft;
                button.Padding = new Padding(SettingsPixels(12), 0, 0, 0);
                button.Size = new Size(SettingsPixels(158), SettingsPixels(42));
                button.Margin = new Padding(0, 0, 0, SettingsPixels(6));
                button.Cursor = Cursors.Hand;
                button.Click += SettingsSection_Click;
                _settingsNavigation.Controls.Add(button);
            }

            _settingsCards = new FlowLayoutPanel();
            _settingsCards.Dock = DockStyle.Fill;
            _settingsCards.AutoScroll = true;
            _settingsCards.WrapContents = false;
            _settingsCards.FlowDirection = FlowDirection.TopDown;
            _settingsCards.Resize += SettingsCards_Resize;
            _settingsBody.Controls.Add(_settingsCards);
            _settingsBody.Controls.Add(_settingsNavigation);
            _settingsCards.BringToFront();
            Controls.Add(_settingsBody);
            Controls.Add(_settingsHeader);
            _settingsHeader.BringToFront();
            _settingsBody.BringToFront();

            BuildGeneralSettings();
            BuildAppearanceSettings();
            BuildPrivacySettings();
            BuildPerformanceSettings();
            BuildExtraSettings();
            BuildAboutSettings();

            _emptySearchLabel = new Label();
            _emptySearchLabel.Text = "No matching settings. Try a different word.";
            _emptySearchLabel.AutoSize = true;
            _emptySearchLabel.Margin = new Padding(SettingsPixels(20));
            _emptySearchLabel.Visible = false;
            _secondaryLabels.Add(_emptySearchLabel);
            _settingsCards.Controls.Add(_emptySearchLabel);

            Disposed += SettingsLayout_Disposed;
            ApplySettingsPageTheme();
            ShowSettingsSection();
            ResumeLayout(true);
        }

        private void BuildGeneralSettings()
        {
            FlowLayoutPanel card = AddSettingsCard("General", "Start your way", "Choose where browsing begins.");
            AddSettingsRow(card, "Search engine", "Used when you search from the address bar.", cbSearchEngine);
            AddSettingsRow(card, "Default home page", "Use the selected search engine's home page.", cbDHP);

            card = AddSettingsCard("General", "Reading & navigation", "Small adjustments for a more comfortable browser.");
            AddSettingsRow(card, "Page zoom", "Default page size, as a percentage.", NumZoom);
            AddSettingsRow(card, "Zoom controls", "Allow browser zoom controls.", zc);
            AddSettingsRow(card, "Pinch to zoom", "Use a touchpad or touchscreen to zoom.", pz);
            AddSettingsRow(card, "Swipe navigation", "Swipe to go back and forward.", BoxSwipeNav);
            AddSettingsRow(card, "Keyboard shortcuts", "Enable the browser's built-in accelerator keys.", BoxKeys);
            AddSettingsRow(card, "Status bar", "Show link destinations and page status.", cbStatusBar);

            card = AddSettingsCard("General", "PDF toolbar", "Checked items are hidden. Select None to keep the full toolbar.");
            HiddenPDFGroupBox.Text = "";
            HiddenPDFGroupBox.AutoSize = false;
            HiddenPDFGroupBox.Margin = new Padding(0);
            card.Controls.Add(HiddenPDFGroupBox);
            _cardSearchText[card] += " documents print save bookmarks fullscreen rotate PDF";
        }

        private void BuildAppearanceSettings()
        {
            FlowLayoutPanel card = AddSettingsCard("Appearance", "A look of your own", "Personalise Quartz without changing the way you browse.");
            AddSettingsRow(card, "Theme", "A theme change may ask you to restart Quartz.", ComboBoxTheme);
            AddSettingsRow(card, "Default tab icon", "Choose the fallback icon for pages without a favicon.", combDefaultFavicon);
            AddSettingsRow(card, "Downloads position", "Where the download dialog opens in your window.", cbDownloadAlighment);
            AddSettingsRow(card, "Settings navigation", "Place this page's section navigation on any edge.", comboSettingsTabAlinement);
        }

        private void BuildPrivacySettings()
        {
            FlowLayoutPanel card = AddSettingsCard("Privacy", "Tracking protection", "Choose how strongly the browser limits website trackers.");
            AddSettingsRow(card, "Prevention level", "Stricter protection can affect how some websites work.", ComboBoxTracking);

            card = AddSettingsCard("Privacy", "Autofill & passwords", "Control what the browser can remember for you.");
            AddSettingsRow(card, "General autofill", "Allow saved information to fill website forms.", autofillCheckBox);
            AddSettingsRow(card, "Password autosave", "Allow the browser to offer to save passwords.", autoSaveCheckBox);

            card = AddSettingsCard("Privacy", "Website capabilities", "Settings that affect how pages run.");
            AddSettingsRow(card, "JavaScript", "Allow websites to run scripts. Many sites need this to work.", cbScripts);
            AddSettingsRow(card, "Developer tools", "Allow Inspect and the F12 developer tools.", BoxDev);
        }

        private void BuildPerformanceSettings()
        {
            FlowLayoutPanel card = AddSettingsCard("Performance", "Keep things comfortable", "Choose the balance that suits your computer.");
            AddSettingsRow(card, "Reduced memory use", "Ask WebView2 to prioritise lower memory consumption.", checkBoxMemory);
            AddSettingsRow(card, "Interface animations", "Animate supported Quartz controls and dialogs.", cbAnimation);

            card = AddSettingsCard("Performance", "Window behaviour", "Preferences for Quartz's secondary windows.");
            AddSettingsRow(card, "Draggable forms", "Move supported dialogs by dragging their surface.", cbDrag);
            AddSettingsRow(card, "Escape to close", "Use Escape to close supported dialogs and settings.", cbESC);
        }

        private void BuildExtraSettings()
        {
            FlowLayoutPanel card = AddSettingsCard("Extras", "Time Machine", "Preview Quartz's date-based features. This does not change your computer's clock.");
            AddSettingsRow(card, "Simulate a date", "Try seasonal themes and your saved birthdays.", cbtimeMachine);
            _timeMachinePanel = new Panel();
            _timeMachinePanel.Margin = new Padding(0);
            _timeMachinePanel.Controls.Add(txtTimeMachine);
            _timeMachinePanel.Controls.Add(btnDown);
            _timeMachinePanel.Controls.Add(mcTimeMachine);
            txtTimeMachine.Font = _settingsBodyFont;
            btnDown.Font = _settingsBodyFont;
            mcTimeMachine.VisibleChanged += TimeMachine_VisibleChanged;
            card.Controls.Add(_timeMachinePanel);
            _cardSearchText[card] += " birthday calendar Easter Christmas seasonal date";
            Label tip = SettingsDescription("Use the calendar's right-click menu for holidays and birthdays.");
            card.Controls.Add(tip);
        }

        private void BuildAboutSettings()
        {
            FlowLayoutPanel card = AddSettingsCard("About", "Your Quartz", "Built with care. Made to be yours.");
            Panel identity = new Panel();
            identity.Height = SettingsPixels(108);
            identity.Margin = new Padding(0);
            pictureBox1.Size = new Size(SettingsPixels(56), SettingsPixels(56));
            pictureBox1.Location = new Point(0, SettingsPixels(10));
            pictureBox1.BackgroundImageLayout = ImageLayout.Zoom;
            LoadingProgress.Size = pictureBox1.Size;
            LoadingProgress.Location = pictureBox1.Location;
            AboutTitle.Font = _settingsTitleFont;
            AboutTitle.Location = new Point(SettingsPixels(74), 0);
            labelVersion.Font = _settingsSmallFont;
            labelVersion.Location = new Point(SettingsPixels(78), SettingsPixels(55));
            txtUpdate.Font = _settingsSmallFont;
            txtUpdate.Location = new Point(SettingsPixels(78), SettingsPixels(78));
            identity.Controls.Add(pictureBox1);
            identity.Controls.Add(LoadingProgress);
            identity.Controls.Add(AboutTitle);
            identity.Controls.Add(labelVersion);
            identity.Controls.Add(txtUpdate);
            card.Controls.Add(identity);
            _secondaryLabels.Add(labelVersion);
            _secondaryLabels.Add(txtUpdate);
            CoppyRightLable.Font = _settingsSmallFont;
            CoppyRightLable.Margin = new Padding(0, 0, 0, SettingsPixels(12));
            card.Controls.Add(CoppyRightLable);
            _secondaryLabels.Add(CoppyRightLable);
            buttonChech.Text = "Check for updates";
            buttonChech.Font = _settingsBodyFont;
            buttonChech.Size = new Size(SettingsPixels(180), SettingsPixels(38));
            buttonChech.Margin = new Padding(0);
            card.Controls.Add(buttonChech);

            card = AddSettingsCard("About", "Stay up to date", "Choose when Quartz checks for a newer version.");
            AddSettingsRow(card, "Automatic update checks", "You can still check manually at any time.", cbUpdateCheckFrequency);
        }

        private FlowLayoutPanel AddSettingsCard(string section, string title, string description)
        {
            FlowLayoutPanel card = new FlowLayoutPanel();
            card.FlowDirection = FlowDirection.TopDown;
            card.WrapContents = false;
            card.AutoSize = true;
            card.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            card.Padding = new Padding(SettingsPixels(22));
            card.Margin = new Padding(0, 0, 0, SettingsPixels(18));
            card.Paint += SettingsCard_Paint;

            Label heading = new Label();
            heading.Text = title;
            heading.Font = _settingsSectionFont;
            heading.AutoSize = true;
            heading.Margin = new Padding(0, 0, 0, SettingsPixels(6));
            card.Controls.Add(heading);
            Label caption = SettingsDescription(description);
            caption.Margin = new Padding(0, 0, 0, SettingsPixels(18));
            card.Controls.Add(caption);

            _cards.Add(card);
            _cardSections[card] = section;
            _cardSearchText[card] = section + " " + title + " " + description;
            _settingsCards.Controls.Add(card);
            return card;
        }

        private Label SettingsDescription(string text)
        {
            Label label = new Label();
            label.Text = text;
            label.Font = _settingsSmallFont;
            label.AutoSize = true;
            label.UseMnemonic = false;
            _secondaryLabels.Add(label);
            return label;
        }

        private void AddSettingsRow(FlowLayoutPanel card, string title, string description, Control setting)
        {
            TableLayoutPanel row = new TableLayoutPanel();
            row.ColumnCount = 2;
            row.RowCount = 1;
            row.AutoSize = true;
            row.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            row.Margin = new Padding(0, 0, 0, SettingsPixels(18));
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, SettingsPixels(205)));

            FlowLayoutPanel text = new FlowLayoutPanel();
            text.FlowDirection = FlowDirection.TopDown;
            text.WrapContents = false;
            text.AutoSize = true;
            text.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            text.Margin = new Padding(0, 0, SettingsPixels(20), 0);
            Label name = new Label();
            name.Text = title;
            name.AutoSize = true;
            name.Margin = new Padding(0, 0, 0, SettingsPixels(5));
            text.Controls.Add(name);
            Label details = SettingsDescription(description);
            details.Margin = new Padding(0);
            text.Controls.Add(details);

            setting.Font = _settingsBodyFont;
            setting.Dock = DockStyle.None;
            setting.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            setting.Margin = new Padding(0, SettingsPixels(4), 0, 0);
            setting.AccessibleName = title;
            if (setting is CheckBox checkbox)
            {
                checkbox.Text = "";
                checkbox.AutoSize = true;
                checkbox.Padding = new Padding(SettingsPixels(8));
            }
            else
            {
                setting.Width = SettingsPixels(195);
            }

            row.Controls.Add(text, 0, 0);
            row.Controls.Add(setting, 1, 0);
            card.Controls.Add(row);
            _cardSearchText[card] += " " + title + " " + description;
        }

        private void SettingsSection_Click(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            _selectedSettingsSection = (string)button.Tag;
            _settingsSearch.Clear();
            ShowSettingsSection();
        }

        private void SettingsSearch_TextChanged(object sender, EventArgs e)
        {
            ShowSettingsSection();
        }

        private void ShowSettingsSection()
        {
            if (_settingsCards == null)
            {
                return;
            }

            string search = _settingsSearch.Text.Trim();
            int matches = 0;
            _settingsCards.SuspendLayout();
            foreach (FlowLayoutPanel card in _cards)
            {
                bool show = _cardSections[card] == _selectedSettingsSection;
                if (search.Length > 0)
                {
                    show = _cardSearchText[card].IndexOf(search, StringComparison.CurrentCultureIgnoreCase) >= 0;
                }

                card.Visible = show;
                if (show)
                {
                    matches++;
                }
            }

            if (_emptySearchLabel != null)
            {
                _emptySearchLabel.Visible = matches == 0;
            }

            foreach (Button button in _settingsNavigation.Controls)
            {
                bool selected = search.Length == 0 && (string)button.Tag == _selectedSettingsSection;
                button.FlatAppearance.BorderSize = 0;
                button.BackColor = selected ? _selectedColor : _pageColor;
                button.ForeColor = selected ? _accentColor : _mutedColor;
            }

            _settingsCards.AutoScrollPosition = Point.Empty;
            _settingsCards.ResumeLayout(true);
            ArrangeSettingsCards();
        }

        private void SettingsCards_Resize(object sender, EventArgs e)
        {
            ArrangeSettingsCards();
        }

        private void ArrangeSettingsCards()
        {
            if (_settingsCards == null || _arrangingSettings)
            {
                return;
            }

            _arrangingSettings = true;
            _settingsCards.SuspendLayout();
            int available = Math.Max(SettingsPixels(280), _settingsCards.ClientSize.Width - SettingsPixels(48) - SystemInformation.VerticalScrollBarWidth);
            int width = Math.Min(SettingsPixels(880), available);
            int left = Math.Max(SettingsPixels(20), (_settingsCards.ClientSize.Width - width - SystemInformation.VerticalScrollBarWidth) / 2);
            _settingsCards.Padding = new Padding(left, SettingsPixels(8), SettingsPixels(20), SettingsPixels(28));

            foreach (FlowLayoutPanel card in _cards)
            {
                card.MinimumSize = new Size(width, 0);
                card.MaximumSize = new Size(width, 0);
                int contentWidth = width - card.Padding.Horizontal;
                foreach (Control control in card.Controls)
                {
                    control.MaximumSize = new Size(contentWidth, 0);
                    if (control is TableLayoutPanel row)
                    {
                        row.Width = contentWidth;
                        row.MinimumSize = new Size(contentWidth, 0);
                        FlowLayoutPanel text = (FlowLayoutPanel)row.GetControlFromPosition(0, 0);
                        int fieldWidth = Math.Min(SettingsPixels(205), contentWidth / 2);
                        row.ColumnStyles[1].Width = fieldWidth;
                        int textWidth = Math.Max(SettingsPixels(80), contentWidth - fieldWidth - text.Margin.Horizontal);
                        text.MaximumSize = new Size(textWidth, 0);
                        foreach (Label label in text.Controls)
                        {
                            label.MaximumSize = new Size(textWidth, 0);
                        }

                        Control field = row.GetControlFromPosition(1, 0);
                        field.MaximumSize = new Size(fieldWidth, 0);
                    }
                    else if (!(control is Label) && !(control is Button))
                    {
                        control.Width = contentWidth;
                    }
                }
            }

            ArrangePdfSettings();
            ArrangeTimeMachine();
            _settingsCards.ResumeLayout(true);
            _arrangingSettings = false;
        }

        private void ArrangePdfSettings()
        {
            int columnWidth = Math.Max(SettingsPixels(120), (HiddenPDFGroupBox.Width - SettingsPixels(24)) / 2);
            int index = 0;
            foreach (CheckBox checkbox in HiddenPDFGroupBox.Controls.OfType<CheckBox>().OrderBy(control => control.TabIndex))
            {
                // Keep the original Text values: the PDF handlers use them as setting names.
                checkbox.Font = _settingsSmallFont;
                checkbox.Location = new Point(SettingsPixels(12) + (index % 2) * columnWidth, SettingsPixels(18) + (index / 2) * SettingsPixels(32));
                checkbox.AutoSize = false;
                checkbox.Size = new Size(columnWidth, SettingsPixels(28));
                index++;
            }

            HiddenPDFGroupBox.Height = SettingsPixels(26) + ((index + 1) / 2) * SettingsPixels(32);
        }

        private void TimeMachine_VisibleChanged(object sender, EventArgs e)
        {
            ArrangeTimeMachine();
        }

        private void ArrangeTimeMachine()
        {
            if (_timeMachinePanel == null)
            {
                return;
            }

            txtTimeMachine.Location = new Point(0, SettingsPixels(4));
            txtTimeMachine.Width = Math.Min(SettingsPixels(260), _timeMachinePanel.Width - SettingsPixels(40));
            btnDown.Location = new Point(txtTimeMachine.Right + SettingsPixels(6), SettingsPixels(3));
            btnDown.Size = new Size(SettingsPixels(30), txtTimeMachine.Height + SettingsPixels(2));
            mcTimeMachine.Location = new Point(0, txtTimeMachine.Bottom + SettingsPixels(12));
            _timeMachinePanel.Height = txtTimeMachine.Bottom + SettingsPixels(16);
            if (mcTimeMachine.Visible)
            {
                _timeMachinePanel.Height = mcTimeMachine.Bottom + SettingsPixels(16);
            }
        }

        private void UpdateSettingsNavigation()
        {
            if (_settingsNavigation == null)
            {
                return;
            }

            string alignment = SettingsService.Get("SettingsTabAlignment");
            bool horizontal = alignment == "top" || alignment == "bottom";
            _settingsBody.SuspendLayout();
            if (horizontal)
            {
                _settingsNavigation.Dock = alignment == "top" ? DockStyle.Top : DockStyle.Bottom;
                _settingsNavigation.FlowDirection = FlowDirection.LeftToRight;
                _settingsNavigation.WrapContents = true;
                int columns = Math.Max(1, (_settingsBody.Width - SettingsPixels(32)) / SettingsPixels(130));
                int rows = (6 + columns - 1) / columns;
                _settingsNavigation.Height = SettingsPixels(14 + rows * 48);
            }
            else
            {
                _settingsNavigation.Dock = alignment == "right" ? DockStyle.Right : DockStyle.Left;
                _settingsNavigation.FlowDirection = FlowDirection.TopDown;
                _settingsNavigation.WrapContents = false;
                _settingsNavigation.Width = SettingsPixels(190);
            }

            foreach (Button button in _settingsNavigation.Controls)
            {
                button.Width = SettingsPixels(horizontal ? 124 : 158);
                button.Margin = new Padding(0, 0, SettingsPixels(horizontal ? 6 : 0), SettingsPixels(6));
            }

            _settingsBody.ResumeLayout(true);
            ArrangeSettingsCards();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            UpdateSettingsNavigation();
        }

        private void ApplySettingsPageTheme()
        {
            string theme = SettingsService.Get("Theme");
            bool dark = theme == "dark" || theme == "black";
            _pageColor = Color.FromArgb(244, 247, 247);
            _cardColor = Color.White;
            _textColor = Color.FromArgb(29, 43, 45);
            _mutedColor = Color.FromArgb(94, 111, 114);
            _accentColor = Color.FromArgb(0, 110, 105);
            _selectedColor = Color.FromArgb(220, 239, 234);
            _borderColor = Color.FromArgb(221, 229, 228);
            if (dark)
            {
                _pageColor = Color.FromArgb(24, 29, 31);
                _cardColor = Color.FromArgb(33, 40, 42);
                _textColor = Color.FromArgb(233, 240, 239);
                _mutedColor = Color.FromArgb(169, 186, 186);
                _accentColor = Color.FromArgb(125, 219, 199);
                _selectedColor = Color.FromArgb(40, 70, 66);
                _borderColor = Color.FromArgb(53, 67, 68);
            }

            if (theme == "black")
            {
                _pageColor = Color.Black;
                _cardColor = Color.FromArgb(16, 19, 20);
            }
            else if (theme == "aqua")
            {
                _pageColor = Color.FromArgb(227, 250, 251);
                _accentColor = Color.FromArgb(0, 91, 140);
                _selectedColor = Color.FromArgb(192, 232, 241);
            }
            else if (theme == "xmas")
            {
                _pageColor = Color.FromArgb(248, 242, 238);
                _accentColor = Color.FromArgb(153, 45, 50);
                _selectedColor = Color.FromArgb(243, 222, 219);
            }

            ApplySettingsControlTheme(this);
            BackColor = _pageColor;
            _settingsHeader.BackColor = _pageColor;
            _settingsBody.BackColor = _pageColor;
            _settingsNavigation.BackColor = _pageColor;
            _settingsCards.BackColor = _pageColor;
            foreach (Control control in _settingsHeader.Controls)
            {
                control.BackColor = _pageColor;
                if (control is Panel panel)
                {
                    foreach (Control child in panel.Controls)
                    {
                        child.BackColor = _pageColor;
                    }
                }
            }

            foreach (Label label in _secondaryLabels)
            {
                label.ForeColor = _mutedColor;
            }

            ShowSettingsSection();
        }

        private void ApplySettingsControlTheme(Control parent)
        {
            parent.BackColor = _cardColor;
            parent.ForeColor = _textColor;
            if (parent is Button button)
            {
                button.FlatStyle = FlatStyle.Flat;
                button.FlatAppearance.BorderSize = 1;
                button.FlatAppearance.BorderColor = _borderColor;
                button.FlatAppearance.MouseOverBackColor = _selectedColor;
                button.ForeColor = _accentColor;
            }
            else if (parent is ComboBox combo)
            {
                combo.FlatStyle = FlatStyle.Flat;
                combo.DropDownStyle = ComboBoxStyle.DropDownList;
            }
            else if (parent is CheckBox checkbox)
            {
                checkbox.FlatStyle = FlatStyle.Standard;
            }

            foreach (Control control in parent.Controls)
            {
                ApplySettingsControlTheme(control);
            }
        }

        private void SettingsCard_Paint(object sender, PaintEventArgs e)
        {
            Control card = (Control)sender;
            using (Pen border = new Pen(_borderColor))
            {
                e.Graphics.DrawRectangle(border, 0, 0, card.Width - 1, card.Height - 1);
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control | Keys.F))
            {
                _settingsSearch.Focus();
                _settingsSearch.SelectAll();
                return true;
            }

            if (keyData == Keys.Escape && _settingsSearch.Text.Length > 0)
            {
                _settingsSearch.Clear();
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void SettingsLayout_Disposed(object sender, EventArgs e)
        {
            updating = false;
            _settingsBodyFont.Dispose();
            _settingsTitleFont.Dispose();
            _settingsSectionFont.Dispose();
            _settingsSmallFont.Dispose();
        }
    }
}
