using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text;
using static Fix.Dictionary;

namespace FixTests;

[TestClass]
public class ReaderTests
{
    #region Private Methods

    static void FieldsEqual(string[,] expected, Fix.Message message)
    {
        Assert.AreEqual(expected.Length / 2, message.Fields.Count, "Field count");

        for (int index = 0; index < message.Fields.Count; ++index)
        {
            Fix.Field actual = message.Fields[index];

            if (!int.TryParse(expected[index, 0], out int tag))
            {
                Assert.Fail($"Non numeric tag '{expected[index, 0]}={expected[index, 1]}'");
            }

            Assert.AreEqual(tag, actual.Tag, "Tag");
            Assert.AreEqual(expected[index, 1], actual.Value, "Value");
        }
    }

    #endregion

    [TestMethod]
    public void TestReadEmail()
    {
        string[,] expected =
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

        byte[] data = Encoding.ASCII.GetBytes("8=FIX.4.09=12735=C49=ITGHK56=KODIAK_KGEHVWAP34=452=20090630-23:37:1294=033=158=RemotedHost#Name=gateQA-p01,Ip=10.132.3.125,Port=7081#10=128");

        var message = new Fix.Message(data);

        FieldsEqual(expected, message);

        Assert.AreEqual(FIX_5_0SP2.Messages.Email.MsgType, message.MsgType);
    }

    /*
    [TestMethod]
    public void TestReadKodiakOrderWave()
    {
        string[,] expected =
        {
            { "8", "FIX.4.0" },
            { "9", "236" },
            { "35", "UWO" },
            { "49", "KODIAK" },
            { "56", "server" },
            { "34", "566" },
            { "50", "kgehvwap" },
            { "97", "N" },
            { "52", "20090723-04:27:20" },
            { "66", "0" },
            { "68", "1" },
            { "11", "kgehvwap.52.52" },
            { "15", "AUD" },
            { "21", "2" },
            { "38", "56" },
            { "40", "2" },
            { "44", "39.590000" },
            { "47", "A" },
            { "54", "1" },
            { "55", "RIO.AX" },
            { "59", "3" },
            { "63", "0" },
            { "100", "ASX" },
            { "203", "1" },
            { "6000", "" },
            { "6001", "test" },
            { "6002", "" },
            { "6005", "ALT=41.58" },
            { "7050", "kgehvwap_test" },
            { "10", "077" }
        };

        byte[] data = Encoding.ASCII.GetBytes("8=FIX.4.09=23635=UWO49=KODIAK56=server34=56650=kgehvwap97=N52=20090723-04:27:2066=068=111=kgehvwap.52.5215=AUD21=238=5640=244=39.59000047=A54=155=RIO.AX59=363=0100=ASX203=16000=6001=test6002=6005=ALT=41.587050=kgehvwap_test10=077");

        var message = new Fix.Message(data);

        FieldsEqual(expected, message);

        Assert.AreEqual(Fix.Dictionary.FIX_4_0.Messages.KodiakWaveOrder.MsgType, message.MsgType);
    }
    */

    /*
    [TestMethod]
    public void TestReadKodiakExecutionReport()
    {
        string[,] expected =
        {
            { "8", "FIX.4.0" },
            { "9", "206" },
            { "35", "8" },
            { "49", "GEHVWAP" },
            { "56", "HKGATE" },
            { "128", "test" },
            { "34", "220" },
            { "97", "N" },
            { "52", "20090723-04:27:20" },
            { "6", "39.590000" },
            { "11", "11" },
            { "14", "100" },
            { "17", "26" },
            { "20", "0" },
            { "30", "ASX" },
            { "31", "39.590000" },
            { "32", "56" },
            { "37", "11" },
            { "38", "100" },
            { "39", "2" },
            { "40", "1" },
            { "44", "0.000000" },
            { "54", "1" },
            { "55", "RIO.AX" },
            { "59", "0" },
            { "60", "20090723-04:27:20" },
            { "10", "130" }
        };

        byte[] data = Encoding.ASCII.GetBytes("8=FIX.4.09=20635=849=GEHVWAP56=HKGATE128=test34=22097=N52=20090723-04:27:206=39.59000011=1114=10017=2620=030=ASX31=39.59000032=5637=1138=10039=240=144=0.00000054=155=RIO.AX59=060=20090723-04:27:2010=130");

        var message = new Fix.Message(data);

        FieldsEqual(expected, message);

        //Assert.AreEqual(Fix.Dictionary.FIX_4_0.Messages.KodiakOrderWave, message.MsgType);
    }
    */

    [TestMethod]
    public void TestDataFieldWithNoPrecedingSize()
    {
        byte[] data = Encoding.ASCII.GetBytes("89=��#�e���E0LX��10=147");
        Assert.Throws<Exception>(() => new Fix.Message(data));
    }

    [TestMethod]
    public void TestDataFieldWithNonNumericPrevious()
    {
        byte[] data = Encoding.ASCII.GetBytes("8=FIX.4.09=13135=C49=KODIAK56=server34=750=kgehvwap52=20090724-07:20:4194=033=158=UserRegisterRequest#7,13,13#93=XX89=\xA1\xE6\x23\x0E\xC9\x65\x95\x98\x18\x9C\x45\x30\x4C\x58\xC1\xD5\x01\x31\x30=147");
        Assert.Throws<Exception>(() => new Fix.Message(data));
    }

