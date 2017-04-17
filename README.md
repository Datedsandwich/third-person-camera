## Camera Movement and Collision for Unity 5!
This repository contains a simple script for player controlled movement, and a third person camera script which won't clip through walls.

## Usage:
To use the Camera Movement script, apply it to your camera and set the target variable in the editor to whatever you want the camera to look at. The camera does NOT need to be a child of the player. All data members in the script have tooltips that can be viewed in the Unity Editor, and the entire script is commented,explaining what is happening and how. Camera is controlled using the mouse to orbit around the target. To disable Camera Collisions, comment out the method call for CameraCollision in the LateUpdate() method.

## Variable Documentation:
target :- What the camera will look at, and orbit around.
distance :- The current distance between the camera and it's target.
xSpeed :- Horizontal Speed of the camera.
ySpeed :- Vertical Speed of the camera.

yMinLimit :- Minimum angle the camera can take on the y axis.
yMaxLimit :- Maximum angle the camera can take on the y axis.
xMinLimit :- Minimum angle the camera can take on the x axis.
xMaxLimit :- Maximum angle the camera can take on the x axis.

distanceMin :- Minimum distance the camera can be from it's target. Very small numbers can lead to camera clipping into target.
distanceMax :- Maximum distance the camera can be from it's target. Without any editing, camera will always attempt to zoom out to this value.

thinRadius :- Radius of the thin SphereCast, used to detect camera collisions.
thickRadius :- Radius of the thick SphereCast, used to detect camera collisions.

layerMask :- The layer that will trigger collisions.

### PLAYER MOVEMENT:
A script is also included to handle player movement relative to the camera. Apply this script to an object to gain control of it using the Input Axes.