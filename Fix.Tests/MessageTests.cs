using Microsoft.VisualStudio.TestTools.UnitTesting;
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
}

