/////////////////////////////////////////////////
//
// FIX Client
//
// Copyright @ 2021 VIRTU Financial Inc.
// All rights reserved.
//
// Filename: CustomFieldForm.cs
// Author:   Gary Hughes
//
/////////////////////////////////////////////////
using System.ComponentModel;

namespace FixClient;

public partial class CustomFieldForm : Form
{
    CustomField _customField = new();
    readonly Fix.Dictionary.Version _version;
    readonly ErrorProvider _errorProvider;

    public CustomFieldForm(Fix.Dictionary.Version version, bool editing)
    {
        _version = version;

        InitializeComponent();

        valueTextBox.Validating += ValueTextBoxValidating;
        valueTextBox.Validated += ValueTextBoxValidated;

        nameTextBox.Validating += NameTextBoxValidating;
        nameTextBox.Validated += NameTextBoxValidated;

        _errorProvider = new ErrorProvider();
        ClearErrors();

        valueTextBox.Enabled = !editing;

        UpdateButtonState();
    }

    void ClearErrors()
    {
        _errorProvider.Clear();
        _errorProvider.SetIconPadding(valueTextBox, 3);
        _errorProvider.SetIconPadding(nameTextBox, 3);
    }

    void NameTextBoxValidated(object? sender, EventArgs e)
    {
        ClearErrors();
    }

    void NameTextBoxValidating(object? sender, CancelEventArgs e)
    {
        if (cancelButton.Focused)
            return;

        if (nameTextBox.Text.Trim() == string.Empty)
        {
            _errorProvider.SetError(nameTextBox, "You must enter a name for the field");
            e.Cancel = true;
            return;
        }

        Field.Name = nameTextBox.Text.Trim();
    }

    void ValueTextBoxValidated(object? sender, EventArgs e)
    {
        ClearErrors();
    }

    void ValueTextBoxValidating(object? sender, CancelEventArgs e)
    {
        if (cancelButton.Focused)
            return;

        if (valueTextBox.Text.Trim() == string.Empty)
        {
            _errorProvider.SetError(valueTextBox, "You must enter a numeric value for the field");
            e.Cancel = true;
            return;
        }

        int tag = Convert.ToInt32(valueTextBox.Text);

        if (Version.Fields.TryGetValue(tag, out var existing) && existing != null)
        {
            //
            // This is a standard field, we allow the user so set these as custom fields so they can be
            // added to messages which they are not usually used in. Populate the name with the standard.
            //
            nameTextBox.Text = existing.Name;
        }

        Field.Tag = tag;
    }

    Fix.Dictionary.Version Version
    {
        get { return _version; }
    }

    void UpdateButtonState()
    {
        deleteEnumButton.Enabled = enumGrid.Rows.Count != 0;
    }

    void NewEnumButtonClick(object? sender, EventArgs e)
    {
    }

    void DeleteEnumButtonClick(object? sender, EventArgs e)
    {
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public CustomField Field
    {
        get
        {
            return _customField;
        }
        set
        {
            _customField = value;
            valueTextBox.Text = value.Tag.ToString();
            nameTextBox.Text = value.Name;
        }
    }
}
