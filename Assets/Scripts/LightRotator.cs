using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LightRotator : MonoBehaviour
{
    public GameObject[] lightObjects; // 灯光对象数组
    public GameObject targetObject; // 目标物体
    public float rotationSpeed = 60f; // 旋转速度

    public Button StartRotateButton;
    private bool isRotating = false; // 添加一个布尔类型的变量来控制旋转状态

    void Start()
    {
        // 为按钮添加点击事件
        StartRotateButton.onClick.AddListener(ToggleRotation);
    }

    void Update()
    {
        if (isRotating)
        {
            for (int i = 0; i < lightObjects.Length; i++)
            {
                // 计算当前灯光相对于目标物体的旋转角度
                Vector3 relativePosition = targetObject.transform.position - lightObjects[i].transform.position;
                Quaternion rotation = Quaternion.LookRotation(relativePosition);

                // 应用旋转到当前灯光对象
                lightObjects[i].transform.rotation = rotation;

                // 绕Y轴旋转当前灯光
                lightObjects[i].transform.RotateAround(targetObject.transform.position, Vector3.up, rotationSpeed * Time.deltaTime);
            }
        }
    }

    // 切换旋转状态的方法
    private void ToggleRotation()
    {
        isRotating = !isRotating;
    }
}
