using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking.Match;

public class MatchInfo : MonoBehaviour {

    public MatchInfoSnapshot matchInfoSnapshot;

    public void JoinButton()
    {
        GameObject.Find("NetworkManager").GetComponent<GameNetworkManager>().JoinGame(matchInfoSnapshot);
    }
}
