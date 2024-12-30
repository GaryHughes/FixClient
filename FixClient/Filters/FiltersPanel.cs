/////////////////////////////////////////////////
//
// FIX Client
//
// Copyright @ 2021 VIRTU Financial Inc.
// All rights reserved.
//
// Filename: FiltersPanel.cs
// Author:   Gary Hughes
//
/////////////////////////////////////////////////
using System.ComponentModel;
using System.Data;
using System.Drawing;

namespace FixClient;

partial class FiltersPanel : FixClientPanel
{
    Session? _session;

    readonly FilterMessageDataGridView _messageGrid;
    readonly FilterFieldDataGridView _fieldGrid;

    readonly FilterMessageDataTable _messageTable;
    readonly DataView _messageView;

    readonly FilterFieldDataTable _fieldTable;
    readonly DataView _fieldView;

    readonly ToolStripButton _checkAllMessagesButton;
    readonly ToolStripButton _uncheckAllMessagesButton;

    readonly ToolStripMenuItem _checkAllMessagesMenuItem;
    readonly ToolStripMenuItem _uncheckAllMessagesMenuItem;

    readonly ToolStripButton _checkAllFieldsButton;
    readonly ToolStripButton _uncheckAllFieldsButton;

    readonly ToolStripMenuItem _checkAllFieldsMenuItem;
    readonly ToolStripMenuItem _uncheckAllFieldsMenuItem;

    readonly SearchTextBox _messageSearchTextBox;
    readonly SearchTextBox _fieldSearchTextBox;

