using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NETWORK_ENGINE;
using UnityEngine.UI;
using TMPro;

public class PlayerControllerScript : NetworkComponent
{
    public Vector2 lastMove;
    public float speed = 4;
    public bool canJump;
    public bool isHurt = false;
    public float jumpSpeed = 8;

    public string color;

    public int health;
    public int maxHealth = 10;

    public int attackDamage = 1;
    public bool canAttack = true;
    public bool isAttack = false;

    public PotionScript potionSlot;
    public SpellScript spellSlot;
    public List<ItemScript> itemsInRange = new List<ItemScript>();
    public ItemScript[] items;
    public Image potionIcon;
    public Image spellIcon;
    public Image healthBar;
    public GameObject ui;
    public GameObject pauseMenu;

    public Rigidbody2D rig;
    public SpriteRenderer rend;
    public Animator anim;

    public float respawnTime;
    public float attackTime = .5f;
    public Vector3 spawnLocation;
    public float hurtCooldown = 0.8f;
    public bool dead = false;

    public int playerKills;
    public int enemyKills;
    public int bossKills;

    public float moveVal;
    public int dir = 1;

    public AudioClip hurt;
    public AudioClip attack;
    public AudioClip potion;
    public AudioClip jump;
    public AudioClip portalOpen;

    public AudioSource myNoise;

