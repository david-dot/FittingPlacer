using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Linq;

namespace FittingPlacer
{
	public class Furnisher
	{
        // Data members

        protected List<FittingType> fittingTypes;
        protected List<FittingModel> fittingModels;
        protected List<FaceType> faceTypes;
        public const string DefaultSemanticsFilePath = "FittingDatabase.xml";
        private bool hasSemanticsLoaded = false;

        private FaceType wallFaceType = new FaceType("wall");
        private FaceType doorFaceType = new FaceType("door");
        private FaceType windowFaceType = new FaceType("window");


        // Constructor

        public Furnisher(String semanticsFilePath = DefaultSemanticsFilePath)
        {
            LoadFittingSemantics(semanticsFilePath);
        }


        // Methods

        /// <summary>Load fitting semantics from XML file to memory</summary>
        public void LoadFittingSemantics(String semanticsFilePath = DefaultSemanticsFilePath)
        {
            // Add default face types
            faceTypes = new List<FaceType>();
            faceTypes.Add(wallFaceType);
            faceTypes.Add(doorFaceType);
            faceTypes.Add(windowFaceType);

            // Open semantics XML document file
            XDocument doc = XDocument.Load(semanticsFilePath);

            // Load face types
            List<Tuple<XElement, FaceType>> xFaceTypeRelations = new List<Tuple<XElement, FaceType>>();
            foreach (XElement xFaceType in doc.Element("fitting_database").Element("face_types").Elements("face_type"))
            {
                faceTypes.Add(new FaceType(
                  xFaceType.Element("face_type_id").Value
                ));

                // Save spatial relation elements
                if (xFaceType.Element("relations") != null)
                {
                    foreach (XElement xSpatialRelation in xFaceType.Element("relations").Elements("relation"))
                    {
                        xFaceTypeRelations.Add(new Tuple<XElement, FaceType>(xSpatialRelation, faceTypes[faceTypes.Count - 1]));
                    }
                }
            }

            // Load face relations
            foreach (Tuple<XElement, FaceType> xFaceTypeRelationTuple in xFaceTypeRelations)
            {
                XElement xFaceTypeRelation = xFaceTypeRelationTuple.Item1;
                FaceType relatedFaceType = faceTypes.Find(faceTypeEvaluated => faceTypeEvaluated.Id == xFaceTypeRelation.Element("support_face_type_id").Value);
                xFaceTypeRelationTuple.Item2.AddRelation(
                  new SpatialRelation(
                    relatedFaceType,
                    (float)(double)xFaceTypeRelation.Element("distance")
                  )
                );
            }

            // Load fitting types
            fittingTypes = new List<FittingType>();
            foreach (XElement xFittingType in doc.Element("fitting_database").Element("fitting_types").Elements("fitting_type"))
            {
                // Create and add fitting type
                FittingType fittingType = new FittingType(xFittingType.Element("fitting_type_id").Value);
                fittingTypes.Add(fittingType);

                // Save face elements
                foreach (XElement xFace in xFittingType.Element("faces").Elements("face"))
                {
                    // Parse and cast facing enum
                    Facing facing;
                    Enum.TryParse<Facing>(xFace.Element("facing").Value, true, out facing);

                    fittingType.AddFace(
                      new Face(
                        facing,
                        faceTypes.Find(faceTypeEvaluated => faceTypeEvaluated.Id == xFace.Element("face_type_id").Value)
                      )
                    );
                }

            }

            // Load fitting models
            fittingModels = new List<FittingModel>();
            foreach (XElement xFittingModel in doc.Element("fitting_database").Element("fitting_models").Elements("fitting_model"))
            {
                // Parse fitting type and find reference
                string typeId = xFittingModel.Element("fitting_type_id").Value;
                FittingType fittingType = fittingTypes.Find(fittingTypeEvaluated => fittingTypeEvaluated.Id == typeId);

                // Create fitting model with parsed values
                FittingModel fittingModel = new FittingModel(
                  xFittingModel.Element("fitting_model_id").Value,
                  fittingType,
                  (float)(double)xFittingModel.Element("bounding_box").Element("width"),
                  (float)(double)xFittingModel.Element("bounding_box").Element("depth"),
                  (float)(double)xFittingModel.Element("bounding_box").Element("height")
                );

                // Add clearance areas
                foreach (XElement xClearanceArea in xFittingModel.Element("clearance_areas").Elements("clearance_area"))
                {
                    if ((double)xClearanceArea.Element("perpendicular_length") > 0)
                    {
                        // Parse and cast facing enum
                        Facing facing;
                        Enum.TryParse<Facing>(xClearanceArea.Element("side").Value, true, out facing);

                        fittingModel.AddClearanceArea(facing, (float)(double)xClearanceArea.Element("perpendicular_length"));
                    }
                }

                // Add fitting model
                fittingModels.Add(fittingModel);
            }

            hasSemanticsLoaded = true;
        }

