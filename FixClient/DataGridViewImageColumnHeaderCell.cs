/////////////////////////////////////////////////
//
// FIX Client
//
// Copyright @ 2021 VIRTU Financial Inc.
// All rights reserved.
//
// Filename: DataGridViewImageColumnHeaderCell.cs
// Author:   Gary Hughes
//
/////////////////////////////////////////////////
using System.ComponentModel;
using System.Drawing;
using System.Globalization;

namespace FixClient;

/// <summary>
/// Custom column header cell that can display an Image or Icon in addition to the typical Value.
/// </summary>
public class DataGridViewImageColumnHeaderCell : DataGridViewColumnHeaderCell
{
    Padding _imagePadding;
    bool _imageBeforeValue;
    Image? _image;
    Icon? _icon;

    /// <summary>
    /// Constructor that sets the default values
    /// </summary>
    public DataGridViewImageColumnHeaderCell()
    {
        _imageBeforeValue = true;
    }

    /// <summary>
    /// Represents the icon displayed by the cell
    /// </summary>
    [
        DefaultValue(null)
    ]
    public Icon? Icon
    {
        get
        {
            return _icon;
        }
        set
        {
            _icon = value;
            DataGridView?.UpdateCellValue(ColumnIndex, -1);
        }
    }

    /// <summary>
    /// Represents the image displayed by the cell
    /// </summary>
    [
        DefaultValue(null)
    ]
    public Image? Image
    {
        get
        {
            return _image;
        }
        set
        {
            _image = value;
            /*
            if (this.DataGridView != null)
            {
                this.DataGridView.UpdateCellValue(this.ColumnIndex, -1);
            }
            */
        }
    }

    /// <summary>
    /// Determines if the Image or Icon is displayed on the left of right of the Value.
    /// </summary>
    [
        DefaultValue(true)
    ]
    public bool ImageBeforeValue
    {
        get
        {
            return _imageBeforeValue;
        }
        set
        {
            if (ImageBeforeValue != value)
            {
                _imageBeforeValue = value;
                DataGridView?.InvalidateCell(this);
            }
        }
    }

    /// <summary>
    /// Defines a padding around the image or icon.
    /// </summary>
    public Padding ImagePadding
    {
        get
        {
            return _imagePadding;
        }
        set
        {
            if (ImagePadding != value)
            {
                if (value.Left < 0 || value.Right < 0 || value.Top < 0 || value.Bottom < 0)
                {
                    if (value.All != -1)
                    {
                        value.All = 0;
                    }
                    else
                    {
                        value.Left = Math.Max(0, value.Left);
                        value.Right = Math.Max(0, value.Right);
                        value.Top = Math.Max(0, value.Top);
                        value.Bottom = Math.Max(0, value.Bottom);
                    }
                }
                _imagePadding = value;
                /*
                if (this.DataGridView != null)
                {
                    this.DataGridView.UpdateCellValue(this.ColumnIndex, -1);
                }
                */
            }
        }
    }

    /// <summary>
    /// Custom Clone implementation that copies the cell specific properties.
    /// </summary>
    public override object Clone()
    {
        var dataGridViewCell = base.Clone() as DataGridViewImageColumnHeaderCell ?? throw new NullReferenceException("DataGridViewColumnHeaderCell.Clone() returned a null reference");

        dataGridViewCell.ImageBeforeValue = ImageBeforeValue;
        dataGridViewCell.ImagePadding = ImagePadding;
        dataGridViewCell.Image = Image;
        dataGridViewCell.Icon = Icon;

        return dataGridViewCell;
    }

