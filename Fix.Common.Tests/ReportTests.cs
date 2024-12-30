using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Fix.Common.Tests;

struct Order
{
    public string Sender;
    public string Side;
    public long OrderQty;
    public string Symbol;
    public decimal Price;
};

[TestClass]
public class ReportTests
{
    readonly Order[] Orders = new[] {
        new Order { Sender = "CLIENT", Side = "Buy", OrderQty = 200000, Symbol = "PANW", Price = 461.28m },
        new Order { Sender = "CLIENT", Side = "Sell", OrderQty = 400000, Symbol = "PANW", Price = 461.27m },
        new Order { Sender = "CLIENT", Side = "Buy", OrderQty = 500000, Symbol = "PANW", Price = 461.24m },
        new Order { Sender = "CLIENT", Side = "Buy", OrderQty = 350000, Symbol = "PANW", Price = 461.25m },
    };

    [TestMethod]
    public void TestReport()
    {
        const string ColumnSender = "Sender\nCompID";
        const string ColumnSide = "Side";
        const string ColumnOrderQty = "OrderQty";
        const string ColumnSymbol = "Symbol";
        const string ColumnPrice = "Price";

        var report = new Common.Report("Orders");

        report.AddColumn(ColumnSender);
        report.AddColumn(ColumnSide);
        report.AddColumn(ColumnOrderQty, Common.Report.ColumnJustification.Right);
        report.AddColumn(ColumnSymbol);
        report.AddColumn(ColumnPrice, Common.Report.ColumnJustification.Right);

        long totalQty = 0;

        foreach (var order in Orders)
        {
            report.AddRow(order.Sender, order.Side, order.OrderQty, order.Symbol, order.Price);
            totalQty += order.OrderQty;
        }

        report.SetFooter(null, null, totalQty, null, null);

        string expected =
@"                Orders
--------------------------------------
Sender                                
CompID  Side  OrderQty  Symbol   Price
--------------------------------------
CLIENT  Buy     200000  PANW    461.28
CLIENT  Sell    400000  PANW    461.27
CLIENT  Buy     500000  PANW    461.24
CLIENT  Buy     350000  PANW    461.25
--------------------------------------
               1450000                ";

        string actual = report.ToString();

        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    public void TestReportWithCustomSeparator()
    {
        const string ColumnSender = "Sender\nCompID";
        const string ColumnSide = "Side";
        const string ColumnOrderQty = "OrderQty";
        const string ColumnSymbol = "Symbol";
        const string ColumnPrice = "Price";

        var report = new Common.Report("Orders")
        {
            SeparatorCharacter = '\u2500'
        };

        report.AddColumn(ColumnSender);
        report.AddColumn(ColumnSide);
        report.AddColumn(ColumnOrderQty, Common.Report.ColumnJustification.Right);
        report.AddColumn(ColumnSymbol);
        report.AddColumn(ColumnPrice, Common.Report.ColumnJustification.Right);

        long totalQty = 0;

        foreach (var order in Orders)
        {
            report.AddRow(order.Sender, order.Side, order.OrderQty, order.Symbol, order.Price);
            totalQty += order.OrderQty;
        }

        report.SetFooter(null, null, totalQty, null, null);

        string expected =
@"                Orders
──────────────────────────────────────
Sender                                
CompID  Side  OrderQty  Symbol   Price
──────────────────────────────────────
CLIENT  Buy     200000  PANW    461.28
CLIENT  Sell    400000  PANW    461.27
CLIENT  Buy     500000  PANW    461.24
CLIENT  Buy     350000  PANW    461.25
──────────────────────────────────────
               1450000                ";

        string actual = report.ToString();

        Assert.AreEqual(expected, actual);
    }

}



