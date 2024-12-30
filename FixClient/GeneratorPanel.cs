/////////////////////////////////////////////////
//
// FIX Client
//
// Copyright @ 2021 VIRTU Financial Inc.
// All rights reserved.
//
// Filename: GeneratorPanel.cs
// Author:   Gary Hughes
//
/////////////////////////////////////////////////
using System.ComponentModel;
using System.Drawing;

namespace FixClient;

public partial class GeneratorPanel : FixClientPanel
{
    readonly ToolStripButton _startButton;
    readonly ToolStripButton _pauseButton;
    readonly ToolStripButton _stopButton;

    readonly ToolStripMenuItem _startMenuItem;
    readonly ToolStripMenuItem _pauseMenuItem;
    readonly ToolStripMenuItem _stopMenuItem;

    Session? _session;
    //Fix.OrderGenerator _generator;

    public GeneratorPanel()
    {
        InitializeComponent();

        #region ToolStrip
        var toolStrip = new ToolStrip
        {
            GripStyle = ToolStripGripStyle.Hidden,
            BackColor = LookAndFeel.Color.ToolStrip,
            Renderer = new ToolStripRenderer()
        };
        TopToolStripPanel.Join(toolStrip);

        _startButton = new ToolStripButton
        {
            ToolTipText = "Start the generator",
            Image = Properties.Resources.Start,
            ImageTransparentColor = Color.Black
        };
        _startButton.Click += StartButtonClick;
        toolStrip.Items.Add(_startButton);

        _pauseButton = new ToolStripButton
        {
            ToolTipText = "Pause the generator",
            Image = Properties.Resources.Pause,
            ImageTransparentColor = Color.Black
        };
        _pauseButton.Click += PauseButtonClick;
        toolStrip.Items.Add(_pauseButton);

        _stopButton = new ToolStripButton
        {
            ToolTipText = "Stop the generator",
            Image = Properties.Resources.Stop,
            ImageTransparentColor = Color.Black
        };
        _stopButton.Click += StopButtonClick;
        toolStrip.Items.Add(_stopButton);

        #endregion

        #region MenuStrip
        var menu = new ToolStripMenuItem("Action");

        _startMenuItem = new ToolStripMenuItem("Start",
                                                _startButton.Image,
                                                StartButtonClick);
        menu.DropDownItems.Add(_startMenuItem);

        _pauseMenuItem = new ToolStripMenuItem("Pause",
                                                _pauseButton.Image,
                                                PauseButtonClick);
        menu.DropDownItems.Add(_pauseMenuItem);

        _stopMenuItem = new ToolStripMenuItem("Stop",
                                                _stopButton.Image,
                                                StopButtonClick);
        menu.DropDownItems.Add(_stopMenuItem);

        SetMenuStrip(menu);
        #endregion

        //_generator = new Fix.OrderGenerator();

        UpdateUiState();
    }

    static void StopButtonClick(object? sender, EventArgs e)
    {
    }

    static void PauseButtonClick(object? sender, EventArgs e)
    {
    }

    static void StartButtonClick(object? sender, EventArgs e)
    {
    }

    public static void UpdateUiState()
    {
        /*
        bool enabled = true;

        if (Session == null || Session.Transport == null || !Session.Transport.Connected)
        {
            enabled = false;
        }

        _startButton.Enabled = enabled;
        _startMenuItem.Enabled = enabled;
        _pauseButton.Enabled = enabled;
        _pauseMenuItem.Enabled = enabled;
        _stopButton.Enabled = enabled;
        _stopMenuItem.Enabled = enabled;
        */
    }

    //void Reload()
    //{
    //    UpdateUiState();
    //}

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Session? Session
    {
        get
        {
            return _session;
        }
        set
        {
            /*
            if (_session != null)
            {
                if (_session.Transport != null)
                {
                    _session.Transport.Opened -= TransportOpened;
                    _session.Transport.Closed -= TransportClosed;
                }
            }
            */
            _session = value;
            //_session.MessagesReset += SessionMessagesReset;
            //_session.SessionReset += SessionSessionReset;
            /*
            Reload();
                
            if (_session != null)
            {
                if (_session.Transport != null)
                {
                    _session.Transport.Opened += TransportOpened;
                    _session.Transport.Closed += TransportClosed;
                }
            }
            */
        }
    }
    /*
    void TransportClosed(object sender, EventArgs e)
    {
        if (InvokeRequired)
        {
            BeginInvoke(new MethodInvoker(() => TransportClosed(sender, e)));
            return;
        }

        UpdateUiState();
    }

    void TransportOpened(object sender, EventArgs e)
    {
        if (InvokeRequired)
        {
            BeginInvoke(new MethodInvoker(() => TransportOpened(sender, e)));
            return;
        }

        UpdateUiState();
    }
    */
}

