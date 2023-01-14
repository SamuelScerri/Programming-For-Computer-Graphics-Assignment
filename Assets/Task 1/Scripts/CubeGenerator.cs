using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeGenerator
{
	public static Mesh GenerateCubeData(byte height, byte width)
	{
		Vector3 offset = new Vector3(width / 2, 0, width / 2);

		Vector3[] vertices =
		{
			new Vector3(0, 0, 0) - offset,
			new Vector3(width, 0, 0) - offset,
			new Vector3(width, height, 0) - offset,
			new Vector3(0, height, 0) - offset,
			new Vector3(0, height, width) - offset,
			new Vector3(width, height, width) - offset,
			new Vector3(width, 0, width) - offset,
			new Vector3(0, 0, width) - offset,
		};

		int[] triangles =
		{
			0, 2, 1,
			0, 3, 2,
			2, 3, 4,
			2, 4, 5,
			1, 2, 5,
			1, 5, 6,
			0, 7, 4,
			0, 4, 3,
			5, 4, 7,
			5, 7, 6,
			0, 6, 7,
			0, 1, 6
		};

		Mesh newMesh = new Mesh();
		newMesh.vertices = vertices;
		newMesh.triangles = triangles;
		newMesh.RecalculateNormals();
		newMesh.Optimize();

		return newMesh;
	}
}
