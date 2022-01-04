using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NETWORK_ENGINE;
using UnityEngine.AI;

public class EnemyControllerScript : NetworkComponent
{
    public int health;
    public int maxHealth = 10;

    public bool canAttack = true;
    public int attackDamage = 1;
    public float attackTime =0.5f;

    public bool isHurt = false;
    public float hurtCooldown = 1.0f;

    public float viewDistance;
    public float moveSpeed;

    public Rigidbody2D rig;
    public Animator anim;
    public SpriteRenderer rend;

    public int room;

    public int enemyDir = -1;
    public bool canJump;
    public bool noticed = false;


    public GameObject[] heroes;
    public RaycastHit2D hit1;
    public RaycastHit2D hit2;
    public RaycastHit2D hit3;
    public RaycastHit2D hit4;
    public int unnoticed;
    public RaycastHit2D myHit;
    public string name = "";

    public AudioClip enemAttack;
    public AudioSource soundCreator;

    public override void HandleMessage(string flag, string value)
    {
        if(flag == "HURT")
        {
            if(IsClient)
            {
                StartCoroutine(Hurt(hurtCooldown));
            }
        }

        if (flag == "ATTACK")
        {
            if (IsServer)
            {
                canAttack = false;
                SendUpdate("ATTACK", "1");
                StartCoroutine(Attack(attackTime));
            }
            if (IsClient)
            {
                SendCommand("ATTACK", "1");
                soundCreator.clip = enemAttack;
                soundCreator.PlayOneShot(soundCreator.clip);
                StartCoroutine(Attack(attackTime));
            }
        }

        if(flag == "DEAD")
        {
            if (IsClient)
            {
                StartCoroutine(OnDeath());
            }
        }

        if (flag == "MOVE" && IsClient)
        {
            if (value == "1" || value == "2")
            {
                rend.flipX = false;
            }
        }

        if(flag == "REVMOVE" && IsClient)
        {
            if (value == "1" || value == "2")
            {
                rend.flipX = true;
            }
        }

        if (flag == "JUMP" && IsServer)
        {
            canJump = false;
            rig.velocity += new Vector2(0, 5);
        }

        if(flag == "NOMOVE" && IsClient)
        {

        }
    }

    public override void NetworkedStart()
    {
        if(IsServer)
        {
            health = maxHealth;
        }
    }

