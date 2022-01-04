using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NETWORK_ENGINE;

public class ChestScript : NetworkComponent
{
    public bool used = false;
    public ItemScript[] items;
    public int numItems = 2;

    public override void HandleMessage(string flag, string value)
    {
        
    }

    public override void NetworkedStart()
    {
        
    }

    public override IEnumerator SlowUpdate()
    {
        yield return new WaitForSeconds(1);
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
        if(!used && IsServer && collision.gameObject.tag == "Player")
        {
            used = true;
            StartCoroutine(SpawnItems());
        }
    }

    public IEnumerator SpawnItems()
    {
        if(IsServer)
        {
            int i = 0;
            while (i < numItems)
            {
                i++;
                int temp = Random.Range(0, items.Length);
                GameObject spawned = MyCore.NetCreateObject(MyCore.FindType(items[temp].name), -1, transform.position, Quaternion.identity);
                spawned.GetComponent<Rigidbody2D>().velocity = new Vector2(Random.Range(-1, 1), 2);
                yield return new WaitForSeconds(1);
            }
            MyCore.NetDestroyObject(this.NetId);
        }
    }
}
