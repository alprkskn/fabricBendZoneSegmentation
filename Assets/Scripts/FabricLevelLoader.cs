using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using UnityEngine;
using Random = UnityEngine.Random;

public class FabricLevelLoader : MonoBehaviour
{
	public string LevelDir;
	public Material _dummyMaterial;

	private int[,,] _world;
	private Vector3[] _bendPoints;
	private Bounds _levelBounds;


	private List<Bounds> _segmentationBounds;

	private void loadLevel(string path)
	{
		path = Path.Combine(path, "level.xml");

		XmlDocument doc = new XmlDocument();
		doc.Load(path);

		var node = doc.SelectSingleNode("/level/tiles");

		List<Vector3> newBendPoints = new List<Vector3>();

		List<int> blockComponents = new List<int>();
		bool firstNode = true;
		foreach(XmlNode nd in node.ChildNodes)
		{
			if (nd == null) continue;

			var type = nd.Attributes["type"].Value;
			var positionComponents = nd.Attributes["position"].Value.Split().Select(x => int.Parse(x)).ToArray();

			blockComponents.Add(positionComponents[0]);
			blockComponents.Add(positionComponents[1]);
			blockComponents.Add(positionComponents[2]);

			var position = new Vector3(positionComponents[0],
										positionComponents[1],
										positionComponents[2]);

			if (firstNode)
			{
				_levelBounds = new Bounds(position, Vector3.zero);
				firstNode = false;
			}
			else
			{
				_levelBounds.Encapsulate(position);
			}

			if (type == "BendTile")
			{
				newBendPoints.Add(position);
			}
		}
		var size = _levelBounds.size;

		_world = new int[(int)size.x+1, (int)size.y+1, (int)size.z+1];

		var offsetX = -(int)_levelBounds.min.x;
		var offsetY = -(int)_levelBounds.min.y;
		var offsetZ = -(int)_levelBounds.min.z;

		for(int i = 0; i < blockComponents.Count; i += 3)
		{
			try
			{
				_world[blockComponents[i] + offsetX, blockComponents[i + 1] + offsetY, blockComponents[i + 2] + offsetZ] = 1;
			}
			catch(IndexOutOfRangeException e)
			{
				Debug.LogError(e.Message);
				Debug.LogErrorFormat("i: {0}, blockComponents.Count: {1}", i, blockComponents.Count);
				Debug.LogErrorFormat("_world dimensions are ({0},{1},{2}), given indices are ({3},{4},{5})",
										(int)size.x, (int)size.y, (int)size.z,
										blockComponents[i+0] + offsetX, blockComponents[i+1] + offsetY, blockComponents[i+2] + offsetZ);
			}
		}

		_bendPoints = newBendPoints.ToArray();
		Debug.LogFormat("Found {0} bendTiles.", newBendPoints.Count);
	}