        /// <summary>Generate fitting layout and return the fittings' placements</summary>
        public FittingPlacement[] GeneratePlacements(Room room, List<string> fittingsToPlace, int randomizationSeed = 0)
        {
            // Find fitting models in database
            List<FittingModel> fittingModelsToPlace = new List<FittingModel>();
            foreach (string fittingToPlace in fittingsToPlace)
            {
                fittingModelsToPlace.Add(fittingModels.Find(evalued => evalued.Id == fittingToPlace));
            }

            // Generate layout
            FittingLayout generatedFittingLayout = GenerateFittingLayout(room, fittingModelsToPlace, randomizationSeed);
            List<Fitting> placedFittings = generatedFittingLayout.PlacedFittings;

            // Copy Fitting information to FittingPlacement objects
            FittingPlacement[] fittingPlacements = new FittingPlacement[placedFittings.Count];
            int i = 0;
            foreach (Fitting placedFitting in placedFittings)
            {
                fittingPlacements[i] = new FittingPlacement(placedFitting.Position.X, placedFitting.Position.Y, placedFitting.Orientation, new RepresentationObject(placedFitting.FittingModel.Id, placedFitting.FittingModel.FittingType.Id));
                i++;
            }

            return fittingPlacements;
        }

        /// <summary>Generate fitting layout and return the fittings' placements</summary>
        public FittingPlacement[] GeneratePlacements(float width, float depth, float height, List<string> fittingsToPlace, int randomizationSeed = 0)
        {
            return GeneratePlacements(new Room(width, depth, height), fittingsToPlace, randomizationSeed);
        }

