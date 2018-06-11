namespace FittingPlacer
{
    public class RepresentationObject
    {
        // Data members

        public string FittingModelId { get; private set; }
        public string FittingTypeId { get; private set; }


        // Constructor

        public RepresentationObject(string fittingModelId, string fittingTypeId)
        {
            FittingModelId = fittingModelId;
            FittingTypeId = fittingTypeId;
        }
    }
}