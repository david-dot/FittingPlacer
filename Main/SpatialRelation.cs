namespace FittingPlacer
{
	public class SpatialRelation
	{
        // Data members

		public FaceType SupportFaceType { get; private set; }
        public float Distance { get; private set; }


        // Constructor

        public SpatialRelation(FaceType supportFaceType, float distance)
        {
            SupportFaceType = supportFaceType;
            Distance = distance;
        }
    }
}
