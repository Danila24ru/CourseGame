﻿using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class Weapon : NetworkBehaviour {

    [SyncVar]
    public NetworkInstanceId parentNetId;

    public EWeaponAnimationType weaponAnimType;

    [SerializeField]
    private Transform muzzleFlash;

    public RaycastHit hit;
    public GameObject hitPointEffect;
    public GameObject hitBloodEffect;

    public float verticalSpread;
    public float horizontalSpread;
    public float minDistance = 15f;
    public float midDistance = 70f;
    public float longDistance = 80f;
    
    
    public WeaponData weaponData;

    [SerializeField]
    private AudioSource audioSource;
    public AudioClip shootSound;
    public AudioClip reloadSound;

    private float nextFireTime;

    public override void OnStartClient()
    {
        Transform hand = ClientScene.FindLocalObject(parentNetId).transform.Find("root/pelvis/spine_01/spine_02/spine_03/clavicle_r/upperarm_r/lowerarm_r/hand_r");
        transform.SetParent(hand);
    }

    [ClientRpc]
    public void RpcSetTransform(Vector3 position, Quaternion rotation)
    {
        transform.localPosition = position;
        transform.localRotation = rotation;
    }

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    [Command]
    public void CmdFire(Vector3 HitPosition)
    {
        if(nextFireTime < Time.time && weaponData.currentClipAmmo > 0)
        {
            nextFireTime = Time.time + weaponData.fireRate;

            if (weaponData.fireType == EWeaponFireType.Single)
            {
                InstantFire(HitPosition);
            }
            else if (weaponData.fireType == EWeaponFireType.Spread)
            {
                for (int i = 0; i < weaponData.spreadCount; i++)
                {
                    InstantFire(HitPosition);
                }
            }
            weaponData.currentClipAmmo -= 1;
            RpcPlayShootSound();
            
        }
    }

    void InstantFire(Vector3 directionPoint)
    {
        Vector3 shootDirection = directionPoint - muzzleFlash.position;
        if (shootDirection.magnitude < minDistance)
            horizontalSpread = verticalSpread = 0.1f;
        else if (shootDirection.magnitude >= midDistance && shootDirection.magnitude < longDistance)
            horizontalSpread = verticalSpread = 0.4f;
        else
            horizontalSpread = verticalSpread = 1.2f;

        shootDirection.x += Random.Range(-horizontalSpread, horizontalSpread);
        shootDirection.y += Random.Range(-verticalSpread, verticalSpread);

        Debug.DrawRay(muzzleFlash.position, shootDirection * 20f, Color.green, 10f);
        if (Physics.Raycast(muzzleFlash.position, shootDirection, out hit, weaponData.range))
        {
            Debug.Log(hit.transform.name);
    
            if(hit.transform.gameObject.GetComponent<IDamageble>() != null)
            {
                IDamageble damagedObject = hit.transform.gameObject.GetComponent<IDamageble>();
                if(damagedObject.IsAlive())
                    damagedObject.CmdTakeDamage(weaponData.damage);
                GameObject hitEffectBlood = Instantiate(hitBloodEffect, hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal));
                Destroy(hitEffectBlood, 20f);
                RpcSpawnBlood(hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal));
            }
            else
            {
                GameObject hitEffect = Instantiate(hitPointEffect, hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal));
                hitEffect.transform.position += new Vector3(0, 0.02f, 0);
                Destroy(hitEffect, 15f);
                RpcSpawnBulletHole(hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal));
            }
            
        }
    }

    [Command(channel = 0)]
    public void CmdReload()
    {
        RpcPlayReloadSound();
        if (weaponData.currentAmmo > 0)
        {
            if (weaponData.currentAmmo >= weaponData.MaxClipAmmo)
            {
                weaponData.currentAmmo += weaponData.currentClipAmmo - weaponData.MaxClipAmmo;
                weaponData.currentClipAmmo = weaponData.MaxClipAmmo;
            }
            else
            {
                if (weaponData.currentClipAmmo == 0)
                {
                    weaponData.currentClipAmmo = weaponData.currentAmmo;
                    weaponData.currentAmmo = 0;
                }
                else
                {
                    weaponData.currentClipAmmo += weaponData.currentAmmo;
                    weaponData.currentAmmo = 0;
                    if (weaponData.currentClipAmmo > weaponData.MaxClipAmmo)
                    {
                        weaponData.currentAmmo = weaponData.currentClipAmmo - weaponData.MaxClipAmmo;
                        weaponData.currentClipAmmo -= weaponData.currentAmmo;
                    }
                }
            }
        }
    }

    [ClientRpc(channel = 1)]
    void RpcPlayShootSound()
    {
        SoundUtilities.PlaySound(audioSource, shootSound, 1f, 0.2f);
    }

    [ClientRpc(channel = 1)]
    void RpcPlayReloadSound()
    {
        SoundUtilities.PlaySound(audioSource, reloadSound, 1f, 0);
    }

    [ClientRpc(channel = 1)]
    void RpcSpawnBulletHole(Vector3 pos, Quaternion rot)
    {
        GameObject hitEffect = Instantiate(hitPointEffect, pos, rot);
        hitEffect.transform.position += new Vector3(0, 0.02f, 0);
        Destroy(hitEffect, 15f);
    }
    [ClientRpc(channel = 1)]
    void RpcSpawnBlood(Vector3 pos, Quaternion rot)
    {
        GameObject hitEffectBlood = Instantiate(hitBloodEffect, pos, rot);
        Destroy(hitEffectBlood, 3f);
    }

}

public enum EWeaponAnimationType
{
    Gun1Handed_1HandGrip = 1,
    Gun1Handed_2HandGrip = 2,
    Gun2Handed_Long = 3,
    Gun2Handed_Short = 4
}

public enum EWeaponFireType
{
    Single = 0,
    Spread = 1,
    Projectile = 2,
}

[System.Serializable]
public struct WeaponData
{
    public string name;
    public Sprite icon;
    public string description;

    [SyncVar]
    public int currentClipAmmo, currentAmmo;
    [SyncVar]
    public int MaxClipAmmo, MaxAmmo;
    public int spreadCount;

    public float damage;
    public float fireRate;
    public float reloadRate;
    public float range;

    public EWeaponFireType fireType;
}

[System.Serializable]
public struct WeaponSounds
{
    public AudioClip shootSound;
    public AudioClip reloadSound;
    public AudioClip equipSound;
}