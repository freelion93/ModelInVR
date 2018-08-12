using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VertMove : MonoBehaviour {

    private Vector3 screenPoint;
    private Vector3 offset;
    public static bool Activator = false;

    void OnMouseDown()
    {
        Activator = true;
        int f;
        Vector3[] vertices = gameObject.transform.parent.gameObject.GetComponent<MeshFilter>().mesh.vertices;
        for (int i = 0; i < vertices.Length; i++)
        {
            if (gameObject.transform.position == vertices[i])
            {
                f = i;
                Debug.Log(f);
            }
        }

        screenPoint = Camera.main.WorldToScreenPoint(gameObject.transform.position);
        offset = gameObject.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));
    }


    void OnMouseDrag()
    {
            Vector3 cursorPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);
            Vector3 cursorPosition = Camera.main.ScreenToWorldPoint(cursorPoint) + offset;
            transform.position = cursorPosition;
    }
}
