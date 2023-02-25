/////////////////////////////////////////////////
//
// FIX Client
//
// Copyright @ 2021 VIRTU Financial Inc.
// All rights reserved.
//
// Filename: OrdersPanel.cs
// Author:   Gary Hughes
//
/////////////////////////////////////////////////
using System.Data;
using System.Text;
using static Fix.Dictionary;

namespace FixClient;

partial class OrdersPanel : FixClientPanel
{
    readonly OrderDataGridView _orderGrid;
    readonly OrderDataTable _orderTable;
    readonly DataView _orderView;

    readonly SearchTextBox _orderSearchTextBox;

    readonly MessagesPanel _messageDefaults;
    readonly ToolStripButton _defaultsButton;

    readonly ToolStrip _clientToolStrip;
    readonly ToolStrip _serverToolStrip;

    readonly ToolStripSplitButton _cancelButton;
    readonly ToolStripMenuItem _cancelAllButton;
    readonly ToolStripButton _amendButton;
    readonly ToolStripButton _statusButton;
    readonly ToolStripButton _listExecuteButton;
    readonly ToolStripButton _listStatusButton;
    readonly ToolStripButton _listCancelButton;

    readonly ToolStripSplitButton _acceptorCancelButton;
    readonly ToolStripMenuItem _acceptorCancelAllButton;
    readonly ToolStripDropDownButton _ackButton;
    readonly ToolStripMenuItem _ackAllButton;
    readonly ToolStripDropDownButton _rejectButton;
    readonly ToolStripMenuItem _rejectAllButton;
    readonly ToolStripButton _reportButton;

    readonly ToolStripMenuItem _clientMenuStrip;
    readonly ToolStripMenuItem _serverMenuStrip;

    readonly ToolStripMenuItem _cancelMenuItem;
    readonly ToolStripMenuItem _amendMenuItem;
    readonly ToolStripMenuItem _statusMenuItem;
    readonly ToolStripMenuItem _listExecuteMenuItem;
    readonly ToolStripMenuItem _listStatusMenuItem;
    readonly ToolStripMenuItem _listCancelMenuItem;


    readonly ToolStripMenuItem _ackMenuItem;
    readonly ToolStripMenuItem _rejectMenuItem;
    readonly ToolStripMenuItem _reportMenuItem;

    Session? _session;

