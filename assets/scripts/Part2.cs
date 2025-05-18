using Godot;
using System.Collections.Generic;

public partial class Part2 : Node3D {
	
	[Export] public int radialSegments = 6;
	[Export] public int rings = 3;
    [Export] public Color AlbedoColor = new Color(1f, 1f, 1f);
    [Export] public float Roughness = .75f;
    [Export] public float Metallic = 0.4f;
    [Export] public float NormalDebugLength = 0.2f; // Length of normal debug lines
    
    private MeshInstance3D _meshInstance;
    private MeshInstance3D _normalDebugMesh;
    private SurfaceTool st;

	public override void _Ready() {
		_meshInstance = new MeshInstance3D();
		AddChild(_meshInstance);

		st = new SurfaceTool();
		st.Begin(Mesh.PrimitiveType.Triangles);





		// create front faces
		float angle = Mathf.Pi * 2 / radialSegments;
		float radius = 1f;
		float extension = .5f;
		float totalExtension = 0;

		// ********************************************
		// create front face/cone
		// ********************************************

		// decrement totalExtension
		totalExtension -= extension;
		for (int i = 0; i < radialSegments; i++) {
			createVertex(0, 0, 0);
			createVertex(
				Mathf.Cos(angle * (i + 1)) * radius, 
				Mathf.Sin(angle * (i + 1)) * radius, 
				totalExtension
			);
			createVertex(
				Mathf.Cos(angle * i) * radius, 
				Mathf.Sin(angle * i) * radius, 
				totalExtension
			);
		}


		// ********************************************
		// create rings
		// ********************************************

		for (int ring = 0; ring < rings; ring++) {
			
			for (int i = 0; i < radialSegments; i++) {

				float lagAngle = angle * i;
				float leadAngle = angle * (i + 1);
				float cosLag = Mathf.Cos(lagAngle) * radius;
				float sinLag = Mathf.Sin(lagAngle) * radius;
				float cosLead = Mathf.Cos(leadAngle) * radius;
				float sinLead = Mathf.Sin(leadAngle) * radius;

				// create a quad for each radial segment composed of 2 triangles

				// c <- b
				// |  /
				// a

				createVertex(cosLag, sinLag, totalExtension); // a
				createVertex(cosLead, sinLead, totalExtension - extension); // b
				createVertex(cosLag, sinLag, totalExtension - extension); // c

				//      f
				//   /  |
				// d -> e

				createVertex(cosLag, sinLag, totalExtension); // d
				createVertex(cosLead, sinLead, totalExtension); // e
				createVertex(cosLead, sinLead, totalExtension - extension); // f
				
				
			}
			// decrement totalExtension
			totalExtension -= extension;
		}

		// ********************************************
		// create rear faces
		// ********************************************

		for (int i = 0; i < radialSegments; i++) {
			createVertex(
				Mathf.Cos(angle * i) * radius, 
				Mathf.Sin(angle * i) * radius, 
				totalExtension
			);
			createVertex(
				Mathf.Cos(angle * (i + 1)) * radius, 
				Mathf.Sin(angle * (i + 1)) * radius, 
				totalExtension
			);
			createVertex(0, 0, totalExtension - extension);
		}




		st.Index();
		st.GenerateNormals();
		st.GenerateTangents();
		ArrayMesh mesh = st.Commit();

		// Create material
		var material = new StandardMaterial3D {
			AlbedoColor = AlbedoColor,
			Roughness = Roughness,
			Metallic = Metallic
		};

		// Apply mesh and material
		_meshInstance.Mesh = mesh;
		_meshInstance.MaterialOverride = material;
		
		// Create normal debug visualization
		CreateNormalDebugVisualization(mesh);
		
		// Add debug visualization
		// GD.Print($"Mesh created with {vertices.Count} vertices and {indices.Count/3} triangles");
		GD.Print($"Mesh bounds: {mesh.GetAabb()}");
	}

	private void CreateNormalDebugVisualization(ArrayMesh mesh) {
		_normalDebugMesh = new MeshInstance3D();
		AddChild(_normalDebugMesh);

		var immediateMesh = new ImmediateMesh();
		var debugMaterial = new StandardMaterial3D {
			AlbedoColor = new Color(1, 0, 0, 0.8f), // Red color for normals with alpha
			NoDepthTest = true, // Make sure lines are always visible
			Transparency = BaseMaterial3D.TransparencyEnum.Alpha
		};

		immediateMesh.ClearSurfaces();
		immediateMesh.SurfaceBegin(Mesh.PrimitiveType.Lines);

		// Get vertex positions and normals from the mesh
		var arrays = mesh.SurfaceGetArrays(0);
		var vertices = (Vector3[])arrays[(int)Mesh.ArrayType.Vertex];
		var normals = (Vector3[])arrays[(int)Mesh.ArrayType.Normal];

		// Draw a line for each normal
		for (int i = 0; i < vertices.Length; i++) {
			immediateMesh.SurfaceSetColor(new Color(1, 0, 0));
			immediateMesh.SurfaceAddVertex(vertices[i]);
			immediateMesh.SurfaceAddVertex(vertices[i] + normals[i] * NormalDebugLength);
		}

		immediateMesh.SurfaceEnd();
		_normalDebugMesh.Mesh = immediateMesh;
		_normalDebugMesh.MaterialOverride = debugMaterial;
	}

	private void createVertex(float x, float y, float z) {
		st.SetColor(AlbedoColor);
		st.SetUV(new Vector2(0, 0));
		st.AddVertex(new Vector3(x, y, z));
	}

}