using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossScript : MeleeEnemyScript
{
    // Start is called before the first frame update
    void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    void Update()
    {
        base.Update();
    }

    public override void NetworkedStart()
    {
        if (IsServer)
        {
            base.NetworkedStart();
            StartCoroutine(SlowUpdate());
            StartCoroutine(Chase());
        }
    }

    public override IEnumerator SlowUpdate()
    {
        yield return new WaitForSeconds(0.1f);
    }

    public new IEnumerator Chase()
    {
        while (IsServer)
        {
            //We check that the player is in their sights and they are not hurt
            if (noticed && !isHurt)
            {
                rig.velocity = new Vector2(moveSpeed / 2 * -enemyDir, rig.velocity.y);
                if (Mathf.Abs(transform.position.x - myHit.point.x) <= (2 + GetComponent<BoxCollider2D>().size.x / 2))
                {
                    //We check that it is a player, that they are in range, and that they are NOT currently attacking.
                    if (!myHit.collider.GetComponent<PlayerControllerScript>().isAttack && canAttack)
                    {
                        SendUpdate("ATTACK", "1");
                        myHit.collider.GetComponent<PlayerControllerScript>().ReceiveDamage(0, attackDamage, null);
                    }
                }
                //Otherwise, they keep moving
                else if (enemyDir == -1)
                {
                    SendUpdate("REVMOVE", "1");
                }
                else if (enemyDir == 1)
                {
                    SendUpdate("MOVE", "1");
                }

            }
            yield return new WaitForSeconds(0.1f);
        }
    }
}
