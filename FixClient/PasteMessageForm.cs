/////////////////////////////////////////////////
//
// FIX Client
//
// Copyright @ 2021 VIRTU Financial Inc.
// All rights reserved.
//
// Filename: PasteMessageForm.cs
// Author:   Gary Hughes
//
/////////////////////////////////////////////////
using System.ComponentModel;

namespace FixClient;

public partial class PasteMessageForm : Form
{
    public PasteMessageForm()
    {
        InitializeComponent();
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public bool FilterEmptyFields
    {
        get { return filterEmptyFieldsCheckBox.Checked; }
        set { filterEmptyFieldsCheckBox.Checked = value; }
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public bool DefineUnknownAsCustom
    {
        get { return defineUnknownAsCustomCheckBox.Checked; }
        set { defineUnknownAsCustomCheckBox.Checked = value; }
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public bool ResetExistingMessage
    {
        get { return resetMessageCheckBox.Checked; }
        set { resetMessageCheckBox.Checked = value; }
    }
}

