using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemHoldRotator : MonoBehaviour {
    
    private Transform item;
    public GameObject itemObject;

	void Start () {
        item = transform.GetChild(0);
        itemObject = Resources.Load<GameObject>(item.name);
    }
	
	void Update () {
        if (transform.GetChild(0).GetComponent<IItem>() == null)
        {
            Destroy(gameObject);
        }
        item.Rotate(new Vector3(0, 1, 0));
    }
}
