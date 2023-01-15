/////////////////////////////////////////////////
//
// FIX Client
//
// Copyright @ 2021 VIRTU Financial Inc.
// All rights reserved.
//
// Filename: CustomisePanel.cs
// Author:   Gary Hughes
//
/////////////////////////////////////////////////
using System.Data;
using System.Drawing;

namespace FixClient;

public partial class CustomisePanel : FixClientPanel
{
    Session? _session;

    readonly CustomiseFieldDataGridView _fieldGrid;
    readonly CustomFieldDataTable _fieldTable;

    readonly ToolStripButton _newFieldButton;
    readonly ToolStripButton _editFieldButton;
    readonly ToolStripButton _deleteFieldButton;
    readonly ToolStripComboBox _categoryComboBox;
    readonly ToolStripButton _addCategoryButton;

    readonly ToolStripMenuItem _newFieldMenuItem;
    readonly ToolStripMenuItem _editFieldMenuItem;
    readonly ToolStripMenuItem _deleteFieldMenuItem;

    public CustomisePanel()
    {
        InitializeComponent();

        _fieldTable = new CustomFieldDataTable("CustomFields");
        _fieldGrid = new CustomiseFieldDataGridView
        {
            Dock = DockStyle.Fill,
            DataSource = _fieldTable
        };
        _fieldGrid.SelectionChanged += FieldGridSelectionChanged;
        //
        // Create the field toolstrip.
        //
        var toolStrip = new ToolStrip
        {
            GripStyle = ToolStripGripStyle.Hidden,
            BackColor = LookAndFeel.Color.ToolStrip,
            Renderer = new ToolStripRenderer()
        };
        TopToolStripPanel.Join(toolStrip);

        _newFieldButton = new ToolStripButton
        {
            Image = Properties.Resources.NewCustomField,
            ImageTransparentColor = Color.Magenta,
            ToolTipText = "Create a new custom field"
        };
        _newFieldButton.Click += NewFieldButtonClick;

        _editFieldButton = new ToolStripButton
        {
            Image = Properties.Resources.EditCustomField,
            ImageTransparentColor = Color.Magenta,
            ToolTipText = "Edit the selected custom field"
        };
        _editFieldButton.Click += EditFieldButtonClick;

        _deleteFieldButton = new ToolStripButton
        {
            Image = Properties.Resources.DeleteCustomField,
            ImageTransparentColor = Color.Magenta,
            ToolTipText = "Delete the selected custom field"
        };
        _deleteFieldButton.Click += DeleteFieldButtonClick;

        _categoryComboBox = new ToolStripComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList
        };

        var customFields = new CustomFieldCollection(Properties.Resources.CustomFields);

        foreach (CustomFieldCategory category in customFields.Fields.Values)
        {
            _categoryComboBox.Items.Add(category);
        }

        if (_categoryComboBox.Items.Count > 0)
        {
            _categoryComboBox.SelectedIndex = 0;
        }

        _addCategoryButton = new ToolStripButton
        {
            Image = Properties.Resources.AddCategoryFields,
            ImageTransparentColor = Color.Magenta,
            ToolTipText = "Add custom fields for the selected category"
        };
        _addCategoryButton.Click += AddCategoryButtonClick;

        toolStrip.Items.Add(_newFieldButton);
        toolStrip.Items.Add(_editFieldButton);
        toolStrip.Items.Add(_deleteFieldButton);
        toolStrip.Items.Add(new ToolStripSeparator());
        toolStrip.Items.Add(_categoryComboBox);
        toolStrip.Items.Add(_addCategoryButton);

        var menu = new ToolStripMenuItem("Action");
        SetMenuStrip(menu);

        _newFieldMenuItem = new ToolStripMenuItem("New Field...", _newFieldButton.Image);
        _newFieldMenuItem.Click += NewFieldButtonClick;
        menu.DropDownItems.Add(_newFieldMenuItem);

