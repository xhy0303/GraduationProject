using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectState:MonoBehaviour
//记录可交互物品（包括敌人）在同一次游戏中同一场景内的状态
//避免重复加载（如被击败的敌人在重新加载场景后复活）
{
    public int objIndex;
    public string objName;
    public bool isActive = true;//物品是否可见

    int currentGame;
    string objID;

    public void GenerateID()
    {
        currentGame = GameManager.Instance.gameData.gameTime;
        objID = currentGame.ToString() + "_" + objName + "_" + objIndex.ToString();
    }

    public void UpdateActive(bool isActive, bool isEnemy = false)
    {
        this.isActive = isActive;
        if(isEnemy)
        {
            //如果是敌人，先等待两秒，再设置为不可见
            //这样可以避免敌人被击败后立刻消失，给玩家一个反馈
            //可以考虑使用协程

            //gameObject.SetActive(isActive);

            StartCoroutine(DisableAfterDelay(2f));
        }
        else
        {
            if (!transform.GetComponent<ChestController>())
                //如果不是敌人且不是宝箱，设置为不可见
                gameObject.SetActive(isActive);
            else
            {
                gameObject.GetComponent<ChestController>().isOpen = !isActive;
            }
        }
    }
    private IEnumerator DisableAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        gameObject.SetActive(false);
    }
}
