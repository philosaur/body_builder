using Godot;
using System.Collections.Generic;

public partial class Body : Node3D
{
    [Export] public float CrossSectionRadius = 0.5f;
    [Export] public int CrossSectionPointCount = 12;
    [Export] public int CrossSectionCount = 6;
    [Export] public Vector3 BaseScale = new Vector3(0.5f, 0.5f, 0.5f);
    [Export] public Vector3 Size = new Vector3(3.0f, 3.0f, 3.0f);
    [Export] public Color AlbedoColor = new Color(1f, 1f, 1f);
    [Export] public float Roughness = .75f;
    [Export] public float Metallic = 0.4f;

    private Vector3 _primaryAnchor;
    private List<Vector3> _crossSectionPoints;
    private MeshInstance3D _meshInstance;
    private Vector3[] _crossSectionScales;

    public override void _Ready()
    {
        _meshInstance = new MeshInstance3D();
        AddChild(_meshInstance);
        RegenerateGeometry();
    }

    public void RegenerateGeometry() {
        GenerateCrossSectionScales();
        GenerateCrossSection();
        GenerateSkin();
    }

    private void GenerateCrossSectionScales()
    {
        _crossSectionScales = new Vector3[CrossSectionCount];
        float step = 1.0f / (CrossSectionCount - 1);
        
        for (int i = 0; i < CrossSectionCount; i++)
        {
            float t = i * step;
            float scale = Mathf.Sin(t * Mathf.Pi); // Smooth transition from 0 to 1 and back
            _crossSectionScales[i] = BaseScale * (scale + 0.3f);
        }
    }

    private void GenerateCrossSection()
    {
        _primaryAnchor = Vector3.Zero;
        _crossSectionPoints = new List<Vector3>();

        float angleStep = 2.0f * Mathf.Pi / CrossSectionPointCount;
        float offset = CrossSectionPointCount % 2 == 0 ? angleStep / 2 : 0; // Add offset for even number of points
        
        // Generate points around the primary anchor
        for (int i = 0; i < CrossSectionPointCount; i++)
        {
            float angle = i * angleStep - Mathf.Pi / 2.0f + offset; // Start from top (90 degrees)
			var radius = CrossSectionRadius * ((i - 1) % 3 == 0 ? 0.5f : 1.0f);
            float x = Mathf.Cos(angle) * radius;
            float y = Mathf.Sin(angle) * radius;
            _crossSectionPoints.Add(new Vector3(x, y, 0));
        }
    }

    private void GenerateSkin()
    {
        // Create vertices for all cross-sections
        var vertices = new List<Vector3>();
        var indices = new List<int>();
        var normals = new List<Vector3>();

        // Calculate total length scale for normalization
        float totalLengthScale = 0;
        foreach (var scale in _crossSectionScales)
        {
            totalLengthScale += scale.Z;
        }

        // Generate vertices for each cross-section
        int[] sectionStartIndices = new int[_crossSectionScales.Length];
        float currentZ = Size.Z / 2; // Start at front

        for (int section = 0; section < _crossSectionScales.Length; section++)
        {
            sectionStartIndices[section] = vertices.Count;
            Vector3 scale = _crossSectionScales[section];
            
            foreach (var point in _crossSectionPoints)
            {
                vertices.Add(new Vector3(
                    point.X * scale.X * Size.X,
                    point.Y * scale.Y * Size.Y,
                    currentZ
                ));
                normals.Add(Vector3.Forward); // Will be updated for side faces
            }

            // Update Z position for next section
            if (section < _crossSectionScales.Length - 1)
            {
                float sectionLength = (scale.Z / totalLengthScale) * Size.Z;
                currentZ -= sectionLength;
            }
        }

        // Calculate and set proper normals for front and rear faces
        // Front face normal calculation
        Vector3 frontCenter = Vector3.Zero;
        for (int i = 0; i < CrossSectionPointCount; i++)
        {
            frontCenter += vertices[sectionStartIndices[0] + i];
        }
        frontCenter /= CrossSectionPointCount;
        
        // Calculate front face normal using first three vertices
        Vector3 frontNormal = Vector3.Zero;
        for (int i = 0; i < CrossSectionPointCount - 2; i++)
        {
            Vector3 v1 = vertices[sectionStartIndices[0] + i + 1] - vertices[sectionStartIndices[0]];
            Vector3 v2 = vertices[sectionStartIndices[0] + i + 2] - vertices[sectionStartIndices[0]];
            frontNormal += v1.Cross(v2).Normalized();
        }
        frontNormal = frontNormal.Normalized();

        // Set front face normals
        for (int i = 0; i < CrossSectionPointCount; i++)
        {
            normals[sectionStartIndices[0] + i] = frontNormal;
        }

        // Rear face normal calculation
        Vector3 backCenter = Vector3.Zero;
        for (int i = 0; i < CrossSectionPointCount; i++)
        {
            backCenter += vertices[sectionStartIndices[sectionStartIndices.Length - 1] + i];
        }
        backCenter /= CrossSectionPointCount;

        // Calculate rear face normal using first three vertices
        Vector3 backNormal = Vector3.Zero;
        for (int i = 0; i < CrossSectionPointCount - 2; i++)
        {
            Vector3 v1 = vertices[sectionStartIndices[sectionStartIndices.Length - 1] + i + 1] - vertices[sectionStartIndices[sectionStartIndices.Length - 1]];
            Vector3 v2 = vertices[sectionStartIndices[sectionStartIndices.Length - 1] + i + 2] - vertices[sectionStartIndices[sectionStartIndices.Length - 1]];
            backNormal += v1.Cross(v2).Normalized();
        }
        backNormal = backNormal.Normalized();

        // Set rear face normals
        for (int i = 0; i < CrossSectionPointCount; i++)
        {
            normals[sectionStartIndices[sectionStartIndices.Length - 1] + i] = backNormal;
        }

        // Generate triangles for front face (clockwise winding when looking from outside)
        for (int i = 0; i < CrossSectionPointCount - 2; i++)
        {
            indices.Add(sectionStartIndices[0] + i + 1);
            indices.Add(sectionStartIndices[0] + i + 2);
            indices.Add(sectionStartIndices[0]);
        }

        // Generate triangles for back face (clockwise winding when looking from outside)
        int backStartIndex = sectionStartIndices[sectionStartIndices.Length - 1];
        for (int i = 0; i < CrossSectionPointCount - 2; i++)
        {
			indices.Add(backStartIndex + i + 0);
            indices.Add(backStartIndex + i + 1);
            indices.Add(backStartIndex + i + 2);
            
        }

        // Generate triangles for sides between each cross-section
        for (int section = 0; section < _crossSectionScales.Length - 1; section++)
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

        // Create arrays for the mesh
        var arrays = new Godot.Collections.Array();
        arrays.Resize((int)Mesh.ArrayType.Max);
        arrays[(int)Mesh.ArrayType.Vertex] = vertices.ToArray();
        arrays[(int)Mesh.ArrayType.Index] = indices.ToArray();
        arrays[(int)Mesh.ArrayType.Normal] = normals.ToArray();

        // Create the mesh
        var mesh = new ArrayMesh();
        mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);

        // Create material
        var material = new StandardMaterial3D
        {
            AlbedoColor = AlbedoColor,
            Roughness = Roughness,
            Metallic = Metallic
        };

        // Apply mesh and material
        _meshInstance.Mesh = mesh;
        _meshInstance.MaterialOverride = material;
    }
}