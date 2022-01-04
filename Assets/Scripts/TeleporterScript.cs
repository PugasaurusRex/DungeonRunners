using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NETWORK_ENGINE;

public class TeleporterScript : NetworkComponent
{
    public Vector2 teleportTo;
    public bool isActivated;
    public bool isEntrance;
    public bool isExit;
    public int room;

    public override void HandleMessage(string flag, string value)
    {
        
    }

    public override void NetworkedStart()
    {
        
    }

    public override IEnumerator SlowUpdate()
    {
        yield return new WaitForSeconds(.1f);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D c)
    {
        if(isActivated && IsServer && c.gameObject.tag == "Player")
        {
            if(isExit)
            {
                // If player reaches activated exit they have finished
                // Tell gamemanager to set player to end state
                FindObjectOfType<GameManagerScript>().EndRun(c.GetComponent<PlayerControllerScript>(), false);
            }
            else if(!isEntrance)
            {
                // Teleport player to next entrance
                c.gameObject.transform.position = teleportTo;
                c.gameObject.GetComponent<PlayerControllerScript>().spawnLocation = teleportTo;
            }
        }
    }
}
