using System;
using System.Collections.Generic;

namespace FittingPlacer
{
	public class Room
	{
        // Data members

        // Dimensions
		public float Width { get; private set; }
        public float Depth { get; private set; }
        public float Height { get; private set; }

        // Static faces
        public List<Wall> Walls { get; private set; } = new List<Wall>();
        public List<Door> Doors { get; private set; } = new List<Door>();
        public List<Window> Windows { get; private set; } = new List<Window>();

        // Discretization step size for floor grids
        private const float DefaultGridCellSize = 0.1f;
        public float GridCellSize { get; private set; } = DefaultGridCellSize;

        /// <summary>Discretized room floor area, with element values signifying cell obstruction status</summary>
        /// <value>
        /// The following element values mean the cell is...
        /// -2: Occupied, -1: Occupied and has a window clearance area underneath, 0: Unoccupied and free, 
        ///  >0: Tells the number of fittings/windows/doors that have reserved the area as clearance space 
        /// </value>
        private int[,] obstructionGrid;

        /// <summary>Discretized room floor area, with element values signifying maximum fitting height allowed at cell</summary>
        /// <value>Element values tells the max height (in centimeters) of any fitting placed in the clearance area</value>
        private int[,] maxHeightGrid;


        // Constructors

        public Room(float width, float depth, float height, float gridCellSize = DefaultGridCellSize)
        {
            // Set dimensions
            Width = width;
            Depth = depth;
            Height = height;

            // Set grid cell size
            GridCellSize = gridCellSize;

            // Create walls
            CreateWalls();

            // Discretize room floor area
            CreateFloorGrids();
        }

        public Room(float width, float depth, float height, List<float[]> doors, List<float[]> windows, float gridCellSize = DefaultGridCellSize)
        {
            // Set dimensions
            Width = width;
            Depth = depth;
            Height = height;

            // Set grid cell size
            GridCellSize = gridCellSize;

            // Create walls
            CreateWalls();

            // Add doors
            foreach (float[] door in doors)
            {
                // Elements mean... [0]: center x position, [1]: center y position, [2]: door breadth in meters, 
                // [3]: axis-aligned normal direction into room, optional [4]: height

                // Convert inwards normal direction from radians to axis direction number
                int inwardsNormalDirection = (int)Math.Round(door[3] / Math.PI * 2) % 4;

                if (door.Length == 5)
                {
                    // With optional element
                    this.Doors.Add(new Door(new Vector2D(door[0], door[1]), door[2], inwardsNormalDirection, door[4]));
                }
                else
                {
                    this.Doors.Add(new Door(new Vector2D(door[0], door[1]), door[2], inwardsNormalDirection));
                }
            }

            // Add windows
            foreach (float[] window in windows)
            {
                // Elements mean... [0]: center x position, [1]: center y position, [2]: window breadth in meters, 
                // [3]: axis-aligned normal direction into room, optional [4]: height, optional [5]: elevation above floor

                // Convert inwards normal direction from radians to axis direction number
                int inwardsNormalDirection = (int)Math.Round(window[3] / Math.PI * 2) % 4;

                if (window.Length == 6)
                {
                    // With optional elements
                    this.Windows.Add(new Window(new Vector2D(window[0], window[1]), window[2], inwardsNormalDirection, window[4], window[5]));
                }
                else
                {
                    this.Windows.Add(new Window(new Vector2D(window[0], window[1]), window[2], inwardsNormalDirection));
                }
            }

            // Discretize room floor area
            CreateFloorGrids();
        }

        public Room(float width, float depth, float height, List<Wall> walls, List<Door> doors, List<Window> windows, int[,] obstructionGrid, int[,] maxHeightGrid, float gridCellSize)
        {
            // Set dimensions
            Width = width;
            Depth = depth;
            Height = height;

            // Set grid cell size
            GridCellSize = gridCellSize;

            // Take argument lists of walls, doors, and windows
            Walls = walls;
            Doors = doors;
            Windows = windows;

            // Take discretized room floor area argument arrays
            this.obstructionGrid = obstructionGrid;
            this.maxHeightGrid = maxHeightGrid;
        }


