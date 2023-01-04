using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
	Reference: https://www.youtube.com/watch?v=vFvwyu_ZKfU
*/

[System.Serializable]
struct GeneralSettings
{
	public bool generate;
	public float threshold;
}

public class TerrainGenerator : MonoBehaviour
{
	[SerializeField]
	private byte _terrainScale;

	[SerializeField]
	private GeneralSettings _grassSettings;

	[SerializeField]
	private GeneralSettings _treeSettings;

	[SerializeField]
	private GeneralSettings _pathSettings;

	private Terrain _terrain;

	private void Start()
	{
		//The Terrain Will Be Generated Using A Procedurally Generated Terrain Data Object
		_terrain = GetComponent<Terrain>();

		//We Generate The Terrain With A Random Seed, This Seed Will Be Used So That The Terrain Generated Will Always Be Different In The Same Position
		CreateProceduralTerrain(_terrain.terrainData, (byte) Random.Range(0, 255));

		//We Then Generate Grass, This Will Also Use Perlin Noise
		if (_grassSettings.generate)
			CreateProceduralGrass(_terrain.terrainData, _grassSettings.threshold);

		if (_pathSettings.generate)
			CreateProceduralPath(_terrain.terrainData, _pathSettings.threshold);
	}

	private void CreateProceduralTerrain(TerrainData terrainData, byte seed)
	{
		List<TreeInstance> trees = new List<TreeInstance>();
		float[,] heightmap = new float[terrainData.heightmapResolution, terrainData.heightmapResolution];

		for (int x = 0; x < terrainData.heightmapResolution; x++)
			for (int y = 0; y < terrainData.heightmapResolution; y++)
			{
				heightmap[x, y] = GenerateHeightmap(x, y, terrainData, seed);

				if (_treeSettings.generate)
				{
					TreeInstance newTree = new TreeInstance();

					float normalizedX = x * 1.0f / (terrainData.heightmapResolution - 1);
					float normalizedY = y * 1.0f / (terrainData.heightmapResolution - 1);

					newTree.position = new Vector3(normalizedX, -1, normalizedY);
					newTree.color = Color.white;
					newTree.heightScale = 1;
					newTree.widthScale = 1;
					newTree.rotation = Random.Range(0, 360);

					float angle = terrainData.GetSteepness(normalizedX, normalizedY);
						if (angle < _treeSettings.threshold)
							trees.Add(newTree);
				}
			}
				

		//We Will Now Start To Procedurally Generate The Terrain Using Perlin Noise
		terrainData.SetHeights(0, 0, heightmap);

		terrainData.SetTreeInstances(trees.ToArray(), true);
	}

	private void CreateProceduralPath(TerrainData terrainData, float threshold)
	{
		float[,,] splatmap = new float[terrainData.alphamapWidth, terrainData.alphamapHeight, 3];

		for (int x = 0; x < terrainData.alphamapWidth; x++)
			for (int y = 0; y < terrainData.alphamapHeight; y++)
			{	
				float normalizedX = x * 1.0f / (terrainData.alphamapWidth - 1);
				float normalizedY = y * 1.0f / (terrainData.alphamapHeight - 1);

				float angle = terrainData.GetSteepness(normalizedX, normalizedY);

				float fraction = angle / 90;
				splatmap[x, y, 0] = (float) fraction;
				splatmap[x, y, 1] = (float) (1 - fraction);
			}	

		terrainData.SetAlphamaps(0, 0, splatmap);
	}

	private void CreateProceduralGrass(TerrainData terrainData, float threshold)
	{
		int[,] detailmap = new int[terrainData.detailWidth, terrainData.detailHeight];
		//float[,] heightmap = terrainData.GetHeights(0, 0, terrainData.heightmapResolution, terrainData.heightmapResolution);

		for (int x = 0; x < terrainData.detailWidth; x++)
			for (int y = 0; y < terrainData.detailHeight; y++)
			{
				float normalizedX = x * 1.0f / (terrainData.detailWidth - 1);
				float normalizedY = y * 1.0f / (terrainData.detailHeight - 1);

				float angle = terrainData.GetSteepness(normalizedX, normalizedY);

				if (angle < threshold)
					detailmap[x, y] = 1;
				else detailmap[x, y] = 0;
			}

		terrainData.SetDetailLayer(0, 0, 0, detailmap);

		
	}

	private float GenerateHeightmap(int x, int y, TerrainData terrainData, byte seed)
	{
		float xCoordinate = (float) x / (int) terrainData.size.x * _terrainScale;
		float yCoordinate = (float) y / (int) terrainData.size.z * _terrainScale;

		return Mathf.PerlinNoise(xCoordinate + seed, yCoordinate + seed);
	}
}
