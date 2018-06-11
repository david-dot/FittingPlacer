using System;
using System.Collections.Generic;

namespace FittingPlacer
{
	public class PlacementUnit
	{
        // Data members

        ///<summary>List of fittings that belong to this placement unit</summary>
		public List<Fitting> Members { get; private set; }

        // Extreme values
        private float minX;
        private float maxX;
        private float minY;
        private float maxY;

        ///<summary>Wall relation constraints</summary>
        public List<Tuple<int, float>> wallDirectionsAndWallDistances = new List<Tuple<int, float>>();

        ///<summary>Placement domain</summary>
        public List<Tuple<Vector2D, int>> positionsAndRotationsDomain = new List<Tuple<Vector2D, int>>();


        // Constructor

        public PlacementUnit(Fitting member)
        {
            Members = new List<Fitting>();
            AddMember(member);
        }


        // Properties

        public float XLength
        {
            get
            {
                return (maxX - minX);
            }
        }

        public float YLength
        {
            get
            {
                return (maxY - minY);
            }
        }

        public Vector2D MiddlePosition
        {
            get
            {
                return new Vector2D(
                  (minX + maxX) / 2,
                  (minY + maxY) / 2
                );
            }
        }


        // Methods

        public void AbsorbPlacementUnit(PlacementUnit unitToMerge)
		{
            foreach (Fitting newMember in unitToMerge.Members)
            {
                AddMember(newMember);
            }
        }

        private void AddMember(Fitting newMember)
        {
            newMember.ChangePlacementUnit(this);

            Members.Add(newMember);

            foreach (ParticularFace face in newMember.Faces)
            {
                TestForExtremePositions(face.FaceCenterPosition + face.NormalVector * face.Fitting.FittingModel.GetClearanceAreaLength(face.Face.Facing));
            }
        }

        private void TestForExtremePositions(Vector2D testPosition)
        {
            // Potentially adjust extreme x positions

            if (testPosition.X < minX)
            {
                minX = testPosition.X;
            }
            else if (testPosition.X > maxX)
            {
                maxX = testPosition.X;
            }


            // Potentially adjust extreme y positions

            if (testPosition.Y < minY)
            {
                minY = testPosition.Y;
            }
            else if (testPosition.Y > maxY)
            {
                maxY = testPosition.Y;
            }
        }

        /// <summary>Rotates all member fittings around position</summary>
        /// <param name="position">Position to rotate around</param>
        /// <param name="rotationDelta">Number of counterclockwise 90 degree turns</param>
        public void RotateAround(Vector2D position, int rotationDelta)
        {
            rotationDelta = ((rotationDelta % 4) + 4) % 4;

            if (rotationDelta > 0)
            {
                // Rotate member
                foreach (Fitting member in Members)
                {
                    member.RotateAround(position, rotationDelta);
                }

                // Rotate extreme values
                if (Math.Abs(rotationDelta % 2) == 1)
                {
                    float newMinX = minY;
                    float newMaxX = maxY;

                    minY = minX;
                    maxY = maxX;

                    minX = newMinX;
                    maxX = newMaxX;
                }

                // Update wall related side constraints
                for (int i = 0; i < wallDirectionsAndWallDistances.Count; i++)
                {
                    int direction = (wallDirectionsAndWallDistances[i].Item1 + rotationDelta) % 4;
                    wallDirectionsAndWallDistances[i] = new Tuple<int, float>(direction, wallDirectionsAndWallDistances[i].Item2);
                }
            }
        }

        /// <summary>Translate all member fitting positions in plane</summary>
        /// <param name="offset">Offset to translate by</param>
        public void Translate(Vector2D offset)
        {
            // Translate member positions
            foreach (Fitting member in Members)
            {
                member.Translate(offset);
            }

            // Update wall related side constraints
            for (int i = 0; i < wallDirectionsAndWallDistances.Count; i++)
            {
                int direction = wallDirectionsAndWallDistances[i].Item1;
                float distance = wallDirectionsAndWallDistances[i].Item2;

                if (direction == 0)
                {
                    distance += offset.X;
                }
                else if (direction == 2)
                {
                    distance -= offset.X;
                }
                else if (direction == 1)
                {
                    distance += offset.Y;
                }
                else
                {
                    distance -= offset.Y;
                }

                wallDirectionsAndWallDistances[i] = new Tuple<int, float>(direction, distance);
            }

        }