    public override void HandleMessage(string flag, string value)
    {
        if (flag == "MOVE")
        {
            if(IsServer && !dead)
            {
                moveVal = float.Parse(value);
                // Set lastmove based on received input
                lastMove = new Vector2(moveVal, 0) * speed;

                // Set direction player is facing
                if (moveVal > 0)
                {
                    dir = 1;
                    SendUpdate("MOVE", "1");
                }
                else if (moveVal < 0)
                {
                    dir = -1;
                    SendUpdate("MOVE", "-1");
                }
            }
            if(IsClient)
            {
                int temp = int.Parse(value);
                if(temp == 1)
                {
                    rend.flipX = false;
                }
                else
                {
                    rend.flipX = true;
                }
            }
        }

        if (flag == "HURT")
        {
            if (IsClient)
            {
                StartCoroutine(Hurt(hurtCooldown));
            }
        }

        if (flag == "JUMP")
        {
            if(IsServer && canJump && !dead)
            {
                canJump = false;
                rig.velocity += new Vector2(0, jumpSpeed);
                SendUpdate("JUMP", canJump.ToString());
            }
            if(IsClient)
            {
                canJump = bool.Parse(value);
                if(IsLocalPlayer && canJump == false)
                {
                     myNoise.clip = jump;
                     myNoise.PlayOneShot(myNoise.clip);
                }
            }
        }

        if(flag == "ATTACK")
        {
            if(IsServer && !dead)
            {
                // If can attack start attack coroutine on client and server
                if(canAttack)
                {
                    isAttack = true;
                    canAttack = false;
                    SendUpdate("ATTACK", "1");
                    StartCoroutine(Attack(attackTime));
                }
            }
            if(IsClient)
            {
                StartCoroutine(Attack(attackTime));
            }
        }

        if(flag == "HEALTH")
        {
            // Update health variable and healthbar
            if(IsClient)
            {
                health = int.Parse(value);
            }
            if (IsLocalPlayer)
            {
                healthBar.fillAmount = (float)health / (float)maxHealth;
            }
        }

        if(flag == "DEAD")
        {
            // Play dead animation
            if(IsClient)
            {
                StartCoroutine(OnDeath());
            }
        }

        if(flag == "SWAP")
        {
            if (IsServer && !dead)
            {
                if (itemsInRange.Count > 0)
                {
                    // If item is a potion
                    if (itemsInRange[0].GetComponent<PotionScript>() != null)
                    {
                        // Get item id and assign potion script to potion slot
                        if (potionSlot != null)
                        {
                            // Spawn potion currently in inventory
                            int id = MyCore.FindType(potionSlot.name);
                            if (id != -1)
                            {
                                GameObject temp = MyCore.NetCreateObject(id, -1, new Vector2(transform.position.x + (0.85f * dir), transform.position.y), Quaternion.identity);
                                temp.GetComponent<Rigidbody2D>().velocity = new Vector2(3 * dir, 1);
                            }
                            else
                            {
                                Debug.Log("Incorrect name for item to swap");
                            }
                        }
                        potionSlot = items[itemsInRange[0].GetComponent<ItemScript>().id].GetComponent<PotionScript>();
                    }
                    // Else item is a spell
                    else
                    {
                        // Get item id and assign spell script to spell slot
                        if (spellSlot != null)
                        {
                            // Spawn spell currently in inventory
                            int id = MyCore.FindType(spellSlot.name);
                            if(id != -1)
                            {
                                GameObject temp = MyCore.NetCreateObject(id, -1, new Vector2(transform.position.x + (0.85f * dir), transform.position.y), Quaternion.identity);
                                temp.GetComponent<Rigidbody2D>().velocity = new Vector2(3 * dir, 1);
                            }
                            else
                            {
                                Debug.Log("Incorrect name for item to swap");
                            }
                        }
                        spellSlot = items[itemsInRange[0].GetComponent<ItemScript>().id].GetComponent<SpellScript>();
                    }
                    SendUpdate("SWAP", itemsInRange[0].GetComponent<ItemScript>().id.ToString());
                    // Destroy item from world
                    itemsInRange[0].RemoveFromWorld();
                }
            }

            if(IsLocalPlayer)
            {
                int img = int.Parse(value);
                // Set client inventory images
                if(items[img].GetComponent<PotionScript>() != null)
                {
                    potionIcon.sprite = items[img].GetComponent<PotionScript>().icon;
                    potionIcon.color = new Color32(255, 255, 255, 255);
                }
                else
                {
                    spellIcon.sprite = items[img].GetComponent<SpellScript>().icon;
                    spellIcon.color = new Color32(255, 255, 255, 255);
                }
            }
        }

        if(flag == "POTION")
        {
            // When potion is used call use in item and then update inventory icon on client
            if (IsServer && potionSlot != null && !dead)
            {
                if(potionSlot.throwable)
                {
                    GameObject temp = MyCore.NetCreateObject(MyCore.FindType(potionSlot.name), -1, new Vector2(transform.position.x + (dir * 0.85f), transform.position.y), Quaternion.identity);
                    temp.GetComponent<Rigidbody2D>().velocity = new Vector2(speed * dir, 1);
                    temp.GetComponent<PotionScript>().player = this;
                    temp.GetComponent<PotionScript>().thrown = true;
                }
                else
                {
                    potionSlot.Use(GetComponent<PlayerControllerScript>());
                }
                potionSlot = null;
                SendUpdate("POTION", "1");
            }
            if(IsLocalPlayer)
            {
                potionIcon.sprite = null;
                potionIcon.color = new Color32(255, 255, 255, 0);
                myNoise.clip = potion;
                myNoise.PlayOneShot(myNoise.clip);
            }
        }

        if(flag == "SPELL")
        {
            // When spell is used call use in item and then update inventory icon on client
            if (IsServer && spellSlot != null && !dead)
            {
                ProjectileScript temp = MyCore.NetCreateObject(MyCore.FindType(spellSlot.projectile.name), -1, new Vector2(transform.position.x + (dir * 0.85f), transform.position.y), Quaternion.identity).GetComponent<ProjectileScript>();
                temp.instigator = this;
                temp.dir = dir;
                temp.speed = spellSlot.speed;
                temp.damage = spellSlot.damage;
                temp.damageType = spellSlot.damageType;
                temp.FlipSprite(dir);
                spellSlot = null;
                SendUpdate("SPELL", "1");
            }
            if (IsLocalPlayer)
            {
                spellIcon.sprite = null;
                spellIcon.color = new Color32(255, 255, 255, 0);
            }
        }

        if (flag == "PAUSE")
        {
            if (IsServer)
            {
                SendUpdate("PAUSE", "1");
            }
            if (IsLocalPlayer)
            {
                if (pauseMenu.activeSelf)
                {
                    pauseMenu.SetActive(false);
                }
                else
                {
                    pauseMenu.SetActive(true);
                }
            }
        }

        if(flag == "PORTAL")
        {
            if (IsLocalPlayer)
            {
                myNoise.clip = portalOpen;
                myNoise.PlayOneShot(myNoise.clip);
            }
        }
    }

