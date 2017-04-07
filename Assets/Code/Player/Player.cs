﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityStandardAssets.ImageEffects;
public class Player : NetworkBehaviour, IDamageble {

    private const float MAX_HEALTH = 100.0f;

    [SyncVar]
    public float health = MAX_HEALTH;
    public float rateOfRegeneration;
    public float healthToRegenerate;

    VignetteAndChromaticAberration vignette;

    void Start()
    {
        vignette = Camera.main.gameObject.GetComponent<VignetteAndChromaticAberration>();
    }

    void Update()
    {
        if (!isLocalPlayer)
            return;
        vignette.intensity = 1f - health / MAX_HEALTH;
    }

    public bool IsAlive()
    {
        return health > 0 ? true : false; 
    }

    public void CmdTakeDamage(float damage)
    {
        if (IsAlive())
        {
            health -= damage;
            if (health < MAX_HEALTH)
                StartCoroutine(startHealthRegeneration());
        }
        if (!IsAlive())
            Die();
    }

    public void Die()
    {
        Transform startPos = NetworkManager.singleton.GetStartPosition();
        var newPlayer = (GameObject)Instantiate(NetworkManager.singleton.playerPrefab, startPos.position, startPos.rotation);
        NetworkServer.Destroy(this.gameObject);
        NetworkServer.ReplacePlayerForConnection(this.connectionToClient, newPlayer, this.playerControllerId);
    }

    IEnumerator startHealthRegeneration()
    {
        while(health < MAX_HEALTH)
        {
            health += healthToRegenerate;
            if (health > MAX_HEALTH)
                health = MAX_HEALTH;
            yield return new WaitForSeconds(rateOfRegeneration);
        }
    }
}
