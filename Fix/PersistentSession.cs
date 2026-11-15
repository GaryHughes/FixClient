using Fix.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System.ComponentModel;

namespace Fix;

public class PersistentSession : Session, IDisposable
{
    readonly Lock _syncObject = new();

    readonly DirtyTimer _writeTimer = new();
    JsonSerializer? _serialiser;
    Writer? _historyWriter;
    string? _fileName;

    bool _closing;

    public PersistentSession()
    {
        PersistMessages = true;
        _writeTimer.Dirty += sender => Write();
    }

    // This constructor is only used for cloning
    public PersistentSession(PersistentSession session)
    : base(session)
    {
        FileName = session.FileName;
        PersistMessages = session.PersistMessages;
    }

    [Browsable(false)]
    public string? FileName
    {
        get { return _fileName; }
        set
        {
            if (!string.IsNullOrEmpty(value))
            {
                lock (_syncObject)
                {
                    _fileName = GetFileNamePrefix(value) + ".session";
                }
            }
        }
    }

    [Browsable(false)]
    public bool PersistMessages { get; set; }

    protected static string GetFileNamePrefix(string? filename)
    {
        if (string.IsNullOrEmpty(filename))
        {
            throw new Exception("filename is null or empty");
        }

        return Path.GetDirectoryName(filename) + Path.DirectorySeparatorChar +
                Path.GetFileNameWithoutExtension(filename);
    }

    string? MessagesFileName
    {
        get
        {
            if (FileName == null)
                return null;
            return GetFileNamePrefix(FileName) + ".history";
        }
    }

    public override void Open()
    {
        StartSessionWriter();
        StartHistoryWriter();
        base.Open();
    }

    public override void Close()
    {
        _closing = true;
        StopHistoryWriter();
        StopSessionWriter();
        base.Close();
        _closing = false;
    }

    protected override void OnMessageReceived(Message message)
    {
        base.OnMessageReceived(message);
        if (PersistMessages)
        {
            Messages.Add((Message)message.Clone());
        }
        _writeTimer.SetDirty();
    }

    protected override void OnMessageSending(Message message)
    {
        base.OnMessageSending(message);
        if (PersistMessages)
        {
            Messages.Add((Message)message.Clone());
        }
        _writeTimer.SetDirty();
    }

    void StartHistoryWriter()
    {
        lock (_syncObject)
        {
            if (_historyWriter == null)
            {
                if (string.IsNullOrEmpty(MessagesFileName))
                    return;

                _historyWriter = new Writer(new FileStream(MessagesFileName, FileMode.Append, FileAccess.Write, FileShare.Read), false);
                Messages.MessageAdded += MessagesMessageAdded;
            }
        }
    }

    bool StopHistoryWriter()
    {
        bool wasRunning = false;

        lock (_syncObject)
        {
            Messages.MessageAdded -= MessagesMessageAdded;
            _historyWriter?.Dispose();
            _historyWriter = null;
            wasRunning = true;
        }

        return wasRunning;
    }

    void StartSessionWriter()
    {
        _writeTimer.Start(1000, 1000);
    }

    void StopSessionWriter()
    {
        _writeTimer.Stop();
        WriteSession();
    }

    private bool Reading { get; set; }

    public virtual void Read()
    {
        lock (_syncObject)
        {
            if (FileName == null)
            {
                throw new Exception("FileName has not been set");
            }

            var serializer = CreateSerializer();

            using (var reader = new JsonTextReader(new StreamReader(new FileStream(FileName, FileMode.Open))))
            {
                serializer.ObjectCreationHandling = ObjectCreationHandling.Replace;
                serializer.Populate(reader, this);
            }

            Reading = true;
            ReadMessages();
            Reading = false;
        }
    }

    public virtual void Write()
    {
        WriteSession();
    }

