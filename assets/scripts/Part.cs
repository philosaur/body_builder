using Godot;
using System.Collections.Generic;

public partial class Part : Node3D {
	
    [Export] public Color AlbedoColor = new Color(1f, 1f, 1f);
    [Export] public float Roughness = .75f;
    [Export] public float Metallic = 0.4f;
    
    private MeshInstance3D _meshInstance;
    

	public override void _Ready() {
		_meshInstance = new MeshInstance3D();
		AddChild(_meshInstance);

		// Create vertices for all cross-sections
		var vertices = new List<Vector3>();
		var indices = new List<int>();
		var normals = new List<Vector3>();

		// First face (front)
		vertices.Add(new Vector3(0, 0, 0));      // 0
		vertices.Add(new Vector3(0, 1, 0));      // 1
		vertices.Add(new Vector3(-1, 0, 0));     // 2
		vertices.Add(new Vector3(0, -1, 0));     // 3
		vertices.Add(new Vector3(1, 0, 0));      // 4

		// Add indices for first face
		indices.Add(0);
		indices.Add(1);
		indices.Add(2);
		indices.Add(0);
		indices.Add(2);
		indices.Add(3);
		indices.Add(0);
		indices.Add(3);
		indices.Add(4);
		indices.Add(0);
		indices.Add(4);
		indices.Add(1);

		// Add normals for first face (all pointing forward)
		for (int i = 0; i < 5; i++) {
			normals.Add(Vector3.Forward);
		}

		// Second face (top)
		// vertices.Add(new Vector3(0, 1, 0));      // 5 (shared position with vertex 1)
		// vertices.Add(new Vector3(0, 1, -1));     // 6
		// vertices.Add(new Vector3(-1, 0, -1));    // 7
		// vertices.Add(new Vector3(-1, 0, 0));    // 8
		// // Add indices for second face
		// indices.Add(5);
		// indices.Add(6);
		// indices.Add(7);
		// indices.Add(5);
		// indices.Add(7);
		// indices.Add(8);


		// Calculate and add normals for second face
		// Vector3 v1 = vertices[6] - vertices[5];
		// Vector3 v2 = vertices[7] - vertices[5];
		// Vector3 normal = v1.Cross(v2).Normalized();
		// normals.Add(normal);
		// normals.Add(normal);
		// normals.Add(normal);

		// Third face
		// v1 = vertices[7] - vertices[5];
		// v2 = vertices[8] - vertices[5];
		// normal = v1.Cross(v2).Normalized();
		// normals.Add(normal);
		// normals.Add(normal);
		// normals.Add(normal);
		// vertices.Add(new Vector3(0, -1, -1));
		// vertices.Add(new Vector3(1, 0, 1));
		// vertices.Add(new Vector3(1, 0, -1));
		


		var arrays = new Godot.Collections.Array();
		arrays.Resize((int)Mesh.ArrayType.Max);
		arrays[(int)Mesh.ArrayType.Vertex] = vertices.ToArray();
		arrays[(int)Mesh.ArrayType.Index] = indices.ToArray();
		arrays[(int)Mesh.ArrayType.Normal] = normals.ToArray();

		// Create the mesh
		var mesh = new ArrayMesh();
		mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);

		// Create material
		var material = new StandardMaterial3D {
		AlbedoColor = AlbedoColor,
		Roughness = Roughness,
		Metallic = Metallic
		};

		// Apply mesh and material
		_meshInstance.Mesh = mesh;
		_meshInstance.MaterialOverride = material;
		
		// Add debug visualization
		GD.Print($"Mesh created with {vertices.Count} vertices and {indices.Count/3} triangles");
		GD.Print($"Mesh bounds: {mesh.GetAabb()}");
	}


}