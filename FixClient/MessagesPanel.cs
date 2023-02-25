/////////////////////////////////////////////////
//
// FIX Client
//
// Copyright @ 2021 VIRTU Financial Inc.
// All rights reserved.
//
// Filename: MessagesPanel.cs
// Author:   Gary Hughes
//
/////////////////////////////////////////////////
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using static Fix.Dictionary;

namespace FixClient;

partial class MessagesPanel : FixClientPanel
{
    readonly MessageTypeDataGridView _messageGrid;
    readonly MessageTypeDataTable _messageTable;
    readonly DataView _messageView;
    readonly BindingSource _messageBindingSource;
    string? _selectedMsgType;
    bool _suppressMsgTypeUpdate;

    readonly EditableMessageFieldDataGridView _fieldGrid;
    FieldDataTable? _fieldTable;
    DataView? _fieldView;

    readonly TabControl _tabControl;
    readonly NativeTabControl _nativeTabControl = new(new Padding(-4, -2, 4, 4));
    readonly InspectorPanel _inspectorPanel;
    readonly MessageOptionsPanel _messageOptionsPanel;

    readonly SearchTextBox _messageSearchTextBox;
    readonly SearchTextBox _fieldSearchTextBox;

    Session? _session;

    readonly Timer _timer;

    readonly ToolStripButton _sendButton;
    readonly ToolStripButton _filterButton;
    readonly ToolStripButton _removeFilterButton;
    readonly ToolStripButton _repeatButton;
    readonly ToolStripButton _removeButton;
    readonly ToolStripButton _resetButton;
    readonly ToolStripButton _pasteButton;
    readonly ToolStripButton _editGoaButton;

    readonly ToolStripMenuItem _sendMenuItem;
    readonly ToolStripMenuItem _filterMenuItem;
    readonly ToolStripMenuItem _removeFilterMenuItem;
    readonly ToolStripMenuItem _repeatMenuItem;
    readonly ToolStripMenuItem _removeMenuItem;
    readonly ToolStripMenuItem _resetMenuItem;
    readonly ToolStripMenuItem _pasteMenuItem;
    readonly ToolStripMenuItem _editGoaMenuItem;

    readonly ToolStripMenuItem _repeatContextMenuItem;
    readonly ToolStripMenuItem _removeContextMenuItem;
    readonly ToolStripMenuItem _resetContextMenuItem;
    readonly ToolStripMenuItem _insertContextMenuItem;

    readonly ContextMenuStrip _contextMenu;
    //
    // This event is used by the mainform to updated the context of the dictionary view
    //
    public delegate void MessageDelegate(Fix.Message message);
    public event MessageDelegate? MessageSelected;

    protected void OnMessageSelected(Fix.Message message)
    {
        if (!_suppressMsgTypeUpdate)
        {
            _selectedMsgType = message.MsgType;
        }

        MessageSelected?.Invoke(message);
    }

    public MessagesPanel()
    {
        var leftSplitter = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Vertical
        };

