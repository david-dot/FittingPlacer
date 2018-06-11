using System.Collections.Generic;

namespace FittingPlacer
{
	public class FittingType
	{
        // Data members

        public string Id { get; private set; }
        public List<Face> Faces { get; private set; } = new List<Face>();


        // Constructor

        public FittingType(string id)
        {
            Id = id;
        }


        // Methods

        public void AddFace(Face face)
        {
            Faces.Add(face);
        }
    }
}
