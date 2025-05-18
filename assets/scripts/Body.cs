using Godot;
using System.Collections.Generic;

public partial class Body : Node3D
{
    [Export] public float CrossSectionRadius = 0.5f;
    [Export] public int CrossSectionPointCount = 5;
    [Export] public Vector3[] CrossSectionScales = new Vector3[] 
    {
        new Vector3(0.10f, 0.10f, 0.50f),
        new Vector3(0.40f, 0.70f, 0.50f),
        new Vector3(0.50f, 0.80f, 0.50f),
		new Vector3(0.50f, 0.80f, 0.50f),
		new Vector3(0.50f, 0.70f, 0.50f),
        new Vector3(0.40f, 0.60f, 0.50f),
        new Vector3(0.30f, 0.50f, 0.50f),
		new Vector3(0.20f, 0.30f, 0.50f),
		new Vector3(0.10f, 0.10f, 0.50f)
    };
    [Export] public float Length = 2.0f;
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

        // Create vertices for all cross-sections
        var vertices = new List<Vector3>();
        var indices = new List<int>();
        var normals = new List<Vector3>();

        // Calculate total length scale for normalization
        float totalLengthScale = 0;
        foreach (var scale in CrossSectionScales)
        {
            totalLengthScale += scale.Z;
        }

        // Generate vertices for each cross-section
        int[] sectionStartIndices = new int[CrossSectionScales.Length];
        float currentZ = Length / 2; // Start at front

        for (int section = 0; section < CrossSectionScales.Length; section++)
        {
            sectionStartIndices[section] = vertices.Count;
            Vector3 scale = CrossSectionScales[section];
            
            foreach (var point in _crossSectionPoints)
            {
                vertices.Add(new Vector3(
                    point.X * scale.X,
                    point.Y * scale.Y,
                    currentZ
                ));
                normals.Add(Vector3.Forward); // Will be updated for side faces
            }

            // Update Z position for next section
            if (section < CrossSectionScales.Length - 1)
            {
                float sectionLength = (scale.Z / totalLengthScale) * Length;
                currentZ -= sectionLength;
            }
        }

        // Generate triangles for front face (clockwise winding when looking from outside)
        for (int i = 0; i < CrossSectionPointCount - 2; i++)
        {
            indices.Add(sectionStartIndices[0] + i + 2);
            indices.Add(sectionStartIndices[0] + i + 1);
            indices.Add(sectionStartIndices[0]);
        }

        // Generate triangles for back face (clockwise winding when looking from outside)
        int backStartIndex = sectionStartIndices[sectionStartIndices.Length - 1];
        for (int i = 0; i < CrossSectionPointCount - 2; i++)
        {
            indices.Add(backStartIndex + i + 1);
            indices.Add(backStartIndex + i + 2);
            indices.Add(backStartIndex);
        }

        // Generate triangles for sides between each cross-section
        for (int section = 0; section < CrossSectionScales.Length - 1; section++)
        {
            int currentStart = sectionStartIndices[section];
            int nextStart = sectionStartIndices[section + 1];

            for (int i = 0; i < CrossSectionPointCount; i++)
            {
                int nextI = (i + 1) % CrossSectionPointCount;
                
                // Calculate side face normal
                Vector3 currentPoint = _crossSectionPoints[i];
                Vector3 nextPoint = _crossSectionPoints[nextI];
                Vector3 sideNormal = new Vector3(nextPoint.Y - currentPoint.Y, currentPoint.X - nextPoint.X, 0).Normalized();
                
                // Add vertices for side face with proper normals
                int sideStartIndex = vertices.Count;
                vertices.Add(vertices[currentStart + i]);
                vertices.Add(vertices[nextStart + i]);
                vertices.Add(vertices[currentStart + nextI]);
                vertices.Add(vertices[nextStart + nextI]);
                
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