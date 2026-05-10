namespace BitcaskDotnet;

public class BitcaskStream : Stream
{
    private readonly Stream _source;
    public BitcaskStream(Stream source, string name)
    {
        _source = source;
        Name = name;
    }
    public BitcaskStream(FileStream source)
        : this(source, source.Name)
    { }


    public string Name { get; }
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

    protected override void Dispose(bool disposing)
    {
        _source.Dispose();

        base.Dispose(disposing);
    }
}
