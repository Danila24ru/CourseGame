using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryWindow : MonoBehaviour {

    private GameObject inventoryWeaponItem; // ссылка на префаб с классом InventoryWeaponItem
    public PlayerInventory inventory;
    Transform panelInventory;
    bool isOpen;

    void Start()
    {
        inventoryWeaponItem = Resources.Load<GameObject>("UI/WeaponInventoryItem");
        panelInventory = transform.Find("PanelInventory");
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.I))
        {
            isOpen = !isOpen;
            panelInventory.gameObject.SetActive(isOpen);
            if (isOpen == true)
            {
                Debug.Log(inventory.weapons[0]);
                RefreshWeaponList();
            }
    
        }
        
        
    }
    
    void AddItemInWeaponList(Weapon weapon)
    {
        Transform GridItems = transform.Find("PanelInventory/PanelWeaponList/GridWithElements");
        GameObject weaponItemObject = Instantiate(inventoryWeaponItem, GridItems);
        InventoryWeaponItem weaponItem = weaponItemObject.GetComponent<InventoryWeaponItem>();
        weaponItem.SetPropertys(weapon.weaponData.icon, weapon.weaponData.name, weapon.weaponData.description);
    }
    void ClearItemsInWeaponList()
    {
        Transform GridItems = transform.Find("PanelInventory/PanelWeaponList/GridWithElements");
        foreach (Transform item in GridItems)
            Destroy(item.gameObject);
    }

    void RefreshWeaponList()
    {
        ClearItemsInWeaponList();
        foreach(var weapon in inventory.weapons)
        {
            AddItemInWeaponList(weapon);
        }
    }
}
