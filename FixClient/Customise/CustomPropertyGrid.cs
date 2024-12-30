/////////////////////////////////////////////////
//
// FIX Client
//
// Copyright @ 2021 VIRTU Financial Inc.
// All rights reserved.
//
// Filename: CustomPropertyGrid.cs
// Author:   Gary Hughes
//
/////////////////////////////////////////////////
using System.Collections;
using System.ComponentModel;

namespace FixClient;

/// <summary>
/// Allows tabbing through items and has globalized tooltips.
/// </summary>
public class CustomPropertyGrid : PropertyGrid
{
    //ResourceManager _rm;

    public CustomPropertyGrid()
    {
        Refresh();
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public bool ExpandOnTab { get; set; }

    /*
	public override void Refresh() {
	    if (_rm == null) {
	        _rm = new ResourceManager(GetType());
	    }
	    // Find ToolBar in Controls.
	    foreach (Control control in Controls) {
	        var toolBar = control as ToolBar;
            if (toolBar == null)
                continue;
	        foreach (ToolBarButton button in toolBar.Buttons) {
	            // Hacked, but no other way to do it.
	            // As soon as MS decides to change the implementation
	            // this will break ofcourse.
	            switch (button.ImageIndex) {
	                case 0 :
	                    button.ToolTipText = _rm.GetString("Alphabetic.ToolTip");
	                    break;
	                case 1 :
	                    button.ToolTipText = _rm.GetString("Categorized.ToolTip");
	                    break;
	                case 3 :
	                    button.ToolTipText = _rm.GetString("PropertyPages.ToolTip");
	                    break;
	            }
	        }
	    }
	    base.Refresh();
	}
	*/

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if ((keyData == Keys.Tab) || (keyData == (Keys.Tab | Keys.Shift)))
        {
            GridItem? selectedItem = SelectedGridItem;
            GridItem? root = selectedItem;
            while (root?.Parent != null)
            {
                root = root.Parent;
            }
            if (root is null)
            {
                return base.ProcessCmdKey(ref msg, keyData);
            }
            // Find all expanded items and put them in a list.
            var items = new ArrayList();
            AddExpandedItems(root, items);
            if (selectedItem != null)
            {
                // Find selectedItem.
                int foundIndex = items.IndexOf(selectedItem);
                if ((keyData & Keys.Shift) == Keys.Shift)
                {
                    foundIndex--;

                    if (foundIndex < 0)
                    {
                        foundIndex = items.Count - 1;
                    }

                    if (items[foundIndex] is GridItem item)
                    {

                        SelectedGridItem = item;
                    }

                    if (ExpandOnTab && (SelectedGridItem?.GridItems.Count > 0))
                    {
                        SelectedGridItem.Expanded = false;
                    }
                }
                else
                {
                    foundIndex++;

                    if (foundIndex >= items.Count)
                    {
                        foundIndex = 0;
                    }

                    if (items[foundIndex] is GridItem item)
                    {
                        SelectedGridItem = item;
                    }

                    if (ExpandOnTab && (SelectedGridItem?.GridItems.Count > 0))
                    {
                        SelectedGridItem.Expanded = true;
                    }
                }

                return true;
            }
        }

        return base.ProcessCmdKey(ref msg, keyData);
    }

    static void AddExpandedItems(GridItem parent, IList items)
    {
        if (parent.PropertyDescriptor != null)
        {
            items.Add(parent);
        }
        if (parent.Expanded)
        {
            foreach (GridItem child in parent.GridItems)
            {
                AddExpandedItems(child, items);
            }
        }
    }
}

