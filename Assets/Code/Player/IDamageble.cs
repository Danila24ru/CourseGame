using UnityEngine.Networking;

public interface IDamageble {

    [Command]
    void CmdTakeDamage(float damage);
    [Command]
    void CmdDie();
    bool IsAlive();
}
