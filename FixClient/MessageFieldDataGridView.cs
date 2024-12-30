/////////////////////////////////////////////////
//
// FIX Client
//
// Copyright @ 2021 VIRTU Financial Inc.
// All rights reserved.
//
// Filename: MessageFieldDataGridView.cs
// Author:   Gary Hughes
//
/////////////////////////////////////////////////
using System.Data;
using System.Drawing;

namespace FixClient;

public partial class MessageFieldDataGridView : DataGridView
{
    public MessageFieldDataGridView()
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
        Dock = DockStyle.Fill;
        BackgroundColor = LookAndFeel.Color.GridCellBackground;
        BorderStyle = BorderStyle.None;
        CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        MultiSelect = true;
        RowHeadersVisible = false;
        DefaultCellStyle.WrapMode = DataGridViewTriState.False;
        RowTemplate.Resizable = DataGridViewTriState.False;
        GridColor = LookAndFeel.Color.Grid;
        AllowUserToAddRows = false;
        DefaultCellStyle.Font = new Font("Arial", 8);
        EnableHeadersVisualStyles = false;
        RowTemplate.Height -= 3;
        DefaultCellStyle.BackColor = LookAndFeel.Color.GridCellBackground;
        DefaultCellStyle.Padding = new Padding(3, 0, 3, 0);
        DefaultCellStyle.SelectionBackColor = LookAndFeel.Color.GridCellSelectedBackground;
        DefaultCellStyle.SelectionForeColor = LookAndFeel.Color.GridCellSelectedForeground;
        EditMode = DataGridViewEditMode.EditOnEnter;
        AutoGenerateColumns = false;
        //
        // We need to manually define the columns so we can configure the ComboBox column.
        //
        DataGridViewColumn column;

        column = new DataGridViewControls.DataGridViewIndentColumn
        {
            Name = FieldDataTable.ColumnIndent,
            DataPropertyName = FieldDataTable.ColumnIndent,
            ReadOnly = true,
            HeaderText = string.Empty,
            Width = 15
        };
        Columns.Add(column);

        column = new DataGridViewTextBoxColumn
        {
            Name = FieldDataTable.ColumnTag,
            DataPropertyName = FieldDataTable.ColumnTag,
            AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells,
            ReadOnly = true,
            DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft }
        };
        Columns.Add(column);

        column = new DataGridViewTextBoxColumn
        {
            Name = FieldDataTable.ColumnName,
            DataPropertyName = FieldDataTable.ColumnName,
            AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells,
            ReadOnly = true
        };
        Columns.Add(column);

        column = new DataGridViewTextBoxColumn
        {
            Name = FieldDataTable.ColumnValue,
            DataPropertyName = FieldDataTable.ColumnValue,
            AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells
        };
        Columns.Add(column);

        var comboColumn = new DataGridViewComboBoxColumn
        {
            Name = FieldDataTable.ColumnDescription,
            DisplayMember = FieldDataTable.ColumnName,
            ValueMember = FieldDataTable.ColumnValue,
            FlatStyle = FlatStyle.Flat,
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
            DisplayStyle = DataGridViewComboBoxDisplayStyle.Nothing
        };

        Columns.Add(comboColumn);
    }

    protected override void OnColumnHeaderMouseClick(DataGridViewCellMouseEventArgs e)
    {
        if (ModifierKeys == Keys.Control)
        {
            if (DataSource is DataView view)
            {
                view.Sort = string.Empty;
                Refresh();
                return;
            }
        }

        base.OnColumnHeaderMouseClick(e);
    }

    public Fix.Field? FieldAtIndex(int rowIndex)
    {
        DataGridViewRow row = Rows[rowIndex];

        if (row.DataBoundItem is not DataRowView rowView)
        {
            return null;
            // HMM - this was thrown when changing from the messages to the history view
            //throw new Exception(string.Format("MessageFieldDataGridView Row.DataBoundItem at index {0} is not a DataRowView ", rowIndex));
        }

        if (rowView.Row is not FieldDataRow dataRow)
        {
            throw new Exception(string.Format("MessageFieldDataGridView RowView.Row at index {0} is not a FieldDataRow", rowIndex));
        }

        return dataRow.Field;
    }

    /*
    protected string ToolTipForCell(int rowIndex, int columnIndex, string value)
    {
        DataGridViewRow row = Rows[rowIndex];

        if (row.DataBoundItem is not DataRowView rowView || rowView.IsNew)
        {
            return null;
        }

        var dataRow = (FieldDataRow)rowView.Row;

        Fix.Field field = dataRow.Field;

        var definition = FIX_5_0SP2.Fields[field.Tag];

        if (definition == null)
        {
            return null;
        }

        if (definition.Values.Count == 0)
        {
            return null;
        }

        return string.Join("\n", definition.Description.SplitInParts(100));
    }
    */

    /*
    protected override void OnCellToolTipTextNeeded(DataGridViewCellToolTipTextNeededEventArgs e)
    {
        if (e.RowIndex < 0)
        {
            return;
        }

        DataGridViewColumn column = Columns[e.ColumnIndex];

        if (column.Name != FieldDataTable.ColumnDescription && column.Name != FieldDataTable.ColumnValue)
            {
            return;
        }

        DataGridViewCell cell = Rows[e.RowIndex].Cells[e.ColumnIndex];

        if (cell.Value == null || cell.Value == DBNull.Value)
            {
            return;
        }

        if (!char.TryParse(cell.Value.ToString(), out char enumValue))
        {
            if (!(cell.Value is int))
            {
                return;
            }
            enumValue = Convert.ToChar(cell.Value);
        }

        // TODO
        //e.ToolTipText = ToolTipForCell(e.RowIndex, e.ColumnIndex, enumValue);
    }
    */

    protected override void OnCellFormatting(DataGridViewCellFormattingEventArgs e)
    {
        try
        {
            DataGridViewRow row = Rows[e.RowIndex];

            if (row.DataBoundItem is not DataRowView rowView || rowView.IsNew)
            {
                return;
            }

            var dataRow = (FieldDataRow)rowView.Row;

            DataGridViewColumn column = Columns[e.ColumnIndex];

            if (column.Name != FieldDataTable.ColumnIndent)
            {
                object customValue = dataRow[FieldDataTable.ColumnCustom];
                bool custom = false;

                if (customValue != null && customValue != DBNull.Value)
                {
                    custom = (bool)customValue;
                }

                if (e.CellStyle is not null)
                {
                    if (custom)
                    {
                        e.CellStyle.ForeColor = Color.Brown;
                    }
                    else
                    {
                        object requiredValue = dataRow[FieldDataTable.ColumnRequired];
                        var required = (bool)requiredValue;
                        e.CellStyle.ForeColor = required ? LookAndFeel.Color.Incoming : LookAndFeel.Color.Outgoing;
                    }
                }
            }
        }
        finally
        {
            base.OnCellFormatting(e);
        }
    }
}

