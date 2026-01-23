using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New AttackData", menuName = "Data/Equipment Data")]

public class EquipmentData_SO : ScriptableObject
{
    public float attcakRange;

    public float coolDown;

    public int minDamage;
    public int maxDamage;

    public int defense;

    public float criticalMultiplier;
    public float criticalChance;

    public int weight;

}
