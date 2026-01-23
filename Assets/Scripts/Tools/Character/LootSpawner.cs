using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootSpawner : MonoBehaviour
{
    [System.Serializable]

    public class LootItem
    {
        public GameObject item;

        [Range(0f, 1f)]
        public float weight;

    }

    public LootItem[] lootItems;

    public void SpawnLoot()
    {
        float value = Random.Range(0f, 1f);

        for (int i = 0; i < lootItems.Length; i++)
        {
            if (value < lootItems[i].weight)
            {
                GameObject obj = Instantiate(lootItems[i].item);
                obj.transform.position = transform.position;
            }
        }
    }
}
