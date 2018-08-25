using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VertMove : MonoBehaviour
{
    Vector3 screenPoint;
    Vector3 offset;
    List<Vector3> vertexBuffer = new List<Vector3>();
    int Index;

    void OnMouseDown()
    {
        screenPoint = Camera.main.WorldToScreenPoint(gameObject.transform.position);
        offset = gameObject.transform.localPosition - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));
        Index = Int32.Parse(gameObject.name);
        MeshFilter NewMeshFilter = gameObject.transform.parent.GetComponent<MeshFilter>();
        NewMeshFilter.sharedMesh.GetVertices(vertexBuffer);
    }

    void OnMouseDrag()
    {
        Vector3 cursorPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);
        Vector3 cursorPosition = Camera.main.ScreenToWorldPoint(cursorPoint) + offset;
        transform.localPosition = cursorPosition;
        vertexBuffer[Index] = transform.localPosition;
        gameObject.transform.parent.GetComponent<MeshFilter>().sharedMesh.SetVertices(vertexBuffer);
        //CatmullClark cc = gameObject.transform.parent.GetComponent<CatmullClark>();
        //transform.position = cursorPosition;
        //vertexBuffer = cc.cur_control_mesh_vertices;
        //vertexBuffer[i] = cursorPosition;
        //cc.cur_control_mesh_vertices = vertexBuffer;
    }
    void OnMouseUp()
    {
        gameObject.transform.parent.GetComponent<MeshFilter>().sharedMesh.RecalculateNormals();
    }

}
