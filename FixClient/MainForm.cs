/////////////////////////////////////////////////
//
// FIX Client
//
// Copyright @ 2021 VIRTU Financial Inc.
// All rights reserved.
//
// Filename: MainForm.cs
// Author:   Gary Hughes
//
/////////////////////////////////////////////////
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Net;
using System.Xml.Serialization;

namespace FixClient;

partial class MainForm : Form
{
    readonly ToolStripButton _openButton;
    readonly ToolStripDropDownButton _openRecentButton;
    readonly ToolStripButton _newButton;
    readonly ToolStripButton _connectButton;
    readonly ToolStripButton _disconnectButton;
    readonly ToolStripButton _editSessionButton;
    readonly ToolStripButton _resetButton;

    readonly ToolStripButton _messagesButton;
    readonly ToolStripButton _historyButton;
    readonly ToolStripButton _ordersButton;
    //readonly ToolStripButton _generatorButton;
    readonly ToolStripButton _filtersButton;
    readonly ToolStripButton _customiseButton;
    //readonly ToolStripButton _dictionaryButton;
    readonly ToolStripButton _parserButton;
    readonly ToolStripButton _logButton;

    FixClientPanel? _currentPanel;

    readonly ToolStripMenuItem _fileNew;
    readonly ToolStripMenuItem _fileOpen;
    readonly ToolStripMenuItem _fileRecent;
    readonly ToolStripMenuItem _fileExit;

    readonly ToolStripMenuItem _sessionConnect;
    readonly ToolStripMenuItem _sessionDisconnect;
    readonly ToolStripMenuItem _sessionOptions;
    readonly ToolStripMenuItem _sessionReset;

    readonly ToolStripMenuItem _viewMessages;
    readonly ToolStripMenuItem _viewHistory;
    readonly ToolStripMenuItem _viewOrders;
    //readonly ToolStripMenuItem _viewGenerator;
    readonly ToolStripMenuItem _viewFilters;
    readonly ToolStripMenuItem _viewCustomise;
    //readonly ToolStripMenuItem _viewDictionary;
    readonly ToolStripMenuItem _viewParser;
    readonly ToolStripMenuItem _viewLog;

    readonly ToolStripMenuItem _helpAbout;

    readonly MenuStrip _mainMenu;

    List<string> _mru = [];

    readonly MessagesPanel _messagesPanel;
    readonly OrdersPanel _ordersPanel;
    readonly HistoryPanel _historyPanel;
    readonly GeneratorPanel _generatorPanel;
    readonly FiltersPanel _filtersPanel;
    readonly CustomisePanel _customisePanel;
    readonly DictionaryPanel _dictionaryPanel;
    readonly ParserPanel _parserPanel;
    readonly LogPanel _logPanel;

    readonly ToolStripContainer _container;

    readonly ToolStrip _toolStrip;
    readonly ToolStrip _viewToolStrip;

    Session? _currentSession;
    Session? CurrentSession
    {
        get { return _currentSession; }
        set
        {
            if (value == _currentSession)
                return;

            _currentSession?.Dispose();

            _currentSession = value;
        }
    }

    bool _expectingDisconnect;

