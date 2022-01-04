using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NETWORK_ENGINE;

public class SpectatorScript : NetworkComponent
{
    public int playerToSpectate = 0;
    public PlayerControllerScript p;
    public GameObject pauseMenu;

    public override void HandleMessage(string flag, string value)
    {
        if (flag == "SPECTATE")
        {
            int temp = int.Parse(value);
            if (IsServer)
            {
                SendUpdate("SPECTATE", temp.ToString());
            }
            if (IsClient)
            {
                PlayerControllerScript[] players = FindObjectsOfType<PlayerControllerScript>();

                temp += 1;
                if (temp >= players.Length)
                {
                    temp = 0;
                }

                playerToSpectate = temp;

                p = players[playerToSpectate];
            }
        }

        if (flag == "PAUSE")
        {
            if (IsServer)
            {
                SendUpdate("PAUSE", "1");
            }
            if (IsLocalPlayer)
            {
                if (pauseMenu.activeSelf)
                {
                    pauseMenu.SetActive(false);
                }
                else
                {
                    pauseMenu.SetActive(true);
                }
            }
        }
    }

    public override void NetworkedStart()
    {
        if(IsLocalPlayer)
        {
            AxisEventCallers.InputEvents["Fire1"].OnAxisKeyDown += OnChangeSpectate;
            AxisEventCallers.InputEvents["Cancel"].OnAxisKeyDown += OnPause;
        }
    }

    public override IEnumerator SlowUpdate()
    {
        if(IsServer && IsDirty)
        {
            SendUpdate("SPECTATE", playerToSpectate.ToString());
            yield return new WaitForSeconds(1);
            IsDirty = false;
        }
        
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(IsLocalPlayer)
        {
            Camera.main.transform.position = new Vector3(p.transform.position.x, p.transform.position.y, -5);
        }
    }

    public void OnChangeSpectate()
    {
        SendCommand("SPECTATE", playerToSpectate.ToString());
    }

    public void OnPause()
    {
        SendCommand("PAUSE", "1");
    }

    public void Quit()
    {
        MyCore.UI_Quit();
    }
}