        /// <summary>Generate fitting layout consisting of the fittings and their placements</summary>
        private FittingLayout GenerateFittingLayout(Room room, List<FittingModel> fittingModelsToBePlaced, int randomizationSeed = 0)
        {
            // Make sure fitting semantics are loaded
            if (!hasSemanticsLoaded)
            {
                LoadFittingSemantics(DefaultSemanticsFilePath);
            }

            // Start a Random instance
            Random globalRandom;
            if (randomizationSeed == 0)
            {
                // Base randomization on time seed, if no seed given
                globalRandom = new Random();
            }
            else
            {
                globalRandom = new Random(randomizationSeed);
            }


            // Solve placements

            // Register fittings and their faces
            List<Fitting> fittingsToBePlaced = new List<Fitting>();
            List<ParticularFace> fittingsFaces = new List<ParticularFace>();
            foreach (FittingModel fittingModelToBePlaced in fittingModelsToBePlaced)
            {
                Fitting fittingToBePlaced = new Fitting(fittingModelToBePlaced);
                fittingsToBePlaced.Add(fittingToBePlaced);

                foreach (ParticularFace fittingFace in fittingToBePlaced.Faces)
                {
                    fittingsFaces.Add(fittingFace);
                }
            }


            // Assign attacher faces to compatible support faces

            // List to be filled with up to one face with wall relation per fitting, that is selected to be satisfied
            List<Tuple<ParticularFace, SpatialRelation>> selectedAttacherFacesAndWallRelations = new List<Tuple<ParticularFace, SpatialRelation>>();

            foreach (Fitting fittingToBePlaced in fittingsToBePlaced)
            {
                // List to be filled with faces with fitting relations that can be satisfied
                List<Tuple<ParticularFace, ParticularFace, SpatialRelation>> attacherFacesAndSupportFacesAndRelations = new List<Tuple<ParticularFace, ParticularFace, SpatialRelation>>();
                int fittingRelationCount = 0;

                // List to be filled with faces with wall relations
                List<Tuple<ParticularFace, SpatialRelation>> attacherFacesAndWallRelations = new List<Tuple<ParticularFace, SpatialRelation>>();

                // Go through every relation in the fitting's faces, and register satisfiable ones
                foreach (ParticularFace aFace in fittingToBePlaced.Faces)
                {
                    foreach (SpatialRelation relation in aFace.Face.Type.SpatialRelations)
                    {
                        if (relation.SupportFaceType == wallFaceType || relation.SupportFaceType == doorFaceType || relation.SupportFaceType == windowFaceType)
                        {
                            if (relation.SupportFaceType == wallFaceType)
                            {
                                // Register wall related face
                                attacherFacesAndWallRelations.Add(new Tuple<ParticularFace, SpatialRelation>(aFace, relation));
                            }
                        }
                        else
                        {
                            fittingRelationCount++;

                            // Register found support faces
                            List<ParticularFace> supportFaces = new List<ParticularFace>();
                            foreach (ParticularFace sFace in fittingsFaces)
                            {
                                if (sFace.Face.Type == relation.SupportFaceType)
                                {
                                    supportFaces.Add(sFace);
                                }
                            }

                            // Assign support faces to fulfill relation
                            if (supportFaces.Count > 0)
                            {
                                // Pick a random support face as the default and check if there are less occupied ones
                                int startIndex = globalRandom.Next() % supportFaces.Count;
                                ParticularFace minOccupiedSupportFace = supportFaces[startIndex];
                                foreach (ParticularFace supportFace in supportFaces)
                                {
                                    if (supportFace.ReservedLength < minOccupiedSupportFace.ReservedLength)
                                    {
                                        minOccupiedSupportFace = supportFace;
                                    }
                                }

                                if (minOccupiedSupportFace.CanAttachFace(aFace, relation))
                                {
                                    // Register face pair that satisfies relation
                                    attacherFacesAndSupportFacesAndRelations.Add(new Tuple<ParticularFace, ParticularFace, SpatialRelation>(aFace, minOccupiedSupportFace, relation));
                                }
                                else
                                {
                                    // Pick a random support face as the default and check if there are freer ones
                                    startIndex = globalRandom.Next() % supportFaces.Count;
                                    ParticularFace mostFreeSupportFace = supportFaces[startIndex];
                                    foreach (ParticularFace supportFace in supportFaces)
                                    {
                                        if (supportFace.SideLength - supportFace.ReservedLength > mostFreeSupportFace.SideLength - mostFreeSupportFace.ReservedLength)
                                        {
                                            mostFreeSupportFace = supportFace;
                                        }
                                    }

                                    if (mostFreeSupportFace.CanAttachFace(aFace, relation))
                                    {
                                        // Register face pair that satisfies relation
                                        attacherFacesAndSupportFacesAndRelations.Add(new Tuple<ParticularFace, ParticularFace, SpatialRelation>(aFace, mostFreeSupportFace, relation));
                                    }
                                }
                            }
                        }
                    }
                }

                // Assign one fitting attacher face to its support face
                if (attacherFacesAndSupportFacesAndRelations.Count() > 0)
                {
                    // Pick only one fitting relation to fulfill per fitting
                    int pickedIndex = globalRandom.Next() % attacherFacesAndSupportFacesAndRelations.Count;
                    Tuple<ParticularFace, ParticularFace, SpatialRelation> attacherFaceAndSupportFaceAndRelation = attacherFacesAndSupportFacesAndRelations[pickedIndex];
                    attacherFaceAndSupportFaceAndRelation.Item2.AttachFace(attacherFaceAndSupportFaceAndRelation.Item1, attacherFaceAndSupportFaceAndRelation.Item3);
                }
                else if (fittingRelationCount > 0)
                {
                    Console.WriteLine("No fitting support face with capacity could be found for a {0} {1}. ", fittingToBePlaced.FittingModel.Id, fittingToBePlaced.FittingModel.FittingType.Id);
                }

                // Pick only one wall relation to fulfill per fitting
                if (attacherFacesAndWallRelations.Count > 0)
                {
                    int selectedIndex = globalRandom.Next() % attacherFacesAndWallRelations.Count;
                    selectedAttacherFacesAndWallRelations.Add(attacherFacesAndWallRelations[selectedIndex]);
                }

            }

            // Place attacher faces (and their fittings with them) in relation to support faces, in grouping placement units
            foreach (ParticularFace sFace in fittingsFaces)
            {
                foreach (Tuple<ParticularFace, SpatialRelation> aFaceAndRelation in sFace.attacherFacesAndRelations)
                {
                    ParticularFace aFace = aFaceAndRelation.Item1;
                    SpatialRelation relation = aFaceAndRelation.Item2;

                    // Rotate objects to match faces
                    aFace.RotateToFace(sFace.Direction);

                    // Move objects to match face distance
                    aFace.TranslateToDistanceFromFace(relation.Distance, sFace);

                    // Merge placement units
                    sFace.Fitting.PlacementUnit.AbsorbPlacementUnit(aFace.Fitting.PlacementUnit);
                }
            }

            // Register wall-attacher faces to placement units
            foreach (Tuple<ParticularFace, SpatialRelation> attacherFaceAndWallRelation in selectedAttacherFacesAndWallRelations)
            {
                attacherFaceAndWallRelation.Item1.Fitting.PlacementUnit.AddWallConstraint(attacherFaceAndWallRelation.Item1, attacherFaceAndWallRelation.Item2.Distance);
            }

            // Register all consolidated placement units
            List<PlacementUnit> placementUnits = new List<PlacementUnit>();
            foreach (Fitting fitting in fittingsToBePlaced)
            {
                if (!placementUnits.Contains(fitting.PlacementUnit))
                {
                    placementUnits.Add(fitting.PlacementUnit);
                }
            }

            // Set placement domains for the placement units
            foreach (PlacementUnit placementUnit in placementUnits)
            {
                // Set placement domain
                placementUnit.SetDomain(room);

                // Shuffle placement domain
                placementUnit.ShuffleDomain(globalRandom);
            }

            // Test placements for all placement units
            if (!TestPlacements(ref placementUnits, room.Clone()))
            {
                // No possible fitting layout could be found
                Console.WriteLine("No possible fitting layout could be found. ");
                return (new FittingLayout(room, new List<Fitting>()));
            }

            // Return finished list of placed fittings
            return (new FittingLayout(room, fittingsToBePlaced));
        }