        // Methods

        public Room Clone()
        {
            int[,] obstructionGridClone = (int[,])obstructionGrid.Clone();
            int[,] maxHeightGridClone = (int[,])maxHeightGrid.Clone();

            return (new Room(Width, Depth, Height, Walls, Doors, Windows, obstructionGridClone, maxHeightGridClone, GridCellSize));
        }

        private void CreateWalls()
        {
            // Right wall
            Walls.Add(new Wall(
              new Vector2D(Width/2, 0),
              Depth,
              2 
            ));

            // Back wall
            Walls.Add(new Wall(
              new Vector2D(0, Depth / 2),
              Width,
              3
            ));

            // Left wall
            Walls.Add(new Wall(
              new Vector2D(-Width / 2, 0),
              Depth,
              0
            ));

            // Front wall
            Walls.Add(new Wall(
              new Vector2D(0, -Depth / 2),
              Width,
              1
            ));
        }

        private void CreateFloorGrids()
        {
            // Create floor grid
            int xSteps = (int)Math.Ceiling((double)Width / GridCellSize);
            int ySteps = (int)Math.Ceiling((double)Depth / GridCellSize);
            obstructionGrid = new int[xSteps, ySteps];
            maxHeightGrid = new int[xSteps, ySteps];

            // Reserve clearance areas for static faces
            foreach (Door door in Doors)
            {
                RegisterStaticFaceInFloorGrids(door);
            }
            foreach (Window window in Windows)
            {
                RegisterStaticFaceInFloorGrids(window);
            }

        }

        private void RegisterStaticFaceInFloorGrids(StaticFace staticFace)
        {
            // Get extreme positions for clearance area

            Vector2D minPositionClearanceArea;
            Vector2D maxPositionClearanceArea;

            if (staticFace.InwardsNormalDirection == 0)
            {
                minPositionClearanceArea = staticFace.Position - staticFace.VectorAlongFace / 2 * staticFace.SideLength;
                maxPositionClearanceArea = staticFace.Position + staticFace.VectorAlongFace / 2 * staticFace.SideLength + staticFace.InwardsNormalVector * staticFace.ClearanceAreaLength;
            }
            else if (staticFace.InwardsNormalDirection == 1)
            {
                minPositionClearanceArea = staticFace.Position + staticFace.VectorAlongFace / 2 * staticFace.SideLength;
                maxPositionClearanceArea = staticFace.Position - staticFace.VectorAlongFace / 2 * staticFace.SideLength + staticFace.InwardsNormalVector * staticFace.ClearanceAreaLength;
            }
            else if (staticFace.InwardsNormalDirection == 2)
            {
                minPositionClearanceArea = staticFace.Position + staticFace.VectorAlongFace / 2 * staticFace.SideLength + staticFace.InwardsNormalVector * staticFace.ClearanceAreaLength;
                maxPositionClearanceArea = staticFace.Position - staticFace.VectorAlongFace / 2 * staticFace.SideLength;
            }
            else
            {
                minPositionClearanceArea = staticFace.Position - staticFace.VectorAlongFace / 2 * staticFace.SideLength + staticFace.InwardsNormalVector * staticFace.ClearanceAreaLength;
                maxPositionClearanceArea = staticFace.Position + staticFace.VectorAlongFace / 2 * staticFace.SideLength;
            }

            // Reserve clearance area
            if (staticFace.StaticFaceType == StaticFace.StaticFaceTypes.Window)
            {
                ReserveArea(minPositionClearanceArea, maxPositionClearanceArea, ((Window)staticFace).Elevation);
            }
            else
            {
                ReserveArea(minPositionClearanceArea, maxPositionClearanceArea);
            }
        }

