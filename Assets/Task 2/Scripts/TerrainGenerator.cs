using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
	Samuel Scerri,

	The Basic Procedural Terrain Code Was Inspired From Brackey's Terrain Generator Tutorial Which Used Perlin Noise,
	In Order To Ensure That The Environment Is Always Different I Decided To Generate The Perlin Noise Based On A Random Seed From 0 To 255 (But Can Be Changed To Higher Values),

	Most Of The Code Was Inspired From The Unity's Documentation, Alpha Maps & Detail Maps Where Used To Paint Textures & Grass.
	The Documentation Links Are As Follows:
		(Insert Links Here)

	Trees Where Created Based On The Heightmap. As Long As The Height Is Larger Than The Water Height Then Trees Are Instanced, Trees Are Created Using The Set Tree Instance Command,
	Which Is Responsible For Drawing Trees Efficiently On The Terrain Component.

	Water Is Quite Simple, It Is Essentially A Large Plane That Has The Y-Axis Set To The Water Height, The Water Has A Simple Effect Where Its Offset Slowly To The Right,
	Giving A Basic Illusion Of Water Current.

	On Average It Takes Around 5-10 Seconds To Create New Terrain, Depending On The Heightmap And Detailmap Resolution, In This Case I Used A Very Large Resolution So It Takes Quite A While To Generate.
*/

//These Are General Settings That Every Element Will Have
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
	private GameObject _playerPrefab;

	[SerializeField]
	private byte _waterHeight;

	[SerializeField]
	private byte _mountainHeight;

	[SerializeField]
	private byte _beachSize;

	[SerializeField]
	private byte _treeSpacing;

	///This Option Is Very Resource Intensive If The Value Is Very Hight
	[SerializeField]
	private bool _enableCustomDetailDistance;

	[SerializeField]
	private int _customDetailDistance;

	private Terrain _terrain;
	private TerrainCollider _terrainCollider;

	private void Start()
	{
		//The Terrain Will Be Generated Using A Procedurally Generated Terrain Data Object
		_terrain = GetComponent<Terrain>();
		_terrainCollider = GetComponent<TerrainCollider>();

		/*
			There Is An Issue Where Tree Colliders Aren't Being Positioned Properly After Generating The Terrain,
			A Similar Issue Was Resolved Here: https://answers.unity.com/questions/287361/tree-colliders-not-working.html
			This Method Seemed To Work Perfectly So Far
		*/
		_terrainCollider.enabled = false;

		if (_enableCustomDetailDistance)
			_terrain.detailObjectDistance = _customDetailDistance;

		//We Generate The Terrain With A Random Seed, This Seed Will Be Used So That The Terrain Generated Will Always Be Different In The Same Position
		CreateProceduralTerrain(_terrain.terrainData, (byte) Random.Range(0, 255));

		//We Then Generate Grass, This Will Also Use The Detailmap
		if (_grassSettings.generate)
			CreateProceduralGrass(_terrain.terrainData, _grassSettings.threshold);

		//Textures Are Then Painted, This Is Based On The Heightmap
		if (_pathSettings.generate)
			CreateProceduralPath(_terrain.terrainData, _pathSettings.threshold);

		//Trees Are Then Added Using A Similar Process To Grass, But With Larger Spacing Between Them
		if (_treeSettings.generate)
			CreateProceduralTrees(_terrain.terrainData, _treeSettings.threshold, _treeSpacing);

		//We Will Now Enable The Terrain Collider, So That Tree Colliders Will Be Calculated Properly
		_terrainCollider.enabled = true;

		Instantiate(_waterPrefab, new Vector3(_terrain.terrainData.size.x / 2, _waterHeight, _terrain.terrainData.size.z / 2), Quaternion.Euler(90, 0, 0));
		Instantiate(_playerPrefab, new Vector3(Random.Range(0, _terrain.terrainData.size.x), 128, Random.Range(0, _terrain.terrainData.size.z)), Quaternion.identity);
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

				/*
					Trees Work Differently Than Detail Maps, These Are Basically Places On The Terrain With The Normalized Space Coordinates,
					This Is Done By Normalizing The Terrain Coordinates, Which Will Basically Be A 2D Vector From 0 To 1
				*/

				newTree.position = new Vector3(normalizedX, 0, normalizedY);

				//This Is To Ensure That The Tree Won't Be Transparent Or Have The Wrong Scale
				newTree.color = Color.white;
				newTree.heightScale = 1;
				newTree.widthScale = 1;

				//Self Explanatory, The Rotation Is Randomized To A More Natural Look To The Environment
				newTree.rotation = Random.Range(0, 360);

				//Trees Will Only Generate If The Angle Of The Terrain That They Are Currently On Is Smaller Than The Threshold Inputted In The Editor
				if (angle < _treeSettings.threshold && height > _waterHeight + _beachSize && height < _mountainHeight)
					trees.Add(newTree);
			}

		/*
			Since A List Was Used, It Has Been Converted To An Array Since That's What This Function Requires,
			This Most Likely Could Be More Optimized But For Readibility Sake I Kept Using The List
		*/
		terrainData.SetTreeInstances(trees.ToArray(), true);
	}

	private void CreateProceduralPath(TerrainData terrainData, float threshold)
	{
		//The Splat Map Is Basically Another Term For Alpha Map
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

				/*
					This Code Has Been Adapted From The Documentation Link Provided Below:
						(Documentation Link Here)

					Essentially The Steeper The Hill, The Less Grass There Is
				*/
				float fraction = angle / 45;
				splatmap[x, y, 0] = (float) (1 - fraction);
				splatmap[x, y, 1] = (float) fraction;

				//When The Height Is Smaller Than The Beach Size, We Will Paint Sand Here
				if (height < _waterHeight + _beachSize)
					splatmap[x, y, 2] = 1;
				else splatmap[x, y, 2] = 0;
			}	

		terrainData.SetAlphamaps(0, 0, splatmap);
	}

	private void CreateProceduralGrass(TerrainData terrainData, float threshold)
	{
		int[,] detailmap = new int[terrainData.detailWidth, terrainData.detailHeight];

		//We Translate The Coordinate So That The Height Will Be Correctly Calculated Based On The Given Coordinates
		float translatedCoordinate = (float) terrainData.heightmapResolution / (float) terrainData.detailWidth;

		for (int y = 0; y < terrainData.detailHeight; y++)
			for (int x = 0; x < terrainData.detailWidth; x++)
			{
				float normalizedX = x * 1.0f / (terrainData.detailWidth - 1);
				float normalizedY = y * 1.0f / (terrainData.detailHeight - 1);

				float angle = terrainData.GetSteepness(normalizedX, normalizedY);
				float height = terrainData.GetHeight((int) (translatedCoordinate * y), (int) (translatedCoordinate * x));

				//Essentially This Means That Grass Will Not Spawn On Sand, Only On Dirt Or Grass
				if (height > _waterHeight + _beachSize && height < _mountainHeight)
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
