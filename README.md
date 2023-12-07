# unity-spline-extrusion
A simple script that extrudes a mesh along a Spline in Unity

## How it works
For a deeper explanation of how this script works, see [my Medium Article](https://medium.com/@lkuppers11/how-to-extrude-a-custom-mesh-along-splines-in-unity-833a97440c1b).

## How to use this script
You can simply download the two `.cs` files into your Unity Project.

The included scripts use Unity's new `Splines` package (`UnityEngine.Splines`).

To extrude a mesh along a spline, follow these steps:
1. Create a template mesh that the extruded mesh will be based on. A template mesh must be a 2D shape extruded along one axis (only tested using Z axis!). [More info](https://medium.com/@lkuppers11/how-to-extrude-a-custom-mesh-along-splines-in-unity-833a97440c1b#dc0c).
2. Attach the `SplineMeshExtrude` script to a gameobject that also contains a Spline.
3. Set your template mesh under the `extrusion template mesh` field.
4. Select the extrusion axis of your template mesh under the `extrusion axis` field. (only tested using Z axis)
5. If you want the mesh to have smooth shading, check the `smooth faces` box. For flat shading, uncheck the box.
6. If you want the mesh to only allow rotation about the world up (Y) axis, check the `use world up` box. To allow 'twisting' around the spline, uncheck the box.

Enjoy!