        /// <summary>Checks if area can be occupied</summary>
        /// <param name="minCornerPosition">Minimum position of area rectangle</param>
        /// <param name="maxCornerPosition">Maximum position of area rectangle</param>
        /// <param name="height">Height of what is checking if area can be occupied in meters</param>
        /// <returns>Whether area can be occupied</returns>
        public bool CanAreaBeOccupied(Vector2D minCornerPosition, Vector2D maxCornerPosition, float height)
        {
            if (minCornerPosition.X == maxCornerPosition.X || minCornerPosition.Y == maxCornerPosition.Y)
            {
                // No area to occupy
                return true;
            }
            else
            {
                // Convert height to discrete centimeters
                int heightInCentimeters = (int)(height * 100);

                // Find cell indices for rectangle corners
                int minIndexX;
                int maxIndexX;
                int minIndexY;
                int maxIndexY;
                PositionToCellIndices(minCornerPosition, out minIndexX, out minIndexY, false);
                PositionToCellIndices(maxCornerPosition, out maxIndexX, out maxIndexY, true);

                for (int indexX = minIndexX; indexX <= maxIndexX; indexX++)
                {
                    for (int indexY = minIndexY; indexY <= maxIndexY; indexY++)
                    {
                        if (!CanCellBeOccupied(indexX, indexY, heightInCentimeters))
                        {
                            return false;
                        }
                    }
                }

                return true;
            }
        }

        /// <summary>Occupies floor area</summary>
        /// <param name="minCornerPosition">Minimum position of area rectangle</param>
        /// <param name="maxCornerPosition">Maximum position of area rectangle</param>
        /// <remarks>Assumes CanAreaBeOccupied() has been checked</remarks>
        public void OccupyArea(Vector2D minCornerPosition, Vector2D maxCornerPosition)
        {
            if (minCornerPosition.X == maxCornerPosition.X || minCornerPosition.Y == maxCornerPosition.Y)
            {
                // No area to occupy
            }
            else
            {
                // Find cell indices for rectangle corners
                int minIndexX;
                int maxIndexX;
                int minIndexY;
                int maxIndexY;
                PositionToCellIndices(minCornerPosition, out minIndexX, out minIndexY, false);
                PositionToCellIndices(maxCornerPosition, out maxIndexX, out maxIndexY, true);

                for (int indexX = minIndexX; indexX <= maxIndexX; indexX++)
                {
                    for (int indexY = minIndexY; indexY <= maxIndexY; indexY++)
                    {
                        OccupyCell(indexX, indexY);
                    }
                }
            }
        }

        /// <summary>Undoes occupation of floor area</summary>
        /// <param name="minCornerPosition">Minimum position of area rectangle</param>
        /// <param name="maxCornerPosition">Maximum position of area rectangle</param>
        /// <remarks>Assumes OccupyArea() has previously been performed for the same area</remarks>
        public void UnoccupyArea(Vector2D minCornerPosition, Vector2D maxCornerPosition)
        {
            if (minCornerPosition.X == maxCornerPosition.X || minCornerPosition.Y == maxCornerPosition.Y)
            {
                // No area to unoccupy
            }
            else
            {
                // Find cell indices for rectangle corners
                int minIndexX;
                int maxIndexX;
                int minIndexY;
                int maxIndexY;
                PositionToCellIndices(minCornerPosition, out minIndexX, out minIndexY, false);
                PositionToCellIndices(maxCornerPosition, out maxIndexX, out maxIndexY, true);

                for (int indexX = minIndexX; indexX <= maxIndexX; indexX++)
                {
                    for (int indexY = minIndexY; indexY <= maxIndexY; indexY++)
                    {
                        UnoccupyCell(indexX, indexY);
                    }
                }
            }
        }