        /// <summary>Tries to rotate and set positions for placement units in a way so they fit the room</summary>
        /// <param name="placementUnits">List of placement unit to place in room</param>
        /// <param name="room">Room to place placement units in</param>
        /// <returns>Whether there were any possible arrangement of the placement units in the room</returns>
        private bool TestPlacements(ref List<PlacementUnit> placementUnits, Room room)
        {
            return TestPlacementRecursive(ref placementUnits, 0, ref room);
        }

        /// <summary>Tries to fit placement units in list -- from indexToTest to the end of the list -- in the room</summary>
        /// <param name="placementUnits">List of placement unit to place in room</param>
        /// <param name="indexToTest">List index of placement unit to try to fit in room</param>
        /// <param name="room">Room to place placement units in</param>
        /// <returns>
        /// Whether there were any possible arrangement of the placement units 
        /// at indexToTest in list and subsequent placement units in list
        /// </returns>
        private bool TestPlacementRecursive(ref List<PlacementUnit> placementUnits, int indexToTest, ref Room room)
        {
            if (indexToTest >= placementUnits.Count)
            {
                // Print the room grid of obstruction values
                //Console.WriteLine(room);
                // All placement units in placementUnits have been tested
                return true;
            }
            else
            {
                PlacementUnit placementUnit = placementUnits[indexToTest];

                // Test if placements in domains are possible
                foreach (Tuple<Vector2D, int> positionAndRotation in placementUnit.positionsAndRotationsDomain)
                {
                    // Place placement unit only if possible and then move on to test next unit
                    if (placementUnit.TryFitAt(positionAndRotation, ref room))
                    {
                        // Move on to the subsequent placement unit, and test if any placements in its domain fit
                        bool couldNextUnitBePlaced = TestPlacementRecursive(ref placementUnits, indexToTest + 1, ref room);

                        if (couldNextUnitBePlaced)
                        {
                            return true;
                        }
                        else
                        {
                            // Move back placementUnit to default placement and unregister placement
                            placementUnit.Unplace(positionAndRotation, ref room);
                        }
                    }
                }

                // No placements fit for this and all subsequent placement units
                return false;
            }
        }
    }
}
