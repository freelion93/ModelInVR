using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VertMove : MonoBehaviour
{
    Vector3 ScreenPoint;
    Vector3 TheOffset;
    List<Vector3> vertexBuffer = new List<Vector3>();
    //Vector3[] vertexBuffer;
    int Index;

    void OnMouseDown()
    {
        ScreenPoint = Camera.main.WorldToScreenPoint(gameObject.transform.position);
        TheOffset = gameObject.transform.localPosition - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, ScreenPoint.z));
        Index = Int32.Parse(gameObject.name);
        MeshFilter NewMeshFilter = gameObject.transform.parent.GetComponent<MeshFilter>();
        NewMeshFilter.sharedMesh.GetVertices(vertexBuffer);
    }

    void OnMouseDrag()
    {
        Vector3 CursorPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, ScreenPoint.z);
        Vector3 CursorPosition = Camera.main.ScreenToWorldPoint(CursorPoint) + TheOffset;
        transform.localPosition = CursorPosition;
        vertexBuffer[Index] = transform.localPosition;
        gameObject.transform.parent.GetComponent<MeshFilter>().sharedMesh.SetVertices(vertexBuffer);
        //CatmullClark cc = gameObject.transform.parent.GetComponent<CatmullClark>();
        //vertexBuffer = cc.cur_control_mesh_vertices;
        //vertexBuffer[Index] = cursorPosition;
        //cc.cur_control_mesh_vertices = vertexBuffer;
    }

    void OnMouseUp()
    {
        gameObject.transform.parent.GetComponent<MeshFilter>().sharedMesh.RecalculateNormals();
    }

    void MoveMultipleVertices(List<GameObject> spheres)
    {
        //calculating the resulting vector between selected objects
        Vector3 direction = Vector3.zero;
        for (int i = 0; i < spheres.Count; i++)
        {
            direction += spheres[i].transform.position;
        }

        GameObject vector = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        vector.transform.position = direction;
        vector.name = "TheVector";
        vector.GetComponent<Renderer>().material.color = Color.yellow;



    }

}
