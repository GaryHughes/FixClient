using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using static Fix.Dictionary;

namespace FixTests;

[TestClass]
public class FieldTests
{
    /*
    [TestMethod]
    public void TestDescriptionOfExecInstWithMultipleValues()
    {
        var field = new Fix.Field(FIX_5_0SP2.Fields.ExecInst, "c x");
        Assert.AreEqual("Ignore price validity checks, Ignore notional value checks", field.ValueDescription);
    }
    */

    /*
    [TestMethod]
    public void TestExecInstWithMultipleValues()
    {
        var field = new Fix.Field(FIX_5_0SP2.Fields.ExecInst, "c x");
        var values = (Fix.ExecInst[])field;
        Assert.AreEqual(2, values.Length);
        Assert.AreEqual(FIX_5_0SP2.ExecInst.IgnorePriceValidityChecks, values[0]);
        Assert.AreEqual(FIX_5_0SP2.ExecInst.IgnoreNotionalValueChecks, values[1]);
    }
    */

    [TestMethod]
    public void TestIntTagConstructor()
    {
        var field = new Fix.Field(35, "D");
        Assert.AreEqual(35, field.Tag);
        Assert.AreEqual("D", field.Value);
    }

    [TestMethod]
    public void TestStringTagConstructor()
    {
        var field = new Fix.Field("35", "D");
        Assert.AreEqual(35, field.Tag);
        Assert.AreEqual("D", field.Value);
    }

    [TestMethod]
    public void TestInvalidStringTagConstructor()
    {
        Assert.Throws<ArgumentException>(() => new Fix.Field("blah", "D"));
    }

    [TestMethod]
    public void TestClone()
    {
        var one = new Fix.Field("35", "D");
        var two = (Fix.Field)one.Clone();
        Assert.AreEqual(one.Tag, two.Tag);
        Assert.AreEqual(one.Value, two.Value);
        two.Value = "E";
        Assert.AreEqual(one.Tag, two.Tag);
        Assert.AreNotEqual(one.Value, two.Value);
        Assert.AreEqual("D", one.Value);
        Assert.AreEqual("E", two.Value);
    }

    [TestMethod]
    public void TestShortTimestampDateTimeConversion()
    {
        var field = new Fix.Field(FIX_5_0SP2.Fields.ExpireTime, "20140306-06:01:32");
        Assert.IsNotNull((DateTime?)field);
    }

    [TestMethod]
    public void TestLongTimestampDateTimeConversion()
    {
        var field = new Fix.Field(FIX_5_0SP2.Fields.ExpireTime, "20140306-06:01:32.345");
        Assert.IsNotNull((DateTime?)field);
    }

    [TestMethod]
    public void TestDateDateTimeConversion()
    {
        var field = new Fix.Field(FIX_5_0SP2.Fields.ExpireTime, "20140306");
        Assert.IsNotNull((DateTime?)field);
    }

    [TestMethod]
    public void TestInvalidDateTimeConversion()
    {
        var field = new Fix.Field(FIX_5_0SP2.Fields.ExpireTime, "blah");
        Assert.IsNull((DateTime?)field);
    }

    [TestMethod]
    public void TestDescribeCanDescribeAllFIX50SP2Fields()
    {
        foreach (var field in FIX_5_0SP2.Fields)
        {
            if (field is null)
            {
                continue;
            }

            var description = Fix.Field.Describe(null, field.Tag, string.Empty);

            if (field.IsValid)
            {
                Assert.IsFalse(string.IsNullOrEmpty(description.Name), $"Failed to describe field with Tag={field.Tag}");
            }
            else
            {
                Assert.IsTrue(string.IsNullOrEmpty(description.Name), $"Unexpectedly found valid description for field with Tag={field.Tag}");
            }
        }
    }

    [TestMethod]
    public void TestDescribeCanDescribeAllFIX44Fields()
    {
        foreach (var field in FIX_4_4.Fields)
        {
            if (field is null)
            {
                continue;
            }

            var description = Fix.Field.Describe(null, field.Tag, string.Empty);

            if (field.IsValid)
            {
                Assert.IsFalse(string.IsNullOrEmpty(description.Name), $"Failed to describe field with Tag={field.Tag}");
            }
            else
            {
                Assert.IsTrue(string.IsNullOrEmpty(description.Name), $"Unexpectedly found valid description for field with Tag={field.Tag}");
            }
        }
    }

    [TestMethod]
    public void TestDescribeCanDescribeAllFIX42Fields()
    {
        foreach (var field in FIX_4_2.Fields)
        {
            if (field is null)
            {
                continue;
            }

            var description = Fix.Field.Describe(null, field.Tag, string.Empty);

            if (field.IsValid)
            {
                Assert.IsFalse(string.IsNullOrEmpty(description.Name), $"Failed to describe field with Tag={field.Tag}");
            }
            else
            {
                Assert.IsTrue(string.IsNullOrEmpty(description.Name), $"Unexpectedly found valid description for field with Tag={field.Tag}");
            }
        }
    }

    [TestMethod]
    public void TestDescribeFieldValue()
    {
        var field = new Fix.Field(FIX_5_0SP2.TimeInForce.Day);
        var description = field.Describe(null).ValueDefinition;
        Assert.IsNotNull(description);
        Assert.AreEqual("Day", description?.Name);
        Assert.AreEqual("A buy or sell order that, if not executed expires at the end of the trading day on which it was entered.", description?.Description);
    }
}

