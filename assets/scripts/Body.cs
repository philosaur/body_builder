using Godot;
using System.Collections.Generic;

public partial class Body : Node3D
{
    [Export] public float CrossSectionRadius = 0.5f;
    [Export] public int CrossSectionPointCount = 4;
    [Export] public float Length = 1.0f;
    [Export] public float FrontScale = 0.75f;
    [Export] public Color AlbedoColor = new Color(0.8f, 0.8f, 0.8f);
    [Export] public float Roughness = 0.5f;
    [Export] public float Metallic = 0.0f;

    private Vector3 _primaryAnchor;
    private List<Vector3> _crossSectionPoints;

    public override void _Ready()
    {
        GenerateCrossSection();
        GenerateCube();
    }

    private void GenerateCrossSection()
    {
        _primaryAnchor = Vector3.Zero;
        _crossSectionPoints = new List<Vector3>();

        float angleStep = 2.0f * Mathf.Pi / CrossSectionPointCount;
        
        // Generate points around the primary anchor
        for (int i = 0; i < CrossSectionPointCount; i++)
        {
            float angle = i * angleStep - Mathf.Pi / 2.0f; // Start from top (90 degrees)
            float x = Mathf.Cos(angle) * CrossSectionRadius;
            float y = Mathf.Sin(angle) * CrossSectionRadius;
            _crossSectionPoints.Add(new Vector3(x, y, 0));
        }
    }

    private void GenerateCube()
    {
        // Create a new MeshInstance3D
        var meshInstance = new MeshInstance3D();
        AddChild(meshInstance);

        // Create vertices for front and back faces
        var vertices = new List<Vector3>();
        var indices = new List<int>();
        var normals = new List<Vector3>();

        // Add front face vertices (scaled)
        int frontStartIndex = vertices.Count;
        foreach (var point in _crossSectionPoints)
        {
            vertices.Add(new Vector3(point.X * FrontScale, point.Y * FrontScale, Length / 2));
            normals.Add(Vector3.Back); // Front face normal points backward (since we're using clockwise winding)
        }

        // Add back face vertices
        int backStartIndex = vertices.Count;
        foreach (var point in _crossSectionPoints)
        {
            vertices.Add(new Vector3(point.X, point.Y, -Length / 2));
            normals.Add(Vector3.Forward); // Back face normal points forward (since we're using clockwise winding)
        }

        // Generate triangles for front face (clockwise winding when looking from outside)
        for (int i = 0; i < CrossSectionPointCount - 2; i++)
        {
            indices.Add(frontStartIndex + i + 2);
            indices.Add(frontStartIndex + i + 1);
            indices.Add(frontStartIndex);
        }

        // Generate triangles for back face (clockwise winding when looking from outside)
        for (int i = 0; i < CrossSectionPointCount - 2; i++)
        {
            indices.Add(backStartIndex + i + 1);
            indices.Add(backStartIndex + i + 2);
            indices.Add(backStartIndex);
        }

        // Generate triangles for sides (clockwise winding when looking from outside)
        for (int i = 0; i < CrossSectionPointCount; i++)
        {
            int nextI = (i + 1) % CrossSectionPointCount;
            
            // Calculate side face normal (flipped to match clockwise winding)
            Vector3 currentPoint = _crossSectionPoints[i];
            Vector3 nextPoint = _crossSectionPoints[nextI];
            Vector3 sideNormal = new Vector3(nextPoint.Y - currentPoint.Y, currentPoint.X - nextPoint.X, 0).Normalized();
            
            // Add vertices for side face with proper normals
            int sideStartIndex = vertices.Count;
            vertices.Add(vertices[frontStartIndex + i]);
            vertices.Add(vertices[backStartIndex + i]);
            vertices.Add(vertices[frontStartIndex + nextI]);
            vertices.Add(vertices[backStartIndex + nextI]);
            
            normals.Add(sideNormal);
            normals.Add(sideNormal);
            normals.Add(sideNormal);
            normals.Add(sideNormal);

            // First triangle of the side quad (clockwise when looking from outside)
            indices.Add(sideStartIndex + 2);
            indices.Add(sideStartIndex + 1);
            indices.Add(sideStartIndex + 0);

            // Second triangle of the side quad (clockwise when looking from outside)
            indices.Add(sideStartIndex + 2);
            indices.Add(sideStartIndex + 3);
            indices.Add(sideStartIndex + 1);
        }

        // Create the mesh
        var mesh = new ArrayMesh();
        var arrays = new Godot.Collections.Array();
        arrays.Resize((int)ArrayMesh.ArrayType.Max);
        arrays[(int)ArrayMesh.ArrayType.Vertex] = vertices.ToArray();
        arrays[(int)ArrayMesh.ArrayType.Normal] = normals.ToArray();
        arrays[(int)ArrayMesh.ArrayType.Index] = indices.ToArray();

        mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);

        // Create and set the material
        var material = new StandardMaterial3D
        {
            AlbedoColor = AlbedoColor,
            Roughness = Roughness,
            Metallic = Metallic
        };
        mesh.SurfaceSetMaterial(0, material);

        meshInstance.Mesh = mesh;
    }
}