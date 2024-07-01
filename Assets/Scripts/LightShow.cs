using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LightShow : MonoBehaviour
{
    public GameObject[] lights; // 假设你已经有了四个灯光对象
    public Button lightbutton;
    bool isLight = true;

    void Start()
    {
        lightbutton.onClick.AddListener(ToggleLights);
    }

    void ToggleLights()
    {
        isLight = true;
        foreach (GameObject light in lights)
        {
            light.SetActive(isLight);
        }
    }
}