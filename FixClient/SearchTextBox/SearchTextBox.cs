/////////////////////////////////////////////////
//
// FIX Client
//
// Copyright @ 2021 VIRTU Financial Inc.
// All rights reserved.
//
// Filename: SearchTextBox.cs
// Author:   Gary Hughes
//
/////////////////////////////////////////////////
namespace FixClient;

class SearchTextBox : CueTextBox
{
    public SearchTextBox()
    {
        Cue = "Search...";
        BackColor = LookAndFeel.Color.GridCellBackground;
        Button.FlatStyle = FlatStyle.Flat;
        Button.FlatAppearance.MouseOverBackColor = BackColor;
        Button.FlatAppearance.MouseDownBackColor = BackColor;
        Button.FlatAppearance.BorderSize = 0;
        Button.ForeColor = System.Drawing.Color.Gray;
        Button.Text = "X";
        Button.Size = new System.Drawing.Size(16, 16);
        Button.Click += (sender, ev) =>
        {
            if (string.IsNullOrEmpty(Text))
            {
                OnTextChanged(new EventArgs());
            }
            else
            {
                Text = null;
            }
        };
    }
}

