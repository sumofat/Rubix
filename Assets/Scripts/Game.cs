/*
 * The two functions GetSlice and GetSliceForRotation are the main interesting functions
 * here.
 * 
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

#region Types
public struct SliceData
{
    public List<Entity> entities;
    public List<int> ids;
    public int[,] ids_matrix;
    public SliceFormat format;
    public int slice_id;
    public Vector3 rotation_axis;
    public FaceType face_type;
    public Vector3 up;
    public Vector3 right;
    public Vector3 down;
    public Vector3 left;
    public bool Clockwise;
    public SliceType SliceType;
}
#endregion
public static class Game
{
    #region Variables
    public static bool MoveInProgress = false;
    public static bool GameStarted;
    public static float GameTimer = 0.0f;
    public static Transform ObjectHit = null;
    public static float SwipeTolerance = 30;
    public static Vector2 StartMouseP;
    public static bool StartedDrag;
    public static SliceData CurrentSliceData;
    public static bool StartedCameraMove = false;
    public static bool Randomizing = true;
    public static bool GameOver = false;
    public static int RandomMoveCount = 0;
    public static GameUI ReferenceTOGameUI;
    #endregion

    #region Functions
    public static void InitGame()
    {
        //Assumes we will always have the GAME UI Script attached to the main camera
        //in this case its an easy yes.
        ReferenceTOGameUI = GameCamera.CameraComponent.gameObject.GetComponent<GameUI>();
        if (ReferenceTOGameUI == null)
        {
            //This needs to be caught right away if not true
            Debug.Assert(false);
        }
    }
    //This Function will trigger the animation.
    public static void StartMove(List<Entity> slice)
    {

        foreach (var r in slice)
        {
            r.go.transform.parent = Cube.GlobalRotateCubeGO.transform;
        }
        MoveInProgress = true;
    }

    //Used to reset state when new button is used during game play
    public static void ResetState()
    {
        //Vars
        GameTimer = 0.0f;
        Cube.RubixCube.blocks.Clear();
        Cube.RubixCube.dim = 0;

        List<Transform> delete_gos = new List<Transform>();
        foreach (var e in Cube.Entities)
        {
            for (int i = 0; i < e.go.transform.childCount; i++)
            {
                delete_gos.Add(e.go.transform.GetChild(i));
            }
            delete_gos.Add(e.go.transform);
        }

        foreach (var ec in delete_gos)
        {
            GameObject.DestroyImmediate(ec.gameObject);
        }

        Cube.Entities.Clear();
        Cube.GlobalId = 0;
        //public static int[,,] CubeIds;
        //GlobalRotateCubeGO;
        MoveInProgress = false;
        //as far as the game is concerned there are three horizontal slices
        //and 6 vertical ones at any time.
        ObjectHit = null;

        Animation.RotateAngle = 90f;
        Animation.RotateProgress = 0.0f;
        // Update is called once per frame
        SwipeTolerance = 30;
        StartMouseP = Vector2.zero;
        StartedDrag = false;
        {
            if (CurrentSliceData.entities != null)
            {
                CurrentSliceData.entities.Clear();
            }
            if (CurrentSliceData.ids != null)
            {
                CurrentSliceData.ids.Clear();
            }

            CurrentSliceData.face_type = FaceType.none;
            CurrentSliceData.format = SliceFormat.xy;
            CurrentSliceData.rotation_axis = Vector3.zero;
            CurrentSliceData.slice_id = 0;
            CurrentSliceData.Clockwise = true;
        }
        GameOver = false;
        GameCamera.GameOverAngleAmount = 0;
        StartedCameraMove = false;
        RandomMoveCount = 0;
        Randomizing = true;
    }

    public static void DoRandomMove()
    {
        if (MoveInProgress == false)
        {
            Vector3[] vector3s = new Vector3[4];
            vector3s[0] = Vector3.up;
            vector3s[1] = Vector3.down;
            vector3s[2] = Vector3.right;
            vector3s[3] = Vector3.left;
            int RandomIndex = Random.Range(0, 4);
            Vector2 random_swipe_dir = vector3s[RandomIndex];// (MouseP - StartMouseP).normalized;
            Entity random_entity = Cube.Entities[Random.Range(0, Cube.Entities.Count - 1)];
            Transform random_transform = random_entity.go.transform.GetChild(Random.Range(0, random_entity.go.transform.childCount - 1));

            CurrentSliceData = GetSelectedSliceForRotation(random_transform, random_swipe_dir,Cube.CubeDim);
            if (CurrentSliceData.entities != null)
            {
                StartMove(CurrentSliceData.entities);
                RandomMoveCount++;
            }
        }
    }

    //Key function to ensure that the data representation of our cube is 
    //mirrored with visual representation.
    //The parameter here transposed is a slice of sthe cube that has 
    //been rotated and transposed as needed by the rotation of the slice.
    //See GetSlice and GetSliceForRotation.
    public static void FinalizeCubeDataAfterRotation(int[,] transposed)
    {
        int cube_dim = Cube.CubeDim;
        //rewrite the 3d array with the new arrangement of the slice after rotation
        if (CurrentSliceData.format == SliceFormat.zy)
        {
            int slice_id = CurrentSliceData.slice_id;
            for (int z = 0; z < cube_dim; z++)
            {
                for (int y = 0; y < cube_dim; y++)
                {
                    Cube.CubeIds[slice_id, y, z] = transposed[z, y];
                }
            }
        }
        else if (CurrentSliceData.format == SliceFormat.xz)
        {
            int slice_id = CurrentSliceData.slice_id;
            for (int z = 0; z < cube_dim; z++)
            {
                for (int x = 0; x < cube_dim; x++)
                {
                    Cube.CubeIds[x, slice_id, z] = transposed[x, z];
                }
            }
        }
        else if (CurrentSliceData.format == SliceFormat.xy)
        {
            int slice_id = CurrentSliceData.slice_id;
            for (int x = 0; x < cube_dim; x++)
            {
                for (int y = 0; y < cube_dim; y++)
                {
                    Cube.CubeIds[x, y, slice_id] = transposed[x, y];
                }
            }
        }
    }

    //This was quickly added at the end I think it 
    //it doesnt always get the correct result but may not 
    //have time to debug before handing in the assignment.
    public static bool ISCubeSolved(int cube_dim)
    {
        for (int z = 0; z < cube_dim; z++)
        {
            for (int x = 0; x < cube_dim; x++)
            {
                for (int y = 0; y < cube_dim; y++)
                {
                    int id = Cube.CubeIds[x, y, z];
                    int sid = Cube.StartingCubeIds[x, y, z];
                    if (id != sid)
                    {
                        return false;
                    }
                }
            }
        }

        return true;
    }

    //Takes a point and direction and a swipe direction
    //using the point to create a new point in 3d space than converts those two points to 
    //screenspace and than in screen space create a new vector that we can use to 
    //test the dot product against in screen space.
    //The dot product result is returned.
    public static float GetDotForDirection(Vector3 reference_p, Vector3 dir, Vector3 swipe_dir)
    {
        Vector3 ep = Camera.main.WorldToScreenPoint(reference_p + dir);
        Vector3 bp = Camera.main.WorldToScreenPoint(reference_p);
        Vector3 fp = (ep - bp).normalized;
        swipe_dir = swipe_dir.normalized;
        return Vector3.Dot(swipe_dir, fp);
    }

    //Using the Result of the above operation we can use it to get the swipe direction
    public static FaceEdgeTypeDetails GetDirectionOfSwipe(Transform t, Vector3 swipe_dir, Vector3 up, Vector3 down, Vector3 right, Vector3 left)
    {
        float up_dot = GetDotForDirection(t.position, up, swipe_dir);
        float down_dot = GetDotForDirection(t.position, down, swipe_dir);
        float right_dot = GetDotForDirection(t.position, right, swipe_dir);
        float left_dot = GetDotForDirection(t.position, left, swipe_dir);
        FaceEdgeTypeDetails dir = FaceEdgeTypeDetails.none;
        float diff = 0;

        for (int p = 0; p < 4; p++)
        {
            if (p == 0 && up_dot > diff) { diff = up_dot; dir = FaceEdgeTypeDetails.t; }
            else if (p == 1 && down_dot > diff) { diff = down_dot; dir = FaceEdgeTypeDetails.b; }
            else if (p == 2 && right_dot > diff) { diff = right_dot; dir = FaceEdgeTypeDetails.r; }
            else if (p == 3 && left_dot > diff) { diff = left_dot; dir = FaceEdgeTypeDetails.l; }
        }

        return dir;
    }
    
    //Very important function that gets the slice of blocks as entities and other relavent data
    //that we need to perform a rotation and the inverse rotation in the case of an undo.
    //This data is returned in the form of a SliceData result.
    public static SliceData GetSlice(SliceType slice_type, int slice_id, int cube_dim)
    {
        SliceData results = new SliceData();
        results.SliceType = slice_type;
        results.entities = new List<Entity>();
        results.ids = new List<int>();
        results.ids_matrix = new int[cube_dim, cube_dim];
        for (int z = 0; z < cube_dim; z++)
        {
            for (int x = 0; x < cube_dim; x++)
            {
                for (int y = 0; y < cube_dim; y++)
                {
                    if (slice_type == SliceType.x)//x dir
                    {
                        if (x == slice_id && y < cube_dim)
                        {
                            int idx = Cube.CubeIds[x, y, z];
                            Entity e = Cube.Entities[idx];
                            results.entities.Add(e);
                            results.ids.Add(idx);
                            results.ids_matrix[z, y] = idx;
                            results.format = SliceFormat.zy;
                            results.slice_id = slice_id;
                            results.rotation_axis = Vector3.right;
                        }
                    }
                    else if (slice_type == SliceType.z)//z dir
                    {
                        if (z == slice_id)
                        {
                            int idx = Cube.CubeIds[x, y, z];
                            Entity e = Cube.Entities[idx];
                            results.entities.Add(e);
                            results.ids.Add(idx);
                            results.ids_matrix[x, y] = idx;
                            results.format = SliceFormat.xy;
                            results.slice_id = slice_id;
                            results.rotation_axis = -Vector3.forward;
                        }
                    }
                    else if (slice_type == SliceType.y)//y dir
                    {
                        if (y == slice_id)
                        {
                            int idx = Cube.CubeIds[x, y, z];
                            Entity e = Cube.Entities[idx];
                            results.entities.Add(e);
                            results.ids.Add(idx);
                            results.ids_matrix[x, z] = idx;
                            results.format = SliceFormat.xz;
                            results.slice_id = slice_id;
                            results.rotation_axis = Vector3.up;
                        }
                    }
                }
            }
        }
        return results;
    }

    public static SliceData GetSelectedSliceForRotation(Transform t, Vector3 swipe_dir,int cube_dim)
    {
        SliceData results = new SliceData();
        for (int z = 0; z < cube_dim; z++)
        {
            for (int x = 0; x < cube_dim; x++)
            {
                for (int y = 0; y < cube_dim; y++)
                {
                    int id = Cube.CubeIds[x, y, z];
                    GameObject go = Cube.Entities[id].go;
                    if (go.transform == t.gameObject.transform.parent)
                    {
                        //find min max direction
                        SliceType min = SliceType.none;
                        SliceType max = SliceType.none;

                        for (int i = 0; i < t.gameObject.transform.parent.childCount; i++)
                        {
                            //Create a cube that is the parent of all other cubes for allowing rotation fo cube if wished to do so.
                            //otherwise this wouldnt work in the case of the rubiks cube rotating. 
                            Transform child_t = t.gameObject.transform.parent.GetChild(i);
                            if (child_t == t)
                            {
                                Vector3 cp = child_t.position;
                                Vector3 normal = t.gameObject.GetComponent<MeshFilter>().mesh.normals[0];
                                Vector3 world_normal = t.localToWorldMatrix * new Vector4(normal.x, normal.y, normal.z, 0);
                                world_normal = Vector3.Normalize(world_normal);
                                cp = world_normal;
                                //Using Min Max to find the facing direction using the normal in worldspace
                                if (cp.z > cp.x && cp.z > cp.y && cp.x >= -0.0001f && cp.y >= -0.0001f) { max = SliceType.z; }
                                else if (cp.x > cp.y && cp.x > cp.z && cp.y >= -0.0001f && cp.z >= -0.0001f) { max = SliceType.x; }
                                else if (cp.y > cp.x && cp.y > cp.z && cp.x >= -0.0001f && cp.z >= -0.0001f) { max = SliceType.y; }
                                else if (cp.z < cp.x && cp.z < cp.y) { min = SliceType.z; }
                                else if (cp.x < cp.z && cp.x < cp.y) { min = SliceType.x; }
                                else if (cp.y < cp.z && cp.y < cp.x) { min = SliceType.y; }

                                //Get all 4 directions possible in local quad cooridinate space
                                //than convert to world space than to screen space
                                //than dot all of those agains screen space swipe dir
                                //closest to 1 will be chosen as the direction of swipe relative to 
                                //the quad local dir space.
                                //than we find the slice dir corresponding to the swipe dir using the result of previous
                                //operation
                                Vector3 left = (Vector3.left);
                                Vector3 right = (Vector3.right);
                                Vector3 up = (Vector3.up);
                                Vector3 down = (Vector3.down);

                                results.up = up;
                                results.down = down;
                                results.left = left;
                                results.right = right;
                                UndoEntry entry = new UndoEntry();
                                FaceEdgeTypeDetails dir = FaceEdgeTypeDetails.none;
                                //for ever face we figure out the direction of swipe
                                //and use that to determine which slice we need for rotation
                                //and tell the animation if its a clockwise or counter clockwise rotation.
                                if (min == SliceType.z)//front face
                                {
                                    dir = GetDirectionOfSwipe(t, swipe_dir, up, down, left, right);
                                    if (dir == FaceEdgeTypeDetails.t)
                                    {
                                        results = GetSlice(SliceType.x, x, cube_dim);
                                        results.Clockwise = true;
                                    }
                                    else if (dir == FaceEdgeTypeDetails.b)
                                    {
                                        results = GetSlice(SliceType.x, x, cube_dim);
                                        results.Clockwise = false;
                                    }
                                    else if (dir == FaceEdgeTypeDetails.l)
                                    {
                                        results = GetSlice(SliceType.y, y, cube_dim);
                                        results.Clockwise = false;
                                    }
                                    else if (dir == FaceEdgeTypeDetails.r)
                                    {
                                        results = GetSlice(SliceType.y, y, cube_dim);
                                        results.Clockwise = true;
                                    }

                                    //get slice dir x or y
                                    //Debug.Log("FrontFace");
                                    results.face_type = FaceType.front;
                                }
                                else if (min == SliceType.x)//left face
                                {
                                    left = Quaternion.AngleAxis(90, Vector3.up) * left;
                                    right = Quaternion.AngleAxis(90, Vector3.up) * right;

                                    dir = GetDirectionOfSwipe(t, swipe_dir, up, down, left, right);

                                    if (dir == FaceEdgeTypeDetails.t)
                                    {
                                        results = GetSlice(SliceType.z, z, cube_dim);
                                        results.Clockwise = true;
                                    }
                                    else if (dir == FaceEdgeTypeDetails.b)
                                    {
                                        results = GetSlice(SliceType.z, z, cube_dim);
                                        results.Clockwise = false;
                                    }
                                    else if (dir == FaceEdgeTypeDetails.l)
                                    {
                                        results = GetSlice(SliceType.y, y, cube_dim);
                                        results.Clockwise = false;
                                    }
                                    else if (dir == FaceEdgeTypeDetails.r)
                                    {
                                        results = GetSlice(SliceType.y, y, cube_dim);
                                        results.Clockwise = true;
                                    }
                                    results.face_type = FaceType.left;
                                    //Debug.Log("LeftFace");
                                }
                                else if (min == SliceType.y)//bottom face
                                {
                                    up = Quaternion.AngleAxis(90, Vector3.right) * up;
                                    down = Quaternion.AngleAxis(90, Vector3.right) * down;

                                    dir = GetDirectionOfSwipe(t, swipe_dir, up, down, left, right);

                                    if (dir == FaceEdgeTypeDetails.t)
                                    {
                                        results = GetSlice(SliceType.x, x, cube_dim);
                                        results.Clockwise = false;
                                    }
                                    else if (dir == FaceEdgeTypeDetails.b)
                                    {
                                        results = GetSlice(SliceType.x, x, cube_dim);
                                        results.Clockwise = true;
                                    }
                                    else if (dir == FaceEdgeTypeDetails.l)
                                    {
                                        results = GetSlice(SliceType.z, z, cube_dim);
                                        results.Clockwise = false;
                                    }
                                    else if (dir == FaceEdgeTypeDetails.r)
                                    {
                                        results = GetSlice(SliceType.z, z, cube_dim);
                                        results.Clockwise = true;
                                    }

                                    results.face_type = FaceType.bottom;
                                    //Debug.Log("BottomFace");
                                }
                                else if (max == SliceType.x)//right face
                                {
                                    left = Quaternion.AngleAxis(-90, Vector3.up) * left;
                                    right = Quaternion.AngleAxis(-90, Vector3.up) * right;

                                    dir = GetDirectionOfSwipe(t, swipe_dir, up, down, left, right);

                                    if (dir == FaceEdgeTypeDetails.t)
                                    {
                                        results = GetSlice(SliceType.z, z, cube_dim);
                                        results.Clockwise = false;
                                    }
                                    else if (dir == FaceEdgeTypeDetails.b)
                                    {
                                        results = GetSlice(SliceType.z, z, cube_dim);
                                        results.Clockwise = true;
                                    }
                                    else if (dir == FaceEdgeTypeDetails.l)
                                    {
                                        results = GetSlice(SliceType.y, y, cube_dim);
                                        results.Clockwise = false;
                                    }
                                    else if (dir == FaceEdgeTypeDetails.r)
                                    {
                                        results = GetSlice(SliceType.y, y, cube_dim);
                                        results.Clockwise = true;
                                    }
                                    results.face_type = FaceType.right;
                                    //Debug.Log("RightFace");
                                }
                                else if (max == SliceType.y)//top face
                                {
                                    up = Quaternion.AngleAxis(-90, Vector3.right) * up;
                                    down = Quaternion.AngleAxis(-90, Vector3.right) * down;

                                    dir = GetDirectionOfSwipe(t, swipe_dir, up, down, left, right);

                                    if (dir == FaceEdgeTypeDetails.t)
                                    {
                                        results = GetSlice(SliceType.x, x, cube_dim);
                                        results.Clockwise = false;
                                    }
                                    else if (dir == FaceEdgeTypeDetails.b)
                                    {
                                        results = GetSlice(SliceType.x, x, cube_dim);
                                        results.Clockwise = true;
                                    }
                                    else if (dir == FaceEdgeTypeDetails.l)
                                    {
                                        results = GetSlice(SliceType.z, z, cube_dim);
                                        results.Clockwise = true;
                                    }
                                    else if (dir == FaceEdgeTypeDetails.r)
                                    {
                                        results = GetSlice(SliceType.z, z, cube_dim);
                                        results.Clockwise = false;
                                    }
                                    results.face_type = FaceType.top;
                                    //get sice dire z or x
                                    //Debug.Log("TopFace");
                                }
                                else if (max == SliceType.z)//back face
                                {
                                    dir = GetDirectionOfSwipe(t, swipe_dir, up, down, left, right);

                                    if (dir == FaceEdgeTypeDetails.t)
                                    {
                                        results = GetSlice(SliceType.x, x, cube_dim);
                                        results.Clockwise = false;
                                    }
                                    else if (dir == FaceEdgeTypeDetails.b)
                                    {
                                        results = GetSlice(SliceType.x, x, cube_dim);
                                        results.Clockwise = true;
                                    }
                                    else if (dir == FaceEdgeTypeDetails.l)
                                    {
                                        results = GetSlice(SliceType.y, y, cube_dim);
                                        results.Clockwise = true;
                                    }
                                    else if (dir == FaceEdgeTypeDetails.r)
                                    {
                                        results = GetSlice(SliceType.y, y, cube_dim);
                                        results.Clockwise = false;
                                    }
                                    results.face_type = FaceType.back;
                                    // Debug.Log("BackFace");
                                }
                                entry.SliceID = results.slice_id;
                                entry.SliceType = results.SliceType;
                                entry.SwipDir = dir;
                                entry.Clockwise = results.Clockwise;
                                Undo.RecordMove(entry);
                            }
                        }
                    }
                }
            }
        }
        return results;
    }

    public static void UpdateGame(TextMeshProUGUI timer_text)
    {
        //Dont do anything till we have clicked on title screen
        if (GameStarted == false && GameOver == true)
        {
            bool cam_is_done = GameCamera.UpdateCamera(true);
            if (cam_is_done == true)
            {
                ReferenceTOGameUI.ReferenceToMain.GameOverText.gameObject.SetActive(true);
            }
        }
        if (GameStarted == false) { return; };


        GameTimer += Time.deltaTime;
        //TODO(Ray):Garbage Created here fix later.
        timer_text.text = ((int)GameTimer).ToString();
        if (MoveInProgress == true)
        {
            Animation.UpdateRotateSlice();
            if (Animation.RotateProgress >= 1.0f)
            {
                MoveInProgress = false;
                Animation.FinishRotateCube();

                int cube_dim = Cube.CubeDim;

                //rotate data by transposing the swapping rows in clockwise case and than swap columns in counter clockwise case
                //first put the data in a matrix format
                //transpose
                //also creates garbage
                int[,] transposed = MathExtend.Transpose(Cube.CubeDim, CurrentSliceData.ids_matrix);

                // swap columns clockwise
                if (CurrentSliceData.Clockwise)
                {
                    transposed = MathExtend.SwapColumns(Cube.CubeDim, transposed);
                }
                else
                {
                    transposed = MathExtend.SwapRows(Cube.CubeDim, transposed);
                }

                FinalizeCubeDataAfterRotation(transposed);

                //Debug.Log("IS CUBE SOLVED " + Game.ISCubeSolved(Cube.CubeDim));
                if (ISCubeSolved(Cube.CubeDim))
                {
                    StartEndGame();
                }
            }
        }


        if (Randomizing == true)
        {
            DoRandomMove();

            GameCamera.UpdateCamera();

            if (RandomMoveCount > 30)
            {
                Randomizing = false;
            }
        }
        else
        {
            GameInput.CheckUserInput();
        }
    }

    public static void StartEndGame()
    {
        ReferenceTOGameUI.ReferenceToMain.UndoButton.gameObject.SetActive(false);
        GameOver = true;
        GameStarted = false;
    }
    #endregion
}
