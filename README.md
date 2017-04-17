## Camera Movement and Collision for Unity 5!
This repository contains a simple script for player controlled movement, and a third person camera script which won't clip through walls.

## Usage:
To use the Camera Movement script, apply it to your camera and set the target variable in the editor to whatever you want the camera to look at. The camera does NOT need to be a child of the player. All data members in the script have tooltips that can be viewed in the Unity Editor, and the entire script is commented,explaining what is happening and how. Camera is controlled using the mouse to orbit around the target. To disable Camera Collisions, comment out the method call for CameraCollision in the LateUpdate() method.

### Player Movement:
A script is also included to handle player movement relative to the camera. Apply this script to an object to gain control of it using the Input Axes.
