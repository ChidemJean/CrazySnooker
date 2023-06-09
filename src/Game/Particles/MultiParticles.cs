using Godot;
using System;
using System.Collections.Generic;

namespace CrazySnooker.Game.Particles
{
    public class MultiParticles : Spatial
    {
        private List<Godot.Particles> particles = new List<Godot.Particles>();
        private float maxTime = 0f;

        public override void _Ready()
        {
            foreach (Node particle in GetChildren()) {
                if (particle is Godot.Particles) {
                    particles.Add((Godot.Particles) particle);
                    float lifetime = (float)particle.Get("lifetime");
                    if (lifetime > maxTime) {
                        maxTime = lifetime;
                    }
                }
            }
        }

        public async void Play()
        {
            foreach (Godot.Particles particle in particles) {
                particle.Emitting = true;
            }
            await ToSignal(GetTree().CreateTimer(maxTime), "timeout");
            QueueFree();
        }
    }
}