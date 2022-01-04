using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NETWORK_ENGINE;
using TMPro;

public class Score
{
    public string color;
    public float time;
    public int enemyKills, playerKills, bossKills;
    public bool dnf;

    public int fscore = 0;
    public string scoreString = "";

    public Score(string color, float time, int enemyKills, int playerKills, int bossKills, bool dnf)
    {
        this.color = color;
        this.time = time;
        this.enemyKills = enemyKills;
        this.playerKills = playerKills;
        this.bossKills = bossKills;
        this.dnf = dnf;
    }

    public void CalculateScore(int place)
    {
        // Give score for time
        switch(place)
        {
            case 1:
                fscore += 20;
                break;
            case 2:
                fscore += 18;
                break;
            case 3:
                fscore += 15;
                break;
            case 4:
                fscore += 12;
                break;
            default:
                break;
        }

        // Add score for kills
        fscore += enemyKills;
        fscore += playerKills;
        fscore += 5 * bossKills;
    }
}

public class GameManagerScript : NetworkComponent
{
    public bool gameStarted;
    public GameObject uiBackground;
    public GameObject victoryBackground;
    public GameObject pauseMenu;

    public int roomCount = 3;
    public List<GameObject> rooms = new List<GameObject>();
    List<GameObject> spawnedRooms = new List<GameObject>();
    public List<GameObject> melee = new List<GameObject>();
    public List<GameObject> ranged = new List<GameObject>();
    public List<GameObject> boss = new List<GameObject>();

    List<TeleporterScript> portalList = new List<TeleporterScript>();
    List<TeleporterScript> entranceList = new List<TeleporterScript>();
    public List<EnemyControllerScript> aliveEnemies = new List<EnemyControllerScript>();
    public List<PlayerControllerScript> alivePlayers = new List<PlayerControllerScript>();

    public bool gameOver = false;
    public float timeStart;

    List<Score> playerScores = new List<Score>();

    public Vector2 playerSpawn;
    
    public override void HandleMessage(string flag, string value)
    {
        if (flag == "GAMESTART")
        {
            gameStarted = true;
            uiBackground.SetActive(false);
        }

        if(flag == "GAMEOVER")
        {
            if(IsClient)
            {
                string temp = value;
                temp = temp.Replace(";", "\n");
                victoryBackground.GetComponentInChildren<TMP_Text>().text = temp;
                victoryBackground.SetActive(true);
            }
        }
    }

    public override void NetworkedStart()
    {
        if(IsServer)
        {
            MyCore.MaxConnections = 4;
        }
    }

