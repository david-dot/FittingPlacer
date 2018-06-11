using System.Collections.Generic;

namespace FittingPlacer
{
	public class FaceType
	{
        // Data members

		public string Id { get; private set; }
        public List<SpatialRelation> SpatialRelations { get; private set; } = new List<SpatialRelation>();


        // Constructor

        public FaceType(string id)
        {
            Id = id;
        }


        // Methods

        public void AddRelation(SpatialRelation spatialRelation)
        {
            SpatialRelations.Add(spatialRelation);
        }
    }
}
