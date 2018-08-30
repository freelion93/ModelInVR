using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class CatmullClark : MonoBehaviour
{

    public int subdiv_level = 0;
    public bool limited_update = false;

    MeshFilter _mesh_filter;
    MeshCollider _mesh_collider;
    MeshData _orig_mesh_data;
    MeshData _cur_control_mesh;
    bool _control_mesh_changed = false;
    MeshData _cur_mesh_data;

    void Awake()
    {
        _mesh_filter = GetComponent<MeshFilter>();
        _mesh_collider = GetComponent<MeshCollider>();
        _orig_mesh_data = new MeshData(_mesh_filter.sharedMesh.vertices, _mesh_filter.sharedMesh.GetIndices(0));
        _cur_mesh_data = _orig_mesh_data;
        _cur_control_mesh = _orig_mesh_data;
        _update_collision_mesh();
    }

    // Update is called once per frame
    void Update()
    {
        bool changed = false;
        if (_control_mesh_changed || _cur_mesh_data.subdiv_level > subdiv_level)
        {
            _cur_mesh_data = _cur_control_mesh;
            _control_mesh_changed = false;
            changed = true;
        }
        int target_level = limited_update ? System.Math.Min(_cur_mesh_data.subdiv_level + 1, subdiv_level) : subdiv_level;
        while (_cur_mesh_data.subdiv_level < target_level)
        {
            _cur_mesh_data = subdivide(_cur_mesh_data);
            changed = true;
        }
        if (changed)
        {
            _mesh_filter.mesh.Clear();
            _mesh_filter.mesh.vertices = _cur_mesh_data.vertices;
            _mesh_filter.mesh.SetIndices(_cur_mesh_data.indices, MeshTopology.Quads, 0);
            _mesh_filter.mesh.RecalculateBounds();
            if (!limited_update || _cur_mesh_data.subdiv_level == subdiv_level)
            {
                _mesh_filter.mesh.RecalculateNormals();
            }
            // update mesh collider with triangle mesh of current subdiv level
            _update_collision_mesh();
        }
    }


    /*
     *  Subdivides a pure quad mesh
     */
    MeshData subdivide(MeshData mesh_data)
    {
        Vector3[] orig_vertices = mesh_data.vertices;
        int[] orig_indices = mesh_data.indices;
        // Compute new face points and collect edges and their adjacent faces for later
        Dictionary<Edge, List<int>> edges = new Dictionary<Edge, List<int>>();
        Vector3[] face_points = new Vector3[orig_indices.Length / 4];
        for (int i = 0; i < orig_indices.Length; i += 4)
        {
            int face_index = i / 4;
            Vector3 face_point = new Vector3();
            for (int fi = 0; fi < 4; fi++)
            {
                face_point += orig_vertices[orig_indices[i + fi]];
                Edge edge = new Edge(orig_indices[i + fi], orig_indices[i + ((fi + 1) % 4)]);
                if (edges.ContainsKey(edge))
                {
                    edges[edge].Add(face_index);
                }
                else
                {
                    edges.Add(edge, new List<int>(new int[] { face_index }));
                }
            }
            face_points[face_index] = face_point / 4;
        }
        // Compute updated positions of existing vertices
        Vector3[] vertex_points = new Vector3[orig_vertices.Length];
        for (int vi = 0; vi < orig_vertices.Length; vi++)
        {
            Vector3 F = new Vector3();
            int neighbour_face_count = 0;
            for (int i = 0; i < orig_indices.Length; i++)
            {
                if (orig_indices[i] == vi)
                {
                    neighbour_face_count++;
                    F += face_points[i / 4];
                }
            }
            F *= 1.0f / neighbour_face_count;
            List<Edge> incident_edges = new List<Edge>();
            foreach (Edge e in edges.Keys)
            {
                if (e.from == vi || e.to == vi)
                {
                    incident_edges.Add(e);
                }
            }
            int neighbour_edge_count = incident_edges.Count;
            if (neighbour_edge_count == neighbour_face_count)  // interior vertex
            {
                Vector3 R = new Vector3();
                foreach (Edge e in incident_edges)
                {
                    R += (orig_vertices[e.from] + orig_vertices[e.to]) * 0.5f;
                }
                R *= 1.0f / neighbour_edge_count;
                vertex_points[vi] = (F + 2.0f * R + (neighbour_face_count - 3) * orig_vertices[vi]) * (1.0f / neighbour_face_count);
            }
            else  // crease boundary vertex
            {
                List<Edge> boundary = new List<Edge>(2);
                foreach (Edge e in incident_edges)
                {
                    if (edges[e].Count < 2)
                    {
                        boundary.Add(e);
                    }
                }
                if (boundary.Count != 2)
                {
                    Debug.LogError("Non-manifold boundary detected!");
                }
                Vector3[] neighbour_vertices = new Vector3[2];
                for (int n = 0; n < 2; n++)
                {
                    if (boundary[n].from == vi)
                    {
                        neighbour_vertices[n] = orig_vertices[boundary[n].to];
                    }
                    else
                    {
                        neighbour_vertices[n] = orig_vertices[boundary[n].from];
                    }
                }
                vertex_points[vi] = (neighbour_vertices[0] + 6 * orig_vertices[vi] + neighbour_vertices[1]) * (1.0f / 8);
            }
        }
        // Compute new edge points (average of end points and new adjacent face points)
        int edge_offset = vertex_points.Length;
        List<Vector3> edge_points = new List<Vector3>(edges.Count);
        Dictionary<Edge, int> edge_point_index = new Dictionary<Edge, int>();
        foreach (Edge e in edges.Keys)
        {
            List<int> neighbour_faces = edges[e];
            Vector3 ep = orig_vertices[e.from] + orig_vertices[e.to];
            if (neighbour_faces.Count < 2)  // crease boundary edge
            {
                neighbour_faces.Clear();
            }
            foreach (int nf in neighbour_faces)
            {
                ep += face_points[nf];
            }
            ep *= 1.0f / (2 + neighbour_faces.Count);
            int ei = edge_points.Count;
            edge_points.Add(ep);
            edge_point_index[e] = ei + edge_offset;
        }
        // Assemble subdivided mesh from new points
        int face_offset = edge_offset + edge_points.Count;
        Vector3[] new_vertices = new Vector3[vertex_points.Length + edge_points.Count + face_points.Length];
        vertex_points.CopyTo(new_vertices, 0);
        edge_points.CopyTo(new_vertices, edge_offset);
        face_points.CopyTo(new_vertices, face_offset);
        List<int> new_indices = new List<int>();
        List<int> root_indices = new List<int>();
        List<int> dir_from_parent = new List<int>();
        for (int i = 0; i < orig_indices.Length; i += 4)
        {
            int face_index = i / 4;
            int face_point_index = face_offset + face_index;
            for (int fi = 0; fi < 4; fi++)
            {
                Edge prev = new Edge(orig_indices[i + ((fi + 3) % 4)], orig_indices[i + fi]);
                Edge next = new Edge(orig_indices[i + fi], orig_indices[i + ((fi + 1) % 4)]);
                // ensure faces are consistently oriented (e.g. first vertex is always south-east, or similar)
                int[] face_indices = { orig_indices[i + fi], edge_point_index[next], face_point_index, edge_point_index[prev] };
                for (int rotated_index = 4 - fi; rotated_index < 4 - fi + 4; rotated_index++)
                {
                    new_indices.Add(face_indices[rotated_index % 4]);
                }
                root_indices.Add(mesh_data.root_indices[face_index]);
                int cur_dir_from_parent = mesh_data.dir_from_parent[face_index];
                cur_dir_from_parent = cur_dir_from_parent << 2;  // make room for 2 bits to indicate direction for current subdiv level
                cur_dir_from_parent = cur_dir_from_parent | fi;  // store corner of the parent face which corresponds to the current face
                dir_from_parent.Add(cur_dir_from_parent);
            }
        }
        return new MeshData(new_vertices, new_indices.ToArray(), mesh_data.subdiv_level + 1, root_indices.ToArray(), dir_from_parent.ToArray());
    }


    public void reset()
    {
        _cur_control_mesh = _orig_mesh_data;
        _control_mesh_changed = true;
    }

    public void apply()
    {
        _orig_mesh_data = _cur_control_mesh;
    }

    public Vector3[] cur_control_mesh_vertices
    {
        get { return _cur_control_mesh.vertices; }
        set
        {
            _cur_control_mesh.vertices = value;
            _control_mesh_changed = true;
        }
    }

    public int[] cur_control_mesh_indices
    {
        get { return _cur_control_mesh.indices; }
        set
        {
            _cur_control_mesh.indices = value;
            _control_mesh_changed = true;
        }
    }

    public Vector3[] orig_control_mesh_vertices
    {
        get { return _orig_mesh_data.vertices; }
    }

    public int[] orig_control_mesh_indices
    {
        get { return _orig_mesh_data.indices; }
    }

    public ParametricLocation param_location(int triangle_index, Vector3 barycentric_coords)
    {
        int quad_index = triangle_index / 2;
        int half = triangle_index % 2;
        int root_index = _cur_mesh_data.root_indices[quad_index];
        int dir_from_parent = _cur_mesh_data.dir_from_parent[quad_index];
        Vector4 param_range = new Vector4(0.0f, 0.0f, 1.0f, 1.0f);  // U, V, U-size, V-size
        for (int i = _cur_mesh_data.subdiv_level; i > 0; i--)
        {
            // Debug.Log("subdiv level " + (_cur_mesh_data.subdiv_level - i) + ": " + ((dir_from_parent >> ((i - 1) * 2)) & 0x3));

            // every 2 bit in dir_from_parent indicate for one subdivision step 
            // the corner of the parent face to which the current face belongs:
            //
            //    v
            //    ^ 3---2
            //    | |-|-|
            //    | 0---1
            //    |
            //    x--------->u

            switch ((dir_from_parent >> ((i - 1) * 2)) & 0x3)
            {
                case 0:
                    param_range[2] = param_range[2] * 0.5f;  // U-size
                    param_range[3] = param_range[3] * 0.5f;  // V-size
                    break;
                case 1:
                    param_range[0] = param_range[0] + param_range[2] * 0.5f;  // U
                    param_range[2] = param_range[2] * 0.5f;  // U-size
                    param_range[3] = param_range[3] * 0.5f;  // V-size
                    break;
                case 2:
                    param_range[0] = param_range[0] + param_range[2] * 0.5f;  // U
                    param_range[1] = param_range[1] + param_range[3] * 0.5f;  // V
                    param_range[2] = param_range[2] * 0.5f;  // U-size
                    param_range[3] = param_range[3] * 0.5f;  // V-size
                    break;
                case 3:
                    param_range[2] = param_range[2] * 0.5f;  // U-size
                    param_range[1] = param_range[1] + param_range[3] * 0.5f;  // V
                    param_range[3] = param_range[3] * 0.5f;  // V-size
                    break;
            }
        }
        Vector2[] param_domain;
        if (half == 0)
        {
            param_domain = new Vector2[] {
                new Vector2(param_range[0], param_range[1]),
                new Vector2(param_range[0] + param_range[2], param_range[1]),
                new Vector2(param_range[0] + param_range[2], param_range[1] + param_range[3])
            };
        }
        else
        {
            param_domain = new Vector2[] {
                new Vector2(param_range[0], param_range[1]),
                new Vector2(param_range[0] + param_range[2], param_range[1] + param_range[3]),
                new Vector2(param_range[0], param_range[1] + param_range[3])
            };
        }
        Vector2 param_uv = new Vector2();
        for (int i = 0; i < 3; i++)
        {
            param_uv += barycentric_coords[i] * param_domain[i];
        }
        param_uv.x = Mathf.Clamp(param_uv.x, 0.0f, 0.9999f);
        param_uv.y = Mathf.Clamp(param_uv.y, 0.0f, 0.9999f);
        return new ParametricLocation(root_index, param_uv);
    }


    public void save_obj(string filename, MeshData mesh_data)
    {
        System.IO.StreamWriter file = new System.IO.StreamWriter(filename);
        foreach (Vector3 v in mesh_data.vertices)
        {
            file.WriteLine(string.Format("v {0} {1} {2}", v.x, v.y, v.z));
        }
        for (int i = 0; i < mesh_data.indices.Length; i += 4)
        {
            file.WriteLine(string.Format("f {0} {1} {2} {3}", mesh_data.indices[i] + 1, mesh_data.indices[i + 1] + 1, mesh_data.indices[i + 2] + 1, mesh_data.indices[i + 3] + 1));
        }
        file.Close();
    }


    void _update_collision_mesh()
    {
        if (_mesh_collider)
        {
            int[] quad_indices = _cur_mesh_data.indices;
            int[] triangles = new int[quad_indices.Length / 4 * 6];
            for (int i = 0; i < quad_indices.Length; i += 4)
            {
                int face_index = i / 4;
                // first triangle
                triangles[face_index * 6] = quad_indices[i];
                triangles[face_index * 6 + 1] = quad_indices[i + 1];
                triangles[face_index * 6 + 2] = quad_indices[i + 2];
                // second triangle
                triangles[face_index * 6 + 3] = quad_indices[i];
                triangles[face_index * 6 + 4] = quad_indices[i + 2];
                triangles[face_index * 6 + 5] = quad_indices[i + 3];
            }
            Mesh collision_mesh = new Mesh();
            collision_mesh.vertices = _cur_mesh_data.vertices;
            collision_mesh.triangles = triangles;
            _mesh_collider.sharedMesh = collision_mesh;
            Debug.Log("Updated collision mesh");
        }
    }
}


