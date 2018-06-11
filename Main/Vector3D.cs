using System;

namespace FittingPlacer
{
    public struct Vector3D
    {
        // Data members
        public readonly float X;
        public readonly float Y;
        public readonly float Z;

        // Static properties
        public static readonly Vector3D Zero = new Vector3D(0, 0, 0);
        public static readonly Vector3D One = new Vector3D(1.0f, 1.0f, 1.0f);
        public static readonly Vector3D UnitX = new Vector3D(1.0f, 0.0f, 0.0f);
        public static readonly Vector3D UnitY = new Vector3D(0.0f, 1.0f, 0.0f);
        public static readonly Vector3D UnitZ = new Vector3D(0.0f, 0.0f, 1.0f);


        // Constructor

        public Vector3D(float x, float y, float z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }


        // Methods

        public override string ToString()
        {
            return (String.Format("({0}, {1}, {2})", X, Y, Z));
        }

        public override bool Equals(Object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (obj.GetType() != this.GetType())
                return false;
            else
            {
                Vector3D other = (Vector3D)obj;
                return (other.X == this.X && other.Y == this.Y && other.Z == this.Z);
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
                hash = hash * 23 + Z.GetHashCode();
                return hash;
            }
        }

        public float Length()
        {
            return (float)Math.Sqrt(Vector3D.Dot(this, this));
        }

        public float LengthSquared()
        {
            return Vector3D.Dot(this, this);
        }


        // Static methods

        public static float Dot(Vector3D value1, Vector3D value2)
        {
            return (value1.X * value2.X + value1.Y * value2.Y + value1.Z * value2.Z);
        }

        public static float Distance(Vector3D value1, Vector3D value2)
        {
            Vector3D difference = value1 - value2;
            return difference.Length();
        }

        public static float DistanceSquared(Vector3D value1, Vector3D value2)
        {
            Vector3D difference = value1 - value2;
            return difference.LengthSquared();
        }

        public static Vector3D Normalize(Vector3D value)
        {
            return value / value.Length();
        }

        public static Vector3D Cross(Vector3D left, Vector3D right)
        {
            return new Vector3D(
              left.Y * right.Z - left.Z * right.Y,
              left.Z * right.X - left.X * right.Z,
              left.X * right.Y - left.Y * right.X
            );
        }

        public static Vector3D Reflect(Vector3D vector, Vector3D normal)
        {
            normal = Vector3D.Normalize(normal);
            return (vector - 2f * Dot(vector, normal) * normal);
        }

        public static Vector3D Lerp(Vector3D value1, Vector3D value2, float amount)
        {
            return value2 * amount + value1 * (1f - amount);
        }

        public static Vector3D Abs(Vector3D value)
        {
            return new Vector3D(Math.Abs(value.X), Math.Abs(value.Y), Math.Abs(value.Z));
        }


        // Operator methods

        public static bool operator ==(Vector3D left, Vector3D right)
        {
            return (left.X == right.X && left.Y == right.Y && left.Z == right.Z);
        }

        public static bool operator !=(Vector3D left, Vector3D right)
        {
            return (left.X != right.X || left.Y != right.Y || left.Z != right.Z);
        }

        public static Vector3D operator -(Vector3D value)
        {
            return new Vector3D(-value.X, -value.Y, -value.Z);
        }

        public static Vector3D operator +(Vector3D left, Vector3D right)
        {
            return new Vector3D(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
        }

        public static Vector3D operator -(Vector3D left, Vector3D right)
        {
            return new Vector3D(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
        }

        /// <summary>Element-wise multiplication of two vectors</summary>
        public static Vector3D operator *(Vector3D left, Vector3D right)
        {
            return new Vector3D(left.X * right.X, left.Y * right.Y, left.Z * right.Z);
        }

        public static Vector3D operator *(float left, Vector3D right)
        {
            return new Vector3D(right.X * left, right.Y * left, right.Z * left);
        }

        public static Vector3D operator *(Vector3D left, float right)
        {
            return new Vector3D(left.X * right, left.Y * right, left.Z * right);
        }

        public static Vector3D operator /(Vector3D numerator, Vector3D denominator)
        {
            return new Vector3D(numerator.X / denominator.X, numerator.Y / denominator.Y, numerator.Z / denominator.Z);
        }

        public static Vector3D operator /(Vector3D numerator, float denominator)
        {
            return new Vector3D(numerator.X / denominator, numerator.Y / denominator, numerator.Z / denominator);
        }

    }
}
