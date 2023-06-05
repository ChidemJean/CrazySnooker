using Godot;
using System;

namespace CrazySnooker.Utils
{
    public class MathUtils
    {
        public static int RandiRange(int min, int max)
        {
            Random rnd = new Random();
            return rnd.Next(min, max);
        }

        public static Vector3 FloatArrayToVector3(float[] vec)
        {
            return new Vector3(vec[0], vec[1], vec[2]);
        }

        public static float[] Vector3ToFloatArray(Vector3 vec)
        {
            return new float[] { vec.x, vec.y, vec.z };
        }
    }
}