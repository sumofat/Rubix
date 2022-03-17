using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameInput
{
    public static void CheckUserInput()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Input.GetMouseButton(0) && Game.MoveInProgress == false)
        {
            if (Physics.Raycast(ray, out RaycastHit hit) && Game.StartedCameraMove == false)
            {
                //Figure out which face we hit so we can figure out which directions are possible to move in based on user input.
                if (Game.StartedDrag == false)
                {
                    Game.StartMouseP = Input.mousePosition;
                    Game.ObjectHit = hit.transform;
                }

                Game.StartedDrag = true;
            }
            else if (Game.StartedDrag == false)
            {
                Game.StartedCameraMove = true;
                GameCamera.UpdateCamera();
            }
        }
        else if (Game.StartedDrag == true && Game.Randomizing == false)
        {
            //Debug.Log(objectHit.transform.parent.name);
            // Do something with the object that was hit by the raycast.
            Vector2 mouse_p = Input.mousePosition;
            float mouse_delta = Vector2.Distance(Game.StartMouseP, mouse_p);
            if (mouse_delta > Game.SwipeTolerance && Game.ObjectHit != null)
            {
                Vector2 swipe_dir = (mouse_p - Game.StartMouseP).normalized;
                Game.CurrentSliceData = Game.GetSelectedSliceForRotation(Game.ObjectHit, swipe_dir,Cube.CubeDim);
                if (Game.CurrentSliceData.entities != null)
                {
                    Game.StartMove(Game.CurrentSliceData.entities);
                }
                Game.StartedDrag = false;
            }
        }
        else if (Game.StartedCameraMove == true)
        {
            Game.StartedCameraMove = false;
            GameCamera.FirstTouch = false;
        }
    }
}