        var rightSplitter = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Vertical,
            SplitterDistance = 1000
        };

        _messageTable = new MessageTypeDataTable("MessageTypes");
        _messageView = new DataView(_messageTable);
        _messageBindingSource = new BindingSource { DataSource = _messageView };

        _messageGrid = new MessageTypeDataGridView { Dock = DockStyle.Fill };
        _messageGrid.SelectionChanged += MessageGridSelectionChanged;
        _messageGrid.DataSource = _messageBindingSource;

        _fieldGrid = new EditableMessageFieldDataGridView();
        _fieldGrid.SelectionChanged += FieldGridSelectionChanged;
        _fieldGrid.CellContextMenuStripNeeded += FieldGridCellContextMenuStripNeeded;
        _fieldGrid.FieldValueChanged += sender => _session?.SetDirty();

        _inspectorPanel = new InspectorPanel { Dock = DockStyle.Fill };
        _messageOptionsPanel = new MessageOptionsPanel { Dock = DockStyle.Fill };

        var inspectorPage = new TabPage("Dictionary");
        inspectorPage.Controls.Add(_inspectorPanel);

        var optionsPage = new TabPage("Options");
        optionsPage.Controls.Add(_messageOptionsPanel);

        _tabControl = new TabControl { Dock = DockStyle.Fill };
        _nativeTabControl.AssignHandle(_tabControl.Handle);

        _tabControl.TabPages.Add(inspectorPage);
        _tabControl.TabPages.Add(optionsPage);


        #region Message ToolStrip

        var messageToolStrip = new ToolStrip
        {
            GripStyle = ToolStripGripStyle.Hidden,
            BackColor = LookAndFeel.Color.ToolStrip,
            Renderer = new ToolStripRenderer()
        };

        _sendButton = new ToolStripButton
        {
            ToolTipText = "Send the selected message to the remote server",
            Image = Properties.Resources.Send,
            ImageTransparentColor = Color.White
        };
        _sendButton.Click += SendButtonClick;
        messageToolStrip.Items.Add(_sendButton);

        #endregion

        #region Field ToolStrip

        var fieldToolStrip = new ToolStrip
        {
            GripStyle = ToolStripGripStyle.Hidden,
            BackColor = LookAndFeel.Color.ToolStrip,
            Renderer = new ToolStripRenderer()
        };

        _filterButton = new ToolStripButton
        {
            ToolTipText = "Filter all optional fields which do not have assigned values",
            Image = Properties.Resources.Filter,
            ImageTransparentColor = Color.White
        };
        _filterButton.Click += FilterButtonClick;
        fieldToolStrip.Items.Add(_filterButton);

        _removeFilterButton = new ToolStripButton
        {
            ToolTipText = "Remove all filters from the optional fields",
            Image = Properties.Resources.RemoveFilter,
            ImageTransparentColor = Color.White
        };
        _removeFilterButton.Click += RemoveFilterButtonClick;
        fieldToolStrip.Items.Add(_removeFilterButton);

        _repeatButton = new ToolStripButton
        {
            ToolTipText = "Repeat the selected fields",
            Image = Properties.Resources.RepeatFields,
            ImageTransparentColor = Color.Magenta
        };
        _repeatButton.Click += RepeatButtonClick;
        fieldToolStrip.Items.Add(_repeatButton);

        _removeButton = new ToolStripButton
        {
            ToolTipText = "Remove the selected fields",
            Image = Properties.Resources.RemoveFields,
            ImageTransparentColor = Color.Magenta
        };
        _removeButton.Click += RemoveButtonClick;
        fieldToolStrip.Items.Add(_removeButton);

        _resetButton = new ToolStripButton
        {
            ToolTipText = "Reset the fields of this message to their defaults",
            Image = Properties.Resources.ResetFields,
            ImageTransparentColor = Color.Magenta
        };
        _resetButton.Click += ResetButtonClick;
        fieldToolStrip.Items.Add(_resetButton);

        _pasteButton = new ToolStripButton
        {
            ToolTipText = "Paste a formatted log message or raw FIX message",
            Image = Properties.Resources.Paste,
            ImageTransparentColor = Color.White
        };
        _pasteButton.Click += async (sender, ev) => await PasteButtonClick(sender, ev);
        fieldToolStrip.Items.Add(_pasteButton);

        _editGoaButton = new ToolStripButton
        {
            ToolTipText = "Edit GATE Generic Order Attributes",
            Image = Properties.Resources.GOA,
            ImageTransparentColor = Color.Magenta
        };
        _editGoaButton.Click += EditGoaButtonClick;
        fieldToolStrip.Items.Add(_editGoaButton);
        #endregion

        #region MenuStrip
        var menu = new ToolStripMenuItem("Action");

        _sendMenuItem = new ToolStripMenuItem("Send",
                                                _sendButton.Image,
                                                SendButtonClick);
        menu.DropDownItems.Add(_sendMenuItem);

        menu.DropDownItems.Add(new ToolStripSeparator());

        _filterMenuItem = new ToolStripMenuItem("Filter",
                                                _filterButton.Image,
                                                FilterButtonClick);
        menu.DropDownItems.Add(_filterMenuItem);

        _removeFilterMenuItem = new ToolStripMenuItem("Remove Filters",
                                                        _removeFilterButton.Image,
                                                        RemoveFilterButtonClick);
        menu.DropDownItems.Add(_removeFilterMenuItem);


        _repeatMenuItem = new ToolStripMenuItem("Repeat Fields",
                                                _repeatButton.Image,
                                                RepeatButtonClick);
        menu.DropDownItems.Add(_repeatMenuItem);

        _removeMenuItem = new ToolStripMenuItem("Remove Fields",
                                                _removeButton.Image,
                                                RemoveButtonClick);

        menu.DropDownItems.Add(_removeMenuItem);

        _resetMenuItem = new ToolStripMenuItem("Reset Fields",
                                                _resetButton.Image,
                                                ResetButtonClick);

        menu.DropDownItems.Add(_resetMenuItem);

        _pasteMenuItem = new ToolStripMenuItem("Paste",
                                                _pasteButton.Image,
                                                async (sender, ev) => await PasteButtonClick(sender, ev),
                                                Keys.Control | Keys.V);
        menu.DropDownItems.Add(_pasteMenuItem);

        _editGoaMenuItem = new ToolStripMenuItem("Edit GOA",
                                                _editGoaButton.Image,
                                                EditGoaButtonClick,
                                                Keys.Control | Keys.G);
        menu.DropDownItems.Add(_editGoaMenuItem);

        SetMenuStrip(menu);
        #endregion

        #region ContextMenu
        _contextMenu = new ContextMenuStrip();

        _repeatContextMenuItem = new ToolStripMenuItem("Repeat Fields",
                                                        _repeatButton.Image,
                                                        RepeatButtonClick);

        _removeContextMenuItem = new ToolStripMenuItem("Remove Fields",
                                                        _removeButton.Image,
                                                        RemoveButtonClick);

        _insertContextMenuItem = new ToolStripMenuItem("Insert Custom Field",
                                                        Properties.Resources.Customise)
        {
            ImageTransparentColor = Color.Magenta
        };

        _resetContextMenuItem = new ToolStripMenuItem("Reset Fields",
                                                        _resetButton.Image,
                                                        ResetButtonClick);

        _contextMenu.Items.Add(_repeatContextMenuItem);
        _contextMenu.Items.Add(_removeContextMenuItem);
        _contextMenu.Items.Add(_insertContextMenuItem);
        _contextMenu.Items.Add(_resetContextMenuItem);
        #endregion

        _messageSearchTextBox = new SearchTextBox
        {
            Dock = DockStyle.Top
        };
        _messageSearchTextBox.TextChanged += MessageSearchTextBoxTextChanged;
        _fieldSearchTextBox = new SearchTextBox
        {
            Dock = DockStyle.Top
        };
        _fieldSearchTextBox.TextChanged += FieldSearchTextBoxTextChanged;


        var messageContainer = new ToolStripContainer
        {
            Dock = DockStyle.Fill,
            BackColor = LookAndFeel.Color.ToolStrip
        };
        messageContainer.TopToolStripPanel.BackColor = LookAndFeel.Color.ToolStrip;
        messageContainer.ContentPanel.Controls.Add(_messageGrid);
        messageContainer.ContentPanel.Controls.Add(_messageSearchTextBox);
        messageContainer.TopToolStripPanel.Controls.Add(messageToolStrip);

        var fieldContainer = new ToolStripContainer
        {
            Dock = DockStyle.Fill,
            BackColor = LookAndFeel.Color.ToolStrip
        };

        fieldContainer.TopToolStripPanel.BackColor = LookAndFeel.Color.ToolStrip;
        fieldContainer.ContentPanel.Controls.Add(_fieldGrid);
        fieldContainer.ContentPanel.Controls.Add(_fieldSearchTextBox);
        fieldContainer.TopToolStripPanel.Controls.Add(fieldToolStrip);

        leftSplitter.Panel1.Controls.Add(messageContainer);
        leftSplitter.Panel2.Controls.Add(fieldContainer);

        rightSplitter.Panel1.Controls.Add(leftSplitter);
        rightSplitter.Panel2.Controls.Add(_tabControl);

        ContentPanel.Controls.Add(rightSplitter);

        _timer = new Timer
        {
            Interval = 1000,
            Enabled = true
        };
        _timer.Tick += TimerTick;

        InitializeComponent();

        UpdateUiState();
    }

    void ApplyFieldSearch()
    {
        if (_fieldView == null)
        {
            return;
        }

        string? search = null;

        if (string.IsNullOrEmpty(_fieldSearchTextBox.Text))
        {
            _fieldView.Sort = string.Empty;
        }
        else
        {
            search = string.Format("CONVERT({0}, System.String) LIKE '%{3}%' OR {1} LIKE '%{3}%' OR {2} LIKE '%{3}%'",
                                        FieldDataTable.ColumnTag,
                                        FieldDataTable.ColumnName,
                                        FieldDataTable.ColumnValue,
                                        _fieldSearchTextBox.Text);
        }

        if (_session is null)
        {
            return;
        }

        if (SelectedMessage is null)
        {
            return;
        }

        _fieldView.RowFilter = _session.FieldRowFilter(SelectedMessage.MsgType, search);
    }

    void FieldSearchTextBoxTextChanged(object? sender, EventArgs e)
    {
        if (_fieldView == null || SelectedMessage == null)
        {
            return;
        }
        ApplyFieldSearch();
        _fieldSearchTextBox.Focus();
    }

    void MessageSearchTextBoxTextChanged(object? sender, EventArgs e)
    {
        if (_messageView == null)
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
            search = string.Format("{0} LIKE '%{2}%' OR {1} LIKE '%{2}%'",
                                    MessageTypeDataTable.ColumnSearchMsgType,
                                    MessageTypeDataTable.ColumnSearchMsgTypeDescription,
                                    _messageSearchTextBox.Text.ToUpper());
        }

        if (_session is null)
        {
            return;
        }

        _messageView.RowFilter = _session.MessageRowFilter(search);
        _messageSearchTextBox.Focus();
    }

    Fix.Message? SelectedMessage
    {
        get
        {
            if (_messageGrid.SelectedRows.Count == 0)
            {
                return null;
            }

            if (_messageGrid.SelectedRows[0].DataBoundItem is not DataRowView rowView)
            {
                return null;
            }

            if (rowView.Row is not MessageTypeDataRow messageRow)
            {
                return null;
            }

            if (messageRow.Message is not Fix.Dictionary.Message message)
            {
                return null;
            }

            return _session?.MessageForTemplate(message);
        }
    }

    Fix.Field? SelectedField
    {
        get
        {
            if (_fieldGrid.SelectedRows.Count == 0)
                return null;

            if (_fieldGrid.SelectedRows[0].DataBoundItem is not DataRowView rowView)
                return null;

            if (rowView.Row is not FieldDataRow fieldRow)
                return null;

            return fieldRow.Field;
        }
    }

    void EditGoaButtonClick(object? sender, EventArgs e)
    {
        Fix.Message? message = SelectedMessage;

        if (message == null || _fieldGrid.SelectedRows.Count != 1)
        {
            MessageBox.Show(this,
                            "You must select a single field to edit GOA",
                            Application.ProductName,
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
            return;
        }

        DataGridViewRow row = _fieldGrid.SelectedRows[0];

        if (row.DataBoundItem is not DataRowView rowView)
        {
            throw new Exception(string.Format("MessageFieldDataGridView Row.DataBoundItem at index {0} is not a DataRowView ", row.Index));
        }

        if (rowView.Row is not FieldDataRow dataRow)
            return;

        var fieldTag = (int)dataRow[FieldDataTable.ColumnTag];
        var fieldName = (string)dataRow[FieldDataTable.ColumnName];

        using GoaEditor editor = new();

        editor.Text = string.Format("{0} - {1}", fieldTag, fieldName);

        if (row.Cells[FieldDataTable.ColumnValue].Value.ToString() is string goa)
        {
            editor.Goa = goa;
        }

        if (editor.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        row.Cells[FieldDataTable.ColumnValue].Value = editor.Goa;
    }

    void FieldGridCellContextMenuStripNeeded(object? sender, DataGridViewCellContextMenuStripNeededEventArgs e)
    {
        if (Session is null)
        {
            return;
        }

        if (e.RowIndex < 0)
        {
            return;
        }

        DataGridViewRow gridRow = _fieldGrid.Rows[e.RowIndex];

        if (gridRow.DataBoundItem is not DataRowView rowView)
        {
            return;
        }

        DataRow row = rowView.Row;

        if (_fieldTable is null)
        {
            return;
        }

        ContextMenuRowIndex = _fieldTable.Rows.IndexOf(row);

        if (Session.CustomFields.Any())
        {
            _insertContextMenuItem.DropDownItems.Clear();

            ToolStripMenuItem[] items = new ToolStripMenuItem[Session.CustomFields.Count];

            int index = 0;

            foreach (CustomField field in Session.CustomFields.Values)
            {
                items[index] = new ToolStripMenuItem(string.Format("{0} - {1}", field.Tag, field.Name));
                items[index].Click += InsertCustomFieldClick;
                items[index++].Tag = field;
            }

            _insertContextMenuItem.DropDownItems.AddRange(items);
        }

        _repeatContextMenuItem.Enabled = _fieldGrid.SelectedRows.Count > 0;
        _removeContextMenuItem.Enabled = _fieldGrid.SelectedRows.Count > 0;
        _insertContextMenuItem.Enabled = Session.CustomFields.Any();
        _resetContextMenuItem.Enabled = _messageGrid.SelectedRows.Count > 0;

        e.ContextMenuStrip = _contextMenu;
    }

    int ContextMenuRowIndex { get; set; }

    void InsertCustomFieldClick(object? sender, EventArgs e)
    {
        Fix.Message? message = SelectedMessage;

        if (message is null)
        {
            return;
        }

        if (sender is null)
        {
            return;
        }

        var item = (ToolStripMenuItem)sender;
        var field = (CustomField)item.Tag;

        message.Fields.Insert(ContextMenuRowIndex, new Fix.Field(field.Tag.ToString(), string.Empty));

        MessageGridSelectionChanged(this, EventArgs.Empty);
    }

    void ResetButtonClick(object? sender, EventArgs e)
    {
        try
        {
            Fix.Message? message = SelectedMessage;

            if (message is null)
            {
                return;
            }

            if (_session is null)
            {
                return;
            }

            _session.ResetTemplateMessage(message.MsgType);

            RemoveFilterButtonClick(this, EventArgs.Empty);
            MessageGridSelectionChanged(this, EventArgs.Empty);
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

    void RemoveButtonClick(object? sender, EventArgs e)
    {
        if (Session is null)
        {
            return;
        }

        if (_fieldGrid.SelectedRows.Count == 0)
        {
            return;
        }

        if (_fieldTable is null)
        {
            return;
        }

        Fix.Message? message = SelectedMessage;

        if (message == null)
        {
            return;
        }
        //
        // We need to get the indexes of the rows in the underlying table not the view as the view
        // may have been filtered.
        //
        if (_fieldGrid.SelectedRows[0].DataBoundItem is not DataRowView endDataRowView)
        {
            return;
        }

        int end = _fieldTable.Rows.IndexOf(endDataRowView.Row);

        if (_fieldGrid.SelectedRows[_fieldGrid.SelectedRows.Count - 1].DataBoundItem is not DataRowView beginDataRowView)
        {
            return;
        }

        int begin = _fieldTable.Rows.IndexOf(beginDataRowView.Row);

        if (end < begin)
        {
            //
            // If the user selects the rows bottom up thats the order the rows will be in SelectedRows so we need to 
            // reverse the order here.
            //
            int tmp = end;
            end = begin;
            begin = tmp;
        }
        //
        // Update some fields automatically to make life easier for the user.
        //
        if (message.MsgType == FIX_5_0SP2.Messages.NewOrderList.MsgType || message.MsgType == FIX_4_0.Messages.KodiakWaveOrder.MsgType)
        {
            //
            // In the OrderList message the ClOrdID is the delimiter for a repeating group
            // so decrement the ListNoOrders and NoOrders fields by this count.
            //
            int clOrdIdCount = (from FieldDataRow row
                                in _fieldTable.Rows
                                where row.Field?.Tag == FIX_5_0SP2.Fields.ClOrdID.Tag
                                select row).Count();

            Fix.Field? totNoOrders = message.Fields.Find(FIX_5_0SP2.Fields.TotNoOrders);

            if (totNoOrders is not null)
            {
                if (Session.AutoTotNoOrders)
                {
                    message.Fields.Set(FIX_5_0SP2.Fields.TotNoOrders, clOrdIdCount);
                }

                if (Session.AutoNoOrders)
                {
                    message.Fields.Set(FIX_5_0SP2.Fields.NoOrders, clOrdIdCount);
                }
            }
        }

        message.Fields.Remove(begin, end - begin + 1);
        //
        // Force the field grid to be updated so the fields disappear.
        //
        MessageGridSelectionChanged(this, EventArgs.Empty);
    }

    void FieldGridSelectionChanged(object? sender, EventArgs e)
    {
        Fix.Field? field = SelectedField;

        _inspectorPanel.Field = field;

        _repeatButton.Enabled = field is not null;
        _removeButton.Enabled = field is not null;
        _repeatMenuItem.Enabled = field is not null;
        _removeMenuItem.Enabled = field is not null;
        _repeatContextMenuItem.Enabled = field is not null;
        _removeContextMenuItem.Enabled = field is not null;
        _insertContextMenuItem.Enabled = field is not null;
    }

    void RepeatButtonClick(object? sender, EventArgs e)
    {
        if (Session is null)
        {
            return;
        }

        if (_fieldTable is null)
        {
            return;
        }

        if (_fieldGrid.SelectedRows.Count == 0)
        {
            return;
        }
        //
        // We need to get the indexes of the rows in the underlying table not the view as the view
        // may have been filtered.
        //
        if (_fieldGrid.SelectedRows[0].DataBoundItem is not DataRowView endDataRowView)
        {
            return;
        }

        if (_fieldGrid.SelectedRows[_fieldGrid.SelectedRows.Count - 1].DataBoundItem is not DataRowView beginDataRowView)
        {
            return;
        }

        int end = _fieldTable.Rows.IndexOf(endDataRowView.Row);
        int begin = _fieldTable.Rows.IndexOf(beginDataRowView.Row);

        if (end < begin)
        {
            //
            // If the user selects the rows bottom up thats the order the rows will be in SelectedRows so we need to 
            // reverse the order here.
            //
            int tmp = end;
            end = begin;
            begin = tmp;
        }

        Fix.Message? message = SelectedMessage;

        if (message == null)
            return;

        message.Fields.Repeat(begin, end - begin + 1);
        //
        // Update some fields automatically to make life easier for the user.
        //
        if (message.MsgType == FIX_5_0SP2.Messages.NewOrderList.MsgType || message.MsgType == FIX_4_0.Messages.KodiakWaveOrder.MsgType)
        {
            //
            // In the OrderList message the ClOrdID is the delimiter for a repeating group
            // so increment the ListNoOrders and NoOrders fields by this count.
            //
            int clOrdIdCount = (from DataGridViewRow row in _fieldGrid.SelectedRows
                                select row.DataBoundItem as DataRowView
                                into rowView
                                select rowView.Row
                                into dataRow
                                select (int)dataRow[FieldDataTable.ColumnTag]).Count(tag => tag == FIX_5_0SP2.Fields.ClOrdID.Tag);

            Fix.Field? totNoOrders = message.Fields.Find(FIX_5_0SP2.Fields.TotNoOrders);

            if (totNoOrders is not null && totNoOrders.Value != null && totNoOrders.Value.Trim() != string.Empty)
            {
                clOrdIdCount += Convert.ToInt32(totNoOrders.Value);
            }
            else
            {
                //
                // If the TotNoOrders was not set at all then we have to scan the entire message
                // to get it right.
                //
                clOrdIdCount = message.Fields.Count(field => field.Tag == FIX_5_0SP2.Fields.ClOrdID.Tag);
            }

            if (Session.AutoTotNoOrders)
            {
                message.Fields.Set(FIX_5_0SP2.Fields.TotNoOrders, clOrdIdCount);
            }

            if (Session.AutoNoOrders)
            {
                message.Fields.Set(FIX_5_0SP2.Fields.NoOrders, clOrdIdCount);
            }
        }
        //
        // Force the field grid to be updated so we get our new fields.
        //
        MessageGridSelectionChanged(this, EventArgs.Empty);
    }

    async Task PasteButtonClick(object? sender, EventArgs e)
    {
        if (Session is null)
        {
            return;
        }

        if (!Clipboard.ContainsText(TextDataFormat.Text))
        {
            return;
        }

        Fix.Message? parsedMessage = await Fix.Message.Parse(Clipboard.GetText(TextDataFormat.Text));

        if (parsedMessage is null || parsedMessage.Fields.Count == 0)
        {
            MessageBox.Show(this,
                "Unable to parse the contents of the clipboard",
                Application.ProductName,
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        if (FindMessage(parsedMessage.MsgType) is not Fix.Message message)
        {
            // TODO
            return;
        }

        using PasteMessageForm form = new();
        form.DefineUnknownAsCustom = Session.PasteDefineCustomFields;
        form.FilterEmptyFields = Session.PasteFilterEmptyFields;
        form.ResetExistingMessage = Session.PasteResetExisting;

        if (form.ShowDialog() != DialogResult.OK)
            return;

        Session.PasteDefineCustomFields = form.DefineUnknownAsCustom;
        Session.PasteFilterEmptyFields = form.FilterEmptyFields;
        Session.PasteResetExisting = form.ResetExistingMessage;

        Session.Write();

        if (form.ResetExistingMessage)
        {
            Session.ResetTemplateMessage(message.MsgType);
        }

        SimplePaste(parsedMessage, message, form.DefineUnknownAsCustom);

        SelectMessage(message.MsgType);
        MessageGridSelectionChanged(null, EventArgs.Empty);
        RemoveFilterButtonClick(this, EventArgs.Empty);

        if (form.FilterEmptyFields)
        {
            FilterButtonClick(this, EventArgs.Empty);
        }

        if (form.DefineUnknownAsCustom)
        {
            Session.WriteCustomFields();
        }
    }

    void SimplePaste(Fix.Message parsedMessage, Fix.Message message, bool defineUnknownAsCustom)
    {
        if (Session is null)
        {
            return;
        }

        foreach (Fix.Field field in parsedMessage.Fields)
        {
            if (!Session.Version.Fields.TryGetValue(field.Tag, out _))
            {
                CustomField custom;
                if (!Session.CustomFields.TryGetValue(field.Tag, out _) && defineUnknownAsCustom)
                {
                    custom = new CustomField { Tag = field.Tag, Name = field.Tag.ToString() };
                    Session.AddCustomField(custom);
                }
            }

            if (field.Tag == FIX_5_0SP2.Fields.ClOrdID.Tag)
            {
                message.Fields.Add(field.Tag, Session.AutoClOrdId ? Session.FormatClOrdId(Session.NextClOrdId) : field.Value);
            }
            else if (field.Tag == FIX_5_0SP2.Fields.OrderID.Tag)
            {
                message.Fields.Add(field.Tag, Session.NextOrderId);
            }
            else if (field.Tag == FIX_5_0SP2.Fields.ExecID.Tag)
            {
                message.Fields.Add(field.Tag, Session.NextExecId);
            }
            else if (field.Tag == FIX_5_0SP2.Fields.ListID.Tag)
            {
                message.Fields.Add(field.Tag, Session.NextListId);
            }
            else if (field.Tag == FIX_5_0SP2.Fields.AllocID.Tag)
            {
                message.Fields.Add(field.Tag, Session.NextAllocId);
            }
            else if (field.Tag == FIX_5_0SP2.Fields.NextExpectedMsgSeqNum.Tag)
            {
                message.Fields.Add(field.Tag, Session.IncomingSeqNum);
            }
            else
            {
                message.Fields.Add(field.Tag, field.Value);
            }
        }

        Session.WriteCustomFields();
    }

    void RemoveFilterButtonClick(object? sender, EventArgs e)
    {
        if (Session is null)
        {
            return;
        }

        if (SelectedMessage is not Fix.Message message)
        {
            return;
        }

        try
        {
            Session.SetAllMessageFieldsVisible(message.MsgType);
        }
        finally
        {
            Session.WriteFilters();
        }

        ApplyFieldSearch();
    }

    void FilterButtonClick(object? sender, EventArgs e)
    {
        if (Session is null)
        {
            return;
        }

        if (SelectedMessage is not Fix.Message message)
        {
            return;
        }

        var messageDefinition = FIX_5_0SP2.Messages[message.MsgType];

        try
        {
            foreach (Fix.Field field in message.Fields)
            {
                bool visible = false;

                if (field.Describe(messageDefinition) is Fix.FieldDescription description)
                {
                    if (description.Required)
                    {
                        visible = true;
                    }
                    else if (!string.IsNullOrEmpty(field.Value.Trim()))
                    {
                        visible = true;
                    }
                }

                Session.SetFieldVisible(message.MsgType, field.Tag, visible);
            }
        }
        finally
        {
            Session.WriteFilters();
        }
        //
        // Construct the basic filter that hides deselected fields from the Filters view.
        //
        ApplyFieldSearch();
    }

    void SendButtonClick(object? sender, EventArgs e)
    {
        if (_fieldTable is null)
        {
            return;
        }

        if (SelectedMessage is not Fix.Message defaults)
        {
            return;
        }

        if (Session == null || !Session.Connected)
        {
            return;
        }

        var message = new Fix.Message();

        message.Fields.Clear();
        //
        // Count the number of ClOrdId's in the message so we can allocate that many in the session.
        //
        int clOrdIdCount = 0;

        foreach (Fix.Field field in defaults.Fields)
        {
            if (string.IsNullOrEmpty(field.Value))
                continue;

            if (!Session.IsFieldVisible(defaults.MsgType, field.Tag))
                continue;
            //
            // Only allocate a ClOrdId if we are actually sending that field.
            //
            if (field.Tag == FIX_5_0SP2.Fields.ClOrdID.Tag)
                ++clOrdIdCount;

            message.Fields.Add(new Fix.Field(field.Tag, field.Value));
        }

        /*
        if (message.MsgType == Fix.Message.Logout)
        {
            //
            // Set this so we don't bounce back another logout when we receive the reply logout.
            //
            Session.ExpectingLogout = true;
        }
        */

        Session.Send(message);
        //
        // This message requires us to change our next outgoing MsgSeqNo
        //
        // When the remote end receives a SequenceReset it is required to ignore the
        // MsgSeqNum and then expect the NewSeqNo as the next incoming MsgSeqNum. We
        // therefore set our next outgoing MsgSeqNo AFTER sending the SequenceReset
        // because the engine sets the MsgSeqNum for us.
        //
        /*
        if (message.MsgType.CompareTo(Fix.Message.SequenceReset) == 0)
        {
            foreach (DataGridViewRow row in _fieldGrid.Rows)
            {
                DataRowView rowView = row.DataBoundItem as DataRowView;
                DataRow dataRow = rowView.Row;
                Fix.Field.FieldId fieldId = (Fix.Field.FieldId) dataRow[FieldDataTable.ColumnId];

                if (fieldId == Fix.Field.FieldId.NewSeqNo)
                {
                    Session.NextSeqNoOut = Convert.ToInt32(row.Cells[FieldDataTable.ColumnValue].Value);
                    break;
                }
            }
        }
        */

        var updatedFields = new List<KeyValuePair<int, string>>();

        if (message.MsgType == FIX_5_0SP2.Messages.NewOrderSingle.MsgType ||
            message.MsgType == FIX_5_0SP2.Messages.NewOrderList.MsgType ||
            message.MsgType == FIX_5_0SP2.Messages.OrderCancelRequest.MsgType ||
            message.MsgType == FIX_5_0SP2.Messages.OrderCancelReplaceRequest.MsgType ||
            message.MsgType == FIX_5_0SP2.Messages.AllocationInstruction.MsgType ||
            message.MsgType == FIX_4_0.Messages.KodiakWaveOrder.MsgType ||
            message.MsgType == FIX_4_0.Messages.KodiakWaveOrderCancelRequest.MsgType ||
            message.MsgType == FIX_4_0.Messages.KodiakWaveOrderCorrectionRequest.MsgType)
        {
            //
            // Allocate any ClOrdIds that were used.
            //
            if (Session.AutoClOrdId)
            {
                Session.NextClOrdId += clOrdIdCount;
            }
            //
            // We may have multiple ClOrdIds in one message such as order list but we don't want to
            // allocate them until the message is actually sent so we just increment the counter
            // locally.
            //
            int nextClOrdId = Session.NextClOrdId;

            for (int index = 0; index < defaults.Fields.Count; ++index)
            {
                Fix.Field field = defaults.Fields[index];

                if (field.Tag == FIX_5_0SP2.Fields.MsgSeqNum.Tag)
                {
                    defaults.Fields[index].Value = Session.OutgoingSeqNum.ToString();
                    updatedFields.Add(new KeyValuePair<int, string>(index, Session.OutgoingSeqNum.ToString()));
                }
                else if (field.Tag == FIX_5_0SP2.Fields.ClOrdID.Tag && Session.AutoClOrdId)
                {
                    string value = Session.FormatClOrdId(nextClOrdId);
                    defaults.Fields[index].Value = value;
                    updatedFields.Add(new KeyValuePair<int, string>(index, value));
                    ++nextClOrdId;
                }
                else if (field.Tag == FIX_5_0SP2.Fields.ListID.Tag && Session.AutoListId)
                {
                    if (message.MsgType != FIX_5_0SP2.Messages.OrderCancelRequest.MsgType &&
                        message.MsgType != FIX_5_0SP2.Messages.OrderCancelReplaceRequest.MsgType)
                    {
                        int listId = Session.NextListId++;
                        defaults.Fields[index].Value = listId.ToString();
                        updatedFields.Add(new KeyValuePair<int, string>(index, listId.ToString()));
                    }
                }
                else if (field.Tag == FIX_5_0SP2.Fields.AllocID.Tag && Session.AutoAllocId)
                {
                    int allocId = Session.NextAllocId++;
                    defaults.Fields[index].Value = allocId.ToString();
                    updatedFields.Add(new KeyValuePair<int, string>(index, allocId.ToString()));
                }
            }
        }
        else if (message.MsgType == FIX_5_0SP2.Messages.ExecutionReport.MsgType)
        {
            for (int index = 0; index < defaults.Fields.Count; ++index)
            {
                Fix.Field field = defaults.Fields[index];

                if (field.Tag == FIX_5_0SP2.Fields.MsgSeqNum.Tag)
                {
                    defaults.Fields[index].Value = Session.OutgoingSeqNum.ToString();
                    updatedFields.Add(new KeyValuePair<int, string>(index, Session.OutgoingSeqNum.ToString()));
                }
                else if (field.Tag == FIX_5_0SP2.Fields.OrderID.Tag)
                {
                    int orderId = Session.NextOrderId++;
                    defaults.Fields[index].Value = orderId.ToString();
                    updatedFields.Add(new KeyValuePair<int, string>(index, orderId.ToString()));
                }
                else if (field.Tag == FIX_5_0SP2.Fields.ExecID.Tag)
                {
                    int execId = Session.NextExecId++;
                    defaults.Fields[index].Value = execId.ToString();
                    updatedFields.Add(new KeyValuePair<int, string>(index, execId.ToString()));
                }
            }
        }

        foreach (KeyValuePair<int, string> field in updatedFields)
        {
            _fieldTable.Rows[field.Key][FieldDataTable.ColumnValue] = field.Value;
        }

        _fieldGrid.RefreshEdit();
    }

    public void UpdateUiState()
    {
        if (Session == null || !Session.Connected)
        {
            _sendButton.Enabled = false;
            _sendMenuItem.Enabled = false;
        }
        else
        {
            _sendButton.Enabled = true;
            _sendMenuItem.Enabled = true;
        }

        _filterButton.Enabled = Session != null;
        _removeFilterButton.Enabled = Session != null;
        _pasteButton.Enabled = Session != null;
        _editGoaButton.Enabled = Session != null;

        _filterMenuItem.Enabled = Session != null;
        _removeFilterMenuItem.Enabled = Session != null;
        _pasteMenuItem.Enabled = Session != null;
        _editGoaMenuItem.Enabled = Session != null;

        _repeatButton.Enabled = _fieldGrid.SelectedRows.Count > 0;
        _removeButton.Enabled = _fieldGrid.SelectedRows.Count > 0;
        _resetButton.Enabled = _messageGrid.SelectedRows.Count > 0;

        _repeatMenuItem.Enabled = _fieldGrid.SelectedRows.Count > 0;
        _removeMenuItem.Enabled = _fieldGrid.SelectedRows.Count > 0;
        _resetMenuItem.Enabled = _messageGrid.SelectedRows.Count > 0;

        _messageSearchTextBox.Enabled = Session != null;
        _fieldSearchTextBox.Enabled = Session != null;
    }

    public Session? Session
    {
        get
        {
            return _session;
        }
        set
        {
            if (_session != null)
            {
                _session.MessageFilterChanged -= SessionMessageFilterChanged;
                _session.FieldFilterChanged -= SessionFieldFilterChanged;
                _session.SessionReset -= SessionSessionReset;
            }

            _session = value;
            Reload();

            if (_session != null)
            {
                _session.MessageFilterChanged += SessionMessageFilterChanged;
                _session.FieldFilterChanged += SessionFieldFilterChanged;
                _session.SessionReset += SessionSessionReset;
            }
        }
    }

    void SessionSessionReset(object? sender, EventArgs e)
    {
        if (InvokeRequired)
        {
            BeginInvoke(new MethodInvoker(() => SessionSessionReset(sender, e)));
            return;
        }

        MessageGridSelectionChanged(this, EventArgs.Empty);
    }

    void SessionMessageFilterChanged(object? sender, EventArgs e)
    {
        _messageView.RowFilter = Session?.MessageRowFilter();
    }

    void SessionFieldFilterChanged(object? sender, EventArgs e)
    {
        if (Session == null)
        {
            return;
        }

        if (SelectedMessage is null)
        {
            return;
        }

        ApplyFieldSearch();
    }

    public void Reload()
    {
        if (_session is null)
        {
            return;
        }

        try
        {
            _suppressMsgTypeUpdate = true;
            _messageTable.BeginLoadData();
            _messageTable.Clear();

            foreach (Fix.Dictionary.Message message in _session.Version.Messages)
            {
                if (_messageTable.NewRow() is MessageTypeDataRow row)
                {
                    row.Message = message;
                    row[MessageTypeDataTable.ColumnMsgType] = message.MsgType;
                    row[MessageTypeDataTable.ColumnMsgTypeDescription] = message.Name;
                    // The row filter does not give us a means to do case insensitive search or ToUpper() etc so
                    // stuff an uppercase copy of the MsgType in here just for searching.
                    row[MessageTypeDataTable.ColumnSearchMsgType] = message.MsgType.ToUpper();
                    row[MessageTypeDataTable.ColumnSearchMsgTypeDescription] = message.Name.ToUpper();
                    _messageTable.Rows.Add(row);
                }
            }

            _messageView.RowFilter = _session.MessageRowFilter();
        }
        finally
        {
            _messageTable.EndLoadData();
            _suppressMsgTypeUpdate = false;
        }
        //
        // Preserve the user's selection across disconnects.
        //
        if (!string.IsNullOrEmpty(_selectedMsgType))
        {
            SelectMessage(_selectedMsgType);
        }

        _messageOptionsPanel.Session = Session;
    }

    void MessageGridSelectionChanged(object? sender, EventArgs e)
    {
        if (Session is null)
        {
            return;
        }

        Fix.Message? message = SelectedMessage;

        _inspectorPanel.Message = message is null ? null : Session.Version.Messages[message.MsgType];

        try
        {
            _fieldGrid.DataSource = null;
            _fieldTable = new FieldDataTable("Fields");
            _fieldGrid.Message = message;

            if (message == null)
            {
                return;
            }

            message.Fields.Set(FIX_5_0SP2.Fields.BeginString, Session.BeginString.BeginString);

            if (Session.BeginString.BeginString == Versions.FIXT_1_1.BeginString &&
                message.MsgType == FIX_5_0SP2.Messages.Logon.MsgType)
            {
                message.Fields.Set(FIX_5_0SP2.Fields.DefaultApplVerID, Session.DefaultApplVerId.ApplVerID);
                message.Fields.Set(FIX_5_0SP2.Fields.EncryptMethod, FIX_5_0SP2.EncryptMethod.None.Value);
                message.Fields.Set(FIX_5_0SP2.Fields.HeartBtInt, Session.HeartBtInt);
            }

            message.Fields.Set(FIX_5_0SP2.Fields.SenderCompID, Session.SenderCompId);
            message.Fields.Set(FIX_5_0SP2.Fields.TargetCompID, Session.TargetCompId);

            if (Session.AutoSetMsgSeqNum)
            {
                message.Fields.Set(FIX_5_0SP2.Fields.MsgSeqNum, Session.OutgoingSeqNum);
            }
            //
            // We may have multiple ClOrdIds in one message such as order list but we don't want to
            // allocate them until the message is actually sent so we just increment the counter
            // locally.
            //
            int nextClOrdId = Session.NextClOrdId;
            int nextAllocId = Session.NextAllocId;
            int nextListSeqNo = 1;
            int index = 0;
            string time = Fix.Field.TimestampString(Session.OutgoingTimestampFormat);

            foreach (Fix.Field field in message.Fields)
            {
                //
                // Update any time fields as required.
                //
                if ((field.Tag == FIX_5_0SP2.Fields.SendingTime.Tag && Session.AutoSendingTime) ||
                    (field.Tag == FIX_5_0SP2.Fields.TransactTime.Tag && Session.AutoTransactTime))
                {
                    message.Fields[index].Value = time;
                }

                if (field.Tag == FIX_5_0SP2.Fields.NextExpectedMsgSeqNum.Tag &&
                    Session.NextExpectedMsgSeqNum)
                {
                    message.Fields[index].Value = Session.IncomingSeqNum.ToString();
                }

                //
                // Update any of the various ID's as required
                //
                if (message.MsgType == FIX_5_0SP2.Messages.NewOrderSingle.MsgType ||
                    message.MsgType == FIX_5_0SP2.Messages.NewOrderList.MsgType ||
                    message.MsgType == FIX_4_0.Messages.KodiakWaveOrder.MsgType)
                {

                    if (field.Tag == FIX_5_0SP2.Fields.ClOrdID.Tag)
                    {
                        message.Fields[index].Value = Session.FormatClOrdId(nextClOrdId);
                        ++nextClOrdId;
                    }
                    else if (field.Tag == FIX_5_0SP2.Fields.OrderID.Tag)
                    {
                        message.Fields[index].Value = Session.NextOrderId.ToString();
                    }
                    else if (field.Tag == FIX_5_0SP2.Fields.ExecID.Tag)
                    {
                        message.Fields[index].Value = Session.NextExecId.ToString();
                    }
                    else if (field.Tag == FIX_5_0SP2.Fields.ListID.Tag)
                    {
                        message.Fields[index].Value = Session.NextListId.ToString();
                    }
                    else if (field.Tag == FIX_5_0SP2.Fields.ListSeqNo.Tag && message.MsgType != FIX_4_0.Messages.KodiakWaveOrder.MsgType)
                    {
                        if (Session.AutoListSeqNo)
                        {
                            message.Fields[index].Value = nextListSeqNo.ToString();
                            ++nextListSeqNo;
                        }
                    }
                    else if (field.Tag == FIX_5_0SP2.Fields.AllocID.Tag)
                    {
                        message.Fields[index].Value = nextAllocId.ToString();
                    }
                }
                else if (message.MsgType == FIX_5_0SP2.Messages.AllocationInstruction.MsgType)
                {
                    if (field.Tag == FIX_5_0SP2.Fields.AllocID.Tag)
                    {
                        message.Fields[index].Value = nextAllocId.ToString();
                    }
                }

                ++index;
            }

            message.Fields.Set(FIX_5_0SP2.Fields.BodyLength, message.ComputeBodyLength());

            //
            // Now the message is ready populate the field view
            //
            int previousIndent = 0;

            var messageDefinition = FIX_5_0SP2.Messages[message.MsgType];

            foreach (Fix.Field field in message.Fields)
            {
                if (field.Tag == FIX_5_0SP2.Fields.CheckSum.Tag)
                {
                    //
                    // This gets set automatically by the library when sending a message across
                    // the transport so there is no need to make it editable.
                    //
                    continue;
                }

                if (_fieldTable.NewRow() is not FieldDataRow dataRow)
                {
                    continue;
                }

                dataRow.Field = field;

                var description = field.Describe(messageDefinition);

                if (description != null)
                {
                    dataRow[FieldDataTable.ColumnIndent] = description.Depth;
                    dataRow[FieldDataTable.ColumnName] = description.Name;
                    dataRow[FieldDataTable.ColumnRequired] = description.Required;
                    dataRow[FieldDataTable.ColumnCustom] = false;
                    previousIndent = description.Depth;
                }
                else
                {
                    if (Session.CustomFields.TryGetValue(field.Tag, out var custom))
                    {
                        dataRow[FieldDataTable.ColumnName] = custom.Name;
                        dataRow[FieldDataTable.ColumnCustom] = true;
                    }
                    else
                    {
                        dataRow[FieldDataTable.ColumnName] = string.Empty;
                        dataRow[FieldDataTable.ColumnCustom] = false;
                    }

                    dataRow[FieldDataTable.ColumnIndent] = previousIndent;
                    dataRow[FieldDataTable.ColumnRequired] = false;
                }

                dataRow[FieldDataTable.ColumnTag] = field.Tag;
                dataRow[FieldDataTable.ColumnValue] = field.Value;

                _fieldTable.Rows.Add(dataRow);
            }
        }
        finally
        {
            _fieldView = new DataView(_fieldTable);

            FieldSearchTextBoxTextChanged(_fieldSearchTextBox, EventArgs.Empty);
            _fieldGrid.DataSource = _fieldView;
            if (message != null)
            {
                ApplyFieldSearch();
            }
            _fieldGrid.Focus();
        }

        OnMessageSelected(message);
    }

    void TimerTick(object? sender, EventArgs e)
    {
        if (_session == null || string.IsNullOrEmpty(_session.FileName))
            return;

        if (_fieldTable == null)
            return;

        string time = Fix.Field.TimestampString(_session.OutgoingTimestampFormat);

        foreach (FieldDataRow row in _fieldTable.Rows)
        {
            if (row.Field is not Fix.Field field)
            {
                continue;
            }

            if ((field.Tag == FIX_5_0SP2.Fields.SendingTime.Tag && _session.AutoSendingTime) ||
                (field.Tag == FIX_5_0SP2.Fields.TransactTime.Tag && _session.AutoTransactTime))
            {
                row[FieldDataTable.ColumnValue] = time;
            }
        }
    }

    public static void UpdateMessage(Fix.Message message, Fix.Order order)
    {
        //
        // Order.Messages are stored in arrival order so we just iterate through and get
        // the most recent version of each field.
        //
        foreach (Fix.Field field in message.Fields)
        {
            if (field.Tag == FIX_5_0SP2.Fields.MsgType.Tag)
                continue;

            foreach (Fix.Message source in order.Messages)
            {
                if (source.Fields.Find(field.Tag) is not Fix.Field sourceField)
                {
                    continue;
                }

                message.Fields.Set(field.Tag, sourceField.Value);
            }
        }
    }

    public void AcknowledgeAmend(Fix.Order order)
    {
        if (Session is null)
        {
            return;
        }

        if (FindMessage(FIX_5_0SP2.Messages.ExecutionReport.MsgType) is not Fix.Message message)
        {
            // TODO - message
            return;
        }

        if (order.PendingMessage is not Fix.Message pending)
        {
            // TODO - message
            return;
        }

        if (pending.ClOrdID is not string ClOrdID)
        {
            // TODO - message
            return;
        }

        if (pending.OrigClOrdID is not string OrigClOrdID)
        {
            // TODO - message
            return;
        }

        Fix.Message? lastMessage = order.Messages.LastOrDefault();

        var ordStatus = FIX_5_0SP2.OrdStatus.Replaced;
        var execType = FIX_5_0SP2.ExecType.Replaced;

        if (lastMessage != null && lastMessage.MsgType == FIX_5_0SP2.Messages.OrderCancelReplaceRequest.MsgType)
        {
            ordStatus = FIX_5_0SP2.OrdStatus.PendingReplace;
            execType = FIX_5_0SP2.ExecType.PendingReplace;
            message.Fields.Set(FIX_5_0SP2.Fields.ClOrdID, ClOrdID);
            message.Fields.Set(FIX_5_0SP2.Fields.OrigClOrdID, OrigClOrdID);
        }
        else
        {
            message.Fields.Set(FIX_5_0SP2.Fields.ClOrdID, ClOrdID);
            message.Fields.Set(FIX_5_0SP2.Fields.OrigClOrdID, OrigClOrdID);
        }

        if (order.Side is FieldValue sideValue)
        {
            message.Fields.Set(sideValue);
        }

        message.Fields.Set(FIX_5_0SP2.Fields.Symbol, order.Symbol);
        message.Fields.Set(FIX_5_0SP2.Fields.OrderQty, order.OrderQty);
        message.Fields.Set(ordStatus);
        if (order.OrderID != null)
        {
            message.Fields.Set(FIX_5_0SP2.Fields.OrderID, order.OrderID);
        }
        message.Fields.Set(FIX_5_0SP2.Fields.ExecID, Session.NextExecId.ToString());
        message.Fields.Set(FIX_5_0SP2.Fields.LastQty, 0);
        message.Fields.Set(FIX_5_0SP2.Fields.LastPx, 0);
        message.Fields.Set(FIX_5_0SP2.Fields.CumQty, order.CumQty ?? 0);
        message.Fields.Set(FIX_5_0SP2.Fields.AvgPx, order.AvgPx ?? 0);

        if (Session.Version.BeginString != "FIX.4.0")
        {
            message.Fields.Set(execType);
            message.Fields.Set(FIX_5_0SP2.Fields.LeavesQty, 0);
        }

        if (Session.Version.BeginString.StartsWith("FIX.4."))
        {
            message.Fields.Set(FIX_4_2.ExecTransType.New);
        }

        SelectMessage(message.MsgType);
    }

    public void AcknowledgeCancel(Fix.Order order)
    {
        if (Session is null)
        {
            return;
        }

        if (FindMessage(FIX_5_0SP2.Messages.ExecutionReport.MsgType) is not Fix.Message message)
        {
            return;
        }

        message.Fields.Set(FIX_5_0SP2.Fields.ClOrdID, order.ClOrdID);

        if (order.Side is FieldValue sideValue)
        {
            message.Fields.Set(sideValue);
        }

        message.Fields.Set(FIX_5_0SP2.Fields.Symbol, order.Symbol);
        message.Fields.Set(FIX_5_0SP2.Fields.OrderQty, order.OrderQty);
        message.Fields.Set(FIX_5_0SP2.OrdStatus.Canceled);
        if (order.OrderID != null)
        {
            message.Fields.Set(FIX_5_0SP2.Fields.OrderID, order.OrderID);
        }
        message.Fields.Set(FIX_5_0SP2.Fields.ExecID, Session.NextExecId.ToString());
        message.Fields.Set(FIX_5_0SP2.Fields.LastQty, 0);
        message.Fields.Set(FIX_5_0SP2.Fields.LastPx, 0);
        message.Fields.Set(FIX_5_0SP2.Fields.CumQty, order.CumQty ?? 0);
        message.Fields.Set(FIX_5_0SP2.Fields.AvgPx, order.AvgPx ?? 0);

        if (Session.Version.BeginString != "FIX.4.0")
        {
            message.Fields.Set(FIX_5_0SP2.ExecType.Canceled);
            message.Fields.Set(FIX_5_0SP2.Fields.LeavesQty, 0);
        }

        if (Session.Version.BeginString.StartsWith("FIX.4."))
        {
            message.Fields.Set(FIX_4_2.ExecTransType.New);
        }

        SelectMessage(message.MsgType);
    }

    public void AcknowledgeOrder(Fix.Order order)
    {
        if (Session is null)
        {
            return;
        }

        if (FindMessage(FIX_5_0SP2.Messages.ExecutionReport.MsgType) is not Fix.Message message)
        {
            return;
        }

        message.Fields.Set(FIX_5_0SP2.Fields.ClOrdID, order.ClOrdID);

        if (order.Side is FieldValue sideValue)
        {
            message.Fields.Set(sideValue);
        }

        message.Fields.Set(FIX_5_0SP2.Fields.Symbol, order.Symbol);
        message.Fields.Set(FIX_5_0SP2.Fields.OrderQty, order.OrderQty);
        message.Fields.Set(FIX_5_0SP2.OrdStatus.New);

        if (order.OrderID == null)
        {
            order.OrderID = Session.NextOrderId.ToString();
        }

        message.Fields.Set(FIX_5_0SP2.Fields.OrderID, order.OrderID);
        message.Fields.Set(FIX_5_0SP2.Fields.ExecID, Session.NextExecId.ToString());
        message.Fields.Set(FIX_5_0SP2.Fields.LastQty, 0);
        message.Fields.Set(FIX_5_0SP2.Fields.LastPx, 0);
        message.Fields.Set(FIX_5_0SP2.Fields.CumQty, 0);
        message.Fields.Set(FIX_5_0SP2.Fields.AvgPx, 0);

        if (Session.Version.BeginString != "FIX.4.0")
        {
            message.Fields.Set(FIX_5_0SP2.ExecType.New);
            message.Fields.Set(FIX_5_0SP2.Fields.LeavesQty, order.OrderQty);
        }

        if (Session.Version.BeginString.StartsWith("FIX.4."))
        {
            message.Fields.Set(FIX_4_2.ExecTransType.New);
        }

        SelectMessage(message.MsgType);
    }

    public void RejectOrder(Fix.Order order)
    {
        if (Session is null)
        {
            return;
        }

        if (FindMessage(FIX_5_0SP2.Messages.ExecutionReport.MsgType) is not Fix.Message message)
        {
            return;
        }

        message.Fields.Set(FIX_5_0SP2.Fields.ClOrdID, order.ClOrdID);

        if (order.Side is FieldValue sideValue)
        {
            message.Fields.Set(sideValue);
        }

        message.Fields.Set(FIX_5_0SP2.Fields.Symbol, order.Symbol);
        message.Fields.Set(FIX_5_0SP2.Fields.OrderQty, order.OrderQty);
        message.Fields.Set(FIX_5_0SP2.OrdStatus.Rejected);

        if (order.OrderID == null)
        {
            order.OrderID = Session.NextOrderId.ToString();
        }

        message.Fields.Set(FIX_5_0SP2.Fields.OrderID, order.OrderID);
        message.Fields.Set(FIX_5_0SP2.Fields.ExecID, Session.NextExecId.ToString());
        message.Fields.Set(FIX_5_0SP2.Fields.LastQty, 0);
        message.Fields.Set(FIX_5_0SP2.Fields.LastPx, 0);
        message.Fields.Set(FIX_5_0SP2.Fields.CumQty, 0);
        message.Fields.Set(FIX_5_0SP2.Fields.AvgPx, 0);

        if (Session.Version.BeginString != "FIX.4.0")
        {
            message.Fields.Set(FIX_5_0SP2.ExecType.Rejected);
            message.Fields.Set(FIX_5_0SP2.Fields.LeavesQty, 0);
        }

        if (Session.Version.BeginString.StartsWith("FIX.4."))
        {
            message.Fields.Set(FIX_4_2.ExecTransType.New);
        }

        SelectMessage(message.MsgType);
    }

    public void ReportOrder(Fix.Order order)
    {
        if (Session is null)
        {
            return;
        }

        if (FindMessage(FIX_5_0SP2.Messages.ExecutionReport.MsgType) is not Fix.Message message)
        {
            return;
        }

        message.Fields.Set(FIX_5_0SP2.Fields.ClOrdID, order.ClOrdID);

        if (order.Side is FieldValue sideValue)
        {
            message.Fields.Set(sideValue);
        }

        message.Fields.Set(FIX_5_0SP2.Fields.Symbol, order.Symbol);
        message.Fields.Set(FIX_5_0SP2.Fields.OrderQty, order.OrderQty);
        message.Fields.Set(FIX_5_0SP2.OrdStatus.Filled);

        if (order.OrderID == null)
        {
            order.OrderID = Session.NextOrderId.ToString();
        }

        message.Fields.Set(FIX_5_0SP2.Fields.OrderID, order.OrderID);
        message.Fields.Set(FIX_5_0SP2.Fields.ExecID, Session.NextExecId.ToString());
        message.Fields.Set(FIX_5_0SP2.Fields.LastQty, order.LeavesQty ?? order.OrderQty);
        message.Fields.Set(FIX_5_0SP2.Fields.LastPx, order.Price ?? 0);
        message.Fields.Set(FIX_5_0SP2.Fields.CumQty, order.OrderQty);
        message.Fields.Set(FIX_5_0SP2.Fields.AvgPx, order.Price ?? 0);

        if (Session.Version.BeginString != "FIX.4.0")
        {
            if (Session.Version.BeginString == "FIX.4.1" ||
                Session.Version.BeginString == "FIX.4.2")
            {
                message.Fields.Set(FIX_4_2.ExecType.Fill);
            }
            else
            {
                message.Fields.Set(FIX_5_0SP2.ExecType.Trade);
            }
            message.Fields.Set(FIX_5_0SP2.Fields.LeavesQty, 0);
        }

        if (Session.Version.BeginString.StartsWith("FIX.4."))
        {
            message.Fields.Set(FIX_4_2.ExecTransType.New);
        }

        SelectMessage(message.MsgType);
    }

    Fix.Message? FindMessage(string msgType)
    {
        if (_messageTable.Rows.Find(msgType) is not MessageTypeDataRow row)
        {
            MessageBox.Show(this,
                            string.Format("Unable to find the {0} message", msgType),
                            Application.ProductName,
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
            return null;
        }

        if (_session is null)
        {
            return null;
        }

        if (row.Message is not Fix.Dictionary.Message message)
        {
            return null;
        }

        return _session.MessageForTemplate(message);
    }

    void SelectMessage(string msgType)
    {
        int previous = _messageBindingSource.Position;

        _messageBindingSource.Position = _messageBindingSource.Find(MessageTypeDataTable.ColumnMsgType, msgType);

        if (_messageBindingSource.Position == previous)
        {
            MessageGridSelectionChanged(this, EventArgs.Empty);
        }
    }

    public void UnsolicitedCancelOrder(Fix.Order order)
    {
        if (Session is null)
        {
            return;
        }

        if (FindMessage(FIX_5_0SP2.Messages.ExecutionReport.MsgType) is not Fix.Message message)
        {
            return;
        }

        message.Fields.Set(FIX_5_0SP2.Fields.ClOrdID, order.ClOrdID);

        if (order.Side is FieldValue sideValue)
        {
            message.Fields.Set(sideValue);
        }

        message.Fields.Set(FIX_5_0SP2.Fields.Symbol, order.Symbol);
        message.Fields.Set(FIX_5_0SP2.Fields.OrderQty, order.OrderQty);
        message.Fields.Set(FIX_5_0SP2.OrdStatus.Canceled);

        if (order.OrderID == null)
        {
            order.OrderID = Session.NextOrderId.ToString();
        }

        message.Fields.Set(FIX_5_0SP2.Fields.OrderID, order.OrderID);
        message.Fields.Set(FIX_5_0SP2.Fields.ExecID, Session.NextExecId.ToString());
        message.Fields.Set(FIX_5_0SP2.Fields.LastQty, 0);
        message.Fields.Set(FIX_5_0SP2.Fields.LastPx, 0);
        message.Fields.Set(FIX_5_0SP2.Fields.CumQty, order.OrderQty);
        message.Fields.Set(FIX_5_0SP2.Fields.AvgPx, order.Price ?? 0);

        if (Session.Version.BeginString != "FIX.4.0")
        {
            message.Fields.Set(FIX_4_2.ExecType.Canceled);
            message.Fields.Set(FIX_5_0SP2.Fields.LeavesQty, 0);
        }

        if (Session.Version.BeginString.StartsWith("FIX.4."))
        {
            message.Fields.Set(FIX_4_2.ExecTransType.New);
        }

        SelectMessage(message.MsgType);
    }

    public void CancelOrder(Fix.Order order)
    {
        if (Session is null)
        {
            return;
        }

        if (FindMessage(FIX_5_0SP2.Messages.OrderCancelRequest.MsgType) is not Fix.Message message)
        {
            return;
        }

        UpdateMessage(message, order);

        message.Fields.Set(FIX_5_0SP2.Fields.OrigClOrdID, order.ClOrdID);
        message.Fields.Set(FIX_5_0SP2.Fields.ClOrdID, Session.FormatClOrdId(Session.NextClOrdId));
        //
        // This field was removed from later versions.
        //
        Fix.Field? beginString = message.Fields.Find(FIX_5_0SP2.Fields.BeginString);

        if (beginString is not null && beginString.Value == "FIX.4.0")
        {
            message.Fields.Set(FIX_4_2.Fields.CxlType, "F");
        }
        //
        // If we've previously cancelled an order from an OrderList in this session the
        // CancelRequest will have ListID set, we need to unset this if we are cancelling
        // an OrderSingle.
        //
        if (order.ListID == null)
        {
            message.Fields.Set(FIX_5_0SP2.Fields.ListID, "");
        }

        SelectMessage(message.MsgType);
    }

    public void AmendOrder(Fix.Order order)
    {
        if (Session is null)
        {
            return;
        }

        if (FindMessage(FIX_5_0SP2.Messages.OrderCancelReplaceRequest.MsgType) is not Fix.Message message)
        {
            return;
        }

        UpdateMessage(message, order);

        message.Fields.Set(FIX_5_0SP2.Fields.OrigClOrdID, order.ClOrdID);
        message.Fields.Set(FIX_5_0SP2.Fields.ClOrdID, Session.FormatClOrdId(Session.NextClOrdId));
        //
        // This field is not required in OrderList but it is in OrderCancelReplaceRequest so set it
        // to a default if it was not set.
        //
        // TODO

        //if (orderSingle.FindField(Fix.Field.FieldId.HandlInst) == null)
        //{
        //    message.SetField(Fix.Field.FieldId.HandlInst, "1");
        //}

        //
        // If we've previously amended an order from an OrderList in this session the
        // CancelReplaceRequest will have ListID set, we need to unset this if we are 
        // amending an OrderSingle.
        //
        if (order.ListID == null)
        {
            message.Fields.Set(FIX_5_0SP2.Fields.ListID, "");
        }

        SelectMessage(message.MsgType);
    }

    public void OrderStatus(Fix.Order order)
    {
        if (FindMessage(FIX_5_0SP2.Messages.OrderStatusRequest.MsgType) is not Fix.Message message)
        {
            return;
        }

        UpdateMessage(message, order);

        SelectMessage(message.MsgType);
    }

    public void ListExecute(string listId)
    {
        if (FindMessage(FIX_5_0SP2.Messages.ListExecute.MsgType) is not Fix.Message message)
        {
            return;
        }

        message.Fields.Set(FIX_5_0SP2.Fields.ListID, listId);

        SelectMessage(message.MsgType);
    }

    public void ListStatus(string listId)
    {
        if (FindMessage(FIX_5_0SP2.Messages.ListStatusRequest.MsgType) is not Fix.Message message)
        {
            return;
        }

        message.Fields.Set(FIX_5_0SP2.Fields.ListID, listId);

        SelectMessage(message.MsgType);
    }

    public void ListCancel(string listId)
    {
        if (FindMessage(FIX_5_0SP2.Messages.ListCancelRequest.MsgType) is not Fix.Message message)
        {
            return;
        }

        message.Fields.Set(FIX_5_0SP2.Fields.ListID, listId);

        SelectMessage(message.MsgType);
    }
}
