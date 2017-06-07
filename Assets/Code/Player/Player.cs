using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityStandardAssets.ImageEffects;

public class Player : NetworkBehaviour, IDamageble {

    [SyncVar]
    public string Nickname = "Nick";
    TextMesh nicknameText;

    private const float MAX_HEALTH = 100.0f;

    [SyncVar]
    public float health = MAX_HEALTH;
    private float rateOfRegeneration;
    private float healthToRegenerate;

    public Weapon currentWeapon;
    public Weapon primaryWeapon;
    public Weapon secondWeapon;

    //private Gloves[] rightGlove;
    //private Gloves leftGlove;
    public List<Gloves> LeftGlove;
    public List<Gloves> RightGlove;

    /*public Gloves RightGlove
    {
        get
        {
            if (rightGlove != null)
                return rightGlove[0];
            else
                return null;
        }
        set
        {
            if (value.switchHands == Gloves.EHands.right)
                rightGlove.Add(value);
        }
    }
    public Gloves LeftGlove
    {
        get
        {
            if (leftGlove != null)
                return leftGlove[0];
            else
                return null;
        }
        set
        {
            if (value.switchHands == Gloves.EHands.left)
               leftGlove.Add(value);
        }
    }*/


    VignetteAndChromaticAberration vignette;

    void Start()
    {
        vignette = Camera.main.gameObject.GetComponent<VignetteAndChromaticAberration>();
        nicknameText = transform.Find("Nickname").GetComponent<TextMesh>();
        if (nicknameText != null && isLocalPlayer)
        {
            GameObject.Find("Canvas").GetComponent<PlayerStatWindow>().owner = this;
            CmdSetPlayerName(GameObject.Find("NetworkManager").GetComponent<GameNetworkManager>().PlayerName);
            nicknameText.transform.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        nicknameText.text = Nickname;
        nicknameText.transform.rotation = Quaternion.LookRotation(nicknameText.transform.position - Camera.main.transform.position);

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
            CmdDie();
    }

    public void CmdDie()
    {
        GameNetworkManager.RespawnPlayer(this);
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

    [Command(channel = 0)]
    void CmdSetPlayerName(string name)
    {
        Nickname = name;
    }


}
