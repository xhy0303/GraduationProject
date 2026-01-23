using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemToolTip : MonoBehaviour
{
    public Text itemName;
    public Text itemInfo;

    RectTransform rectTransform;

    public void SetUpItemToolTip(ItemData_SO item)
    {
        itemName.text = item.itemName;
        itemInfo.text = item.itemDescription;
    }

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
        UpdatePosition();
    }

    private void Update()
    {
        UpdatePosition();
    }


    public void UpdatePosition()
    {
        Vector3 mousePos = Input.mousePosition;

        Vector3[] corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);

        float width = corners[3].x - corners[0].x;
        float height = corners[1].y - corners[0].y;

        if (mousePos.y < height)// Û±Íµ◊≤øø’º‰≤ª◊„
        {
            rectTransform.position = mousePos + Vector3.up * height * 0.6f;
        }
        else if (Screen.width - mousePos.x > width) // Û±Í”“≤‡ø’º‰≥‰◊„
        {
            rectTransform.position = mousePos + Vector3.right * width * 0.6f;
        }
        else
        {
            rectTransform.position = mousePos + Vector3.left * width * 0.6f;
        }

    }

}
