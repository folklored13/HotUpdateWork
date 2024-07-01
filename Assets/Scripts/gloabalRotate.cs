using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class gloabalRotate : MonoBehaviour
{
    public GameObject obj;
    public float rotationSpeed = 50f;
    public Button button;
    private bool isRotating = false; // 添加一个布尔变量来控制旋转

    void Start()
    {
        button.onClick.AddListener(ToggleRotation); // 更改按钮监听事件为ToggleRotation
    }

    // Update is called once per frame
    void Update()
    {
        if (isRotating) // 如果正在旋转，则继续旋转
        {
            obj.transform.Rotate(Vector3.forward * rotationSpeed * Time.deltaTime);
        }
    }

    void ToggleRotation() // 切换旋转状态的函数
    {
        isRotating = !isRotating; // 反转旋转状态
    }
}
