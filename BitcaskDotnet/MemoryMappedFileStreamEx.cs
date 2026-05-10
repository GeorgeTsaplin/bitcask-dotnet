namespace BitcaskDotnet;

internal class MemoryMappedFileStreamEx(Stream _source, string _name) : Stream
{
    public string Name => _name;
    public override bool CanRead => _source.CanRead;

    public override bool CanSeek => _source.CanSeek;

    public override bool CanWrite => _source.CanWrite;

    public override long Length => _source.Length;

    public override long Position { get => _source.Position; set => _source.Position = value; }

    public override void Flush()
    {
        _source.Flush();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        return _source.Read(buffer, offset, count);
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        return _source.Seek(offset, origin);
    }

    public override void SetLength(long value)
    {
        _source.SetLength(value);
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        _source.Write(buffer, offset, count);
    }
}
