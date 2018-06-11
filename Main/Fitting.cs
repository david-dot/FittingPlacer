using System.Collections.Generic;

namespace FittingPlacer
{
	public class Fitting
	{
        // Data members

		public Vector2D Position { get; private set; }
        private int orientation;

		public FittingModel FittingModel { get; private set; }
        public PlacementUnit PlacementUnit { get; private set; }
        public List<ParticularFace> Faces { get; private set; }


        // Constructor

        public Fitting(FittingModel fittingModel)
        {
            Position = Vector2D.Zero;
            Orientation = 0;
            FittingModel = fittingModel;

            Faces = new List<ParticularFace>();
            foreach (Face fittingFace in fittingModel.FittingType.Faces)
            {
                float length;
                if (fittingFace.Facing == Facing.Left || fittingFace.Facing == Facing.Right)
                    length = FittingModel.BoundingBox.Depth;
                else
                    length = FittingModel.BoundingBox.Width;

                Faces.Add(new ParticularFace(this, fittingFace, length));
            }

            PlacementUnit = new PlacementUnit(this);
        }

        // Properties

        public int Orientation
        {
            get
            {
                return orientation;
            }
            set
            {
                orientation = (value % 4 + 4) % 4;
            }
        }

        public float XLength
        {
            get
            {
                if (Orientation == 0 || Orientation == 2)
                {
                    return FittingModel.BoundingBox.Width;
                }
                else
                {
                    return FittingModel.BoundingBox.Depth;
                }
            }
        }

        public float YLength
        {
            get
            {
                if (Orientation == 0 || Orientation == 2)
                {
                    return FittingModel.BoundingBox.Depth;
                }
                else
                {
                    return FittingModel.BoundingBox.Width;
                }
            }
        }


        // Methods

        public float ClearanceAreaLengthInDirection(int direction)
        {
            int facing = ((direction - Orientation) % 4 + 4) % 4;
            return FittingModel.GetClearanceAreaLength((Facing)facing);
        }

        public void ChangePlacementUnit(PlacementUnit newPlacementUnit)
        {
            PlacementUnit = newPlacementUnit;
        }

        /// <summary>Translates fitting position in plane</summary>
        /// <param name="offset">Offset</param>
        /// <remarks>Pay attention to when whole placement unit should be translated instead</remarks>
        public void Translate(Vector2D offset)
        {
            Position += offset;
        }

        /// <summary>Rotates fitting's placement unit in plane around fitting</summary>
        /// <param name="rotationDelta">Number of 90 degree counterclockwise turns</param>
        public void RotatePlacementUnit(int rotationDelta)
        {
            PlacementUnit.RotateAround(Position, rotationDelta);
        }

        /// <summary>Rotates fitting in plane around position</summary>
        /// <param name="nave">Position to rotate around</param>
        /// <param name="rotationDelta">Number of 90 degree counterclockwise turns</param>
        /// <remarks>
        /// Pay attention to when whole placement unit should be rotated 
        /// using RotatePlacementUnit() instead
        /// </remarks>
        public void RotateAround(Vector2D nave, int rotationDelta)
        {
            // Number of 90 degree counterclockwise turns
            rotationDelta = ((rotationDelta % 4) + 4) % 4;

            // Rotate orientation
            Orientation += rotationDelta;

            // Rotate position

            // Get sin and cos values for rotation
            int sin = 0;
            int cos = 1;
            switch (rotationDelta)
            {
                case 1:
                    sin = 1;
                    cos = 0;
                    break;
                case 2:
                    sin = 0;
                    cos = -1;
                    break;
                case 3:
                    sin = -1;
                    cos = 0;
                    break;
                default:
                    break;
            }

            // Translate to nave
            Position -= nave;

            // Rotate in relation to nave
            Position = new Vector2D(
              Position.X * cos - Position.Y * sin,
              Position.X * sin + Position.Y * cos
            );

            // Translate back
            Position += nave;
        }
    }
}
