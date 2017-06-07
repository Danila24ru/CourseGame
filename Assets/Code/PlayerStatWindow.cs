using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatWindow : MonoBehaviour {

    public Text playerStat;
    public Player owner;

	void Update () {

        if (playerStat != null && owner != null)
        {
            if(owner.primaryWeapon != null && owner.secondWeapon != null && owner.currentWeapon != null)
            playerStat.text = string.Format("Name: {5}\n Health: {0}\n 1st weapon: {1}\n 2nd weapon: {2}\n Ammo: {3}\n MaxAmmo: {4}",
                owner.health,
                owner.primaryWeapon.name,
                owner.secondWeapon.name,
                owner.currentWeapon.weaponData.currentClipAmmo,
                owner.currentWeapon.weaponData.currentAmmo,
                owner.Nickname);
        }
		
	}
}
