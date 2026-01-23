using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerStats : CharacterStats
{

    public enum ExposureState { Peace, Alert, Chase };
    //角色的状态

    public enum PlayerShape { Human, Yokai, Invisiable };

    [Header("Player")]
    public RuntimeAnimatorController baseAnimator;

    public PlayerShape currentShape;
    public bool EagleVision;

    public ExposureState exposureState = ExposureState.Peace;
    public float alertTimer;
    public float remainAlertTime;

    [Header("Weapon")]

    public EquipmentData_SO weaponData;
    public EquipmentData_SO shieldData;
    public EquipmentData_SO accessoryData;


    //public Transform weaponSlot;
    //public Transform shieldSlot;
    //切换形态以后Slot位置会时常改变，所以不在此处保存



    #region Euqipment

    public void ChangeWeapon(ItemData_SO weapon)
    {
        bool isTwoHanded = (weapon.itemName == "砍柴斧"
            || weapon.itemName == "战士之斧"
            || weapon.itemName == "重锤"
            || weapon.itemName == "巨剑");

        UnEquipWeapon(); // 传递参数，告诉卸下武器时是否需要恢复盾牌槽
        EquipWeapon(weapon);

        if (isTwoHanded)
        {
            // 装备双手武器时卸下盾牌，同时还原数据库
            UnEquipShield();
            var shield = InventoryManager.Instance.shieldData.inventoryItems[0].itemData;
            if (shield != null)
            {
                InventoryManager.Instance.shieldData.inventoryItems[0].itemData = null;
                InventoryManager.Instance.shieldData.inventoryItems[0].num = 0;
                InventoryManager.Instance.inventoryData.AddItem(shield, 1);
                InventoryManager.Instance.inventoryUI.RefreshUI();
                InventoryManager.Instance.shieldUI.RefreshUI();
            }

            if (GameManager.Instance.playerStats.currentShape == PlayerShape.Human)
            {
                InventoryManager.Instance.ChangeShieldSlot(false);
            }
        }


    }

    public void EquipWeapon(ItemData_SO weaponData)
    {
        if (weaponData.weaponPrefab != null)
        {
            //更新属性
            this.weaponData = weaponData.weaponData;
            //切换动画
            GetComponent<Animator>().runtimeAnimatorController = weaponData.weaponAnimator;
            Transform weaponSlot = transform.GetChild(2).GetComponent<ShapeShiftController>().weaponSlot;
            GameObject newWeapon = Instantiate(weaponData.weaponPrefab, weaponSlot);
        }
    }
    public void UnEquipWeapon()
    {
        // 还原属性
        this.weaponData = null;
        // 还原动画
        GetComponent<Animator>().runtimeAnimatorController = baseAnimator;

        if (transform.GetChild(2).GetComponent<ShapeShiftController>().weaponSlot.transform.childCount != 0)
        {
            for (int i = 0; i < transform.GetChild(2).GetComponent<ShapeShiftController>().weaponSlot.transform.childCount; i++)
            {
                if (!transform.GetChild(2).GetComponent<ShapeShiftController>().weaponSlot.transform.GetChild(i).CompareTag("Player"))
                {
                    Destroy(transform.GetChild(2).GetComponent<ShapeShiftController>().
                        weaponSlot.transform.GetChild(i).gameObject);
                }
            }
        }

        // 根据参数决定是否还原盾牌槽
        if (GameManager.Instance.playerStats.currentShape == PlayerShape.Human)
        {
            InventoryManager.Instance.ChangeShieldSlot(true);
        }
    }




    public void ChangeShield(ItemData_SO weapon)
    {
        UnEquipShield();
        EquipShield(weapon);
    }
    public void EquipShield(ItemData_SO shieldData)
    {
        if (shieldData.weaponPrefab != null)
        {
            //更新属性
            this.shieldData = shieldData.weaponData;

            Instantiate(shieldData.weaponPrefab, transform.GetChild(2).GetComponent<ShapeShiftController>().shieldSlot);
        }


    }
    public void UnEquipShield()
    {
        this.shieldData = null;
        if (transform.GetChild(2).GetComponent<ShapeShiftController>().shieldSlot.transform.childCount != 0)
        {
            for (int i = 0; i < transform.GetChild(2).GetComponent<ShapeShiftController>().shieldSlot.transform.childCount; i++)
            {
                if (!transform.GetChild(2).GetComponent<ShapeShiftController>().shieldSlot.transform.GetChild(i).CompareTag("Player"))
                {
                    Destroy(transform.GetChild(2).GetComponent<ShapeShiftController>().
                        shieldSlot.transform.GetChild(i).gameObject);
                }
            }
        }
    }

    public void ChangeAccessory(ItemData_SO accessory)
    {   
        //切换饰品时还原武器和盾牌槽
        UnEquipWeapon();
        var weapon = InventoryManager.Instance.weaponData.inventoryItems[0].itemData;
        if (weapon != null)
        {
            InventoryManager.Instance.weaponData.inventoryItems[0].itemData = null;
            InventoryManager.Instance.weaponData.inventoryItems[0].num = 0;
            InventoryManager.Instance.inventoryData.AddItem(weapon, 1);
            InventoryManager.Instance.inventoryUI.RefreshUI();
            InventoryManager.Instance.weaponUI.RefreshUI();
        }


        UnEquipShield();
        var shield = InventoryManager.Instance.shieldData.inventoryItems[0].itemData;
        if (shield != null)
        {
            InventoryManager.Instance.shieldData.inventoryItems[0].itemData = null;
            InventoryManager.Instance.shieldData.inventoryItems[0].num = 0;
            InventoryManager.Instance.inventoryData.AddItem(shield, 1);
            InventoryManager.Instance.inventoryUI.RefreshUI();
            InventoryManager.Instance.shieldUI.RefreshUI();
        }

        if (GameManager.Instance.playerStats.currentShape == PlayerShape.Human)
        {
            InventoryManager.Instance.ChangeShieldSlot(false);
        }

        UnEquipAccessory();
        EquipAccessory(accessory);

        

    }
    public void EquipAccessory(ItemData_SO accessoryData)
    {
        //改变外形在PlayerController中
        switch (accessoryData.itemName)
        {
            case "隐形帽":
                GameManager.Instance.playerStats.gameObject.GetComponent<PlayerController>().
                    ShapeShift("隐形");

                break;

            case "幻形之戒":
                GameManager.Instance.playerStats.gameObject.GetComponent<PlayerController>().
                    ShapeShift("兽人");
                break;

            case "鹰眼宝石":
                GameManager.Instance.playerStats.EagleVision = true;
                break;
        }
        //InventoryManager.Instance.weaponUI.RefreshUI();
        //InventoryManager.Instance.shieldUI.RefreshUI();

    }
    public void UnEquipAccessory()
    {
        this.accessoryData = null;

        //还原鹰眼视觉
        GameManager.Instance.playerStats.EagleVision = false;

        //还原外形
        GameManager.Instance.playerStats.gameObject.GetComponent<PlayerController>().
                    ShapeShift("人形");

        //InventoryManager.Instance.weaponUI.RefreshUI();
        //InventoryManager.Instance.shieldUI.RefreshUI();
    }

    override public int GetMinDamage()
    {
        if (InventoryManager.Instance.accessoryData.inventoryItems[0].itemData == null)
        {
            if (weaponData != null)
                return characterData.baseAttack + weaponData.minDamage;
            else
                return characterData.baseAttack;
        }
        else
        {
            if (InventoryManager.Instance.accessoryData.inventoryItems[0].itemData.itemName == "力量之戒")
            {
                if (weaponData != null)
                    return (characterData.baseAttack + weaponData.minDamage) * 5;
                else
                    return characterData.baseAttack * 5;
            }
            else
            {
                if (weaponData != null)
                    return characterData.baseAttack + weaponData.minDamage;
                else
                    return characterData.baseAttack;
            }
        }
    }

    override public int GetMaxDamage()
    {
        if (InventoryManager.Instance.accessoryData.inventoryItems[0].itemData == null)
        {
            if (weaponData != null)
                return characterData.baseAttack + weaponData.maxDamage;
            else
                return characterData.baseAttack;
        }
        else
        {
            if (InventoryManager.Instance.accessoryData.inventoryItems[0].itemData.itemName == "力量之戒")
            {
                if (weaponData != null)
                    return (characterData.baseAttack + weaponData.maxDamage) * 5;
                else
                    return characterData.baseAttack * 5;
            }
            else
            {
                if (weaponData != null)
                    return characterData.baseAttack + weaponData.maxDamage;
                else
                    return characterData.baseAttack;
            }
        }
    }

    override public int GetDefense()
    {
        if (InventoryManager.Instance.accessoryData.inventoryItems[0].itemData == null)
        {
            if (shieldData != null)
                return characterData.baseDefense + shieldData.defense;
            else
                return characterData.baseDefense;
        }
        else
        {
            if (InventoryManager.Instance.accessoryData.inventoryItems[0].itemData.itemName == "力量之戒")
            {
                if (shieldData != null)
                    return (characterData.baseDefense + shieldData.defense) * 5;
                else
                    return characterData.baseDefense * 5;
            }
            else
            {
                if (shieldData != null)
                    return characterData.baseDefense + shieldData.defense;
                else
                    return characterData.baseDefense;
            }
        }
    }

    public float GetAttackRange()
    {
        if (weaponData == null)
        {
            return characterData.attackRange;
        }
        else
        {
            return weaponData.attcakRange;
        }
    }

    public float GetCoolDown()
    {
        if (weaponData == null)
            return characterData.coolDown;
        else
            return weaponData.coolDown;
    }

    public float GetCriticalChance()
    {
        if (weaponData == null)
            return characterData.criticalChance;
        else
            return weaponData.criticalChance;
    }
    public float GetCriticalMultiplier()
    {
        if (weaponData == null)
        {
            return characterData.criticalMultiplier;
        }
        else
        {
            return weaponData.criticalMultiplier;
        }
    }
    #endregion

    #region 使用道具

    public void ApplyHealth(int health)
    {
        CurrentHealth = Mathf.Min(CurrentHealth + health, MaxHealth);
    }

    #endregion

    public void ChangeExposure(PlayerStats.ExposureState state, float totalAlertTime = 0)
    {
        if (SceneManager.GetActiveScene().name != "ThirdFloor")
        {
            switch (state)
            {
                case PlayerStats.ExposureState.Peace:
                    exposureState = PlayerStats.ExposureState.Peace;
                    break;

                case PlayerStats.ExposureState.Alert:
                    if (exposureState == PlayerStats.ExposureState.Peace)
                    {
                        exposureState = PlayerStats.ExposureState.Alert;
                        alertTimer = totalAlertTime;
                        remainAlertTime = alertTimer;
                    }
                    break;

                case PlayerStats.ExposureState.Chase:
                    exposureState = PlayerStats.ExposureState.Chase;
                    break;
            }
        }
        else
        {
            exposureState = ExposureState.Chase;
        }

        bool isIndoor = SceneManager.GetActiveScene().name != "Begin"
                        && SceneManager.GetActiveScene().name != "Main";
        BGMManager.Instance.UpdateBGM(isIndoor, exposureState);

    }

}
