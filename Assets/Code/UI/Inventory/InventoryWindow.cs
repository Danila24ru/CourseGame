using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryWindow : MonoBehaviour {

    Transform panelInventory;
    bool isOpen;

    void Start()
    {
        panelInventory = transform.Find("PanelInventory");
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.I))
        {
            isOpen = !isOpen;
            panelInventory.gameObject.SetActive(isOpen);
        }
    }
}
