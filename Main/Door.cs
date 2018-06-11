using System;

namespace FittingPlacer
{
    public class Door : StaticFace
    {
        // Data members

        ///<summary>Height in meters</summary>
        public float Height { get; private set; }


        // Constructors

        public Door(Vector2D position, float sideLength, int inwardsNormalDirection)
            : base(StaticFaceTypes.Door, position, sideLength, inwardsNormalDirection)
        {
            Height = 2.1f;
        }

        public Door(Vector2D position, float sideLength, int inwardsNormalDirection, float height)
            : base(StaticFaceTypes.Door, position, sideLength, inwardsNormalDirection)
        {
            Height = height;
        }

    }
}
