using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using Avalonia.Controls;
using Fix;
using FixClient.Views;

namespace FixClient.ViewModels;

public class TabItem
{
    public required string Name { get; init; }
    public required UserControl Content { get; init; }
}

public partial class MainWindowViewModel : ObservableObject
{
    PersistentSession? _session;
    
    public ObservableCollection<TabItem> TabItems { get; } = [
        new() { Name = "Session", Content = new SessionView() },
        new() { Name = "Messages", Content = new MessagesView() },
        new() { Name = "History", Content = new HistoryView() },
        new() { Name = "Orders", Content = new OrdersView() },
        new() { Name = "Log", Content = new LogView() }
    ];

    [RelayCommand]
    private async Task NewSession()
    {
        _session = new PersistentSession();
    }

    [RelayCommand]
    private async Task OpenSession()
    {
    }
    
    [RelayCommand(CanExecute = nameof(CanConnectSession))]
    private async Task ConnectSession()
    {
    }

    private bool CanConnectSession()
    {
        return _session?.State == State.Disconnected;
    }

    [RelayCommand(CanExecute = nameof(CanDisconnectSession))]
    private async Task DisconnectSession()
    {
    }

    private bool CanDisconnectSession()
    {
        return _session != null && _session.State != State.Disconnected;
    }
  
    [RelayCommand(CanExecute = nameof(CanResetSession))]
    private async Task ResetSession()
    {
    }

    private bool CanResetSession()
    {
        return false;
    }
}