    /// <summary>
    /// Utility function that adjusts the vertical padding of the cell to account 
    /// for the additional image or icon.
    /// </summary>
    Padding GetAdjustedCellPadding(DataGridViewCellStyle cellStyle)
    {
        if (_image != null)
        {
            if (_imageBeforeValue)
            {
                return new Padding(cellStyle.Padding.Left + _imagePadding.Horizontal + _image.Width, cellStyle.Padding.Top, cellStyle.Padding.Right, cellStyle.Padding.Bottom);
            }

            return new Padding(cellStyle.Padding.Left, cellStyle.Padding.Top, cellStyle.Padding.Right + _imagePadding.Horizontal + _image.Width, cellStyle.Padding.Bottom);
        }

        if (_icon != null)
        {
            if (_imageBeforeValue)
            {
                return new Padding(cellStyle.Padding.Left + _imagePadding.Horizontal + _icon.Width, cellStyle.Padding.Top, cellStyle.Padding.Right, cellStyle.Padding.Bottom);
            }

            return new Padding(cellStyle.Padding.Left, cellStyle.Padding.Top, cellStyle.Padding.Right + _imagePadding.Horizontal + _icon.Width, cellStyle.Padding.Bottom);
        }
        return cellStyle.Padding;
    }

    /// <summary>
    /// Custom implementation of GetContentBounds to ensure that the potential image or icon is not part
    /// of the content bounds.
    /// </summary>
    protected override Rectangle GetContentBounds(Graphics graphics, DataGridViewCellStyle cellStyle, int rowIndex)
    {
        if (_image != null || _icon != null)
        {
            ArgumentNullException.ThrowIfNull(cellStyle);
            cellStyle.Padding = GetAdjustedCellPadding(cellStyle);
        }

        return base.GetContentBounds(graphics, cellStyle, rowIndex);
    }

    /// <summary>
    /// Custom implementation of GetPreferredSize that take into account the potential
    /// image or icon and its padding.
    /// </summary>
    protected override Size GetPreferredSize(Graphics graphics, DataGridViewCellStyle cellStyle, int rowIndex, Size constraintSize)
    {
        ArgumentOutOfRangeException.ThrowIfNotEqual(rowIndex, -1);

        if (DataGridView == null)
        {
            return new Size(-1, -1);
        }

        Size basePreferredSize = base.GetPreferredSize(graphics, cellStyle, rowIndex, constraintSize);

        if (constraintSize.Width == 0)
        {
            if (_image != null)
            {
                basePreferredSize.Width += _image.Width + ImagePadding.Horizontal;
            }
            else if (_icon != null)
            {
                basePreferredSize.Width += _icon.Width + ImagePadding.Horizontal;
            }
        }

        if (constraintSize.Height == 0)
        {
            var dgvabsPlaceholder = new DataGridViewAdvancedBorderStyle();
            DataGridViewAdvancedBorderStyle dgvabsEffective = DataGridView.AdjustColumnHeaderBorderStyle(DataGridView.AdvancedColumnHeadersBorderStyle,
                                                                                                            dgvabsPlaceholder,
                                                                                                            false /*isFirstDisplayedColumn*/,
                                                                                                            false /*isLastVisibleColumn*/);
            Rectangle borderWidthsRect = BorderWidths(dgvabsEffective);
            int borderAndPaddingHeights = borderWidthsRect.Top + borderWidthsRect.Height + cellStyle.Padding.Vertical;
            if (_image != null)
            {
                basePreferredSize.Height = Math.Max(basePreferredSize.Height, _image.Height + borderAndPaddingHeights + ImagePadding.Vertical);
            }
            else if (_icon != null)
            {
                basePreferredSize.Height = Math.Max(basePreferredSize.Height, _icon.Height + borderAndPaddingHeights + ImagePadding.Vertical);
            }
        }

        return basePreferredSize;
    }

