using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NETWORK_ENGINE;

public class LavaWallScript : NetworkComponent
{
    public override void HandleMessage(string flag, string value)
    {
        
    }

    public override void NetworkedStart()
    {
        
    }

    public override IEnumerator SlowUpdate()
    {
        while(IsServer)
        {
            // Moves wall
            gameObject.transform.position += new Vector3(.005f, 0, 0); // 1/200th units every tick
            yield return new WaitForSeconds(.006f);
        }
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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(IsServer)
        {
            if (collision.gameObject.tag == "Player")
            {
                PlayerControllerScript temp = collision.GetComponent<PlayerControllerScript>();
                FindObjectOfType<GameManagerScript>().EndRun(temp, true);
            }
            else if (collision.gameObject.tag == "Enemy")
            {
                collision.GetComponent<EnemyControllerScript>().ReceiveDamage(0, 9999, null);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            PlayerControllerScript temp = collision.GetComponent<PlayerControllerScript>();
            FindObjectOfType<GameManagerScript>().EndRun(temp, true);
        }
        else if (collision.gameObject.tag == "Enemy")
        {
            collision.GetComponent<EnemyControllerScript>().ReceiveDamage(0, 9999, null);
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            PlayerControllerScript temp = collision.GetComponent<PlayerControllerScript>();
            FindObjectOfType<GameManagerScript>().EndRun(temp, true);
        }
        else if (collision.gameObject.tag == "Enemy")
        {
            collision.GetComponent<EnemyControllerScript>().ReceiveDamage(0, 9999, null);
        }
    }
}
