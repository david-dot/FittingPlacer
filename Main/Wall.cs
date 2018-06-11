using System;

namespace FittingPlacer
{
    public class Wall : StaticFace
    {
        // Data members

        ///<summary>Height in meters</summary>
        public float Height { get; private set; }


        // Constructors

        public Wall(Vector2D position, float sideLength, int inwardsNormalDirection) : base(StaticFaceTypes.Wall, position, sideLength, inwardsNormalDirection)
        {
            Height = 2.6f;
        }

        public Wall(Vector2D position, float sideLength, int inwardsNormalDirection, float height) : base(StaticFaceTypes.Wall, position, sideLength, inwardsNormalDirection)
        {
            Height = height;
        }
        
    }
}
