/////////////////////////////////////////////////
//
// FIX Client
//
// Copyright @ 2021 VIRTU Financial Inc.
// All rights reserved.
//
// Filename: ResetForm.cs
// Author:   Gary Hughes
//
/////////////////////////////////////////////////
using System.Drawing;

namespace FixClient;

public partial class ResetForm : Form
{
    public ResetForm()
    {
        InitializeComponent();
        // Use a stock icon rendered at the scaled size rather than stretching the 32px SystemIcons bitmap.
        iconPictureBox.Image = SystemIcons.GetStockIcon(StockIconId.Help, iconPictureBox.Width).ToBitmap();
    }

    public Fix.OrderBook.Retain Retain
    {
        get
        {
            Fix.OrderBook.Retain retain = Fix.OrderBook.Retain.None;

            if (retainActiveGtcOrdersCheckBox.Checked)
            {
                retain |= Fix.OrderBook.Retain.ActiveGTC;
            }

            if (retainActiveGtdOrdersCheckBox.Checked)
            {
                retain |= Fix.OrderBook.Retain.ActiveGTD;
            }

            return retain;
        }
    }

    public bool ResetGeneratedIds
    {
        get { return resetGeneratedIdsCheckBox.Checked; }
    }
}

