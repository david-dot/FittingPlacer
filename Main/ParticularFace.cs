using System;
using System.Collections.Generic;

namespace FittingPlacer
{
	public class ParticularFace
	{
        // Data members

        ///<summary>Fitting this particular face belongs to</summary>
		public Fitting Fitting;

		public Face Face;

		public float SideLength;

        // Implementation variables
        public float ReservedLength = 0;
        public List<Tuple<ParticularFace, SpatialRelation>> attacherFacesAndRelations = new List<Tuple<ParticularFace, SpatialRelation>>();
        public int PlacedAttacherFacesCount = 0;
        public float FilledLength = 0;


        // Constructor

        public ParticularFace(Fitting fitting, Face face, float sideLength)
        {
            Fitting = fitting;
            Face = face;
            SideLength = sideLength;
        }


        // Properties

        public int Direction
        {
            get
            {
                return (Fitting.Orientation + (int)Face.Facing) % 4;
            }
        }

        public bool IsXAligned
        {
            get
            {
                return (Direction == 1 || Direction == 3);
            }
        }

        public float DistanceFromFittingCenter
        {
            get
            {
                if (Face.Facing == Facing.Right || Face.Facing == Facing.Left)
                {
                    return Fitting.FittingModel.BoundingBox.Width / 2;
                }
                else
                {
                    return Fitting.FittingModel.BoundingBox.Depth / 2;
                }
            }
        }

        public Vector2D NormalVector
        {
            get
            {
                switch (Direction)
                {
                    case 0:
                        return Vector2D.UnitX;
                    case 1:
                        return Vector2D.UnitY;
                    case 2:
                        return -Vector2D.UnitX;
                    case 3:
                        return -Vector2D.UnitY;
                    default:
                        // This should never happen
                        return Vector2D.Zero;
                }
            }
        }

        /// <summary>Vector along face in counterclockwise direction</summary>
        public Vector2D VectorAlongFace
        {
            get
            {
                switch (Direction)
                {
                    case 0:
                        return Vector2D.UnitY;
                    case 1:
                        return -Vector2D.UnitX;
                    case 2:
                        return -Vector2D.UnitY;
                    case 3:
                        return Vector2D.UnitX;
                    default:
                        // This should never happen
                        return Vector2D.Zero;
                }
            }
        }

        public Vector2D FaceCenterPosition
        {
            get
            {
                return (Fitting.Position + NormalVector * DistanceFromFittingCenter);
            }
        }

        public float SurroundingClearanceAreaLength
        {
            get
            {
                switch (Face.Facing)
                {
                    case Facing.Right:
                        return (Fitting.FittingModel.GetClearanceAreaLength(Facing.Back) + Fitting.FittingModel.GetClearanceAreaLength(Facing.Front));
                    case Facing.Back:
                        return (Fitting.FittingModel.GetClearanceAreaLength(Facing.Left) + Fitting.FittingModel.GetClearanceAreaLength(Facing.Right));
                    case Facing.Left:
                        return (Fitting.FittingModel.GetClearanceAreaLength(Facing.Back) + Fitting.FittingModel.GetClearanceAreaLength(Facing.Front));
                    case Facing.Front:
                        return (Fitting.FittingModel.GetClearanceAreaLength(Facing.Left) + Fitting.FittingModel.GetClearanceAreaLength(Facing.Right));
                    default:
                        // This should never happen
                        return 0;
                }
            }
        }


        // Methods
        
        /// <summary>Checks if there is enough unreserved room left in face for an attacher-face</summary>
        /// <param name="attacherFace">Face to check if it can be supported by this face</param>
        /// <param name="spatialRelation">Attacher-face relation to this support-face</param>
        /// <returns>Whether attacher-face can be supported by this face</returns>
        public bool CanAttachFace(ParticularFace attacherFace, SpatialRelation spatialRelation)
        {
            if (Face.Type == spatialRelation.SupportFaceType)
            {
                if (ReservedLength == 0 || ReservedLength + attacherFace.SideLength + attacherFace.SurroundingClearanceAreaLength < SideLength)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>Saves face as attacher-face and reserves length of this support-face</summary>
        /// <param name="attacherFace">Face to attach to this face</param>
        /// <param name="spatialRelation">Attacher-face relation to this support-face</param>
        /// <returns>Whether attacher-face was attached to this face</returns>
        public bool AttachFace(ParticularFace attacherFace, SpatialRelation spatialRelation)
		{
            if (Face.Type == spatialRelation.SupportFaceType)
            {
                if (ReservedLength == 0 || ReservedLength + attacherFace.SideLength + attacherFace.SurroundingClearanceAreaLength < SideLength)
                {
                    ReservedLength += (attacherFace.SideLength + attacherFace.SurroundingClearanceAreaLength);
                    attacherFacesAndRelations.Add(new Tuple<ParticularFace, SpatialRelation>(attacherFace, spatialRelation));

                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
		}

        /// <summary>Rotates face's fitting / placement unit so this face faces direction</summary>
        /// <param name="direction">Direction number of direction to face</param>
        public void RotateToFace(int direction)
        {
            // Set direction to point in, in order to face the direction
            int directionToPointIn = (direction + 2) % 4;

            // Calculate the delta in orientation
            int rotationDelta = directionToPointIn - Direction;

            Fitting.RotatePlacementUnit(rotationDelta);
        }

        /// <summary>Translate face's fitting / placement unit so this face is positioned at a specified distance from support-face</summary>
        /// <param name="distance">Distance to support-face</param>
        /// <param name="supportFace">Support-face to position against</param>
        public void TranslateToDistanceFromFace(float distance, ParticularFace supportFace)
        {
            Vector2D offsetInDistanceDirection = supportFace.NormalVector * (supportFace.DistanceFromFittingCenter + distance + DistanceFromFittingCenter);

            Vector2D offsetAlongFace = Vector2D.Zero;
            if (supportFace.attacherFacesAndRelations.Count > 1)
            {
                float spacing = (supportFace.SideLength - supportFace.ReservedLength) / (supportFace.attacherFacesAndRelations.Count + 1);
                offsetAlongFace = supportFace.VectorAlongFace * (-(supportFace.SideLength / 2) + spacing * (supportFace.PlacedAttacherFacesCount + 1) + supportFace.FilledLength + Fitting.ClearanceAreaLengthInDirection((supportFace.Direction + 3) % 4) + SideLength / 2);
            }
            supportFace.PlacedAttacherFacesCount++;
            supportFace.FilledLength += (SideLength + SurroundingClearanceAreaLength);

            // Perform translation
            Fitting.PlacementUnit.Translate(supportFace.Fitting.Position + offsetInDistanceDirection + offsetAlongFace - Fitting.Position);
        }

    }
}
