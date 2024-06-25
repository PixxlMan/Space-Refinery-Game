// This file is included in Space Refinery Game so that the global using will continue to work.

#if DEBUG
#define IncludeUnits
#endif

#if IncludeUnits
global using Space_Refinery_Engine.Units;
#else
global using MassUnit = Space_Refinery_Game.DecimalNumber;
global using UnitsMath = Space_Refinery_Game.DecimalNumber;
#endif