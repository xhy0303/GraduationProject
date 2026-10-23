using System;
using System.Collections.Generic;
using UnityEngine;

// Referenced from Resources so every saved item is available before its scene loads.
public sealed class ItemCatalog : ScriptableObject
{
    public List<ItemData_SO> items = new List<ItemData_SO>();

    public Dictionary<string, ItemData_SO> CreateLookup()
    {
        var result = new Dictionary<string, ItemData_SO>(StringComparer.Ordinal);
        foreach (var item in items)
        {
            if (item == null || string.IsNullOrEmpty(item.PersistentId) || result.ContainsKey(item.PersistentId))
                throw new InvalidOperationException("物品目录存在空引用、空 ID 或重复 ID。");
            result.Add(item.PersistentId, item);
        }
        return result;
    }
}
