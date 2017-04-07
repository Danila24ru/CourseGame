﻿using UnityEngine;
using UnityEngine.Networking;

public class PlayerInventory : NetworkBehaviour {

    public GameObject currentWeapon, firstWeapon, secondWeapon;

    public GameObject weaponHolder;

    private Animator playerAnimator;


	// Use this for initialization
	void Start () {
        playerAnimator = GetComponent<Animator>();
	}
	
	// Update is called once per frame
	void Update () {

        if (!isLocalPlayer)
            return;

        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetAxisRaw("Mouse ScrollWheel") != 0)
        {
            CmdChangeCurrentWeaponTo(secondWeapon.name);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            CmdChangeCurrentWeaponTo(firstWeapon.name);
        }
    }

    [Command(channel = 0)]
    void CmdChangeCurrentWeaponTo(string weaponName)
    {
        if (currentWeapon)
            NetworkServer.Destroy(currentWeapon);
        GameObject weaponOrigin = Resources.Load<GameObject>(weaponName);

        GameObject spawnWeapon = Instantiate(weaponOrigin);
        spawnWeapon.GetComponent<Weapon>().parentNetId = netId;

        spawnWeapon.transform.SetParent(weaponHolder.transform);
        spawnWeapon.transform.localPosition = weaponOrigin.transform.position;
        spawnWeapon.transform.localRotation = weaponOrigin.transform.rotation;
        
        currentWeapon = spawnWeapon;

        NetworkServer.Spawn(spawnWeapon);
        spawnWeapon.GetComponent<Weapon>().RpcSetTransform(weaponOrigin.transform.position, weaponOrigin.transform.rotation);

        playerAnimator.SetInteger("WeaponType", (int)weaponOrigin.GetComponent<Weapon>().weaponAnimType);
        
        RpcSpawnOnClient("WeaponType", (int)weaponOrigin.GetComponent<Weapon>().weaponAnimType);
    }
    
    [ClientRpc(channel = 1)]
    void RpcSpawnOnClient(string id, int value)
    {
        playerAnimator.SetInteger(id, value);
    }
}