    public override IEnumerator SlowUpdate()
    {
        while (IsServer)
        {
            heroes = GameObject.FindGameObjectsWithTag("Player");
            if (heroes.Length >= 1 && heroes[0] != null)
            {
                if(name != "Wizard" && name != "Dragon" && name != "Bandit")
                {
                    hit1 = Physics2D.Raycast(new Vector2(this.transform.position.x, heroes[0].GetComponent<Rigidbody2D>().transform.position.y), Vector2.left * enemyDir);
                }
                else
                {
                    hit1 = Physics2D.Raycast(new Vector2(this.transform.position.x, heroes[0].GetComponent<Rigidbody2D>().transform.position.y), Vector2.left * enemyDir);
                }
            }
            if (heroes.Length >= 2 && heroes[1] != null)
            {
                if (name != "Wizard" && name != "Dragon" && name != "Bandit")
                {
                    hit2 = Physics2D.Raycast(new Vector2(this.transform.position.x, heroes[1].GetComponent<Rigidbody2D>().transform.position.y), Vector2.left * enemyDir);
                }
                else
                {
                    hit2 = Physics2D.Raycast(new Vector2(this.transform.position.x, heroes[1].GetComponent<Rigidbody2D>().transform.position.y), Vector2.left * enemyDir);
                }
            }
            if (heroes.Length >= 3 && heroes[2] != null)
            {
                if (name != "Wizard" && name != "Dragon" && name != "Bandit")
                {
                    hit3 = Physics2D.Raycast(new Vector2(this.transform.position.x, heroes[2].GetComponent<Rigidbody2D>().transform.position.y), Vector2.left * enemyDir);
                }
                else
                {
                    hit3 = Physics2D.Raycast(new Vector2(this.transform.position.x, heroes[2].GetComponent<Rigidbody2D>().transform.position.y), Vector2.left * enemyDir);
                }
            }
            if (heroes.Length >= 4 && heroes[3] != null)
            {
                if (name != "Wizard" && name != "Dragon" && name != "Bandit")
                {
                    hit4 = Physics2D.Raycast(new Vector2(this.transform.position.x, heroes[3].GetComponent<Rigidbody2D>().transform.position.y), Vector2.left * enemyDir);
                }
                else
                {
                    hit4 = Physics2D.Raycast(new Vector2(this.transform.position.x, heroes[3].GetComponent<Rigidbody2D>().transform.position.y), Vector2.left * enemyDir);
                }
            }

            if (hit1.collider != null)
            {
                if (Mathf.Abs(transform.position.x - hit1.point.x) < (viewDistance + GetComponent<BoxCollider2D>().size.x/2) && (Mathf.Abs(transform.position.y-hit1.point.y) <= 2 || name == "Dragon")) //We measure distance between xs
                {
                    if (hit1.collider.tag == "Player") //We check if the first player has been hit, setting myHit to them if so.
                    {
                        myHit = hit1;
                        noticed = true;
                    }
                    else
                    {
                        unnoticed += 1;
                    }
                }
                else
                {
                    unnoticed += 1;
                }
            }
            else
            {
                unnoticed += 1;
            }

            if (heroes.Length >= 2 && hit2.collider != null)
            {
                if (Mathf.Abs(transform.position.x - hit2.point.x) < (viewDistance + GetComponent<BoxCollider2D>().size.x/2) && (Mathf.Abs(transform.position.y - hit2.point.y) <= 2 || name == "Dragon")) //We measure distance between xs
                {
                    if (hit2.collider.tag == "Player")
                    {
                        if (noticed && myHit == hit1)
                        {
                            if(Mathf.Abs(hit2.point.x-transform.position.x) < Mathf.Abs(hit1.point.x-transform.position.x))
                            {
                                myHit = hit2;
                            }
                        }
                        else if(noticed == false)
                        {
                            noticed = true;
                            myHit = hit2;
                        }
                    }
                    else
                    {
                        unnoticed += 1;
                    }
                }
                else
                {
                    unnoticed += 1;
                }
            }
            else
            {
                unnoticed += 1;
            }

            if (heroes.Length >= 3 && hit3.collider != null)
            {
                if (Mathf.Abs(transform.position.x - hit3.point.x) < (viewDistance + GetComponent<BoxCollider2D>().size.x/2) && (Mathf.Abs(transform.position.y - hit3.point.y) <= 2 || name == "Dragon")) //We measure distance between xs
                {
                    if (hit3.collider.tag == "Player")
                    {
                        if (noticed && myHit == hit1)
                        {
                            if (Mathf.Abs(hit3.point.x - transform.position.x) < Mathf.Abs(hit1.point.x - transform.position.x))
                            {
                                myHit = hit3;
                            }
                        }
                        else if(noticed && myHit == hit2)
                        {
                            if (Mathf.Abs(hit3.point.x - transform.position.x) < Mathf.Abs(hit2.point.x - transform.position.x))
                            {
                                myHit = hit3;
                            }
                        }
                        else if (noticed == false)
                        {
                            noticed = true;
                            myHit = hit3;
                        }
                    }
                    else
                    {
                        unnoticed += 1;
                    }
                }
                else
                {
                    unnoticed += 1;
                }
            }
            else
            {
                unnoticed += 1;
            }

            if (heroes.Length >= 4 && hit4.collider != null)
            {
                if (Mathf.Abs(transform.position.x - hit4.point.x) < (viewDistance + GetComponent<BoxCollider2D>().size.x/2) && (Mathf.Abs(transform.position.y - hit4.point.y) <= 2 || name == "Dragon")) //We measure distance between xs
                {
                    if (hit4.collider.tag == "Player")
                    {
                        if (noticed && myHit == hit1)
                        {
                            if (Mathf.Abs(hit4.point.x - transform.position.x) < Mathf.Abs(hit1.point.x - transform.position.x))
                            {
                                myHit = hit4;
                            }
                        }
                        else if (noticed && myHit == hit2)
                        {
                            if (Mathf.Abs(hit4.point.x - transform.position.x) < Mathf.Abs(hit2.point.x - transform.position.x))
                            {
                                myHit = hit4;
                            }
                        }
                        else if (noticed && myHit == hit3)
                        {
                            if (Mathf.Abs(hit4.point.x - transform.position.x) < Mathf.Abs(hit3.point.x - transform.position.x))
                            {
                                myHit = hit4;
                            }
                        }
                        else if (noticed == false)
                        {
                            noticed = true;
                            myHit = hit4;
                        }
                    }
                    else
                    {
                        unnoticed += 1;
                    }
                }
                else
                {
                    unnoticed += 1;
                }
            }
            else
            {
                unnoticed += 1;
            }
            if (unnoticed == 4) //Here, we check if all four enemies were unnoticed. If so, we have no choice but to change noticed to false.
            {
                noticed = false;
            }
            unnoticed = 0;
            yield return new WaitForSeconds(.1f);
        }
    }

