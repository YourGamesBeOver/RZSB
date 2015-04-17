# RZSB
A C# API to the Razer Switchblade UI

I decided to make a C# wrapper for the Razer Switchblade UI after seeing how awful the one is that comes with the SDK.
Instead of just forwarding the C++ functions and structs like Razer did, this project is a full-featured API and library.

Some existing/planned features:
 - A truly C#-style interface to the Switchblade UI, no messing with IntPtrs, or using Marshal
 - Have the API handle things for you
    - when the application goes into the background, the API automatically redraws your contents, etc.
    - no need to shut down! The API will handle that for you
 - Pre-made UI elements and frameworks
    - Ready-to-use Dynamic key buttons and button pages
    - Touchpad UI system (RZSB.TouchpadGraphics) featuring:
       - Ready to use panels, labels, and text boxes
       - Planned: Scroll panels, buttons, draggable objects, progress bars, animated objects, swappable look-and-feels
    - Planned: integrated full page solution that merges the dynamic key ui with the touchpad ui
    
    
    
TODO:
  Documentation!

This project is set up to use .NET 4.5, but last I checked, it can run just fine on 2.0, meaning it should work with Unity3D
  
Feel free to use this for personal projects, just give credit.  Also note that this is an unfinished project, use it at your own risk!
