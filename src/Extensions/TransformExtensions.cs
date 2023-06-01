using Godot;
using System;

namespace CrazySnooker.Extensions
{
   public static class TransformExtensions
   {
      public static Basis LookAtBasis(this Transform owner, Vector3 target)
      {
         Basis lookingAtBasis = new Basis();
         lookingAtBasis.z = target - owner.origin;
         lookingAtBasis.x = Vector3.Up.Cross(lookingAtBasis.z);
         lookingAtBasis.y = lookingAtBasis.z.Cross(lookingAtBasis.x);
         lookingAtBasis = lookingAtBasis.Orthonormalized();
         return lookingAtBasis;
      }
   }
}