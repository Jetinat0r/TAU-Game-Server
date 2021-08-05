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
        if (Input.GetKeyDown(KeyCode.I))
        {
            //foreach (KeyValuePair<int , Client> item in Server.clients)
            //{
            //    Debug.Log(item.Key + " " + item.Value.player.name);
            //}
            StartRound();
        }
    }

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
}
