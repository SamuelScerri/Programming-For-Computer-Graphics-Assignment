using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
	[SerializeField]
	private byte _sceneIndex;

	private void OnTriggerEnter(Collider collision)
	{
		SceneManager.LoadScene(_sceneIndex);
	}
}