    public override void NetworkedStart()
    {
        if (IsLocalPlayer)
        {
            // Set axis callbacks
            AxisEventCallers.current.OnDirectionChanged += OnMove;
            AxisEventCallers.InputEvents["Jump"].OnAxisKeyDown += OnJump;
            AxisEventCallers.InputEvents["Fire1"].OnAxisKeyDown += OnAttack;
            AxisEventCallers.InputEvents["UsePotion"].OnAxisKeyDown += OnUsePotion;
            AxisEventCallers.InputEvents["UseSpell"].OnAxisKeyDown += OnUseSpell;
            AxisEventCallers.InputEvents["Swap"].OnAxisKeyDown += OnSwapItem;
            AxisEventCallers.InputEvents["Cancel"].OnAxisKeyDown += OnPause;
        }
        if(IsServer)
        {
            // Set starting health
            health = maxHealth;
        }
        if(!IsLocalPlayer)
        {
            ui.SetActive(false);
        }
    }

    public IEnumerator Hurt(float timer)
    {
        if (IsClient)
        {
            anim.SetBool("hurt", true);
            if(IsLocalPlayer)
            {
                myNoise.clip = hurt;
                myNoise.PlayOneShot(myNoise.clip);
            }
        }
        yield return new WaitForSeconds(timer);
        if (IsClient)
        {
            anim.SetBool("hurt", false);
        }
        if (IsServer)
        {
            canAttack = true;
            isHurt = false;
        }
    }

    public override IEnumerator SlowUpdate()
    {
        while(IsConnected)
        {
            // Get current input; if it is zero send to server
            // Current fix to running without input
            if(IsLocalPlayer)
            {
                if(moveVal == 0)
                {
                    SendCommand("MOVE", 0.ToString());
                }
                else if(moveVal == 1)
                {
                    SendCommand("MOVE", 1.ToString());
                }
            }
            if(IsServer && transform.position.y < -30)
            {
                ReceiveDamage(0, 9999, null);
            }
            if(IsServer && IsDirty)
            {
                SendUpdate("HEALTH", health.ToString());
            }
            yield return new WaitForSeconds(.1f);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        rig = GetComponent<Rigidbody2D>();
        if (rig == null)
        {
            throw new System.Exception("Rigid body is null");
        }
        anim = this.GetComponent<Animator>();
        rend = this.GetComponent<SpriteRenderer>();
        myNoise = this.GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        // On server check for can jump, update velocity
        if (IsServer)
        {
            // Continually update position
            rig.velocity = lastMove + new Vector2(0, rig.velocity.y);

            // Check for jump
            if (!canJump)
            {
                RaycastHit2D hit = Physics2D.Raycast(transform.position, -Vector2.up);
                if (hit.collider != null)
                {
                    if (Vector2.Distance(transform.position, hit.point) < 1f && rig.velocity.y <= 0)
                    {
                        canJump = true;
                        SendUpdate("JUMP", canJump.ToString());
                    }
                }
            }
        }

        // On client update animations
        if (IsClient)
        {
            // Set walk if moving in x axis
            if (Mathf.Abs(rig.velocity.x) > .05f)
            {
                anim.SetBool("walk", true);
            }
            else
            {
                anim.SetBool("walk", false);
            }

            // Set jump if moving in y axis
            if (!canJump || rig.velocity.y > 0 || rig.velocity.y < 0)
            {
                anim.SetBool("jump", true);
            }
            else
            {
                anim.SetBool("jump", false);
            }
        }

        // On local player update camera
        if (IsLocalPlayer)
        {
            // Camera follows player
            Camera.main.transform.position = new Vector3(this.transform.position.x, this.transform.position.y, -8);
        }
    }

    public IEnumerator Attack(float timer)
    {
        if(IsClient)
        {
            anim.SetBool("attack", true);
            if (IsLocalPlayer)
            {
                myNoise.clip = attack;
                myNoise.PlayOneShot(myNoise.clip);
            }
        }

        if(IsServer)
        {
            isAttack = true;
            // Raycast to see if enemy or player is in range
            // If there is an enemy or player deal damage to them
            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.right * dir);
            if (hit.collider != null)
            {
                if (Mathf.Abs(transform.position.x - hit.point.x) <= (2.5 + GetComponent<CapsuleCollider2D>().size.x / 2))
                {
                    if(hit.collider.tag == "Enemy" && hit.collider != null)
                    {
                        hit.collider.GetComponent<EnemyControllerScript>().ReceiveDamage(0, attackDamage, this);
                    }
                    if (hit.collider.tag == "Player" && hit.collider != null)
                    {
                        hit.collider.GetComponent<PlayerControllerScript>().ReceiveDamage(0, attackDamage, this);
                    }
                }
            }
        }
        
        yield return new WaitForSeconds(timer);
        anim.SetBool("attack", false);
        canAttack = true;
        isAttack = false;
    }

