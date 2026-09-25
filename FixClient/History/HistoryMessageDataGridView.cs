/////////////////////////////////////////////////
//
// FIX Client
//
// Copyright @ 2021 VIRTU Financial Inc.
// All rights reserved.
//
// Filename: HistoryMessageDataGridView.cs
// Author:   Gary Hughes
//
/////////////////////////////////////////////////
using System.Drawing;

namespace FixClient;

public sealed partial class HistoryMessageDataGridView : DataGridView
{
    public HistoryMessageDataGridView()
    {
        InitializeComponent();

        EnableHeadersVisualStyles = false;
        ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        ColumnHeadersDefaultCellStyle.Font = new Font("Arial", 8, FontStyle.Bold);
        ColumnHeadersDefaultCellStyle.BackColor = LookAndFeel.Color.GridColumnHeader;
        ColumnHeadersDefaultCellStyle.ForeColor = Color.WhiteSmoke;
        ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
        ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        ColumnHeadersHeight -= 3;
        BackgroundColor = LookAndFeel.Color.GridCellBackground;
        BorderStyle = BorderStyle.None;
        SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        MultiSelect = false;
        RowHeadersVisible = false;
        DefaultCellStyle.WrapMode = DataGridViewTriState.False;
        RowTemplate.Resizable = DataGridViewTriState.False;
        GridColor = LookAndFeel.Color.Grid;
        AllowUserToAddRows = false;
        AllowUserToDeleteRows = false;
        DefaultCellStyle.Font = new Font("Arial", 8);
        ReadOnly = true;
        CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        DefaultCellStyle.BackColor = LookAndFeel.Color.GridCellBackground;
        DefaultCellStyle.Padding = new Padding(3, 0, 3, 0);
        DefaultCellStyle.SelectionBackColor = LookAndFeel.Color.GridCellSelectedBackground;
        DefaultCellStyle.SelectionForeColor = LookAndFeel.Color.GridCellSelectedForeground;
        DoubleBuffered = true;
        RowTemplate.Height -= 3;
        ShowCellToolTips = false;
        AutoGenerateColumns = false;

        DataGridViewColumn column = new DataGridViewTextBoxColumn
        {
            Name = MessageDataTable.ColumnSendingTime,
            AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells
        };

        Columns.Add(column);

        column = new DataGridViewTextBoxColumn
        {
            Name = MessageDataTable.ColumnMsgSeqNum,
            AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
            DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight }
        };

        Columns.Add(column);

        column = new DataGridViewImageColumn
        {
            Name = MessageDataTable.ColumnStatusImage,
            AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
            // Scale the 16px status images with the row height rather than drawing them at 96 DPI size.
            ImageLayout = DataGridViewImageCellLayout.Zoom,
            DefaultCellStyle = { NullValue = null },
            HeaderCell = new DataGridViewImageColumnHeaderCell
            {
                Image = CreateHeaderImage(Properties.Resources.MessageStatusInfo),
                Value = null
            },
        };

        Columns.Add(column);

        column = new DataGridViewTextBoxColumn
        {
            Name = MessageDataTable.ColumnMsgTypeDescription,
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
        };

        Columns.Add(column);
    }

    //
    // The status icon is a blue disc with a white "i" which disappears against the blue column
    // header, so invert it to a white disc with a header coloured "i" to match the header text.
    //
    static Bitmap CreateHeaderImage(Image source)
    {
        var bitmap = new Bitmap(source);

        for (int y = 0; y < bitmap.Height; ++y)
        {
            for (int x = 0; x < bitmap.Width; ++x)
            {
                Color pixel = bitmap.GetPixel(x, y);

                if (pixel.A == 0)
                {
                    continue;
                }

                Color colour = pixel.GetBrightness() > 0.8f ? LookAndFeel.Color.GridColumnHeader : Color.White;
                bitmap.SetPixel(x, y, Color.FromArgb(pixel.A, colour));
            }
        }

        return bitmap;
    }
}
