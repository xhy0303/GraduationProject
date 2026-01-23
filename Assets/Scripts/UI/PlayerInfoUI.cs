using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInfoUI : MonoBehaviour
{
    //注意组件的类型

    Image healthSlider;
    Image expSlider;

    private void Awake()
    {
        healthSlider = transform.GetChild(0).GetChild(0).GetComponent<Image>();
        expSlider = transform.GetChild(1).GetComponent<Image>();
    }

    private void Update()
    {
        UpdateHealth();
        UpdateExposure();
    }

    void UpdateHealth()
    {
        //int currentHealth = player.GetComponent<CharaterStats>().currentHealth;
        //int maxHealth = player.GetComponent<CharaterStats>().maxHealth;

        float healthPercent = (float)GameManager.Instance.playerStats.CurrentHealth
            / GameManager.Instance.playerStats.MaxHealth;//currentHealth / maxHealth;

        healthSlider.fillAmount = healthPercent;
    }

    void UpdateExposure()
    {
        switch(GameManager.Instance.playerStats.exposureState)
        {
            case PlayerStats.ExposureState.Peace:
                expSlider.color = Color.green;
                expSlider.fillAmount = 1;
                break;

            case PlayerStats.ExposureState.Alert:
                expSlider.color= Color.yellow;

                GameManager.Instance.playerStats.remainAlertTime -=Time.deltaTime;
                expSlider.fillAmount = GameManager.Instance.playerStats.remainAlertTime 
                    / GameManager.Instance.playerStats.alertTimer;
                if (GameManager.Instance.playerStats.remainAlertTime <= 0)
                {
                    GameManager.Instance.playerStats.exposureState = PlayerStats.ExposureState.Chase;
                }
                break;

            case PlayerStats.ExposureState.Chase:
                expSlider.color = Color.red;
                expSlider.fillAmount = 1;
                break;
        }
    }

    

}