    public OrdersPanel(MessagesPanel messageDefaults, ToolStripButton defaultsButton)
    {
        _messageDefaults = messageDefaults;
        _defaultsButton = defaultsButton;

        #region ToolStrip
        _clientToolStrip = new ToolStrip
        {
            GripStyle = ToolStripGripStyle.Hidden,
            BackColor = LookAndFeel.Color.ToolStrip,
            Renderer = new ToolStripRenderer()
        };

        _cancelButton = new ToolStripSplitButton
        {
            Text = "Cancel",
            ToolTipText = "Cancel the selected order"
        };
        _cancelButton.Click += CancelButtonClick;

        _cancelAllButton = new ToolStripMenuItem
        {
            Text = "Cancel All",
            ToolTipText = "Cancel all open orders immediately"
        };
        _cancelAllButton.Click += CancelAllButtonClick;
        _cancelButton.DropDownItems.Add(_cancelAllButton);

        _amendButton = new ToolStripButton
        {
            Text = "Amend",
            ToolTipText = "Amend the selected order"
        };
        _amendButton.Click += AmendButtonClick;

        _statusButton = new ToolStripButton
        {
            Text = "Status",
            ToolTipText = "Request a status update for the selected order"
        };
        _statusButton.Click += StatusButtonClick;

        _listExecuteButton = new ToolStripButton
        {
            Text = "ListExecute",
            ToolTipText = "Execute the Order List associated with the selected order"
        };
        _listExecuteButton.Click += ListExecuteButtonClick;

        _listStatusButton = new ToolStripButton
        {
            Text = "ListStatus",
            ToolTipText = "Request the status of the Order List associated with the selected order"
        };
        _listStatusButton.Click += ListStatusButtonClick;

        _listCancelButton = new ToolStripButton
        {
            Text = "ListCancel",
            ToolTipText = "Cancel the Order List associated with the selected order"
        };
        _listCancelButton.Click += ListCancelButtonClick;

        _clientToolStrip.Items.AddRange(new ToolStripItem[]
        {
            _cancelButton,
            _amendButton,
            _statusButton,
            new ToolStripSeparator(),
            _listExecuteButton,
            _listStatusButton,
            _listCancelButton
        });

        _serverToolStrip = new ToolStrip
        {
            GripStyle = ToolStripGripStyle.Hidden,
            BackColor = LookAndFeel.Color.ToolStrip,
            Renderer = new ToolStripRenderer()
        };

        _ackButton = new ToolStripDropDownButton
        {
            Text = "Ack",
            ToolTipText = "Acknowledge the selected order"
        };
        _ackButton.Click += AckButtonClick;

        _ackAllButton = new ToolStripMenuItem
        {
            Text = "Acknowledge All",
            ToolTipText = "Acknowledge all pending orders immediately"
        };
        _ackAllButton.Click += AcknowledgeAllButtonClick;
        _ackButton.DropDownItems.Add(_ackAllButton);

        _rejectButton = new ToolStripDropDownButton
        {
            Text = "Reject",
            ToolTipText = "Reject the selected order"
        };
        _rejectButton.Click += RejectButtonClick;

        _rejectAllButton = new ToolStripMenuItem
        {
            Text = "Reject All",
            ToolTipText = "Reject all pending orders immediately"
        };
        _rejectAllButton.Click += RejectAllButtonClick;
        _rejectButton.DropDownItems.Add(_rejectAllButton);

        _acceptorCancelButton = new ToolStripSplitButton
        {
            Text = "Cancel",
            ToolTipText = "Cancel the selected order"
        };
        _acceptorCancelButton.Click += CancelButtonClick;

        _acceptorCancelAllButton = new ToolStripMenuItem
        {
            Text = "Cancel All",
            ToolTipText = "Cancel all open orders immediately"
        };
        _acceptorCancelAllButton.Click += CancelAllButtonClick;
        _acceptorCancelButton.DropDownItems.Add(_acceptorCancelAllButton);

        _reportButton = new ToolStripButton
        {
            Text = "Report",
            ToolTipText = "Send an execution teport for the selected order"
        };
        _reportButton.Click += ReportButtonClick;

        _serverToolStrip.Items.AddRange(new ToolStripItem[]
        {
            _ackButton,
            _rejectButton,
            _acceptorCancelButton,
            _reportButton
        });

        #endregion

        #region MenuStrip
        _clientMenuStrip = new ToolStripMenuItem("Action");

        _cancelMenuItem = new ToolStripMenuItem("Cancel", _cancelButton.Image);
        _cancelMenuItem.Click += CancelButtonClick;
        _clientMenuStrip.DropDownItems.Add(_cancelMenuItem);

        _amendMenuItem = new ToolStripMenuItem("Amend", _amendButton.Image);
        _amendMenuItem.Click += AmendButtonClick;
        _clientMenuStrip.DropDownItems.Add(_amendMenuItem);

        _statusMenuItem = new ToolStripMenuItem("Status", _statusButton.Image);
        _statusMenuItem.Click += StatusButtonClick;
        _clientMenuStrip.DropDownItems.Add(_statusMenuItem);

        _clientMenuStrip.DropDownItems.Add(new ToolStripSeparator());

        _listExecuteMenuItem = new ToolStripMenuItem("ListExecute", _listExecuteButton.Image);
        _listExecuteMenuItem.Click += ListExecuteButtonClick;
        _clientMenuStrip.DropDownItems.Add(_listExecuteMenuItem);

        _listStatusMenuItem = new ToolStripMenuItem("ListStatus", _listStatusButton.Image);
        _listStatusMenuItem.Click += ListStatusButtonClick;
        _clientMenuStrip.DropDownItems.Add(_listStatusMenuItem);

        _listCancelMenuItem = new ToolStripMenuItem("ListCancel", _listCancelButton.Image);
        _listCancelMenuItem.Click += ListCancelButtonClick;
        _clientMenuStrip.DropDownItems.Add(_listCancelMenuItem);

        _serverMenuStrip = new ToolStripMenuItem("Action");

        _ackMenuItem = new ToolStripMenuItem("Acknowledge", _ackButton.Image);
        _ackMenuItem.Click += AckButtonClick;
        _serverMenuStrip.DropDownItems.Add(_ackMenuItem);

        _rejectMenuItem = new ToolStripMenuItem("Reject", _rejectButton.Image);
        _rejectMenuItem.Click += RejectButtonClick;
        _serverMenuStrip.DropDownItems.Add(_rejectMenuItem);

        _reportMenuItem = new ToolStripMenuItem("Report", _reportButton.Image);
        _reportMenuItem.Click += ReportButtonClick;
        _serverMenuStrip.DropDownItems.Add(_reportMenuItem);
        #endregion

        _orderTable = new OrderDataTable("Orders");
        _orderView = new DataView(_orderTable);

        _orderGrid = new OrderDataGridView
        {
            Dock = DockStyle.Fill,
            DataSource = _orderView
        };
        _orderGrid.SelectionChanged += (sender, ev) => UpdateUiState();

        _orderSearchTextBox = new SearchTextBox
        {
            Dock = DockStyle.Top
        };
        _orderSearchTextBox.TextChanged += (sender, ev) => ApplyFilters();

        ContentPanel.Controls.Add(_orderGrid);
        ContentPanel.Controls.Add(_orderSearchTextBox);

        UpdateUiState();
        ApplyFilters();

        IntPtr h = Handle;
    }