    public FiltersPanel()
    {
        InitializeComponent();

        _messageSearchTextBox = new SearchTextBox
        {
            Dock = DockStyle.Top,
        };
        _messageSearchTextBox.TextChanged += MessageSearchTextBoxTextChanged;
        _fieldSearchTextBox = new SearchTextBox
        {
            Dock = DockStyle.Top,
        };
        _fieldSearchTextBox.TextChanged += FieldSearchTextBoxTextChanged;

        #region Message ToolStrip

        var messageToolstrip = new ToolStrip
        {
            GripStyle = ToolStripGripStyle.Hidden,
            BackColor = LookAndFeel.Color.ToolStrip,
            Renderer = new ToolStripRenderer()
        };

        _checkAllMessagesButton = new ToolStripButton
        {
            Image = Properties.Resources.CheckAll,
            ImageTransparentColor = Color.White,
            ToolTipText = "Check all the messages"
        };
        _checkAllMessagesButton.Click += (sender, ev) => SetAllMessagesVisibility(true);
        messageToolstrip.Items.Add(_checkAllMessagesButton);

        _uncheckAllMessagesButton = new ToolStripButton
        {
            Image = Properties.Resources.UnCheckAll,
            ImageTransparentColor = Color.White,
            ToolTipText = "UnCheck all the messages"
        };
        _uncheckAllMessagesButton.Click += (sender, ev) => SetAllMessagesVisibility(false);
        messageToolstrip.Items.Add(_uncheckAllMessagesButton);

        #endregion

        #region Field ToolStrip

        var fieldToolStrip = new ToolStrip
        {
            GripStyle = ToolStripGripStyle.Hidden,
            BackColor = LookAndFeel.Color.ToolStrip
        };

        _checkAllFieldsButton = new ToolStripButton
        {
            Image = Properties.Resources.CheckAll,
            ImageTransparentColor = Color.White,
            ToolTipText = "Check all the fields of the selected message"
        };
        _checkAllFieldsButton.Click += (sender, ev) => SetAllFieldsVisibility(true);
        fieldToolStrip.Items.Add(_checkAllFieldsButton);

        _uncheckAllFieldsButton = new ToolStripButton
        {
            Image = Properties.Resources.UnCheckAll,
            ImageTransparentColor = Color.White,
            ToolTipText = "UnCheck all the fields of the selected message"
        };
        _uncheckAllFieldsButton.Click += (sender, ev) => SetAllFieldsVisibility(false);
        fieldToolStrip.Items.Add(_uncheckAllFieldsButton);

        #endregion

        #region Action Menu

        var menu = new ToolStripMenuItem("Action");
        SetMenuStrip(menu);

        _checkAllMessagesMenuItem = new ToolStripMenuItem("Check All Messages", _checkAllMessagesButton.Image);
        _checkAllMessagesMenuItem.Click += (sender, ev) => SetAllMessagesVisibility(true);
        menu.DropDownItems.Add(_checkAllMessagesMenuItem);

        _uncheckAllMessagesMenuItem = new ToolStripMenuItem("UnCheck All Messages", _uncheckAllMessagesButton.Image);
        _uncheckAllMessagesMenuItem.Click += (sender, ev) => SetAllMessagesVisibility(false);
        menu.DropDownItems.Add(_uncheckAllMessagesMenuItem);

        menu.DropDownItems.Add(new ToolStripSeparator());

        _checkAllFieldsMenuItem = new ToolStripMenuItem("Check All Fields", _checkAllFieldsButton.Image);
        _checkAllFieldsMenuItem.Click += (sender, ev) => SetAllFieldsVisibility(true);
        menu.DropDownItems.Add(_checkAllFieldsMenuItem);

        _uncheckAllFieldsMenuItem = new ToolStripMenuItem("UnCheck All Fields", _uncheckAllFieldsButton.Image);
        _uncheckAllFieldsMenuItem.Click += (sender, ev) => SetAllFieldsVisibility(false);
        menu.DropDownItems.Add(_uncheckAllFieldsMenuItem);


        #endregion

        _messageTable = new FilterMessageDataTable("Messages");
        _messageView = new DataView(_messageTable);

        _fieldTable = new FilterFieldDataTable("Fields");
        _fieldView = new DataView(_fieldTable);

        _messageGrid = new FilterMessageDataGridView
        {
            Dock = DockStyle.Fill,
            VirtualMode = true
        };
        _messageGrid.CellValueNeeded += MessageGridCellValueNeeded;
        _messageGrid.SelectionChanged += MessageGridSelectionChanged;
        _messageGrid.CellContentClick += MessageGridCellContentClick;

        _fieldGrid = new FilterFieldDataGridView
        {
            Dock = DockStyle.Fill,
            VirtualMode = true
        };
        _fieldGrid.CellValueNeeded += FieldGridCellValueNeeded;
        _fieldGrid.CellContentClick += FieldGridCellContentClick;

        var messageContainer = new ToolStripContainer
        {
            Dock = DockStyle.Fill,
            BackColor = LookAndFeel.Color.ToolStrip
        };
        messageContainer.TopToolStripPanel.BackColor = LookAndFeel.Color.ToolStrip;
        messageContainer.ContentPanel.Controls.Add(_messageGrid);
        messageContainer.ContentPanel.Controls.Add(_messageSearchTextBox);
        messageContainer.TopToolStripPanel.Controls.Add(messageToolstrip);

        var fieldContainer = new ToolStripContainer
        {
            Dock = DockStyle.Fill,
            BackColor = LookAndFeel.Color.ToolStrip
        };
        fieldContainer.TopToolStripPanel.BackColor = LookAndFeel.Color.ToolStrip;
        fieldContainer.ContentPanel.Controls.Add(_fieldGrid);
        fieldContainer.ContentPanel.Controls.Add(_fieldSearchTextBox);
        fieldContainer.TopToolStripPanel.Controls.Add(fieldToolStrip);

        var splitter = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Vertical
        };

        splitter.Panel1.Controls.Add(messageContainer);
        splitter.Panel2.Controls.Add(fieldContainer);

        ContentPanel.Controls.Add(splitter);

