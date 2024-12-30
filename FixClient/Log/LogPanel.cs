/////////////////////////////////////////////////
//
// FIX Client
//
// Copyright @ 2021 VIRTU Financial Inc.
// All rights reserved.
//
// Filename: LogPanel.cs
// Author:   Gary Hughes
//
/////////////////////////////////////////////////
using System.ComponentModel;
using System.Data;
using System.Drawing;

namespace FixClient;

public partial class LogPanel : FixClientPanel
{
    const uint MaximumMessages = 1000;
    readonly LogMessageDataGridView _messageGrid;
    readonly LogMessageDataTable _messageTable = new("Messages");
    readonly DataView _messageView;
    readonly ToolStripButton _clearButton;
    readonly SearchTextBox _searchTextBox;
    Session? _session;

    public LogPanel()
    {
        InitializeComponent();

        #region Toolstrip
        _clearButton = new ToolStripButton(Properties.Resources.Clear)
        {
            ToolTipText = "Clear all log messages",
            ImageTransparentColor = Color.Magenta
        };
        _clearButton.Click += ClearButtonClick;

        var toolstrip = new ToolStrip(new ToolStripItem[]
        {
            _clearButton,
        })
        {
            GripStyle = ToolStripGripStyle.Hidden,
            BackColor = LookAndFeel.Color.ToolStrip,
            Renderer = new ToolStripRenderer()
        };
        #endregion

        _searchTextBox = new SearchTextBox
        {
            Dock = DockStyle.Top
        };
        _searchTextBox.TextChanged += SearchTextBoxTextChanged;

        _messageView = new DataView(_messageTable);

        _messageGrid = new LogMessageDataGridView
        {
            Dock = DockStyle.Fill,
            DataSource = _messageView
        };

        var container = new ToolStripContainer
        {
            Dock = DockStyle.Fill,
            BackColor = LookAndFeel.Color.ToolStrip,
        };
        container.TopToolStripPanel.BackColor = LookAndFeel.Color.ToolStrip;

        container.ContentPanel.Controls.Add(_messageGrid);
        container.ContentPanel.Controls.Add(_searchTextBox);
        container.TopToolStripPanel.Join(toolstrip);

        ContentPanel.Controls.Add(container);

        IntPtr handle = _messageGrid.Handle;
        handle = Handle;
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Session Session
    {
        set
        {
            if (_session != null)
            {
                _session.Information -= SessionInformation;
                _session.Warning -= SessionWarning;
                _session.Error -= SessionError;
            }

            _session = value;

            if (_session != null)
            {
                _session.Information += SessionInformation;
                _session.Warning += SessionWarning;
                _session.Error += SessionError;
            }
        }
    }

    void SessionError(object sender, Fix.Session.LogEvent ev)
    {
        AddMessage(LogLevel.Error, ev.TimeStamp, ev.Message);
    }

    void SessionWarning(object sender, Fix.Session.LogEvent ev)
    {
        AddMessage(LogLevel.Warn, ev.TimeStamp, ev.Message);
    }

    void SessionInformation(object sender, Fix.Session.LogEvent ev)
    {
        AddMessage(LogLevel.Info, ev.TimeStamp, ev.Message);
    }

    void AddMessage(LogLevel level, DateTime timestamp, string message)
    {
        if (InvokeRequired)
        {
            BeginInvoke(new MethodInvoker(() => AddMessage(level, timestamp, message)));
            return;
        }

        try
        {
            _messageTable.BeginLoadData();

            if (_messageTable.Rows.Count > MaximumMessages)
                _messageTable.Rows.RemoveAt(0);

            DataRow row = _messageTable.NewRow();

            row[LogMessageDataTable.ColumnTimestamp] = timestamp.TimeOfDay.ToString();
            row[LogMessageDataTable.ColumnMessage] = message;
            row[LogMessageDataTable.ColumnLevel] = level;

            _messageTable.Rows.Add(row);
        }
        finally
        {
            _messageTable.EndLoadData();
        }
        //
        // Always scroll to the bottom so we can see the latest messages.
        //
        if (_messageGrid.Rows.Count > 0)
        {
            _messageGrid.FirstDisplayedScrollingRowIndex = _messageGrid.Rows.Count - 1;
        }
    }

    void SearchTextBoxTextChanged(object? sender, EventArgs e)
    {
        _messageView.RowFilter = string.IsNullOrEmpty(_searchTextBox.Text) ? null : string.Format("Message LIKE '*{0}*'", _searchTextBox.Text);
        _searchTextBox.Focus();
    }

    void ClearButtonClick(object? sender, EventArgs e)
    {
        DialogResult result = MessageBox.Show(this,
                                                "This will delete all log messages, are you sure?",
                                                Application.ProductName,
                                                MessageBoxButtons.YesNo,
                                                MessageBoxIcon.Question);

        if (result != DialogResult.Yes)
            return;

        _messageTable.Clear();
    }
}