        public void SetPlacementUnitOriginToPlacementUnitMiddle()
        {
            Translate(-MiddlePosition);
        }

        /// <summary>Add wall distance constraint from a fitting face to wall</summary>
        /// <param name="attacherFace">Attacher-face to be placed face-forward relative to wall</param>
        /// <param name="wallDistance">Distance from face to wall</param>
        public void AddWallConstraint(ParticularFace attacherFace, float wallDistance)
        {
            float placementUnitOriginToWall = (attacherFace.NormalVector * attacherFace.FaceCenterPosition).Length() + wallDistance;
            wallDirectionsAndWallDistances.Add(new Tuple<int, float>(attacherFace.Direction, placementUnitOriginToWall));
        }

        /// <summary>Reduce wall relations to only satisfiable combinations</summary>
        /// <returns>Whether all wall relations can be satisfied</returns>
        public bool TrimWallRelations()
        {
            bool hasOnlyCompatibleConstraints = true;

            if (wallDirectionsAndWallDistances.Count <= 1)
            {
                return true;
            }
            else
            {
                float?[] wallDistances = new float?[4] {null, null, null, null};

                foreach (Tuple<int, float> wallDirectionAndWallDistance in wallDirectionsAndWallDistances)
                {
                    int direction = wallDirectionAndWallDistance.Item1;
                    float distance = wallDirectionAndWallDistance.Item2;

                    if (!wallDistances[direction].HasValue)
                    {
                        wallDistances[direction] = distance;
                    }
                    else
                    {
                        if (distance != wallDistances[direction])
                        {
                            // Two wall distances in the same direction
                            hasOnlyCompatibleConstraints = false;

                            // Replace distance with the longest one
                            if (distance > wallDistances[direction])
                            {
                                wallDistances[direction] = distance;
                            }
                        }
                    }
                }

                // Remove one of opposite directions, as both cannot be satisifed 
                // (except for in one special case where distance between opposite walls match)

                if (wallDistances[0].HasValue && wallDistances[2].HasValue)
                {
                    hasOnlyCompatibleConstraints = false;

                    if (wallDistances[0] < wallDistances[2])
                    {
                        wallDistances[2] = null;
                    }
                    else
                    {
                        wallDistances[0] = null;
                    }
                }
                if (wallDistances[1].HasValue && wallDistances[3].HasValue)
                {
                    hasOnlyCompatibleConstraints = false;

                    if (wallDistances[1] < wallDistances[3])
                    {
                        wallDistances[3] = null;
                    }
                    else
                    {
                        wallDistances[1] = null;
                    }
                }

                // Reset wall relation list with only trimmed result
                wallDirectionsAndWallDistances = new List<Tuple<int, float>>();
                for (int direction = 0; direction < wallDistances.Length; direction++)
                {
                    if (wallDistances[direction].HasValue)
                    {
                        wallDirectionsAndWallDistances.Add(new Tuple<int, float>(direction, wallDistances[direction].Value));
                    }
                }

                return hasOnlyCompatibleConstraints;
            }
        }
        /// <summary>Set domain of potential placements for placement unit</summary>
        /// <param name="room">Room to test placements in</param>
        public void SetDomain(Room room)
        {
            positionsAndRotationsDomain = new List<Tuple<Vector2D, int>>();

            SetPlacementUnitOriginToPlacementUnitMiddle();

            if (!TrimWallRelations())
            {
                Console.WriteLine("All wall relation constraints could not be satisfied for a placement unit. ");
            }

            if (wallDirectionsAndWallDistances.Count == 0)
            {
                // Set placement domain when there are no wall constraints

                // Set domain for this rotation

                float minDomainX = (-room.Width + XLength) / 2;
                float maxDomainX = (room.Width - XLength) / 2;

                float minDomainY = (-room.Depth + YLength) / 2;
                float maxDomainY = (room.Depth - YLength) / 2;

                float domainXDelta = maxDomainX - minDomainX;
                float domainYDelta = maxDomainY - minDomainY;

                int stepsX = 1 + Convert.ToInt16(Math.Round(domainXDelta / room.GridCellSize));
                int stepsY = 1 + Convert.ToInt16(Math.Round(domainYDelta / room.GridCellSize));

                float stepSizeX = domainXDelta / (stepsX - 1);
                float stepSizeY = domainYDelta / (stepsY - 1);

                for (int stepX = 0; stepX < stepsX; stepX++)
                {
                    for (int stepY = 0; stepY < stepsY; stepY++)
                    {
                        positionsAndRotationsDomain.Add(new Tuple<Vector2D, int>(new Vector2D(minDomainX + stepSizeX * stepX, minDomainY + stepSizeY * stepY), 0));
                        positionsAndRotationsDomain.Add(new Tuple<Vector2D, int>(new Vector2D(minDomainX + stepSizeX * stepX, minDomainY + stepSizeY * stepY), 2));
                    }
                }

                // Set domain for flipped rotations

                minDomainX = (-room.Width + YLength) / 2;
                maxDomainX = (room.Width - YLength) / 2;

                minDomainY = (-room.Depth + XLength) / 2;
                maxDomainY = (room.Depth - XLength) / 2;

                domainXDelta = maxDomainX - minDomainX;
                domainYDelta = maxDomainY - minDomainY;

                stepsX = 1 + Convert.ToInt16(Math.Round(domainXDelta / room.GridCellSize));
                stepsY = 1 + Convert.ToInt16(Math.Round(domainYDelta / room.GridCellSize));

                stepSizeX = domainXDelta / (stepsX - 1);
                stepSizeY = domainYDelta / (stepsY - 1);

                for (int stepX = 0; stepX < stepsX; stepX++)
                {
                    for (int stepY = 0; stepY < stepsY; stepY++)
                    {
                        positionsAndRotationsDomain.Add(new Tuple<Vector2D, int>(new Vector2D(minDomainX + stepSizeX * stepX, minDomainY + stepSizeY * stepY), 1));
                        positionsAndRotationsDomain.Add(new Tuple<Vector2D, int>(new Vector2D(minDomainX + stepSizeX * stepX, minDomainY + stepSizeY * stepY), 3));
                    }
                }
            }
            else if (wallDirectionsAndWallDistances.Count == 1)
            {
                int wallDirection = wallDirectionsAndWallDistances[0].Item1;
                float wallDistance = wallDirectionsAndWallDistances[0].Item2;

                // Rotate any placement unit so wall relation is toward direction 0
                RotateAround(Vector2D.Zero, 4 - wallDirection);

                // Set domain against all walls

                // Set domain against wall in direction 0

                float domainX = room.Width / 2 - wallDistance;

                float minDomainY = (-room.Depth + YLength) / 2;
                float maxDomainY = (room.Depth - YLength) / 2;
                float domainYDelta = maxDomainY - minDomainY;
                    
                int stepsY = Convert.ToInt16(Math.Round(domainYDelta / room.GridCellSize));
                float stepSizeY = domainYDelta / stepsY;

                for (int stepY = 0; stepY < stepsY; stepY++)
                {
                    positionsAndRotationsDomain.Add(new Tuple<Vector2D, int>(new Vector2D(domainX, minDomainY + stepSizeY * stepY), 0));
                }

                // Set domain against wall in direction 2

                domainX = -room.Width / 2 + wallDistance;

                minDomainY = (-room.Depth + YLength) / 2;
                maxDomainY = (room.Depth - YLength) / 2;
                domainYDelta = maxDomainY - minDomainY;

                stepsY = Convert.ToInt16(Math.Round(domainYDelta / room.GridCellSize));
                stepSizeY = domainYDelta / stepsY;

                for (int stepY = 1; stepY <= stepsY; stepY++)
                {
                    positionsAndRotationsDomain.Add(new Tuple<Vector2D, int>(new Vector2D(domainX, minDomainY + stepSizeY * stepY), 2));
                }

                // Set domain against wall in direction 1

                float domainY = room.Depth / 2 - wallDistance;

                float minDomainX = (-room.Width + YLength) / 2;
                float maxDomainX = (room.Width - YLength) / 2;
                float domainXDelta = maxDomainX - minDomainX;

                int stepsX = Convert.ToInt16(Math.Round(domainXDelta / room.GridCellSize));
                float stepSizeX = domainXDelta / stepsX;

                for (int stepX = 1; stepX <= stepsX; stepX++)
                {
                    positionsAndRotationsDomain.Add(new Tuple<Vector2D, int>(new Vector2D(minDomainX + stepSizeX * stepX, domainY), 1));
                }

                // Set domain against wall in direction 3

                domainY = -room.Depth / 2 + wallDistance;

                minDomainX = (-room.Width + YLength) / 2;
                maxDomainX = (room.Width - YLength) / 2;
                domainXDelta = maxDomainX - minDomainX;

                stepsX = Convert.ToInt16(Math.Round(domainXDelta / room.GridCellSize));
                stepSizeX = domainXDelta / stepsX;

                for (int stepX = 0; stepX < stepsX; stepX++)
                {
                    positionsAndRotationsDomain.Add(new Tuple<Vector2D, int>(new Vector2D(minDomainX + stepSizeX * stepX, domainY), 3));
                }

            }
            else if (wallDirectionsAndWallDistances.Count == 2)
            {
                // Rotate all placement unit wall relations to directions 0 and 1
                if (wallDirectionsAndWallDistances[0].Item1 == 1 || wallDirectionsAndWallDistances[1].Item1 == 2)
                {
                    RotateAround(Vector2D.Zero, 3);
                }
                else if (wallDirectionsAndWallDistances[0].Item1 == 2 || wallDirectionsAndWallDistances[1].Item1 == 3)
                {
                    RotateAround(Vector2D.Zero, 2);
                }
                else if (wallDirectionsAndWallDistances[0].Item1 == 3 || wallDirectionsAndWallDistances[1].Item1 == 0)
                {
                    RotateAround(Vector2D.Zero, 1);
                }

                // Set domain to corder points
                positionsAndRotationsDomain.Add(new Tuple<Vector2D, int>(new Vector2D(room.Width / 2 - wallDirectionsAndWallDistances[0].Item2, room.Depth / 2 - wallDirectionsAndWallDistances[1].Item2), 0));
                positionsAndRotationsDomain.Add(new Tuple<Vector2D, int>(new Vector2D(-room.Width / 2 + wallDirectionsAndWallDistances[1].Item2, room.Depth / 2 - wallDirectionsAndWallDistances[0].Item2), 1));
                positionsAndRotationsDomain.Add(new Tuple<Vector2D, int>(new Vector2D(-room.Width / 2 + wallDirectionsAndWallDistances[0].Item2, -room.Depth / 2 + wallDirectionsAndWallDistances[1].Item2), 2));
                positionsAndRotationsDomain.Add(new Tuple<Vector2D, int>(new Vector2D(room.Width / 2 - wallDirectionsAndWallDistances[1].Item2, -room.Depth / 2 + wallDirectionsAndWallDistances[0].Item2), 3));
            }
            else
            {
                Console.WriteLine("Too many wall relation constraints for a placement unit. ");
            }
        }

