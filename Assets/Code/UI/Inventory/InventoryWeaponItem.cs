using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class InventoryWeaponItem : MonoBehaviour {

    private Sprite weaponIcon;
    private Text weaponNameText;
    private Text description;

    public PlayerInventory inventory;
    public Weapon weapon;

    void Awake()
    {
        weaponIcon = transform.Find("WeaponIconBackground/WeaponIcon").GetComponent<Image>().sprite;
        weaponNameText = transform.Find("WeaponInfoBackground/WeaponName").GetComponent<Text>();
        description = transform.Find("WeaponInfoBackground/WeaponDescription").GetComponent<Text>();
    }

    public void EquipButton()
    {
        weapon.Equip(inventory);
    }

    //при открытии инвентаря на "I" задаём каждому итему свойства
    public void SetPropertys(Weapon weapon)
    {
        transform.Find("WeaponIconBackground/WeaponIcon").GetComponent<Image>().sprite = weapon.weaponData.icon;
        weaponNameText.text = weapon.weaponData.name;
        this.description.text = weapon.weaponData.description;
        this.weapon = weapon;
    }

}
