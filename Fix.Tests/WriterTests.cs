using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Text;

namespace FixTests;

[TestClass]
public class WriterTests
{
    [TestMethod]
    public void TestWriteEmail()
    {
        const string expected = "8=FIX.4.09=12735=C49=ITGHK56=KODIAK_KGEHVWAP34=452=20090630-23:37:1294=033=158=RemotedHost#Name=gateQA-p01,Ip=10.132.3.125,Port=7081#10=128";
        var message = new Fix.Message(new Fix.FieldCollection
        {
            {8, "FIX.4.0"},
            //{9, "127"},
            {35, "C"},
            {49, "ITGHK"},
            {56, "KODIAK_KGEHVWAP"},
            {34, "4"},
            {52, "20090630-23:37:12"},
            {94, "0"},
            {33, "1"},
            {58, "RemotedHost#Name=gateQA-p01,Ip=10.132.3.125,Port=7081#"},
        });
        var stream = new MemoryStream();
        using (Fix.Writer writer = new(stream, leaveOpen: true))
        {
            writer.Write(message);
        }
        stream.Close();
        Assert.AreEqual(expected, Encoding.ASCII.GetString(stream.ToArray()));
    }

    [TestMethod]
    public void TestMessageWrittenEvent()
    {
        byte[] data = Encoding.ASCII.GetBytes("8=FIX.4.09=28635=849=ITGHK56=KODIAK_KASFQA34=163357=kasfqa52=20091023-05:40:1637=712-217=420=039=055=649707154=238=100000032=031=0.00000014=06=0.00000011=296.2.240=160=20091023-05:40:1659=047=A30=DMA15=KRW6005=ALT=18500TOT=1256276416111=09886=0.0000009887=09912=09911=010=135");
        using MemoryStream stream = new();
        using Fix.Writer writer = new(stream, leaveOpen: true);
        int messagesWritten = 0;
        writer.MessageWritten += (sender, ev) => ++messagesWritten;
        var message = new Fix.Message(data);
        writer.Write(message);
        Assert.AreEqual(1, messagesWritten);
    }

    [TestMethod]
    public void TestMessageWritingEvent()
    {
        byte[] data = Encoding.ASCII.GetBytes("8=FIX.4.09=28635=849=ITGHK56=KODIAK_KASFQA34=163357=kasfqa52=20091023-05:40:1637=712-217=420=039=055=649707154=238=100000032=031=0.00000014=06=0.00000011=296.2.240=160=20091023-05:40:1659=047=A30=DMA15=KRW6005=ALT=18500TOT=1256276416111=09886=0.0000009887=09912=09911=010=135");
        using MemoryStream stream = new();
        using Fix.Writer writer = new(stream, leaveOpen: true);
        int messagesWriting = 0;
        writer.MessageWriting += (sender, ev) => ++messagesWriting;
        var message = new Fix.Message(data);
        writer.Write(message);
        Assert.AreEqual(1, messagesWriting);
    }

    [TestMethod]
    public void TestMessageWritingWrittenEvents()
    {
        byte[] data = Encoding.ASCII.GetBytes("8=FIX.4.09=28635=849=ITGHK56=KODIAK_KASFQA34=163357=kasfqa52=20091023-05:40:1637=712-217=420=039=055=649707154=238=100000032=031=0.00000014=06=0.00000011=296.2.240=160=20091023-05:40:1659=047=A30=DMA15=KRW6005=ALT=18500TOT=1256276416111=09886=0.0000009887=09912=09911=010=135");
        using MemoryStream stream = new();
        using Fix.Writer writer = new(stream, leaveOpen: true);
        int messagesWriting = 0;
        int messagesWritten = 0;
        writer.MessageWriting += (sender, ev) => ++messagesWriting;
        writer.MessageWritten += (sender, ev) => ++messagesWritten;
        var message = new Fix.Message(data);
        writer.Write(message);
        Assert.AreEqual(1, messagesWriting);
        Assert.AreEqual(1, messagesWritten);
    }
}

