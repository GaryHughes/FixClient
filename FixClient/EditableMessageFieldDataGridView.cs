/////////////////////////////////////////////////
//
// FIX Client
//
// Copyright @ 2021 VIRTU Financial Inc.
// All rights reserved.
//
// Filename: EditableMessageFieldDataGridView.cs
// Author:   Gary Hughes
//
/////////////////////////////////////////////////
using System.ComponentModel;
using System.Data;
using System.Drawing;
using static Fix.Dictionary;

namespace FixClient;

public partial class EditableMessageFieldDataGridView : MessageFieldDataGridView
{
    public delegate void FieldValueHandler(object sender);

    public event FieldValueHandler? FieldValueChanged;

    void OnFieldValueChanged()
    {
        FieldValueChanged?.Invoke(this);
    }


    readonly ToolTip _toolTip = new();

    public EditableMessageFieldDataGridView()
    {
        InitializeComponent();
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Fix.Message? Message { get; set; }

    protected override void OnDataError(bool displayErrorDialogIfNoHandler, DataGridViewDataErrorEventArgs e)
    {
        //
        // This fires if we enter an invalid value for an enumerated field. Suppress it so we don't get an
        // error dialog.
        //
    }

    protected override void OnEditingControlShowing(DataGridViewEditingControlShowingEventArgs e)
    {
        if (e.Control is not ComboBox comboBox)
        {
            return;
        }

        comboBox.DrawMode = DrawMode.OwnerDrawFixed;
        comboBox.DrawItem -= ComboBoxDrawItem;
        comboBox.DrawItem += ComboBoxDrawItem;
        comboBox.DropDownClosed -= ComboBoxDropDownClosed;
        comboBox.DropDownClosed += ComboBoxDropDownClosed;
    }

    void ComboBoxDropDownClosed(object? sender, EventArgs e)
    {
        if (sender is ComboBox comboBox)
        {
            _toolTip.Hide(comboBox);
        }
    }

    void ComboBoxDrawItem(object? sender, DrawItemEventArgs e)
    {
        if (e.Index < 0)
            return;

        if (sender is not ComboBox comboBox)
            return;

        if (comboBox.GetItemText(comboBox.Items[e.Index]) is string text)
        {
            e.DrawBackground();

            using (SolidBrush br = new(e.ForeColor))
            {
                if (e.Font is Font font)
                {
                    e.Graphics.DrawString(text, font, br, e.Bounds);
                }
            }

            if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
            {
                if (comboBox.Items[e.Index] is not EnumDescription enumDescription)
                {
                    return;
                }

                var tooltipText = $"{enumDescription.Value} - {enumDescription.Description}".SplitInParts(100);

                Point position = comboBox.PointToClient(Cursor.Position);
                position.Y += 40;
                _toolTip.Show(tooltipText, comboBox, position);
            }
            else
            {
                _toolTip.Hide(comboBox);
            }

            e.DrawFocusRectangle();
        }
    }

    static VersionField? FindFieldDefinition(int tag)
    {
        // This is the most likley hit for modern users so lets be optimistic which will be faster.
        if (FIX_5_0SP2.Fields[tag] is VersionField definition && definition.IsValid)
        {
            return definition;
        }

        // Fall back to the rest.
        foreach (var version in Versions)
        {
            if (version.BeginString == Versions.FIX_5_0SP2.BeginString)
            {
                continue;
            }

            if (version.Fields.TryGetValue(tag, out var fallbackDefinition) && fallbackDefinition.IsValid)
            {
                return fallbackDefinition;
            }
        }

        return null;
    }

    protected override void OnCellBeginEdit(DataGridViewCellCancelEventArgs e)
    {
        //
        // Prevent the combobox from showing in the description column if this field
        // is not an enumeration.
        //
        if (Columns[e.ColumnIndex].Name != FieldDataTable.ColumnDescription)
        {
            return;
        }

        if (FieldAtIndex(e.RowIndex) is not Fix.Field field)
        {
            return;
        }

        var definition = FindFieldDefinition(field.Tag);

        if (definition == null || definition.Values.Count == 0)
        {
            e.Cancel = true;
        }
    }

    protected override void OnUserDeletingRow(DataGridViewRowCancelEventArgs e)
    {
        //
        // We can never actually delete rows, just use this as a shortcut to clear the value.
        //
        e.Cancel = true;

        if (e.Row is DataGridViewRow row)
        {
            row.Cells[FieldDataTable.ColumnValue].Value = string.Empty;
        }
    }

    protected override void OnRowsAdded(DataGridViewRowsAddedEventArgs e)
    {
        for (int index = e.RowIndex; index < e.RowIndex + e.RowCount; ++index)
        {
            if (FieldAtIndex(index) is not Fix.Field field)
            {
                continue;
            }

            var definition = FindFieldDefinition(field.Tag);

            if (definition == null)
            {
                continue;
            }

            if (definition.Values.Count == 0)
            {
                continue;
            }

            DataGridViewRow row = Rows[index];

            if (row.Cells[FieldDataTable.ColumnDescription] is DataGridViewComboBoxCell cell)
            {
                var collection = new EnumDescriptionCollection(definition);
                cell.DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton;
                cell.ReadOnly = false;
                cell.DataSource = collection;

                if (field.Value != null && definition.Values.TryGetValue(field.Value, out var valueDefinition))
                {
                    InternalChange = true;
                    cell.Value = new EnumDescription(valueDefinition.Name, field.Value, valueDefinition.Description);
                    InternalChange = false;
                }
            }
        }
    }

    bool InternalChange { get; set; }

    protected override void OnCellValueChanged(DataGridViewCellEventArgs e)
    {
        if (InternalChange)
        {
            return;
        }

        if (Message == null)
        {
            return;
        }

        DataGridViewColumn column = Columns[e.ColumnIndex];
        DataGridViewRow row = Rows[e.RowIndex];

        if (row.DataBoundItem is not DataRowView rowView)
        {
            return;
        }

        if (rowView.Row is not FieldDataRow dataRow)
        {
            return;
        }
        //
        // We need to get the index of the row in the underlying table not the view as the view
        // may have been filtered.
        //
        var view = DataSource as DataView;

        if (view?.Table is not DataTable table)
        {
            return;
        }

        int index = table.Rows.IndexOf(dataRow);

        if (dataRow.Field is not Fix.Field field)
        {
            return;
        }

        if (column.Name == FieldDataTable.ColumnDescription)
        {
            //
            // The user has selected an item in a combo box for an field with an enumerated value so
            // update the source field as well.
            //
            if (CurrentCell?.Value is string stringValue)
            {
                dataRow[FieldDataTable.ColumnValue] = stringValue;
                Message.Fields[index].Value = stringValue;
            }
            OnFieldValueChanged();
            return;
        }

        if (column.Name != FieldDataTable.ColumnValue)
            return;

        DataGridViewCell cell = row.Cells[FieldDataTable.ColumnValue];

        string value = (cell.Value?.ToString() ?? "").Trim();

        var definition = FindFieldDefinition(field.Tag);

        if (!string.IsNullOrEmpty(value) && definition != null)
        {
            if (definition.DataType == FIX_5_0SP2.DataTypes.Int.Name ||
                definition.DataType == FIX_5_0SP2.DataTypes.Length.Name ||
                definition.DataType == FIX_5_0SP2.DataTypes.SeqNum.Name)
            {
                if (!int.TryParse(value, out var _))
                {
                    MessageBox.Show(this,
                                    string.Format("{0} must be an integer", definition.Name),
                                    Application.ProductName,
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Information);
                    cell.Value = "";
                    return;
                }
            }
        }

        Message.Fields[index].Value = value;
        OnFieldValueChanged();

        //
        // The bodylength changes whenever the message fields change so update it.
        //
        if (field.Tag == FIX_5_0SP2.Fields.BodyLength.Tag)
            return;

        foreach (DataGridViewRow r in Rows)
        {
            var rv = r.DataBoundItem as DataRowView;

            if (rv?.Row is FieldDataRow fieldDataRow)
            {
                if (fieldDataRow.Field?.Tag == FIX_5_0SP2.Fields.BodyLength.Tag)
                {
                    fieldDataRow[FieldDataTable.ColumnValue] = Message.ComputeBodyLength();
                    break;
                }
            }
        }
        //
        // Enumerated values such as 'OrderSingle.Side' have a description.
        //
        if (definition == null)
            return;

        if (row.Cells[FieldDataTable.ColumnDescription] is not DataGridViewComboBoxCell comboCell)
            return;

        if (CurrentCell != null && CurrentCell.Value != DBNull.Value && definition.Values.Count > 0)
        {
            comboCell.DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton;
            comboCell.FlatStyle = FlatStyle.Flat;
            comboCell.ReadOnly = false;
            comboCell.DataSource = new EnumDescriptionCollection(definition);
            comboCell.ValueMember = FieldDataTable.ColumnValue;
            comboCell.DisplayMember = FieldDataTable.ColumnName;
            comboCell.Value = CurrentCell.Value;
        }
        else
        {
            comboCell.Value = null;
        }
    }
}

