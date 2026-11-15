namespace Fix.Common;

public class DirtyTimer : IDisposable
{
    public delegate void DirtyHandler(object sender);
    public event DirtyHandler? Dirty;

    void OnDirty()
    {
        Dirty?.Invoke(this);
    }

    Timer? _timer;
    int _dirty;
    const int CLEAN = 0;
    const int DIRTY = 1;

    public void SetDirty()
    {
        Interlocked.CompareExchange(ref _dirty, DIRTY, CLEAN);
    }

    public void Start(int dueTime, int period)
    {
        _timer = new Timer(TimerFired, null, dueTime, period);
    }

    public void Stop()
    {
        _timer?.Dispose();
        _timer = null;
    }

    void TimerFired(object? context)
    {
        if (Interlocked.CompareExchange(ref _dirty, CLEAN, DIRTY) == DIRTY)
        {
            OnDirty();
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
            Stop();
            _disposed = true;
        }
    }

    bool _disposed;

    #endregion
}

