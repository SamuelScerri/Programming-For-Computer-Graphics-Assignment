using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
	[SerializeField]
	private byte _sceneIndex;

	[SerializeField]
	private bool _endGame;

	private void OnTriggerEnter(Collider collision)
	{
		if (_endGame)
		{
			Application.Quit();
			Debug.Log("Shut Down");
		}
		else SceneManager.LoadScene(_sceneIndex);
	}
}
