using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName ="New Data",menuName ="Data/Character Data")]
public class CharacterData_SO : ScriptableObject
{
    [Header("Basic")]
    public int maxHealth;
    public int currentHealth;

    public int baseAttack;
    public int baseDefense;

    public float attackRange;
    public float skillRange;

    public float coolDown;

    public float criticalMultiplier;
    public float criticalChance;

}
