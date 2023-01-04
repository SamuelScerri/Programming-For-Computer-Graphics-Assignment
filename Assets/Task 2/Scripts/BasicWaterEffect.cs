using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicWaterEffect : MonoBehaviour
{
    private Material _material;

    private void Start()
    {
        _material = GetComponent<Renderer>().material;
    }

    // Update is called once per frame
    void Update()
    {
        _material.mainTextureOffset += Vector2.right * Time.deltaTime / 16;
    }
}
