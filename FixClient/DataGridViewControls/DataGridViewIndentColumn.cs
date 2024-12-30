/////////////////////////////////////////////////
//
// FIX Client
//
// Copyright @ 2021 VIRTU Financial Inc.
// All rights reserved.
//
// Filename: DataGridViewIndentColumn.cs
// Author:   Gary Hughes
//
/////////////////////////////////////////////////
using System.ComponentModel;

namespace FixClient.DataGridViewControls;

public class DataGridViewIndentColumn : DataGridViewColumn
{
    public DataGridViewIndentColumn()
    : base(new DataGridViewIndentCell())
    {
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public override DataGridViewCell? CellTemplate
    {
        get
        {
            return base.CellTemplate;
        }
        set
        {
            if (value != null && !value.GetType().IsAssignableFrom(typeof(DataGridViewIndentCell)))
            {
                throw new InvalidCastException("Must be a DataGridViewIndentCell");
            }
            base.CellTemplate = value;
        }
    }
}

