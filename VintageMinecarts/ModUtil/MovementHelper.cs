using System;
using Vintagestory.API.MathTools;

namespace VintageMinecarts.ModUtil
{
    public class MovementHelper
    {
        public static Vec3f MoveTowards(Vec3f currentPos, Vec3f targetPos, float step)
        {
            Vec3f diff = targetPos - currentPos;

            float magnitude = diff.Length();
            if (magnitude <= step || magnitude == 0f)
            {
                return targetPos;
            }

            return currentPos + diff / magnitude * step;
        }  

        public static Vec3d MoveTowards(Vec3d currentPos, Vec3d targetPos, float step)
        {
            Vec3d diff = targetPos - currentPos;

            float magnitude = (float)diff.Length();
            if (magnitude <= step || magnitude == 0f)
            {
                return targetPos;
            }

            return currentPos + diff / magnitude * step;
        }   

        public static float MoveTowards(float current, float target, float step)
        {
            if (Math.Abs(target - current) <= step)
            {
                return target;
            }

            return current + Math.Sign(target - current) * step;
        }   

        public static double MoveTowards(double current, double target, float step)
        {
            if (Math.Abs(target - current) <= step)
            {
                return target;
            }

            return current + Math.Sign(target - current) * step;
        }   
        
             
    }
}