    void ApplyFilters()
    {
        if (_orderView.Table is null)
        {
            return;
        }

        var buffer = new StringBuilder();

        string text = _orderSearchTextBox.Text;

        if (!string.IsNullOrEmpty(text))
        {
            foreach (DataColumn column in _orderView.Table.Columns)
            {
                if (column.ColumnMapping == MappingType.Hidden)
                    continue;

                if (column.DataType.IsEnum)
                {
                    buffer.AppendFormat("{0} LIKE '%{1}%' OR ", column.ColumnName + "String", text);
                }
                else
                {
                    buffer.AppendFormat("CONVERT({0}, System.String) LIKE '%{1}%' OR ", column.ColumnName, text);
                }
            }
            buffer.Remove(buffer.Length - 3, 3);
        }

        if (buffer.Length == 0)
        {
            _orderView.RowFilter = null;
            _orderView.Sort = string.Empty;
        }
        else
        {
            _orderView.RowFilter = buffer.ToString();
        }

        _orderSearchTextBox.Focus();
    }

    void AcknowledgeAllButtonClick(object? sender, EventArgs e)
    {
        DialogResult result = MessageBox.Show(this,
                                                "This will acknowledge all pending orders, are you sure?",
                                                Application.ProductName,
                                                MessageBoxButtons.YesNo,
                                                MessageBoxIcon.Information);

        if (result != DialogResult.Yes)
            return;

        AcknowledgeAllPendingOrders();
    }

    void RejectAllButtonClick(object? sender, EventArgs e)
    {
        DialogResult result = MessageBox.Show(this,
                                                "This will reject all pending orders, are you sure?",
                                                Application.ProductName,
                                                MessageBoxButtons.YesNo,
                                                MessageBoxIcon.Information);

        if (result != DialogResult.Yes)
            return;

        RejectAllPendingOrders();
    }

