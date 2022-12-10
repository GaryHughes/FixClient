/////////////////////////////////////////////////
//
// FIX Client
//
// Copyright @ 2021 VIRTU Financial Inc.
// All rights reserved.
//
// Filename: OrderDataGridView.cs
// Author:   Gary Hughes
//
/////////////////////////////////////////////////
using System.Data;
using System.Drawing;
using static Fix.Dictionary;

namespace FixClient;

public sealed partial class OrderDataGridView : DataGridView
{
    public OrderDataGridView()
    {
        InitializeComponent();

        EnableHeadersVisualStyles = false;
        ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        ColumnHeadersDefaultCellStyle.Font = new Font("Arial", 8, FontStyle.Bold);
        ColumnHeadersDefaultCellStyle.BackColor = LookAndFeel.Color.GridColumnHeader;
        ColumnHeadersDefaultCellStyle.ForeColor = Color.WhiteSmoke;
        ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
        ColumnHeadersHeight -= 3;
        MultiSelect = false;
        SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        AllowUserToAddRows = false;
        RowHeadersVisible = false;
        BorderStyle = BorderStyle.None;
        CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        AllowUserToDeleteRows = false;
        RowTemplate.Resizable = DataGridViewTriState.False;
        DefaultCellStyle.SelectionBackColor = LookAndFeel.Color.GridCellSelectedBackground;
        DefaultCellStyle.SelectionForeColor = LookAndFeel.Color.GridCellSelectedForeground;
        DefaultCellStyle.BackColor = LookAndFeel.Color.GridCellBackground;
        DefaultCellStyle.ForeColor = LookAndFeel.Color.GridCellForeground;
        BackgroundColor = LookAndFeel.Color.GridCellBackground;
        GridColor = LookAndFeel.Color.Grid;
        DefaultCellStyle.WrapMode = DataGridViewTriState.False;
        DefaultCellStyle.Font = new Font("Arial", 8);
        RowTemplate.Height -= 3;
        DoubleBuffered = true;
        ReadOnly = true;
    }

    public bool DisplayExDestination { get; set; }

    protected override void OnColumnHeaderMouseClick(DataGridViewCellMouseEventArgs e)
    {
        if (ModifierKeys == Keys.Control)
        {
            if (DataSource is DataView view)
            {
                view.Sort = string.Empty;
                Refresh();
                return;
            }
        }

        base.OnColumnHeaderMouseClick(e);
    }

    protected override void OnColumnAdded(DataGridViewColumnEventArgs e)
    {
        base.OnColumnAdded(e);

        DataGridViewColumn column = e.Column;

        switch (column.Name)
        {
            case OrderDataTable.ColumnSymbol:
                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                break;

            case OrderDataTable.ColumnExDestination:
                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                column.Visible = DisplayExDestination;
                break;

            case OrderDataTable.ColumnTimeInForce:
                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
                column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                break;

            case OrderDataTable.ColumnOrdStatus:
                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                break;

            case OrderDataTable.ColumnSide:
                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                break;

            case OrderDataTable.ColumnQuantity:
                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                break;

            case OrderDataTable.ColumnLimit:
                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                break;

            case OrderDataTable.ColumnAvgPrice:
                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                column.HeaderText = "Avg Price";
                break;

            case OrderDataTable.ColumnDone:
                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                break;

            case OrderDataTable.ColumnLeaves:
                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                break;

            case OrderDataTable.ColumnClOrdId:
                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                break;

            case OrderDataTable.ColumnOrigClOrdId:
                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                break;

            case OrderDataTable.ColumnListId:
                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                break;

            case OrderDataTable.ColumnText:
                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                break;
        }
    }

