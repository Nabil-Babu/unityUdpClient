﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using System.Net.Sockets;
using System.Net;

public class NetworkMan : MonoBehaviour
{
    public UdpClient udp;
    public bool localhostTesting = false; 
    private string myServerIP = "23.22.122.54";
    private string localHost = "localhost";
    void Start()
    {
        udp = new UdpClient();
        if(localhostTesting)
        {
            udp.Connect(localHost,12345);
        }
        else 
        {
            udp.Connect(myServerIP,12345);
        }
            
        Byte[] sendBytes = Encoding.ASCII.GetBytes("connect");
      
        udp.Send(sendBytes, sendBytes.Length);

        udp.BeginReceive(new AsyncCallback(OnReceived), udp);

        InvokeRepeating("HeartBeat", 1, 1);
    }

    void OnDestroy(){
        udp.Close();
        udp.Dispose();
    }


    public enum commands{
        NEW_CLIENT,
        UPDATE,
        DELETE
    };
    
    [Serializable]
    public class Message{
        public commands cmd;
    }
    
    [Serializable]
    public class Player
    {
        [Serializable]
        public struct receivedColor
        {
            public float R;
            public float G;
            public float B;
        }
        public string id;
        public receivedColor color;        
    }

    [Serializable]
    public class ServerClient
    {
        public DateTime lastBeat = new DateTime();
        public float color;
    }


    [Serializable]
    public class NewPlayer{
        public Player player;
    }

    [Serializable]
    public class GameState{
        public Player[] players;
    }
    [Serializable]
    public class ServerState{
        public ServerClient[] clients;
    }

    public Message latestMessage;
    public GameState lastestGameState;
    public ServerState latestServerState;
    public List<GameObject> allPlayers = new List<GameObject>(); 
    public GameObject playerObject;
    public NewPlayer newPlayer; 
    void OnReceived(IAsyncResult result)
    {
        // this is what had been passed into BeginReceive as the second parameter:
        UdpClient socket = result.AsyncState as UdpClient;
        
        // points towards whoever had sent the message:
        IPEndPoint source = new IPEndPoint(0, 0);

        // get the actual message and fill out the source:
        byte[] message = socket.EndReceive(result, ref source);
        
        // do what you'd like with `message` here:
        string returnData = Encoding.ASCII.GetString(message);
        Debug.Log("Got this: " + returnData);
        
        latestMessage = JsonUtility.FromJson<Message>(returnData);
        try
        {
            switch(latestMessage.cmd)
            {
                case commands.NEW_CLIENT:
                    latestServerState = JsonUtility.FromJson<ServerState>(returnData);
                    break;
                case commands.UPDATE:
                    lastestGameState = JsonUtility.FromJson<GameState>(returnData);
                    break;
                case commands.DELETE:
                    Debug.Log("Player has Dropped");
                    break;
                default:
                    Debug.Log("Error");
                    break;
            }
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
        }
        
        // schedule the next receive operation once reading is done:
        socket.BeginReceive(new AsyncCallback(OnReceived), socket);
    }

    void SpawnPlayers()
    {
        if(lastestGameState.players.Length > allPlayers.Count)
        {
            foreach (Player player in lastestGameState.players)
            {
                bool spawnPlayer = true; 
                foreach(GameObject inGamePlayer in allPlayers)
                {
                    if(player.id == inGamePlayer.GetComponent<PlayerID>().ID)
                    {
                        spawnPlayer = false;
                    }
                }

                if(spawnPlayer)
                {
                    GameObject nPlayer = Instantiate(playerObject, new Vector3(UnityEngine.Random.Range(-10,10), 0, 0), Quaternion.identity);
                    nPlayer.GetComponent<PlayerID>().ID = player.id;
                    nPlayer.GetComponent<Renderer>().material.color = new Color(player.color.R, player.color.G, player.color.B);
                    allPlayers.Add(nPlayer);
                }
            }
        }
    }

    void UpdatePlayers()
    {
        foreach (GameObject player in allPlayers)
        {
            foreach (Player serverPlayer in lastestGameState.players)
            {
                if(serverPlayer.id == player.GetComponent<PlayerID>().ID)
                {
                    player.GetComponent<Renderer>().material.color = new Color(serverPlayer.color.R, serverPlayer.color.G, serverPlayer.color.B);
                }
            }
        }
    }

    void DestroyPlayers()
    {
        foreach (GameObject inGamePlayer in allPlayers)
        {
            bool destroyPlayer = true; 
            foreach (Player serverPlayer in lastestGameState.players)
            {
                if(serverPlayer.id == inGamePlayer.GetComponent<PlayerID>().ID)
                {
                   destroyPlayer = false;
                }
            }
            if(destroyPlayer)
            {
                Destroy(inGamePlayer);
                allPlayers.Remove(inGamePlayer);
                allPlayers.TrimExcess();
            }
        }
    }
    
    void HeartBeat()
    {
        Byte[] sendBytes = Encoding.ASCII.GetBytes("heartbeat");
        udp.Send(sendBytes, sendBytes.Length);
    }

    void Update()
    {
        SpawnPlayers();
        UpdatePlayers();
        DestroyPlayers();
    }
}
