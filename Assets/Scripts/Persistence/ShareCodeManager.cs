using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using UnityEngine;

public class ShareCodeManager : MonoBehaviour
{
    [SerializeField] private DungeonSerializer dungeonSerializer;
    [SerializeField] private string shareCodeVersionPrefix = "BVD1:";

    public string ExportCurrentDungeonCode(string dungeonName = "shared_dungeon")
    {
        DungeonSaveData saveData = dungeonSerializer.CaptureLayout(dungeonName);
        string json = JsonUtility.ToJson(saveData, false);
        string compressed = CompressToBase64(json);
        return $"{shareCodeVersionPrefix}{compressed}";
    }

    public bool TryImportCode(string shareCode, out DungeonSaveData data, out string message)
    {
        data = null;
        message = string.Empty;

        if (string.IsNullOrWhiteSpace(shareCode))
        {
            message = "Share code is empty.";
            return false;
        }

        string trimmed = shareCode.Trim();
        if (!trimmed.StartsWith(shareCodeVersionPrefix, StringComparison.Ordinal))
        {
            message = $"Unsupported share code version. Expected {shareCodeVersionPrefix}";
            return false;
        }

        string payload = trimmed.Substring(shareCodeVersionPrefix.Length);
        string json;
        try
        {
            json = DecompressFromBase64(payload);
        }
        catch
        {
            message = "Share code payload is invalid.";
            return false;
        }

        if (!dungeonSerializer.TryFromJson(json, out data))
        {
            message = "Share code data is corrupted.";
            return false;
        }

        return true;
    }

    public bool ApplyImportedDungeon(DungeonSaveData data, out string message)
    {
        return dungeonSerializer.ApplyLayout(data, out message);
    }

    private static string CompressToBase64(string source)
    {
        byte[] inputBytes = Encoding.UTF8.GetBytes(source);
        using MemoryStream outputStream = new();
        using (GZipStream gzip = new(outputStream, CompressionMode.Compress))
        {
            gzip.Write(inputBytes, 0, inputBytes.Length);
        }

        return Convert.ToBase64String(outputStream.ToArray());
    }

    private static string DecompressFromBase64(string base64)
    {
        byte[] inputBytes = Convert.FromBase64String(base64);
        using MemoryStream inputStream = new(inputBytes);
        using GZipStream gzip = new(inputStream, CompressionMode.Decompress);
        using StreamReader reader = new(gzip, Encoding.UTF8);
        return reader.ReadToEnd();
    }
}