        /// <summary>Shuffle order of placements in domain</summary>
        public void ShuffleDomain(Random globalRandom = null)
        {
            // Start a Random instance
            if (globalRandom == null)
            {
                // Base randomization on time seed, if no seed given
                globalRandom = new Random();
            }

            // Randomly swap elements
            for (int i = 0; i < positionsAndRotationsDomain.Count; i++)
            {
                int swapIndex = i + globalRandom.Next() % (positionsAndRotationsDomain.Count - i);
                Tuple<Vector2D, int> swapPlacement = positionsAndRotationsDomain[swapIndex];
                positionsAndRotationsDomain[swapIndex] = positionsAndRotationsDomain[i];
                positionsAndRotationsDomain[i] = swapPlacement;
            }
        }

        /// <summary>Move and rotate placement unit to a spcified position and rotation in room if possible</summary>
        /// <param name="positionAndRotation">Position and rotation of placement unit placement to try</param>
        /// <param name="room">Room to try fitting into</param>
        /// <returns>Whether placement unit was moved and rotated to the specified placement</returns>
        public bool TryFitAt(Tuple<Vector2D, int> positionAndRotation, ref Room room)
        {
            // Rotate placement unit to match placement
            RotateAround(Vector2D.Zero, positionAndRotation.Item2);
            // Translate placement unit to match placement
            Translate(positionAndRotation.Item1);

            // Assume placement units fits, and check if not true
            bool fits = true;
            foreach (Fitting fitting in Members)
            {
                Vector2D minCornerFitting = new Vector2D(fitting.Position.X - fitting.XLength / 2, fitting.Position.Y - fitting.YLength / 2);
                Vector2D maxCornerFitting = new Vector2D(fitting.Position.X + fitting.XLength / 2, fitting.Position.Y + fitting.YLength / 2);

                // Test if fitting fits
                if (!room.CanAreaBeOccupied(minCornerFitting, maxCornerFitting, fitting.FittingModel.BoundingBox.Height))
                {
                    fits = false;
                    break;
                }

                // Test if fitting's clearance areas fit

                // Test clearance area in direction 0
                Vector2D minCornerClearanceAreaDir0 = new Vector2D(maxCornerFitting.X, minCornerFitting.Y);
                Vector2D maxCornerClearanceAreaDir0 = new Vector2D(maxCornerFitting.X + fitting.ClearanceAreaLengthInDirection(0), maxCornerFitting.Y);
                if (!room.CanAreaBeReserved(minCornerClearanceAreaDir0, maxCornerClearanceAreaDir0))
                {
                    fits = false;
                    break;
                }

                // Test clearance area in direction 1
                Vector2D minCornerClearanceAreaDir1 = new Vector2D(minCornerFitting.X, maxCornerFitting.Y);
                Vector2D maxCornerClearanceAreaDir1 = new Vector2D(maxCornerFitting.X, maxCornerFitting.Y + fitting.ClearanceAreaLengthInDirection(1));
                if (!room.CanAreaBeReserved(minCornerClearanceAreaDir1, maxCornerClearanceAreaDir1))
                {
                    fits = false;
                    break;
                }

                // Test clearance area in direction 2
                Vector2D minCornerClearanceAreaDir2 = new Vector2D(minCornerFitting.X - fitting.ClearanceAreaLengthInDirection(2), minCornerFitting.Y);
                Vector2D maxCornerClearanceAreaDir2 = new Vector2D(minCornerFitting.X, maxCornerFitting.Y);
                if (!room.CanAreaBeReserved(minCornerClearanceAreaDir2, maxCornerClearanceAreaDir2))
                {
                    fits = false;
                    break;
                }

                // Test clearance area in direction 3
                Vector2D minCornerClearanceAreaDir3 = new Vector2D(minCornerFitting.X, minCornerFitting.Y - fitting.ClearanceAreaLengthInDirection(3));
                Vector2D maxCornerClearanceAreaDir3 = new Vector2D(maxCornerFitting.X, minCornerFitting.Y);
                if (!room.CanAreaBeReserved(minCornerClearanceAreaDir3, maxCornerClearanceAreaDir3))
                {
                    fits = false;
                    break;
                }
            }

            if (fits)
            {
                Place(ref room);
                return true;
            }
            else
            {
                // Translate placement unit back to default
                Translate(-positionAndRotation.Item1);
                // Rotate placement unit back to default
                RotateAround(Vector2D.Zero, 4 - positionAndRotation.Item2);

                return false;
            }

        }

