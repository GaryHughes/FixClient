using Fix;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace FixDictionary.Tests;

[TestClass]
public class FieldTests
{
    [TestMethod]
    public void TestIndexLessThanZero()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Dictionary.FIX_4_2.Fields[-1]);
    }

    [TestMethod]
    public void TestIndexGreaterThanLength()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Dictionary.FIX_4_2.Fields[Dictionary.FIX_4_2.Fields.MaxTag + 1]);
    }

    [TestMethod]
    public void TestIndexerWithTagZero()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Dictionary.FIX_4_2.Fields[0]);
    }

    [TestMethod]
    public void TestStringIndexerWithInvalidName()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Dictionary.FIX_4_2.Fields["Blah"]);
    }

    [TestMethod]
    public void TestStringIndexerWithValidName()
    {
        Assert.IsNotNull(Dictionary.FIX_4_2.Fields["OrderQty"]);
    }

    [TestMethod]
    public void TestStringIndexerWithValidNameCaseInsensitive()
    {
        Assert.IsNotNull(Dictionary.FIX_4_2.Fields["orderqty"]);
    }

    [TestMethod]
    public void TestIndexWithTagOne()
    {
        Assert.IsNotNull(Dictionary.FIX_4_2.Fields[1]);
    }

    [TestMethod]
    public void TestIndexWithTagEqualToMaxTag()
    {
        var field = Dictionary.FIX_4_2.Fields.Last();
        if (field is null)
        {
            Assert.Fail("field is null");
            return;
        }
        Assert.IsNotNull(Dictionary.FIX_4_2.Fields[field.Tag]);
    }

    [TestMethod]
    public void TestIndexWithTagGreaterThanMaxTag()
    {
        var field = Dictionary.FIX_4_2.Fields.Last();
        if (field is null)
        {
            Assert.Fail("field is null");
            return;
        }
        Assert.Throws<ArgumentOutOfRangeException>(() => Dictionary.FIX_4_2.Fields[field.Tag + 1]);
    }

    [TestMethod]
    public void TestTryGetValueWithTagZeroFails()
    {
        Assert.IsFalse(Dictionary.FIX_4_2.Fields.TryGetValue(0, out var field));
        Assert.IsFalse(Dictionary.IsValid(field));
    }

    [TestMethod]
    public void TestTryGetValueWithTagOneSucceeds()
    {
        Assert.IsTrue(Dictionary.FIX_4_2.Fields.TryGetValue(1, out var field));
        Assert.IsNotNull(field);
    }

    [TestMethod]
    public void TestTryGetValueWithTagEqualToMaxTag()
    {
        var field = Dictionary.FIX_4_2.Fields.Last();
        if (field is null)
        {
            Assert.Fail("field is null");
            return;
        }
        Assert.IsTrue(Dictionary.FIX_4_2.Fields.TryGetValue(field.Tag, out var expected));
        Assert.IsNotNull(expected);
    }

    [TestMethod]
    public void TestTryGetValueWithTagGreaterThanMaxTag()
    {
        var field = Dictionary.FIX_4_2.Fields.Last();
        if (field is null)
        {
            Assert.Fail("field is null");
            return;
        }
        Assert.IsFalse(Dictionary.FIX_4_2.Fields.TryGetValue(field.Tag + 1, out var expected));
        Assert.IsFalse(Dictionary.IsValid(expected));
    }

    [TestMethod]
    public void TestFieldSequenceGaps()
    {
        Assert.IsNotNull(Fix.Dictionary.FIX_4_2.Fields[100]);
        Assert.IsFalse(Fix.Dictionary.IsValid(Fix.Dictionary.FIX_4_2.Fields[101]));
        Assert.IsNotNull(Fix.Dictionary.FIX_4_2.Fields[102]);
    }

    [TestMethod]
    public void TestDefinitions()
    {
        Assert.AreEqual(73, Fix.Dictionary.FIX_4_2.Fields.NoOrders.Tag);
        Assert.AreEqual("NoOrders", Fix.Dictionary.FIX_4_2.Fields.NoOrders.Name);
    }

    [TestMethod]
    public void TestFieldCollections()
    {
        Assert.AreEqual(446, Fix.Dictionary.FIX_4_2.Fields.MaxTag);
        Assert.AreEqual(956, Fix.Dictionary.FIX_4_4.Fields.MaxTag);
        Assert.AreEqual(50002, Fix.Dictionary.FIX_5_0SP2.Fields.MaxTag);
    }

    [TestMethod]
    public void TestFieldCollectionContains()
    {
        Assert.IsTrue(Fix.Dictionary.FIX_4_2.Fields.Contains(Fix.Dictionary.FIX_4_2.Fields.SessionRejectReason.Tag));
        Assert.IsFalse(Fix.Dictionary.FIX_4_2.Fields.Contains(Fix.Dictionary.FIX_4_4.Fields.LegContractSettlMonth.Tag));
    }

    [TestMethod]
    public void TestValues()
    {
        Assert.AreEqual("2", Dictionary.FIX_4_2.Side.Sell.Value);
        Assert.AreEqual("2", Dictionary.FIX_4_4.Side.Sell.Value);
        Assert.AreEqual("2", Dictionary.FIX_5_0SP2.Side.Sell.Value);
    }

    [TestMethod]
    public void TestFieldValueDescription()
    {
        var day = Dictionary.FIX_5_0SP2.TimeInForce.Day;
        Assert.AreEqual("A buy or sell order that, if not executed expires at the end of the trading day on which it was entered.", day.Description);
    }
}
