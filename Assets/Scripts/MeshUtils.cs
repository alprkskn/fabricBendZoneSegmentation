using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshUtils
{

	// Separate the given bounds from the worldMatrix
	// Apply something  similar to marching cubes on it
	// return the generated Mesh
	// http://www.cs.carleton.edu/cs_comps/0405/shape/marching_cubes.html
	public static Mesh CreateSegmentMesh(Vector3[,,] worldMatrix, Bounds segmentBounds)
	{
		Mesh mesh = new Mesh();

		

		return mesh;
	}
}
