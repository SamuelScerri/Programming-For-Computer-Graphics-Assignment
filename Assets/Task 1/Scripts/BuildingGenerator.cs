using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingGenerator : MonoBehaviour
{
	private MeshFilter _meshFilter;
	private MeshCollider _meshCollider;
	private MeshRenderer _meshRenderer;

	[SerializeField]
	private byte _maxHeight, _minHeight, _width, _spawnPercentage;

	[SerializeField]
	private Color _buildingColor;

	private void Start()
	{
		_meshFilter = GetComponent<MeshFilter>();
		_meshCollider = GetComponent<MeshCollider>();
		_meshRenderer = GetComponent<MeshRenderer>();

		if (RandomizeBoolean(_spawnPercentage))
		{
			GenerateBuilding();
			GenerateMaterial();
		}
	}

	private void GenerateBuilding()
	{
		//The Subtraction Is Added To Ensure That The Cube Will Be In The Middle
		_meshFilter.mesh = CubeGenerator.GenerateCubeData((byte) Random.Range(_minHeight, _maxHeight), _width);
		_meshCollider.sharedMesh = _meshFilter.mesh;
	}

	private void GenerateMaterial()
	{
		Material newMaterial = new Material(Shader.Find("Specular"));
		newMaterial.color = _buildingColor;

		_meshRenderer.material = newMaterial;
	}

	/*
		Code Adapted From:
		https://stackoverflow.com/questions/25275873/generate-random-boolean-probability,

		This Will Basically Be True If The Generated Number Is Smaller Than The Percentage,
		If Not Then False Will Be Returned
	*/
	private bool RandomizeBoolean(byte percentage)
	{
		return Random.Range(0, 100) < percentage;
	}
}