        _editFieldMenuItem = new ToolStripMenuItem("Edit Field...", _editFieldButton.Image);
        _editFieldMenuItem.Click += EditFieldButtonClick;
        menu.DropDownItems.Add(_editFieldMenuItem);

        _deleteFieldMenuItem = new ToolStripMenuItem("Delete Field(s)", _deleteFieldButton.Image);
        _deleteFieldMenuItem.Click += DeleteFieldButtonClick;
        menu.DropDownItems.Add(_deleteFieldMenuItem);

        ContentPanel.Controls.Add(_fieldGrid);

        UpdateUiState();
    }

    void AddCategoryButtonClick(object? sender, EventArgs e)
    {
        if (_categoryComboBox.SelectedItem == null)
        {
            return;
        }

        if (Session is null)
        {
            return;
        }

        if (_categoryComboBox.SelectedItem is CustomFieldCategory category)
        {
            foreach (CustomField field in category.Fields)
            {
                try
                {
                    var _ = Session.Version.Fields[field.Tag];
                    MessageBox.Show(this,
                                    string.Format(
                                    "{0} already had a field with tag = {1} ignoring custom field {1} = {2}",
                                    Session.Version.BeginString,
                                    field.Tag,
                                    field.Name),
                                    Application.ProductName,
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Information);
                }
                catch (ArgumentOutOfRangeException)
                {
                    if (!Session.CustomFields.ContainsKey(field.Tag))
                    {
                        AddField(field);
                    }
                }
            }
            Session.WriteCustomFields();
        }
    }

    void FieldGridSelectionChanged(object? sender, EventArgs e)
    {
        if (_fieldGrid.SelectedRows.Count < 1)
        {
            UpdateUiState();
            return;
        }

        UpdateUiState();
    }

    void EditFieldButtonClick(object? sender, EventArgs e)
    {
        if (Session is null)
        {
            return;
        }

        if (_fieldGrid.SelectedRows.Count < 1)
        {
            return;
        }

        using CustomFieldForm form = new(Session.Version, true);
        DataGridViewRow row = _fieldGrid.SelectedRows[0];
        var view = row.DataBoundItem as DataRowView;
        var dataRow = view?.Row as CustomFieldDataRow;
        if (dataRow?.Field is not CustomField field)
        {
            return;
        }

        form.Field = new CustomField { Tag = field.Tag, Name = field.Name };

        if (form.ShowDialog() != DialogResult.OK)
            return;

        field.Name = form.Field.Name;

        _fieldGrid.SelectedRows[0].Cells[CustomiseFieldDataGridView.ColumnFieldName].Value = field.Name;
        _fieldGrid.Update();
        Session.WriteCustomFields();
    }

    void DeleteFieldButtonClick(object? sender, EventArgs e)
    {
        if (Session is null)
        {
            return;
        }

        if (_fieldGrid.SelectedRows.Count < 1)
        {
            return;
        }

        DialogResult result = MessageBox.Show(this,
                                                "Are you sure you want to delete this custom field?",
                                                Application.ProductName,
                                                MessageBoxButtons.YesNo,
                                                MessageBoxIcon.Question);

        if (result != DialogResult.Yes)
        {
            return;
        }

        DataGridViewRow row = _fieldGrid.SelectedRows[0];
        var view = row.DataBoundItem as DataRowView;
        var dataRow = view?.Row as CustomFieldDataRow;
        if (dataRow?.Field is not CustomField customField)
        {
            return;
        }
        //
        // Remove the field definition.
        //
        RemoveField(customField);
        /*
        //
        // Remove any instances of the field in the message defaults.
        //
        foreach(Fix.Message message in Session.MessageDefaults)
        {
            for (int index = 0; index < message.Fields.Count;)
            {
                if (customField != null)
                {
                    if (message.Fields[index].Id == (Fix.Field.FieldId) customField.Id)
                    {
                        message.Fields.Remove(index, index);
                        continue;
                    }
                }

                ++index;
            }
        }
        */
        Session.WriteCustomFields();
    }

    void NewFieldButtonClick(object? sender, EventArgs e)
    {
        if (Session is null)
        {
            return;
        }

        using CustomFieldForm form = new(Session.Version, false);

        if (form.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        CustomField field = form.Field;

        if (!Session.CustomFields.ContainsKey(field.Tag))
        {
            AddField(field);
            Session.WriteCustomFields();
        }
    }

    void UpdateUiState()
    {
        if (Session == null)
        {
            _newFieldButton.Enabled = false;
            _editFieldButton.Enabled = false;
            _deleteFieldButton.Enabled = false;

            _newFieldMenuItem.Enabled = false;
            _editFieldMenuItem.Enabled = false;
            _deleteFieldMenuItem.Enabled = false;

            _categoryComboBox.Enabled = false;
            _addCategoryButton.Enabled = false;

            return;
        }

        _newFieldButton.Enabled = true;
        _newFieldMenuItem.Enabled = true;

        _categoryComboBox.Enabled = true;
        _addCategoryButton.Enabled = true;

        //
        // We can delete many fields at a time.
        //
        if (_fieldGrid.SelectedRows.Count > 0)
        {
            _deleteFieldButton.Enabled = true;
            _deleteFieldMenuItem.Enabled = true;
        }
        else
        {
            _deleteFieldButton.Enabled = false;
            _deleteFieldMenuItem.Enabled = false;
        }

        //
        // We can only edit one field at a time.
        //
        if (_fieldGrid.SelectedRows.Count == 1)
        {
            _editFieldButton.Enabled = true;
            _editFieldMenuItem.Enabled = true;
        }
        else
        {
            _editFieldButton.Enabled = false;
            _editFieldMenuItem.Enabled = false;
        }
    }

    void AddField(CustomField field)
    {
        if (Session is null)
        {
            return;
        }

        Session.AddCustomField(field);
    }

    void RemoveField(CustomField field)
    {
        if (Session is null)
        {
            return;
        }

        Session.RemoveCustomField(field);

        try
        {
            _fieldTable.BeginLoadData();

            if (_fieldTable.Rows.Find(field.Tag) is DataRow row)
            {
                _fieldTable.Rows.Remove(row);
            }
        }
        finally
        {
            _fieldTable.EndLoadData();
        }
    }

    void Reload()
    {
        try
        {
            _fieldTable.BeginLoadData();
            _fieldTable.Clear();

            if (_session == null)
                return;

            foreach (CustomField field in _session.CustomFields.Values)
            {
                AddCustomField(field);
            }
        }
        finally
        {
            _fieldTable.EndLoadData();
        }
    }

    void AddCustomField(CustomField field)
    {
        var row = (CustomFieldDataRow)_fieldTable.NewRow();
        row.Field = field;
        row[CustomFieldDataTable.ColumnNameTag] = field.Tag;
        row[CustomFieldDataTable.ColumnNameName] = field.Name;
        _fieldTable.Rows.Add(row);
    }

    void SessionCustomFieldAdded(object? sender, Session.CustomFieldEventArgs ev)
    {
        if (InvokeRequired)
        {
            BeginInvoke(new MethodInvoker(() => SessionCustomFieldAdded(sender, ev)));
            return;
        }

        try
        {
            _fieldTable.BeginLoadData();
            AddCustomField(ev.Field);
        }
        finally
        {
            _fieldTable.EndLoadData();
        }
    }

    public Session? Session
    {
        get
        {
            return _session;
        }
        set
        {
            if (_session != null)
            {
                _session.CustomFieldAdded -= SessionCustomFieldAdded;
            }

            _session = value;

            if (_session != null)
            {
                _session.CustomFieldAdded += SessionCustomFieldAdded;
            }

            Reload();
            UpdateUiState();
            FieldGridSelectionChanged(this, EventArgs.Empty);
        }
    }
}