        /// <summary>Register placement of placement unit in room</summary>
        /// <param name="room">Room to register placement in</param>
        public void Place(ref Room room)
        {
            // Register placement
            foreach (Fitting fitting in Members)
            {
                // Calculate fitting's extreme positions
                Vector2D minCornerFitting = new Vector2D(fitting.Position.X - fitting.XLength / 2, fitting.Position.Y - fitting.YLength / 2);
                Vector2D maxCornerFitting = new Vector2D(fitting.Position.X + fitting.XLength / 2, fitting.Position.Y + fitting.YLength / 2);

                // Reserve fitting's clearance areas

                // Reserve clearance area in direction 0
                Vector2D minCornerClearanceAreaDir0 = new Vector2D(maxCornerFitting.X, minCornerFitting.Y);
                Vector2D maxCornerClearanceAreaDir0 = new Vector2D(maxCornerFitting.X + fitting.ClearanceAreaLengthInDirection(0), maxCornerFitting.Y);
                room.ReserveArea(minCornerClearanceAreaDir0, maxCornerClearanceAreaDir0);

                // Reserve clearance area in direction 1
                Vector2D minCornerClearanceAreaDir1 = new Vector2D(minCornerFitting.X, maxCornerFitting.Y);
                Vector2D maxCornerClearanceAreaDir1 = new Vector2D(maxCornerFitting.X, maxCornerFitting.Y + fitting.ClearanceAreaLengthInDirection(1));
                room.ReserveArea(minCornerClearanceAreaDir1, maxCornerClearanceAreaDir1);

                // Reserve clearance area in direction 2
                Vector2D minCornerClearanceAreaDir2 = new Vector2D(minCornerFitting.X - fitting.ClearanceAreaLengthInDirection(2), minCornerFitting.Y);
                Vector2D maxCornerClearanceAreaDir2 = new Vector2D(minCornerFitting.X, maxCornerFitting.Y);
                room.ReserveArea(minCornerClearanceAreaDir2, maxCornerClearanceAreaDir2);

                // Reserve clearance area in direction 3
                Vector2D minCornerClearanceAreaDir3 = new Vector2D(minCornerFitting.X, minCornerFitting.Y - fitting.ClearanceAreaLengthInDirection(3));
                Vector2D maxCornerClearanceAreaDir3 = new Vector2D(maxCornerFitting.X, minCornerFitting.Y);
                room.ReserveArea(minCornerClearanceAreaDir3, maxCornerClearanceAreaDir3);

                // Occupy area for fitting
                room.OccupyArea(minCornerFitting, maxCornerFitting);
            }
        }

