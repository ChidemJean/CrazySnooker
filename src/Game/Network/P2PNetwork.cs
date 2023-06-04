using Godot;
using System;
using System.Text;
using CrazySnooker.Game.Controllers;

namespace CrazySnooker.Game.Network
{
   public class P2PNetwork : Node
   {
      [Export]
      private int HOST_PORT = 61535;

      [Export]
      private string HOST_IP = "127.0.0.1";

      HTTPRequest httpRequest;
      MainScene mainScene;
      public bool isConnected = false;
      public bool isHosting = false;

      [Export]
      private NodePath playerPath;

      [Export]
      private NodePath player2Path;

      private PoolCueController player1;
      private PoolCueController player2;

      public override void _Ready()
      {
         mainScene = GetNode<MainScene>("/root/MainScene");
         httpRequest = GetNode<HTTPRequest>("%HTTPRequest");
         httpRequest.Connect("request_completed", this, "OnRequestCompleted");
         player1 = GetNode<PoolCueController>(playerPath);
         player2 = GetNode<PoolCueController>(player2Path);
         isConnected = false;

         GetTree().Connect("connected_to_server", this, nameof(OnConnectToServer));
         GetTree().Connect("network_peer_connected", this, nameof(OnPeerConnected));
         GetTree().Connect("network_peer_disconnected", this, nameof(OnPeerDisconnected));
         GetTree().Connect("server_disconnected", this, nameof(OnServerDisconnected));
      }

      public void Host()
      {
         var host = new NetworkedMultiplayerENet();
         host.CreateServer(HOST_PORT);
         GetTree().NetworkPeer = host;
         GetPublicIP();
         isHosting = true;
         mainScene.ChangeState(MainScene.GameState.CONNECTING);
      }

      public void Join(string ip = "127.0.0.1")
      {
         var host = new NetworkedMultiplayerENet();
         host.CreateClient(ip, HOST_PORT);
         GetTree().NetworkPeer = host;
         mainScene.ChangeState(MainScene.GameState.CONNECTING);
      }

      public void SendMessage()
      {
         string message = "Teste message";
         int id = GetTree().GetNetworkUniqueId();
         Rpc("ReceiveMessage", id, message);
      }

      public async void ReceiveMessage(int id, string message)
      {
         GD.Print($"{id}: {message}");
      }

      public void OnConnectToServer() 
      {
        isConnected = true;
        mainScene.ChangeState(isHosting ? MainScene.GameState.WAITING_OPPONENT : MainScene.GameState.CONNECTED);
        player1.playerID = GetTree().GetNetworkUniqueId();
      }

      public void OnPeerConnected(int id) 
      {
        if (id != player1.playerID) {
            player2.playerID = id;
            mainScene.ChangeState(MainScene.GameState.CONNECTED);
        }
      }

      public void OnPeerDisconnected(int id) 
      {
        if (id != player1.playerID) {
            player2.playerID = -1;
            GD.Print("oponente disconectado");
        }
      }

      public void OnServerDisconnected() 
      {
        GD.Print("servidor disconectado");
      }

      public async void GetPublicIP()
      {
         httpRequest.Request("https://api.ipify.org", null, true, HTTPClient.Method.Get, "format=json");
      }

      public async void OnRequestCompleted(int result, int response_code, string[] headers, byte[] body)
      {
         string ip = Encoding.UTF8.GetString(body);
         mainScene.EmitSignal("UpdateIP", ip);
         //  JSONParseResult json = JSON.Parse(Encoding.UTF8.GetString(body));
      }

   }
}