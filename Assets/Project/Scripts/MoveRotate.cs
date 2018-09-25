using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveRotate : MonoBehaviour
{

    public static void Moving(GameObject gameObject, char dir, bool reverse)
    {
        Vector3 CurrPos = gameObject.transform.position;
        Vector3 NewPos = new Vector3();

        if (dir == 'x')
        {
            NewPos = new Vector3(0.5f, 0, 0);
        }
        if (dir == 'y')
        {
            NewPos = new Vector3(0, 0.5f, 0);
        }
        if (dir == 'z')
        {
            NewPos = new Vector3(0, 0, 0.5f);
        }
        if (reverse)
        {
            NewPos = NewPos * (-1);
        }
        CurrPos += NewPos;
        gameObject.transform.position = CurrPos;
    }

    public static void Rotate(GameObject gameObject, float RotateDegreesPerSecond, bool clockwise)
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
