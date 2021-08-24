using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Stores info about rounds and what not
public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public bool roundStarted = false;

    public Color[] playerColors;
    public int playersPerTraitor;
    public int tasksPerPlayer;

    [Header("Player Vars")]
    public float playerSpeed;
    public float visionRadius;

    [Header("Round Vars")]
    private bool hasACurrentImportantEmergency = false;
    private int mostRecentEmergencyID = -1;
    public float emergencyMeetingTimer = 180f;
    //Used to account for network timing
    public float emergencyMeetingTimerOvertime = 3f;
    //The wait-for so that players can read who voted for who
    public float emergencyMeetingCloseTimer = 5f;

    [Header("Emergency Meeting Vars")]
    private bool isMeetingActive = false;
    private float activeMeetingTimer;

    private List<int> targetPlayers;
    private List<Color> fromPlayers;

    [Header("Teleport Locations")]
    public List<Transform> spawnLocations;
    public List<Transform> roundStartLocations;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.Log("Instance already exists, destroying!");
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (isMeetingActive)
        {
            activeMeetingTimer -= Time.deltaTime;

            if(activeMeetingTimer <= 0)
            {
                EndEmergencyMeeting();
            }
        }

        #region Dev Keys
        //if (Input.GetKeyDown(KeyCode.I))
        //{
        //    //foreach (KeyValuePair<int , Client> item in Server.clients)
        //    //{
        //    //    Debug.Log(item.Key + " " + item.Value.player.name);
        //    //}
        //    StartRound();
        //}

        //if (Input.GetKeyDown(KeyCode.T))
        //{
        //    int x = 0;
        //    foreach (KeyValuePair<int, Client> _item in Server.clients)
        //    {
        //        Debug.Log(_item.Key + " " + _item.Value);

        //        if(_item.Value.player != null)
        //        {
        //            ServerSend.RemoteTeleport(_item.Value.player.id, new Vector3(x, 0, 0));
        //        }

        //        x += 25;
        //    }
        //}
        #endregion
    }

    #region Start Round Stuff
    public void StartRound()
    {
        roundStarted = true;
        NetworkManager.instance.roundStarted = true;

        #region Setup
        int _numActivePlayers = 0;
        List<int> _playerIds = new List<int>();
        foreach (KeyValuePair<int, Client> _client in Server.clients)
        {
            //Check num cur players, [each key.player != null]
            if (_client.Value.player != null)
            {
                _numActivePlayers++;
                _playerIds.Add(_client.Value.player.id);
            }
        }
        #endregion

        #region Roles
        //Sets up roles so they will be synced across all clients
        int[] _playerIdsArray = new int[_playerIds.Count];
        for(int i = 0; i < _playerIds.Count; i++)
        {
            _playerIdsArray[i] = _playerIds[i];
        }
        Array.Sort(_playerIdsArray);

        int[] _roleArray = SetRoles(_playerIdsArray);
        #endregion

        #region Tasks
        //Assign task amounts to player shells
        foreach (KeyValuePair<int, Client> _client in Server.clients)
        {
            Player _player = _client.Value.player;
            //Check num cur players, [each key.player != null]
            if (_player != null)
            {
                _player.AssignTasks(tasksPerPlayer);
            }
        }
        #endregion

        ServerSend.StartRound(DetermineNumInnocents(_numActivePlayers), _numActivePlayers, _roleArray, tasksPerPlayer, playerSpeed, visionRadius);

        #region MoveToStartLocation
        foreach (KeyValuePair<int, Client> _item in Server.clients)
        {
            if (_item.Value.player != null)
            {
                ServerSend.RemoteTeleport(_item.Value.player.id, roundStartLocations[_item.Value.player.id - 1].position);
            }
        }
        #endregion
    }

    //Sets a role for each player in an array that will be sent to everyone
    private int[] SetRoles(int[] _activePlayerIds)
    {
        int _numActivePlayers = _activePlayerIds.Length;
        int[] _roles = new int[_numActivePlayers];

        //int _traitorsRemaining = 0;

        //Makes a default array the size of activePlayers
        for (int i = 0; i < _roles.Length; i++)
        {
            _roles[i] = 1;
        }

        //Creates an array of roles to be sent
        while (_numActivePlayers >= playersPerTraitor)
        {
            int _arrayItem = UnityEngine.Random.Range(0, _roles.Length);

            if (_roles[_arrayItem] != 2)
            {
                _roles[_arrayItem] = 2;
                _numActivePlayers -= playersPerTraitor;
            }
            //_traitorsRemaining++;
        }

        //Sets the roles server side
        for (int i = 0; i < _activePlayerIds.Length; i++)
        {
            Player _player = Server.clients[_activePlayerIds[i]].player;
            if (_player != null)
            {
                _player.gameRole = _roles[i];
            }
        }

        return _roles;
    }

    private int DetermineNumInnocents(int _numActivePlayers)
    {
        int _traitors = 0;
        int _numTotal = _numActivePlayers;
        while (_numActivePlayers >= playersPerTraitor)
        {
            _traitors++;
            _numActivePlayers -= playersPerTraitor;
        }

        return _numTotal - _traitors;
    }

    public void CheckRoundStart()
    {
        if (!roundStarted)
        {
            //Get a list of all active players
            List<Player> activePlayers = new List<Player>();

            foreach (KeyValuePair<int, Client> pair in Server.clients)
            {
                if (pair.Value.player != null)
                {
                    activePlayers.Add(pair.Value.player);
                }
            }

            if (activePlayers.Count < 3)
            {
                return;
            }

            //Check if they have voted
            foreach (Player player in activePlayers)
            {
                //IF: player is not found to have voted, do not end meeting
                if (!player.isReady)
                {
                    return;
                }
            }

            StartRound();
        }
    }
    #endregion


    #region Emergency Task Stuff
    public void ProcessEmergencyStartRequest(int _emergencyID, bool _isFirst)
    {
        if (_isFirst && hasACurrentImportantEmergency)
        {
            return;
        }

        mostRecentEmergencyID = _emergencyID;
        ServerSend.AssignEmergency(_emergencyID);
    }

    //Covers two people fixing the same emergency while a traitor starts it up again, niche application but still good to fix
    public void ProcessEmergencyCompletion(int _emergencyID)
    {
        if (!hasACurrentImportantEmergency)
        {
            return;
        }

        if(_emergencyID == mostRecentEmergencyID)
        {
            ServerSend.RemoteCompleteEmergency(_emergencyID);
        }
    }
    #endregion


    #region Emergency Meeting Stuff
    public void StartEmergencyMeeting()
    {
        targetPlayers = new List<int>();
        fromPlayers = new List<Color>();

        activeMeetingTimer = emergencyMeetingTimer + emergencyMeetingTimerOvertime;
        isMeetingActive = true;
    }

    public void AddVote(int fromPlayerID, int targetPlayerID)
    {
        Player fromPlayer = Server.clients[fromPlayerID].player;
        if (targetPlayerID == 0)
        {
            if(fromPlayer != null)
            {
                targetPlayers.Add(targetPlayerID);
                fromPlayers.Add(fromPlayer.playerColor);
            }
        }
        else
        {
            Player targetPlayer = Server.clients[targetPlayerID].player;
            if (targetPlayer != null && fromPlayer != null)
            {
                targetPlayers.Add(targetPlayerID);
                fromPlayers.Add(fromPlayer.playerColor);
            }
        }

        CheckMeetingAllVotes();
    }

    public void EndEmergencyMeeting()
    {
        isMeetingActive = false;

        #region Determine who was voted for
        //Determine who got voted for
        //Default is "skip"
        int mostFrequentID = 0;
        int maxCount = 0;
        int curCount = 0;

        for (int i = 0; i < targetPlayers.Count; i++)
        {
            for (int j = i; j < targetPlayers.Count; j++)
            {
                if (targetPlayers[i] == targetPlayers[j])
                {
                    curCount++;
                }
            }

            if (curCount > maxCount)
            {
                mostFrequentID = targetPlayers[i];
                maxCount = curCount;
            }
            else if (curCount == maxCount)
            {
                mostFrequentID = 0;
            }

            curCount = 0;
        }
        #endregion

        ServerSend.RemoteSendMeetingVotes(targetPlayers, fromPlayers, emergencyMeetingCloseTimer);

        if(mostFrequentID != 0)
        {
            StartCoroutine(KillVotedPlayer(mostFrequentID, emergencyMeetingCloseTimer));
        }
    }

    private IEnumerator KillVotedPlayer(int playerToKill, float timeToKill)
    {
        yield return new WaitForSeconds(timeToKill);

        ServerSend.RemoteDeath(playerToKill, 1);
    }

    public void CheckMeetingAllVotes()
    {
        if (isMeetingActive)
        {
            //Check if all players have voted

            //Get all active players
            List<Player> activePlayers = new List<Player>();

            foreach (KeyValuePair<int, Client> pair in Server.clients)
            {
                if (pair.Value.player != null && pair.Value.player.isAlive)
                {
                    activePlayers.Add(pair.Value.player);
                }
            }

            //Check if they have voted
            foreach (Player player in activePlayers)
            {
                //IF: player is not found to have voted, do not end meeting
                if (!fromPlayers.Contains(player.playerColor))
                {
                    return;
                }
            }

            EndEmergencyMeeting();
        }
    }
    #endregion
}
