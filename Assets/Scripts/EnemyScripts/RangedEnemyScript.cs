using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedEnemyScript : EnemyControllerScript
{
    public GameObject projectile;
    public int randMove;
    public int projStart;
    public int projEnd;
    public float projSpeed;
    public bool walkin;

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
            StartCoroutine(base.SlowUpdate());
            StartCoroutine(Noticed());
            StartCoroutine(Flee());
        }
    }

    public override IEnumerator SlowUpdate()
    {
        yield return new WaitForSeconds(0.1f);
    }

    public IEnumerator Noticed()
    {
        while (IsServer)
        {
            //We check that the player is in their sights and they are not hurt
            if (noticed && !isHurt && canAttack)
            {
                if (name == "Witch" || name == "Wiz")
                {
                    projectile = MyCore.NetCreateObject(Random.Range(projStart, projEnd), -1, new Vector2(gameObject.transform.position.x - enemyDir, gameObject.transform.position.y), Quaternion.identity);
                    projectile.GetComponent<Projectile>().dir = enemyDir;
                }
                //netcreate projectile HERE
                if(name == "Wizard")
                {
                    projectile = MyCore.NetCreateObject(Random.Range(projStart, projEnd), -1, new Vector2(gameObject.transform.position.x - enemyDir, gameObject.transform.position.y), Quaternion.identity);
                    projectile.GetComponent<Projectile>().dir = enemyDir;
                }
                projectile.GetComponent<Projectile>().hurt = attackDamage;
                SendUpdate("ATTACK", "1");
                yield return new WaitForSeconds(4.0f);
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    public IEnumerator Flee()
    {
        while (IsServer)
        {
            //If fleeing, they should be hurt, so this is the first check.
            if (isHurt)
            {
                if (noticed) //We should ONLY have them turn around if the player has been noticed by them. If they are attacked from behind, they need to keep running in their current direction
                {
                    enemyDir = -enemyDir;
                }
                //They keep fleeing while still hurt
                while (isHurt)
                {
                    //Velocity is added to make them run away
                    walkin = true;
                    rig.velocity += new Vector2(moveSpeed * -enemyDir, 0);
                    //We update the move controllers
                    if (enemyDir == -1)
                    {
                        if(name != "Wizard")
                        {
                            SendUpdate("MOVE", "1");
                        }
                        else
                        {
                            SendUpdate("REVMOVE", "1");
                        }
                    }
                    if (enemyDir == 1)
                    {
                        if (name != "Wizard")
                        {
                            SendUpdate("REVMOVE", "1");
                        }
                        else
                        {
                            SendUpdate("MOVE", "1");
                        }
                    }
                    yield return new WaitForSeconds(.5f);
                }
                enemyDir = -enemyDir;

                if (enemyDir == -1)
                {
                    if(name != "Wizard")
                    {
                        SendUpdate("MOVE", "2");
                    }
                    else
                    {
                        SendUpdate("REVMOVE", "2");
                    }
                }
                if (enemyDir == 1)
                {
                    if (name != "Wizard")
                    {
                        SendUpdate("REVMOVE", "2");
                    }
                    else
                    {
                        SendUpdate("MOVE", "2");
                    }
                }
            }
            yield return new WaitForSeconds(.2f);
        }
    }
}