    static JsonSerializer CreateSerializer()
    {
        var serializer = new JsonSerializer
        {
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.Indented
        };

        serializer.Converters.Add(new StringEnumConverter());
        serializer.Converters.Add(new Json.FixVersionConverter());

        return serializer;
    }

    void WriteSession()
    {
        try
        {
            _serialiser ??= CreateSerializer();

            PersistentSession session;

            lock (_syncObject)
            {
                session = (PersistentSession)Clone();

                if (session.FileName is null)
                {
                    OnError("Ignoring Write request because FileName has not been set");
                    return;
                }

                //
                // Write to a temporary file and then move it over the session file so a crash or power
                // failure part way through a write can't leave a truncated or empty session file.
                //
                string temporary = session.FileName + ".tmp";

                using (FileStream stream = new(temporary, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    using (var streamWriter = new StreamWriter(stream, leaveOpen: true))
                    using (JsonWriter writer = new JsonTextWriter(streamWriter) { Formatting = Formatting.Indented })
                    {
                        JObject environment = JObject.FromObject(session, _serialiser);
                        environment.WriteTo(writer);
                    }

                    stream.Flush(flushToDisk: true);
                }

                try
                {
                    ReplaceFile(temporary, session.FileName);
                }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
                {
                    //
                    // Another program (antivirus, a file sync client) can briefly hold the session file
                    // open. Keep the previous session file and try again on the next write.
                    //
                    OnWarning($"Unable to replace session file '{ex.Message}', will retry");
                    _writeTimer.SetDirty();
                }
            }
        }
        catch (Exception ex)
        {
            OnError($"Unable to write session file '{ex.Message}'");
            if (!_closing)
            {
                Close();
            }
        }
    }

    static void ReplaceFile(string source, string destination)
    {
        const int attempts = 3;

        for (int attempt = 1; ; ++attempt)
        {
            try
            {
                File.Move(source, destination, overwrite: true);
                return;
            }
            catch (Exception ex) when (attempt < attempts && ex is IOException or UnauthorizedAccessException)
            {
                Thread.Sleep(50 * attempt);
            }
        }
    }

    void ReadMessages()
    {
        if (!PersistMessages)
            return;

        int errors = 0;

        try
        {
            if (MessagesFileName is null)
            {
                return;
            }

            using FileStream stream = new(MessagesFileName, FileMode.OpenOrCreate);
            using Reader reader = new(stream);
            for (; ; )
            {
                try
                {
                    Message? message = reader.ReadLine();
                    if (message == null)
                        break;
                    Messages.Add(message);
                }
                catch (Exception ex)
                {
                    ++errors;
                    OnWarning(ex.Message);
                    reader.DiscardLine();
                }
            }
        }
        catch (Exception ex)
        {
            ++errors;
            OnWarning(ex.Message);
        }

        if (errors > 0)
        {
            OnWarning("{0} error{1} occurred reading the message history - messages may be missing or incorrect",
                        errors, errors == 1 ? "" : "s");
        }

        StartHistoryWriter();
    }

    public override void ResetMessages()
    {
        StopSessionWriter();
        bool historyWriterWasRunning = StopHistoryWriter();
        base.ResetMessages();
        if (MessagesFileName is not null)
        {
            File.Delete(MessagesFileName);
        }
        StartSessionWriter();
        if (historyWriterWasRunning)
        {
            StartHistoryWriter();
        }
    }

    void MessagesMessageAdded(object sender, MessageCollection.MessageEvent ev)
    {
        if (Reading)
        {
            return;
        }

        try
        {
            var clone = (Message)ev.Message.Clone();
            _historyWriter?.WriteLine(clone);
        }
        catch (Exception ex)
        {
            OnError(ex.Message);
        }
    }

    #region IDisposable Members

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            StopSessionWriter();
            StopHistoryWriter();
            _writeTimer.Dispose();
            _disposed = true;
        }
    }

    bool _disposed;

    #endregion

    #region ICloneable

    public override object Clone()
    {
        return new PersistentSession(this);
    }

    #endregion
}

