using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

namespace UnityStandardAssets.Vehicles.Ball
{
    public class BallUserControl : MonoBehaviour
    {
        private Ball ball; // Reference to the ball controller.

        private Vector3 move;
        // the world-relative desired move direction, calculated from the camForward and user input.

        private Transform cam; // A reference to the main camera in the scenes transform
        private Vector3 camForward; // The current forward direction of the camera

        private void Awake()
        {
            // Set up the reference.
            ball = GetComponent<Ball>();
            if(ball == null)
            {
                ball = GetComponent<Ball_Single>();
            }

            // get the transform of the main camera
            if (Camera.main != null)
            {
                cam = Camera.main.transform;
            }
            else
            {
                Debug.LogWarning(
                    "Warning: no main camera found. Ball needs a Camera tagged \"MainCamera\", for camera-relative controls.");
                // we use world-relative controls in this case, which may not be what the user wants, but hey, we warned them!
            }
        }


        private void Update()
        {
            if (Cardboard.SDK.Triggered)
            {
                float h = 2;

                if (cam != null)
                {
                    // calculate camera relative direction to move:
                    camForward = Vector3.Scale(cam.forward, new Vector3(1, 0, 1)).normalized;
                    move = (camForward + h * cam.forward).normalized;
                }
                else
                {
                    // we use world-relative directions in the case of no main camera
                    move = (Vector3.forward + h * Vector3.forward).normalized;
                }

                ball.Move(move, false);
            }           
        }
    }
}
