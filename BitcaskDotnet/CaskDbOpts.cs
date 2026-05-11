namespace BitcaskDotnet;

public class CaskDbOpts<TData>(ISerializer<TData> serializer)
{
    public required string DatabaseDirectory { get; set; }
    public int DataFileSizeThresholdInBytes { get; init; } = 16 * 1024 * 1024;
    public bool SoftDeleteDataFiles { get; init; } = true;
    public bool UseSynchronousWrites { get; init; } = false;

    public bool UseMemoryMappedFiles { get; init; } = false;

    public ISerializer<TData> Serializer => serializer;
}

public class CaskDbOpts : CaskDbOpts<string>
{
    public CaskDbOpts() : base(new StringSerializer())
    {
    }
}