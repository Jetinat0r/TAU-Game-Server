using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager instance;

    public int maxPlayers = 10;
    public int port = 7777;

    public GameObject playerPrefab;

    [Header("Game variables")]
    public bool roundStarted;

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

    private void Start()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 50;

        Server.Start(maxPlayers, port);
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.O))
        {
            foreach(Client _client in Server.clients.Values)
            {
                if (_client.player != null)
                {
                    ServerSend.RemoteDisconnect(_client.id, "Nothin personal, kid");
                }
            }
        }
    }

    private void OnApplicationQuit()
    {
        Server.Stop();
    }

    public Player InstantiatePlayer()
    {
        return Instantiate(playerPrefab, Vector3.zero, Quaternion.identity).GetComponent<Player>();
    }
}
