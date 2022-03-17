using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public static class GameCamera
{
    public static Transform GameCam;
    public static Camera CameraComponent;
    public static float CameraAngle;
    public static Vector2 pitch_yaw_degrees;
    public static Vector2 MouseDelta;
    public static Vector3 PreviousMousePosition;
    public static bool FirstTouch = false;
    public static float CameraDistance = 10;
    public static float GameOverAngleAmount = 0.0f;

    public static void InitCamera()
    {
        GameObject go = new GameObject();
        //making a cam a child so that w can have postive z be looking through the cam
        go.name = "CamParent";
        //init
        Transform cam = Camera.main.gameObject.transform;
        cam.SetParent(go.transform);
        cam.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        //cam.Rotate(new Vector3(0, 180, 0), Space.Self);
        cam = go.transform;
        cam.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        cam.position = Cube.GlobalRotateCubeGO.transform.position;
        Quaternion cam_dir = cam.rotation;
        Vector3 cam_p = cam.position;
        cam.position = new Vector3(cam_p.x, cam_p.y, cam_p.z - CameraDistance);
        cam.rotation.SetLookRotation(Cube.GlobalRotateCubeGO.transform.position - cam.position, Vector3.up);
        GameCam = cam.transform;
        FirstTouch = false;
        CameraComponent = Camera.main.GetComponent<Camera>();
    }

    public static bool UpdateCamera(bool is_game_over = false)
    {
        Quaternion q = GameCam.rotation;
        q *= Quaternion.AngleAxis(CameraAngle, Vector3.up);
        GameCam.rotation = q;

        Transform target = Cube.GlobalRotateCubeGO.transform;
        Vector3 mouse_position = Input.mousePosition;
        if (is_game_over == true)
        {
            
            mouse_position.x = PreviousMousePosition.x + 2.0f;
        }
        if (Input.GetMouseButton(0) || is_game_over == true)
        {
            if (FirstTouch == false)
            {
                PreviousMousePosition = Input.mousePosition;
            }
            Vector3 mouse_delta = (PreviousMousePosition - mouse_position);
            pitch_yaw_degrees.x = mouse_delta.x * 0.5f;
            pitch_yaw_degrees.y = mouse_delta.y * 0.5f;
            Quaternion pitch = Quaternion.AngleAxis(pitch_yaw_degrees.y, Vector3.right);
            if (is_game_over == true)
            {
                GameOverAngleAmount += pitch_yaw_degrees.x;
                if (GameOverAngleAmount > (360 * 2) || GameOverAngleAmount < (-360 * 2))
                {
                    return true;
                }
            }
            Quaternion yaw = Quaternion.AngleAxis(pitch_yaw_degrees.x * -1, Vector3.up);
            Quaternion turn_qt = GameCam.rotation * (pitch * yaw);
            Vector3 quat_forward = turn_qt * -Vector3.forward;
            float radius = CameraDistance;
            GameCam.transform.SetPositionAndRotation((target.position + (quat_forward * radius)), turn_qt);
            pitch_yaw_degrees = Vector2.zero;
            FirstTouch = true;
        }

        PreviousMousePosition = mouse_position;
        return false;
    }
}
