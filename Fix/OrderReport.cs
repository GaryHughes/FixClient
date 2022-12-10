using System.IO;
using System.Text;

namespace Fix;

public class OrderReport
{
    public OrderReport(OrderBook orderBook)
    {
        OrderBook = orderBook;
    }

    public OrderBook OrderBook { get; }

    public char SeparatorCharacter { get; set; } = '-';

    public void Print(Stream stream)
    {
        var report = new Common.Report
        {
            SeparatorCharacter = SeparatorCharacter
        };

        report.AddColumn("Sender");
        report.AddColumn("Target");
        report.AddColumn("ClOrdID");
        report.AddColumn("Symbol");
        report.AddColumn("OrdStatus");
        report.AddColumn("Side");
        report.AddColumn("OrderQty", Common.Report.ColumnJustification.Right);
        report.AddColumn("Price", Common.Report.ColumnJustification.Right);
        report.AddColumn("CumQty", Common.Report.ColumnJustification.Right);
        report.AddColumn("AvgPx", Common.Report.ColumnJustification.Right);
        report.AddColumn("SendingTime", Common.Report.ColumnJustification.Right);

        foreach (var order in OrderBook.Orders)
        {
            report.AddRow(order.SenderCompID,
                            order.TargetCompID,
                            order.ClOrdID,
                            order.Symbol,
                            order.OrdStatus,
                            order.Side,
                            order.OrderQty,
                            order.Price,
                            order.CumQty,
                            order.AvgPx,
                            order.SendingTime.ToString(Field.TimestampFormatLong));
        }

        var bytes = Encoding.UTF8.GetBytes("\n" + report.ToString() + "\n\n");
        stream.Write(bytes, 0, bytes.Length);
    }
}
