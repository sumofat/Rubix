using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Animation
{
    public static float RotateAngle = 90f;
    public static float RotateProgress = 0.0f;

    public static void UpdateRotateSlice()
    {
        //check for input from user (mouseonly for now)
        //update game timer UI
        Quaternion q = Cube.GlobalRotateCubeGO.transform.rotation;
        if (Game.CurrentSliceData.Clockwise == false)
        {
            RotateAngle = -90;
        }
        else
        {
            RotateAngle = 90;
        }
        Quaternion new_q = Quaternion.AngleAxis(RotateAngle, Game.CurrentSliceData.rotation_axis);
        Quaternion final_q = Quaternion.Slerp(q, new_q, RotateProgress);
        RotateProgress += Time.deltaTime * 5.5f;
        Cube.GlobalRotateCubeGO.transform.rotation = final_q;
        
    }

    public static void FinishRotateCube()
    {
        Quaternion q = Cube.GlobalRotateCubeGO.transform.rotation;
        Quaternion new_q = Quaternion.AngleAxis(RotateAngle, Game.CurrentSliceData.rotation_axis);
        RotateProgress = 0.0f;
        Quaternion final_q = Quaternion.Slerp(q, new_q, 1.0f);
        Cube.GlobalRotateCubeGO.transform.rotation = final_q;
        List<Transform> unparent_t = new List<Transform>();
        for (int i = 0; i < Cube.GlobalRotateCubeGO.transform.childCount; i++)
        {
            Transform child_t = Cube.GlobalRotateCubeGO.transform.GetChild(i).transform;
            unparent_t.Add(child_t);
        }
        foreach (var trans in unparent_t)
        {
            trans.SetParent(null);
        }

        Quaternion reset_q = Quaternion.AngleAxis(0, Game.CurrentSliceData.rotation_axis);
        Cube.GlobalRotateCubeGO.transform.rotation = reset_q;
    }

    
}
