using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityStandardAssets.ImageEffects;

public class Player : NetworkBehaviour, IDamageble {

    [SyncVar]
    string Nickname = "Nick";
    TextMesh nicknameText;

    private const float MAX_HEALTH = 100.0f;

    [SyncVar]
    public float health = MAX_HEALTH;
    public float rateOfRegeneration;
    public float healthToRegenerate;

    public GameObject currentWeapon;
    public GameObject primaryWeapon;
    public GameObject secondWeapon;

    VignetteAndChromaticAberration vignette;

    void Start()
    {
        vignette = Camera.main.gameObject.GetComponent<VignetteAndChromaticAberration>();
        nicknameText = transform.Find("Nickname").GetComponent<TextMesh>();
        if (nicknameText != null && isLocalPlayer)
        {
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
