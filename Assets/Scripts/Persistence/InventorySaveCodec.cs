using System;
using System.Collections.Generic;
using UnityEngine;

public static class InventorySaveCodec
{
    [Serializable] public sealed class Slot { public string itemId; public int count; }
    [Serializable] public sealed class Data { public int version = 1; public List<Slot> slots = new List<Slot>(); }

    public static string Encode(InventoryData_SO inventory)
    {
        var catalog = Resources.Load<ItemCatalog>("ItemCatalog");
        if (!catalog) throw new InvalidOperationException("缺少物品目录，不能保存背包。");
        var lookup = catalog.CreateLookup();
        var data = new Data();
        foreach (var slot in inventory.inventoryItems)
        {
            if (slot == null || slot.num < 0) throw new InvalidOperationException("背包槽位数据无效。");
            if (slot.itemData == null || slot.num == 0) data.slots.Add(new Slot { itemId = "", count = 0 });
            else
            {
                if (!lookup.TryGetValue(slot.itemData.PersistentId, out var registered) || registered != slot.itemData)
                    throw new InvalidOperationException("物品未登记：" + slot.itemData.name);
                data.slots.Add(new Slot { itemId = slot.itemData.PersistentId, count = slot.num });
            }
        }
        return JsonUtility.ToJson(data);
    }

    public static bool TryDecode(string json, InventoryData_SO target, out string reason)
    {
        reason = "";
        try
        {
            var data = JsonUtility.FromJson<Data>(json);
            if (data == null || data.version != 1 || data.slots == null || data.slots.Count != target.inventoryItems.Count)
            { reason = "背包版本或槽位数量不匹配"; return false; }
            var catalog = Resources.Load<ItemCatalog>("ItemCatalog");
            if (!catalog) { reason = "物品目录不可用"; return false; }
            var lookup = catalog.CreateLookup();
            var restored = new List<InventoryItem>();
            foreach (var slot in data.slots)
            {
                if (slot == null || slot.count < 0) { reason = "物品数量无效"; return false; }
                ItemData_SO item = null;
                if (!string.IsNullOrEmpty(slot.itemId) && !lookup.TryGetValue(slot.itemId, out item))
                { reason = "无法识别物品 ID：" + slot.itemId; return false; }
                if ((item == null) != (slot.count == 0)) { reason = "物品与数量不一致"; return false; }
                restored.Add(new InventoryItem { itemData = item, num = slot.count });
            }
            // Do not partially mutate the target on malformed or unknown data.
            target.inventoryItems = restored;
            return true;
        }
        catch (Exception e) when (e is ArgumentException || e is InvalidOperationException)
        { reason = e.Message; return false; }
    }

    [Serializable] private sealed class LegacyReference { public int instanceID; }
    [Serializable] private sealed class LegacySlot { public LegacyReference itemData; public int num; }
    [Serializable] private sealed class LegacyData { public List<LegacySlot> inventoryItems; }
    public static bool TryDecodeLegacy(string json, InventoryData_SO target)
    {
        try
        {
            var data = JsonUtility.FromJson<LegacyData>(json);
            if (data?.inventoryItems == null || data.inventoryItems.Count != target.inventoryItems.Count) return false;
            // Process-local IDs cannot establish item identity, even if they happen to resolve today.
            foreach (var slot in data.inventoryItems)
                if (slot == null || slot.num != 0 || (slot.itemData != null && slot.itemData.instanceID != 0)) return false;
            target.inventoryItems = data.inventoryItems.ConvertAll(s => new InventoryItem());
            return true;
        }
        catch (ArgumentException) { return false; }
    }
}
