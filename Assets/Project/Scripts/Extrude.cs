using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Extrude : MonoBehaviour
{

    int[] control_indices;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void MakeExtrusion(GameObject snowball, int[] control_indices)
    {
        CreateSubDivObject SubDiv = snowball.gameObject.GetComponent<CreateSubDivObject>();

    }
}
