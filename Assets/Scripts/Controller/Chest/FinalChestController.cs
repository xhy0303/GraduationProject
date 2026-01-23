using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinalChestController : ChestController
{
    protected override void Update()
    {
        if (canOpen && Input.GetKeyDown(KeyCode.E))
        {
            isOpen = true;
            
            GameManager.Instance.playerStats.GetComponent<InteractUI>().UpdateHintUI(false, "");

            GetComponent<ObjectState>().isActive = false;

            GameOverWin();

        }

        if (isOpen)
        {
            chestAnimator.SetTrigger("OpenChest");
            //只有史诗物品栏有物品时才会播放史诗动画
            chestAnimator.SetBool("Epic", isEpic);
        }
    }

    void GameOverWin()
    {
        GameOverManager.Instance.ShowGameOver(true);
        //获胜以后游戏通关次数+1
        GameManager.Instance.gameData.Win();
        //重设相关数据
        SaveManager.Instance.SetDataAfterWin();
    }

}
