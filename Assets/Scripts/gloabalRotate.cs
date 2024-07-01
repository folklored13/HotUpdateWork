using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class gloabalRotate : MonoBehaviour
{
    public GameObject obj;
    public float rotationSpeed = 50f;
    public Button button;
    private bool isRotating = false; // ���һ������������������ת

    void Start()
    {
        button.onClick.AddListener(ToggleRotation); // ���İ�ť�����¼�ΪToggleRotation
    }

    // Update is called once per frame
    void Update()
    {
        if (isRotating) // ���������ת���������ת
        {
            obj.transform.Rotate(Vector3.forward * rotationSpeed * Time.deltaTime);
        }
    }

    void ToggleRotation() // �л���ת״̬�ĺ���
    {
        isRotating = !isRotating; // ��ת��ת״̬
    }
}
