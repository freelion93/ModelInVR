using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;

public class CreateSubDivObject : MonoBehaviour
{
    private float dropForce = 2.0f;

    public float speedH = 2.0f;
    public float speedV = 2.0f;

    private float speed = 2.5f;
    private float speedRotation = 2.0f;
    private float speedIncreaser = 1;

    private int ChangeLevel = 2;
    public Text SubdivLevel;

    private float Size = 1f;
    public Text SizeOfObject;

    public Material Static;
    public Texture2D CursorTexture;
    public Texture2D CursorTexture_Selected;
    public Texture2D CursorTexture_Vertice;

    private Vector3 screenPoint;
    private Vector3 offset;

    Ray myRay;
    RaycastHit hit;

    void Start()
    {
        SubdivLevel.text = "Subdivision Level: " + ChangeLevel.ToString();
        SizeOfObject.text = "Size: " + Size.ToString();
    }

    void Update()
    {
        GetObject();

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            speedIncreaser = 2f;
        }

        if (Input.GetButtonDown("Fire2"))
        {
            // Create a new Mesh GameObject and add Catmull-Clark to it
            GameObject target = new GameObject("Snowball");
            myRay = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(myRay, out hit))
            {
                target.transform.transform.position = hit.point;
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

            for (int i = 0; i < control_mesh.vertices.Length; i++)
            {
                Debug.Log(control_mesh.vertices[i]);
           }
        }

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
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            Size += 0.1f;
            SizeOfObject.text = "Size: " + Size.ToString();
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
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
        //Towards and backward movements
        if (Input.GetKey(KeyCode.W))
        {
            transform.position += transform.forward * speed * Time.deltaTime * speedIncreaser;
        }
        if (Input.GetKey(KeyCode.S))
        {
            transform.position -= transform.forward * speed * Time.deltaTime * speedIncreaser;
        }
        //Keyboard rotation
        if (Input.GetKey(KeyCode.A))
        {
            transform.Rotate(Vector3.down * speedRotation);
        }
        if (Input.GetKey(KeyCode.D))
        {
            transform.Rotate(Vector3.up * speedRotation);
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
                Cursor.SetCursor(this.CursorTexture_Selected, Vector2.zero, CursorMode.Auto);
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    if (hit.collider.transform.childCount < 1)
                    {
                        GetSelect(hit.collider.gameObject);
                        RemoveDubs(hit.collider.gameObject);
                    }
                    else
                    {
                        Deselect(hit.collider.gameObject);
                    }
                }
            }
            else if (hit.collider.name == "Sphere")
            {
                Cursor.SetCursor(this.CursorTexture_Vertice, Vector2.zero, CursorMode.Auto);
                if (VertMove.Activator)
                {
                    for (int i=0; i < hit.collider.gameObject.transform.parent.gameObject.GetComponent<MeshFilter>().mesh.vertices.Length; i++)
                    {
                        if (hit.collider.gameObject.transform.parent.gameObject.GetComponent<MeshFilter>().mesh.vertices[i] == hit.collider.gameObject.transform.position)
                        {
                            
                        }
                    }
                }
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    if(hit.collider.gameObject.GetComponent<Renderer>().material.color == Color.blue)
                    {
                        hit.collider.gameObject.GetComponent<Renderer>().material.color = Color.red;
                    }
                    else
                    {
                        hit.collider.gameObject.GetComponent<Renderer>().material.color = Color.blue;
                    }
                }
            }
            else
            {
                Cursor.SetCursor(this.CursorTexture, Vector2.zero, CursorMode.Auto);
            }
        }
    }

    //Selecting the object and creating the childrens spheres (marks for vertices)
    void GetSelect(GameObject gameObject)
    {
        Vector3[] vertices = gameObject.GetComponent<MeshFilter>().mesh.vertices;
        GameObject[] Spheres = new GameObject[vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            Spheres[i] = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Spheres[i].transform.parent = gameObject.transform;
            Spheres[i].GetComponent<Renderer>().material = Static;
            Spheres[i].transform.position = vertices[i] + gameObject.transform.position;
            Spheres[i].transform.localScale = new Vector3(0.1F, 0.1F, 0.1F) * Size;
            Spheres[i].AddComponent<VertMove>();

        }
    }

    //Deselecting the object and removing the childrens spheres (marks for vertices)
    void Deselect (GameObject gameObject)
    {
        for (int i = 0; i < gameObject.transform.childCount; i++)
        {
            GameObject.Destroy(gameObject.transform.GetChild(i).gameObject);
        }
    }

    //The vertices array contains 24 elements, the function RemoveDubs was written to get rid of the duplicate vertices
    void RemoveDubs(GameObject gameObject)
    {
        //create one loop for array of childrens values
        for (int i = 0; i < gameObject.transform.childCount; i++)
        {
            //create nested loop for compare current values with actual value of array of childrens
            for (int j = i + 1; j < gameObject.transform.childCount; j++)
            {
                //if we found a duplicate position of the two childrens we are removing one of them
                if (gameObject.transform.GetChild(i).gameObject.transform.position == gameObject.transform.GetChild(j).gameObject.transform.position)
                {
                    GameObject.Destroy(gameObject.transform.GetChild(i).gameObject);
                }
            }
        }
    }

    void MoveVerticles()
    {
        
    }



}