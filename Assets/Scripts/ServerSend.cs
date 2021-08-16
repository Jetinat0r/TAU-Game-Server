using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;


class ServerSend
{
    private static void SendTCPData(int _toClient, Packet _packet)
    {
        _packet.WriteLength();
        Server.clients[_toClient].tcp.SendData(_packet);
    }

    private static void SendUDPData(int _toClient, Packet _packet)
    {
        _packet.WriteLength();
        Server.clients[_toClient].udp.SendData(_packet);
    }

    private static void SendTCPDataToAll(Packet _packet)
    {
        _packet.WriteLength();
        for (int i = 1; i <= Server.MaxPlayers; i++)
        {
            Server.clients[i].tcp.SendData(_packet);
        }
    }

    private static void SendTCPDataToAll(int _exceptClient, Packet _packet)
    {
        _packet.WriteLength();
        for (int i = 1; i <= Server.MaxPlayers; i++)
        {
            if (i != _exceptClient)
            {
                Server.clients[i].tcp.SendData(_packet);
            }
        }
    }

    private static void SendUDPDataToAll(Packet _packet)
    {
        _packet.WriteLength();
        for (int i = 1; i <= Server.MaxPlayers; i++)
        {
            Server.clients[i].udp.SendData(_packet);
        }
    }

    private static void SendUDPDataToAll(int _exceptClient, Packet _packet)
    {
        _packet.WriteLength();
        for (int i = 1; i <= Server.MaxPlayers; i++)
        {
            if (i != _exceptClient)
            {
                Server.clients[i].udp.SendData(_packet);
            }
        }
    }

    #region Packets
    public static void Welcome(int _toClient, string _msg)
    {
        using (Packet _packet = new Packet((int)ServerPackets.welcome))
        {
            _packet.Write(_msg);
            _packet.Write(_toClient);

            SendTCPData(_toClient, _packet);
        }
    }

    public static void UDPTest(int _toClient)
    {
        using (Packet _packet = new Packet((int)ServerPackets.udpTest))
        {
            _packet.Write("A test packet for UDP.");

            SendUDPData(_toClient, _packet);
        }
    }

    public static void SpawnPlayer(int _toClient, Player _player)
    {
        using (Packet _packet = new Packet((int)ServerPackets.spawnPlayer))
        {
            _packet.Write(_player.id);
            _packet.Write(_player.username);

            _packet.Write(_player.transform.position);
            _packet.Write(_player.pivotRotation);

            _packet.Write(_player.curWeapon);
            _packet.Write(_player.playerColor);

            // Spawn location index is "- 1" bc player id's are 1-10, and indexes are 0-9
            _packet.Write(GameManager.instance.spawnLocations[_player.id - 1].position);

            SendTCPData(_toClient, _packet);
        }
    }

    public static void PlayerMovement(Player _player)
    {
        using (Packet _packet = new Packet((int)ServerPackets.playerMovement))
        {
            _packet.Write(_player.id);
            _packet.Write(_player.transform.position);
            _packet.Write(_player.pivotRotation);

            SendUDPDataToAll(_player.id, _packet);
        }
    }

    public static void PlayerDisconnected(int _playerId)
    {
        using (Packet _packet = new Packet((int)ServerPackets.playerDisconnected))
        {
            _packet.Write(_playerId);
            _packet.Write(Server.clients[_playerId].player.gameRole);

            SendTCPDataToAll(_packet);
        }
    }

    public static void RemoteDisconnect(int _playerId, string _msg)
    {
        using (Packet _packet = new Packet((int)ServerPackets.remoteDisconnect))
        {
            _packet.Write(_playerId);
            _packet.Write(_msg);

            SendTCPData(_playerId, _packet);
        }
    }

    public static void SetRole(int _playerId, int _role)
    {
        using (Packet _packet = new Packet((int)ServerPackets.setRole))
        {
            _packet.Write(_playerId);
            _packet.Write(_role);

            SendTCPDataToAll(_packet);
        }
    }

    public static void RemoteWeaponSwap(int _playerId, int _weapon)
    {
        using (Packet _packet = new Packet((int)ServerPackets.remoteWeaponSwap))
        {
            _packet.Write(_playerId);
            _packet.Write(_weapon);

            SendTCPDataToAll(_playerId, _packet);
        }
    }

    public static void SpawnBullet(Vector3 _position, Quaternion _rotation, float _velocity, Vector3 _shotPosOffset)
    {
        using (Packet _packet = new Packet((int)ServerPackets.spawnBullet))
        {
            _packet.Write(_position);
            _packet.Write(_rotation);
            _packet.Write(_velocity);
            _packet.Write(_shotPosOffset);

            SendTCPDataToAll(_packet);
        }
    }

    public static void DamagePlayer(int _playerId, float _amount)
    {
        using (Packet _packet = new Packet((int)ServerPackets.damagePlayer))
        {
            _packet.Write(_playerId);
            _packet.Write(_amount);

            SendTCPData(_playerId, _packet);
        }
    }

    public static void RemoteDeath(int _playerId, int _causeOfDeath)
    {
        //Causes Of Death:
        //0: Being Shot
        //1: Being Ejected

        using (Packet _packet = new Packet((int)ServerPackets.remoteDeath))
        {
            _packet.Write(_playerId);
            _packet.Write(_causeOfDeath);

            SendTCPDataToAll(_packet);
        }
    }

    public static void StartRound(int _numActivePlayers, int _numInnocents, int[] _roleArray, int _tasksPerPlayer, float _playerSpeed, float _visionRadius)
    {
        using (Packet _packet = new Packet((int)ServerPackets.startRound))
        {
            _packet.Write(_numActivePlayers);
            _packet.Write(_numInnocents);
            _packet.Write(_roleArray);
            _packet.Write(_tasksPerPlayer);
            _packet.Write(_playerSpeed);
            _packet.Write(_visionRadius);

            SendTCPDataToAll(_packet);
        }
    }

    public static void AssignEmergency(int _emergencyID)
    {
        using (Packet _packet = new Packet((int)ServerPackets.assignEmergency))
        {
            _packet.Write(_emergencyID);

            SendTCPDataToAll(_packet);
        }
    }

    public static void RemoteCompleteEmergency(int _emergencyID)
    {
        using (Packet _packet = new Packet((int)ServerPackets.remoteCompleteEmergency))
        {
            _packet.Write(_emergencyID);

            SendTCPDataToAll(_packet);
        }
    }

    public static void RemoteTeleport(int _playerID, Vector3 _targetPos)
    {
        using (Packet _packet = new Packet((int)ServerPackets.remoteTeleport))
        {
            _packet.Write(_playerID);
            _packet.Write(_targetPos);

            SendTCPDataToAll(_packet);
        }
    }

    public static void RemoteCompleteTask(int _playerID)
    {
        using (Packet _packet = new Packet((int)ServerPackets.remoteCompleteTask))
        {
            _packet.Write(_playerID);

            SendTCPDataToAll(_packet);
        }
    }

    public static void RemoteSendVoiceChat(int _playerID, float[] voiceSamples, int samples, int channels, int maxFreq, bool isRadioActive)
    {
        //Debug.Log(" 3123");

        using (Packet _packet = new Packet((int)ServerPackets.remoteSendVoiceChat))
        {
            _packet.Write(_playerID);

            _packet.Write(voiceSamples);
            _packet.Write(samples);
            _packet.Write(channels);
            _packet.Write(maxFreq);
            _packet.Write(isRadioActive);

            SendTCPDataToAll(_playerID, _packet);
        }

        //foreach (float f in voiceSamples)
        //{
        //    Debug.Log(f);
        //}
    }
    #endregion
}
