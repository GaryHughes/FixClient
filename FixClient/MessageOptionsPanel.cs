/////////////////////////////////////////////////
//
// FIX Client
//
// Copyright @ 2021 VIRTU Financial Inc.
// All rights reserved.
//
// Filename: MessageOptionsPanel.cs
// Author:   Gary Hughes
//
/////////////////////////////////////////////////
using System.ComponentModel;

namespace FixClient;

public partial class MessageOptionsPanel : Panel
{
    class MessageOptions
    {
        const string CategoryAuto = "Auto Set Fields";

        [DisplayName("MsgSeqNum")]
        [Description("Automatically increment the MsgSeqNum field when sending a message")]
        [Category(CategoryAuto)]
        public bool AutoSetMsgSeqNum { get; set; }

        [DisplayName("SendingTime")]
        [Description("Automatically populate the SendingTime field in any messages that have it")]
        [Category(CategoryAuto)]
        public bool AutoSendingTime { get; set; }

        [DisplayName("TransactTime")]
        [Description("Automatically populate the TransactTime field in any messages that have it")]
        [Category(CategoryAuto)]
        public bool AutoTransactTime { get; set; }

        [DisplayName("TotNoOrders")]
        [Description("Automatically increment the TotNoOrders field in an OrderList when repeating a group of fields containing a ClOrdID")]
        [Category(CategoryAuto)]
        public bool AutoTotNoOrders { get; set; }

        [DisplayName("NoOrders")]
        [Description("Automatically increment the NoOrders field in an OrderList when repeating a group of fields containing a ClOrdID")]
        [Category(CategoryAuto)]
        public bool AutoNoOrders { get; set; }

        [DisplayName("ListID")]
        [Description("Automatically increment the ListID field in an OrderList when sending")]
        [Category(CategoryAuto)]
        public bool AutoListId { get; set; }

        [DisplayName("ListSeqNo")]
        [Description("Automatically set the ListSeqNo field in OrderList")]
        [Category(CategoryAuto)]
        public bool AutoListSeqNo { get; set; }

        [DisplayName("ClOrdID")]
        [Description("Automatically set the ClOrdID field in OrderSingle and OrderList")]
        [Category(CategoryAuto)]
        public bool AutoClOrdId { get; set; }

        [DisplayName("AllocID")]
        [Description("Automatically increment the AllocID field in any messages that have it")]
        [Category(CategoryAuto)]
        public bool AutoAllocId { get; set; }
    }

    Session? _session;
    readonly MessageOptions _options = new();
    readonly PropertyGrid _propertyGrid;

    public MessageOptionsPanel()
    {
        InitializeComponent();

        _propertyGrid = new PropertyGrid
        {
            Dock = DockStyle.Fill,
            HelpVisible = false,
            ToolbarVisible = false,
            SelectedObject = _options
        };
        _propertyGrid.PropertyValueChanged += PropertyGridPropertyValueChanged;

        //_propertyGrid.Layout += (sender, ev) => MoveSplitter(_propertyGrid, 190);

        var panel1 = new BorderHidingPanel(_propertyGrid) { Dock = DockStyle.Fill };

        Controls.Add(panel1);

        UpdateUiState();
    }

    //static void MoveSplitter(PropertyGrid propertyGrid, int x)
    //{
    /* TODO
    object propertyGridView = typeof(PropertyGrid).InvokeMember("gridView", 
                                                                BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Instance, 
                                                                null, 
                                                                propertyGrid, 
                                                                null);
    propertyGridView.GetType().InvokeMember("MoveSplitterTo",
                                            BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Instance,
                                            null, 
                                            propertyGridView, 
                                            new object[] { x });
    */
    //}

    void PropertyGridPropertyValueChanged(object? s, PropertyValueChangedEventArgs e)
    {
        if (_session is null)
        {
            return;
        }

        var value = (bool)(e.ChangedItem?.Value ?? false);

        switch (e.ChangedItem?.Label)
        {
            case "MsgSeqNum":
                _session.AutoSetMsgSeqNum = value;
                break;
            case "SendingTime":
                _session.AutoSendingTime = value;
                break;
            case "TransactTime":
                _session.AutoTransactTime = value;
                break;
            case "TotNoOrders":
                _session.AutoTotNoOrders = value;
                break;
            case "NoOrders":
                _session.AutoNoOrders = value;
                break;
            case "ListID":
                _session.AutoListId = value;
                break;
            case "ListSeqNo":
                _session.AutoListSeqNo = value;
                break;
            case "ClOrdID":
                _session.AutoClOrdId = value;
                break;
            case "AllocID":
                _session.AutoAllocId = value;
                break;
        }
        _session.Write();
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Session? Session
    {
        get { return _session; }
        set
        {
            _session = value;

            if (_session is not null)
            {
                _options.AutoAllocId = _session.AutoAllocId;
                _options.AutoClOrdId = _session.AutoClOrdId;
                _options.AutoListId = _session.AutoListId;
                _options.AutoListSeqNo = _session.AutoListSeqNo;
                _options.AutoNoOrders = _session.AutoNoOrders;
                _options.AutoSendingTime = _session.AutoSendingTime;
                _options.AutoSetMsgSeqNum = _session.AutoSetMsgSeqNum;
                _options.AutoTotNoOrders = _session.AutoTotNoOrders;
                _options.AutoTransactTime = _session.AutoTransactTime;
            }

            UpdateUiState();
        }
    }

    void UpdateUiState()
    {
        _propertyGrid.Enabled = Session != null;
    }
}

