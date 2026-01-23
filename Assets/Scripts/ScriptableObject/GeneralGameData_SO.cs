using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Data", menuName = "Data/General Game Data")]

public class GeneralGameData_SO : ScriptableObject
{
    public int gameTime;

    public bool CanShop(int shopTimes)
    {
        return shopTimes < gameTime;
    }

    public void Win()
    {
        gameTime++;
    }

}
