using UnityEngine;
using System.Collections;

public class SplineDrawer : MonoBehaviour {

    public BezierSpline spline;
    public int resolution = 10;


    private Vector3[] vertices;
    Vector2[] uv;
    private float roadWidth = 1f;

    private Mesh mesh;

    private void Awake()
    {
    }

    public void Generate()
    {
        GetComponent<MeshFilter>().mesh = mesh = new Mesh();
        mesh.name = "road";

        vertices = new Vector3[2 * resolution];

        //we're going to have to follow the road
        float stepSize = resolution;//frequency * items.Length;//1f / (frequency * items.Length);
        if (spline.Loop || stepSize == 1)
            stepSize = 1f / stepSize;
        else
            stepSize = 1f / (stepSize - 1);

        //generate vertices along the spline
        for (int p = 0, f = 0; f < vertices.Length; p++, f += 2)
        {
            vertices[f] = spline.GetPoint(p * stepSize);
            vertices[f + 1] = vertices[f];

            //widen these points out a bit
            vertices[f].z = vertices[f].z - (roadWidth / 2);
            vertices[f + 1].z = vertices[f + 1].z + (roadWidth / 2);

            //uv stuff
            //uv[f] = new Vector2(vertices[f].x / resolution, vertices[f].y);
            //uv[f + 1] = new Vector2(vertices[f + 1].x / resolution, vertices[f + 1].y);
        }
        mesh.vertices = vertices;
        SetUV();

        //now we need to link the vertices to trianges
        int[] triangles = new int[resolution * 6];
        bool even = true;
        for (int i = 0, p = 0; i < triangles.Length - 6; i += 3, p++)
        {
            if (even)
            {
                triangles[i] = p;
                triangles[i + 1] = p + 1;
                triangles[i + 2] = p + 2;
            }
            else
            {
                triangles[i] = p + 2;
                triangles[i + 1] = p + 1;
                triangles[i + 2] = p;
            }
            even = !even;
            mesh.triangles = triangles;
        }

        mesh.RecalculateNormals();

    }

    private void SetUV()
    {
        uv = new Vector2[vertices.Length];
        for (int i = 0; i < vertices.Length; i += 4)
        {
            uv[i] = Vector2.zero;
            uv[i + 1] = Vector2.right;
            uv[i + 2] = Vector2.up;
            uv[i + 3] = Vector2.one;
        }
        mesh.uv = uv;
    }
}
