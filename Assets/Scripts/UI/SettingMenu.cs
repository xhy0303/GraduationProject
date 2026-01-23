using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SettingMenu : Manager<SettingMenu>
{

    public GameObject settingPanel;
    public Slider bgmVolumnSlider;

    public Button saveGameBut;
    public Button back2MainBut;

    bool isOpen = false;

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this);

        bgmVolumnSlider.onValueChanged.AddListener(BGMManager.Instance.OnVolumeChanged);
        bgmVolumnSlider.value = BGMManager.Instance.GetVolume();

        saveGameBut.onClick.AddListener(SaveManager.Instance.SaveAllData);
        back2MainBut.onClick.AddListener(SceneController.Instance.Back2Home);
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            isOpen = !isOpen;
        }

        settingPanel.SetActive(isOpen);
    }


}
