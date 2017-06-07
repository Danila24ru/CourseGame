using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MedicineBox : MonoBehaviour, IItem {

    public void Pickup(PlayerInventory inventory)
    {
        if (inventory.owner.health + 100f > 200f)
            return;
        else
        {
            inventory.owner.health += 100f;
            Destroy(transform.parent.gameObject);
        }
    }

    public void Equip(PlayerInventory inventory)
    {
        //to do
    }


    public void Throw()
    {
        //to do
    }
}
