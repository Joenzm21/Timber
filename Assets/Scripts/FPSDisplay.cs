using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FPSDisplay : MonoBehaviour
{
    float time = 0f;
    Text t;
    float fps = 60;

    public float FPS { get => fps;  }

    void Start()
    {
        t = GetComponent<Text>();
    }

    void LateUpdate()
    {
        time += (Time.deltaTime - time) * 0.1f;
        fps = Mathf.Round(1.0f / time * 10) / 10; 
        t.text = fps.ToString() + " FPS";
    }
}