    [TestMethod]
    public void TestDataFieldWithNoTrailingSeparator()
    {
        byte[] data = Encoding.ASCII.GetBytes("8=FIX.4.09=13135=C49=KODIAK56=server34=750=kgehvwap52=20090724-07:20:4194=033=158=UserRegisterRequest#7,13,13#93=1689=\xA1\xE6\x23\x0E\xC9\x65\x95\x98\x18\x9C\x45\x30\x4C\x58\xC1\xD5\x31\x30=147");
        Assert.Throws<Exception>(() => new Fix.Message(data));
    }

    public static byte[] StringToByteArray(string hex)
    {
        return Enumerable.Range(0, hex.Length)
                            .Where(x => x % 2 == 0)
                            .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                            .ToArray();
    }

    [TestMethod]
    public void TestDataField()
    {
        string signature = Convert.ToBase64String(StringToByteArray("A1E6230EC9659598189C45304C58C1D5"));

        string[,] expected =
        {
            {"8", "FIX.4.0"},
            {"9", "131"},
            {"35", "C"},
            {"49", "KODIAK"},
            {"56", "server"},
            {"34", "7"},
            {"50", "kgehvwap"},
            {"52", "20090724-07:20:41"},
            {"94", "0"},
            {"33", "1"},
            {"58", "UserRegisterRequest#7,13,13#"},
            {"93", "16"},
            {"89", signature},
            {"10", "147"}
        };

        byte[] a = Encoding.ASCII.GetBytes("8=FIX.4.09=13135=C49=KODIAK56=server34=750=kgehvwap52=20090724-07:20:4194=033=158=UserRegisterRequest#7,13,13#93=1689=");
        byte[] b = StringToByteArray("A1E6230EC9659598189C45304C58C1D5");
        byte[] c = Encoding.ASCII.GetBytes("10=147");
        byte[] data = a.Concat(b).Concat(c).ToArray();
        var message = new Fix.Message(data);
        FieldsEqual(expected, message);
        Assert.AreEqual(14, message.Fields.Count);
        Assert.AreEqual(131, message.BodyLength);
        Assert.AreEqual("131", message.ComputeBodyLength());
        Assert.AreEqual("147", message.CheckSum);
        Assert.AreEqual("147", message.ComputeCheckSum());

        var stream = new MemoryStream();
        using (Fix.Writer writer = new(stream, leaveOpen: true))
        {
            writer.Write(message);
        }

        byte[] serialised = stream.ToArray();

        Assert.IsTrue(StructuralComparisons.StructuralEqualityComparer.Equals(data, serialised));
    }

    [TestMethod]
    public void TestNoTrailingSeparator()
    {
        string[,] expected =
        {
            { "8", "FIX.4.0" },
            { "9", "286" },
            { "35", "8" },
            { "49", "ITGHK" },
            { "56", "KODIAK_KASFQA" },
            { "34", "1633" },
            { "57", "kasfqa" },
            { "52", "20091023-05:40:16" },
            { "37", "712-2" },
            { "17", "4" },
            { "20", "0" },
            { "39", "0" },
            { "55", "6497071" },
            { "54", "2" },
            { "38", "1000000" },
            { "32", "0" },
            { "31", "0.000000" },
            { "14", "0" },
            { "6", "0.000000" },
            { "11", "296.2.2" },
            { "40", "1" },
            { "60", "20091023-05:40:16" },
            { "59", "0" },
            { "47", "A" },
            { "30", "DMA" },
            { "15", "KRW" },
            { "6005", "ALT=18500TOT=1256276416" },
            { "111", "0" },
            { "9886", "0.000000" },
            { "9887", "0" },
            { "9912", "0" },
            { "9911", "0" },
            { "10", "135" }
        };

        byte[] data = Encoding.ASCII.GetBytes("8=FIX.4.09=28635=849=ITGHK56=KODIAK_KASFQA34=163357=kasfqa52=20091023-05:40:1637=712-217=420=039=055=649707154=238=100000032=031=0.00000014=06=0.00000011=296.2.240=160=20091023-05:40:1659=047=A30=DMA15=KRW6005=ALT=18500TOT=1256276416111=09886=0.0000009887=09912=09911=010=135");
        var message = new Fix.Message(data);
        FieldsEqual(expected, message);
    }

    [TestMethod]
    public void TestMessageReadEvent()
    {
        byte[] data = Encoding.ASCII.GetBytes("8=FIX.4.09=28635=849=ITGHK56=KODIAK_KASFQA34=163357=kasfqa52=20091023-05:40:1637=712-217=420=039=055=649707154=238=100000032=031=0.00000014=06=0.00000011=296.2.240=160=20091023-05:40:1659=047=A30=DMA15=KRW6005=ALT=18500TOT=1256276416111=09886=0.0000009887=09912=09911=010=135");
        using MemoryStream stream = new(data);
        using Fix.Reader reader = new(stream);
        int messagesRead = 0;
        reader.MessageRead += (sender, ev) => ++messagesRead;
        var message = new Fix.Message();
        reader.Read(message);
        Assert.AreEqual(1, messagesRead);
    }
}