    protected override void OnCellFormatting(DataGridViewCellFormattingEventArgs e)
    {
        DataGridViewColumn column = Columns[e.ColumnIndex];

        if (Rows[e.RowIndex].DataBoundItem is not DataRowView rowView)
        {
            return;
        }

        DataRow row = rowView.Row;

        if (column.Name == OrderDataTable.ColumnQuantity)
        {
            object quantityValue = row[OrderDataTable.ColumnQuantity];
            object pendingQuantityValue = row[OrderDataTable.ColumnPendingQuantity];

            if (quantityValue != DBNull.Value && pendingQuantityValue != DBNull.Value)
            {
                int quantity = Convert.ToInt32(quantityValue);
                int pendingQuantity = Convert.ToInt32(pendingQuantityValue);
                e.Value = string.Format("{0} ({1})", quantity, pendingQuantity);
                e.FormattingApplied = true;
            }

            return;
        }

        if (column.Name == OrderDataTable.ColumnLimit)
        {
            object limitValue = row[OrderDataTable.ColumnLimit];
            object pendingLimitValue = row[OrderDataTable.ColumnPendingLimit];

            if (limitValue != DBNull.Value && pendingLimitValue != DBNull.Value)
            {
                decimal limit = Convert.ToDecimal(limitValue);
                decimal pendingLimit = Convert.ToDecimal(pendingLimitValue);
                e.Value = string.Format("{0} ({1})", limit, pendingLimit);
                e.FormattingApplied = true;
            }

            return;
        }

        if (column.Name == OrderDataTable.ColumnSide)
        {
            Color color = LookAndFeel.Color.GridCellForeground;

            if (e.Value == null || e.Value is DBNull)
            {
                e.Value = "Unknown";
                color = LookAndFeel.Color.Unknown;
            }
            else
            {
                var side = (FieldValue)e.Value;

                if (side == FIX_5_0SP2.Side.Buy ||
                    side == FIX_5_0SP2.Side.Cross)
                {
                    color = LookAndFeel.Color.Bid;
                }

                if (side == FIX_5_0SP2.Side.Sell)
                {
                    color = LookAndFeel.Color.Ask;
                }

                e.Value = side.ToString();
            }

            if (e.CellStyle is not null)
            {
                e.CellStyle.ForeColor = color;
                e.FormattingApplied = true;
            }

            return;
        }

        if (column.Name == OrderDataTable.ColumnOrdStatus)
        {
            Color color = LookAndFeel.Color.GridCellForeground;

            if (e.Value == null || e.Value is DBNull)
            {
                //
                // This is not an actual Fix state
                //
                e.Value = "Pending";
                color = LookAndFeel.Color.Pending;
            }
            else
            {
                var status = (FieldValue)e.Value;

                if (status == FIX_5_0SP2.OrdStatus.New ||
                    status == FIX_5_0SP2.OrdStatus.PartiallyFilled)
                {
                    color = LookAndFeel.Color.New;
                }

                if (status == FIX_5_0SP2.OrdStatus.PendingNew ||
                    status == FIX_5_0SP2.OrdStatus.PendingCancel ||
                    status == FIX_5_0SP2.OrdStatus.PendingReplace)
                {
                    color = LookAndFeel.Color.Pending;
                }

                if (status == FIX_5_0SP2.OrdStatus.Expired ||
                    status == FIX_5_0SP2.OrdStatus.Rejected ||
                    status == FIX_5_0SP2.OrdStatus.Stopped ||
                    status == FIX_5_0SP2.OrdStatus.Suspended)
                {
                    color = LookAndFeel.Color.Rejected;
                }

                e.Value = status.ToString();
            }

            if (e.CellStyle is not null)
            {
                e.CellStyle.ForeColor = color;
                e.FormattingApplied = true;
            }

            return;
        }

        if (column.Name == OrderDataTable.ColumnTimeInForce)
        {
            if (e.Value != null && e.Value != DBNull.Value)
            {
                var timeInForce = (FieldValue)e.Value;
                e.Value = ShortTimeInForceDescription(timeInForce);
                e.FormattingApplied = true;
                return;
            }
        }

        base.OnCellFormatting(e);
    }

    public static string ShortTimeInForceDescription(FieldValue timeInForce)
    {
        if (timeInForce == FIX_5_0SP2.TimeInForce.AtTheOpening) return "ATO";
        if (timeInForce == FIX_5_0SP2.TimeInForce.Day) return "DAY";
        if (timeInForce == FIX_5_0SP2.TimeInForce.FillOrKill) return "FOK";
        if (timeInForce == FIX_5_0SP2.TimeInForce.GoodTillCancel) return "GTC";
        if (timeInForce == FIX_5_0SP2.TimeInForce.GoodTillCrossing) return "GTX";
        if (timeInForce == FIX_5_0SP2.TimeInForce.GoodTillDate) return "GTD";
        if (timeInForce == FIX_5_0SP2.TimeInForce.ImmediateOrCancel) return "IOC";
        if (timeInForce == FIX_5_0SP2.TimeInForce.AtTheClose) return "ATC";
        return timeInForce.Name;
    }
}