    public override IEnumerator SlowUpdate()
    {
        // Once connected display lobby ui
        if (IsConnected)
        {
            uiBackground.SetActive(true);
        }
        // Before game start
        while (!gameStarted && IsServer)
        {
            // Check if all players are ready
            bool readyGo = true;
            int count = 0;
            foreach (LobbyPlayerScript lp in FindObjectsOfType<LobbyPlayerScript>())
            {
                if (!lp.ready)
                {
                    readyGo = false;
                    break;
                }
                count++;
            }
            if (count < 1)
            {
                readyGo = false;
            }
            gameStarted = readyGo;
            yield return new WaitForSeconds(1);
        }

        // Once game starts disable lobby ui, spawn rooms, teleporters, and all players
        if (IsServer)
        {
            // Remove from server list
            MyCore.NotifyGameStart();

            // Spawn rooms
            for (int i = 0; i < roomCount; i++)
            {
                int temp = Random.Range(0, rooms.Count);
                spawnedRooms.Add(MyCore.NetCreateObject(MyCore.FindType(rooms[temp].name), -1, new Vector3(i * 100, 0, 0), Quaternion.identity));
            }
            
            // Spawn all objects from eggs
            GameObject[] eggs = GameObject.FindGameObjectsWithTag("Egg");
            foreach(GameObject egg in eggs)
            {
                // Get room that egg is in
                int parentRoom = spawnedRooms.IndexOf(egg.transform.parent.gameObject);
                EggScript e = egg.GetComponent<EggScript>();
                // Spawn gameobject based on egg type
                if (e.type == "Melee")
                {
                    int temp = Random.Range(0, melee.Count);
                    EnemyControllerScript ec = MyCore.NetCreateObject(MyCore.FindType(melee[temp].name), -1, new Vector3(egg.transform.position.x, egg.transform.position.y, 0), Quaternion.identity).GetComponent<EnemyControllerScript>();
                    ec.room = parentRoom;
                }
                else if(e.type == "Ranged")
                {
                    int temp = Random.Range(0, ranged.Count);
                    EnemyControllerScript ec = MyCore.NetCreateObject(MyCore.FindType(ranged[temp].name), -1, new Vector3(egg.transform.position.x, egg.transform.position.y, 0), Quaternion.identity).GetComponent<EnemyControllerScript>();
                    ec.room = parentRoom;
                }
                else if (e.type == "Boss")
                {
                    int temp = Random.Range(0, boss.Count);
                    EnemyControllerScript ec = MyCore.NetCreateObject(MyCore.FindType(boss[temp].name), -1, new Vector3(egg.transform.position.x, egg.transform.position.y, 0), Quaternion.identity).GetComponent<EnemyControllerScript>();
                    ec.room = parentRoom;
                }
                else if (e.type == "Portal")
                {
                    TeleporterScript tc = MyCore.NetCreateObject(MyCore.FindType("Portal"), -1, new Vector3(egg.transform.position.x, egg.transform.position.y, 0), Quaternion.identity).GetComponent<TeleporterScript>();
                    tc.room = parentRoom;
                    if (e.isEntrance)
                    {
                        tc.isEntrance = true;
                    }
                }
                else if (e.type == "Loot")
                {
                    MyCore.NetCreateObject(MyCore.FindType("CommonChest"), -1, new Vector3(egg.transform.position.x, egg.transform.position.y, 0), Quaternion.identity);
                }
            }

            // Get List of all alive enemies
            EnemyControllerScript[] enemies = FindObjectsOfType<EnemyControllerScript>();
            foreach(EnemyControllerScript enemy in enemies)
            {
                aliveEnemies.Add(enemy);
            }

            // Set up portals
            TeleporterScript[] portals = FindObjectsOfType<TeleporterScript>();
            
            // Split portals into entrance array and normal portal array
            foreach(TeleporterScript portal in portals)
            {
                if(portal.isEntrance)
                {
                    entranceList.Add(portal);
                    // If first entrance set to player spawn
                    if(portal.room == 0)
                    {
                        playerSpawn = portal.transform.position;
                    }
                }
                else
                {
                    portalList.Add(portal);
                }
            }
            // Set all non entrance portals to teleport to next entrance
            int lastRoom = spawnedRooms.Count - 1;
            foreach(TeleporterScript portal in portalList)
            {
                foreach(TeleporterScript entrance in entranceList)
                {
                    if(entrance.room == portal.room + 1)
                    {
                        portal.teleportTo = entrance.transform.position;
                    }
                    else if(portal.room == lastRoom)
                    {
                        // If non-entrance portal in last room it is player goal
                        portal.isExit = true;
                    }
                }
            }

            // Spawn all players
            int pNum = 0;
            foreach (LobbyPlayerScript lp in FindObjectsOfType<LobbyPlayerScript>())
            {
                PlayerControllerScript temp;
                switch (pNum)
                {
                    case 0:
                        temp = MyCore.NetCreateObject(MyCore.FindType("PlayerRed"), lp.Owner, playerSpawn, Quaternion.identity).GetComponent<PlayerControllerScript>();
                        break;
                    case 1:
                        temp = MyCore.NetCreateObject(MyCore.FindType("PlayerBlue"), lp.Owner, playerSpawn, Quaternion.identity).GetComponent<PlayerControllerScript>();
                        break;
                    case 2:
                        temp = MyCore.NetCreateObject(MyCore.FindType("PlayerGreen"), lp.Owner, playerSpawn, Quaternion.identity).GetComponent<PlayerControllerScript>();
                        break;
                    case 3:
                        temp = MyCore.NetCreateObject(MyCore.FindType("PlayerYellow"), lp.Owner, playerSpawn, Quaternion.identity).GetComponent<PlayerControllerScript>();
                        break;
                    default:
                        temp = null;
                        break;
                }
                
                temp.spawnLocation = playerSpawn;
                alivePlayers.Add(temp);             
                pNum++;
            }

            // Disable all client lobby ui
            SendUpdate("GAMESTART", gameStarted.ToString());

            // Spawn Lava Wall
            MyCore.NetCreateObject(MyCore.FindType("LavaWall"), -1, new Vector3(-100, 0, 0), Quaternion.identity);
        }

        // Game Loop
        timeStart = Time.time;
        while (!gameOver)
        {
            if (IsDirty)
            {
                SendUpdate("GAMESTART", gameStarted.ToString());
                IsDirty = false;
            }
            yield return new WaitForSeconds(1);
        }

        // Calculate scores (Should already be organized by time)
        int placeNum = 1;
        foreach (Score score in playerScores)
        {
            if(!score.dnf)
            {
                score.CalculateScore(placeNum);
                placeNum++;
            }
            else
            {
                // Send dnf for 0 points on time score
                score.CalculateScore(-1);
            }
        }

        // Sort scores by calculated final score
        for(int i = 0; i < playerScores.Count - 1; i++)
        {
            for (int j = i + 1; j < playerScores.Count; j++)
            {
                // find max
                int max = i;
                if (playerScores[j].fscore > playerScores[i].fscore)
                    max = j;

                // swap
                Score temp = playerScores[max];
                playerScores[max] = playerScores[i];
                playerScores[i] = temp;
            }
        }

        // Create string to display to clients
        string scoreString = "";
        placeNum = 1;
        foreach (Score score in playerScores)
        {
            string place;
            switch (placeNum)
            {
                case 1:
                    place = "1st";
                    break;
                case 2:
                    place = "2nd";
                    break;
                case 3:
                    place = "3rd";
                    break;
                case 4:
                    place = "4th";
                    break;
                default:
                    place = "DNF";
                    break;
            }
            if (score.dnf)
            {
                scoreString += place + " " + score.color + ": Score: " + score.fscore + ", Time: DNF" + ", Enemies: " + score.enemyKills + ", Bosses: " + score.bossKills + ", Players: " + score.playerKills;
            }
            else
            {
                scoreString += place + " " + score.color + ": Score: " + score.fscore + ", Time: " + score.time + ", Enemies: " + score.enemyKills + ", Bosses: " + score.bossKills + ", Players: " + score.playerKills;
            }
            scoreString += ";";
            placeNum++;
        }

        // Update victory screen on all clients
        SendUpdate("GAMEOVER", scoreString);

        // Remove Lava Wall
        MyCore.NetDestroyObject(FindObjectOfType<LavaWallScript>().NetId);

        yield return new WaitForSeconds(20);
        // Stop server
        if(IsServer)
        {
            yield return StartCoroutine(MyCore.DisconnectServer());
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        gameStarted = false;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void CheckPortals(int room)
    {
        if(IsServer)
        {
            bool roomClear = true;
            foreach (EnemyControllerScript enemy in aliveEnemies)
            {
                if (enemy.room == room)
                {
                    roomClear = false;
                    break;
                }
            }

            if (roomClear)
            {
                foreach (TeleporterScript portal in portalList)
                {
                    if (portal.room == room)
                    {
                        portal.isActivated = true;
                    }
                }
                foreach(PlayerControllerScript player in FindObjectsOfType<PlayerControllerScript>())
                {
                    player.PlayPortalSound();
                }
            }
        }
    }

    public void EndRun(PlayerControllerScript p, bool dnf)
    {
        if(IsServer && alivePlayers.Contains(p))
        {
            float timeEnd = Time.time;
            Score temp = new Score(p.color, timeEnd - timeStart, p.enemyKills, p.playerKills, p.bossKills, dnf);
            playerScores.Add(temp);
            alivePlayers.Remove(p);

            // If they were the last player, the game is over
            if (!gameOver && alivePlayers.Count <= 0)
            {
                gameOver = true;
            }
            MyCore.NetCreateObject(MyCore.FindType("Spectator"), p.Owner, Vector3.zero, Quaternion.identity);
            MyCore.NetDestroyObject(p.NetId);
        }
    }
}
