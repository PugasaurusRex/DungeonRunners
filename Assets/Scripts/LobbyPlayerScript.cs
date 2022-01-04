using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NETWORK_ENGINE;
using UnityEngine.UI;
using TMPro;

public class LobbyPlayerScript : NetworkComponent
{
    public bool ready = false;
    public Toggle readyToggle;
    public GameObject quitButton;

    public override void HandleMessage(string flag, string value)
    {
        if (flag == "TOG")
        {
            ready = bool.Parse(value);
            if (IsServer)
            {
                SendUpdate("TOG", value);
            }
            if (IsClient)
            {
                if (!IsLocalPlayer)
                {
                    readyToggle.isOn = ready;
                }
            }
        }
    }

    public override void NetworkedStart()
    {
        if (!IsLocalPlayer)
        {
            readyToggle.interactable = false;
            quitButton.SetActive(false);
        }
        if(IsServer)
        {
            foreach (LobbyPlayerScript lp in FindObjectsOfType<LobbyPlayerScript>())
            {
                lp.SendUpdate("TOG", lp.ready.ToString());
            }
        }
        this.transform.SetParent(FindObjectOfType<GameManagerScript>().transform.GetChild(0).GetChild(0));
    }

    public override IEnumerator SlowUpdate()
    {
        while(IsServer)
        {
            if (IsDirty)
            {
                SendUpdate("TOG", ready.ToString());
                IsDirty = false;
            }
            yield return new WaitForSeconds(.1f);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetReady(bool r)
    {
        if (MyId.IsInit && IsLocalPlayer)
        {
            SendCommand("TOG", r.ToString());
        }
    }

    public void Quit()
    {
        MyCore.UI_Quit();
    }
}