struct Edge
{
    public int from;
    public int to;

    public Edge(int from, int to)
    {
        this.from = from;
        this.to = to;
    }

    public override bool Equals(System.Object obj)
    {
        return obj is Edge && this == (Edge)obj;
    }

    public override int GetHashCode()
    {
        return from.GetHashCode() ^ to.GetHashCode();
    }

    public static bool operator ==(Edge x, Edge y)
    {
        return (x.from == y.from && x.to == y.to) || (x.from == y.to && x.to == y.from);
    }

    public static bool operator !=(Edge x, Edge y)
    {
        return !(x == y);
    }
}


public struct MeshData
{
    public Vector3[] vertices;
    public int[] indices;
    public int[] root_indices;
    public int[] dir_from_parent;
    public int subdiv_level;

    public MeshData(Vector3[] vertices, int[] indices, int subdiv_level = 0, int[] root_indices = null, int[] dir_from_parent = null)
    {
        this.vertices = vertices;
        this.indices = indices;
        this.subdiv_level = subdiv_level;
        int num_faces = indices.Length / 4;
        this.root_indices = root_indices;
        this.dir_from_parent = dir_from_parent;
        if (root_indices == null)
        {
            this.root_indices = new int[num_faces];
        }
        if (dir_from_parent == null)
        {
            this.dir_from_parent = new int[num_faces];
        }
        if (root_indices == null || dir_from_parent == null)
        {
            for (int i = 0; i < num_faces; i++)
            {
                this.root_indices[i] = i;
                this.dir_from_parent[i] = 0;
            }
        }
    }


}


public struct ParametricLocation
{
    public int face_index;
    public Vector2 uv;

    public ParametricLocation(int face_index, Vector2 uv)
    {
        this.face_index = face_index;
        this.uv = uv;
    }
}