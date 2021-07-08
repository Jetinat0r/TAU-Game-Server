using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class ServerHandle
{
    public static void WelcomeReceived(int _fromClient, Packet _packet)
    {
        int _clientIdCheck = _packet.ReadInt();
        string _username = _packet.ReadString();

        Debug.Log(Server.clients[_fromClient].tcp.socket.Client.RemoteEndPoint + " connected successfully and is now player " + _fromClient + ".");
        if (_fromClient != _clientIdCheck)
        {
            Debug.Log("Player " + _username + " (ID: " + _fromClient + ") has assumed the wrong client ID (" + _clientIdCheck + ")!");
        }

        Server.clients[_fromClient].SendIntoGame(_username);
    }

    public static void UDPTestReceived(int _fromClient, Packet _packet)
    {
        string _msg = _packet.ReadString();

        Debug.Log("Received packet from client id: (" + _fromClient + ") via UDP. Contains message: " + _msg);
    }

    public static void PlayerPosRot(int _fromClient, Packet _packet)
    {
        Vector3 _position = _packet.ReadVector3();
        Quaternion _rotation = _packet.ReadQuaternion();

        Server.clients[_fromClient].player.SetPosRot(_position, _rotation);
    }

    public static void WeaponSwap(int _fromClient, Packet _packet)
    {
        int _weapon = _packet.ReadInt();

        Server.clients[_fromClient].player.WeaponSwap(_weapon);
    }

    public static void Shoot(int _fromClient, Packet _packet)
    {
        //TODO?: IF IT LOOKS JANK, DON'T SEND THIS TO THE FROM CLIENT
        Vector3 _position = _packet.ReadVector3();
        Quaternion _rotation = _packet.ReadQuaternion();
        float _velocity = _packet.ReadFloat();

        Server.clients[_fromClient].player.Shoot(_position, _rotation, _velocity);
    }
}
