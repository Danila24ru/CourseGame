using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class PlayerInventory : NetworkBehaviour {

    public GameObject currentWeapon, firstWeapon, secondWeapon;

    public GameObject weaponHolder;

    private Animator playerAnimator;

    private InventoryWindow inventoryWindow;

    public int inventorySize;
    public GameObject[] grenades;
    public Weapon[] weapons;

	// Use this for initialization
	void Start () {
        playerAnimator = GetComponent<Animator>();
        
        if (isLocalPlayer)
        {
            inventoryWindow = GameObject.Find("Canvas").GetComponentInChildren<InventoryWindow>();
            inventoryWindow.inventory = this;
        }
	}
	
	// Update is called once per frame
	void Update () {

        if (!isLocalPlayer)
            return;

        weapons = GetComponentsInChildren<Weapon>();

        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetAxisRaw("Mouse ScrollWheel") > 0)
        {
            CmdChangeCurrentWeaponTo(secondWeapon.name);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetAxisRaw("Mouse ScrollWheel") < 0)
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

    void Equip()
    {

    }
    
    [ClientRpc(channel = 1)]
    void RpcSpawnOnClient(string id, int value)
    {
        playerAnimator.SetInteger(id, value);
    }
}
