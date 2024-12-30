/////////////////////////////////////////////////
//
// FIX Client
//
// Copyright @ 2021 VIRTU Financial Inc.
// All rights reserved.
//
// Filename: GoaEditor.cs
// Author:   Gary Hughes
//
/////////////////////////////////////////////////
using System.ComponentModel;

namespace FixClient;

public partial class GoaEditor : Form
{
    public GoaEditor()
    {
        InitializeComponent();
        goaTextBox.KeyPress += GoaTextBoxKeyPress;
    }

    void GoaTextBoxKeyPress(object? sender, KeyPressEventArgs e)
    {
        if (e.KeyChar == '\x02')
        {
            goaTextBox.Paste("\x02");
            e.Handled = true;
        }
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public string Goa
    {
        get
        {
            string goa_value = goaTextBox.Text;
            if (ctrlBcheckbox.Checked)
            {
                goa_value = goa_value.Replace("^B", "\x02");
            }
            return goa_value;
        }
        set
        {
            goaTextBox.Text = value;
        }
    }
}
