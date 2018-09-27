using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Extrude : MonoBehaviour
{
    static int[] control_idices = new int[4];
    static Vector3[] vertices = new Vector3[8];
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

        //the set of vertices of the object are
        //for (int i = 0; i < vertices.Length; i++)
        //{
        //    vertices[i] = cc_subdiv.cur_control_mesh_vertices[i];
        //}

        //depending on what face indices are we determing the vertices
        //for (int i = 0; i < control_idices.Length; i++)
        //{
        //    newVertices[i] = vertices[control_idices[i]];
        //}

        int[] bottom = { 0, 1, 2, 3, };
        int[] top = { 7, 6, 5, 4, };
        int[] front = { 0, 4, 5, 1, };
        int[] right = { 1, 5, 6, 2, };
        int[] left = { 2, 6, 7, 3, };
        int[] back = { 3, 7, 4, 0, };

        if (control_idices == bottom)
        {
            Debug.Log("Bottom");
        }
        if (control_idices == top)
        {
            Debug.Log("Top");
        }
        if (control_idices == front)
        {
            Debug.Log("Front");
        }
        if (control_idices == right)
        {
            Debug.Log("Right");
        }
        if (control_idices == left)
        {
            Debug.Log("Left");
        }
        if (control_idices == back)
        {
            Debug.Log("Back");
        }
    }


}