        /// <summary>Checks if cell can be occupied</summary>
        /// <param name="indexX">X index in floor grid</param>
        /// <param name="indexY">Y index in floor grid</param>
        /// <param name="heightInCentimeters">Height of what is checking if cell can be occupied in centimeters</param>
        /// <returns>Whether cell can be occupied</returns>
        private bool CanCellBeOccupied(int indexX, int indexY, int heightInCentimeters)
        {
            int obstructionValue = obstructionGrid[indexX, indexY];

            if (obstructionValue == 0)
            {
                // Cell is unoccupied and free
                return true;
            }
            else if (obstructionValue == 1)
            {
                // There is only one clearance area in the cell, check if it merely has a max height constraint
                int maxHeight = maxHeightGrid[indexX, indexY];
                if (maxHeight > 0 && heightInCentimeters < maxHeight)
                {
                    // The fitting fits under the clearance area max height
                    return true;
                }
            }

            // Cell was found to be obstructed or reserved as clearance area
            return false;
        }

        /// <summary>Occupies floor grid cell</summary>
        /// <param name="indexX">X index in floor grid</param>
        /// <param name="indexY">Y index in floor grid</param>
        /// <remarks>Assumes CanCellBeOccupied() has been checked</remarks>
        private void OccupyCell(int indexX, int indexY)
        {
            if (maxHeightGrid[indexX, indexY] == 0)
            {
                // Set cell to occupied
                obstructionGrid[indexX, indexY] = -2;
            }
            else
            {
                // Set to different occupied value to remember that there is a max height clearance area at cell
                obstructionGrid[indexX, indexY] = -1;
            }
        }

        /// <summary>Undoes occupation of floor grid cell</summary>
        /// <param name="indexX">X index in floor grid</param>
        /// <param name="indexY">Y index in floor grid</param>
        /// <remarks>Assumes OccupyCell() has previously been performed for the same cell</remarks>
        private void UnoccupyCell(int indexX, int indexY)
        {
            // Restore to unoccupied state
            obstructionGrid[indexX, indexY] += 2;
        }

        /// <summary>Checks if area can be reserved for a clearance area</summary>
        /// <param name="minCornerPosition">Minimum position of area rectangle</param>
        /// <param name="maxCornerPosition">Maximum position of area rectangle</param>
        /// <returns>Whether area can be reserved for a clearance area</returns>
        public bool CanAreaBeReserved(Vector2D minCornerPosition, Vector2D maxCornerPosition)
        {
            if (minCornerPosition.X == maxCornerPosition.X || minCornerPosition.Y == maxCornerPosition.Y)
            {
                // No area to reserve
                return true;
            }
            else
            {
                // Find cell indices for rectangle corners
                int minIndexX;
                int maxIndexX;
                int minIndexY;
                int maxIndexY;
                PositionToCellIndices(minCornerPosition, out minIndexX, out minIndexY, false);
                PositionToCellIndices(maxCornerPosition, out maxIndexX, out maxIndexY, true);

                for (int indexX = minIndexX; indexX <= maxIndexX; indexX++)
                {
                    for (int indexY = minIndexY; indexY <= maxIndexY; indexY++)
                    {
                        if (!CanCellBeReserved(indexX, indexY))
                        {
                            return false;
                        }
                    }
                }

                return true;
            }
        }

        /// <summary>Reserve floor area for a clearance area</summary>
        /// <param name="minCornerPosition">Minimum position of area rectangle</param>
        /// <param name="maxCornerPosition">Maximum position of area rectangle</param>
        /// <remarks>Assumes CanAreaBeReserved() has been checked for the same area</remarks>
        public void ReserveArea(Vector2D minCornerPosition, Vector2D maxCornerPosition)
        {
            if (minCornerPosition.X == maxCornerPosition.X || minCornerPosition.Y == maxCornerPosition.Y)
            {
                // No area to reserve
            }
            else
            {
                // Find cell indices for rectangle corners
                int minIndexX;
                int minIndexY;
                int maxIndexX;
                int maxIndexY;
                PositionToCellIndices(minCornerPosition, out minIndexX, out minIndexY, false);
                PositionToCellIndices(maxCornerPosition, out maxIndexX, out maxIndexY, true);

                for (int indexX = minIndexX; indexX <= maxIndexX; indexX++)
                {
                    for (int indexY = minIndexY; indexY <= maxIndexY; indexY++)
                    {
                        ReserveCell(indexX, indexY);
                    }
                }
            }
        }

