using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using static Fix.Dictionary;

namespace FixTests;

[TestClass]
public class OrderTests
{
    [TestMethod]
    public void TestConstructorWrongMsgType()
    {
        var message = new Fix.Message { MsgType = FIX_5_0SP2.Messages.ExecutionReport.MsgType };
        Assert.Throws<ArgumentException>(() => new Fix.Order(message));
    }

    [TestMethod]
    public void TestConstructorNoSenderCompId()
    {
        var message = new Fix.Message { MsgType = FIX_5_0SP2.Messages.NewOrderSingle.MsgType };
        Assert.Throws<ArgumentException>(() => new Fix.Order(message));
    }

    [TestMethod]
    public void TestConstructorNoTargetCompId()
    {
        var message = new Fix.Message { MsgType = FIX_5_0SP2.Messages.NewOrderSingle.MsgType };
        message.Fields.Set(FIX_5_0SP2.Fields.SenderCompID, "SENDER");
        Assert.Throws<ArgumentException>(() => new Fix.Order(message));
    }

    [TestMethod]
    public void TestConstructorNoSymbol()
    {
        var message = new Fix.Message { MsgType = FIX_5_0SP2.Messages.NewOrderSingle.MsgType };
        message.Fields.Set(FIX_5_0SP2.Fields.SenderCompID, "SENDER");
        message.Fields.Set(FIX_5_0SP2.Fields.TargetCompID, "TARGET");
        Assert.Throws<ArgumentException>(() => new Fix.Order(message));
    }

    [TestMethod]
    public void TestConstructorNoClOrdId()
    {
        var message = new Fix.Message { MsgType = FIX_5_0SP2.Messages.NewOrderSingle.MsgType };
        message.Fields.Set(FIX_5_0SP2.Fields.SenderCompID, "SENDER");
        message.Fields.Set(FIX_5_0SP2.Fields.TargetCompID, "TARGET");
        message.Fields.Set(FIX_5_0SP2.Fields.Symbol, "BHP");
        Assert.Throws<ArgumentException>(() => new Fix.Order(message));
    }

    [TestMethod]
    public void TestConstructorAllMinimumRequirementsMet()
    {
        var message = new Fix.Message { MsgType = FIX_5_0SP2.Messages.NewOrderSingle.MsgType };
        message.Fields.Set(FIX_5_0SP2.Fields.SenderCompID, "SENDER");
        message.Fields.Set(FIX_5_0SP2.Fields.TargetCompID, "TARGET");
        message.Fields.Set(FIX_5_0SP2.Fields.Symbol, "BHP");
        message.Fields.Set(FIX_5_0SP2.Fields.ClOrdID, "1.2.3");
        message.Fields.Set(FIX_5_0SP2.Fields.OrderQty, 5000);
        var order = new Fix.Order(message);
        Assert.IsNotNull(order);
        Assert.AreEqual("SENDER", order.SenderCompID);
        Assert.AreEqual("TARGET", order.TargetCompID);
        Assert.AreEqual("BHP", order.Symbol);
        Assert.AreEqual("1.2.3", order.ClOrdID);
        Assert.AreEqual(5000, order.OrderQty);
        Assert.HasCount(1, order.Messages);
    }
}

