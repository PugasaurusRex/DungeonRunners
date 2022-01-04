using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NETWORK_ENGINE;
using UnityEngine.UI;

public class ItemScript : NetworkComponent
{
    public int id;
    public Rigidbody2D rig;
    public float speed;
    public int damage;
    public int damageType;
    public PlayerControllerScript player;
    public Sprite icon;

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
    public virtual void Start()
    {
        rig = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public virtual void Use(PlayerControllerScript p)
    {
        if(IsServer)
        {
            player = p;
        }
    }

    virtual public void OnTriggerEnter2D(Collider2D c)
    {
        // On trigger enter set item to be in range for player
        if(IsServer && c.gameObject.tag == "Player")
        {
            c.GetComponent<PlayerControllerScript>().itemsInRange.Add(GetComponent<ItemScript>());
        }
    }

    virtual public void OnTriggerExit2D(Collider2D c)
    {
        // When player exits trigger remove from in range if it is still in range
        if (IsServer && c.gameObject.tag == "Player")
        {
            if(c.GetComponent<PlayerControllerScript>().itemsInRange.Contains(GetComponent<ItemScript>()))
            {
                c.GetComponent<PlayerControllerScript>().itemsInRange.Remove(GetComponent<ItemScript>());
            }
        }
    }

    public void RemoveFromWorld()
    {
        if(IsServer)
        {
            MyCore.NetDestroyObject(MyId.NetId);
        }
    }
}