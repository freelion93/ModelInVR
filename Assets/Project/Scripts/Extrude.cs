using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Extrude : MonoBehaviour
{
    static int[] control_idices = new int[4];
    static Vector3[] vertices = new Vector3[12];
    static Vector3[] newVertices = new Vector3[8];

    public static void MakeExtrusion(GameObject snowball, RaycastHit hit)
    {
        CatmullClark cc_subdiv = snowball.GetComponent<CatmullClark>();
        if (cc_subdiv == null) { return; }
        ParametricLocation loc = cc_subdiv.param_location(hit.triangleIndex, hit.barycentricCoordinate);
        // the face-id for the control mesh is now in `loc.face_index`

        // the vertex indices of that face are then
        for (int i = 0; i < 4; i++)
        {
            control_idices[i] = cc_subdiv.orig_control_mesh_indices[loc.face_index * 4 + i];
            Debug.Log(control_idices[i]);
        }

        int[] bottom = { 0, 1, 2, 3, };
        int[] top = { 7, 6, 5, 4, };
        int[] front = { 0, 4, 5, 1, };
        int[] right = { 1, 5, 6, 2, };
        int[] left = { 2, 6, 7, 3, };
        int[] back = { 3, 7, 4, 0, };

        for (int i = 0; i < 8; i++)
        {
            vertices[i] = cc_subdiv.cur_control_mesh_vertices[i];
        }

        string CurrFace = null;
        int[] var = { 0, 0, 0, 0 };
        float coef = 1f;

        if (control_idices[0] == 0 && control_idices[1] == 1)
        {
            CurrFace = "Bottom";
            vertices[8] = vertices[0] + new Vector3(0, -1f * coef, 0);
            vertices[9] = vertices[1] + new Vector3(0, -1f * coef, 0);
            vertices[10] = vertices[2] + new Vector3(0, -1f * coef, 0);
            vertices[11] = vertices[3] + new Vector3(0, -1f * coef, 0);
            var = bottom;
        }
        if (control_idices[0] == 7)
        {
            CurrFace = "Top";
            vertices[8] = vertices[7] + new Vector3(0, 1f * coef, 0);
            vertices[9] = vertices[6] + new Vector3(0, 1f * coef, 0);
            vertices[10] = vertices[5] + new Vector3(0, 1f * coef, 0);
            vertices[11] = vertices[4] + new Vector3(0, 1f * coef, 0);
            var = top;
        }
        if (control_idices[0] == 0 && control_idices[1] == 4)
        {
            CurrFace = "Front";
            vertices[8] = vertices[0] + new Vector3(0, 0, -1f * coef);
            vertices[9] = vertices[4] + new Vector3(0, 0, -1f * coef);
            vertices[10] = vertices[5] + new Vector3(0, 0, -1f * coef);
            vertices[11] = vertices[1] + new Vector3(0, 0, -1f * coef);
            var = front;
        }
        if (control_idices[0] == 1)
        {
            CurrFace = "Right";
            vertices[8] = vertices[1] + new Vector3(1.5f * coef, 0, 0);
            vertices[9] = vertices[5] + new Vector3(1.5f * coef, 0, 0);
            vertices[10] = vertices[6] + new Vector3(1.5f * coef, 0, 0);
            vertices[11] = vertices[2] + new Vector3(1.5f * coef, 0, 0);
            var = right;
        }
        if (control_idices[0] == 3)
        {
            CurrFace = "Left";
            vertices[8] = vertices[3] + new Vector3(-1.5f * coef, 0, 0);
            vertices[9] = vertices[7] + new Vector3(-1.5f * coef, 0, 0);
            vertices[10] = vertices[4] + new Vector3(-1.5f * coef, 0, 0);
            vertices[11] = vertices[0] + new Vector3(-1.5f * coef, 0, 0);
            var = left;
        }
        if (control_idices[0] == 2)
        {
            CurrFace = "Back";
            vertices[8] = vertices[3] + new Vector3(0, 0, 1f * coef);
            vertices[9] = vertices[2] + new Vector3(0, 0, 1f * coef);
            vertices[10] = vertices[6] + new Vector3(0, 0, 1f * coef);
            vertices[11] = vertices[7] + new Vector3(0, 0, 1f * coef);
            var = back;
        }
        Debug.Log(CurrFace);

        int[] Indices =
         {
                0, 1, 2, 3,
                7, 6, 5, 4,
                0, 4, 5, 1,
                1, 5, 6, 2,
                2, 6, 7, 3,
                3, 7, 4, 0,
                8, 9, 10, 11,
                8, 11, var[3], var[0],
                var[0], var[1], 9, 8,
                11, 10, var[2], var[3],
                10, 9, var[1], var[2],
          };

        if (GameObject.FindWithTag("Manipulator"))
        {
            Destroy(GameObject.FindGameObjectWithTag("Manipulator"));
        }
        else
        {
            CreateManupulator(snowball);
        }

        cc_subdiv.cur_control_mesh_vertices = vertices;

        cc_subdiv.cur_control_mesh_indices = Indices;
        snowball.transform.parent.GetComponent<MeshFilter>().sharedMesh.RecalculateNormals();
    }

    public static void CreateManupulator(GameObject snowball)
    {
        Vector3 centerV = (vertices[8] + vertices[9] + vertices[10] + vertices[11]) / 4 + snowball.transform.position;
        Debug.Log(centerV);
        GameObject theV = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        theV.transform.position = centerV;
        theV.transform.localScale = new Vector3(0.1F, 0.1F, 0.1F) * CreateSubDivObject.Size;
        theV.GetComponent<Renderer>().material.color = Color.yellow;
        theV.tag = "Manipulator";
    }

    public static void Manipulate()
    {

    }

}
