using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveRotate : MonoBehaviour
{

    void Move(GameObject gameObject, string dir, bool reverse)
    {
        Vector3 CurrPos = gameObject.transform.position;
        Vector3 NewPos = new Vector3();
        if (dir == "x")
        {
            NewPos = new Vector3(1, 0, 0);
        }
        if (dir == "y")
        {
            NewPos = new Vector3(0, 1, 0);
        }
        if (dir == "z")
        {
            NewPos = new Vector3(0, 0, 1);
        }
        if (reverse)
        {
            NewPos = NewPos * (-1);
        }
        CurrPos += NewPos;
    }

    void Rotate(GameObject gameObject, float RotateDegreesPerSecond, bool clockwise)
    {
        if (clockwise)
        {
            gameObject.transform.Rotate(Vector3.forward * RotateDegreesPerSecond);
        }
        // Rotate to the left
        else
        {
            gameObject.transform.Rotate(Vector3.back * RotateDegreesPerSecond);
        }
        //Rotate to the right
    }


}
