using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CityEnum
{
	Grass,
	Street,
	Building
};

public class CityGenerator : MonoBehaviour
{
	//The Amount Of Buildings That The Generator Will Create
	[SerializeField]
	private byte _buildingAmount, _cityWidth, _cityHeight;

	//The Array That Will Be Used To Generate The City
	private byte[,] _cityArray;

	private void Start()
	{
		_cityArray = GenerateCity(_cityWidth, _cityHeight, _buildingAmount);
		VisualizeArray(_cityArray, _cityWidth, _cityHeight);
	}

	/*
		This City Generator Will Basically Create An Array With The Size Of The Width And Height,
		It Is Very Primitive Compared To Other City Generators But It Gets The Job Done.

		Essentially It Creates A Grid Of Streets, Each Spaced With Grass In Between,
		Sometimes A Building Will Be Added On Top Of This Empty Plot Of Land.
	*/
	private byte[,] GenerateCity(byte width, byte height, byte amount)
	{
		byte buildingsLeft = amount;
		byte[,] city = new byte[width, height];

		for (byte y = 0; y < height; y++)
			for (byte x = 0; x < width; x++)
			{
				if (y % 2 == 0)
					city[x, y] = (byte) CityEnum.Street;
				else if (x % 2 == 1)
					city[x, y] = (byte) CityEnum.Street;
				else if (buildingsLeft-- > 0 && RandomizeBoolean(50))
					city[x, y] = (byte) CityEnum.Building;
				else city[x, y] = (byte) CityEnum.Grass;
			}

		return city;
	}

	/*
		This Is For Debugging Purposes But Useful For Generating The City Array,
		This Is Responsible For Displaying The Array In The Form Of A Grid, Similar
		To How You Will Look At The City In A Top Down View.
	*/
	private void VisualizeArray(byte[,] array, byte width, byte height)
	{
		string visualizedArray = "";

		for (byte y = 0; y < height; y++)
		{
			for (byte x = 0; x < width; x++)
				visualizedArray += array[x, y].ToString() + ", ";

			Debug.Log(y.ToString() + visualizedArray);
			visualizedArray = "";
		}
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
