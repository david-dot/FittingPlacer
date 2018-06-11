using System;

namespace FittingPlacer
{
    public abstract class StaticFace
    {
        // Data members

        // StaticFace type
        public enum StaticFaceTypes
        {
            Wall,
            Door,
            Window
        };
        public StaticFaceTypes StaticFaceType { get; private set; }

        ///<summary>Center position</summary>
        public Vector2D Position { get; private set; }

        ///<summary>Width in meters</summary>
        public float SideLength { get; private set; }

        ///<summary>Inwards direction as 90 degree counterclockwise turns measured from positive x-axis in floor plane</summary>
        public int InwardsNormalDirection { get; private set; }

        ///<summary>Perpendicular length clearance area in front of face</summary>
        private float clearanceAreaLength;


        // Constructors

        public StaticFace(StaticFaceTypes staticFaceType, Vector2D position, float sideLength, int inwardsNormalDirection)
        {
            StaticFaceType = staticFaceType;
            Position = position;
            SideLength = sideLength;
            InwardsNormalDirection = inwardsNormalDirection;

            // Set clearance area perpendicular length after type
            if (StaticFaceType == StaticFaceTypes.Door)
            {
                // Set clearance area wide enough foor door to open
                ClearanceAreaLength = SideLength;
            }
            else if (StaticFaceType == StaticFaceTypes.Window)
            {
                // Set passage-wide clearance area in front of window
                ClearanceAreaLength = 0.9f;
            }
            else
            {
                ClearanceAreaLength = 0;
            }
        }

        public StaticFace(string staticFaceType, Vector2D position, float sideLength, int inwardsNormalDirection)
            : this ((StaticFaceTypes)Enum.Parse(typeof(StaticFaceTypes), staticFaceType), position, sideLength, inwardsNormalDirection)
        {
            // Parses static face type to enum and calls other constructor
        }


        // Properties

        public Vector2D InwardsNormalVector
        {
            get
            {
                switch (InwardsNormalDirection)
                {
                    case 0:
                        return Vector2D.UnitX;
                    case 1:
                        return Vector2D.UnitY;
                    case 2:
                        return -Vector2D.UnitX;
                    case 3:
                        return -Vector2D.UnitY;
                    default:
                        // This should never happen
                        return Vector2D.Zero;
                }
            }
        }

        public Vector2D VectorAlongFace
        {
            get
            {
                switch (InwardsNormalDirection)
                {
                    case 0:
                        return Vector2D.UnitY;
                    case 1:
                        return -Vector2D.UnitX;
                    case 2:
                        return -Vector2D.UnitY;
                    case 3:
                        return Vector2D.UnitX;
                    default:
                        // This should never happen
                        return Vector2D.Zero;
                }
            }
        }

        public float ClearanceAreaLength
        {
            get
            {
                if (StaticFaceType == StaticFaceTypes.Wall)
                {
                    return 0;
                }
                else
                {
                    return clearanceAreaLength;
                }
            }
            set
            {
                if (StaticFaceType == StaticFaceTypes.Wall)
                {
                    clearanceAreaLength = 0;
                }
                else
                {
                    clearanceAreaLength = value;
                }
            }
        }
        
    }
}