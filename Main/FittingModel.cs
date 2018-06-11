using System.Collections.Generic;

namespace FittingPlacer
{
	public class FittingModel
	{
        // Data members

        public string Id { get; private set; }

        public FittingType FittingType { get; private set; }

        public BoundingBox3D BoundingBox { get; private set; }

        public RepresentationObject RepresentationObject { get; private set; }

        ///<summary>Perpendicular lengths of the faces' clearance areas</summary>
        private Dictionary<Facing, float> clearanceAreaLengths = new Dictionary<Facing, float>();


        // Constructors

        public FittingModel(string fittingModelId, FittingType fittingType, BoundingBox3D boundingBox)
        {
            Id = fittingModelId;
            FittingType = fittingType;
            BoundingBox = boundingBox;
        }

        public FittingModel(string fittingModelId, FittingType fittingType, float width, float depth, float height)
        {
            Id = fittingModelId;
            FittingType = fittingType;
            BoundingBox = new BoundingBox3D(width, depth, height);
        }

        // Methods

        public void AddClearanceArea(Facing side, float perpendicularLength)
        {
            clearanceAreaLengths.Add(side, perpendicularLength);
        }

        public float BaseArea()
        {
            return BoundingBox.Width * BoundingBox.Depth;
        }

        public float GetClearanceAreaLength(Facing side)
        {
            float perpendicularLength;
            if (clearanceAreaLengths.TryGetValue(side, out perpendicularLength))
            {
                return perpendicularLength;
            }
            else
            {
                return 0;
            }
        }

    }
}
