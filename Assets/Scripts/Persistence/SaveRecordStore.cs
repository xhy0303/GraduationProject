using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

// Keep the last verified record for recovery. The legacy keys are never overwritten.
public static class SaveRecordStore
{
    public const string Prefix = "OneMoreTime.Save.v2.";
    private const string IndexKey = Prefix + "keys";
    private const string IgnoreLegacyKey = Prefix + "ignoreLegacy";
    private const string GenerationKey = Prefix + "generation";
    [Serializable] private sealed class Record { public int version; public string kind; public string payload; public string checksum; }
    [Serializable] private sealed class KeyIndex { public List<string> keys = new List<string>(); }
    public static bool CanReadLegacy => PlayerPrefs.GetInt(IgnoreLegacyKey, 0) == 0;
    private static string ActivePrefix => Prefix + PlayerPrefs.GetString(GenerationKey, "initial") + ".";
    public static string PrimaryKey(string key) => ActivePrefix + key;
    public static string BackupKey(string key) => ActivePrefix + key + ".backup";
    public static bool HasRecord(string key) => PlayerPrefs.HasKey(PrimaryKey(key)) || PlayerPrefs.HasKey(BackupKey(key));
    private static string Digest(string value)
    {
        using (var sha = SHA256.Create())
            return Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(value)));
    }
    public static void Write(string key, string kind, string payload)
    {
        var index = ReadIndex();
        if (!index.keys.Contains(key)) index.keys.Add(key);
        PlayerPrefs.SetString(IndexKey, JsonUtility.ToJson(index));
        var previous = PlayerPrefs.GetString(PrimaryKey(key), "");
        if (Unwrap(previous, kind, out _)) PlayerPrefs.SetString(BackupKey(key), previous);
        var record = new Record { version = 2, kind = kind, payload = payload, checksum = Digest(kind + "\n" + payload) };
        PlayerPrefs.SetString(PrimaryKey(key), JsonUtility.ToJson(record));
    }
    public static bool TryRead(string key, string kind, bool backup, out string payload)
    {
        return Unwrap(PlayerPrefs.GetString(backup ? BackupKey(key) : PrimaryKey(key), ""), kind, out payload);
    }
    private static bool Unwrap(string json, string kind, out string payload)
    {
        payload = null;
        if (string.IsNullOrEmpty(json)) return false;
        try
        {
            var record = JsonUtility.FromJson<Record>(json);
            if (record == null || record.version != 2 || record.kind != kind || record.payload == null
                || record.checksum != Digest(kind + "\n" + record.payload)) return false;
            payload = record.payload;
            return true;
        }
        catch (ArgumentException) { return false; }
    }
    private static KeyIndex ReadIndex()
    {
        try
        {
            var value = JsonUtility.FromJson<KeyIndex>(PlayerPrefs.GetString(IndexKey, ""));
            return value?.keys != null ? value : new KeyIndex();
        }
        catch (ArgumentException) { return new KeyIndex(); }
    }
    public static void StartNewGame()
    {
        foreach (var key in ReadIndex().keys)
        {
            PlayerPrefs.DeleteKey(PrimaryKey(key));
            PlayerPrefs.DeleteKey(BackupKey(key));
        }
        PlayerPrefs.DeleteKey(IndexKey);
        // A fresh namespace also isolates any records whose index was damaged.
        PlayerPrefs.SetString(GenerationKey, Guid.NewGuid().ToString("N"));
        PlayerPrefs.SetInt(IgnoreLegacyKey, 1);
        PlayerPrefs.Save();
    }
}
