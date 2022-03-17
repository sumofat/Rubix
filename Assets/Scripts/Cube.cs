using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SliceFormat
{
    zy,
    xy,
    xz,
}


public enum SliceType
{
    none,
    x,
    z,
    y,
}
public enum FaceType
{
    none,//unknown
    top,
    bottom,
    front,
    back,
    right,
    left,
}
public struct Entity
{
    public int id;
    public GameObject go;
}

public enum CornerBoxDetails
{
    none,
    bl,
    br,
    tl,
    tr,
    top,
    bottom,
}
public enum FaceEdgeTypeDetails
{
    none,
    bl,
    br,
    tr,
    tl,
    l,
    r,
    t,
    b,
    front,
    back,
}
public enum RubixColors
{
    none,
    green,
    red,
    yellow,
    blue,
    white,
    orange,
}
public enum CubeType
{
    none,
    edge,
    corner,
}
public struct CubeBlock
{
    public EntityID[] entity;
    public RubixColors[] colors;
    public CubeType type;
}
public struct RubixCube
{
    public int dim;
    public List<CubeBlock> blocks;
}

public static class Cube
{
    #region Variables
    public static GameObject GlobalRotateCubeGO;
    public static int[,,] CubeIds;
    //What we check against to see if we have found the solution
    public static int[,,] StartingCubeIds;
    public static int CubeDim = 2;
    public static RubixCube RubixCube;
    public static List<Entity> Entities;
    public static int GlobalId = 0;
    #endregion

