using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
    public void TestEqualFieldsHaveEqualHashCodes()
    {
        var one = new Fix.Field(35, "D");
        var two = new Fix.Field(35, "D");
        Assert.AreEqual(one, two);
        Assert.AreEqual(one.GetHashCode(), two.GetHashCode());
        Assert.HasCount(1, new HashSet<Fix.Field> { one, two });
        Assert.HasCount(2, new HashSet<Fix.Field> { one, new Fix.Field(35, "8") });
    }

    [TestMethod]
    public void TestFieldAndEqualFieldValueHaveEqualHashCodes()
    {
        var value = FIX_5_0SP2.Side.Buy;
        var field = new Fix.Field(value);
        Assert.IsTrue(field.Equals(value));
        Assert.AreEqual(value.GetHashCode(), field.GetHashCode());
    }

    const int RawDataLengthTag = 95;
    const int RawDataTag = 96;

    [TestMethod]
    public void TestCloneKeepsDataFlag()
    {
        var one = new Fix.Field(RawDataTag, Convert.ToBase64String(new byte[] { 0x01, 0x02, 0x03 })) { Data = true };
        var two = (Fix.Field)one.Clone();
        Assert.IsTrue(two.Data);
        Assert.AreEqual(one.Value, two.Value);
        Assert.AreEqual(one.ComputeCheckSum(), two.ComputeCheckSum());
        Assert.AreEqual(one.ComputeBodyLength(), two.ComputeBodyLength());
    }

    [TestMethod]
    public void TestMessageCloneKeepsDataFlag()
    {
        var message = new Fix.Message();
        message.Fields.Add(new Fix.Field(RawDataLengthTag, 3));
        message.Fields.Add(new Fix.Field(RawDataTag, Convert.ToBase64String(new byte[] { 0x01, 0x02, 0x03 })) { Data = true });
        var clone = (Fix.Message)message.Clone();
        Assert.IsTrue(clone.Fields.Find(RawDataTag)?.Data);
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

    static void WithCulture(CultureInfo culture, Action action)
    {
        var saved = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = culture;
            action();
        }
        finally
        {
            CultureInfo.CurrentCulture = saved;
        }
    }

    static CultureInfo CommaDecimalDotTimeCulture()
    {
        var culture = (CultureInfo)CultureInfo.GetCultureInfo("de-DE").Clone();
        culture.NumberFormat.NumberDecimalSeparator = ",";
        culture.NumberFormat.NumberGroupSeparator = ".";
        culture.DateTimeFormat.TimeSeparator = ".";
        return culture;
    }

    [TestMethod]
    public void TestDecimalValueIsWrittenWithInvariantCulture()
    {
        WithCulture(CommaDecimalDotTimeCulture(), () =>
        {
            Assert.AreEqual("1.5", new Fix.Field(44, 1.5m).Value);
            Assert.AreEqual("1.5", new Fix.Field(FIX_5_0SP2.Fields.Price, 1.5m).Value);
        });
    }

    [TestMethod]
    public void TestDecimalValueIsReadWithInvariantCulture()
    {
        WithCulture(CommaDecimalDotTimeCulture(), () =>
        {
            Assert.AreEqual(1.5m, (decimal?)new Fix.Field(44, "1.5"));
            Assert.AreEqual(1500m, (decimal?)new Fix.Field(44, "1500"));
        });
    }

    [TestMethod]
    public void TestTimestampStringUsesColonsRegardlessOfCulture()
    {
        WithCulture(CommaDecimalDotTimeCulture(), () =>
        {
            string timestamp = Fix.Field.TimestampString();
            Assert.IsTrue(Regex.IsMatch(timestamp, @"^\d{8}-\d{2}:\d{2}:\d{2}\.\d{3}$"), timestamp);
            timestamp = Fix.Field.TimestampString(Fix.Field.TimestampFormatShort);
            Assert.IsTrue(Regex.IsMatch(timestamp, @"^\d{8}-\d{2}:\d{2}:\d{2}$"), timestamp);
        });
    }

    // The original LINQ and string based implementations, kept to check the allocation free ones.
    static int ReferenceCheckSum(Fix.Field field)
    {
        int checksum = System.Text.Encoding.Latin1.GetBytes(field.Tag + "=").Select(b => (int)b).Sum();
        if (!string.IsNullOrEmpty(field.Value))
        {
            byte[] bytes = field.Data ? Convert.FromBase64String(field.Value) : System.Text.Encoding.Latin1.GetBytes(field.Value);
            checksum += bytes.Select(b => (int)b).Sum();
        }
        return checksum + 1;
    }

    static int ReferenceBodyLength(Fix.Field field)
    {
        int bodyLength = System.Text.Encoding.Latin1.GetBytes(field.Tag + "=").Length;
        if (!string.IsNullOrEmpty(field.Value))
        {
            bodyLength += field.Data ? Convert.FromBase64String(field.Value).Length : field.Value.Length;
        }
        return bodyLength + 1;
    }

    static IEnumerable<Fix.Field> ChecksumSampleFields()
    {
        yield return new Fix.Field(8, "FIX.4.4");
        yield return new Fix.Field(35, "D");
        yield return new Fix.Field(58, "");
        yield return new Fix.Field(58, "caf\u00e9 \u00ff");
        // Characters outside Latin-1 are sent as '?'.
        yield return new Fix.Field(58, "\u20ac\u4e2d");
        yield return new Fix.Field(58, new string('x', 1000));
        yield return new Fix.Field(58, new string('\u00e9', 300));
        yield return new Fix.Field(0, "zero");
        yield return new Fix.Field(int.MaxValue, "max");
        yield return new Fix.Field(-1, "negative");
        yield return new Fix.Field(96, Convert.ToBase64String(new byte[] { 0x00, 0x80, 0xFF, 0x01 })) { Data = true };
        yield return new Fix.Field(96, Convert.ToBase64String(Enumerable.Range(0, 2000).Select(i => (byte)i).ToArray())) { Data = true };
    }

    [TestMethod]
    public void TestComputeCheckSumMatchesReference()
    {
        foreach (var field in ChecksumSampleFields())
        {
            Assert.AreEqual(ReferenceCheckSum(field), field.ComputeCheckSum(), $"{field.Tag}={field.Value}");
        }
    }

    [TestMethod]
    public void TestComputeBodyLengthMatchesReference()
    {
        foreach (var field in ChecksumSampleFields())
        {
            Assert.AreEqual(ReferenceBodyLength(field), field.ComputeBodyLength(), $"{field.Tag}={field.Value}");
        }
    }

    [TestMethod]
    public void TestComputeCheckSumOfInvalidDataFieldThrows()
    {
        var field = new Fix.Field(96, "not base64!") { Data = true };
        Assert.Throws<FormatException>(() => field.ComputeCheckSum());
        Assert.Throws<FormatException>(() => field.ComputeBodyLength());
    }

    [TestMethod]
    public void TestInvalidBooleanValue()
    {
        Assert.Throws<FormatException>(() => (bool)new Fix.Field(141, "X"));
    }
}

