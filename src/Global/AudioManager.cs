using Godot;
using System;
using System.Collections.Generic;
using CrazySnooker.Utils;

namespace CrazySnooker.Global
{
    public class AudioManager : Node
    {
        public struct QueuedAudio {
            public object obj;
            public object position;
        }

        [Export] public int qtdPlayers = 8;
        [Export] public int qtdPlayers2D = 8;
        [Export] public int qtdPlayers3D = 8;
        [Export] public string bus = "master";
        [Export] public string bus2D = "master";
        [Export] public string bus3D = "master";

        [Export] public Dictionary<string, AudioGroup> sounds = new Dictionary<string, AudioGroup>();

        private Queue<AudioStreamPlayer> available;
        private Queue<AudioStreamPlayer2D> available2D;
        private Queue<AudioStreamPlayer3D> available3D;
        private Queue<object> queue;
        private Queue<QueuedAudio> queue2D;
        private Queue<QueuedAudio> queue3D;
        
        //
        private Spatial main3dNode;
        private Node2D main2dNode;

        public override void _Ready()
        {
            main3dNode = GetNodeOrNull<Spatial>("%GameScene");
            main2dNode = GetNodeOrNull<Node2D>("%GameScene2D");
            InitUI();
            Init3D();
            Init2D();
        }

        public void InitUI()
        {
            available = new Queue<AudioStreamPlayer>();
            queue = new Queue<object>();

            for (int i = 0; i < qtdPlayers; i++) {
                AudioStreamPlayer stream = new AudioStreamPlayer();
                AddChild(stream);
                available.Enqueue(stream);
                stream.Connect("finished", this, nameof(OnStreamFinished), new Godot.Collections.Array() { stream });
                stream.Bus = bus;
            }
        }

        public void Init2D()
        {
            available2D = new Queue<AudioStreamPlayer2D>();
            queue2D = new Queue<QueuedAudio>();

            for (int i = 0; i < qtdPlayers2D; i++) {
                AudioStreamPlayer2D stream = new AudioStreamPlayer2D();
                main2dNode.AddChild(stream);
                available2D.Enqueue(stream);
                stream.Connect("finished", this, nameof(OnStream2DFinished), new Godot.Collections.Array() { stream });
                stream.Bus = bus2D;
            }
        }

        public void Init3D()
        {
            available3D = new Queue<AudioStreamPlayer3D>();
            queue3D = new Queue<QueuedAudio>();

            for (int i = 0; i < qtdPlayers3D; i++) {
                AudioStreamPlayer3D stream = new AudioStreamPlayer3D();
                stream.DopplerTracking = AudioStreamPlayer3D.DopplerTrackingEnum.IdleStep;
                stream.AttenuationFilterCutoffHz = 20500;
                main3dNode.AddChild(stream);
                available3D.Enqueue(stream);
                stream.Connect("finished", this, nameof(OnStream3DFinished), new Godot.Collections.Array() { stream });
                stream.Bus = bus3D;
            }
        }

        public void OnStream3DFinished(AudioStreamPlayer3D stream)
        {
            available3D.Enqueue(stream);
        }

        public void OnStream2DFinished(AudioStreamPlayer2D stream)
        {
            available2D.Enqueue(stream);
        }

        public void OnStreamFinished(AudioStreamPlayer stream)
        {
            available.Enqueue(stream);
        }

        public void Play(string groupName, string soundName = null, object position = null)
        {
            AudioGroup soundGroup;
            AudioStream sound;
            sounds.TryGetValue(groupName, out soundGroup);

            int idx = soundName != null ? soundName.ToInt() : MathUtils.RandiRange(0, soundGroup.audios.Count - 1);
            sound = soundGroup.audios[idx];

            AudioGroup.PlayMode playMode = soundGroup.playMode;
            EnqueueAudio(sound, playMode, position);
        }

        public void PlayResource(string soundPath, AudioGroup.PlayMode playMode = AudioGroup.PlayMode.UI)
        {
            EnqueueAudio(soundPath, playMode);
        }

        public void EnqueueAudio(object sound, AudioGroup.PlayMode playMode = AudioGroup.PlayMode.UI, object position = null)
        {
            if (position != null) {
                QueuedAudio queuedAudio;
                queuedAudio = new QueuedAudio();
                queuedAudio.obj = sound;
                queuedAudio.position = position;

                switch (playMode) {
                    case AudioGroup.PlayMode.D2:
                        queue2D.Enqueue(queuedAudio);
                        return;
                    case AudioGroup.PlayMode.D3:
                        queue3D.Enqueue(queuedAudio);
                        return;
                }
            }

            queue.Enqueue(sound);
        }

        public override void _Process(float delta)
        {
            UpdateUIQueue();
            Update2DQueue();
            Update3DQueue();
        }

        public void UpdateUIQueue()
        {
            if (queue.Count == 0 || available.Count == 0) return;

            AudioStreamPlayer player = available.Dequeue();
            object stream = queue.Dequeue();
            player.Stream = (stream is string) ? ResourceLoader.Load<AudioStream>((string) stream) : (AudioStream) stream;
            player.Play();
        }

        public void Update2DQueue()
        {
            if (queue2D.Count == 0 || available2D.Count == 0) return;

            QueuedAudio queuedAudio = queue2D.Dequeue();
            object stream = queuedAudio.obj;
            Vector2 position = (Vector2) queuedAudio.position;
            AudioStreamPlayer2D player = available2D.Dequeue();
            player.GlobalPosition = position;
            player.Stream = (stream is string) ? ResourceLoader.Load<AudioStream>((string) stream) : (AudioStream) stream;
            player.Play();
        }

        public void Update3DQueue()
        {
            if (queue3D.Count > 0 && available3D.Count > 0) {
                QueuedAudio queuedAudio = queue3D.Dequeue();
                object stream = queuedAudio.obj;
                Vector3 position = (Vector3) queuedAudio.position;
                AudioStreamPlayer3D player = available3D.Dequeue();
                player.Stream = (stream is string) ? ResourceLoader.Load<AudioStream>((string) stream) : (AudioStream) stream;
                player.GlobalTranslation = position;
                player.Play();
            }
        }

    }
}