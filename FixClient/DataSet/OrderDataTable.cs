using System.Data;
using static Fix.Dictionary;

namespace FixClient;

class OrderDataTable : DataTable
{
    public const string ColumnClOrdId = "ClOrdID";
    public const string ColumnOrigClOrdId = "OrigClOrdID";
    public const string ColumnSide = "Side";
    public const string ColumnSideString = "SideString";
    public const string ColumnQuantity = "Quantity";
    public const string ColumnSymbol = "Symbol";
    public const string ColumnLimit = "Limit";
    public const string ColumnExDestination = "ExDestination";
    public const string ColumnTimeInForce = "TIF";
    public const string ColumnTimeInForceString = "TIFString";
    public const string ColumnOrdStatus = "Status";
    public const string ColumnOrdStatusString = "StatusString";
    public const string ColumnDone = "Done";
    public const string ColumnLeaves = "Leaves";
    public const string ColumnAvgPrice = "AvgPrice";
    public const string ColumnListId = "ListID";
    public const string ColumnText = "Text";
    public const string ColumnPendingQuantity = "PendingQuantity";
    public const string ColumnPendingLimit = "PendingLimit";

    public OrderDataTable(string name)
    : base(name)
    {
        var primaryKey = new List<DataColumn>();

        Columns.Add(ColumnSide, typeof(FieldValue));
        Columns.Add(ColumnSideString).ColumnMapping = MappingType.Hidden;
        Columns.Add(ColumnSymbol);
        Columns.Add(ColumnQuantity);
        Columns.Add(ColumnLimit);
        Columns.Add(ColumnExDestination);
        Columns.Add(ColumnTimeInForce, typeof(FieldValue));
        Columns.Add(ColumnTimeInForceString).ColumnMapping = MappingType.Hidden;
        Columns.Add(ColumnOrdStatus, typeof(FieldValue));
        Columns.Add(ColumnOrdStatusString).ColumnMapping = MappingType.Hidden;
        Columns.Add(ColumnDone);
        Columns.Add(ColumnLeaves);
        Columns.Add(ColumnAvgPrice);
        primaryKey.Add(Columns.Add(ColumnClOrdId));
        Columns.Add(ColumnOrigClOrdId);
        Columns.Add(ColumnListId);
        Columns.Add(ColumnText);
        Columns.Add(ColumnPendingQuantity).ColumnMapping = MappingType.Hidden;
        Columns.Add(ColumnPendingLimit).ColumnMapping = MappingType.Hidden;

        PrimaryKey = primaryKey.ToArray();
    }

    protected override Type GetRowType()
    {
        return typeof(OrderDataRow);
    }

    protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
    {
        return new OrderDataRow(builder);
    }
}