    #region CubeGeneration
    //I have commented some of this for explaination but to be honest its not that interesting.
    //Very quick and dirty to get this going.  
    public static void InitCube()
    {
        GlobalRotateCubeGO = new GameObject();

        CubeIds = new int[CubeDim, CubeDim, CubeDim];
        StartingCubeIds = new int[CubeDim, CubeDim, CubeDim];
        Vector3 p = (Vector3.one * CubeDim) * 0.5f;
        p = new Vector3(p.x - 0.5f, p.y - 0.5f, p.z - 0.5f);
        GlobalRotateCubeGO.transform.position = p;
        GameCamera.InitCamera();
        Undo.InitUndo();
        Game.InitGame();
        RubixCube = new RubixCube();
        RubixCube.dim = CubeDim;
        RubixCube.blocks = new List<CubeBlock>();
        Entities = new List<Entity>();

        //make rubix cube
        //Generate the Cube Faces
        //This is done quick and dirty due to time constraints.
        //Splitting the cube into slices
        //z x y and slices...
        //We build the cube to z positive than  to the right in x and up in y
        for (int z = 0; z < CubeDim; z++)
        {
            for (int x = 0; x < CubeDim; x++)
            {
                bool is_edge = false;
                for (int y = 0; y < CubeDim; y++)
                {
                    //Cubeblock reperesents a single block of a larger rubics cube
                    CubeBlock cube = new CubeBlock();
                    cube.entity = new EntityID[3];
                    cube.colors = new RubixColors[3];
                    cube.colors[0] = RubixColors.none;
                    cube.type = CubeType.none;

                    //cube_id represents the master id of the rubic cube
                    int cube_id = (((x * CubeDim) + (CubeDim * CubeDim * z)) + y);

                    FaceEdgeTypeDetails edge_front_back = FaceEdgeTypeDetails.none;
                    FaceEdgeTypeDetails edge_side_type = FaceEdgeTypeDetails.none;
                    //if z is zero or high bounds we know we have the front or back
                    if (z == 0)
                    {
                        edge_front_back = FaceEdgeTypeDetails.front;
                    }
                    else if (z == (CubeDim - 1))
                    {
                        edge_front_back = FaceEdgeTypeDetails.back;
                    }

                    if (edge_front_back != FaceEdgeTypeDetails.none)
                    {
                        //If we are not front or back than we are a middle 
                        //slice in which case we know that the corner edges
                        //are alwasy an edge there and the bounds and zeros of a 
                        //middle slice
                        if ((x == 0 || x == CubeDim - 1) && //first and last rows + middle tiles
                        (y > 0 && y < CubeDim - 1))  // just middle tiles of all other rows
                        {
                            is_edge = true;
                        }
                        if ((x != 0 && x != CubeDim - 1) && (y == 0 || y == CubeDim - 1))  // just middle tiles of all other rows
                        {
                            is_edge = true;
                        }

                        //This will give us the slice left top bottom or right
                        if (x == 0)
                        {
                            edge_side_type = FaceEdgeTypeDetails.l;
                        }
                        if (y == CubeDim - 1)
                        {
                            edge_side_type = FaceEdgeTypeDetails.t;
                        }
                        if (x == CubeDim - 1)
                        {
                            edge_side_type = FaceEdgeTypeDetails.r;
                        }
                        if (y == 0)
                        {
                            edge_side_type = FaceEdgeTypeDetails.b;
                        }

                    }
                    else//in the case we are front or back
                    {
                        if ((x == 0 && y == 0) ||
                            (x == 0 && y == CubeDim - 1) ||
                            (x == CubeDim - 1 && y == 0) ||
                            (x == CubeDim - 1 && y == CubeDim - 1))// just middle tiles of all other rows
                        {
                            is_edge = true;
                        }

                        //same as above we are looking at our x y indexes 
                        //to determine which face and side we are at.
                        if (x == 0)
                        {
                            edge_side_type = FaceEdgeTypeDetails.l;
                        }
                        if (y == CubeDim - 1)
                        {
                            edge_side_type = FaceEdgeTypeDetails.t;
                        }
                        if (x == CubeDim - 1)
                        {
                            edge_side_type = FaceEdgeTypeDetails.r;
                        }
                        if (y == 0)
                        {
                            edge_side_type = FaceEdgeTypeDetails.b;
                        }

                        if (x == 0 && y == 0)
                        {
                            edge_side_type = FaceEdgeTypeDetails.bl;
                        }
                        if (x == 0 && y == CubeDim - 1)
                        {
                            edge_side_type = FaceEdgeTypeDetails.tl;
                        }
                        if (x == CubeDim - 1 && y == 0)
                        {
                            edge_side_type = FaceEdgeTypeDetails.br;
                        }
                        if (x == CubeDim - 1 && y == CubeDim - 1)
                        {
                            edge_side_type = FaceEdgeTypeDetails.tr;
                        }

                    }

                    //IN the case we are not an edge and we at zero or maximum of our slice than we know its a corner which 
                    //is a special three face cube
                    if (((x == 0 && y == 0) || (x == 0 && y == CubeDim - 1) || (x == CubeDim - 1 && y == 0) || (x == CubeDim - 1 && y == CubeDim - 1)) && is_edge == false)
                    {
                        cube.type = CubeType.corner;
                        CornerBoxDetails corner_type = GetCornerType(x, y);
                        CornerBoxDetails corner_side = CornerBoxDetails.none;
                        if (edge_front_back == FaceEdgeTypeDetails.front)
                        {
                            corner_side = CornerBoxDetails.top;
                        }
                        else if (edge_front_back == FaceEdgeTypeDetails.back)
                        {
                            corner_side = CornerBoxDetails.bottom;
                        }
                        CreateCornerPieces(Entities, new Vector3(x, y, z), corner_type, corner_side, cube_id);
                    }
                    else if (is_edge)
                    {
                        cube.type = CubeType.edge;
                        CreateEdgePieces(Entities, new Vector3(x, y, z), edge_side_type, edge_front_back, cube_id);
                    }
                    else
                    {
                        FaceEdgeTypeDetails side = FaceEdgeTypeDetails.none;
                        if (edge_front_back == FaceEdgeTypeDetails.front)
                        {
                            side = FaceEdgeTypeDetails.front;
                        }
                        if (edge_front_back == FaceEdgeTypeDetails.back)
                        {
                            side = FaceEdgeTypeDetails.back;
                        }
                        if (x == 0)
                        {
                            side = FaceEdgeTypeDetails.l;
                        }
                        if (x == CubeDim - 1)
                        {
                            side = FaceEdgeTypeDetails.r;
                        }
                        if (x > 0 && y == 0 && x < CubeDim - 1)
                        {
                            side = FaceEdgeTypeDetails.b;
                        }
                        if (x > 0 && y == CubeDim - 1 && x < CubeDim - 1)
                        {
                            side = FaceEdgeTypeDetails.t;
                        }
                        CreatePiece(Entities, new Vector3(x, y, z), side, cube_id);
                        cube.type = CubeType.none;
                    }
                    is_edge = false;
                    CubeIds[x, y, z] = cube_id;
                    StartingCubeIds[x, y, z] = cube_id;
                }
            }
        }
    }
    public static CornerBoxDetails GetCornerType(int x, int y)
    {
        CornerBoxDetails corner_type = CornerBoxDetails.none;
        if (y == 0 && x == 0)
        {
            corner_type = CornerBoxDetails.bl;
        }
        else if (x == 0 && y == CubeDim - 1)
        {
            corner_type = CornerBoxDetails.tl;
        }
        else if (((CubeDim * CubeDim) - CubeDim) / CubeDim == x && y == 0)
        {
            corner_type = CornerBoxDetails.br;
        }
        else if (x == ((CubeDim * CubeDim) / CubeDim) - 1 && y == (CubeDim - 1))
        {
            corner_type = CornerBoxDetails.tr;
        }
        return corner_type;
    }
    public static void CreateCornerPieces(List<Entity> entities, Vector3 p, CornerBoxDetails corner_type, CornerBoxDetails corner_side, int cube_id)
    {
        GameObject parent_go = new GameObject();
        Entity entity = new Entity();
        EntityID new_id = new EntityID();
        new_id.id = GlobalId++;
        entity.id = cube_id;
        entity.go = parent_go;
        entity.go.transform.position = p;
        entity.go.transform.localScale = new Vector3(1, 1, 1);

        //Based on the corner type of our face we
        //offset the position and rotate it to make it face the proper direction.
        //And set the face color.
        for (int i = 0; i < 3; i++)
        {
            GameObject quad_go = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad_go.transform.localScale = new Vector3(1, 1, 1);

            if (i == 0)
            {
                Quaternion q = Quaternion.identity;
                q = quad_go.transform.rotation;
                float newx = p.x - 0.5f;
                if (corner_type == CornerBoxDetails.bl)
                {
                    newx = p.x - 0.5f;
                    SetFaceColor(quad_go.transform, FaceEdgeTypeDetails.l);
                }
                else if (corner_type == CornerBoxDetails.br)
                {
                    newx = p.x + 0.5f;
                    q = q * Quaternion.AngleAxis(180, Vector3.up);
                    SetFaceColor(quad_go.transform, FaceEdgeTypeDetails.r);
                }
                else if (corner_type == CornerBoxDetails.tr)
                {
                    newx = p.x + 0.5f;
                    q = q * Quaternion.AngleAxis(180, Vector3.up);
                    SetFaceColor(quad_go.transform, FaceEdgeTypeDetails.r);
                }
                else if (corner_type == CornerBoxDetails.tl)
                {
                    SetFaceColor(quad_go.transform, FaceEdgeTypeDetails.l);
                }
                quad_go.transform.position = new Vector3(newx, p.y, p.z);

                q = q * Quaternion.AngleAxis(90, Vector3.up);

                quad_go.transform.rotation = q;
            }
            else if (i == 1)
            {
                Quaternion q = Quaternion.identity;
                q = quad_go.transform.rotation;
                if (corner_type == CornerBoxDetails.bl)
                {
                    p.y -= 0.5f;
                    q = q * Quaternion.AngleAxis(-90, Vector3.right);
                    SetFaceColor(quad_go.transform, FaceEdgeTypeDetails.b);

                }
                else if (corner_type == CornerBoxDetails.tl)
                {
                    p.y += 0.5f;
                    q = q * Quaternion.AngleAxis(90, Vector3.right);
                    SetFaceColor(quad_go.transform, FaceEdgeTypeDetails.t);
                }
                else if (corner_type == CornerBoxDetails.br)
                {
                    p.y -= 0.5f;
                    q = q * Quaternion.AngleAxis(-90, Vector3.right);
                    SetFaceColor(quad_go.transform, FaceEdgeTypeDetails.b);
                }
                else if (corner_type == CornerBoxDetails.tr)
                {
                    p.y += 0.5f;
                    q = q * Quaternion.AngleAxis(90, Vector3.right);
                    SetFaceColor(quad_go.transform, FaceEdgeTypeDetails.t);
                }

                quad_go.transform.position = p;//new Vector3(x + 0.1f, y + 0.1f, z + 0.1f);
                quad_go.transform.rotation = q;
            }
            else if (i == 2)
            {
                if (corner_type == CornerBoxDetails.bl)
                {
                    p.z -= 0.5f;
                    p.y += 0.5f;
                    SetFaceColor(quad_go.transform, FaceEdgeTypeDetails.front);
                }
                else if (corner_type == CornerBoxDetails.tl)
                {
                    p.z -= 0.5f;
                    p.y -= 0.5f;
                    SetFaceColor(quad_go.transform, FaceEdgeTypeDetails.front);
                }
                else if (corner_type == CornerBoxDetails.br)
                {
                    p.z -= 0.5f;
                    p.y += 0.5f;
                    SetFaceColor(quad_go.transform, FaceEdgeTypeDetails.front);
                }
                else if (corner_type == CornerBoxDetails.tr)
                {
                    p.z -= 0.5f;
                    p.y -= 0.5f;
                    SetFaceColor(quad_go.transform, FaceEdgeTypeDetails.front);
                }

                if (corner_side == CornerBoxDetails.bottom)
                {
                    Quaternion q = Quaternion.identity;
                    q = quad_go.transform.rotation;
                    q = q * Quaternion.AngleAxis(180, Vector3.up);
                    quad_go.transform.rotation = q;

                    p.z += 1f;
                    SetFaceColor(quad_go.transform, FaceEdgeTypeDetails.back);
                }
                quad_go.transform.position = p;
            }

            quad_go.name = "corner_quad_" + corner_type + "_" + corner_side + "_" + i;
            quad_go.transform.parent = parent_go.transform;
        }
        parent_go.name = cube_id.ToString();
        entities.Add(entity);
    }
    public static void CreateEdgePieces(List<Entity> entities, Vector3 p, FaceEdgeTypeDetails edge, FaceEdgeTypeDetails tb, int cube_id)
    {
        GameObject parent_go = new GameObject();
        parent_go.name = cube_id.ToString();
        EntityID new_id = new EntityID();
        new_id.id = cube_id;
        Entity entity = new Entity();

        entity.id = new_id.id;
        entity.go = parent_go;
        entity.go.transform.position = p;

        for (int i = 0; i < 2; i++)
        {
            GameObject quad_go = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad_go.transform.localScale = new Vector3(1, 1, 1);
            quad_go.name = "edge_quad_" + edge + "_" + tb;
            Vector3 new_p = p;
            if (tb == FaceEdgeTypeDetails.front)
            {
                if (edge == FaceEdgeTypeDetails.l)
                {
                    if (i == 0)
                    {
                        new_p.z -= 0.5f;
                        SetFaceColor(quad_go.transform, FaceEdgeTypeDetails.front);
                    }
                    else if (i == 1)
                    {
                        new_p.x -= 0.5f;
                        Quaternion q = Quaternion.identity;
                        q = entity.go.transform.rotation;
                        q = q * Quaternion.AngleAxis(90, Vector3.up);
                        quad_go.transform.rotation = q;
                        SetFaceColor(quad_go.transform, edge);
                    }

                }
                else if (edge == FaceEdgeTypeDetails.r)
                {
                    if (i == 0)
                    {
                        new_p.z -= 0.5f;
                        SetFaceColor(quad_go.transform, FaceEdgeTypeDetails.front);
                    }
                    else if (i == 1)
                    {
                        new_p.x += 0.5f;
                        Quaternion q = Quaternion.identity;
                        q = entity.go.transform.rotation;
                        q = q * Quaternion.AngleAxis(-90, Vector3.up);
                        quad_go.transform.rotation = q;
                        SetFaceColor(quad_go.transform, edge);
                    }
                }
                else if (edge == FaceEdgeTypeDetails.t)
                {
                    if (i == 0)
                    {
                        new_p.z -= 0.5f;
                        SetFaceColor(quad_go.transform, FaceEdgeTypeDetails.front);
                    }
                    else if (i == 1)
                    {
                        new_p.y += 0.5f;
                        Quaternion q = Quaternion.identity;
                        q = entity.go.transform.rotation;
                        q = q * Quaternion.AngleAxis(90, Vector3.right);
                        quad_go.transform.rotation = q;
                        SetFaceColor(quad_go.transform, edge);
                    }
                }
                else if (edge == FaceEdgeTypeDetails.b)
                {
                    if (i == 0)
                    {
                        new_p.z -= 0.5f;
                        SetFaceColor(quad_go.transform, FaceEdgeTypeDetails.front);
                    }
                    else if (i == 1)
                    {
                        new_p.y -= 0.5f;
                        Quaternion q = Quaternion.identity;
                        q = entity.go.transform.rotation;
                        q = q * Quaternion.AngleAxis(-90, Vector3.right);
                        quad_go.transform.rotation = q;
                        SetFaceColor(quad_go.transform, edge);
                    }
                }

            }
            else if (tb == FaceEdgeTypeDetails.back)
            {
                if (edge == FaceEdgeTypeDetails.l)
                {
                    if (i == 0)
                    {
                        new_p.z += 0.5f;
                        Quaternion q = Quaternion.identity;
                        q = entity.go.transform.rotation;
                        q = q * Quaternion.AngleAxis(180, Vector3.up);
                        quad_go.transform.rotation = q;
                        SetFaceColor(quad_go.transform, FaceEdgeTypeDetails.back);
                    }
                    else if (i == 1)
                    {
                        new_p.x -= 0.5f;
                        Quaternion q = Quaternion.identity;
                        q = entity.go.transform.rotation;
                        q = q * Quaternion.AngleAxis(90, Vector3.up);
                        quad_go.transform.rotation = q;
                        SetFaceColor(quad_go.transform, edge);
                    }

                }
                else if (edge == FaceEdgeTypeDetails.r)
                {
                    if (i == 0)
                    {
                        new_p.z += 0.5f;
                        Quaternion q = Quaternion.identity;
                        q = entity.go.transform.rotation;
                        q = q * Quaternion.AngleAxis(180, Vector3.up);
                        quad_go.transform.rotation = q;
                        SetFaceColor(quad_go.transform, FaceEdgeTypeDetails.back);
                    }
                    else if (i == 1)
                    {
                        new_p.x += 0.5f;
                        Quaternion q = Quaternion.identity;
                        q = entity.go.transform.rotation;
                        q = q * Quaternion.AngleAxis(-90, Vector3.up);
                        quad_go.transform.rotation = q;
                        SetFaceColor(quad_go.transform, edge);
                    }
                }
                else if (edge == FaceEdgeTypeDetails.t)
                {
                    if (i == 0)
                    {
                        new_p.z += 0.5f;
                        Quaternion q = Quaternion.identity;
                        q = entity.go.transform.rotation;
                        q = q * Quaternion.AngleAxis(180, Vector3.up);
                        quad_go.transform.rotation = q;
                        SetFaceColor(quad_go.transform, FaceEdgeTypeDetails.back);
                    }
                    else if (i == 1)
                    {
                        new_p.y += 0.5f;
                        Quaternion q = Quaternion.identity;
                        q = entity.go.transform.rotation;
                        q = q * Quaternion.AngleAxis(90, Vector3.right);
                        quad_go.transform.rotation = q;
                        SetFaceColor(quad_go.transform, edge);
                    }
                }
                else if (edge == FaceEdgeTypeDetails.b)
                {
                    if (i == 0)
                    {
                        new_p.z += 0.5f;
                        Quaternion q = Quaternion.identity;
                        q = entity.go.transform.rotation;
                        q = q * Quaternion.AngleAxis(180, Vector3.up);
                        quad_go.transform.rotation = q;
                        SetFaceColor(quad_go.transform, FaceEdgeTypeDetails.back);
                    }
                    else if (i == 1)
                    {
                        new_p.y -= 0.5f;
                        Quaternion q = Quaternion.identity;
                        q = entity.go.transform.rotation;
                        q = q * Quaternion.AngleAxis(-90, Vector3.right);
                        quad_go.transform.rotation = q;
                        SetFaceColor(quad_go.transform, edge);
                    }
                }
            }
            else if (tb == FaceEdgeTypeDetails.none)
            {
                if (edge == FaceEdgeTypeDetails.bl)
                {
                    Quaternion q = Quaternion.identity;
                    q = entity.go.transform.rotation;
                    if (i == 0)
                    {
                        new_p.y -= 0.5f;
                        q = q * Quaternion.AngleAxis(-90, Vector3.right);
                        SetFaceColor(quad_go.transform, FaceEdgeTypeDetails.b);
                    }
                    else if (i == 1)
                    {
                        new_p.x -= 0.5f;
                        q = q * Quaternion.AngleAxis(90, Vector3.up);
                        SetFaceColor(quad_go.transform, FaceEdgeTypeDetails.l);
                    }
                    quad_go.transform.rotation = q;

                }
                else if (edge == FaceEdgeTypeDetails.tl)
                {
                    Quaternion q = Quaternion.identity;
                    q = entity.go.transform.rotation;
                    if (i == 0)
                    {
                        new_p.y += 0.5f;
                        q = q * Quaternion.AngleAxis(90, Vector3.right);
                        SetFaceColor(quad_go.transform, FaceEdgeTypeDetails.t);
                    }
                    else if (i == 1)
                    {
                        new_p.x -= 0.5f;
                        q = q * Quaternion.AngleAxis(90, Vector3.up);
                        SetFaceColor(quad_go.transform, FaceEdgeTypeDetails.l);
                    }
                    quad_go.transform.rotation = q;
                }
                else if (edge == FaceEdgeTypeDetails.tr)
                {
                    Quaternion q = Quaternion.identity;
                    q = entity.go.transform.rotation;
                    if (i == 0)
                    {
                        new_p.y += 0.5f;
                        q = q * Quaternion.AngleAxis(90, Vector3.right);
                        SetFaceColor(quad_go.transform, FaceEdgeTypeDetails.t);
                    }
                    else if (i == 1)
                    {
                        new_p.x += 0.5f;
                        q = q * Quaternion.AngleAxis(-90, Vector3.up);
                        SetFaceColor(quad_go.transform, FaceEdgeTypeDetails.r);
                    }
                    quad_go.transform.rotation = q;
                }
                else if (edge == FaceEdgeTypeDetails.br)
                {
                    Quaternion q = Quaternion.identity;
                    q = entity.go.transform.rotation;
                    if (i == 0)
                    {
                        new_p.y -= 0.5f;
                        q = q * Quaternion.AngleAxis(-90, Vector3.right);
                        SetFaceColor(quad_go.transform, FaceEdgeTypeDetails.b);
                    }
                    else if (i == 1)
                    {
                        new_p.x += 0.5f;
                        q = q * Quaternion.AngleAxis(-90, Vector3.up);
                        SetFaceColor(quad_go.transform, FaceEdgeTypeDetails.r);
                    }
                    quad_go.transform.rotation = q;
                }
            }
            quad_go.transform.position = new_p;
            quad_go.transform.parent = parent_go.transform;
        }
        entities.Add(entity);
    }
    public static void SetFaceColor(Transform t, FaceEdgeTypeDetails details)
    {
        if (details == FaceEdgeTypeDetails.front)
        {
            t.GetComponent<Renderer>().material.SetColor("_Color", Color.white);

        }
        else if (details == FaceEdgeTypeDetails.back)
        {
            t.GetComponent<Renderer>().material.SetColor("_Color", Color.blue);
        }
        else if (details == FaceEdgeTypeDetails.t)
        {
            t.GetComponent<Renderer>().material.SetColor("_Color", Color.green);
        }
        else if (details == FaceEdgeTypeDetails.b)
        {
            t.GetComponent<Renderer>().material.SetColor("_Color", Color.red);
        }
        else if (details == FaceEdgeTypeDetails.r)
        {
            t.GetComponent<Renderer>().material.SetColor("_Color", Color.cyan);
        }
        else if (details == FaceEdgeTypeDetails.l)
        {
            t.GetComponent<Renderer>().material.SetColor("_Color", Color.yellow);
        }
    }
    public static void CreatePiece(List<Entity> entities, Vector3 p, FaceEdgeTypeDetails side, int cube_id)
    {
        GameObject parent_go = new GameObject();
        parent_go.name = cube_id.ToString();
        EntityID new_id = new EntityID();
        new_id.id = cube_id;
        Entity entity = new Entity();
        entity.go = parent_go;// GameObject.CreatePrimitive(PrimitiveType.Quad);
        entity.id = new_id.id;
        entity.go.transform.position = p;
        {
            GameObject quad_go = GameObject.CreatePrimitive(PrimitiveType.Quad);
            Vector3 new_p = p;
            Quaternion q = Quaternion.identity;
            q = entity.go.transform.rotation;
            if (side == FaceEdgeTypeDetails.front)
            {
                new_p.z -= 0.5f;
                SetFaceColor(quad_go.transform, side);
            }
            if (side == FaceEdgeTypeDetails.back)
            {
                new_p.z += 0.5f;
                q = q * Quaternion.AngleAxis(180, Vector3.up);
                SetFaceColor(quad_go.transform, side);
            }
            if (side == FaceEdgeTypeDetails.l)
            {
                new_p.x -= 0.5f;
                q = q * Quaternion.AngleAxis(90, Vector3.up);
                SetFaceColor(quad_go.transform, side);
            }
            if (side == FaceEdgeTypeDetails.r)
            {
                new_p.x += 0.5f;
                q = q * Quaternion.AngleAxis(-90, Vector3.up);
                SetFaceColor(quad_go.transform, side);
            }
            if (side == FaceEdgeTypeDetails.t)
            {
                new_p.y += 0.5f;
                q = q * Quaternion.AngleAxis(90, Vector3.right);
                SetFaceColor(quad_go.transform, side);
            }
            if (side == FaceEdgeTypeDetails.b)
            {
                new_p.y -= 0.5f;
                q = q * Quaternion.AngleAxis(-90, Vector3.right);
                SetFaceColor(quad_go.transform, side);
            }
            quad_go.name = "piece_quad_" + side;
            quad_go.transform.position = p;// new Vector3(x + 0.1f, y + 0.1f, z + 0.1f);

            quad_go.transform.position = new_p;
            quad_go.transform.rotation = q;
            quad_go.transform.parent = parent_go.transform;

        }
        entities.Add(entity);
    }

    #endregion
}
