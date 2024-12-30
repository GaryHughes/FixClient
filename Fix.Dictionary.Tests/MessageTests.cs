using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace FixDictionary.Tests;

[TestClass]
public class MessageTests
{
    [TestMethod]
    public void TestMessageCollections()
    {
        Assert.AreEqual(46, Fix.Dictionary.FIX_4_2.Messages.Length);
        Assert.AreEqual(93, Fix.Dictionary.FIX_4_4.Messages.Length);
        Assert.AreEqual(164, Fix.Dictionary.FIX_5_0SP2.Messages.Length);
    }

    [TestMethod]
    public void TestMessageMsgType()
    {
        Assert.AreEqual("H", Fix.Dictionary.FIX_4_4.Messages.OrderStatusRequest.MsgType);
    }

    [TestMethod]
    public void TestMessageName()
    {
        Assert.AreEqual("OrderStatusRequest", Fix.Dictionary.FIX_4_4.Messages.OrderStatusRequest.Name);
    }

    [TestMethod]
    public void TestMessageDescription()
    {
        Assert.AreEqual("The order status request message is used by the institution to generate an order status message back from the broker.", Fix.Dictionary.FIX_4_4.Messages.OrderStatusRequest.Description);
    }

    [TestMethod]
    public void TestMessageAdded()
    {
        // TODO
        // Assert.AreEqual("FIX.2.7", Fix.Dictionary.FIX_4_4.Messages.OrderStatusRequest.Added);
    }

    [TestMethod]
    public void TestMessageFieldsLength()
    {
        Assert.AreEqual(104, Fix.Dictionary.FIX_4_2.Messages.OrderSingle.Fields.Count);
    }

    [TestMethod]
    public void TestVersionMessageFieldsDefinition()
    {
        Assert.AreEqual("BeginString", Fix.Dictionary.FIX_4_2.Messages.OrderSingle.Fields[0].Name);
        // TODO
        // Assert.AreEqual(true, Fix.Dictionary.FIX_4_2.Messages.OrderSingle.Fields[0].Required);
        // Assert.AreEqual(0, Fix.Dictionary.FIX_4_2.Messages.OrderSingle.Fields[0].Indent);
        // Assert.AreEqual("FIX.4.2", Fix.Dictionary.FIX_4_2.Messages.OrderSingle.Fields[0].Added);
    }

    [TestMethod]
    public void TestMessageFieldNotDefined()
    {
        Assert.IsNull(Fix.Dictionary.FIX_4_4.Messages.NewOrderSingle.Fields.Where(f => f.Tag == 6000).FirstOrDefault());
    }
}