        /// <summary>Reserve floor area for a clearance area</summary>
        /// <param name="minCornerPosition">Minimum position of area rectangle</param>
        /// <param name="maxCornerPosition">Maximum position of area rectangle</param>
        /// <param name="maxHeight">Allow for occupation of things that are up to maxHeight tall in this clearance area</param>
        /// <remarks>Only for initial reservation for static clearance areas</remarks>
        public void ReserveArea(Vector2D minCornerPosition, Vector2D maxCornerPosition, float maxHeight)
        {
            if (minCornerPosition.X == maxCornerPosition.X || minCornerPosition.Y == maxCornerPosition.Y)
            {
                // No area to reserve
            }
            else
            {
                // Find cell indices for rectangle corners
                int minIndexX;
                int maxIndexX;
                int minIndexY;
                int maxIndexY;
                PositionToCellIndices(minCornerPosition, out minIndexX, out minIndexY, false);
                PositionToCellIndices(maxCornerPosition, out maxIndexX, out maxIndexY, true);

                for (int indexX = minIndexX; indexX <= maxIndexX; indexX++)
                {
                    for (int indexY = minIndexY; indexY <= maxIndexY; indexY++)
                    {
                        ReserveCell(indexX, indexY, maxHeight);
                    }
                }
            }

        }

        /// <summary>Undoes one reservation for clerance area in the specified floor area</summary>
        /// <param name="minCornerPosition">Minimum position of area rectangle</param>
        /// <param name="maxCornerPosition">Maximum position of area rectangle</param>
        /// <remarks>Assumes ReserveArea() has previously been performed for the same area</remarks>
        public void UnreserveArea(Vector2D minCornerPosition, Vector2D maxCornerPosition)
        {
            if (minCornerPosition.X == maxCornerPosition.X || minCornerPosition.Y == maxCornerPosition.Y)
            {
                // No area to unoccupy
            }
            else
            {
                // Find cell indices for rectangle corners
                int minIndexX;
                int maxIndexX;
                int minIndexY;
                int maxIndexY;
                PositionToCellIndices(minCornerPosition, out minIndexX, out minIndexY, false);
                PositionToCellIndices(maxCornerPosition, out maxIndexX, out maxIndexY, true);

                for (int indexX = minIndexX; indexX <= maxIndexX; indexX++)
                {
                    for (int indexY = minIndexY; indexY <= maxIndexY; indexY++)
                    {
                        UnreserveCell(indexX, indexY);
                    }
                }
            }
        }

        /// <summary>Checks if cell can be reserved for a clearance area</summary>
        /// <param name="indexX">X index in floor grid</param>
        /// <param name="indexY">Y index in floor grid</param>
        /// <returns>Whether cell can be reserved for a clearance area</returns>
        private bool CanCellBeReserved(int indexX, int indexY)
        {
            if (obstructionGrid[indexX, indexY] >= 0)
            {
                // Cell is unoccupied
                return true;
            }
            else
            {
                // Cell is occupied
                return false;
            }
        }

        /// <summary>Reserves floor grid cell for a clearance area</summary>
        /// <param name="indexX">X index in floor grid</param>
        /// <param name="indexY">Y index in floor grid</param>
        /// <remarks>Assumes CanCellBeReserved() has been checked</remarks>
        private void ReserveCell(int indexX, int indexY)
        {
            if (obstructionGrid[indexX, indexY] >= 0)
            {
                obstructionGrid[indexX, indexY]++;
            }
        }

        /// <summary>Reserves floor grid cell for initial, static clearance area with an allowed maximum height</summary>
        /// <param name="indexX">X index in floor grid</param>
        /// <param name="indexY">Y index in floor grid</param>
        /// <remarks>Allow for occupation of things that are up to maxHeight tall in this clearance area cell</remarks>
        private void ReserveCell(int indexX, int indexY, float maxHeight)
        {
            // Only set one "layer" of reservation for max height clearance area, as it is static
            obstructionGrid[indexX, indexY] = 1;

            if (maxHeight < 1f)
            {
                maxHeight = 1f;
            }
            if (maxHeight > maxHeightGrid[indexX, indexY])
            {
                // Convert to centimeters and store as integer
                maxHeightGrid[indexX, indexY] = (int)(maxHeight * 100);
            }
        }

