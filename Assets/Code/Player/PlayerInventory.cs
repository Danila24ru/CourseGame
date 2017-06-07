using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class PlayerInventory : NetworkBehaviour {

    public GameObject weaponHolder;

    public Player owner;

    private InventoryWindow inventoryWindow;

    public List<GameObject> items;
    
	void Start () {

        if (isLocalPlayer)
        {
            inventoryWindow = GameObject.Find("Canvas").GetComponentInChildren<InventoryWindow>();
            inventoryWindow.inventory = this;
            owner = GetComponent<Player>();
        }
    }

    void OnTriggerEnter(Collider item)
    {
        if(item.transform.GetChild(0).GetComponent<IItem>() != null)
        {
            item.transform.GetChild(0).GetComponent<IItem>().Pickup(this);
            Debug.Log("Вы подняли: " + item.transform.GetChild(0).name);
        }
    }
    
    [ClientRpc(channel = 1)]
    public void RpcSpawnOnClient(string id, int value)
    {
        owner.GetComponent<Animator>().SetInteger(id, value);
    }

}