    public void OnJump()
    {
        SendCommand("JUMP", "1");
    }

    public void OnMove()
    {
        moveVal = Input.GetAxis("Horizontal");
        SendCommand("MOVE", moveVal.ToString("F3"));
    }

    public void OnAttack()
    {
        SendCommand("ATTACK", "1");
    }

    public void OnSwapItem()
    {
        SendCommand("SWAP", "1");
    }

    public void OnUsePotion()
    {
        SendCommand("POTION", "1");
    }

    public void OnUseSpell()
    {
        SendCommand("SPELL", "1");
    }

    public void OnPause()
    {
        SendCommand("PAUSE", "1");
    }

    public void ReceiveDamage(int damageType, int damage, PlayerControllerScript instigator)
    {
        if (IsServer)
        {
            // Deal damage based on received type
            // This will be expanded for all of our potion effects.
            if(!isHurt)
            {
                switch (damageType)
                {
                    case 0: // Normal Damage
                        health -= damage;
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
                isHurt = true;
                canAttack = false;
                StartCoroutine(Hurt(hurtCooldown));

                SendUpdate("HEALTH", health.ToString());
                SendUpdate("HURT", "1");

                if (health <= 0)
                {
                    if(instigator != null)
                    {
                        instigator.playerKills += 1;
                    }
                    SendUpdate("DEAD", "1");
                    StartCoroutine(OnDeath());
                }
            }
        }
    }

    public IEnumerator PotionTick(string type, float amount, PlayerControllerScript instigator)
    {
        if(IsServer)
        {
            switch(type)
            {
                case "Speed":
                    speed += amount;
                    lastMove = new Vector2(moveVal, 0) * speed;
                    yield return new WaitForSeconds(4);
                    speed -= amount;
                    lastMove = new Vector2(moveVal, 0) * speed;
                    break;
                case "Poison":
                    int i = 0;
                    while(i < amount)
                    {
                        i++;
                        health--;

                        SendUpdate("HEALTH", health.ToString());
                        SendUpdate("HURT", "1");

                        if (health <= 0)
                        {
                            if (instigator != null)
                            {
                                instigator.playerKills += 1;
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

    public IEnumerator OnDeath()
    {
        if(IsServer)
        {
            dead = true;
        }
        anim.SetBool("dead", true);
        yield return new WaitForSeconds(respawnTime);
        anim.SetBool("dead", false);

        if(IsServer)
        {
            // Reset health
            health = maxHealth;
            // Respawn at last teleporter
            transform.position = spawnLocation;
            // Set dead to false
            dead = false;
        }
    }

    public void Quit()
    {
        FindObjectOfType<GameManagerScript>().EndRun(this, true);
        MyCore.UI_Quit();
    }

    public void PlayPortalSound()
    {
        if(IsServer)
        {
            SendUpdate("PORTAL", "1");
        }
    }
}
