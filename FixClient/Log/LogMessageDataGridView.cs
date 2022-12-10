/////////////////////////////////////////////////
//
// FIX Client
//
// Copyright @ 2021 VIRTU Financial Inc.
// All rights reserved.
//
// Filename: LogMessageDataGridView.cs
// Author:   Gary Hughes
//
/////////////////////////////////////////////////
using System.Data;
using System.Drawing;

namespace FixClient;

public sealed partial class LogMessageDataGridView : DataGridView
{
    public LogMessageDataGridView()
    {
        InitializeComponent();

        EnableHeadersVisualStyles = false;
        ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        ColumnHeadersDefaultCellStyle.Font = new Font("Arial", 8, FontStyle.Bold);
        ColumnHeadersDefaultCellStyle.BackColor = LookAndFeel.Color.GridColumnHeader;
        ColumnHeadersDefaultCellStyle.ForeColor = Color.WhiteSmoke;
        ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
        MultiSelect = false;
        SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        AllowUserToAddRows = false;
        RowHeadersVisible = false;
        BorderStyle = BorderStyle.None;
        CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        GridColor = LookAndFeel.Color.Grid;
        AllowUserToDeleteRows = false;
        RowTemplate.Resizable = DataGridViewTriState.False;
        EnableHeadersVisualStyles = false;
        ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        ColumnHeadersHeight -= 3;
        BackgroundColor = LookAndFeel.Color.GridCellBackground;
        DefaultCellStyle.SelectionBackColor = LookAndFeel.Color.GridCellSelectedBackground;
        DefaultCellStyle.BackColor = LookAndFeel.Color.GridCellSelectedForeground;
        DefaultCellStyle.WrapMode = DataGridViewTriState.False;
        DefaultCellStyle.Font = new Font("Arial", 8);
        RowTemplate.Height -= 5;
        DoubleBuffered = true;
        ReadOnly = true;

        Padding padding = DefaultCellStyle.Padding;

        padding.Left += 5;
        padding.Right += 5;

        DefaultCellStyle.Padding = padding;
    }

    protected override void OnCellFormatting(DataGridViewCellFormattingEventArgs e)
    {
        try
        {
            DataGridViewColumn column = Columns[e.ColumnIndex];

            if (column.Name == LogMessageDataTable.ColumnMessage)
            {
                DataGridViewRow gridViewRow = Rows[e.RowIndex];

                if (gridViewRow.IsNewRow)
                    return;

                if (gridViewRow.DataBoundItem is DataRowView rowView)
                {
                    DataRow row = rowView.Row;

                    object levelValue = row[LogMessageDataTable.ColumnLevel];
                    LogLevel level = LogLevel.Info;

                    if (levelValue != null && levelValue != DBNull.Value)
                    {
                        level = (LogLevel)levelValue;
                    }

                    if (e.CellStyle is not null)
                    {
                        if (level == LogLevel.Warn)
                        {
                            e.CellStyle.BackColor = LookAndFeel.Color.LogWarningBackColor;
                            e.CellStyle.ForeColor = LookAndFeel.Color.LogWarningForeColor;
                        }
                        else if (level == LogLevel.Error)
                        {
                            e.CellStyle.BackColor = LookAndFeel.Color.LogErrorBackColor;
                            e.CellStyle.ForeColor = LookAndFeel.Color.LogErrorForeColor;
                        }
                    }
                }
            }
        }
        finally
        {
            base.OnCellFormatting(e);
        }
    }

    protected override void OnColumnAdded(DataGridViewColumnEventArgs e)
    {
        base.OnColumnAdded(e);

        DataGridViewColumn column = e.Column;

        switch (column.Name)
        {
            case LogMessageDataTable.ColumnTimestamp:
                column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
                column.DefaultCellStyle.Format = "HH:mm:ss.fff";
                column.SortMode = DataGridViewColumnSortMode.NotSortable;
                break;

            case LogMessageDataTable.ColumnMessage:
                column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                column.SortMode = DataGridViewColumnSortMode.NotSortable;
                break;
        }
    }
}