    // Start is called before the first frame update
    public virtual void Start()
    {
        rig = this.GetComponent<Rigidbody2D>();
        if(rig == null)
        {
            throw new System.Exception("Could not find rigid body.");
        }
        //We gather the more easily referenced components.
        anim = this.GetComponent<Animator>();
        rend = this.GetComponent<SpriteRenderer>();
        soundCreator = this.GetComponent<AudioSource>();
    }

    // Update is called once per frame
    public virtual void Update()
    {
        if(IsServer && (rig.position.y <= -30))
        {
            ReceiveDamage(0, 99999, null);
        }
        if(IsClient)
        {
            if (Mathf.Abs(rig.velocity.x) > .05f)
            {
                anim.SetBool("walk", true);
            }
            else
            {
                anim.SetBool("walk", false);
            }
        }
    }

    public IEnumerator Attack(float timer)
    {
        if(IsClient)
        {
            anim.SetBool("attack", true);
        }
        yield return new WaitForSeconds(timer);
        if(IsClient)
        {
            anim.SetBool("attack", false);
        }
        yield return new WaitForSeconds(timer);
        if(IsServer)
        {
            canAttack = true;
        }
    }

    public IEnumerator OnDeath()
    {
        anim.SetBool("dead", true);
        yield return new WaitForSeconds(1);
        if(name == "Dragon")
        {
            yield return new WaitForSeconds(1.5f);
        }

        if (IsServer)
        {
            GameManagerScript temp = FindObjectOfType<GameManagerScript>();
            temp.aliveEnemies.Remove(this);
            temp.CheckPortals(this.room);
            MyCore.NetDestroyObject(MyId.NetId);
        }
    }

    public IEnumerator Hurt(float timer)
    {
        if(IsClient)
        {
            anim.SetBool("hurt", true);
        }
        if(IsServer)
        {
            canAttack = false;
            isHurt = true;
        }
        yield return new WaitForSeconds(timer);
        if(IsClient)
        {
            anim.SetBool("hurt", false);
        }
        if(IsServer)
        {
            canAttack = true;
            isHurt = false;
        }
    }
    
    //This is called when receiving any kind of damage
    public void ReceiveDamage(int damageType, int damage, PlayerControllerScript instigator)
    {
        //Server should handle game state changes
        if (IsServer)
        {
            //Enemies can NOT be hurt more when they are already in their hurt state.
            if(!isHurt)
            {
                switch (damageType)
                {
                    //Basic physical non-potion damage
                    case 0:
                        //We detract this and send an update to the server.
                        health = health - damage;
                        break;
                    case 1: // Heal
                        if (health + damage > maxHealth)
                        {
                            health = maxHealth;
                        }
                        else
                        {
                            health += damage;
                        }
                        break;
                    case 2: // Speed
                        StartCoroutine(PotionTick("Speed", damage, instigator));
                        break;
                    case 3: // Poison
                        StartCoroutine(PotionTick("Poison", damage, instigator));
                        break;
                    default:
                        break;
                }
                //we need the server to know it is hurt too!
                isHurt = true;
                canAttack = false;
                StartCoroutine(Hurt(hurtCooldown));
                //We want the animation to play
                SendUpdate("HURT", "1");
                // Check for death
                if (health <= 0)
                {
                    if(instigator != null)
                    {
                        if(GetComponent<BossScript>() != null)
                        {
                            instigator.bossKills += 1;
                        }
                        else
                        {
                            instigator.enemyKills += 1;
                        }
                    }
                    SendUpdate("DEAD", "1");
                    StartCoroutine(OnDeath());
                }
            }
        }
    }

    public IEnumerator PotionTick(string type, float amount, PlayerControllerScript instigator)
    {
        if (IsServer)
        {
            switch (type)
            {
                case "Speed":
                    moveSpeed += amount;
                    yield return new WaitForSeconds(4);
                    moveSpeed -= amount;
                    break;
                case "Poison":
                    int i = 0;
                    while (i < amount)
                    {
                        i++;
                        health--;

                        SendUpdate("HURT", "1");

                        if (health <= 0)
                        {
                            if (instigator != null)
                            {
                                if (GetComponent<BossScript>() != null)
                                {
                                    instigator.bossKills += 1;
                                }
                                else
                                {
                                    instigator.enemyKills += 1;
                                }
                            }
                            SendUpdate("DEAD", "1");
                            StartCoroutine(OnDeath());
                        }
                        yield return new WaitForSeconds(1);
                    }
                    break;
                default:
                    yield return new WaitForSeconds(.1f);
                    break;
            }
        }
    }
}