        /// <summary>Undo registration of placement unit placment in room</summary>
        /// <param name="positionAndRotation">Position and rotation of placement unit placement registered before</param>
        /// <param name="room">Room to unregister placement unit placement from</param>
        /// <remarks>Assumes Place() has been performed at the same position and rotation</remarks>
        public void Unplace(Tuple<Vector2D, int> positionAndRotation, ref Room room)
        {
            // Unregister placement
            foreach (Fitting fitting in Members)
            {
                // Calculate fitting's extreme positions
                Vector2D minCornerFitting = new Vector2D(fitting.Position.X - fitting.XLength / 2, fitting.Position.Y - fitting.YLength / 2);
                Vector2D maxCornerFitting = new Vector2D(fitting.Position.X + fitting.XLength / 2, fitting.Position.Y + fitting.YLength / 2);

                // Unreserve fitting's clearance areas

                // Unreserve clearance area in direction 0
                Vector2D minCornerClearanceAreaDir0 = new Vector2D(maxCornerFitting.X, minCornerFitting.Y);
                Vector2D maxCornerClearanceAreaDir0 = new Vector2D(maxCornerFitting.X + fitting.ClearanceAreaLengthInDirection(0), maxCornerFitting.Y);
                room.UnreserveArea(minCornerClearanceAreaDir0, maxCornerClearanceAreaDir0);

                // Unreserve clearance area in direction 1
                Vector2D minCornerClearanceAreaDir1 = new Vector2D(minCornerFitting.X, maxCornerFitting.Y);
                Vector2D maxCornerClearanceAreaDir1 = new Vector2D(maxCornerFitting.X, maxCornerFitting.Y + fitting.ClearanceAreaLengthInDirection(1));
                room.UnreserveArea(minCornerClearanceAreaDir1, maxCornerClearanceAreaDir1);

                // Unreserve clearance area in direction 2
                Vector2D minCornerClearanceAreaDir2 = new Vector2D(minCornerFitting.X - fitting.ClearanceAreaLengthInDirection(2), minCornerFitting.Y);
                Vector2D maxCornerClearanceAreaDir2 = new Vector2D(minCornerFitting.X, maxCornerFitting.Y);
                room.UnreserveArea(minCornerClearanceAreaDir2, maxCornerClearanceAreaDir2);

                // Unreserve clearance area in direction 3
                Vector2D minCornerClearanceAreaDir3 = new Vector2D(minCornerFitting.X, minCornerFitting.Y - fitting.ClearanceAreaLengthInDirection(3));
                Vector2D maxCornerClearanceAreaDir3 = new Vector2D(maxCornerFitting.X, minCornerFitting.Y);
                room.UnreserveArea(minCornerClearanceAreaDir3, maxCornerClearanceAreaDir3);

                // Unccupy area for fitting
                room.UnoccupyArea(minCornerFitting, maxCornerFitting);
            }

            // Translate placement unit back to default
            Translate(-positionAndRotation.Item1);
            // Rotate placement unit back to default
            RotateAround(Vector2D.Zero, 4 - positionAndRotation.Item2);
        }
    }
}