        /// <summary>Undoes one reservation for clerance area in floor grid cell</summary>
        /// <param name="indexX">X index in floor grid</param>
        /// <param name="indexY">Y index in floor grid</param>
        /// <remarks>Assumes ReserveCell() has previously been performed for the same cell</remarks>
        private void UnreserveCell(int indexX, int indexY)
        {
            if (obstructionGrid[indexX, indexY] > 0)
            {
                if (obstructionGrid[indexX, indexY] > 1 || maxHeightGrid[indexX, indexY] == 0)
                {
                    obstructionGrid[indexX, indexY]--;
                }
                // The last clearance area layer is reserved for window
            }
        }

        /// <summary>Returns corresponding floor grid cell indices for a room position</summary>
        /// <param name="position">Position to convert to floor grid cell indices</param>
        /// <param name="indexX">Corresponding cell x index in floor grid</param>
        /// <param name="indexY">Corresponding cell y index in floor grid</param>
        /// <param name="adjustDown">Whether to adjust edge cases in negative direction</param>
        private void PositionToCellIndices(Vector2D position, out int indexX, out int indexY, bool adjustDown)
        {
            // Adjust to origin
            Vector2D positionFromCorner = new Vector2D(position.X + Width / 2, position.Y + Depth / 2);

            //// Adjust for rounding and precision errors
            if (adjustDown)
            {
                positionFromCorner = new Vector2D(positionFromCorner.X - 0.00001f, positionFromCorner.Y - 0.00001f);
            }
            else
            {
                positionFromCorner = new Vector2D(positionFromCorner.X + 0.00001f, positionFromCorner.Y + 0.00001f);
            }

            // Discretize position by StepSize and set out parameters

            positionFromCorner /= GridCellSize;

            indexX = (int)positionFromCorner.X;
            indexY = (int)positionFromCorner.Y;

            // Handle outliers due to insufficient precision
            if (indexX >= obstructionGrid.GetLength(0))
            {
                indexX = obstructionGrid.GetLength(0) - 1;
            }
            else if (indexX <= -1)
            {
                indexX = 0;
            }
            if (indexY >= obstructionGrid.GetLength(1))
            {
                indexY = obstructionGrid.GetLength(1) - 1;
            }
            else if (indexY <= -1)
            {
                indexY = 0;
            }

        }

        private int GetObstructionValueAt(Vector2D position, bool adjustDown)
        {
            // Get indices
            int indexX;
            int indexY;
            PositionToCellIndices(position, out indexX, out indexY, adjustDown);

            // Return value
            return obstructionGrid[indexX, indexY];
        }

        private int GetMaxHeightInCentimetersAtPosition(Vector2D position, bool adjustDown)
        {
            // Get indices
            int indexX;
            int indexY;
            PositionToCellIndices(position, out indexX, out indexY, adjustDown);

            // Return value
            return maxHeightGrid[indexX, indexY];
        }

        /// <summary>
        /// Represents floor obstruction values in a grid
        /// </summary>
        /// <returns>Multi-line string representation of floor obstruction grid map</returns>
        public override string ToString()
        {
            string representation = "Grid of obstruction values for room with dimensions (width, depth, height):("+Width+" , "+Depth+" , "+Height+"): \n";
            for (int indexY = obstructionGrid.GetLength(1) - 1; indexY >= 0; indexY--)
            {
                for (int indexX = 0; indexX < obstructionGrid.GetLength(0); indexX++)
                {
                    representation += obstructionGrid[indexX, indexY].ToString().PadRight(3);
                }
                representation += "\n";
            }
            return representation;
        }
    }
}
