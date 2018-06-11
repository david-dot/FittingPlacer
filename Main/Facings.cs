namespace FittingPlacer
{
    /// <summary>
    /// Specification of side of fitting, irrespective of fitting orientation
    /// </summary>
    /// <remarks>
    /// Facing specified from the default fitting orientation. Where the Front is facing the negative Y direction, 
    /// Back is facing the positive Y direction, Right is facing the positive X direction, & Left is facing the negative X direction. 
    /// </remarks>
    public enum Facing
    {
        Right = 0,
        Back = 1,
        Left = 2,
        Front = 3
    }
}
