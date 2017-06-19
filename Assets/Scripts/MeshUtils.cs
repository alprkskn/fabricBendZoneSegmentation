using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshUtils
{

	// Separate the given bounds from the worldMatrix
	// Apply something  similar to marching cubes on it
	// return the generated Mesh
	// http://www.cs.carleton.edu/cs_comps/0405/shape/marching_cubes.html
	public static List<Mesh> CreateSegmentMesh(int[,,] worldMatrix, Bounds segmentBounds, Vector3 offset)
	{
		List<Mesh> meshList = new List<Mesh>();

		List<Vector3> vertices = new List<Vector3>();
		List<Vector3> normals = new List<Vector3>();
		List<int> tris = new List<int>();
		List<Vector2> uvs = new List<Vector2>();


		var offsetMax = segmentBounds.max + offset;
		var offsetMin = segmentBounds.min + offset;

		for(int i = (int)offsetMin.x; i < (int)offsetMax.x; i++)
		{
			for(int j = (int)offsetMin.y; j < (int)offsetMax.y; j++)
			{
				for(int k = (int)offsetMin.z; k < (int)offsetMax.z; k++)
				{
					// Check up, down, right, left, forward, back of the cube. And add faces if necessary.
					if((j + 1) >= offsetMax.y || worldMatrix[i, j + 1, k] == 0)
					{
						// Use Add faces method, send all needed arguments.
					}
					if((j - 1) < offsetMin.y || worldMatrix[i, j - 1, k] == 0)
					{
						// Use Add faces method, send all needed arguments.
					}
					if((i + 1) >= offsetMax.x || worldMatrix[i + 1, j, k] == 0)
					{
						// Use Add faces method, send all needed arguments.
					}
					if((i - 1) < offsetMin.x || worldMatrix[i - 1, j, k] == 0)
					{
						// Use Add faces method, send all needed arguments.
					}
					if((k + 1) >= offsetMax.z || worldMatrix[i, j, k + 1] == 0)
					{
						// Use Add faces method, send all needed arguments.
					}
					if((k - 1) < offsetMin.z || worldMatrix[i, j, k - 1] == 0)
					{
						// Use Add faces method, send all needed arguments.
					}
				}

			}

		}

		return meshList;
	}

	private static void addFace(Vector3 center, Vector3 size, Vector3 direction, 
								ref List<Vector3> vertices, ref List<Vector3> normal, 
								ref List<Vector2> uvs, ref List<int> tris, ref List<Mesh> meshList)
	{
		// Populate the lists with new faces. If necessary divide and add the added mesh to the mesh list.

	}
}
