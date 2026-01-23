using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(CapsuleCollider))]

[RequireComponent(typeof(Outline))]
[RequireComponent(typeof(ObjectState))]

public class ItemPickUp : MonoBehaviour
{

    public bool canPickUp = false;

    public ItemData_SO itemData;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
           if(canPickUp)
            {
                //装备武器
                //GameManager.Instance.playerStats.EquipWeapon(itemData);//不应该在这里直接装备

                //添加到背包
                InventoryManager.Instance.inventoryData.AddItem(itemData, itemData.itemCount);
                InventoryManager.Instance.inventoryUI.RefreshUI();

                ////删除场景中的物品
                //Destroy(gameObject);
                //不直接销毁物品，而是将其设置为不可见，并在ObjectState里记录状态
                //以免在重新加载场景时物品重新生成
                transform.GetComponent<ObjectState>().UpdateActive(false);

                //关闭交互提示UI
                GameManager.Instance.playerStats.gameObject.GetComponent<InteractUI>().
                    UpdateHintUI(false, "");
            }

        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            canPickUp = true;
            GameManager.Instance.playerStats.gameObject.GetComponent<InteractUI>().UpdateHintUI(canPickUp, "按E拾取");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.gameObject.CompareTag("Player"))
        {
            canPickUp = false;
            GameManager.Instance.playerStats.gameObject.GetComponent<InteractUI>().UpdateHintUI(canPickUp, "");
        }
    }

}
