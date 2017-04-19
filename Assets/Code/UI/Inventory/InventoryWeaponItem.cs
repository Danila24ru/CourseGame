using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryWeaponItem : MonoBehaviour {

    private Sprite weaponIcon;
    private Text weaponNameText;
    private Text description;

    void Awake()
    {
        weaponIcon = transform.Find("WeaponIconBackground/WeaponIcon").GetComponent<Image>().sprite;
        weaponNameText = transform.Find("WeaponInfoBackground/WeaponName").GetComponent<Text>();
        description = transform.Find("WeaponInfoBackground/WeaponDescription").GetComponent<Text>();
    }

    void EquipButton()
    {
        
    }
    public void SetPropertys(Sprite weaponIcon, string weaponName, string description)
    {
        transform.Find("WeaponIconBackground/WeaponIcon").GetComponent<Image>().sprite = weaponIcon;
        weaponNameText.text = weaponName;
        this.description.text = description;
    }

}
