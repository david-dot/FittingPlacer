using System;
using System.Collections.Generic;
using FittingPlacer;

namespace RunApplication
{
    /// <summary>
    /// Console application for testing the fitting placer algorithm
    /// </summary>
    /// <remarks>
    /// Room dimensions, window positions, door positions, 
    /// and list of fitting models to place are hard-coded in this file. 
    /// </remarks>
    class FittingPlacerRunApplication
    {
        // Data members

        ///<summary>Instance of furniture layout algorithm solver</summary>
        private Furnisher furnisher;

        ///<summary>Placement information for fittings</summary>
        private FittingPlacement[] fittingPlacements;


        // Methods

        static void Main(string[] args)
        {
            FittingPlacerRunApplication app = new FittingPlacerRunApplication();

            app.CalculateFittingPlacements();

            // Wait for user to end
            Console.WriteLine("Algorithm has finished. Press any key to exit. ");
            Console.ReadKey();
        }

        private void CalculateFittingPlacements()
        {
            // Load fitting semantics from XML file
            furnisher = new Furnisher("FittingDatabase.xml");

            // Rectangular room dimensions (width, depth, height, 
            // list of arrays for creating the doors (center x position, center y position, 
            // door breadth in meters, axis-aligned normal direction into room, optionally height), 
            // list of arrays for creating the windows (center x position, center y position, 
            // window breadth in meters, axis-aligned normal direction into room, optionally height, optionally elevation above floor))
            Room testRoom = new Room(
              5f,
              4f,
              2.6f,
              new List<float[]>()
              {
                new float[] {-0.5f, 2f, 0.9f, (float)Math.PI/2*3}
              },
              new List<float[]>()
              {
                new float[] {-1.65f, -2f, 0.9f, (float)Math.PI/2*1},
                new float[] {-0.55f, -2f, 0.9f, (float)Math.PI/2*1},
                new float[] {0.55f, -2f, 0.9f, (float)Math.PI/2*1},
                new float[] {1.65f, -2f, 0.9f, (float)Math.PI/2*1},
              }
            );

            // List of fitting models to be placed
            List<string> fittingModelsToBePlaced = new List<string>()
            {
                "Frenhaus burlap sofa",
                "KLEA floor lamp",
                "Armaldi armchair",
                "Armaldi wooden table",
                "Armaldi wooden chair",
                "Donnerstag coffee table",
                "LuckyPanel flatscreen TV",
                "KLEA white bookcase"
            };

            // Run the fitting placer algorithm to get fitting placements
            fittingPlacements = furnisher.GeneratePlacements(testRoom, fittingModelsToBePlaced);

            // Output fitting placements
            foreach (FittingPlacement placement in fittingPlacements)
            {
                Console.WriteLine("{0}: ({1} , {2}) and {3} degrees turned. ", placement.RepresentationObject.FittingTypeId, placement.PositionX, placement.PositionY, (int)(placement.Orientation * 180 / Math.PI));
            }
        }
    }
}
