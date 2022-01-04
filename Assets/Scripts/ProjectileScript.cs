using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NETWORK_ENGINE;

public class ProjectileScript : NetworkComponent
{
    public float speed;
    public int damage;
    public int damageType;
    public int dir = 0;
    public PlayerControllerScript instigator;
    public Rigidbody2D rig;

    public override void HandleMessage(string flag, string value)
    {
        if(flag == "DIR")
        {
            dir = int.Parse(value);
            if (IsClient)
            {
                if (dir == 1)
                {
                    GetComponent<SpriteRenderer>().flipX = false;
                }
                else
                {
                    GetComponent<SpriteRenderer>().flipX = true;
                }
            }
        }
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
        rig = this.GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if(IsServer)
        {
            rig.velocity = new Vector2(speed * dir, 0);
        }
        
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if(IsServer)
        {
            if (collision.gameObject.tag == "Player" && collision.gameObject.GetComponent<PlayerControllerScript>() != instigator)
            {
                collision.GetComponent<PlayerControllerScript>().ReceiveDamage(damageType, damage, instigator);
                MyCore.NetDestroyObject(this.NetId);
            }
            else if (collision.gameObject.tag == "Enemy")
            {
                collision.GetComponent<EnemyControllerScript>().ReceiveDamage(damageType, damage, instigator);
                MyCore.NetDestroyObject(this.NetId);
            }
            else if(collision.gameObject.tag == "Tiles")
            {
                MyCore.NetDestroyObject(this.NetId);
            }
        }
    }

    public void FlipSprite(int d)
    {
        SendUpdate("DIR", d.ToString());
    }
}
