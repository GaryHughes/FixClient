namespace Fix;

public class NonClosingStreamDecorator : Stream
{
    readonly Stream _innerStream;

    public NonClosingStreamDecorator(Stream stream)
    {
        _innerStream = stream ?? throw new ArgumentNullException(nameof(stream));
    }

    public override void Close()
    {
        // NOP
    }

    protected override void Dispose(bool disposing)
    {
        // NOP
    }

    public override bool CanRead => _innerStream.CanRead;

    public override bool CanSeek => _innerStream.CanSeek;

    public override bool CanWrite => _innerStream.CanWrite;

    public override void Flush() => _innerStream.Flush();

    public override long Length => _innerStream.Length;

    public override long Position
    {
        get
        {
            return _innerStream.Position;
        }
        set
        {
            _innerStream.Position = value;
        }
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        return _innerStream.Read(buffer, offset, count);
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        return _innerStream.Seek(offset, origin);
    }

    public override void SetLength(long value)
    {
        _innerStream.SetLength(value);
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        _innerStream.Write(buffer, offset, count);
    }
}

