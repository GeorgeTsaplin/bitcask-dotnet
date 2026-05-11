namespace BitcaskDotnet;

public interface ISerializer<T>
{
    int SerializeTo(T value, Span<byte> bytes);

    T Deserialize(Span<byte> bytes);
}
