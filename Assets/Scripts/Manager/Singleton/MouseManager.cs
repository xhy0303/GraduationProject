using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.AI;
using System;
using UnityEngine.EventSystems;


public class MouseManager : Manager<MouseManager>
{
    RaycastHit hitInfo;//½âÎöÔÚp5 06£º14
    public event Action<Vector3> OnMouseCliked;//p6 03:55
    public event Action<GameObject> OnEnemyClicked;//p11 00:56

    public Texture2D normal, attack, doorWay, talk, chest;



    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this);
    }
    // Update is called once per frame
    void Update()
    {
        if (InteractWithUI())
        {
            return;
        }

        SetCursorTexture();
        MouseControl();
    }


    void SetCursorTexture()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (InteractWithUI())
        {
            Cursor.SetCursor(normal, Vector2.zero, CursorMode.Auto);
            return;
        }

        if (Physics.Raycast(ray, out hitInfo))
        {
            switch (hitInfo.collider.gameObject.tag)
            {
                default:
                    Cursor.SetCursor(normal, new Vector2(10, 6), CursorMode.Auto);
                    break;

                case "Enemy":
                    if(GameManager.Instance.playerStats.currentShape==PlayerStats.PlayerShape.Yokai)
                    {
                        Cursor.SetCursor(talk, new Vector2(8, 8), CursorMode.Auto);
                    }
                    else
                    {
                        Cursor.SetCursor(attack, new Vector2(10, 6), CursorMode.Auto);
                    }
                    break;

                case "Attackable":
                    Cursor.SetCursor(attack, new Vector2(8, 8), CursorMode.Auto);
                    break;

                case "Portal":
                    Cursor.SetCursor(doorWay, new Vector2(8, 8), CursorMode.Auto);
                    break;

                case "NPC":
                    Cursor.SetCursor(talk, new Vector2(8, 8), CursorMode.Auto);
                    break;

                case "Chest":
                case "Shop":
                    Cursor.SetCursor(chest, new Vector2(8, 8), CursorMode.Auto);
                    break;
            }
        }
    }
    void MouseControl()
    {
        if (Input.GetMouseButtonDown(0) && hitInfo.collider != null)
        {
            if (hitInfo.collider.gameObject.tag == "Ground"
                || hitInfo.collider.gameObject.tag == "Portal"
                || hitInfo.collider.gameObject.tag == "Item"
                || hitInfo.collider.gameObject.tag == "NPC"
                || hitInfo.collider.gameObject.tag == "Chest"
                || hitInfo.collider.gameObject.tag == "Shop"
                || (hitInfo.collider.tag == "Enemy"
                && GameManager.Instance.playerStats.currentShape == PlayerStats.PlayerShape.Yokai))   
            {
                OnMouseCliked?.Invoke(hitInfo.point);
            }
            if ((hitInfo.collider.tag == "Enemy" 
                && GameManager.Instance.playerStats.currentShape != PlayerStats.PlayerShape.Yokai)
                ||hitInfo.collider.CompareTag("Attackable")) 
            {
                OnEnemyClicked?.Invoke(hitInfo.collider.gameObject);
            }

        }
    }

    bool InteractWithUI()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return true;
        }
        return false;
    }

}
