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
            Debug.Log("Player " + _username + "  (ID: " + _fromClient + ") has assumed the wrong client ID (" + _clientIdCheck + ")!");
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

    public static void ClientEmergencyStartRequest(int _fromClient, Packet _packet)
    {
        //I think its pretty obvious what this is intended to do
        //*Foreshadowing lmao*

        int _emergencyID = _packet.ReadInt();

        //Used to separate important and un-important emergencies
        //if isFirst == false, it is NOT important
        bool _isFirst = _packet.ReadBool();

        //TODO: Handle this
        //Ask the Game manager if it can send a new emergency out
        GameManager.instance.ProcessEmergencyStartRequest(_emergencyID, _isFirst);
    }

    public static void ClientCompleteEmergency(int _fromClient, Packet _packet)
    {
        int _emergencyID = _packet.ReadInt();

        //Used to separate important and un-important emergencies
        //if isFirst == true, it is important
        bool _isFirst = _packet.ReadBool();

        if (_isFirst)
        {
            GameManager.instance.ProcessEmergencyCompletion(_emergencyID);
            return;
        }

        ServerSend.RemoteCompleteEmergency(_emergencyID);
    }

    public static void ClientCompleteTask(int _fromClient, Packet _packet)
    {
        //int _index = _packet.ReadInt();

        Server.clients[_fromClient].player.completedTasks++;
        //TODO: IMPLEMETN GameManager.CheckTaskWin()

        ServerSend.RemoteCompleteTask(_fromClient);
    }

    public static void ClientSendVoiceChat(int _fromClient, Packet _packet)
    {
        float[] voiceSamples = _packet.ReadFloatArray();
        int samples = _packet.ReadInt();
        int channels = _packet.ReadInt();
        int maxFreq = _packet.ReadInt();
        bool isRadioActive = _packet.ReadBool();

        //Debug.Log("samples:" + samples);
        ServerSend.RemoteSendVoiceChat(_fromClient, voiceSamples, samples, channels, maxFreq, isRadioActive);
        
    }

    public static void ClientStartEmergencyMeeting(int _fromClient, Packet _packet)
    {
        //Teleports everyone to the table
        foreach(KeyValuePair<int, Client> keyPair in Server.clients)
        {
            Player _player = keyPair.Value.player;
            if(_player != null)
            {
                ServerSend.RemoteTeleport(_player.id, GameManager.instance.roundStartLocations[_player.id - 1].position);
            }
        }

        //Starts the meeting
        GameManager.instance.StartEmergencyMeeting();
        ServerSend.RemoteStartEmergencyMeeting(_fromClient);
    }

    public static void ClientSendMeetingVote(int _fromClient, Packet _packet)
    {
        int targetPlayerID = _packet.ReadInt();

        GameManager.instance.AddVote(_fromClient, targetPlayerID);
    }
}
