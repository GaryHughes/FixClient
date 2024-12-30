/////////////////////////////////////////////////
//
// FIX Client
//
// Copyright @ 2021 VIRTU Financial Inc.
// All rights reserved.
//
// Filename: ToolStripCheckBox.cs
// Author:   Gary Hughes
//
/////////////////////////////////////////////////
using System.ComponentModel;
using System.Drawing;

namespace FixClient;

public partial class ToolStripCheckBox : ToolStripControlHost
{
    readonly FlowLayoutPanel _controlPanel;
    readonly CheckBox _checkBox = new();

    public ToolStripCheckBox() : base(new FlowLayoutPanel())
    {
        _controlPanel = (FlowLayoutPanel)Control;
        _controlPanel.BackColor = Color.Transparent;
        //
        // up the top padding a touch to make the alignment look better.
        //
        _checkBox.AutoSize = true;
        _checkBox.Padding = new Padding(_checkBox.Padding.Left,
                                        _checkBox.Padding.Top + 2,
                                        _checkBox.Padding.Right,
                                        _checkBox.Padding.Bottom);
        _checkBox.CheckedChanged += CheckBoxCheckedChanged;
        _controlPanel.Controls.Add(_checkBox);
    }

    void CheckBoxCheckedChanged(object? sender, EventArgs e)
    {
        CheckChanged?.Invoke(sender, e);
    }

    public event EventHandler? CheckChanged;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public bool Checked
    {
        get { return _checkBox.Checked; }
        set { _checkBox.Checked = value; }
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public new string Text
    {
        get { return _checkBox.Text; }
        set { _checkBox.Text = value; }
    }
}

