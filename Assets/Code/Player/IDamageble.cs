using UnityEngine.Networking;

public interface IDamageble {

    [Command]
    void CmdTakeDamage(float damage);
    void Die();
    bool IsAlive();
}
