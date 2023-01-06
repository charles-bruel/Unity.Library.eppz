using EPPZ.Geometry.AddOns;
using TriangleNet.Geometry;
using TriangleNet.Meshing;
using UnityEngine;
using System.Collections.Generic;
using TriangleNet.Meshing.Algorithm;

//My custom extensions for EPPZ
//They are here so they can access internal fields and methods
public static class EPPZExtensions
{
        //Based on Mesh() in EPPZ.Geometry.AddOns.UnityEngineAddOns
    //Modified so that I can reuse meshes
    public static Mesh Mesh(this EPPZ.Geometry.Model.Polygon this_, Mesh prevOrNull, Color color, TriangulatorType triangulator, string name) {
		TriangleNet.Geometry.Polygon polygon = this_.TriangleNetPolygon();

		// Triangulate.
		ConstraintOptions options = new ConstraintOptions();
		// ConformingDelaunay
		// Convex
		// SegmentSplitting
    	QualityOptions quality = new QualityOptions();
		// MinimumAngle
		// MaximumArea
		// UserTest
		// VariableArea
		// SteinerPoints
		IMesh triangulatedMesh = polygon.Triangulate(options, quality, TriangulatorForType(triangulator));

		// Counts.
		int vertexCount = triangulatedMesh.Vertices.Count;
		int triangleCount = triangulatedMesh.Triangles.Count;

		// Mesh store.
		Vector3[] _vertices = new Vector3[vertexCount];
		Vector2[] _uv = new Vector2[vertexCount];
		Vector3[] _normals = new Vector3[vertexCount];
		Color[] _colors = new Color[vertexCount];
		List<int> _triangles = new List<int>(); // Size may vary

		// Vertices.
		int index = 0;
		foreach (TriangleNet.Geometry.Vertex eachVertex in triangulatedMesh.Vertices)
		{
			_vertices[index] = new Vector3(
				(float)eachVertex.x,
				(float)eachVertex.y,
				0.0f // As of 2D
			);

			_uv[index] = _vertices[index];
			_normals[index] = Vector3.forward;
			_colors[index] = color;

			index++;
		}

		// Triangles.
		foreach (TriangleNet.Topology.Triangle eachTriangle in triangulatedMesh.Triangles)
		{
			// Get vertices.
			Point P2 = eachTriangle.GetVertex(2);
			Point P1 = eachTriangle.GetVertex(1);
			Point P0 = eachTriangle.GetVertex(0);
            
			_triangles.Add(P2.ID);
			_triangles.Add(P1.ID);
			_triangles.Add(P0.ID);
		}

		// Create / setup mesh.
		Mesh mesh;
		if(prevOrNull == null) {
			mesh = new Mesh();
			mesh.MarkDynamic();
		} else {
			mesh = prevOrNull;
			mesh.Clear();
		}

		mesh.vertices = _vertices;
		mesh.uv = _uv;
		mesh.normals = _normals;
		mesh.colors = _colors;
		mesh.subMeshCount = 1;
		mesh.SetTriangles(_triangles.ToArray(), 0);
		mesh.name = name;

		return mesh;
    }

    private static ITriangulator TriangulatorForType(TriangulatorType triangulator)
	{
		switch (triangulator)
		{
			case TriangulatorType.Incremental : return new Incremental();
			case TriangulatorType.Dwyer : return new Dwyer();
			case TriangulatorType.SweepLine : return new SweepLine();
			default : return new Dwyer();				
		}
	}
}
