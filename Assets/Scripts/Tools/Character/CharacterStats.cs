using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterStats : MonoBehaviour
{
    [Header("Basic")]
    
    public CharacterData_SO templateData;
    public CharacterData_SO characterData;
    
    public bool isCritical;

    public event Action<int, int> UpdateHealthBarOnAttack;
    


    #region read from characterData

    public int MaxHealth
    {
        get
        {
            if (characterData == null)
                return 0;
            else
                return characterData.maxHealth;
        }
        set
        {
            characterData.maxHealth = value;
        }
    }

    public int CurrentHealth
    {
        get
        {
            if (characterData == null)
                return 0;
            else
                return characterData.currentHealth;
        }
        set
        {
            characterData.currentHealth = value;
        }
    }

    public int BaseAttack
    {
        get
        {
            if (characterData == null)
                return 0;
            else
                return characterData.baseAttack;
        }
        set
        {
            characterData.baseAttack = value;
        }
    }

    public int BaseDefense
    {
        get
        {
            if (characterData == null)
                return 0;
            else
                return characterData.baseDefense;
        }
        set
        {
            characterData.baseDefense = value;
        }
    }

    public float AttackRange
    {
        get
        {
            if (characterData == null)
                return 0;
            else
                return characterData.attackRange;
        }
        set
        {
            characterData.attackRange = value;
        }
    }
    public float SkillRange
    {
        get
        {
            if (characterData == null)
                return 0;
            else
                return characterData.skillRange;
        }
        set
        {
            characterData.skillRange = value;
        }
    }

    public float CoolDown
    {
        get
        {
            if (characterData == null)
                return 0;
            else
                return characterData.coolDown;
        }
        set
        {
            characterData.coolDown = value;
        }
    }

    public float CriticalMultiplier
    {
        get
        {
            if (characterData == null)
                return 0;
            else
                return characterData.criticalMultiplier;
        }
        set
        {
            characterData.criticalMultiplier = value;
        }
    }
    public float CriticalChance
    {
        get
        {
            if (characterData == null)
                return 0;
            else
                return characterData.criticalChance;
        }
        set
        {
            characterData.criticalChance = value;
        }
    }
    #endregion

    private void Awake()
    {
        if (templateData != null) 
        {
            characterData = Instantiate(templateData);
        }

        //游戏一开始更新血条UI
        UpdateHealthBarOnAttack?.Invoke(CurrentHealth,MaxHealth);
    }

    public void TakeDamage(CharacterStats attacker, CharacterStats defender)
        //角色之间的伤害计算
        //永远是defender调用该方法
    {

        int damage = UnityEngine.Random.Range(attacker.GetMinDamage(), attacker.GetMaxDamage())
            - defender.GetDefense();

        if (attacker.isCritical)
        {
            damage = (int)(CriticalMultiplier * damage);

            defender.GetComponent<Animator>().SetTrigger("Hit");
        }
        damage = Mathf.Max(damage, 1);

        if (defender.gameObject.GetComponent<GolemController>())
            //武器攻击石头人伤害减半
        {
            damage /= 2;
        }

        CurrentHealth = Mathf.Max(defender.CurrentHealth - damage, 0);

        //血条UI更新
        UpdateHealthBarOnAttack?.Invoke(CurrentHealth, MaxHealth);

        if(defender.GetComponent<EnemyController>())
        //敌人受到攻击后直接进入chase状态
        {
            defender.GetComponent<EnemyController>().enemyStates = EnemyStates.Chase;
            defender.transform.LookAt(attacker.transform.position);
        }

    }

    public void TakeDamage(int damage, CharacterStats defender)
    {
        int currentDamage = Mathf.Max(damage - defender.GetDefense(), 1);

        if (defender.gameObject.GetComponent<GolemController>())
            //石头反击石头人十倍伤害
        {
            currentDamage *= 10;
        }

        CurrentHealth = Mathf.Max(CurrentHealth - currentDamage, 0);

        UpdateHealthBarOnAttack?.Invoke(CurrentHealth, MaxHealth);

        if (defender.GetComponent<EnemyController>())
        //敌人受到攻击后直接进入chase状态
        {
            defender.GetComponent<EnemyController>().enemyStates = EnemyStates.Chase;
        }

    }

    public virtual int GetMaxDamage()
    {
        return characterData.baseAttack;
    }

    public virtual int GetMinDamage()
    {
        return characterData.baseAttack;
    }

    public virtual int GetDefense()
    {
        return characterData.baseDefense;
    }



}