    public MainForm()
    {
        InitializeComponent();

        _toolStrip = new ToolStrip
        {
            GripStyle = ToolStripGripStyle.Hidden,
            BackColor = LookAndFeel.Color.ToolStrip,
            Renderer = new ToolStripRenderer()
        };

        #region "Session ToolStrip"
        _newButton = new ToolStripButton
        {
            ToolTipText = "Create a new session",
            Image = Properties.Resources.New,
            ImageTransparentColor = Color.Magenta
        };
        _newButton.Click += NewSessionButtonClick;
        _toolStrip.Items.Add(_newButton);

        _openButton = new ToolStripButton
        {
            ToolTipText = "Open an existing session",
            Image = Properties.Resources.Open,
            ImageTransparentColor = Color.Magenta
        };
        _openButton.Click += OpenButtonClick;
        _toolStrip.Items.Add(_openButton);

        _openRecentButton = new ToolStripDropDownButton
        {
            ToolTipText = "Open an existing session"
        };
        _openRecentButton.DropDownItemClicked += OpenButtonDropDownItemClicked;
        _toolStrip.Items.Add(_openRecentButton);

        _connectButton = new ToolStripButton
        {
            ToolTipText = "Connect to the remote server",
            Image = Properties.Resources.Start,
            ImageTransparentColor = Color.Black
        };
        _connectButton.Click += ConnectButtonClick;
        _toolStrip.Items.Add(_connectButton);

        _disconnectButton = new ToolStripButton
        {
            ToolTipText = "Disconnect from the remote server",
            Image = Properties.Resources.Stop,
            ImageTransparentColor = Color.Black
        };
        _disconnectButton.Click += DisconnectButtonClick;
        _toolStrip.Items.Add(_disconnectButton);

        _editSessionButton = new ToolStripButton
        {
            ToolTipText = "Edit the options for the current session",
            Image = Properties.Resources.Options,
            ImageTransparentColor = Color.White
        };
        _editSessionButton.Click += EditSessionButtonClick;
        _toolStrip.Items.Add(_editSessionButton);

        _resetButton = new ToolStripButton
        {
            Image = Properties.Resources.Reset,
            ImageTransparentColor = Color.White,
            ToolTipText = "Reset the sequence numbers of the current session"
        };
        _resetButton.Click += ResetButtonClick;
        _toolStrip.Items.Add(_resetButton);
        #endregion

        _viewToolStrip = new ToolStrip
        {
            GripStyle = ToolStripGripStyle.Hidden,
            BackColor = LookAndFeel.Color.ToolStrip,
            Renderer = new ToolStripRenderer()
        };

        #region "Views"

        _messagesPanel = new MessagesPanel { Dock = DockStyle.Fill };
        _messagesPanel.MessageSelected += MessageDefaultsPanelMessageSelected;

        _messagesButton = new ToolStripButton("Messages", Properties.Resources.Messages, UpdateContentPanel)
        {
            ImageTransparentColor = Color.White,
            Tag = _messagesPanel
        };
        _viewToolStrip.Items.Add(_messagesButton);

        _ordersPanel = new OrdersPanel(_messagesPanel, _messagesButton) { Dock = DockStyle.Fill };

        _historyPanel = new HistoryPanel { Dock = DockStyle.Fill };
        _historyPanel.MessageSelected += MessageDefaultsPanelMessageSelected;

        _historyButton = new ToolStripButton("History", Properties.Resources.History, UpdateContentPanel)
        {
            ImageTransparentColor = Color.White,
            Tag = _historyPanel
        };
        _viewToolStrip.Items.Add(_historyButton);

        _ordersButton = new ToolStripButton("Orders", Properties.Resources.Orders, UpdateContentPanel)
        {
            ImageTransparentColor = Color.White,
            Tag = _ordersPanel
        };
        _viewToolStrip.Items.Add(_ordersButton);

        _generatorPanel = new GeneratorPanel { Dock = DockStyle.Fill };
        /*
        _generatorButton = new ToolStripButton("Generator", Properties.Resources.Function, UpdateContentPanel)
                                {
                                    ImageTransparentColor = Color.Magenta,
                                    Tag = _generatorPanel
                                };
        _sessionFunctionToolStrip.Items.Add(_generatorButton);
        */
        _filtersPanel = new FiltersPanel { Dock = DockStyle.Fill };

        _filtersButton = new ToolStripButton("Filters", Properties.Resources.Filters, UpdateContentPanel)
        {
            ImageTransparentColor = Color.White,
            Tag = _filtersPanel
        };
        _viewToolStrip.Items.Add(_filtersButton);

        _customisePanel = new CustomisePanel { Dock = DockStyle.Fill };

        _customiseButton = new ToolStripButton("Customise", Properties.Resources.Customise, UpdateContentPanel)
        {
            ImageTransparentColor = Color.Magenta,
            Tag = _customisePanel
        };
        _viewToolStrip.Items.Add(_customiseButton);

        _dictionaryPanel = new DictionaryPanel { Dock = DockStyle.Fill };

        //_dictionaryButton = new ToolStripButton("Dictionary", Properties.Resources.Dictionary, UpdateContentPanel)
        //{
        //    ImageTransparentColor = Color.Magenta,
        //    Tag = _dictionaryPanel
        //};
        //_viewToolStrip.Items.Add(_dictionaryButton);

        _parserPanel = new ParserPanel { Dock = DockStyle.Fill };

        _parserButton = new ToolStripButton("Parser", Properties.Resources.Parser, UpdateContentPanel)
        {
            ImageTransparentColor = Color.Magenta,
            Tag = _parserPanel
        };
        _viewToolStrip.Items.Add(_parserButton);

        _logPanel = new LogPanel { Dock = DockStyle.Fill };
        _logButton = new ToolStripButton("Log", Properties.Resources.Log, UpdateContentPanel)
        {
            ImageTransparentColor = Color.Magenta,
            Tag = _logPanel
        };
        _viewToolStrip.Items.Add(_logButton);

        #endregion

        _mainMenu = new MenuStrip
        {
            GripStyle = ToolStripGripStyle.Hidden,
            BackColor = LookAndFeel.Color.ToolStrip
        };

        #region "File Menu"
        var fileMenu = new ToolStripMenuItem("File");

        _fileNew = new ToolStripMenuItem("New...", _newButton.Image);
        _fileNew.Click += NewSessionButtonClick;
        fileMenu.DropDownItems.Add(_fileNew);

        _fileOpen = new ToolStripMenuItem("Open...", _openButton.Image);
        _fileOpen.Click += OpenButtonClick;
        fileMenu.DropDownItems.Add(_fileOpen);

        fileMenu.DropDownItems.Add(new ToolStripSeparator());

        _fileRecent = new ToolStripMenuItem("Recent Files");
        _fileRecent.DropDownItemClicked += OpenButtonDropDownItemClicked;
        fileMenu.DropDownItems.Add(_fileRecent);

        fileMenu.DropDownItems.Add(new ToolStripSeparator());

        _fileExit = new ToolStripMenuItem("Exit");
        _fileExit.Click += FileExitClick;
        fileMenu.DropDownItems.Add(_fileExit);

        #endregion

        #region "Session Menu"
        var sessionMenu = new ToolStripMenuItem("Session");

        _sessionConnect = new ToolStripMenuItem("Connect", _connectButton.Image);
        _sessionConnect.Click += ConnectButtonClick;
        sessionMenu.DropDownItems.Add(_sessionConnect);

        _sessionDisconnect = new ToolStripMenuItem("Disconnect", _disconnectButton.Image);
        _sessionDisconnect.Click += DisconnectButtonClick;
        sessionMenu.DropDownItems.Add(_sessionDisconnect);

        _sessionOptions = new ToolStripMenuItem("Options...", _editSessionButton.Image);
        _sessionOptions.Click += EditSessionButtonClick;
        sessionMenu.DropDownItems.Add(_sessionOptions);

        _sessionReset = new ToolStripMenuItem("Reset", _resetButton.Image);
        _sessionReset.Click += ResetButtonClick;
        sessionMenu.DropDownItems.Add(_sessionReset);
        #endregion

        #region "View Menu"
        var viewMenu = new ToolStripMenuItem("View");

        _viewMessages = new ToolStripMenuItem(_messagesButton.Text, _messagesButton.Image, UpdateContentPanel)
        {
            Tag = _messagesPanel
        };
        viewMenu.DropDownItems.Add(_viewMessages);

        _viewHistory = new ToolStripMenuItem(_historyButton.Text, _historyButton.Image, UpdateContentPanel)
        {
            Tag = _historyPanel
        };
        viewMenu.DropDownItems.Add(_viewHistory);

        _viewOrders = new ToolStripMenuItem(_ordersButton.Text, _ordersButton.Image, UpdateContentPanel)
        {
            Tag = _ordersPanel
        };
        viewMenu.DropDownItems.Add(_viewOrders);

        /*
        _viewGenerator = new ToolStripMenuItem(_generatorButton.Text, _generatorButton.Image, UpdateContentPanel)
                                {
                                    Tag = _generatorPanel
                                };
        viewMenu.DropDownItems.Add(_viewGenerator);
        */
        _viewFilters = new ToolStripMenuItem(_filtersButton.Text, _filtersButton.Image, UpdateContentPanel)
        {
            Tag = _filtersPanel
        };
        viewMenu.DropDownItems.Add(_viewFilters);

        _viewCustomise = new ToolStripMenuItem(_customiseButton.Text, _customiseButton.Image, UpdateContentPanel)
        {
            Tag = _customisePanel
        };
        viewMenu.DropDownItems.Add(_viewCustomise);

        //_viewDictionary = new ToolStripMenuItem(_dictionaryButton.Text, _dictionaryButton.Image, UpdateContentPanel)
        //                      {
        //                          Tag = _dictionaryPanel
        //                      };
        //viewMenu.DropDownItems.Add(_viewDictionary);

        _viewParser = new ToolStripMenuItem(_parserButton.Text, _parserButton.Image, UpdateContentPanel)
        {
            Tag = _parserPanel
        };
        viewMenu.DropDownItems.Add(_viewParser);

        _viewLog = new ToolStripMenuItem(_logButton.Text, _logButton.Image, UpdateContentPanel)
        {
            Tag = _logPanel
        };
        viewMenu.DropDownItems.Add(_viewLog);
        #endregion

        #region "Help Menu"
        var helpMenu = new ToolStripMenuItem("Help");

        _helpAbout = new ToolStripMenuItem("About...");
        _helpAbout.Click += HelpAboutClick;
        helpMenu.DropDownItems.Add(_helpAbout);
        #endregion

        _mainMenu.Items.Add(fileMenu);
        _mainMenu.Items.Add(sessionMenu);
        _mainMenu.Items.Add(viewMenu);
        _mainMenu.Items.Add(helpMenu);

        _container = new ToolStripContainer
        {
            Dock = DockStyle.Fill,
            BackColor = LookAndFeel.Color.ToolStrip
        };

        _container.TopToolStripPanel.BackColor = LookAndFeel.Color.ToolStrip;
        _container.BottomToolStripPanel.BackColor = LookAndFeel.Color.ToolStrip;

        Controls.Add(_container);

        Load += MainFormLoad;

        Size = new Size(1280, 800);
        Text = "FIX Client";
        Icon = Properties.Resources.FixClient;

        LoadMru();
        UpdateUiState();
        //
        // This is just to preload the Dictionary which prevents delays on first use in the GUI.
        //
        foreach (var version in Fix.Dictionary.Versions)
        {
            Console.WriteLine(version.BeginString);
        }
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public override sealed string Text
    {
        get { return base.Text; }
        set { base.Text = value; }
    }

    void MessageDefaultsPanelMessageSelected(Fix.Message message)
    {
        if (message != null)
        {
            _dictionaryPanel.Message = message;
        }
    }

    void MainFormLoad(object? sender, EventArgs e)
    {
        RestoreSizeAndPosition();
        //
        // We have to do this here and not in the constructor or the form hasn't been sized and the
        // toolbars don't get laid out correctly.
        //
        _container.TopToolStripPanel.Join(_mainMenu, 0);
        _container.TopToolStripPanel.Join(_viewToolStrip, 1);
        _container.TopToolStripPanel.Join(_toolStrip, 1);
        UpdateContentPanel(_messagesButton, EventArgs.Empty);
    }

    static void HelpAboutClick(object? sender, EventArgs e)
    {
        using AboutForm form = new();
        form.ShowDialog();
    }

    void FileExitClick(object? sender, EventArgs e)
    {
        Close();
    }

    void UpdateContentPanel(object? sender, EventArgs e)
    {
        Control? control = null;

        if (sender is ToolStripButton stripButton)
        {
            ToolStripButton button = stripButton;

            if (button.Tag != null)
            {
                control = (Control)button.Tag;
            }

        }
        else
        {
            if (sender is ToolStripMenuItem item)
            {
                ToolStripMenuItem menu = item;

                if (menu.Tag != null)
                {
                    control = (Control)menu.Tag;
                }
            }
            else
            {
                return;
            }
        }

        if (control == null)
        {
            _container.ContentPanel.Controls.Clear();
            return;
        }

        _container.ContentPanel.SuspendLayout();
        _container.TopToolStripPanel.SuspendLayout();

        if (!_container.ContentPanel.Controls.Contains(control))
        {
            _container.ContentPanel.Controls.Add(control);
        }

        if (_currentPanel != control)
        {
            if (_currentPanel != null)
            {
                if (_currentPanel.ToolStripMenuItem != null)
                {
                    _mainMenu.Items.Remove(_currentPanel.ToolStripMenuItem);
                }

                _currentPanel = null;
            }

            if (control is FixClientPanel)
            {
                _currentPanel = control as FixClientPanel;

                if (_currentPanel?.ToolStripMenuItem is not null)
                {
                    _mainMenu.Items.Insert(_mainMenu.Items.Count - 1, _currentPanel.ToolStripMenuItem);
                }
            }

            control.BringToFront();
        }

        _container.TopToolStripPanel.ResumeLayout();
        _container.ContentPanel.ResumeLayout();
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        if (CurrentSession != null)
        {
            if (CurrentSession.Connected)
            {
                DialogResult result = MessageBox.Show(this,
                    "Are you sure you want to disconnect the active session and exit?",
                    Application.ProductName,
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result != DialogResult.Yes)
                {
                    e.Cancel = true;
                    return;
                }
            }

            CurrentSession = null;
        }

        DisconnectButtonClick(this, EventArgs.Empty);
        SaveSizeAndPosition();
        Properties.Settings.Default.Save();
        base.OnClosing(e);
    }

    void UpdateUiState()
    {
        //
        // toolbar buttons.
        //
        _connectButton.Enabled = false;
        _disconnectButton.Enabled = false;
        _editSessionButton.Enabled = false;
        _resetButton.Enabled = false;
        //
        // menu items.
        //
        _sessionConnect.Enabled = false;
        _sessionDisconnect.Enabled = false;
        _sessionOptions.Enabled = false;
        _sessionReset.Enabled = false;

        _messagesPanel.UpdateUiState();

        if (CurrentSession == null)
            return;
        //
        // toolbar buttons.
        //
        _editSessionButton.Enabled = true;
        _sessionOptions.Enabled = true;

        if (!CurrentSession.Connected)
        {
            //
            // toolbar buttons.
            //
            _connectButton.Enabled = true;
            _disconnectButton.Enabled = false;
            _resetButton.Enabled = true;

            //
            // menu items.
            //
            _sessionConnect.Enabled = true;
            _sessionDisconnect.Enabled = false;
            _sessionReset.Enabled = true;
        }
        else
        {
            //
            // toolbar buttons.
            //
            _connectButton.Enabled = false;
            _disconnectButton.Enabled = true;
            _resetButton.Enabled = false;

            //
            // menu items.
            //
            _sessionConnect.Enabled = false;
            _sessionDisconnect.Enabled = true;
            _sessionReset.Enabled = false;
        }
    }

    void LoadMru()
    {
        string cacheName = string.Format("{0}\\MRU", Application.UserAppDataPath);

        if (File.Exists(cacheName))
        {
            using FileStream stream = new(cacheName, FileMode.Open);
            try
            {
                var ser = new XmlSerializer(typeof(List<string>));

                if (ser.Deserialize(stream) is not List<string> deserialisedList)
                {
                    return;
                }

                _mru = deserialisedList;

                foreach (string filename in _mru)
                {
                    AddToMruMenu(filename);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this,
                                ex.Message,
                                Application.ProductName,
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
            }
        }
    }

    void SaveMru()
    {
        string cacheName = string.Format("{0}\\MRU", Application.UserAppDataPath);
        using FileStream stream = new(cacheName, FileMode.Create);
        var ser = new XmlSerializer(typeof(List<string>));
        ser.Serialize(stream, _mru);
    }

    void AddToMruMenu(string filename)
    {
        var toolStripItem = new ToolStripMenuItem
        {
            Text = Path.GetFileName(filename),
            ToolTipText = filename

        };
        _openRecentButton.DropDownItems.Add(toolStripItem);

        var menuItem = new ToolStripMenuItem
        {
            Text = Path.GetFileName(filename),
            ToolTipText = filename
        };
        _fileRecent.DropDownItems.Add(menuItem);
    }

    void AddToMru(string filename)
    {
        string root = Path.GetDirectoryName(filename) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(filename);
        if (_mru.Contains(root))
            return;
        _mru.Add(root);
        AddToMruMenu(root);
        SaveMru();
    }

    void OpenButtonClick(object? sender, EventArgs e)
    {
        using OpenFileDialog dlg = new();

        if (dlg.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        try
        {
            CurrentSession = new Session(this);
            _logPanel.Session = CurrentSession;
            CurrentSession.FileName = dlg.FileName;
            CurrentSession.Read();
            CurrentSession.StateChanged += CurrentSessionStateChanged;

            AddToMru(dlg.FileName);

            _messagesPanel.Session = CurrentSession;
            _historyPanel.Session = CurrentSession;
            _filtersPanel.Session = CurrentSession;
            _ordersPanel.Session = CurrentSession;
            _generatorPanel.Session = CurrentSession;
            _customisePanel.Session = CurrentSession;
        }
        catch (Exception ex)
        {
            CurrentSession = null;
            MessageBox.Show(this,
                            ex.Message,
                            Application.ProductName,
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
        }
        finally
        {
            UpdateUiState();
        }
    }

    void CurrentSessionStateChanged(object? sender, Fix.Session.StateEvent ev)
    {
        if (InvokeRequired)
        {
            BeginInvoke(new MethodInvoker(() => CurrentSessionStateChanged(sender, ev)));
            return;
        }

        if (ev.State == Fix.State.Disconnected)
        {
            if (!_expectingDisconnect)
            {
                if (CurrentSession?.Behaviour == Fix.Behaviour.Acceptor)
                {
                    ConnectButtonClick(this, EventArgs.Empty);
                    return;
                }
                else
                {
                    MessageBox.Show(this,
                        "The remote server has disconnected",
                        Application.ProductName,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            }

            _expectingDisconnect = false;
            UpdateUiState();
        }
    }

    void NewSessionButtonClick(object? sender, EventArgs e)
    {
        using (SessionForm form = new())
        {
            form.Session = new Session(this)
            {
                SenderCompId = "INITIATOR",
                TargetCompId = "ACCEPTOR"
            };

            for (; ; )
            {
                if (form.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                CurrentSession = form.Session;

                if (SaveSessionFile())
                {
                    break;
                }
            }

            CurrentSession.StateChanged += CurrentSessionStateChanged;

            _messagesPanel.Session = CurrentSession;
            _historyPanel.Session = CurrentSession;
            _filtersPanel.Session = CurrentSession;
            _ordersPanel.Session = CurrentSession;
            _generatorPanel.Session = CurrentSession;
            _customisePanel.Session = CurrentSession;
            _logPanel.Session = CurrentSession;
        }

        UpdateUiState();
    }

    bool SaveSessionFile()
    {
        if (string.IsNullOrEmpty(CurrentSession?.FileName))
        {
            return FileSaveAsClick(this, EventArgs.Empty);
        }

        CurrentSession.Write();
        return true;
    }

    bool FileSaveAsClick(object? sender, EventArgs e)
    {
        if (CurrentSession is not Session current)
        {
            return false;
        }

        string filename;

        if (!string.IsNullOrEmpty(current.FileName))
        {
            filename = Path.GetFileName(current.FileName);
        }
        else
        {
            filename = current.SenderCompId + "-" + current.TargetCompId + ".session";
        }

        using SaveFileDialog dlg = new();
        
        dlg.Filter = "txt files (*.session)|*.session|All files (*.*)|*.*";
        dlg.FilterIndex = 2;
        dlg.RestoreDirectory = true;
        dlg.FileName = filename;

        if (dlg.ShowDialog() == DialogResult.OK)
        {
            CurrentSession.FileName = dlg.FileName;
            CurrentSession.Write();
            AddToMru(dlg.FileName);
            return true;
        }

        return false;
    }

    void ConnectButtonClick(object? sender, EventArgs e)
    {
        if (CurrentSession is null)
        {
            return;
        }

        IPEndPoint endPoint;
        IPEndPoint bindEndPoint;

        try
        {
            endPoint = CurrentSession.EndPoint();
            bindEndPoint = CurrentSession.BindEndPoint();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this,
                            ex.Message,
                            Application.ProductName,
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
            return;
        }

        string title;

        if (CurrentSession.Behaviour == Fix.Behaviour.Initiator)
        {
            title = string.Format("Connecting to remote FIX server at {0}", endPoint);
        }
        else
        {
            title = string.Format("Listening for FIX connections at {0}",
                                    string.IsNullOrEmpty(CurrentSession.BindHost) ? string.Format("*:{0}", endPoint.Port) : endPoint.ToString());
        }

        CurrentSession.LogInformation(title);

        try
        {
            using (ConnectForm form = new(bindEndPoint, endPoint, CurrentSession.Behaviour))
            {
                form.Text = title;
                _connectButton.Enabled = false;

                if (form.ShowDialog() != DialogResult.OK)
                    return;

                CurrentSession.Stream = form.Stream;
            }

            CurrentSession.Open();
        }
        finally
        {
            UpdateUiState();
        }
    }

    void DisconnectButtonClick(object? sender, EventArgs e)
    {
        if (CurrentSession != null)
        {
            _expectingDisconnect = true;
            CurrentSession.Close();
        }
        UpdateUiState();
    }

    void EditSessionButtonClick(object? sender, EventArgs e)
    {
        using SessionForm form = new();
        form.Readonly = CurrentSession != null && CurrentSession.Connected;

        Session? clone = null;

        if (CurrentSession != null)
        {
            clone = (Session)CurrentSession.Clone();
        }

        form.Session = clone;

        if (form.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        if (CurrentSession != null && clone != null)
        {
            if (CurrentSession.BeginString.BeginString != clone.BeginString.BeginString ||
                CurrentSession.DefaultApplVerId.BeginString != clone.DefaultApplVerId.BeginString)
            {
                CurrentSession.ResetMessageTemplates();

            }

            CurrentSession.CopyPropertiesFrom(clone);
        }

        SaveSessionFile();

        _ordersPanel.Session = CurrentSession;
        //_generatorPanel.Session = Session;
        _filtersPanel.Session = CurrentSession;
        _customisePanel.Session = CurrentSession;
        _messagesPanel.Session = CurrentSession;
    }

    void ResetButtonClick(object? sender, EventArgs e)
    {
        if (CurrentSession == null)
            return;

        using var form = new ResetForm();
        form.StartPosition = FormStartPosition.CenterParent;

        if (form.ShowDialog(this) != DialogResult.Yes)
            return;

        CurrentSession.Reset(form.ResetGeneratedIds, form.Retain);
    }

    void OpenButtonDropDownItemClicked(object? sender, ToolStripItemClickedEventArgs e)
    {
        try
        {
            if (e.ClickedItem is null)
            {
                return;
            }

            CurrentSession = new Session(this);
            _logPanel.Session = CurrentSession;
            CurrentSession.FileName = e.ClickedItem.ToolTipText;
            CurrentSession.Read();
            CurrentSession.StateChanged += CurrentSessionStateChanged;

            _messagesPanel.Session = CurrentSession;
            _filtersPanel.Session = CurrentSession;
            _historyPanel.Session = CurrentSession;
            _ordersPanel.Session = CurrentSession;
            _generatorPanel.Session = CurrentSession;
            _customisePanel.Session = CurrentSession;

            UpdateUiState();
        }
        catch (Exception ex)
        {
            CurrentSession = null;

            MessageBox.Show(this,
                            ex.Message,
                            Application.ProductName,
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
        }
    }

    void RestoreSizeAndPosition()
    {
        //
        // Position the form as it was when we last shutdown.
        //
        string cacheName = string.Format("{0}\\Location", Application.UserAppDataPath);

        if (File.Exists(cacheName))
        {
            using FileStream stream = new(cacheName, FileMode.Open);
            try
            {
                var ser = new XmlSerializer(typeof(Point));

                if (ser.Deserialize(stream) is Point point)
                {
                    Location = point;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this,
                                ex.Message,
                                Application.ProductName,
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
            }
        }

        cacheName = string.Format("{0}\\Size", Application.UserAppDataPath);

        if (File.Exists(cacheName))
        {
            using FileStream stream = new(cacheName, FileMode.Open);
            try
            {
                var ser = new XmlSerializer(typeof(Size));

                if (ser.Deserialize(stream) is Size size)
                {
                    Size = size;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this,
                                ex.Message,
                                Application.ProductName,
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
            }
        }
    }

    void SaveSizeAndPosition()
    {
        //
        // Save the size and position of the form so we can put it back where it was next time
        // we load.
        //
        if (WindowState == FormWindowState.Minimized)
            return;

        string cacheName = string.Format("{0}\\Location", Application.UserAppDataPath);

        using (FileStream stream = new(cacheName, FileMode.Create))
        {
            var ser = new XmlSerializer(typeof(Point));
            ser.Serialize(stream, Location);
        }

        cacheName = string.Format("{0}\\Size", Application.UserAppDataPath);

        using (FileStream stream = new(cacheName, FileMode.Create))
        {
            var ser = new XmlSerializer(typeof(Size));
            ser.Serialize(stream, Size);
        }
    }
}
