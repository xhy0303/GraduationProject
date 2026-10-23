using System;
using UnityEngine;

/// <summary>
/// Preserves the project's legacy lookup order on the pinned Unity 6000.5 editor.
/// Unity's old GetInstanceID implementation returns the low signed 32 bits of EntityId.
/// Do not replace a first-object lookup with an arbitrary-object lookup: cameras,
/// canvases and Timeline directors can have multiple instances in a scene.
/// </summary>
public static class LegacyObjectLookup
{
    public static T[] FindAll<T>() where T : UnityEngine.Object
    {
        T[] objects = UnityEngine.Object.FindObjectsByType<T>();
        Array.Sort(objects, (left, right) => LegacyId(left).CompareTo(LegacyId(right)));
        return objects;
    }

    public static T FindFirst<T>() where T : UnityEngine.Object
    {
        T[] objects = FindAll<T>();
        return objects.Length == 0 ? null : objects[0];
    }

    private static int LegacyId(UnityEngine.Object value)
    {
        return unchecked((int)EntityId.ToULong(value.GetEntityId()));
    }
}
