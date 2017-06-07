using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gloves : MonoBehaviour, IItem {

    public float damageMultiplier = 1.0f;
    public EHands switchHands;

    public void Equip(PlayerInventory inventory)
    {
        if(inventory.owner.currentWeapon == null)
        {
            if (switchHands == EHands.right)
            {
                Debug.Log(inventory.owner.RightGlove);
                inventory.owner.RightGlove.Add(this);
                Debug.Log("Надета на правую руку");
                Debug.Log(inventory.owner.RightGlove);
                transform.parent = null;
            }
            else if(switchHands == EHands.left)
            {
                Debug.Log(inventory.owner.LeftGlove);
                inventory.owner.LeftGlove.Add(this);
                Debug.Log("Надета на левую руку");
                Debug.Log(inventory.owner.LeftGlove);
                transform.parent = null;
            }
            else
            {
                Debug.Log("Перчатка надета уже");
            }
                
        }
        else
        {
            Debug.Log("Нельзя надевать, пока в руках оружие");
        }
    }

    public void Pickup(PlayerInventory inventory)
    {
        //inventory.items.Add(this.gameObject);
        Equip(inventory);
        if(inventory.owner.currentWeapon != null)
            inventory.owner.currentWeapon.CheckModifiers(inventory);
    }

    public void Throw()
    {
        //to do
    }

    public enum EHands
    {
        left = 0,
        right = 1
    }
}
