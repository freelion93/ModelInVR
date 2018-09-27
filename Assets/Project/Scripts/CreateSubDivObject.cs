using UnityEngine;
using UnityEngine.UI;

public class CreateSubDivObject : MonoBehaviour
{
    float speed = 2.5f;
    float speedRotation = 2.0f;

    int ChangeLevel = 2;
    float Size = 1f;
    [SerializeField]
    Text SubdivLevel;
    [SerializeField]
    Text SizeOfObject;

    public Material Static;

    Ray myRay;
    RaycastHit hit;

    char CurrAxis = 'x';
    bool reverse = false;
    [SerializeField]
    Text Axis;
    [SerializeField]
    Text Direction;

    float RotateDegreesPerSecond = 5.0f;
    bool clockwise = false;
    [SerializeField]
    Text Degrees;
    [SerializeField]
    Text Clockwise;

    void Start()
    {
        SubdivLevel.text = "Subdivision Level: " + ChangeLevel.ToString();
        SizeOfObject.text = "Size: " + Size.ToString();
        Axis.text = "Axis: " + CurrAxis.ToString().ToUpper();
        Direction.text = "Inverse Direction: " + "No";
        Degrees.text = "Degrees Per Sec: " + RotateDegreesPerSecond.ToString();
        Clockwise.text = "Clockwise: " + "No";
    }

    void Update()
    {
        CreateObject();
        ControlCreateObject();
        GetObject();
        MovementControl();
    }

