using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using static Fix.Dictionary;

namespace FixTests;

[TestClass]
public class MessageTests
{
    [TestMethod]
    public void TestNoMsgType()
    {
        byte[] data = Encoding.ASCII.GetBytes("8=FIX.4.09=12749=ITGHK56=KODIAK_KGEHVWAP34=452=20090630-23:37:1294=033=158=RemotedHost#Name=gateQA-p01,Ip=10.132.3.125,Port=7081#10=128");
        var message = new Fix.Message(data);
        Assert.Throws<Fix.MissingFieldException>(() => message.MsgType);
    }

    [TestMethod]
    public void TestCheckSum()
    {
        byte[] data = Encoding.ASCII.GetBytes("8=FIX.4.09=006234=5835=A49=GLUE52=20040323-23:08:4856=KODIAK98=0108=6010=178");
        var message = new Fix.Message(data);
        Assert.AreEqual("178", message.ComputeCheckSum());
        Assert.AreEqual(message.CheckSum, message.ComputeCheckSum());
    }

    [TestMethod]
    public void TestCheckSumIsPaddedToThreeCharacters()
    {
        byte[] data = Encoding.ASCII.GetBytes("8=FIX.4.09=23635=UWO49=KODIAK56=server34=56650=kgehvwap97=N52=20090723-04:27:2066=068=111=kgehvwap.52.5215=AUD21=238=5640=244=39.59000047=A54=155=RIO.AX59=363=0100=ASX203=16000=6001=test6002=6005=ALT=41.587050=kgehvwap_test10=077");
        var message = new Fix.Message(data);
        Assert.AreEqual("077", message.ComputeCheckSum());
        Assert.AreEqual(message.CheckSum, message.ComputeCheckSum());
    }

    [TestMethod]
    public void TestBodyLength()
    {
        byte[] data = Encoding.ASCII.GetBytes("8=FIX.4.09=006234=5835=A49=GLUE52=20040323-23:08:4856=KODIAK98=0108=6010=178");
        var message = new Fix.Message(data);
        Assert.AreEqual("62", message.ComputeBodyLength());
    }

    [TestMethod]
    public void TestNameValuePairConstructor()
    {
        string[,] fields =
        {
            { "8", "FIX.4.0" },
            { "9", "127" },
            { "35", "C" },
            { "49", "ITGHK" },
            { "56", "KODIAK_KGEHVWAP" },
            { "34", "4" },
            { "52", "20090630-23:37:12" },
            { "94", "0" },
            { "33", "1" },
            { "58", "RemotedHost#Name=gateQA-p01,Ip=10.132.3.125,Port=7081#" },
            { "10", "128" }
        };
        var message = new Fix.Message(fields);
        Assert.AreEqual("ITGHK", message.Fields.Find(49)?.Value);
    }

    [TestMethod]
    public void TestMsgTypeIsSetFromDefinition()
    {
        var message = new Fix.Message(FIX_5_0SP2.Messages.NewOrderSingle);
        Assert.AreEqual("D", message.MsgType);
    }

    const string Soh = "\u0001";

    // Assembles a complete message with a correct BodyLength(9) and CheckSum(10) for the supplied body bytes.
    // Use "\u0001" (or Soh) rather than "\x01" in string literals; C# \x escapes consume up to four hex digits,
    // so "\x0135=8" is U+0135 followed by "=8", not SOH followed by "35=8".
    internal static byte[] BuildMessage(string beginString, byte[] body)
    {
        var bytes = new List<byte>();
        bytes.AddRange(Encoding.Latin1.GetBytes($"8={beginString}{Soh}9={body.Length}{Soh}"));
        bytes.AddRange(body);
        int checksum = bytes.Sum(b => b) % 256;
        bytes.AddRange(Encoding.Latin1.GetBytes($"10={checksum:D3}{Soh}"));
        return bytes.ToArray();
    }

    internal static byte[] BuildDataFieldBody(byte[] raw)
    {
        // RawDataLength(95) / RawData(96) - RawData is a data field so its value is opaque octets.
        var body = new List<byte>();
        body.AddRange(Encoding.Latin1.GetBytes($"35=B{Soh}148=Headline{Soh}95={raw.Length}{Soh}96="));
        body.AddRange(raw);
        body.AddRange(Encoding.Latin1.GetBytes(Soh));
        return body.ToArray();
    }

    // Enough bytes >= 0x80 that summing them as signed values drives the total negative.
    internal static readonly byte[] HighByteData = Enumerable.Repeat((byte)0x80, 32).Append((byte)0x01).Append((byte)0xFF).ToArray();

    [TestMethod]
    public void TestComputeCheckSumWithNonAsciiCharacters()
    {
        byte[] data = BuildMessage("FIXT.1.1", Encoding.Latin1.GetBytes($"35=8{Soh}58=\u00C7\u00C0\u00CE{Soh}"));
        var message = new Fix.Reader(new MemoryStream(data)).Read();
        Assert.IsNotNull(message);
        Assert.AreEqual("8", message.MsgType);
        Assert.AreEqual("\u00C7\u00C0\u00CE", message.Fields.Find(58)?.Value);
        Assert.AreEqual("12", message.ComputeBodyLength());
        Assert.AreEqual(message.CheckSum, message.ComputeCheckSum());
    }

    [TestMethod]
    public void TestComputeCheckSumWithHighBytesInDataField()
    {
        byte[] data = BuildMessage("FIX.4.4", BuildDataFieldBody(HighByteData));
        var message = new Fix.Reader(new MemoryStream(data)).Read();
        Assert.IsNotNull(message);
        var field = message.Fields.Find(96);
        Assert.IsNotNull(field);
        Assert.IsTrue(field.Data);
        CollectionAssert.AreEqual(HighByteData, System.Convert.FromBase64String(field.Value));
        Assert.AreEqual(message.CheckSum, message.ComputeCheckSum());
    }

    [TestMethod]
    public void TestPrettyPrintPreservesNonAsciiCharacters()
    {
        byte[] data = BuildMessage("FIX.4.4", Encoding.Latin1.GetBytes($"35=B{Soh}148=\u00C7\u00C0\u00CE{Soh}"));
        var message = new Fix.Reader(new MemoryStream(data)).Read();
        Assert.IsNotNull(message);
        string text = message.PrettyPrint();
        Assert.IsTrue(text.Contains("\u00C7\u00C0\u00CE"), text);
        Assert.IsFalse(text.Contains('\0'), "PrettyPrint output contains trailing NUL characters");
    }
}
