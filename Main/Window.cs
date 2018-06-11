using System;

namespace FittingPlacer
{
    public class Window : StaticFace
    {
        // Data members

        /// <summary>Elevation above floor in meters</summary>
        public float Elevation { get; private set; }

        /// <summary>Height in meters</summary>
        public float Height { get; private set; }


        // Constructors

        public Window(Vector2D position, float sideLength, int inwardsNormalDirection)
            : base(StaticFaceTypes.Window, position, sideLength, inwardsNormalDirection)
        {
            Elevation = 0.8f;
            Height = 1.5f;
        }

        public Window(Vector2D position, float sideLength, int inwardsNormalDirection, float height, float elevation)
            : base(StaticFaceTypes.Window, position, sideLength, inwardsNormalDirection)
        {
            Elevation = elevation;
            Height = height;
        }

    }
}
