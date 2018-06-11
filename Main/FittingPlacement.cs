using System;

namespace FittingPlacer
{
	public class FittingPlacement
	{
        // Data members

		public float PositionX { get; private set; }
        public float PositionY { get; private set; }
        public float Orientation { get; private set; }
		public RepresentationObject RepresentationObject { get; private set; }


        // Constructor

        public FittingPlacement(float x, float y, int orientation, RepresentationObject representationObject)
        {
            PositionX = x;
            PositionY = y;
            Orientation = (float)(orientation * Math.PI / 2);
            RepresentationObject = representationObject;
        }
    }
}
