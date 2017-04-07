using UnityEngine;
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

    [SerializeField]
    private WeaponData weaponData;

    [SerializeField]
    private AudioSource audioSource;

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
        if(nextFireTime < Time.time)
        {
            nextFireTime = Time.time + weaponData.fireRate;

            if (weaponData.fireType == EWeaponFireType.Single)
                InstantFire(HitPosition);
            else if (weaponData.fireType == EWeaponFireType.Spread)
            {
                for (int i = 0; i < weaponData.spreadCount; i++)
                {
                    InstantFire(HitPosition);
                }
            }
            RpcPlayShootSound();
        }
    }

    void InstantFire(Vector3 directionPoint)
    {
        Vector3 shootDirection = directionPoint - muzzleFlash.position;
        shootDirection.x += Random.Range(-0.05f, 0.05f);
        shootDirection.y += Random.Range(-0.05f, 0.05f);

        Debug.DrawRay(muzzleFlash.position, shootDirection * 20f, Color.green, 10f);
        if (Physics.Raycast(muzzleFlash.position, shootDirection, out hit, weaponData.range))
        {
            Debug.Log(hit.transform.name);
    
            if(hit.transform.gameObject.GetComponent<IDamageble>() != null)
            {
                IDamageble damagedObject = hit.transform.gameObject.GetComponent<IDamageble>();
                damagedObject.CmdTakeDamage(weaponData.damage);
            }else
            if(hitPointEffect != null)
            {
                GameObject hitEffect = Instantiate(hitPointEffect, hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal));
                hitEffect.transform.position += new Vector3(0, 0.02f, 0);
                Destroy(hitEffect, 15f);
                RpcSpawnBulletHole(hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal));
            }
            
        }
    }

    [ClientRpc]
    void RpcPlayShootSound()
    {
        audioSource.Play();
    }

    [ClientRpc(channel = 1)]
    void RpcSpawnBulletHole(Vector3 pos, Quaternion rot)
    {
        GameObject hitEffect = Instantiate(hitPointEffect, pos, rot);
        hitEffect.transform.position += new Vector3(0, 0.02f, 0);
        Destroy(hitEffect, 15f);
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

    public int currentClipAmmo, currentAmmo;
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