using Godot;
using System;
using CrazySnooker.Game.Controllers;
using CrazySnooker.Game.Network;

namespace CrazySnooker 
{

    public class MainScene : Control
    {
        [Export]
        private NodePath ipLabelPath;
        private Label ipLabel;

        [Export]
        private NodePath hostCtnPath;
        private Control hostCtn;

        [Export]
        private NodePath enterIpPath;
        private Control enterIp;

        [Export]
        private NodePath enterIpInputPath;
        private LineEdit enterIpInput;

        [Export]
        private NodePath helpBoxPath;
        private Control helpBox;

        [Export]
        private NodePath loadingPath;
        private Label loading;

        [Signal]
        public delegate void UpdateIP(string ip);

        public enum GameState { INITIAL, STARTING_SERVER, WAITING_OPPONENT, WAITING_IP_INPUT, CONNECTING, CONNECTED }

        public GameState state = GameState.INITIAL;

        public P2PNetwork network;

        public override void _Ready()
        {
            ipLabel = GetNode<Label>(ipLabelPath);
            hostCtn = GetNode<Control>(hostCtnPath);
            enterIp = GetNode<Control>(enterIpPath);
            loading = GetNode<Label>(loadingPath);
            network = GetNode<P2PNetwork>("%P2PNetwork");
            helpBox = GetNode<Control>(helpBoxPath);
            enterIpInput = GetNode<LineEdit>(enterIpInputPath);

            Input.MouseMode = Input.MouseModeEnum.Visible;
            
            Connect("UpdateIP", this, nameof(OnUpdateIP));
        }

        public void OnUpdateIP(string ip = null)
        {
            if (ip == null) {
                hostCtn.Visible = false;
                return;
            }
            hostCtn.Visible = true;
            ipLabel.Text = ip;
        }

        public override void _Input(InputEvent _event)
        {
            if (_event is InputEventKey) {
                var eventKey = (InputEventKey) _event;
                if (!eventKey.IsPressed()) {
                    if (eventKey.Scancode == (uint) KeyList.Escape) {
                        GetTree().Quit();
                        return;
                    }
                    if (state == GameState.INITIAL) {
                        if (eventKey.Scancode == (uint) KeyList.H) {
                            ChangeState(GameState.STARTING_SERVER);
                        }
                        if (eventKey.Scancode == (uint) KeyList.J) {
                            ChangeState(GameState.WAITING_IP_INPUT);
                        }
                    }
                }
            }
        }

        public void ChangeState(GameState newState)
        {
            switch (newState) {
                case GameState.STARTING_SERVER: 
                    network.Host();
                    helpBox.Visible = false;
                    break;
                case GameState.WAITING_IP_INPUT: 
                    loading.Visible = false;
                    enterIp.Visible = true;
                    break;
                case GameState.WAITING_OPPONENT: 
                    hostCtn.Visible = true;
                    loading.Visible = false;
                    break;
                case GameState.CONNECTING:
                    enterIp.Visible = false;
                    loading.Visible = true;
                    break;
                case GameState.CONNECTED: 
                    loading.Visible = false;
                    break;
            }
        }

        public void OnBtnEnterClicked()
        {
            EnterServer();
        }

        public void EnterServer()
        {
            string ip = enterIpInput.Text;
            if (ip == "") {
                network.Join();
            } else {
                network.Join(ip);
            }
        }
    }
}