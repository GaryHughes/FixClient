/////////////////////////////////////////////////
//
// FIX Client
//
// Copyright @ 2021 VIRTU Financial Inc.
// All rights reserved.
//
// Filename: FieldDataGridView.cs
// Author:   Gary Hughes
//
/////////////////////////////////////////////////
using System.Data;
using System.Drawing;

namespace FixClient;

public partial class FieldDataGridView : DataGridView
{
    public FieldDataGridView()
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
        BackgroundColor = LookAndFeel.Color.GridCellBackground;
        BorderStyle = BorderStyle.None;
        SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        MultiSelect = false;
        RowHeadersVisible = false;
        DefaultCellStyle.WrapMode = DataGridViewTriState.False;
        RowTemplate.Resizable = DataGridViewTriState.False;
        AllowUserToAddRows = false;
        AllowUserToDeleteRows = false;
        ReadOnly = true;
        CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        GridColor = LookAndFeel.Color.Grid;
        DefaultCellStyle.BackColor = LookAndFeel.Color.GridCellBackground;
        DefaultCellStyle.Padding = new Padding(3, 0, 3, 0);
        DefaultCellStyle.SelectionBackColor = LookAndFeel.Color.GridCellSelectedBackground;
        DefaultCellStyle.SelectionForeColor = LookAndFeel.Color.GridCellSelectedForeground;
        DefaultCellStyle.Font = new Font("Arial", 8);
        RowTemplate.Height -= 3;
        EnableHeadersVisualStyles = false;

        ColumnHeaderMouseClick += HistoryFieldDataGridViewColumnHeaderMouseClick;
    }

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
        switch (e.Column.Name)
        {
            case FieldDataTable.ColumnIndent:
                e.Column.Width = 20;
                e.Column.HeaderText = string.Empty;
                e.Column.ReadOnly = true;
                break;

            case FieldDataTable.ColumnTag:
            case FieldDataTable.ColumnName:
                e.Column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                e.Column.SortMode = DataGridViewColumnSortMode.Programmatic;
                break;

            case FieldDataTable.ColumnValue:
                e.Column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                e.Column.SortMode = DataGridViewColumnSortMode.Programmatic;
                e.Column.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
                break;

            case FieldDataTable.ColumnDescription:
                e.Column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                e.Column.SortMode = DataGridViewColumnSortMode.Programmatic;
                break;
        }

        base.OnColumnAdded(e);
    }

    protected override void OnCellFormatting(DataGridViewCellFormattingEventArgs e)
    {
        try
        {
            if (Rows[e.RowIndex].DataBoundItem is not DataRowView rowView)
            {
                return;
            }

            if (rowView.Row is not FieldDataRow row)
            {
                return;
            }

            if (e.CellStyle is not null)
            {
                var custom = (bool)row[FieldDataTable.ColumnCustom];
                var required = (bool)row[FieldDataTable.ColumnRequired];

                if (custom)
                {
                    e.CellStyle.ForeColor = LookAndFeel.Color.Custom;
                }
                else
                {
                    e.CellStyle.ForeColor = required ? LookAndFeel.Color.Incoming : LookAndFeel.Color.Outgoing;
                }
            }
        }
        finally
        {
            base.OnCellFormatting(e);
        }
    }

    void HistoryFieldDataGridViewColumnHeaderMouseClick(object? sender, DataGridViewCellMouseEventArgs e)
    {
        DataGridViewColumn column = Columns[e.ColumnIndex];
        DataGridViewColumnHeaderCell header = column.HeaderCell;

        if (DataSource is not DataView view)
        {
            return;
        }
        //
        // Apply the default sort for the column data type.
        //
        if (header.SortGlyphDirection == SortOrder.Ascending)
        {
            header.SortGlyphDirection = SortOrder.Descending;
            view.Sort = string.Format("{0} DESC", column.Name);
        }
        else
        {
            header.SortGlyphDirection = SortOrder.Ascending;
            view.Sort = string.Format("{0} ASC", column.Name);
        }

        RefreshEdit();
    }
}
