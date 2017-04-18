using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking.Types;
using UnityEngine.Networking.Match;

public class FindGameWindow : MonoBehaviour {

    public GameObject MatchLine;
    public GameNetworkManager netManager;
    public InputField matchNameToHost;

	// Use this for initialization
	void Start () {
        netManager = GameObject.Find("NetworkManager").GetComponent<GameNetworkManager>();
        netManager.StartMatchMaker();
        matchNameToHost = transform.Find("PanelSearchGameOption/PanelHostGame/InputFieldNameOfMatch").GetComponent<InputField>();
	}
	
    public void HostGame()
    {
        netManager.HostGame(matchNameToHost.text, 10, "");
    }

    public void RefreshMatchList()
    {
        netManager.GetMatchesList(MatchLine);
    }
}
