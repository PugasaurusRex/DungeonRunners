using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PotionScript : ItemScript
{
    public bool throwable;
    public bool thrown = false;
    public List<GameObject> inRange = new List<GameObject>();

    // Start is called before the first frame update
    override public void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public override void Use(PlayerControllerScript p)
    {
        p.ReceiveDamage(damageType, damage, null);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(thrown)
        {
            foreach(GameObject o in inRange)
            {
                if(o.GetComponent<PlayerControllerScript>() != null)
                {
                    o.GetComponent<PlayerControllerScript>().ReceiveDamage(damageType, damage, player);
                }
                else
                {
                    o.GetComponent<EnemyControllerScript>().ReceiveDamage(damageType, damage, player);
                }
            }
            MyCore.NetDestroyObject(this.NetId);
        }
    }

    override public void OnTriggerEnter2D(Collider2D collision)
    {
        base.OnTriggerEnter2D(collision);
        if((collision.gameObject.tag == "Player" || collision.gameObject.tag == "Enemy") && !inRange.Contains(collision.gameObject))
        {
            inRange.Add(collision.gameObject);
        }
    }

    override public void OnTriggerExit2D(Collider2D c)
    {
        base.OnTriggerExit2D(c);
        if ((c.gameObject.tag == "Player" || c.gameObject.tag == "Enemy") && inRange.Contains(c.gameObject))
        {
            inRange.Remove(c.gameObject);
        }
    }
}