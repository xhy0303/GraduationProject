using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VirtualCamController : MonoBehaviour
{
    CinemachineVirtualCamera virtualCamera;
    CinemachineFramingTransposer transposer;

    private void Awake()
    {
        // 获取虚拟相机组件
        virtualCamera = GetComponent<CinemachineVirtualCamera>();

        // 确保虚拟相机组件的 Body 部分是 CinemachineTransposer
        if (virtualCamera != null)
        {
            transposer = virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
        }

    }

    private void Update()
    {
        // 如果 transposer 已经成功获取到，则控制相机距离
        if (transposer != null)
        {
            // 鼠标滚轮控制虚拟摄像机离玩家的距离
            if (Input.GetAxis("Mouse ScrollWheel") > 0f)
            {
                transposer.m_CameraDistance -= 1f;  // 向玩家靠近
            }
            else if (Input.GetAxis("Mouse ScrollWheel") < 0f)
            {
                transposer.m_CameraDistance += 1f;
            }
        }
    }
}
