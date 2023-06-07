using Godot;
using System;
using CrazySnooker.Game.Controllers;
using CrazySnooker.Game.Network;
using CrazySnooker.Game.Managers;

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

        [Export]
        private NodePath waitingPath;
        private Label waiting;

        [Signal]
        public delegate void UpdateIP(string ip);

        [Signal]
        public delegate void HostServer();

        [Signal]
        public delegate void JoinServer(string ip);

        public enum GameState { INITIAL, STARTING_SERVER, JOIN_SERVER, WAITING_OPPONENT, WAITING_IP_INPUT, CONNECTING, CONNECTED }

        public GameState state = GameState.INITIAL;

        [Export]
        public NodePath gameManagerPath;
        public GameManager gameManager;

        [Export]
        private NodePath winPath;
        private Control win;

        [Export]
        private NodePath losePath;
        private Control lose;

        [Export]
        private NodePath youCategoryPath;
        private Label youCategory;

        [Export]
        private NodePath opponentCategoryPath;
        private Label opponentCategory;

        [Export]
        private NodePath youPath;
        private Control you;

        [Export]
        private NodePath opponentPath;
        private Control opponent;

        [Export]
        private NodePath youQtdPath;
        private Label youQtd;

        [Export]
        private NodePath opponentQtdPath;
        private Label opponentQtd;

        [Export]
        private NodePath entrarBtnPath;
        private Button entrarBtn;

        public override void _Ready()
        {
            gameManager = GetNode<GameManager>(gameManagerPath);
            ipLabel = GetNode<Label>(ipLabelPath);
            hostCtn = GetNode<Control>(hostCtnPath);
            enterIp = GetNode<Control>(enterIpPath);
            loading = GetNode<Label>(loadingPath);
            waiting = GetNode<Label>(waitingPath);
            helpBox = GetNode<Control>(helpBoxPath);
            enterIpInput = GetNode<LineEdit>(enterIpInputPath);
            win = GetNode<Control>(winPath);
            lose = GetNode<Control>(losePath);
            you = GetNode<Control>(youPath);
            opponent = GetNode<Control>(opponentPath);
            youCategory = GetNode<Label>(youCategoryPath);
            opponentCategory = GetNode<Label>(opponentCategoryPath);
            youQtd = GetNode<Label>(youQtdPath);
            opponentQtd = GetNode<Label>(opponentQtdPath);
            entrarBtn = GetNode<Button>(entrarBtnPath);

            Input.MouseMode = Input.MouseModeEnum.Visible;
            
            Connect("UpdateIP", this, nameof(OnUpdateIP));
            entrarBtn.Connect("button_up", this, nameof(OnEntrarClick));
            gameManager.Connect("WinnerEvent", this, nameof(OnWin));
            gameManager.Connect("LoserEvent", this, nameof(OnLose));
            gameManager.Connect("InitCategory", this, nameof(OnInitCategory));
            gameManager.Connect("ChangeTurnEvent", this, nameof(OnChangeTurn));
            gameManager.Connect("BallPocketed", this, nameof(OnBallPocketed));

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
                        if (eventKey.Scancode == (uint) KeyList.E) {
                            ChangeState(GameState.JOIN_SERVER);
                        }
                    }
                }
            }
        }

        public void OnEntrarClick()
        {
            ChangeState(GameState.JOIN_SERVER);
        }

        public void ChangeState(GameState newState)
        {
            state = newState;
            switch (newState) {
                case GameState.STARTING_SERVER: 
                    EmitSignal("HostServer");
                    helpBox.Visible = false;
                    break;
                case GameState.WAITING_IP_INPUT: 
                    loading.Visible = false;
                    enterIp.Visible = true;
                    break;
                case GameState.JOIN_SERVER: 
                    EmitSignal("JoinServer", "");
                    enterIp.Visible = false;
                    loading.Visible = true;
                    hostCtn.Visible = false;
                    helpBox.Visible = false;
                    waiting.Visible = false;
                    break;
                case GameState.WAITING_OPPONENT: 
                    hostCtn.Visible = false;
                    helpBox.Visible = false;
                    waiting.Visible = true;
                    loading.Visible = false;
                    break;
                case GameState.CONNECTING:
                    hostCtn.Visible = false;
                    helpBox.Visible = false;
                    waiting.Visible = false;
                    loading.Visible = true;
                    break;
                case GameState.CONNECTED:
                    helpBox.Visible = false;
                    enterIp.Visible = false;
                    loading.Visible = false;
                    waiting.Visible = false;
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
                EmitSignal("JoinServer", "127.0.0.1");
            } else {
                EmitSignal("JoinServer", ip);
            }
        }

        public void OnWin() 
        {
            win.Visible = true;
        }

        public void OnLose() 
        {
            lose.Visible = true;
        }
        
        public void OnInitCategory() 
        {
            youCategory.Text = gameManager.GetCategoryName(gameManager.yourBallCategory);
            opponentCategory.Text = gameManager.GetCategoryName(gameManager.opponentBallCategory);
        }
        
        public void OnBallPocketed() 
        {
            youQtd.Text = gameManager.categoriesQtd[gameManager.yourBallCategory].ToString();
            opponentQtd.Text = gameManager.categoriesQtd[gameManager.opponentBallCategory].ToString();
        }

        public void OnChangeTurn()
        {
            float toAlpha = .2f;
            Vector2 toScale = new Vector2(1.2f, 1.2f);

            SceneTreeTween tween = GetTree().CreateTween();

            tween.TweenProperty(you, "modulate:a", gameManager.IsYourTurn() ? 1f : toAlpha, .3f);
            tween.TweenProperty(you, "rect_rotation", gameManager.IsYourTurn() ? -6f : 0, .3f).SetEase(Tween.EaseType.In).SetTrans(Tween.TransitionType.Elastic);
            tween.TweenProperty(you, "rect_scale", gameManager.IsYourTurn() ? toScale : Vector2.One, .3f).SetEase(Tween.EaseType.In).SetTrans(Tween.TransitionType.Elastic);

            tween.TweenProperty(opponent, "modulate:a", gameManager.IsYourTurn() ? toAlpha : 1f, .3f);
            tween.TweenProperty(opponent, "rect_rotation", gameManager.IsYourTurn() ? 0 : 6f, .3f).SetEase(Tween.EaseType.In).SetTrans(Tween.TransitionType.Elastic);
            tween.TweenProperty(opponent, "rect_scale", gameManager.IsYourTurn() ? Vector2.One : toScale, .3f).SetEase(Tween.EaseType.In).SetTrans(Tween.TransitionType.Elastic);
        }
    }
}