/////////////////////////////////////////////////
//
// FIX Client
//
// Copyright @ 2021 VIRTU Financial Inc.
// All rights reserved.
// 
// Filename: Order.cs
// Author:   Gary Hughes
//
/////////////////////////////////////////////////
using static Fix.Dictionary;

namespace Fix;

public class Order : ICloneable
{
    public Order(Message message)
    {
        if (message.MsgType != FIX_5_0SP2.Messages.NewOrderSingle.MsgType)
        {
            throw new ArgumentException("Message is not an NewOrderSingle");
        }

        if (message.Fields.Find(FIX_5_0SP2.Fields.SenderCompID) is not Field senderField || string.IsNullOrEmpty(senderField.Value))
        {
            throw new ArgumentException("Message does not contain a SenderCompID");
        }

        SenderCompID = senderField.Value;

        if (message.Fields.Find(FIX_5_0SP2.Fields.TargetCompID) is not Field targetField || string.IsNullOrEmpty(targetField.Value))
        {
            throw new ArgumentException("Message does not contain a TargetCompID");
        }

        TargetCompID = targetField.Value;

        if (message.Fields.Find(FIX_5_0SP2.Fields.Symbol) is Field symbolField && !string.IsNullOrEmpty(symbolField.Value))
        {
            Symbol = symbolField.Value;
        }
        else
        {
            if (message.Fields.Find(FIX_5_0SP2.Fields.SecurityID) is not Field securityIdField || string.IsNullOrEmpty(securityIdField.Value))
            {
                throw new ArgumentException("Message does not contain a Symbol or SecurityID");
            }

            Symbol = securityIdField.Value;
        }

        if (message.Fields.Find(FIX_5_0SP2.Fields.ClOrdID) is not Field clOrdIdField || string.IsNullOrEmpty(clOrdIdField.Value))
        {
            throw new ArgumentException("Message does not contain a ClOrdID");
        }

        ClOrdID = clOrdIdField.Value;

        if (message.Fields.Find(FIX_5_0SP2.Fields.OrderQty) is not Field orderQtyField || string.IsNullOrEmpty(orderQtyField.Value))
        {
            throw new ArgumentException("Message does not contain an OrderQty");
        }

        if ((long?)orderQtyField is long qty)
        {
            OrderQty = qty;
        }

        if (message.Fields.Find(FIX_5_0SP2.Fields.ExDestination) is Field field)
        {
            ExDestination = field.Value;
        }

        if (message.Fields.Find(FIX_5_0SP2.Fields.Price) is Field priceField)
        {
            Price = (decimal?)priceField;
        }

        if (message.Fields.Find(FIX_5_0SP2.Fields.Side) is Field sideField)
        {
            Side = (FieldValue?)sideField;
        }

        if (message.Fields.Find(FIX_5_0SP2.Fields.TimeInForce) is Field timeInForceField)
        {
            TimeInForce = (FieldValue?)timeInForceField;
        }

        if (message.Fields.Find(FIX_5_0SP2.Fields.Text) is Field textField)
        {
            Text = textField?.Value;
        }

        if (message.Fields.Find(FIX_5_0SP2.Fields.ListID) is Field listIdField)
        {
            ListID = listIdField.Value;
        }

        if (message.Fields.Find(FIX_5_0SP2.Fields.SendingTime) is Field sendingTimeField && !string.IsNullOrEmpty(sendingTimeField.Value))
        {
            if ((DateTime?)sendingTimeField is DateTime sendingTime)
            {
                SendingTime = sendingTime;
            }
        }

        Messages = new List<Message>
        {
            message
        };

        Key = CreateKey(SenderCompID, TargetCompID, ClOrdID);
    }

    public List<Message> Messages { get; private set; }

    public string SenderCompID { get; set; }
    public string TargetCompID { get; set; }
    public string ClOrdID { get; set; }
    public string? NewClOrdID { get; set; } // This is for replaced orders, it is the reverse of OrigClOrdID
    public string Symbol { get; set; }
    public long OrderQty { get; set; }

    public long? CumQty { get; set; }
    public long? LeavesQty { get; set; }
    public string? ListID { get; set; }
    public string? OrigClOrdID { get; set; }
    public decimal? Price { get; set; }
    public decimal? AvgPx { get; set; }
    public FieldValue? Side { get; set; }
    public FieldValue? TimeInForce { get; set; }
    public FieldValue? OrdStatus { get; set; }
    public FieldValue? PreviousOrdStatus { get; set; }
    public string? OrderID { get; set; }
    public string? Text { get; set; }
    public string? ExDestination { get; set; }

    public Message? PendingMessage { get; set; }
    public long? PendingOrderQty { get; set; }
    public decimal? PendingPrice { get; set; }
    public DateTime SendingTime { get; private set; }

    public bool Active
    {
        get
        {
            if (OrdStatus == null)
            {
                return true;
            }

            if (OrdStatus.Value == FIX_5_0SP2.OrdStatus.New.Value ||
                OrdStatus.Value == FIX_5_0SP2.OrdStatus.PartiallyFilled.Value ||
                OrdStatus.Value == FIX_5_0SP2.OrdStatus.Filled.Value ||
                OrdStatus.Value == FIX_5_0SP2.OrdStatus.PendingCancel.Value ||
                OrdStatus.Value == FIX_5_0SP2.OrdStatus.PendingNew.Value ||
                OrdStatus.Value == FIX_5_0SP2.OrdStatus.Calculated.Value ||
                OrdStatus.Value == FIX_5_0SP2.OrdStatus.AcceptedForBidding.Value ||
                OrdStatus.Value == FIX_5_0SP2.OrdStatus.PendingReplace.Value)
            {
                return true;
            }

            /*
                case Fix.OrdStatus.DoneForDay:
                case Fix.OrdStatus.Canceled:
                case Fix.OrdStatus.Replaced:
                case Fix.OrdStatus.Stopped:
                case Fix.OrdStatus.Rejected:
                case Fix.OrdStatus.Suspended:
                case Fix.OrdStatus.Expired:
                    break;
            */

            return false;
        }
    }

    public string Key { get; private set; }

    public void UpdateKey()
    {
        Key = CreateKey(SenderCompID, TargetCompID, ClOrdID);
    }

    static string CreateKey(string SenderCompID, string TargetCompID, string ClOrdID)
    {
        return $"{SenderCompID}-{TargetCompID}-{ClOrdID}";
    }

    public object Clone()
    {
        var clone = (Order)MemberwiseClone();
        clone.Messages = new List<Message>();
        foreach (var message in Messages)
        {
            clone.Messages.Add((Message)message.Clone());
        }
        return clone;
    }
}

