namespace FittingPlacer
{
	public class Face
	{
        // Data members

        public Facing Facing;
		public FaceType Type;


        // Constructor

        public Face(Facing facing, FaceType type)
        {
            Facing = facing;
            Type = type;
        }
    }
}
