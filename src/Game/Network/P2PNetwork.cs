using Godot;
using System;
using System.Text;
using CrazySnooker.Game.Controllers;
using CrazySnooker.Game.Entities.Balls;
using CrazySnooker.Game.Network.Messages;
using MessagePack;

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
      private NodePath playerYouPath;

      [Export]
      private NodePath playerOpponentPath;

      private PoolCueController playerYou;
      private PoolCueController playerOpponent;

      public int playerTurnId = -1;

		public WhiteBall whiteBall;

		private MessagePackSerializerOptions lz4Options;

      public override void _Ready()
      {
         mainScene = GetNode<MainScene>("/root/MainScene");
         httpRequest = GetNode<HTTPRequest>("%HTTPRequest");
         whiteBall = GetNode<WhiteBall>("%WhiteBall");
         httpRequest.Connect("request_completed", this, "OnRequestCompleted");
         playerYou = GetNode<PoolCueController>(playerYouPath);
         playerOpponent = GetNode<PoolCueController>(playerOpponentPath);
         isConnected = false;

         GetTree().Connect("connected_to_server", this, nameof(OnConnectToServer));
         GetTree().Connect("network_peer_connected", this, nameof(OnPeerConnected));
         GetTree().Connect("network_peer_disconnected", this, nameof(OnPeerDisconnected));
         GetTree().Connect("server_disconnected", this, nameof(OnServerDisconnected));

         mainScene.Connect("HostServer", this, nameof(Host));
         mainScene.Connect("JoinServer", this, nameof(Join));

			var lz4Options = MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray);
      }

      public void Host()
      {
         var host = new NetworkedMultiplayerENet();
         host.CreateServer(HOST_PORT);
         GetTree().NetworkPeer = host;
         isHosting = true;
         mainScene.ChangeState(MainScene.GameState.CONNECTING);
         OnConnectToServer();
      }

      public void Join(string ip = "127.0.0.1")
      {
         var host = new NetworkedMultiplayerENet();
         host.CreateClient(ip, HOST_PORT);
         GetTree().NetworkPeer = host;
         mainScene.ChangeState(MainScene.GameState.CONNECTING);
      }

      public void OnConnectToServer()
      {
         if (isHosting) GetPublicIP();
         GD.Print("conectado");
         isConnected = true;
         mainScene.ChangeState(isHosting ? MainScene.GameState.WAITING_OPPONENT : MainScene.GameState.CONNECTED);
         playerYou.UpdateID(GetTree().GetNetworkUniqueId());
      }

      public void OnPeerConnected(int id)
      {
         GD.Print("oponente conectado");
         playerOpponent.UpdateID(id);

         if (isHosting)
         {
            mainScene.ChangeState(MainScene.GameState.CONNECTED);
         }
         InitFirstTurn();
      }

      public void InitFirstTurn()
      {
         if (isHosting)
         {
            int idTurn = playerYou.playerID;
            playerTurnId = idTurn;
            SendInitTurn(idTurn);
            playerYou.ChangeTurn(idTurn);
            playerOpponent.ChangeTurn(idTurn);
         }
      }

      public void OnPeerDisconnected(int id)
      {
         if (id != playerYou.playerID)
         {
            playerOpponent.UpdateID(-1);
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

      public void SendInitTurn(int idTurn)
      {
         int id = GetTree().GetNetworkUniqueId();
         Rpc("ReceiveInitTurn", id, idTurn);
      }

      [Remote]
      public async void ReceiveInitTurn(int id, int idTurn)
      {
         GD.Print($"{id}: turno passou para {idTurn}");
         playerTurnId = idTurn;
         playerYou.ChangeTurn(idTurn);
         playerOpponent.ChangeTurn(idTurn);
      }

      public void SendRotationCue(Vector3 rotation)
      {
         int id = GetTree().GetNetworkUniqueId();
         Rpc("ReceiveRotationCue", id, rotation);
      }

      [Remote]
      public async void ReceiveRotationCue(int id, Vector3 rotation)
      {
         playerOpponent.UpdateRotationCue(rotation);
      }

      public void SendMoveCue(int move)
      {
         int id = GetTree().GetNetworkUniqueId();
         Rpc("ReceiveMoveCue", id, move);
      }

      [Remote]
      public async void ReceiveMoveCue(int id, int move)
      {
         playerOpponent.Move(move);
      }

      public void SendShot()
      {
         int id = GetTree().GetNetworkUniqueId();
         Rpc("ReceiveShot", id);
      }

      [Remote]
      public async void ReceiveShot(int id)
      {
         playerOpponent.Shot();
      }

      public void SendWhiteBallState(BallState whiteBallState)
      {
         var bytesState = MessagePackSerializer.Serialize(whiteBallState, lz4Options);
         Rpc(nameof(ReceiveWhiteBallState), bytesState);
      }

      [Remote]
      public void ReceiveWhiteBallState(byte[] bytesState)
      {
         var state = MessagePackSerializer.Deserialize<BallState>(bytesState, lz4Options);
			whiteBall.networkState = state;
      }
   }
}