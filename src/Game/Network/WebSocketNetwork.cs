using CrazySnooker.Game.Network.Messages;
using Godot;
using System;
using CrazySnooker.Game.Managers;
using CrazySnooker.Utils;
using MessagePack;

namespace CrazySnooker.Game.Network
{
   public class WebSocketNetwork : Node, INetwork
   {
      [Export]
      public string SOCKET_URL = "ws://127.0.0.1:9001";

      WebSocketClient client = new WebSocketClient();

      MainScene mainScene;

      public bool isConnected = false;

      private GameManager gameManager;

      const int INIT_MATCH = 1;
      const int UPDATE_ID = 2;
      const int PASS_TURN = 3;
      const int UPDATE_BALLS = 4;
      const int MOVE_CUE = 5;
      const int ROTATE_CUE = 6;
      const int SHOT = 7;

      MessagePackSerializerOptions lz4Options;

      public override void _Ready()
      {
         gameManager = GetNode<GameManager>("%GameManager");
         mainScene = GetNode<MainScene>("/root/MainScene");
         client.Connect("connection_closed", this, nameof(OnConnectionClosed));
         client.Connect("connection_error", this, nameof(OnConnectionError));
         client.Connect("connection_established", this, nameof(OnConnected));
         client.Connect("data_received", this, nameof(OnDataReceived));

         mainScene.Connect("JoinServer", this, nameof(Join));

         lz4Options = MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray);

         SetProcess(false);
      }

      public override void _Process(float delta)
      {
         client.Poll();
      }

      public void OnConnectionClosed(bool wasClean = false)
      {
         GD.Print($"Closed, clead: {wasClean}");
         SetProcess(false);
      }

      public void OnConnectionError()
      {
         GD.Print($"Connection failed");
         SetProcess(false);
      }

      public void OnConnected(string proto = "")
      {
         GD.Print($"Connected with protocol: {proto}");
         isConnected = true;
         mainScene.ChangeState(MainScene.GameState.WAITING_OPPONENT);
      }

      public void OnDataReceived()
      {
         byte[] payloadBytes = client.GetPeer(1).GetPacket();
         int payloadSize = payloadBytes.Length;
         
         if (payloadSize > 200) {
            UpdatePackage updateState = MessagePackSerializer.Deserialize<UpdatePackage>(payloadBytes, lz4Options);
            gameManager.BallsPackageReceive(updateState);
            return;
         }

         var payload = JSON.Parse(payloadBytes.GetStringFromUTF8()).Result;
         
         if (payload is Godot.Collections.Dictionary)
         {
            Godot.Collections.Dictionary dict = (Godot.Collections.Dictionary)payload;
            int type = Convert.ToInt32((Single) dict["type"]);

            switch (type)
            {
               case UPDATE_ID:
                  int id = ((String) dict["id"]).ToInt();
                  gameManager.playerYou.UpdateID(id);
                  break;
               case INIT_MATCH:
						int idOpponent = ((String) dict["id"]).ToInt();
						int idFirstTurn = ((String) dict["idTurn"]).ToInt();
                  gameManager.playerOpponent.UpdateID(idOpponent);
						mainScene.ChangeState(MainScene.GameState.CONNECTED);
                  gameManager.UpdateTurn(idFirstTurn);
                  break;
               case PASS_TURN:
                  int idTurn = gameManager.playerYou.playerID;
                  gameManager.UpdateTurn(idTurn);
                  break;
               case MOVE_CUE:
                  int move = Convert.ToInt32((Single) dict["move"]);
                  gameManager.playerOpponent.Move(move);
                  break;
               case ROTATE_CUE:
                  Vector3 rotation = new Vector3((float) dict["x"], (float) dict["y"], (float) dict["z"]);
                  gameManager.playerOpponent.UpdateRotationCue(rotation);
                  break;
               case SHOT:
                  gameManager.playerOpponent.Shot();
                  break;
            }
         }
      }

      public void Join(string ip)
      {
         Error err = client.ConnectToUrl(SOCKET_URL);
         if (err != Error.Ok)
         {
            GD.Print("Unable to connect");
            SetProcess(false);
            return;
         }
         SetProcess(true);
         mainScene.ChangeState(MainScene.GameState.CONNECTING);
      }

      public void SendBallsState(UpdatePackage updatePackage)
      {
         byte[] bytesState = MessagePackSerializer.Serialize(updatePackage, lz4Options);
         client.GetPeer(1).PutPacket(bytesState);
      }

      public void SendMoveCue(int move)
      {
         Godot.Collections.Dictionary message = new Godot.Collections.Dictionary();
         message["type"] = MOVE_CUE;
         message["move"] = move;
         client.GetPeer(1).PutPacket(JSON.Print(message).ToUTF8());
      }

      public void SendRotationCue(Vector3 rotation)
      {
         Godot.Collections.Dictionary message = new Godot.Collections.Dictionary();
         message["type"] = ROTATE_CUE;
         message["x"] = rotation.x;
         message["y"] = rotation.y;
         message["z"] = rotation.z;
         client.GetPeer(1).PutPacket(JSON.Print(message).ToUTF8());
      }

      public void SendShot()
      {
         Godot.Collections.Dictionary message = new Godot.Collections.Dictionary();
         message["type"] = SHOT;
         client.GetPeer(1).PutPacket(JSON.Print(message).ToUTF8());
      }

      public void SendUpdateTurn(int idTurn)
      {
         Godot.Collections.Dictionary message = new Godot.Collections.Dictionary();
         message["type"] = PASS_TURN;
         client.GetPeer(1).PutPacket(JSON.Print(message).ToUTF8());
      }
   }
}