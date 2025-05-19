using Godot;
using System.Collections.Generic;
using System.Runtime;


public struct Section {
	public float xScale;
	public float yScale;
	public float extension;

	public Section(float xScale, float yScale, float extension) {
		this.xScale = xScale;
		this.yScale = yScale;
		this.extension = extension;
	}
}

public partial class Part2 : Node3D {
	
	[Export] public int radialSegments = 6;
    [Export] public Color AlbedoColor = new Color(1f, 1f, 1f);
    [Export] public float Roughness = 0.2f;
    [Export] public float Metallic = 0.2f;
    [Export] public float NormalDebugLength = 0.2f; // Length of normal debug lines

	public Section[] sections = new Section[] {
		new Section(0.20f, 0.10f, 0.10f),
		new Section(0.25f, 0.20f, 0.30f),
		new Section(0.30f, 0.30f, 0.30f),
		new Section(0.35f, 0.40f, 0.30f),
		new Section(0.40f, 0.50f, 0.30f),
		new Section(0.43f, 0.50f, 0.30f),
		new Section(0.46f, 0.50f, 0.30f),
		new Section(0.50f, 0.90f, 0.30f),
		new Section(0.50f, 0.90f, 0.30f),
		new Section(0.50f, 0.50f, 0.30f),
		new Section(0.50f, 0.50f, 0.30f),
		new Section(0.40f, 0.90f, 0.30f),
		new Section(0.40f, 0.90f, 0.30f),
		new Section(0.30f, 0.50f, 0.20f),
		new Section(0.20f, 0.40f, 0.10f)
	};
    
    private MeshInstance3D _meshInstance;
    private MeshInstance3D _normalDebugMesh;
    private SurfaceTool st;

	public override void _Ready() {
		_meshInstance = new MeshInstance3D();
		AddChild(_meshInstance);

		st = new SurfaceTool();
		st.Begin(Mesh.PrimitiveType.Triangles);

		float angle = Mathf.Pi * 2 / radialSegments;
		float currentExtension = 0;

		currentExtension = createFrontCap(angle, currentExtension);		
		currentExtension = createRings(angle, currentExtension);
		currentExtension =createRearCap(angle, currentExtension);

		

		st.Index();
		st.Deindex();
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
		// move the mesh forward by half the currentExtension
		_meshInstance.Translate(new Vector3(0, 0, -currentExtension / 2));

		// Create normal debug visualization
		// CreateNormalDebugVisualization(mesh);
	}

	private void createVertex(float x, float y, float z) {
		st.SetColor(AlbedoColor);
		st.SetUV(new Vector2(0, 0));
		st.AddVertex(new Vector3(x, y, z));
	}

	private float createFrontCap(float angle, float currentExtension) {
		// ********************************************
		// create front cap
		// ********************************************		
		for (int i = 0; i < radialSegments; i++) {
			createVertex(0, 0, 0);
			createVertex(
				Mathf.Cos(angle * i) * sections[0].xScale, 
				Mathf.Sin(angle * i) * sections[0].yScale, 
				sections[0].extension
			);
			createVertex(
				Mathf.Cos(angle * (i + 1)) * sections[0].xScale, 
				Mathf.Sin(angle * (i + 1)) * sections[0].yScale, 
				sections[0].extension
			);
		}
		return currentExtension + sections[0].extension;
	}

	private float createRearCap(float angle, float currentExtension) {
		for (int i = 0; i < radialSegments; i++) {
			createVertex(0, 0, currentExtension + sections[sections.Length - 1].extension);
			createVertex(
				Mathf.Cos(angle * (i + 1)) * sections[sections.Length - 1].xScale, 
				Mathf.Sin(angle * (i + 1)) * sections[sections.Length - 1].yScale, 
				currentExtension
			);
			createVertex(
				Mathf.Cos(angle * i) * sections[sections.Length - 1].xScale, 
				Mathf.Sin(angle * i) * sections[sections.Length - 1].yScale, 
				currentExtension
			);
		}
		return currentExtension + sections[sections.Length - 1].extension;
	}

	private float createRings(float angle, float currentExtension) {
		for (int ring = 0; ring < sections.Length - 1; ring++) {

			Section frontSegment = sections[ring];
			Section rearSegment = sections[ring + 1];
			
			for (int i = 0; i < radialSegments; i++) {

				float leftAngle = angle * i;
				float rightAngle = angle * (i + 1);

				float leftX = Mathf.Cos(leftAngle);
				float leftY = Mathf.Sin(leftAngle);

				float rightX = Mathf.Cos(rightAngle);
				float rightY = Mathf.Sin(rightAngle);

				float frontExtension = currentExtension;
				float rearExtension = currentExtension + frontSegment.extension;

				// create a quad for each radial segment composed of 2 triangles

				// b -> c	-- rear extension
				// |  /
				// a		-- front extension

				createVertex( // a
					leftX * frontSegment.xScale, 
					leftY * frontSegment.yScale, 
					frontExtension
				);
				createVertex( // b
					leftX * rearSegment.xScale, 
					leftY * rearSegment.yScale, 
					rearExtension
				);
				createVertex( // c
					rightX * rearSegment.xScale, 
					rightY * rearSegment.yScale, 
					rearExtension
				);

				//      e	-- rear extension
				//   /  |
				// d <- f	-- front extension

				createVertex( // d
					leftX * frontSegment.xScale, 
					leftY * frontSegment.yScale, 
					frontExtension
				);
				createVertex( // e
					rightX * rearSegment.xScale, 
					rightY * rearSegment.yScale, 
					rearExtension
				);
				createVertex( // f
					rightX * frontSegment.xScale, 
					rightY * frontSegment.yScale, 
					frontExtension
				);
			}
			// decrement totalExtension
			currentExtension += frontSegment.extension;
		}
		return currentExtension;
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

	

}