using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia.Threading;
using Fix;

namespace FixClient.Models;

[ObservableObject]
public partial class Session : Fix.PersistentSession
{
    public ObservableCollection<LogEvent> LogEvents { get; } = [];
    // TODO - Should we make Fix.Session.Messages observable or remove it? Needs to be replaced in some form to support replay etc
    public ObservableCollection<Message> HistoryMessages { get; } = [];

    public Session()
    {
        void AddHistoryMessage(Fix.Message message)
        {
            message.Definition = Dictionary.FIX_5_0SP2.Messages[message.MsgType];
            HistoryMessages.Add(message);
        }
        
        Information += (sender, ev) => Dispatcher.UIThread.InvokeAsync(() => LogEvents.Add(ev));
        Warning += (sender, ev) => Dispatcher.UIThread.InvokeAsync(() => LogEvents.Add(ev)); 
        Error += (sender, ev) => Dispatcher.UIThread.InvokeAsync(() => LogEvents.Add(ev));
        MessageSent += (sender, ev) => Dispatcher.UIThread.InvokeAsync(() => AddHistoryMessage(ev.Message));
        MessageReceived += (sender, ev) => Dispatcher.UIThread.InvokeAsync(() => AddHistoryMessage(ev.Message));
    }
    
    public Fix.Behaviour Behaviour { get; set; }
    public string BindHost { get; set; }
    public int BindPort { get; set; }
    public string Host { get; set; }
    public int Port { get; set; }
    
}