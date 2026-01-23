using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class InteractUI : MonoBehaviour
{
    public Button hintUIPrefab;
    public GameObject hintUIPos;

    Transform hintUI;
    Transform camPos;
    Text hintText;

    CharacterStats characterStats;


    private void Awake()
    {
        characterStats = GetComponent<CharacterStats>();
    }

    private void OnEnable()
    {
        camPos = Camera.main.transform;

        foreach (Canvas canvas in FindObjectsOfType<Canvas>())
        {
            if (canvas.name == "InteractUICanvas")
            {
                hintUI = Instantiate(hintUIPrefab, canvas.transform).transform;
                hintUI.gameObject.SetActive(false);
                hintText=hintUI.GetChild(0).GetComponent<Text>();
            }
        }

    }


    public void UpdateHintUI(bool isOpen, string hintText)
    {
        hintUI.gameObject.SetActive(isOpen);
        this.hintText.text = hintText;
    }

    private void LateUpdate()
    //在所有的update方法结束后才执行
    {
        if (hintUI != null)
        {
            hintUI.position = hintUIPos.transform.position;

            hintUI.forward = camPos.forward;
            hintUI.rotation = camPos.rotation;
        }
    }

}
