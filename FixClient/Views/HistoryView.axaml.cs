using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FixClient.Models;
using FixClient.ViewModels;

namespace FixClient.Views;

public partial class HistoryView : UserControl
{
    public HistoryView()
    {
        InitializeComponent();
    }

    private void HistoryMessagesDataGrid_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel model)
        {
            return;
        }

        if (sender is not DataGrid { SelectedItem: Fix.Message { Definition: Fix.Dictionary.Message messageDefinition } })
        {
            model.HistoryDataDictionaryInspectorProperties.ClearMessageProperties();
            return;
        }

        model.HistoryDataDictionaryInspectorProperties.Message = new MessageProperties(messageDefinition);
    }

    private void HistoryFieldsDataGrid_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel model)
        {
            return;
        }
        
        if (sender is not DataGrid { SelectedItem: Fix.Field { Definition: Fix.FieldDescription fieldDescription } })
        {
            model.HistoryDataDictionaryInspectorProperties.ClearFieldProperties();
            return;
        }

        model.HistoryDataDictionaryInspectorProperties.Field = new FieldProperties(fieldDescription);

        if (fieldDescription.ValueDefinition is not Fix.Dictionary.FieldValue valueDefinition)
        {
            model.HistoryDataDictionaryInspectorProperties.ClearValueProperties();
            return;
        }
        
        model.HistoryDataDictionaryInspectorProperties.Value = new ValueProperties(valueDefinition);
    }
}