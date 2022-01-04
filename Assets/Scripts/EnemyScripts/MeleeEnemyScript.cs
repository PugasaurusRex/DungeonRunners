using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeEnemyScript : EnemyControllerScript
{
    public float hitRange;
    public int randMove;
    public float moveTimer = 2.0f;
    public float moveAmt;
    public bool walkin = false;
    public float knockbackAmt;
    public bool knockedBack = false;
    public RaycastHit2D enemyHit;

    // Start is called before the first frame update
    override public void Start()
    {
        noticed = false;
        base.Start();
    }

    // Update is called once per frame
    override public void Update()
    {
        base.Update();
    }

    public override void NetworkedStart()
    {
        if (IsServer)
        {
            base.NetworkedStart();
            StartCoroutine(RandMove());
            StartCoroutine(base.SlowUpdate());
            StartCoroutine(Chase());
            StartCoroutine(Knockback());
        }
    }
    public override IEnumerator SlowUpdate()
    {
        yield return new WaitForSeconds(0.1f);
    }

    public IEnumerator RandMove()
    {
        while (IsServer)
        {
            if(!noticed && !isHurt)
            {
                knockedBack = false;
                randMove = Random.Range(0, 3);
                if (randMove == 1)
                {
                    moveAmt = Random.Range(2, moveSpeed);
                    if(name != "Dragon" && name != "Bandit")
                    {
                        SendUpdate("MOVE", "1");
                        enemyDir = -1;
                    }
                    else
                    {
                        SendUpdate("REVMOVE", "1");
                        enemyDir = 1;
                    }
                    rig.velocity += new Vector2(moveAmt, 0);
                    walkin = true;
                }
                else if (randMove == 2)
                {
                    moveAmt = Random.Range(2, moveSpeed);
                    if (name != "Dragon" && name != "Bandit")
                    {
                        SendUpdate("REVMOVE", "1");
                        enemyDir = 1;
                    }
                    else
                    {
                        SendUpdate("MOVE", "1");
                        enemyDir = -1;
                    }
                    rig.velocity += new Vector2(-moveAmt, 0);
                    walkin = true;
                }
            }
            yield return new WaitForSeconds(moveTimer);
        }
    }

    public IEnumerator Chase()
    {
        while(IsServer)
        {
            //We check that the player is in their sights and they are not hurt
            if(noticed && !isHurt)
            {
                knockedBack = false;
                rig.velocity = new Vector2(moveSpeed/2 * -enemyDir, rig.velocity.y);
                if (Mathf.Abs(transform.position.x - myHit.point.x) <= (1 + GetComponent<BoxCollider2D>().size.x/2))
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
                    if(name != "Dragon" && name != "Bandit")
                    {
                        SendUpdate("MOVE", "1");
                    }
                    else
                    {
                        SendUpdate("REVMOVE", "1");
                    }
                }
                else if(enemyDir == 1)
                {
                    if (name != "Dragon" && name != "Bandit")
                    {
                        SendUpdate("REVMOVE", "1");
                    }
                    else
                    {
                        SendUpdate("MOVE", "1");
                    }
                }

            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    public IEnumerator Knockback()
    {
        while(IsServer)
        {
            //If hit, they need to be knocked back
            if (isHurt && !knockedBack)
            {
                if(name != "Dragon" && name != "Bandit")
                {
                    enemyHit = Physics2D.Raycast(transform.position, Vector2.left * enemyDir);
                }
                else
                {
                    enemyHit = Physics2D.Raycast(transform.position, Vector2.right * enemyDir);
                }

                if (enemyHit.collider == null || enemyHit.collider.tag != "Player")
                {
                    if(enemyHit.collider.tag != "Player") //Here, we check if they struck from the FRONT. If they did not, then we need to flip the sprite to face them.
                    {
                        if(enemyDir == -1)
                        {
                            enemyDir = 1;
                            if(name != "Dragon" && name != "Bandit")
                            {
                                SendUpdate("MOVE", "2"); //We need to TURN the sprite, but we do NOT need to walk
                            }
                            else
                            {
                                SendUpdate("REVMOVE", "2"); //We need to TURN the sprite, but we do NOT need to walk
                            }
                        }
                        else if(enemyDir == 1)
                        {
                            enemyDir = -1;
                            if (name != "Dragon" && name != "Bandit")
                            {
                                SendUpdate("REVMOVE", "2"); //We need to TURN the sprite, but we do NOT need to walk
                            }
                            else
                            {
                                SendUpdate("MOVE", "2"); //We need to TURN the sprite, but we do NOT need to walk
                            }
                        }
                    }
                }
                knockedBack = true;
                rig.velocity += new Vector2(knockbackAmt * enemyDir, 0);
            }
            yield return new WaitForSeconds(.1f);
        }
    }
}
