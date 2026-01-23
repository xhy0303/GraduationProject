using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    public GameObject healthUIPrefab;
    public GameObject healthBarPos;

    public bool alwaysVisible;
    public float visibleTime = 3f;
    private float remainVisibleTime;

    Image healthSlider;

    Transform UIBar;
    Transform camPos;

    CharacterStats characterStats;

    private void Awake()
    {
        characterStats = GetComponent<CharacterStats>();
        characterStats.UpdateHealthBarOnAttack += UpdateHealthBar;
    }

    private void OnEnable()
    {
        camPos = Camera.main.transform;

        foreach (Canvas canvas in FindObjectsOfType<Canvas>())
        {
            //if (canvas.renderMode == RenderMode.WorldSpace)
            //    //仅有血条UI为World Space模式
            if (canvas.name == "HealthBarCanvas") 
            {
                UIBar = Instantiate(healthUIPrefab, canvas.transform).transform;
                healthSlider = UIBar.GetChild(0).GetComponent<Image>();
                UIBar.gameObject.SetActive(alwaysVisible);
            }
        }

    }


    private void UpdateHealthBar(int currentHealth, int maxHealth)
    {
        if (currentHealth <= 0)
        {
            Destroy(UIBar.gameObject);
            return;
        }
        UIBar.gameObject.SetActive(true);
        remainVisibleTime = visibleTime;

        float healthPercent = (float)currentHealth / maxHealth;
        healthSlider.fillAmount = healthPercent;
    }

    private void LateUpdate()
        //在所有的update方法结束后才执行
    {
        if (UIBar != null)
        {
            UIBar.position = healthBarPos.transform.position;
            UIBar.forward = -camPos.forward;

            if (remainVisibleTime <= 0 && !alwaysVisible) 
            {
                UIBar.gameObject.SetActive(false);
            }
            else
            {
                remainVisibleTime -= Time.deltaTime;
            }
        }
    }
}