        UpdateUiState();
    }

    void MessageGridCellContentClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (Session is null)
        {
            return;
        }

        if (e.RowIndex < 0)
        {
            return;
        }

        if (_messageGrid.Columns[e.ColumnIndex].Name != FilterMessageDataTable.ColumnVisible)
        {
            return;
        }

        DataRowView view = _messageView[e.RowIndex];
        DataRow dataRow = view.Row;

        if (dataRow == null)
        {
            return;
        }

        var visible = (bool)dataRow[FilterMessageDataTable.ColumnVisible];
        var msgType = (string)dataRow[FilterMessageDataTable.ColumnMsgType];
        dataRow[FilterMessageDataTable.ColumnVisible] = !visible;
        Session.MessageVisible(msgType, !visible);
    }

    void FieldGridCellContentClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (Session is null)
        {
            return;
        }

        if (e.RowIndex < 0)
        {
            return;
        }

        if (_fieldGrid.Columns[e.ColumnIndex].Name != FilterFieldDataTable.ColumnVisible)
        {
            return;
        }

        DataRowView view = _fieldView[e.RowIndex];
        DataRow dataRow = view.Row;

        if (dataRow == null)
        {
            return;
        }

        if (SelectedMessage is not Fix.Dictionary.Message message)
        {
            return;
        }

        var visible = (bool)dataRow[FilterFieldDataTable.ColumnVisible];
        var tag = (int)dataRow[FilterFieldDataTable.ColumnTag];
        dataRow[FilterFieldDataTable.ColumnVisible] = !visible;
        Session.SetFieldVisible(message.MsgType, tag, !visible);
    }

    void FieldSearchTextBoxTextChanged(object? sender, EventArgs e)
    {
        string? search = null;

        if (string.IsNullOrEmpty(_fieldSearchTextBox.Text))
        {
            _fieldView.Sort = string.Empty;
        }
        else
        {
            search = string.Format("CONVERT({0}, System.String) LIKE '%{2}%' OR {1} LIKE '%{2}%'",
                                    FilterFieldDataTable.ColumnTag,
                                    FilterFieldDataTable.ColumnName,
                                    _fieldSearchTextBox.Text);
        }
        _fieldGrid.RowCount = 0;
        _fieldView.RowFilter = search;
        _fieldGrid.RowCount = _fieldView.Count;
        _fieldSearchTextBox.Focus();
    }

    void MessageSearchTextBoxTextChanged(object? sender, EventArgs e)
    {
        if (Session is null)
        {
            return;
        }

        string? search = null;

        if (string.IsNullOrEmpty(_messageSearchTextBox.Text))
        {
            _messageView.Sort = string.Empty;
        }
        else
        {
            search = string.Format("{1} LIKE '%{0}%' OR {2} LIKE '%{0}%'",
                                    _messageSearchTextBox.Text,
                                    FilterMessageDataTable.ColumnMsgType,
                                    FilterMessageDataTable.ColumnName);
        }

        _messageGrid.RowCount = 0;
        _messageView.RowFilter = Session.MessageRowFilter(search);
        _messageGrid.RowCount = _messageView.Count;
        _messageSearchTextBox.Focus();
    }

    void MessageGridCellValueNeeded(object? sender, DataGridViewCellValueEventArgs e)
    {
        e.Value = _messageView[e.RowIndex][e.ColumnIndex];
    }

    void FieldGridCellValueNeeded(object? sender, DataGridViewCellValueEventArgs e)
    {
        e.Value = _fieldView[e.RowIndex][e.ColumnIndex];
    }

    void SetAllMessagesVisibility(bool visible)
    {
        if (Session is null)
        {
            return;
        }

        try
        {
            //Session.AutoWriteFilters = false;
            _messageTable.BeginLoadData();

            string? msgType = null;

            foreach (DataRow row in _messageTable.Rows)
            {
                msgType = (string)row[FilterMessageDataTable.ColumnMsgType];
                row[FilterMessageDataTable.ColumnVisible] = visible;
                Session.MessageVisible(msgType, visible, false);
            }

            if (!string.IsNullOrEmpty(msgType))
            {
                Session.MessageVisible(msgType, visible);
            }
        }
        finally
        {
            _messageTable.EndLoadData();
            _messageGrid.Refresh();
            Session.WriteFilters();
        }
    }

    void SetAllFieldsVisibility(bool visible)
    {
        if (Session is null)
        {
            return;
        }

        try
        {
            _fieldTable.BeginLoadData();

            if (SelectedMessage is not Fix.Dictionary.Message message)
            {
                return;
            }

            foreach (DataRow row in _fieldTable.Rows)
            {
                var tag = (int)row[FilterFieldDataTable.ColumnTag];
                row[FilterFieldDataTable.ColumnVisible] = visible;
                Session.SetFieldVisible(message.MsgType, tag, visible);
            }
        }
        finally
        {
            _fieldTable.EndLoadData();
            _fieldGrid.Refresh();
            Session.WriteFilters();
        }
    }

    Fix.Dictionary.Message? SelectedMessage
    {
        get
        {
            if (_messageGrid.SelectedRows.Count == 0)
            {
                return null;
            }

            DataRowView view = _messageView[_messageGrid.SelectedRows[0].Index];
            if (view.Row is not FilterMessageDataRow dataRow)
            {
                return null;
            }

            return dataRow.Message;
        }
    }

    void MessageGridSelectionChanged(object? sender, EventArgs e)
    {
        if (Session is null)
        {
            return;
        }

        try
        {
            _fieldGrid.RowCount = 0;
            _fieldTable.BeginLoadData();
            _fieldTable.Clear();

            if (SelectedMessage is not Fix.Dictionary.Message message)
            {
                return;
            }

            foreach (var field in message.Fields)
            {
                DataRow row = _fieldTable.NewRow();
                row[FilterFieldDataTable.ColumnVisible] = Session.IsFieldVisible(message.MsgType, field.Tag);
                row[FilterFieldDataTable.ColumnTag] = field.Tag;
                row[FilterFieldDataTable.ColumnName] = field.Name;
                _fieldTable.Rows.Add(row);
            }
        }
        finally
        {
            _fieldTable.EndLoadData();
            _fieldGrid.RowCount = _fieldTable.Rows.Count;
            UpdateUiState();
        }
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Session? Session
    {
        get { return _session; }
        set
        {
            try
            {
                _session = value;

                if (_session == null)
                    return;

                LoadMessages();

                System.Threading.Tasks.Task.Run(() =>
                {
                    _session.LogInformation("Beginning {0} data dictionary load", _session.Version.BeginString);
                    foreach (var message in _session.Version.Messages)
                    {
                        var fields = message.Fields;
                    }
                    _session.LogInformation("Completed {0} data dictionary load", _session.Version.BeginString);
                });
            }
            finally
            {
                UpdateUiState();
            }
        }
    }

    void LoadMessages()
    {
        if (Session is null)
        {
            return;
        }

        try
        {
            _messageGrid.RowCount = 0;
            _messageTable.BeginLoadData();
            _messageTable.Clear();

            foreach (var message in Session.Version.Messages)
            {
                var row = (FilterMessageDataRow)_messageTable.NewRow();

                row.Message = message;

                row[FilterMessageDataTable.ColumnMsgType] = message.MsgType;
                row[FilterMessageDataTable.ColumnName] = message.Name;
                row[FilterMessageDataTable.ColumnVisible] = Session.IsMessageVisible(message.MsgType);

                _messageTable.Rows.Add(row);
            }
        }
        finally
        {
            _messageGrid.RowCount = _messageView.Count;
            _messageTable.EndLoadData();
        }
    }

    void UpdateUiState()
    {
        _messageSearchTextBox.Enabled = _session != null;
        _fieldSearchTextBox.Enabled = _session != null;
        _checkAllMessagesButton.Enabled = _session != null;
        _uncheckAllMessagesButton.Enabled = _session != null;
        _checkAllMessagesMenuItem.Enabled = _session != null;
        _uncheckAllMessagesMenuItem.Enabled = _session != null;
        _checkAllFieldsButton.Enabled = _session != null;
        _uncheckAllFieldsButton.Enabled = _session != null;
        _checkAllFieldsMenuItem.Enabled = _session != null;
        _uncheckAllFieldsMenuItem.Enabled = _session != null;
    }
}
