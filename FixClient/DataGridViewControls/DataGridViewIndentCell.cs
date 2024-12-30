/////////////////////////////////////////////////
//
// FIX Client
//
// Copyright @ 2021 VIRTU Financial Inc.
// All rights reserved.
//
// Filename: DataGridViewIndentCell.cs
// Author:   Gary Hughes
//
/////////////////////////////////////////////////
using System.Drawing;

namespace FixClient;

public class DataGridViewIndentCell : DataGridViewCell
{
    static readonly Brush DefaultBrush = new SolidBrush(LookAndFeel.Color.GridCellBackground);

    static readonly Brush[] Brushes =
    [
        new SolidBrush(LookAndFeel.Color.GridCellBackground),
        new SolidBrush(Color.FromArgb(226, 226, 226)),
        new SolidBrush(Color.FromArgb(204, 204, 204)),
        new SolidBrush(Color.FromArgb(152, 152, 152)),
        new SolidBrush(Color.FromArgb(120, 120, 120)),
        new SolidBrush(Color.Black)
    ];

    protected override void Paint(Graphics graphics, Rectangle clipBounds, Rectangle cellBounds, int rowIndex,
                                    DataGridViewElementStates cellState, object? value, object? formattedValue, string? errorText,
                                    DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle,
                                    DataGridViewPaintParts paintParts)
    {
        if (value is not int)
        {
            throw new ArgumentException("Value is not an Int");
        }

        var indent = (int)value;

        if (indent > Brushes.Length - 1 || indent < 0)
        {
            indent = 0;
        }

        Rectangle rect = cellBounds;
        graphics.FillRectangle(DefaultBrush, rect);
        Brush brush = Brushes[indent];
        rect.Inflate(-(rect.Width / 3) + 1, 0);
        graphics.FillRectangle(brush, rect);
    }
}
