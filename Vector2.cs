using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace ProjectRogue
{
    public class Vector2
    {
        public double x, y;

        public static Vector2 zero 
        {
            get { return new Vector2(0, 0); }
        }

        public Vector2(double x, double y)
        {
            this.x = x;
            this.y = y;
        }

        public static Vector2 operator +(Vector2 vec1, Vector2 vec2)
        {
            return new Vector2(vec1.x + vec2.x, vec1.y + vec2.y);
        }

        public static Vector2 operator -(Vector2 vec1, Vector2 vec2)
        {
            return new Vector2(vec1.x - vec2.x, vec1.y - vec2.y);
        }

        public static Vector2 operator *(double num, Vector2 vec)
        {
            return new Vector2(vec.x * num, vec.y * num);
        }

        public static Vector2 operator *(Vector2 vec, double num)
        {
            return new Vector2(vec.x * num, vec.y * num);
        }

        public static Vector2 operator /(Vector2 vec, double num)
        {
            return new Vector2(vec.x / num, vec.y / num);
        }

        public double length()
        {
            return Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2));
        }

        public Vector2 Normalize()
        {
            return this / this.length();
        }

        public override bool Equals(Object obj)
        {
            Vector2 vec = obj as Vector2;
            if ((object)vec == null)
                return false;

            return this == vec;
        }

        public override int GetHashCode()
        {
            return (int) (x + y);
        }

        public static bool operator ==(Vector2 vec1, Vector2 vec2)
        {
            // If both are null, or both are same instance, return true.
            if (Object.ReferenceEquals(vec1, vec2))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)vec1 == null) || ((object)vec2 == null))
            {
                return false;
            }

            // Return true if the fields match:
            return vec1.x == vec2.x && vec1.y == vec2.y;
        }

        public static bool operator !=(Vector2 a, Vector2 b)
        {
            return !(a == b);
        }


        public static bool LinearIndependent(Vector2 vec1, Vector2 vec2)
        {            
            return (vec1.x * vec2.y) - (vec2.x * vec1.y) != 0;
        }

        public static implicit operator Microsoft.Xna.Framework.Vector2(Vector2 vec)
        {
            return new Microsoft.Xna.Framework.Vector2((float)vec.x, (float)vec.y);
        }
    }

}