    /// <summary>
    /// Custom painting implementation that paints the image or icon on top of the base implementation.
    /// </summary>
    protected override void Paint(Graphics graphics,
        Rectangle clipBounds,
        Rectangle cellBounds,
        int rowIndex,
        DataGridViewElementStates dataGridViewElementState,
        object? value,
        object? formattedValue,
        string? errorText,
        DataGridViewCellStyle cellStyle,
        DataGridViewAdvancedBorderStyle advancedBorderStyle,
        DataGridViewPaintParts paintParts)
    {
        ArgumentNullException.ThrowIfNull(cellStyle);

        Padding cellStylePadding = cellStyle.Padding;
        int imageHeight = 0, imageWidth = 0;

        if (_image != null || _icon != null)
        {
            if (_image != null)
            {
                imageHeight = _image.Height;
                imageWidth = _image.Width;
            }
            else if (_icon != null)
            {
                imageHeight = _icon.Height;
                imageWidth = _icon.Width;
            }
            cellStyle.Padding = GetAdjustedCellPadding(cellStyle);
        }

        base.Paint(graphics,
                    clipBounds,
                    cellBounds,
                    rowIndex,
                    dataGridViewElementState,
                    value,
                    formattedValue,
                    errorText,
                    cellStyle,
                    advancedBorderStyle,
                    paintParts);

        if (_image != null || _icon != null)
        {
            Rectangle valBounds = cellBounds;
            Rectangle borderWidths = BorderWidths(advancedBorderStyle);
            valBounds.Offset(borderWidths.X, borderWidths.Y);
            valBounds.Width -= borderWidths.Right;
            valBounds.Height -= borderWidths.Bottom;

            if (DataGridView is null)
            {
                return;
            }

            if (valBounds.Width > 0 && valBounds.Height > 0)
            {
                if (_imageBeforeValue)
                {
                    if (DataGridView.RightToLeft == RightToLeft.Yes)
                    {
                        valBounds.Offset(Math.Max(cellStylePadding.Right + ImagePadding.Right, valBounds.Width - cellStylePadding.Left - ImagePadding.Left - imageWidth), cellStylePadding.Top + ImagePadding.Top);
                    }
                    else
                    {
                        valBounds.Offset(cellStylePadding.Left + ImagePadding.Left, cellStylePadding.Top + ImagePadding.Top);
                    }
                }
                else
                {
                    if (DataGridView.RightToLeft == RightToLeft.Yes)
                    {
                        valBounds.Offset(cellStylePadding.Right + ImagePadding.Right, cellStylePadding.Top + ImagePadding.Top);
                    }
                    else
                    {
                        valBounds.Offset(Math.Max(cellStylePadding.Left + ImagePadding.Left, valBounds.Width - cellStylePadding.Right - ImagePadding.Right - imageWidth), cellStylePadding.Top + ImagePadding.Top);
                    }
                }
                valBounds.Width -= cellStylePadding.Horizontal + ImagePadding.Horizontal;
                valBounds.Height -= cellStylePadding.Vertical + ImagePadding.Vertical;
                if (valBounds.Width > 0 && valBounds.Height > 0)
                {
                    switch (cellStyle.Alignment)
                    {
                        case DataGridViewContentAlignment.MiddleCenter:
                        case DataGridViewContentAlignment.MiddleLeft:
                        case DataGridViewContentAlignment.MiddleRight:
                            valBounds.Y += Math.Max(0, (valBounds.Height - imageHeight) / 2);
                            break;
                        case DataGridViewContentAlignment.BottomCenter:
                        case DataGridViewContentAlignment.BottomLeft:
                        case DataGridViewContentAlignment.BottomRight:
                            valBounds.Y += Math.Max(0, valBounds.Height - imageHeight);
                            break;
                    }
                    Region reg = graphics.Clip;
                    graphics.SetClip(Rectangle.Intersect(valBounds, Rectangle.Truncate(graphics.VisibleClipBounds)));
                    try
                    {
                        if (_image != null)
                        {
                            var imageBounds = new Rectangle(valBounds.Location, _image.Size);
                            graphics.DrawImage(_image, imageBounds);
                        }
                        else if (_icon != null)
                        {
                            var iconBounds = new Rectangle(valBounds.Location, _icon.Size);
                            graphics.DrawIconUnstretched(_icon, iconBounds);
                        }
                    }
                    finally
                    {
                        graphics.Clip = reg;
                    }
                }
            }
            cellStyle.Padding = cellStylePadding;
        }
    }

    /// <summary>
    /// Custom string representation of this custom cell type.
    /// </summary>
    public override string ToString()
    {
        return "DataGridViewImageColumnHeaderCell { ColumnIndex=" + ColumnIndex.ToString(CultureInfo.CurrentCulture) + " }";
    }
}