	List<Bounds> levelSegmentation(Vector3[] bendPoints, Bounds levelBounds)
	{
		Vector3 minBounds, maxBounds;

		minBounds = levelBounds.min;
		maxBounds = levelBounds.max;

		uint[,,] world = new uint[(int)(maxBounds.x - minBounds.x), (int)(maxBounds.y - minBounds.y), (int)(maxBounds.z - minBounds.z)];

		var offset = -minBounds;
		maxBounds += offset;

		List<int[]> matchingPoints = new List<int[]>();

		var normalizedBendPoints = bendPoints.Select(x => x + offset).ToList();

		// Find Matching Points on the 3d matrix
		for(int i = 0; i < bendPoints.Length-1; i++)
		{
			for(int j = i+1; j < bendPoints.Length; j++)
			{
				var sum = 0;
				if ((int)bendPoints[i].x == (int)bendPoints[j].x) sum += 1;
				if ((int)bendPoints[i].y == (int)bendPoints[j].y) sum += 1;
				if ((int)bendPoints[i].z == (int)bendPoints[j].z) sum += 1;

				if (sum == 2) matchingPoints.Add(new int[2] { i, j });
			}
		}
		///////////////////////////////////////

		// Increment the volumes defined by each matching points tuple with a power of 2.
		// NOTE: To avoid overflow, we can change the identification method. For now we use
		// unsigned int to keep it simple.
		uint increment = 1;
		foreach(var mp in matchingPoints)
		{
			int minX = ((normalizedBendPoints[mp[0]].x - normalizedBendPoints[mp[1]].x) == 0) ? 0 : (int)Mathf.Min(normalizedBendPoints[mp[0]].x, normalizedBendPoints[mp[1]].x) + 1;
			int minY = ((normalizedBendPoints[mp[0]].y - normalizedBendPoints[mp[1]].y) == 0) ? 0 : (int)Mathf.Min(normalizedBendPoints[mp[0]].y, normalizedBendPoints[mp[1]].y) + 1;
			int minZ = ((normalizedBendPoints[mp[0]].z - normalizedBendPoints[mp[1]].z) == 0) ? 0 : (int)Mathf.Min(normalizedBendPoints[mp[0]].z, normalizedBendPoints[mp[1]].z) + 1;

			int maxX = ((normalizedBendPoints[mp[0]].x - normalizedBendPoints[mp[1]].x) == 0) ? (int)maxBounds.x : (int)Mathf.Max(normalizedBendPoints[mp[0]].x, normalizedBendPoints[mp[1]].x);
			int maxY = ((normalizedBendPoints[mp[0]].y - normalizedBendPoints[mp[1]].y) == 0) ? (int)maxBounds.y : (int)Mathf.Max(normalizedBendPoints[mp[0]].y, normalizedBendPoints[mp[1]].y);
			int maxZ = ((normalizedBendPoints[mp[0]].z - normalizedBendPoints[mp[1]].z) == 0) ? (int)maxBounds.z : (int)Mathf.Max(normalizedBendPoints[mp[0]].z, normalizedBendPoints[mp[1]].z);

			for(int i = minX; i < maxX; i++)
				for(int j = minY; j < maxY; j++)
					for(int k = minZ; k < maxZ; k++)
					{
						world[i, j, k] += increment;
					}

			increment *= 2;
		}
		//////////////////////////////////////////////////////////////////////////////////

		// Now we have to extract the bounds out of the matrix.
		List<Bounds> bounds = new List<Bounds>();
		Stack<Vector3> stack = new Stack<Vector3>();
		HashSet<Vector3> visited = new HashSet<Vector3>();

		stack.Push(Vector3.zero);

		while(stack.Count > 0)
		{
			var min = stack.Pop();
			var cursor = min;

			if (!insideBounds(min, minBounds, maxBounds) || visited.Contains(min)) continue;

			visited.Add(min);

			var value = world[(int)min.x, (int)min.y, (int)min.z];

			while (insideBounds(cursor, minBounds, maxBounds) && world[(int)cursor.x, (int)cursor.y, (int)cursor.z] == value) cursor.x++;
			cursor.x--;
			while (insideBounds(cursor, minBounds, maxBounds) && world[(int)cursor.x, (int)cursor.y, (int)cursor.z] == value) cursor.y++;
			cursor.y--;
			while (insideBounds(cursor, minBounds, maxBounds) && world[(int)cursor.x, (int)cursor.y, (int)cursor.z] == value) cursor.z++;
			cursor.z--;

			cursor += Vector3.one;
			var bound = new Bounds();
			bound.SetMinMax(min - offset, cursor - offset + Vector3.one);
			bounds.Add(bound);


			stack.Push(new Vector3(cursor.x, min.y, min.z));
			stack.Push(new Vector3(min.x, cursor.y, min.z));
			stack.Push(new Vector3(min.x, min.y, cursor.z));
		}
		//////////////////////////////////////////////////////

		return bounds;
	}

	private bool insideBounds(Vector3 cursor, Vector3 MinBounds, Vector3 MaxBounds)
	{
		return ((int)cursor.x < MaxBounds.x && (int)cursor.y < MaxBounds.y && (int)cursor.z < MaxBounds.z);
	}

	void Start()
	{
		if(LevelDir != "")
		{
			loadLevel(LevelDir);
			_segmentationBounds = levelSegmentation(_bendPoints, _levelBounds);

			foreach(var bnd in _segmentationBounds)
			{
				var normalizedBounds = bnd;
				normalizedBounds.center -= normalizedBounds.min;
				var meshes = MeshUtils.CreateSegmentMesh(_world, normalizedBounds, Vector3.one);

				foreach (var m in meshes)
				{
					var go = new GameObject("segment");
					var mf = go.AddComponent<MeshFilter>();
					var mr = go.AddComponent<MeshRenderer>();

					go.transform.position = bnd.min;

					mf.sharedMesh = m;
					mr.sharedMaterial = _dummyMaterial;
				}

			}
		}
	}

	private void OnDrawGizmos()
	{
		if(_segmentationBounds != null)
		{
			Random.InitState(5);

			foreach (var b in _segmentationBounds)
			{
				Gizmos.color = Random.ColorHSV();
				Gizmos.DrawWireCube(b.center, b.size);
			}
		}
	}
}
