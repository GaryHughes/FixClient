using System.Globalization;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Fix.Dictionary;

namespace FixTests;

[TestClass]
public class OrderReportTests
{
    [TestMethod]
    public void TestSendingTimeIsAFixTimestampInAnyCulture()
    {
        var message = new Fix.Message { MsgType = FIX_5_0SP2.Messages.NewOrderSingle.MsgType };
        message.Fields.Set(FIX_5_0SP2.Fields.SenderCompID, "SENDER");
        message.Fields.Set(FIX_5_0SP2.Fields.TargetCompID, "TARGET");
        message.Fields.Set(FIX_5_0SP2.Fields.SendingTime, "20261116-10:15:03.123");
        message.Fields.Set(FIX_5_0SP2.Fields.ClOrdID, "1");
        message.Fields.Set(FIX_5_0SP2.Fields.Symbol, "BHP");
        message.Fields.Set(FIX_5_0SP2.Fields.OrderQty, 5000);

        var book = new Fix.OrderBook();
        Assert.AreEqual(Fix.OrderBookMessageEffect.Modified, book.Process(message));

        //
        // Finnish, for one, separates the parts of a time with a full stop.
        //
        var culture = (CultureInfo)CultureInfo.InvariantCulture.Clone();
        culture.DateTimeFormat.TimeSeparator = ".";

        CultureInfo original = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = culture;
        try
        {
            using var stream = new MemoryStream();
            new Fix.OrderReport(book).Print(stream);
            string report = Encoding.UTF8.GetString(stream.ToArray());
            Assert.Contains("20261116-10:15:03.123", report);
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
        }
    }
}
