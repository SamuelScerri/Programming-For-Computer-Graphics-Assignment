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

	[SerializeField]
	private GameObject _waterPrefab;

	[SerializeField]
	private byte _waterHeight;

	[SerializeField]
	private byte _beachSize;

	[SerializeField]
	private byte _treeSpacing;

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

		if (_treeSettings.generate)
			CreateProceduralTrees(_terrain.terrainData, _treeSettings.threshold, _treeSpacing);

		Instantiate(_waterPrefab, new Vector3(_terrain.terrainData.size.x / 2, _waterHeight, _terrain.terrainData.size.z / 2), Quaternion.Euler(90, 0, 0));
	}

	private void CreateProceduralTerrain(TerrainData terrainData, byte seed)
	{
		float[,] heightmap = new float[terrainData.heightmapResolution, terrainData.heightmapResolution];

		for (int x = 0; x < terrainData.heightmapResolution; x++)
			for (int y = 0; y < terrainData.heightmapResolution; y++)
				heightmap[x, y] = GenerateHeightmap(x, y, terrainData, seed);
				

		//We Will Now Start To Procedurally Generate The Terrain Using Perlin Noise
		terrainData.SetHeights(0, 0, heightmap);
	}

	private void CreateProceduralTrees(TerrainData terrainData, float threshold, byte spacing)
	{
		List<TreeInstance> trees = new List<TreeInstance>();

		for (int y = 0; y < terrainData.heightmapResolution; y += spacing)
			for (int x = 0; x < terrainData.heightmapResolution; x += spacing)
			{
				TreeInstance newTree = new TreeInstance();

				float normalizedX = x * 1.0f / (terrainData.heightmapResolution - 1);
				float normalizedY = y * 1.0f / (terrainData.heightmapResolution - 1);

				float height = terrainData.GetHeight(x, y);
				float angle = terrainData.GetSteepness(normalizedX, normalizedY);

				newTree.position = new Vector3(normalizedX, 0, normalizedY);
				newTree.color = Color.white;
				newTree.heightScale = 1;
				newTree.widthScale = 1;
				newTree.rotation = Random.Range(0, 360);

				if (angle < _treeSettings.threshold && height > _waterHeight + _beachSize)
					trees.Add(newTree);
			}

		terrainData.SetTreeInstances(trees.ToArray(), true);
	}

	private void CreateProceduralPath(TerrainData terrainData, float threshold)
	{
		float[,,] splatmap = new float[terrainData.alphamapWidth, terrainData.alphamapHeight, 3];

		//We Translate The Coordinate So That The Height Will Be Correctly Calculated Based On The Given Coordinates
		float translatedCoordinate = (float) terrainData.heightmapResolution / (float) terrainData.alphamapWidth;

		for (int y = 0; y < terrainData.alphamapHeight; y++)
			for (int x = 0; x < terrainData.alphamapWidth; x++)
			{	
				float normalizedX = x * 1.0f / (terrainData.alphamapWidth - 1);
				float normalizedY = y * 1.0f / (terrainData.alphamapHeight - 1);

				float angle = terrainData.GetSteepness(normalizedY, normalizedX);
				float height = terrainData.GetHeight((int) (translatedCoordinate * y), (int) (translatedCoordinate * x));

				float fraction = angle / 45;
				splatmap[x, y, 0] = (float) (1 - fraction);
				splatmap[x, y, 1] = (float) fraction;

				if (height < _waterHeight + _beachSize)
					splatmap[x, y, 2] = 1;
				else splatmap[x, y, 2] = 0;
			}	

		terrainData.SetAlphamaps(0, 0, splatmap);
	}

	private void CreateProceduralGrass(TerrainData terrainData, float threshold)
	{
		int[,] detailmap = new int[terrainData.detailWidth, terrainData.detailHeight];
		//float[,] heightmap = terrainData.GetHeights(0, 0, terrainData.heightmapResolution, terrainData.heightmapResolution);

		//We Translate The Coordinate So That The Height Will Be Correctly Calculated Based On The Given Coordinates
		float translatedCoordinate = (float) terrainData.heightmapResolution / (float) terrainData.detailWidth;

		for (int y = 0; y < terrainData.detailHeight; y++)
			for (int x = 0; x < terrainData.detailWidth; x++)
			{
				float normalizedX = x * 1.0f / (terrainData.detailWidth - 1);
				float normalizedY = y * 1.0f / (terrainData.detailHeight - 1);

				float angle = terrainData.GetSteepness(normalizedX, normalizedY);
				float height = terrainData.GetHeight((int) (translatedCoordinate * y), (int) (translatedCoordinate * x));

				if (x == 0 && y == 0)
					print(height);

				if (height > _waterHeight + _beachSize)
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

	public float GetWaterHeight()
	{
		return _waterHeight;
	}
}
