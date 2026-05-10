using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace BitcaskDotnet;

class KeyDir(ILogger logger) : ConcurrentDictionary<string, FileValue>
{
    private FileOps _fileOps = new FileOps();

    public void InitializeWithDataFile(BitcaskStream dataFile)
    {
        logger.LogDebug("Initializing keyDir with dataFile {df}", dataFile.Name);

        foreach (var (key, val) in _fileOps.EnumerateFileValues(dataFile))
        {
            logger.LogDebug("Read key: {@key} value: {@value}", key, val);

            if (val.ValueSize == 0)
                this.TryRemove(key, out _);
            else
                this[key] = val;
        }
    }

    public void InitializeWithHintFile(BitcaskStream hintFile, string dataFileName)
    {
        logger.LogDebug(
            "Initializing keyDir with hintFile :{hf}, dataFileName: {dfn}",
            hintFile.Name,
            dataFileName
        );

        foreach (var (key, val) in _fileOps.EnumerateHintFileRecords(hintFile))
        {
            logger.LogDebug("Read key: {@key} value: {@value}", key, val);
            val.FileId = dataFileName;
            this[key] = val;
        }
    }

    internal void Insert(string key, FileValue val)
    {
        this[key] = val;
    }

    internal void Remove(string key)
    {
        this.TryRemove(key, out _);
    }
}
