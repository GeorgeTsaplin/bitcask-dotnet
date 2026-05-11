using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using BitcaskDotnet;
using Microsoft.Extensions.Logging.Abstractions;
using ProtoBuf;
using ProtoBuf.Meta;
using System.Text;

namespace CaskDb.Bench;

// [ShortRunJob]
[MemoryDiagnoser]
public class MemoryBenchmarkerDemo
{
    private byte[] span;
    private FileStream fs1;
    private FileStream fs2;
    private byte[] buffer;
    public string[] Guids { get; set; }


    [GlobalSetup]
    public void Setup()
    {
        span = new byte[1024];
        this.buffer = new byte[32];
        
        this.Guids = Enumerable.Range(0, 1000).Select(i => Guid.NewGuid().ToString("N")).ToArray();
        
        this.fs1 = File.Open(
            "fs1",
            FileMode.Append,
            FileAccess.Write,
            FileShare.Read
        );
        
        this.fs2 = File.Open(
            "fs2",
            FileMode.Append,
            FileAccess.Write,
            FileShare.Read
        );
    }


    // [Benchmark]
    public void FileAppend()
    {
        using (var stream = new FileStream("fs1", FileMode.Append))
        {
            stream.Write(new Span<byte>(span));
        }
    }

    // this order of magnitude better
    // [Benchmark]
    public void FileWrite()
    {
            fs1.Write(new Span<byte>(span));
    }
    
    [Benchmark]
    public void FileWrite_WithAllocation()
    {
        foreach (var guid in Guids)
        {
            fs1.Write(Encoding.UTF8.GetBytes(guid));
        }
    }
    
    // slightly better
    [Benchmark]
    public void FileWrite_WithoutAllocation()
    {
        foreach (var guid in Guids)
        {
            Encoding.UTF8.GetBytes(guid, buffer);
            fs2.Write(buffer);
        }
    }

}

public class CascDbBenchmark
{
    private const int DataAmount = 100_000;
    private const int ReadOps = 300;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private string _dbDir;
    private Dictionary<string, SampleData> _data;
    private CaskDB<SampleData> _db;
    private CaskDB<SampleData> _dbInMemoryRO;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    [GlobalSetup]
    public void Setup()
    {
        _data = Enumerable.Range(0, DataAmount)
            .ToDictionary(
                ks => ks.ToString(),
                vs => new SampleData
                {
                    Name = $"name#{vs}",
                    I18N = Enumerable.Range(0, Random.Shared.Next(1, 2))
                        .ToDictionary(ks => $"loc#{ks}", vs => Guid.NewGuid().ToString("N"))
                });

        _dbDir = $"cdb/{DateTime.UtcNow:yyyy-MM-dd--HH-mm-ss-ffff}";

        var serializer = new SampleDataSerializer();

        var db = new CaskDB<SampleData>(
            new(serializer)
            {
                DatabaseDirectory = _dbDir
            },
            NullLogger.Instance);

        foreach (var item in _data)
        {
            db.Put(item.Key, item.Value);
        }
        db.Sync();
        db.Dispose();

        _dbInMemoryRO = new CaskDB<SampleData>(
            new(serializer)
            {
                DatabaseDirectory = _dbDir,
                UseMemoryMappedFiles = true
            },
            NullLogger.Instance);

        _db = new CaskDB<SampleData>(
            new(serializer)
            {
                DatabaseDirectory = _dbDir,
            },
            NullLogger.Instance);

        _ = _dbInMemoryRO.Get(_data.First().Key);
        _ = _db.Get(_data.First().Key);
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _dbInMemoryRO.Dispose();
        _db.Dispose();

        //Directory.Delete(_dbDir);
    }

    [Benchmark(Baseline = true)]
    public void ReadFromDictionary()
    {
        for(int i = 0; i < ReadOps; i++)
        {
            var key = Random.Shared.Next(0, DataAmount).ToString();

            if (!_data.TryGetValue(key, out _))
            {
                throw new ApplicationException("Could not get value from memory");
            }
        }
    }

    [Benchmark]
    public void ReadFromBitcaskDb()
    {
        for (int i = 0; i < ReadOps; i++)
        {
            var key = Random.Shared.Next(0, DataAmount).ToString();

            _ = _db.Get(key)
                ?? throw new ApplicationException("Could not get value from DB");
        }
    }

    [Benchmark]
    public void ReadFromInMemoryBitcaskDb()
    {
        for (int i = 0; i < ReadOps; i++)
        {
            var key = Random.Shared.Next(0, DataAmount).ToString();

            _ = _dbInMemoryRO.Get(key)
                ?? throw new ApplicationException("Could not get value from in-memory DB");
        }
    }

    [ProtoContract]
    public class SampleData
    {
        [ProtoMember(1)]
        public string Name { get; set; } = null!;

        [ProtoMember(2)]
        public Dictionary<string, string> I18N { get; set; } = null!;
    }

    private class SampleDataSerializer : ISerializer<SampleData>
    {
        public SampleData Deserialize(Span<byte> bytes)
            => ProtoBuf.Serializer.Deserialize<SampleData>(bytes);

        public int SerializeTo(SampleData value, Span<byte> bytes)
        {
            // TODO: optimize
            using var ms = new MemoryStream();
            ProtoBuf.Serializer.Serialize(ms, value);
            ms.Position = 0;
            return ms.Read(bytes);
        }
    }
}


class Program
{
    static void Main(string[] args)
    {
        //var summary = BenchmarkRunner.Run<MemoryBenchmarkerDemo>();
        var summary = BenchmarkRunner.Run<CascDbBenchmark>(/*new BenchmarkDotNet.Configs.DebugInProcessConfig()*/);
    }
}