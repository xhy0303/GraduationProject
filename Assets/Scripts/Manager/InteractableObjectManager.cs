using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class ObjectStateData
{
    public int objIndex;
    public string objName;
    public bool isActive;
}

[System.Serializable]
public class InteractableObjectManagerData
{
    public List<ObjectStateData> objectStates = new List<ObjectStateData>();
}



public class InteractableObjectManager : MonoBehaviour
    //记录可交互物品（包括敌人）在同一次游戏中同一场景内的状态
    //避免重复加载（如被击败的敌人在重新加载场景后复活）
{

    public List<ObjectState> objectStates;


    private void Awake()
    {
        // 确保 objectStates 已经初始化
        if (objectStates == null)
        {
            objectStates = new List<ObjectState>();
        }

        // 先遍历所有子对象，把 ObjectState 收集到列表中（无论是否是第一次加载都这么做）
        for (int i = 0; i < transform.childCount; i++)
        {
            ObjectState objState = transform.GetChild(i).GetComponent<ObjectState>();
            if (objState != null)
            {
                // 初始化每个物品的数据
                objState.objIndex = i;
                objState.objName = transform.GetChild(i).gameObject.name;
                objState.isActive = true;
                objState.GenerateID();

                // 添加到列表中
                objectStates.Add(objState);
            }
        }

        // 尝试加载保存的数据，并更新物体状态
        LoadObjectStates();
    }

    public void SaveObjectStates()
    {
        // 新建数据容器
        InteractableObjectManagerData data = new InteractableObjectManagerData();

        // 遍历所有的 ObjectState，将数据复制进去
        foreach (var obj in objectStates)
        {
            ObjectStateData stateData = new ObjectStateData();
            stateData.objIndex = obj.objIndex;
            stateData.objName = obj.objName;
            stateData.isActive = obj.isActive;
            data.objectStates.Add(stateData);
        }

        // 构造唯一的 key（示例：场景名 + 管理器对象名 + 当前游戏次数）
        string key = SceneManager.GetActiveScene().name 
            + gameObject.name 
            + GameManager.Instance.gameData.gameTime.ToString();

        // 调用 SaveManager 保存数据
        SaveManager.Instance.Save(data, key);
        //在 Unity 中，默认的 Object 指的是 UnityEngine.Object，
        //而你的 InteractableObjectManagerData 并不继承自 UnityEngine.Object，而是普通的 C# 类。
        //将方法的参数类型从 UnityEngine.Object 改为 System.Object，
        //    这样就能接受任何普通的 C# 对象。
        //    你可以直接使用小写的 object，因为 object 就是 System.Object。
    }

    public void LoadObjectStates()
    {
        // 新建数据容器（数据会加载到这个对象上）
        InteractableObjectManagerData data = new InteractableObjectManagerData();

        // 构造与保存时相同的 key
        string key = SceneManager.GetActiveScene().name 
            + gameObject.name
            + GameManager.Instance.gameData.gameTime.ToString();

        // 调用 SaveManager 加载数据
        SaveManager.Instance.Load(data, key);

        // 遍历加载到的数据，更新场景中的 ObjectState
        foreach (var stateData in data.objectStates)
        {
            // 这里假设 objIndex 是唯一标识，可以用它来找到对应的 ObjectState
            ObjectState objState = objectStates.Find(x => x.objIndex == stateData.objIndex);
            if (objState != null)
            {
                // 根据加载的数据更新状态
                objState.UpdateActive(stateData.isActive);
            }
        }
    }



}
