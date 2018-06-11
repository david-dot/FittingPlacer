using System;

namespace FittingPlacer
{
    public struct Vector2D
    {
        // Data members
        public readonly float X;
        public readonly float Y;

        // Static properties
        public static readonly Vector2D Zero = new Vector2D(0, 0);
        public static readonly Vector2D One = new Vector2D(1.0f, 1.0f);
        public static readonly Vector2D UnitX = new Vector2D(1.0f, 0.0f);
        public static readonly Vector2D UnitY = new Vector2D(0.0f, 1.0f);


        // Constructors

        public Vector2D(float x, float y)
        {
            this.X = x;
            this.Y = y;
        }


        // Methods

        public override string ToString()
        {
            return (String.Format("({0}, {1})", X, Y));
        }

        public override bool Equals(Object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (obj.GetType() != this.GetType())
                return false;
            else
            {
                Vector2D other = (Vector2D)obj;
                return (other.X == this.X && other.Y == this.Y);
            }
        }

        public override int GetHashCode()
        {
            // Integer overflow is fine as it will just wrap around
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + X.GetHashCode();
                hash = hash * 23 + Y.GetHashCode();
                return hash;
            }
        }

        public float Length()
        {
            return (float)Math.Sqrt(Vector2D.Dot(this, this));
        }

        public float LengthSquared()
        {
            return Vector2D.Dot(this, this);
        }


        // Static methods

        public static float Dot(Vector2D value1, Vector2D value2)
        {
            return (value1.X * value2.X + value1.Y * value2.Y);
        }

        public static float Distance(Vector2D value1, Vector2D value2)
        {
            Vector2D difference = value1 - value2;
            return difference.Length();
        }

        public static float DistanceSquared(Vector2D value1, Vector2D value2)
        {
            Vector2D difference = value1 - value2;
            return difference.LengthSquared();
        }

        public static Vector2D Normalize(Vector2D value)
        {
            return value / value.Length();
        }

        public static Vector2D Reflect(Vector2D vector, Vector2D normal)
        {
            normal = Vector2D.Normalize(normal);
            return (vector - 2f * Dot(vector, normal) * normal);
        }

        public static Vector2D Lerp(Vector2D value1, Vector2D value2, float amount)
        {
            return value2 * amount + value1 * (1f - amount);
        }

        public static Vector2D Abs(Vector2D value)
        {
            return new Vector2D(Math.Abs(value.X), Math.Abs(value.Y));
        }


        // Operator methods

        public static bool operator ==(Vector2D left, Vector2D right)
        {
            return (left.X == right.X && left.Y == right.Y);
        }

        public static bool operator !=(Vector2D left, Vector2D right)
        {
            return (left.X != right.X || left.Y != right.Y);
        }

        public static Vector2D operator -(Vector2D value)
        {
            return new Vector2D(-value.X, -value.Y);
        }

        public static Vector2D operator +(Vector2D left, Vector2D right)
        {
            return new Vector2D(left.X + right.X, left.Y + right.Y);
        }

        public static Vector2D operator -(Vector2D left, Vector2D right)
        {
            return new Vector2D(left.X - right.X, left.Y - right.Y);
        }

        /// <summary>Element-wise multiplication of two vectors</summary>
        public static Vector2D operator *(Vector2D left, Vector2D right)
        {
            return new Vector2D(left.X * right.X, left.Y * right.Y);
        }

        public static Vector2D operator *(float left, Vector2D right)
        {
            return new Vector2D(right.X * left, right.Y * left);
        }

        public static Vector2D operator *(Vector2D left, float right)
        {
            return new Vector2D(left.X * right, left.Y * right);
        }

        public static Vector2D operator /(Vector2D numerator, Vector2D denominator)
        {
            return new Vector2D(numerator.X / denominator.X, numerator.Y / denominator.Y);
        }

        public static Vector2D operator /(Vector2D numerator, float denominator)
        {
            return new Vector2D(numerator.X / denominator, numerator.Y / denominator);
        }

    }
}
