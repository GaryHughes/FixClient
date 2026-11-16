using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FixClientTests;

[TestClass]
public class ClOrdIdTests
{
    [TestMethod]
    public void TestClOrdIdDateUsesTheGregorianCalendarInAnyCulture()
    {
        using var session = new FixClient.Session(new System.Windows.Forms.Control())
        {
            AppendDateToClOrdID = true
        };

        //
        // Thai uses the Buddhist calendar, which is 543 years ahead of the Gregorian one.
        //
        CultureInfo original = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = new CultureInfo("th-TH");
        try
        {
            string today = DateTime.Today.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
            Assert.AreEqual($"5-{today}", session.FormatClOrdId(5));
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
        }
    }
}
