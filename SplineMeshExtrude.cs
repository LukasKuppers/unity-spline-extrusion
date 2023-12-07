using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Splines;

public class SplineMeshExtrude : MonoBehaviour
{
    private enum Axis
    {
        X, Y, Z
    }

    [SerializeField]
    private Mesh extrusionTemplateMesh;
    [SerializeField]
    private Axis extrusionAxis;
    [SerializeField]
    private float extrusionInterval = 10f;
    [SerializeField]
    private bool smoothFaces = true;
    [SerializeField]
    private bool useWorldUp = true;

    private MeshFilter meshFilter;
    private SplineContainer splineContainer;
    private Spline spline;

    private Vector3[] templateVertices;

    private void Awake()
    {
        meshFilter = gameObject.GetComponent<MeshFilter>();
        if (!meshFilter)
            Debug.LogError($"SplineMeshExtrude: Awake: Gameobject {gameObject.name} does not have an attached mesh filter.");

        splineContainer = gameObject.GetComponent<SplineContainer>();
        spline = splineContainer.Spline;

        Mesh generatedMesh = GenerateMesh();
        meshFilter.mesh = generatedMesh;
    }

    private Mesh GenerateMesh()
    {
        Mesh mesh = new Mesh();
        bool success = SplineUtil.SampleSplineInterval(spline, transform, extrusionInterval, 
                                                       out Vector3[] positions, out Vector3[] tangents, out Vector3[] upVectors);
        if (!success)
        {
            Debug.LogError("SplineMeshExtrude: GenerateMesh: Error encountered when sampling spline. Aborting");
            return mesh;
        }

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();

        // distinguish verticies from first and second edges
        (int[] firstEdge, int[] secondEdge) = GetEdgeIndicies(extrusionTemplateMesh.vertices, extrusionAxis);

        templateVertices = CollapsePointsOnAxis(extrusionTemplateMesh.vertices, extrusionAxis);

        for (int i = 0; i < positions.Length - 1; i++)
        {
            AppendMeshSegment(vertices, triangles, normals, uvs,
                              positions[i], tangents[i], upVectors[i], positions[i + 1], tangents[i + 1], upVectors[i + 1],
                              firstEdge, secondEdge);
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.normals = normals.ToArray();
        mesh.uv = uvs.ToArray();

        return mesh;
    }

    private void AppendMeshSegment(List<Vector3> vertices, List<int> triangles, List<Vector3> normals, List<Vector2> uvs,
        Vector3 firstPos, Vector3 firstTangent, Vector3 firstUp, Vector3 secondPos, Vector3 secondTangent, Vector3 secondUp, 
        int[] firstEdgeIndicies, int[] secondEdgeIndicies)
    {
        Vector3[] newVertices = new Vector3[templateVertices.Length];
        Vector3[] newNormals = new Vector3[extrusionTemplateMesh.normals.Length];


        Quaternion rotation = useWorldUp ? Quaternion.LookRotation(new Vector3(firstTangent.x, 0, firstTangent.z), Vector3.up) : 
                                           Quaternion.LookRotation(firstTangent, firstUp);
        Quaternion flatRotation = Quaternion.identity;
        if (!smoothFaces)
        {
            if (useWorldUp)
            {
                Vector3 avgTangentDir = new Vector3(firstTangent.x + secondTangent.x, 0, firstTangent.z + secondTangent.z);
                flatRotation = Quaternion.LookRotation(avgTangentDir, Vector3.up);
            }
            else
            {
                Vector3 avgTangentDir = firstTangent + secondTangent;
                Vector3 avgUpDir = firstUp + secondUp;
                flatRotation = Quaternion.LookRotation(avgTangentDir, avgUpDir);
            }
        }
        Quaternion normalRotation = smoothFaces ? rotation : flatRotation;

        foreach (int index in firstEdgeIndicies)
        {
            // transform verticies and normals to match firstPos
            newVertices[index] = (rotation * templateVertices[index]) + firstPos;
            newNormals[index] = normalRotation * extrusionTemplateMesh.normals[index];
        }

        rotation = useWorldUp ? Quaternion.LookRotation(new Vector3(secondTangent.x, 0, secondTangent.z), Vector3.up) :
                                Quaternion.LookRotation(secondTangent, secondUp);
        normalRotation = smoothFaces ? rotation : flatRotation;
        foreach (int index in secondEdgeIndicies)
        {
            // transform verticies and normals to match secondPos
            newVertices[index] = (rotation * templateVertices[index]) + secondPos;
            newNormals[index] = normalRotation * extrusionTemplateMesh.normals[index];
        }

        int prevVerticiesLength = vertices.Count;

        vertices.AddRange(newVertices);
        triangles.AddRange(extrusionTemplateMesh.triangles.Select(index => index + prevVerticiesLength));
        normals.AddRange(newNormals);
        uvs.AddRange(extrusionTemplateMesh.uv);
    }

    private (int[] first, int[] second) GetEdgeIndicies(Vector3[] templateVertices, Axis axis)
    {
        List<int> firstIndicies = new List<int>();
        List<int> secondIndicies = new List<int>();

        int vectorIndex = axis == Axis.X ? 0 : axis == Axis.Y ? 1 : 2;

        for (int i = 0; i < templateVertices.Length; i++)
        {
            if (templateVertices[i][vectorIndex] < 0)
                firstIndicies.Add(i);
            else
                secondIndicies.Add(i);
        }

        return (firstIndicies.ToArray(), secondIndicies.ToArray());
    }

    // set the specified axis to zero for each point
    // returns a new array, and does not modify the input array
    private Vector3[] CollapsePointsOnAxis(Vector3[] points, Axis axis)
    {
        Vector3[] collapsedPoints = new Vector3[points.Length];
        Vector3 axisCollapseVector = axis == Axis.X ? new Vector3(0, 1, 1) :
                                     axis == Axis.Y ? new Vector3(1, 0, 1) : 
                                                      new Vector3(1, 1, 0);

        for (int i = 0; i < points.Length; i++)
        {
            // element wise multiplication
            collapsedPoints[i] = Vector3.Scale(points[i], axisCollapseVector);
        }
        return collapsedPoints;
    }
}
