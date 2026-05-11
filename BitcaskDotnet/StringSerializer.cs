namespace BitcaskDotnet;

public class StringSerializer : ISerializer<string>
{
    public string Deserialize(Span<byte> bytes)
        => System.Text.Encoding.UTF8.GetString(bytes);

    public int SerializeTo(string value, Span<byte> bytes)
        => System.Text.Encoding.UTF8.GetBytes(value, bytes);
}
