using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FixClient.Models;
using FixClient.ViewModels;

namespace FixClient.Views;

public partial class MessagesView : UserControl
{
    public MessagesView()
    {
        InitializeComponent();
    }

    private void MessagesDataGrid_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel model)
        {
            return;
        }

        if (sender is not DataGrid { SelectedItem: Fix.Dictionary.Message messageDefinition })
        {
            model.HistoryDataDictionaryInspectorProperties.ClearMessageProperties();
            return;
        }

        model.HistoryDataDictionaryInspectorProperties.Message = new MessageProperties(messageDefinition);
    }
}