using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshUtils
{

	// Separate the given bounds from the worldMatrix
	// Apply something  similar to marching cubes on it
	// return the generated Mesh
	// Assuming the segment bounds represent the normalized bounds within the worldMatrix indices.
	public static List<Mesh> CreateSegmentMesh(int[,,] worldMatrix, Bounds segmentBounds, Vector3 voxelSize)
	{
		List<Mesh> meshList = new List<Mesh>();

		List<Vector3> vertices = new List<Vector3>();
		List<Vector3> normals = new List<Vector3>();
		List<int> tris = new List<int>();
		List<Vector2> uvs = new List<Vector2>();


		var max = segmentBounds.max;
		var min = segmentBounds.min;

		var vertexOffset = - segmentBounds.extents;

		for(int i = (int)min.x; i < (int)max.x; i++)
		{
			for(int j = (int)min.y; j < (int)max.y; j++)
			{
				for(int k = (int)min.z; k < (int)max.z; k++)
				{
					if (worldMatrix[i, j, k] == 0) continue;

					Vector3 center = new Vector3(voxelSize.x * (0.5f + i), voxelSize.y * (0.5f + j), voxelSize.z * (0.5f + k));

					// Check up, down, right, left, forward, back of the cube. And add faces if necessary.
					if((j + 1) >= max.y || worldMatrix[i, j + 1, k] == 0)
					{
						// Use Add faces method, send all needed arguments. UP
						addFace(center, voxelSize, Vector3.up, Vector3.right, ref vertices, ref normals, ref uvs, ref tris, ref meshList);
					}
					if((j - 1) < min.y || worldMatrix[i, j - 1, k] == 0)
					{
						// Use Add faces method, send all needed arguments. DOWN
						addFace(center, voxelSize, Vector3.down, Vector3.left, ref vertices, ref normals, ref uvs, ref tris, ref meshList);
					}
					if((i + 1) >= max.x || worldMatrix[i + 1, j, k] == 0)
					{
						// Use Add faces method, send all needed arguments. RIGHT
						addFace(center, voxelSize, Vector3.right, Vector3.down, ref vertices, ref normals, ref uvs, ref tris, ref meshList);
					}
					if((i - 1) < min.x || worldMatrix[i - 1, j, k] == 0)
					{
						// Use Add faces method, send all needed arguments. LEFT
						addFace(center, voxelSize, Vector3.left, Vector3.up, ref vertices, ref normals, ref uvs, ref tris, ref meshList);
					}
					if((k + 1) >= max.z || worldMatrix[i, j, k + 1] == 0)
					{
						// Use Add faces method, send all needed arguments. FORWARD
						addFace(center, voxelSize, Vector3.forward, Vector3.down, ref vertices, ref normals, ref uvs, ref tris, ref meshList);
					}
					if((k - 1) < min.z || worldMatrix[i, j, k - 1] == 0)
					{
						// Use Add faces method, send all needed arguments. BACK
						addFace(center, voxelSize, Vector3.back, Vector3.up, ref vertices, ref normals, ref uvs, ref tris, ref meshList);
					}
				}
			}
		}

		// Convert the residue lists into a mesh
		Mesh mesh = new Mesh();
		mesh.vertices = vertices.ToArray();
		mesh.triangles = tris.ToArray();
		mesh.normals = normals.ToArray();
		mesh.uv = uvs.ToArray();
		meshList.Add(mesh);

		return meshList;
	}

	private static void addFace(Vector3 center, Vector3 size, Vector3 up, Vector3 right, 
								ref List<Vector3> vertices, ref List<Vector3> normals, 
								ref List<Vector2> uvs, ref List<int> tris, ref List<Mesh> meshList)
	{
		// Populate the lists with new faces. If necessary divide and add the added mesh to the mesh list.
		// If mesh max vertexCount is reached. Create a mesh and clear the lists.
		if(vertices.Count + 4 >= 65000)
		{
			Mesh mesh = new Mesh();
			mesh.vertices = vertices.ToArray();
			mesh.triangles = tris.ToArray();
			mesh.normals = normals.ToArray();
			mesh.uv = uvs.ToArray();
			meshList.Add(mesh);

			vertices.Clear();
			normals.Clear();
			tris.Clear();
			uvs.Clear();
		}

		var extents = size * 0.5f;
		var forward = Vector3.Cross(up, right);
		forward.Normalize();
		int cursor = vertices.Count;

		vertices.Add(new Vector3(center.x + (up.x - right.x - forward.x) * extents.x, center.y + (up.y - right.y - forward.y) * extents.y, center.z + (up.z - right.z - forward.z) * extents.z));
		vertices.Add(new Vector3(center.x + (up.x - right.x + forward.x) * extents.x, center.y + (up.y - right.y + forward.y) * extents.y, center.z + (up.z - right.z + forward.z) * extents.z));
		vertices.Add(new Vector3(center.x + (up.x + right.x + forward.x) * extents.x, center.y + (up.y + right.y + forward.y) * extents.y, center.z + (up.z + right.z + forward.z) * extents.z));
		vertices.Add(new Vector3(center.x + (up.x + right.x - forward.x) * extents.x, center.y + (up.y + right.y - forward.y) * extents.y, center.z + (up.z + right.z - forward.z) * extents.z));

		normals.Add(up);
		normals.Add(up);
		normals.Add(up);
		normals.Add(up);

		uvs.Add(new Vector2(0,0));
		uvs.Add(new Vector2(0,1));
		uvs.Add(new Vector2(1,1));
		uvs.Add(new Vector2(1,0));

		tris.AddRange(new List<int>() { cursor + 0, cursor + 2, cursor + 1, cursor + 2, cursor + 0, cursor + 3});
	}
}
