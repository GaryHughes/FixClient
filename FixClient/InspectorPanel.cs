/////////////////////////////////////////////////
//
// FIX Client
//
// Copyright @ 2021 VIRTU Financial Inc.
// All rights reserved.
//
// Filename: InspectorPanel.cs
// Author:   Gary Hughes
//
/////////////////////////////////////////////////
using System.ComponentModel;

namespace FixClient;

class MessageProperties
{
    readonly Fix.Dictionary.Message _message;

    public MessageProperties(Fix.Dictionary.Message message)
    {
        _message = message;
    }

    const string Category = "Message";

    [Category(Category)]
    public string Name => _message.Name;

    [Category(Category)]
    public string MsgType => _message.MsgType;

    [Category(Category)]
    public string Pedigree => _message.Pedigree.ToString();
}

class FieldProperties
{
    readonly Fix.FieldDescription _description;

    public FieldProperties(Fix.FieldDescription description)
    {
        _description = description;
    }

    const string Category = "Field";

    [Category(Category)]
    public string Name => _description.Name;

    [Category(Category)]
    public int Tag => _description.Tag;

    [Category(Category)]
    public string DataType => _description.DataType;

    [Category(Category)]
    public int Depth => _description.Depth;

    [Category(Category)]
    public bool Required => _description.Required;

    [Category(Category)]
    public Fix.Dictionary.Pedigree Pedigree => _description.Pedigree;


}

class ValueProperties
{
    const string Category = "Value";

    public ValueProperties(Fix.Dictionary.FieldValue value)
    {
        Name = value.Name;
        Value = value.Value;
        Pedigree = value.Pedigree;
    }

    [Category(Category)]
    public string Name { get; }

    [Category(Category)]
    public string Value { get; }

    [Category(Category)]
    public Fix.Dictionary.Pedigree Pedigree { get; }
}

public partial class InspectorPanel : Panel
{
    readonly PropertyGrid _messagePropertyGrid;
    readonly TextBox _messageDescription;
    readonly PropertyGrid _fieldPropertyGrid;
    readonly TextBox _fieldDescription;
    readonly PropertyGrid _valuePropertyGrid;
    readonly TextBox _valueDescription;

    Fix.Dictionary.Message? _messageDefinition;
    Fix.FieldDescription? _fieldDefinition;

    public InspectorPanel()
    {
        InitializeComponent();

        _messagePropertyGrid = new PropertyGrid
        {
            Dock = DockStyle.Fill,
            HelpVisible = false,
            ToolbarVisible = false
        };

        _messageDescription = new TextBox
        {
            Dock = DockStyle.Fill,
            Multiline = true,
            ReadOnly = true
        };

        _fieldPropertyGrid = new PropertyGrid
        {
            Dock = DockStyle.Fill,
            HelpVisible = false,
            ToolbarVisible = false
        };

        _fieldDescription = new TextBox
        {
            Dock = DockStyle.Fill,
            Multiline = true,
            ReadOnly = true
        };

        _valuePropertyGrid = new PropertyGrid
        {
            Dock = DockStyle.Fill,
            HelpVisible = false,
            ToolbarVisible = false
        };

        _valueDescription = new TextBox
        {
            Dock = DockStyle.Fill,
            Multiline = true,
            ReadOnly = true
        };

        var splitterOne = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Horizontal
        };

        var splitterTwo = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Horizontal,
            SplitterDistance = 500
        };

        var messageSplitter = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Horizontal
        };

        var fieldSplitter = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Horizontal
        };

        var valueSplitter = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Horizontal
        };

        var panel1 = new BorderHidingPanel(_messagePropertyGrid) { Dock = DockStyle.Fill };
        var panel2 = new BorderHidingPanel(_messageDescription) { Dock = DockStyle.Fill };

        var panel3 = new BorderHidingPanel(_fieldPropertyGrid) { Dock = DockStyle.Fill };
        var panel4 = new BorderHidingPanel(_fieldDescription) { Dock = DockStyle.Fill };

        var panel5 = new BorderHidingPanel(_valuePropertyGrid) { Dock = DockStyle.Fill };
        var panel6 = new BorderHidingPanel(_valueDescription) { Dock = DockStyle.Fill };

        messageSplitter.Panel1.Controls.Add(panel1);
        messageSplitter.Panel2.Controls.Add(panel2);

        fieldSplitter.Panel1.Controls.Add(panel3);
        fieldSplitter.Panel2.Controls.Add(panel4);

        valueSplitter.Panel1.Controls.Add(panel5);
        valueSplitter.Panel2.Controls.Add(panel6);

        splitterOne.Panel1.Controls.Add(messageSplitter);
        splitterOne.Panel2.Controls.Add(fieldSplitter);

        splitterTwo.Panel1.Controls.Add(splitterOne);
        splitterTwo.Panel2.Controls.Add(valueSplitter);

        Controls.Add(splitterTwo);
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Fix.Dictionary.Message? Message
    {
        set
        {
            _messageDefinition = value;
            _messagePropertyGrid.SelectedObject = value is null ? null : new MessageProperties(value);
            _messageDescription.Text = value?.Description;
        }
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Fix.Field? Field
    {
        set
        {
            _fieldDefinition = value?.Describe(_messageDefinition);

            _fieldPropertyGrid.SelectedObject = _fieldDefinition is null ? null : new FieldProperties(_fieldDefinition);
            _fieldDescription.Text = _fieldDefinition?.Description;

            if (_fieldDefinition?.ValueDefinition is Fix.Dictionary.FieldValue fieldValue)
            {
                _valuePropertyGrid.SelectedObject = new ValueProperties(fieldValue);
                _valueDescription.Text = fieldValue.Description;
            }
            else
            {
                _valuePropertyGrid.SelectedObject = null;
                _valueDescription.Text = null;
            }
        }
    }
}

