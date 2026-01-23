using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : Manager<InventoryManager>
{
    public class DragData
    {
        public SlotHolder originalHolder;
        public RectTransform originalParent;
    }

    [Header("InventoryData")]
    public InventoryData_SO inventoryTemplate;
    public InventoryData_SO inventoryData;

    public InventoryData_SO weaponTemplate;
    public InventoryData_SO weaponData;

    public InventoryData_SO shieldTemplate;
    public InventoryData_SO shieldData;

    public InventoryData_SO accessoryTemplate;
    public InventoryData_SO accessoryData;

    public InventoryData_SO accessoryShopTemplate;
    public InventoryData_SO accessoryShopData;

    [Header("Container")]
    public ContainerUI inventoryUI;
    public ContainerUI weaponUI;
    public ContainerUI shieldUI;
    public ContainerUI accessoryUI;

    public ContainerUI accessoryShopUI;

    [Header("Drag Canvas")]
    public Canvas dragCanvas;
    public DragData currentDrag;

    [Header("UI Panel")]
    public GameObject bagPanel;
    public GameObject shopPanel;

    bool isOpen = false;

    [Header("StatsText")]
    public Text healthText;
    public Text attackText;
    public Text defenseText;

    [Header("ToolTip")]
    public ItemToolTip toolTip;

    [Header("GameData")]
    public Text gameData;

    protected override void Awake()
    {
        base.Awake();
        if (inventoryTemplate != null)
        {
            inventoryData = Instantiate(inventoryTemplate);
        }
        if (weaponTemplate != null) 
        {
            weaponData = Instantiate(weaponTemplate);
        }
        if (shieldTemplate != null) 
        {
            shieldData = Instantiate(shieldTemplate);
        }
        if (accessoryTemplate != null) 
        {
            accessoryData = Instantiate(accessoryTemplate);
        }
        if (accessoryShopTemplate != null)
        {
            accessoryShopData = Instantiate(accessoryShopTemplate);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            isOpen = !isOpen;

        }
        bagPanel.SetActive(isOpen);

        //打开背包时暂停游戏
        //后续打开设置菜单时也暂停游戏
        //开始对话时也暂停游戏
        GameManager.Instance.TogglePause(isOpen
            || SettingMenu.Instance.settingPanel.activeSelf
            || DialogueUI.Instance.dialoguePanel.activeSelf);

        UpdateStatsText(GameManager.Instance.playerStats.MaxHealth,
            GameManager.Instance.playerStats.GetMinDamage(),
            GameManager.Instance.playerStats.GetMaxDamage(),
            GameManager.Instance.playerStats.GetDefense());

        gameData.text = "×" + GameManager.Instance.gameData.gameTime;

    }

    public void UpdateEquipmentSlot(PlayerStats.PlayerShape shape)
    //随着玩家形态的切换，更新装备栏的可用性
    {
        //switch (GameManager.Instance.playerStats.currentShape)
        switch (shape) 
        {
            case PlayerStats.PlayerShape.Human:
                weaponUI.transform.GetChild(0).gameObject.SetActive(true);
                shieldUI.transform.GetChild(0).gameObject.SetActive(true);
                accessoryUI.transform.GetChild(0).gameObject.SetActive(true);
                break;
            case PlayerStats.PlayerShape.Invisiable:
                weaponUI.transform.GetChild(0).gameObject.SetActive(true);
                shieldUI.transform.GetChild(0).gameObject.SetActive(true);
                accessoryUI.transform.GetChild(0).gameObject.SetActive(true);
                break;
            case PlayerStats.PlayerShape.Yokai:
                weaponUI.transform.GetChild(0).gameObject.SetActive(false);
                shieldUI.transform.GetChild(0).gameObject.SetActive(false);
                accessoryUI.transform.GetChild(0).gameObject.SetActive(true);
                break;

        }
    }

    public void ChangeShieldSlot(bool enable)
    {
        shieldUI.transform.GetChild(0).gameObject.SetActive(enable);
    }

    public void CloseBag()
    {
        isOpen = false;
    }
    
    public void UpdateStatsText(int health, int minDamage, int maxDamage, int defense)
    {
        healthText.text = health.ToString();
        attackText.text = minDamage + " - " + maxDamage;
        defenseText.text = defense.ToString();
    }

    public void RefreshUI()
    {
        accessoryUI.RefreshUI();
        inventoryUI.RefreshUI();
        weaponUI.RefreshUI();
        shieldUI.RefreshUI();
        
        accessoryShopUI.RefreshUI();
    }

    public void SaveInventoryData()
    {
        SaveManager.Instance.Save(inventoryData, inventoryData.name);
        SaveManager.Instance.Save(weaponData, weaponData.name);
        SaveManager.Instance.Save(shieldData, shieldData.name);
        SaveManager.Instance.Save(accessoryData, accessoryData.name);
        SaveManager.Instance.Save(accessoryShopData, accessoryShopData.name);
    }

    public void LoadInventoryData()
    {
        SaveManager.Instance.Load(inventoryData, inventoryData.name);
        SaveManager.Instance.Load(weaponData, weaponData.name);
        SaveManager.Instance.Load(shieldData, shieldData.name);
        SaveManager.Instance.Load(accessoryData, accessoryData.name);
        SaveManager.Instance.Load(accessoryShopData, accessoryShopData.name);
    }


    #region 检查拖拽物品是否在slot范围内

    public bool CheckInventoryUI(Vector3 position)
    {
        for (int i = 0; i < inventoryUI.slotHolders.Length; i++)
        {
            RectTransform t = inventoryUI.slotHolders[i].transform as RectTransform;

            if (RectTransformUtility.RectangleContainsScreenPoint(t, position))
                return true;
        }
        return false;
    }

    public bool CheckWeaponUI(Vector3 position)
    {
        for (int i = 0; i < weaponUI.slotHolders.Length; i++)
        {
            RectTransform t = weaponUI.slotHolders[i].transform as RectTransform;

            if (RectTransformUtility.RectangleContainsScreenPoint(t, position))
                return true;
        }
        return false;
    }

    public bool CheckShieldUI(Vector3 position)
    {
        for (int i = 0; i < shieldUI.slotHolders.Length; i++)
        {
            RectTransform t = shieldUI.slotHolders[i].transform as RectTransform;

            if (RectTransformUtility.RectangleContainsScreenPoint(t, position))
                return true;
        }
        return false;
    }

    public bool CheckAccessoryUI(Vector3 position)
    {
        for (int i = 0; i < accessoryUI.slotHolders.Length; i++)
        {
            RectTransform t = accessoryUI.slotHolders[i].transform as RectTransform;

            if (RectTransformUtility.RectangleContainsScreenPoint(t, position))
                return true;
        }
        return false;
    }

    public bool CheckShopUI(Vector3 position)
    {
        for (int i = 0; i < accessoryShopUI.slotHolders.Length; i++)
        {
            RectTransform t = accessoryShopUI.slotHolders[i].transform as RectTransform;

            if (RectTransformUtility.RectangleContainsScreenPoint(t, position))
                return true;
        }
        return false;
    }


    #endregion


}
