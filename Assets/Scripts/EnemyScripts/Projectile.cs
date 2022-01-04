using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NETWORK_ENGINE;

public class Projectile : NetworkComponent
{
    public int dir = 0;
    public Rigidbody2D rig;
    public float moveSpeed;
    public int hurt;

    public override void HandleMessage(string flag, string value)
    {
        
    }

    public override void NetworkedStart()
    {
        
    }

    public override IEnumerator SlowUpdate()
    {
        rig.velocity = new Vector2(moveSpeed * -dir, 0);
        yield return new WaitForSeconds(0.2f);
    }

    // Start is called before the first frame update
    void Start()
    {
        rig = this.GetComponent<Rigidbody2D>();
        if(dir == -1)
        {
            GetComponent<SpriteRenderer>().flipX = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "Tiles")
        {
            MyCore.NetDestroyObject(this.NetId);
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Player")
        {
            collision.GetComponent<PlayerControllerScript>().ReceiveDamage(0, hurt, null);
        }
        if(collision.tag != "Enemy")
        {
            MyCore.NetDestroyObject(this.NetId);
        }
    }
}
