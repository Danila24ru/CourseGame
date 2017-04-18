using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.Networking.Match;

public class GameNetworkManager : NetworkManager {


	public void HostGame(string matchName, int matchSize, string matchPassword)
    {
        matchMaker.CreateMatch(matchName, (uint)matchSize, true, matchPassword, "", "", 0, 0, OnMatchCreate);
    }

    public void GetMatchesList(GameObject matchLinePanel)
    {
        matchMaker.ListMatches(0, 20, "", true, 0, 0, OnMatchList);
        var matches = this.matches;

        Transform gridWithMatches = GameObject.Find("PanelMatchList/GridWithElements").transform;

        foreach (Transform child in gridWithMatches)
            Destroy(child.gameObject);

        if (matches != null)
        {
            foreach (var match in matches)
            {
                GameObject matchLine = Instantiate(matchLinePanel);
                matchLine.transform.SetParent(gridWithMatches);
                Text info = matchLine.transform.Find("Text").GetComponent<Text>();
                info.text = string.Format("{0}    {1}          {2}/{3}", match.networkId, match.name, match.currentSize, match.maxSize);
                MatchInfo matchInfo = matchLine.GetComponent<MatchInfo>();
                matchInfo.matchInfoSnapshot = match;
            }
        }
        else
        {
            GameObject noMatchInfo = Instantiate(matchLinePanel);
            noMatchInfo.transform.SetParent(gridWithMatches);
            Text info = noMatchInfo.transform.Find("Text").GetComponent<Text>();
            info.text = "Games not found :(";
        }
    }

    public void JoinGame(MatchInfoSnapshot matchInfoSnapshot)
    {
        matchName = matchInfoSnapshot.name;
        matchSize = (uint)matchInfoSnapshot.currentSize;
        matchMaker.JoinMatch(matchInfoSnapshot.networkId, "", "", "", 0, 0, OnMatchJoined);
    }

    public static void RespawnPlayer(Player player)
    {
        Transform startPos = singleton.GetStartPosition();
        var newPlayer = (GameObject)Instantiate(singleton.playerPrefab, startPos.position, startPos.rotation);
        NetworkServer.Destroy(player.gameObject);
        NetworkServer.ReplacePlayerForConnection(player.connectionToClient, newPlayer, player.playerControllerId);
    }


}
