using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class NetworkUtilities : MonoBehaviour {

    public RectTransform networkStatPanel;
    public Text networkStatText;
    bool netStatHide = true;

	void Update () {
        
        if (networkStatPanel != null && networkStatText != null && netStatHide == true)
        {
            networkStatText.text = string.Format("Last Ping: {0} \n",
                NetworkManager.singleton.client.GetRTT());
        }
        if(Input.GetKeyDown(KeyCode.H))
        {
            netStatHide = !netStatHide;
            networkStatPanel.gameObject.SetActive(netStatHide);
        }
	}
}
