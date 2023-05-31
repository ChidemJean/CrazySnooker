using Godot;
using System;

namespace CrazySnooker.Extensions
{
    public static class TransformExtensions
    {
        public static Basis LookingAt(this Transform owner, Transform target)
        {
            Basis lookingAtBasis = new Basis();
            lookingAtBasis.z = target.origin - owner.origin;
            lookingAtBasis.x = Vector3.Up.Cross(lookingAtBasis.z);
            lookingAtBasis.y = lookingAtBasis.z.Cross(lookingAtBasis.x);
            return lookingAtBasis;
        }
    }
}