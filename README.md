# Fitting Placer
This is a C# 6.0 solution for asynchronically and automatically placing furniture or interior objects in a rectangular room according to simple principles. Use it for procedural generation of furniture layouts. It works by registering furniture sides (and wall sides) and connecting specified related sides at specified distances from each other. It then places all furniture at positions inside the room while checking for overlaps and also respecting specified clearance areas around furniture (and also for windows and doors). 

## Trying it out
Open the solution in your IDE and build it from the FittingPlacerRunApplication.cs. Make sure to place the XML file with specified furniture model semantics FittingDatabase.xml in your target run folder so the run-application can find it. It should now run a test arrangement generation process and print the result. By studying the input the run-application gives the algorithm and how it processes the output, you should be able to integrate it to your application for async generation of rectangular furniture arrangements. 

Note how the furniture models to-be-placed must match the models named in the FittingDatabase.xml file. That file can be edited to match the interior object models you are using in your application, as long a you follow the same structure. 

## Help files
While the code is heavily commented, there is no extensive documentation included. See the academic paper Ringqvist (2018) for algorithm explanations. An incomplete overview class structure is included in the UML schema in "HelpFiles/Class UML Schematic - incomplete overview.png". Additionally an overview of the algorithm is shown in "HelpFiles/Algorithm flow chart simplified.png". 

## License
This project is dual-licensed under the both MIT License (see the LICENSE.txt file for details) and Creative Commons 0 public domain license (see the LICENSE-Alternative.txt file for details). Use either licence. 

