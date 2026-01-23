using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType
{
    Useable, Weapon, Shield, Accessory
}


[CreateAssetMenu(fileName = "New Item", menuName = "Data/Item Data")]

public class ItemData_SO : ScriptableObject
{
    public ItemType itemType;

    public string itemName;
    [TextArea]
    public string itemDescription = "";

    public Sprite itemIcon;

    public int itemCount;
    public bool countable;

    [Header("UseableItem")]
    public UseableItemData_SO useableItemData;

    [Header("Weapon")]
    public GameObject weaponPrefab;
    public EquipmentData_SO weaponData;

    public AnimatorOverrideController weaponAnimator;
}