    void AcknowledgeAllPendingOrders()
    {
        if (Session is null)
        {
            return;
        }

        foreach (DataGridViewRow row in _orderGrid.Rows)
        {
            try
            {
                var view = row.DataBoundItem as DataRowView;
                var orderRow = view?.Row as OrderDataRow;

                if (orderRow?.Order is null)
                {
                    continue;
                }

                Fix.Order order = orderRow.Order;

                if (order is null)
                {
                    continue;
                }

                if (order.OrdStatus != null)
                {
                    continue;
                }

                var message = new Fix.Message { MsgType = FIX_5_0SP2.Messages.ExecutionReport.MsgType };

                message.Fields.Set(FIX_5_0SP2.Fields.ClOrdID, order.ClOrdID);

                if (order.Side is FieldValue side)
                {
                    message.Fields.Set(order.Side);
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

                Session.Send(message);
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

    void RejectAllPendingOrders()
    {
        if (Session is null)
        {
            return;
        }

        foreach (DataGridViewRow row in _orderGrid.Rows)
        {
            try
            {
                var view = row.DataBoundItem as DataRowView;
                var orderRow = view?.Row as OrderDataRow;
                Fix.Order? order = orderRow?.Order;

                if (order is null)
                {
                    continue;
                }

                if (order.OrdStatus != null)
                {
                    continue;
                }

                var message = new Fix.Message { MsgType = FIX_5_0SP2.Messages.ExecutionReport.MsgType };

                message.Fields.Set(FIX_5_0SP2.Fields.ClOrdID, order.ClOrdID);

                if (order.Side is FieldValue side)
                {
                    message.Fields.Set(order.Side);
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

                Session.Send(message);
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

    void CancelAllButtonClick(object? sender, EventArgs e)
    {
        if (Session is null)
        {
            return;
        }

        DialogResult result = MessageBox.Show(this,
            "This will cancel all open orders, are you sure?",
            Application.ProductName,
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Information);

        if (result != DialogResult.Yes)
            return;

        if (Session.OrderBehaviour == Fix.Behaviour.Initiator)
        {
            CancelAllOpenOrders();
        }
        else
        {
            UnsolicitedCancelAllOpenOrders();
        }
    }

    void UnsolicitedCancelAllOpenOrders()
    {
        if (Session is null)
        {
            return;
        }

        foreach (DataGridViewRow row in _orderGrid.Rows)
        {
            try
            {
                var view = row.DataBoundItem as DataRowView;
                var orderRow = view?.Row as OrderDataRow;
                Fix.Order? order = orderRow?.Order;

                if (order is null)
                {
                    continue;
                }

                if (order.OrdStatus == FIX_5_0SP2.OrdStatus.Canceled ||
                    order.OrdStatus == FIX_5_0SP2.OrdStatus.Rejected ||
                    order.OrdStatus == FIX_5_0SP2.OrdStatus.DoneForDay)
                {
                    continue;
                }

                var message = new Fix.Message { MsgType = FIX_5_0SP2.Messages.ExecutionReport.MsgType };

                message.Fields.Set(FIX_5_0SP2.Fields.ClOrdID, order.ClOrdID);

                if (order.Side is FieldValue side)
                {
                    message.Fields.Set(side);
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

                Session.Send(message);
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

    void CancelAllOpenOrders()
    {
        if (Session is null)
        {
            return;
        }

        foreach (DataGridViewRow row in _orderGrid.Rows)
        {
            if (_session is null)
            {
                return;
            }

            try
            {
                var view = row.DataBoundItem as DataRowView;
                var orderRow = view?.Row as OrderDataRow;
                Fix.Order? order = orderRow?.Order;

                if (order is null)
                {
                    return;
                }

                if (order.OrdStatus == FIX_5_0SP2.OrdStatus.Canceled ||
                    order.OrdStatus == FIX_5_0SP2.OrdStatus.Rejected ||
                    order.OrdStatus == FIX_5_0SP2.OrdStatus.DoneForDay)
                {
                    continue;
                }

                var message = new Fix.Message { MsgType = FIX_5_0SP2.Messages.OrderCancelRequest.MsgType };

                MessagesPanel.UpdateMessage(message, order);

                message.Fields.Set(FIX_5_0SP2.Fields.TransactTime, Fix.Field.TimestampString(_session.OutgoingTimestampFormat));

                if (order.Side is FieldValue side)
                {
                    message.Fields.Set(FIX_5_0SP2.Fields.Side, side.Value);
                }

                message.Fields.Set(FIX_5_0SP2.Fields.Symbol, order.Symbol);
                message.Fields.Set(FIX_5_0SP2.Fields.OrigClOrdID, order.ClOrdID);
                message.Fields.Set(FIX_5_0SP2.Fields.ClOrdID, Session.FormatClOrdId(Session.NextClOrdId++));
                //
                // This field was removed from later versions.
                //
                Fix.Field? beginString = message.Fields.Find(FIX_5_0SP2.Fields.BeginString);

                if (beginString is not null && beginString.Value == "FIX.4.0")
                {
                    message.Fields.Set(FIX_4_2.Fields.CxlType, "F");
                }

                Session.Send(message);
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

    void ListCancelButtonClick(object? sender, EventArgs e)
    {
        if (SelectedOrder is not Fix.Order order)
        {
            return;
        }

        if (order.ListID == null)
        {
            MessageBox.Show(this,
                            "The selected order does not have a ListID so a ListCancel cannot be performed",
                            Application.ProductName,
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
            return;
        }

        _defaultsButton.PerformClick();
        _messageDefaults.ListCancel(order.ListID);
    }

    void ListStatusButtonClick(object? sender, EventArgs e)
    {
        if (SelectedOrder is not Fix.Order order)
        {
            return;
        }

        if (order.ListID == null)
        {
            MessageBox.Show(this,
                            "The selected order does not have a ListID so a ListStatus cannot be performed",
                            Application.ProductName,
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
            return;
        }

        _defaultsButton.PerformClick();
        _messageDefaults.ListStatus(order.ListID);
    }

    void ListExecuteButtonClick(object? sender, EventArgs e)
    {
        if (SelectedOrder is not Fix.Order order)
        {
            return;
        }

        if (order.ListID == null)
        {
            MessageBox.Show(this,
                            "The selected order does not have a ListID so a ListExecute cannot be performed",
                            Application.ProductName,
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
            return;
        }

        _defaultsButton.PerformClick();
        _messageDefaults.ListExecute(order.ListID);
    }

    void ReportButtonClick(object? sender, EventArgs e)
    {
        if (SelectedOrder is not Fix.Order order)
        {
            return;
        }

        _defaultsButton.PerformClick();
        _messageDefaults.ReportOrder(order);
    }

    void RejectButtonClick(object? sender, EventArgs e)
    {
        if (SelectedOrder is not Fix.Order order)
        {
            return;
        }

        _defaultsButton.PerformClick();
        _messageDefaults.RejectOrder(order);
    }

    void AckButtonClick(object? sender, EventArgs e)
    {
        if (SelectedOrder is not Fix.Order order)
        {
            return;
        }

        _defaultsButton.PerformClick();

        if (order.OrdStatus == FIX_5_0SP2.OrdStatus.PendingCancel)
        {
            _messageDefaults.AcknowledgeCancel(order);
        }
        else if (order.OrdStatus == FIX_5_0SP2.OrdStatus.PendingReplace)
        {
            _messageDefaults.AcknowledgeAmend(order);
        }
        else
        {
            _messageDefaults.AcknowledgeOrder(order);
        }
    }

    void StatusButtonClick(object? sender, EventArgs e)
    {
        if (SelectedOrder is not Fix.Order order)
        {
            return;
        }

        _defaultsButton.PerformClick();
        _messageDefaults.OrderStatus(order);
    }

    void AmendButtonClick(object? sender, EventArgs e)
    {
        if (SelectedOrder is not Fix.Order order)
        {
            return;
        }

        _defaultsButton.PerformClick();
        _messageDefaults.AmendOrder(order);
    }

    void CancelButtonClick(object? sender, EventArgs e)
    {
        if (SelectedOrder is not Fix.Order order)
        {
            return;
        }

        if (Session is null)
        {
            return;
        }

        _defaultsButton.PerformClick();

        if (Session.OrderBehaviour == Fix.Behaviour.Initiator)
        {
            _messageDefaults.CancelOrder(order);
        }
        else
        {
            _messageDefaults.UnsolicitedCancelOrder(order);
        }
    }

    Fix.Order? SelectedOrder
    {
        get
        {
            if (_orderGrid.SelectedRows.Count == 0)
            {
                return null;
            }

            var row = _orderGrid.SelectedRows[0].DataBoundItem as DataRowView;
            var orderRow = row?.Row as OrderDataRow;
            return orderRow?.Order;
        }
    }

    void UpdateUiState()
    {
        bool enabled = false;
        bool listEnabled = false;

        if (Session != null && Session.Connected && _orderGrid.SelectedRows.Count > 0)
        {
            enabled = true;

            if (SelectedOrder is Fix.Order order)
            {
                if (order.ListID != null)
                {
                    listEnabled = true;
                }
            }
        }

        _cancelButton.Enabled = enabled;
        _amendButton.Enabled = enabled;
        _statusButton.Enabled = enabled;
        _listExecuteButton.Enabled = listEnabled;
        _listStatusButton.Enabled = listEnabled;
        _listCancelButton.Enabled = listEnabled;

        _ackButton.Enabled = enabled;
        _rejectButton.Enabled = enabled;
        _reportButton.Enabled = enabled;

        _cancelMenuItem.Enabled = enabled;
        _amendMenuItem.Enabled = enabled;
        _statusMenuItem.Enabled = enabled;
        _listExecuteMenuItem.Enabled = listEnabled;
        _listStatusMenuItem.Enabled = listEnabled;
        _listCancelButton.Enabled = listEnabled;

        _ackMenuItem.Enabled = enabled;
        _rejectMenuItem.Enabled = enabled;
        _reportMenuItem.Enabled = enabled;
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
                _session.OrderBook.Messages.Reset -= MessagesReset;
                _session.OrderBook.OrderInserted -= OrderBookOrderInserted;
                _session.OrderBook.OrderUpdated -= OrderBookOrderUpdated;
                _session.SessionReset -= SessionSessionReset;
                _session.StateChanged -= SessionStateChanged;
            }

            _session = value;
            Reload();

            if (_session != null)
            {
                _session.OrderBook.Messages.Reset += MessagesReset;
                _session.OrderBook.OrderInserted += OrderBookOrderInserted;
                _session.OrderBook.OrderUpdated += OrderBookOrderUpdated;
                _session.SessionReset += SessionSessionReset;
                _session.StateChanged += SessionStateChanged;
            }

            if (value?.OrderBehaviour == Fix.Behaviour.Initiator)
            {
                TopToolStripPanel.Controls.Clear();
                TopToolStripPanel.Join(_clientToolStrip);
                SetMenuStrip(_clientMenuStrip);
            }
            else
            {
                TopToolStripPanel.Controls.Clear();
                TopToolStripPanel.Join(_serverToolStrip);
                SetMenuStrip(_serverMenuStrip);
            }
        }
    }

    void SessionSessionReset(object? sender, EventArgs ev)
    {
        if (InvokeRequired)
        {
            BeginInvoke(new MethodInvoker(() => SessionSessionReset(sender, ev)));
            return;
        }

        Reload();
    }

    void MessagesReset(object? sender)
    {
        if (InvokeRequired)
        {
            BeginInvoke(new MethodInvoker(() => MessagesReset(sender)));
            return;
        }

        Reload();
    }

    void SessionStateChanged(object? sender, Fix.Session.StateEvent ev)
    {
        if (InvokeRequired)
        {
            BeginInvoke(new MethodInvoker(() => SessionStateChanged(sender, ev)));
            return;
        }

        UpdateUiState();
    }

    void OrderBookOrderUpdated(object? sender, Fix.Order order)
    {
        if (InvokeRequired)
        {
            BeginInvoke(new MethodInvoker(() => OrderBookOrderUpdated(sender, order)));
            return;
        }

        if (_orderTable.Rows.Find(order.ClOrdID) is not OrderDataRow row)
        {
            return;
        }

        row.Order = order;
        UpdateRow(row);
        _orderGrid.RefreshEdit();
    }

    void OrderBookOrderInserted(object? sender, Fix.Order order)
    {
        if (InvokeRequired)
        {
            BeginInvoke(new MethodInvoker(() => OrderBookOrderInserted(sender, order)));
            return;
        }

        AddOrder(order);
    }

    void AddOrder(Fix.Order order)
    {
        if (_orderTable.Rows.Find(order.ClOrdID) is OrderDataRow _)
        {
            return;
        }

        var row = (OrderDataRow)_orderTable.NewRow();
        row.Order = order;
        row[OrderDataTable.ColumnClOrdId] = order.ClOrdID;
        //
        // Prime these with 0's so the grid looks pretty.
        //
        row[OrderDataTable.ColumnDone] = 0;
        row[OrderDataTable.ColumnLeaves] = 0;
        row[OrderDataTable.ColumnAvgPrice] = 0;
        UpdateRow(row);
        _orderTable.Rows.Add(row);
    }

    static void UpdateRow(OrderDataRow row)
    {
        if (row.Order is not Fix.Order order)
        {
            return;
        }

        row[OrderDataTable.ColumnQuantity] = order.OrderQty;
        row[OrderDataTable.ColumnPendingQuantity] = order.PendingOrderQty;
        row[OrderDataTable.ColumnSymbol] = order.Symbol;

        if (order.TimeInForce != null)
        {
            row[OrderDataTable.ColumnTimeInForce] = order.TimeInForce;
            row[OrderDataTable.ColumnTimeInForceString] = OrderDataGridView.ShortTimeInForceDescription(order.TimeInForce);
        }

        if (order.OrdStatus != null)
        {
            row[OrderDataTable.ColumnOrdStatus] = order.OrdStatus;
            row[OrderDataTable.ColumnOrdStatusString] = order.OrdStatus.Name;
        }
        else
        {
            row[OrderDataTable.ColumnOrdStatusString] = FIX_5_0SP2.OrdStatus.PendingNew.Name;
        }

        if (order.OrigClOrdID != null)
            row[OrderDataTable.ColumnOrigClOrdId] = order.OrigClOrdID;

        if (order.Side != null)
        {
            row[OrderDataTable.ColumnSide] = order.Side;
            row[OrderDataTable.ColumnSideString] = order.Side.Name;
        }

        long leavesQty = 0;
        long cumQty = order.CumQty ?? 0;

        if (order.LeavesQty.HasValue)
        {
            leavesQty = order.LeavesQty.Value;
        }
        else if (order.CumQty.HasValue)
        {
            if (order.Active)
            {
                leavesQty = order.OrderQty - order.CumQty ?? 0;
            }
        }

        row[OrderDataTable.ColumnLeaves] = leavesQty;
        row[OrderDataTable.ColumnDone] = cumQty;

        if (order.AvgPx != null)
            row[OrderDataTable.ColumnAvgPrice] = order.AvgPx.Value;

        if (order.Price != null)
            row[OrderDataTable.ColumnLimit] = order.Price.Value;

        row[OrderDataTable.ColumnPendingLimit] = order.PendingPrice;

        if (order.ListID != null)
            row[OrderDataTable.ColumnListId] = order.ListID;

        if (order.Text != null)
            row[OrderDataTable.ColumnText] = order.Text;
    }

    void Reload()
    {
        if (Session is null)
        {
            return;
        }

        try
        {
            _orderTable.BeginLoadData();
            _orderTable.Clear();

            foreach (Fix.Order order in Session.OrderBook.Orders)
            {
                AddOrder(order);
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
        finally
        {
            _orderTable.EndLoadData();
            UpdateUiState();
        }
    }
}
