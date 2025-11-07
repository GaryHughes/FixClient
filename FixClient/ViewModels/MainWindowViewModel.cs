using System;
using System.Collections.ObjectModel;
using System.Net.Sockets;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Avalonia.Controls;
using Avalonia.Threading;
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
    private TcpClient? _tcpClient;
    
    [ObservableProperty] private FixClient.Models.Session? _session;
    
    public ObservableCollection<TabItem> TabItems { get; } 

    public MainWindowViewModel()
    {
        // TODO - What is the best way to set the data context?
        TabItems = [
            new TabItem() { Name = "Session", Content = new SessionView() },
            new TabItem() { Name = "Messages", Content = new MessagesView() },
            new TabItem() { Name = "History", Content = new HistoryView() },
            new TabItem() { Name = "Orders", Content = new OrdersView() },
            new TabItem() { Name = "Log", Content = new LogView() }
        ];
    }

    [RelayCommand]
    private async Task NewSession()
    {
        Session = new FixClient.Models.Session();
        Session.StateChanged += (sender, args) => Dispatcher.UIThread.Invoke(UpdateSessionUiState);
        UpdateSessionUiState();
    }

    [RelayCommand]
    private async Task OpenSession()
    {
        UpdateSessionUiState();
    }
    
    [RelayCommand(CanExecute = nameof(CanConnectSession))]
    private async Task ConnectSession()
    {
        // TODO - add UI and error handling
        if (Session is null)
        {
            return;
        }

        Session.BeginString = Dictionary.Versions.FIXT_1_1;
        Session.SenderCompId = "INITIATOR";
        Session.TargetCompId = "ACCEPTOR";
        Session.Host = "127.0.0.1";
        Session.Port = 8089;
        var endpoint = Session.EndPoint();
        _tcpClient = new TcpClient();
        await _tcpClient.ConnectAsync(endpoint);
        Session.Stream = _tcpClient.GetStream();
        Session.Open();
        UpdateSessionUiState();
    }

    private bool CanConnectSession()
    {
        return Session?.State == State.Disconnected;
    }

    [RelayCommand(CanExecute = nameof(CanDisconnectSession))]
    private void DisconnectSession()
    {
        if (Session is not null)
        {
            Session.Close();
            _tcpClient?.Dispose();
            _tcpClient = null;
        }

        UpdateSessionUiState();
    }

    private bool CanDisconnectSession()
    {
        return Session != null && Session.State != State.Disconnected;
    }
  
    [RelayCommand(CanExecute = nameof(CanResetSession))]
    private void ResetSession()
    {
        Session?.Reset();
        UpdateSessionUiState();
    }

    private bool CanResetSession()
    {
        return Session?.State switch
        {
            null => true,
            State.Disconnected => true,
            _ => false
        };
    }

    private void UpdateSessionUiState()
    {
        ConnectSessionCommand.NotifyCanExecuteChanged();
        DisconnectSessionCommand.NotifyCanExecuteChanged();
        ResetSessionCommand.NotifyCanExecuteChanged();
    }
    
    [RelayCommand(CanExecute = nameof(CanClearLogMessages))]
    private void ClearLogMessages() => Session?.LogEvents.Clear();
   
    private bool CanClearLogMessages() => Session?.LogEvents.Any() ?? false;

   
    [RelayCommand(CanExecute = nameof(CanClearLogMessages))]
    private void ClearMessages()
    {
    }

    private bool CanClearMessages() => Session?.HistoryMessages.Any() ?? false;
   
    [RelayCommand(CanExecute = nameof(CanResendMessage))]
    private void ResendMessage()
    {
    }

    private bool CanResendMessage()
    {
        return false;
    }

    [RelayCommand(CanExecute = nameof(CanExportMessages))]
    private void ExportMessages()
    {
    }

    private bool CanExportMessages()
    {
        return false;
    }
    
}
