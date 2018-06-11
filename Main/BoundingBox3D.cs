namespace FittingPlacer
{
    public struct BoundingBox3D
    {
        // Data members

        public readonly float Width;
        public readonly float Depth;
        public readonly float Height;


        // Constructor

        public BoundingBox3D(float width, float depth, float height)
        {
            Width = width;
            Depth = depth;
            Height = height;
        }
    }
}