    void CreateObject()
    {
        if (Input.GetButtonDown("Fire2"))
        {
            // Create a new Mesh GameObject and add Catmull-Clark to it

            GameObject target = new GameObject("Snowball");
            myRay = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(myRay, out hit))
            {
                target.transform.position = hit.point;
            }
            else
            {
                Debug.Log("emptyarea");
                Destroy(target);
            }

            Vector3[] vertices = new Vector3[8];
            vertices[0] = new Vector3(-1, -1, -1) * Size;
            vertices[1] = new Vector3(1, -1, -1) * Size;
            vertices[2] = new Vector3(1, -1, 1) * Size;
            vertices[3] = new Vector3(-1, -1, 1) * Size;
            vertices[4] = new Vector3(-1, 1, -1) * Size;
            vertices[5] = new Vector3(1, 1, -1) * Size;
            vertices[6] = new Vector3(1, 1, 1) * Size;
            vertices[7] = new Vector3(-1, 1, 1) * Size;

            int[] indices =
            {
                0, 1, 2, 3,
                7, 6, 5, 4,
                0, 4, 5, 1,
                1, 5, 6, 2,
                2, 6, 7, 3,
                3, 7, 4, 0,
            };

            Mesh control_mesh = new Mesh();
            control_mesh.Clear();
            control_mesh.vertices = vertices;
            control_mesh.SetIndices(indices, MeshTopology.Quads, 0);
            control_mesh.RecalculateNormals();

            MeshFilter mf = target.AddComponent<MeshFilter>();
            mf.mesh = control_mesh;
            MeshRenderer mr = target.AddComponent<MeshRenderer>();
            mr.material.color = Color.white;
            MeshCollider mc = target.AddComponent<MeshCollider>();
            mc.material = null;

            CatmullClark cc = target.AddComponent<CatmullClark>();
            cc.subdiv_level = ChangeLevel;
        }
    }

    void ControlCreateObject()
    {
        //Changing the Level of subdivision
        if (Input.GetAxis("Mouse ScrollWheel") > 0f) // Increase
        {
            if (ChangeLevel < 4)
            {
                ChangeLevel++;
                SubdivLevel.text = "Subdivision Level: " + ChangeLevel.ToString();
            }
            else
            {
                Debug.Log("Level of smoothing is too big");
            }
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0f) // Decrease
        {
            if (ChangeLevel > 0)
            {
                ChangeLevel--;
                SubdivLevel.text = "Subdivision Level: " + ChangeLevel.ToString();
            }
            else
            {
                Debug.Log("Level of smoothing is too low");
            }
        }
        //Increasing and decreasing the size of object
        if (Input.GetKeyDown(KeyCode.PageUp))
        {
            Size += 0.1f;
            SizeOfObject.text = "Size: " + Size.ToString();
        }
        if (Input.GetKeyDown(KeyCode.PageDown))
        {
            if (Size > 0.1)
            {
                Size -= 0.1f;
                SizeOfObject.text = "Size: " + Size.ToString();
            }
            else
            {
                Debug.Log("Size can't be negative");
            }
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (CurrAxis == 'x')
            {
                CurrAxis = 'y';
            }
            else if (CurrAxis == 'y')
            {
                CurrAxis = 'z';
            }
            else
            {
                CurrAxis = 'x';
            }
            Axis.text = "Axis: " + CurrAxis.ToString().ToUpper();
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (!reverse)
            {
                reverse = true;
                Direction.text = "Inverse Direction: " + "Yes";

            }
            else
            {
                reverse = false;
                Direction.text = "Inverse Direction: " + "No";
            }
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (RotateDegreesPerSecond < 90)
            {
                RotateDegreesPerSecond += 5.0f;
            }
            else
            {
                Debug.Log("Value > 90' ");
            }
            Degrees.text = "Degrees Per Sec: " + RotateDegreesPerSecond.ToString();
        }
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (RotateDegreesPerSecond > 0)
            {
                RotateDegreesPerSecond -= 5.0f;
            }
            else
            {
                Debug.Log("Value < 0' ");
            }
            Degrees.text = "Degrees Per Sec: " + RotateDegreesPerSecond.ToString();
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            if (!clockwise)
            {
                clockwise = true;
                Clockwise.text = "Clockwise: " + "Yes";
            }
            else
            {
                clockwise = false;
                Clockwise.text = "Clockwise: " + "No";
            }

        }

    }

    void GetObject()
    {
        myRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        bool doHit = Physics.Raycast(myRay, out hit, 500f);

        if (doHit)
        {
            if (hit.collider.name == "Snowball")
            {
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    if (hit.collider.transform.childCount < 1)
                    {
                        GetSelect(hit.collider.gameObject);
                    }
                    else
                    {
                        Deselect(hit.collider.gameObject);
                    }
                }

                if (Input.GetButtonDown("Fire1"))
                {
                    MoveRotate.Moving(hit.collider.gameObject, CurrAxis, reverse);
                }
                if (Input.GetButtonDown("Fire3"))
                {
                    MoveRotate.Rotate(hit.collider.gameObject, RotateDegreesPerSecond, clockwise);
                }
                if (Input.GetKeyDown(KeyCode.E))
                {
                    Extrude.MakeExtrusion(hit.collider.gameObject, hit);
                }
            }
            if (hit.collider.tag == "Selector")
            {
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    if (hit.collider.gameObject.GetComponent<Renderer>().material.color == Color.blue)
                    {
                        hit.collider.gameObject.GetComponent<Renderer>().material.color = Color.red;
                    }
                    else
                    {
                        hit.collider.gameObject.GetComponent<Renderer>().material.color = Color.blue;
                    }
                }
            }
        }
    }

    //Towards and backward movements & Keyboard rotation
    void MovementControl()
    {
        if (Input.GetKey(KeyCode.W))
        {
            transform.position += transform.forward * speed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.S))
        {
            transform.position -= transform.forward * speed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.A))
        {
            transform.Rotate(Vector3.down * speedRotation);
        }
        if (Input.GetKey(KeyCode.D))
        {
            transform.Rotate(Vector3.up * speedRotation);
        }

    }

    //Selecting the object and creating the childrens spheres (marks for vertices)
    void GetSelect(GameObject snowball)
    {
        Vector3[] LocalVertices = snowball.GetComponent<MeshFilter>().mesh.vertices;

        GameObject[] Spheres = new GameObject[LocalVertices.Length];
        for (int i = 0; i < LocalVertices.Length; i++)
        {
            Spheres[i] = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Spheres[i].name = i.ToString();
            Spheres[i].transform.parent = snowball.transform;
            Spheres[i].GetComponent<Renderer>().material = Static;
            Spheres[i].transform.position = LocalVertices[i] + snowball.transform.position;
            Spheres[i].transform.localScale = new Vector3(0.1F, 0.1F, 0.1F) * Size;
            Spheres[i].AddComponent<VertMove>();
            Spheres[i].tag = "Selector";
        }

        //The vertices array contains 24 elements, 
        //so here we are getting rid of the duplicate vertices
        //we start with creating one loop for array of childrens values
        for (int i = 0; i < snowball.transform.childCount; i++)
        {
            //create nested loop for compare current values with actual value of array of childrens
            for (int j = i + 1; j < snowball.transform.childCount; j++)
            {
                //if we found a duplicate position of the two childrens we are removing one of them
                if (snowball.transform.GetChild(i).gameObject.transform.position == snowball.transform.GetChild(j).gameObject.transform.position)
                {
                    Destroy(snowball.transform.GetChild(i).gameObject);
                }
            }
        }
    }

    //Deselecting the object and removing the childrens spheres (marks for vertices)
    void Deselect(GameObject snowball)
    {
        for (int i = 0; i < snowball.transform.childCount; i++)
        {
            Destroy(snowball.transform.GetChild(i).gameObject);
        }
    }
}
