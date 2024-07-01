using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LightRotator : MonoBehaviour
{
    public GameObject[] lightObjects; // �ƹ��������
    public GameObject targetObject; // Ŀ������
    public float rotationSpeed = 60f; // ��ת�ٶ�

    public Button StartRotateButton;
    private bool isRotating = false; // ���һ���������͵ı�����������ת״̬

    void Start()
    {
        // Ϊ��ť��ӵ���¼�
        StartRotateButton.onClick.AddListener(ToggleRotation);
    }

    void Update()
    {
        if (isRotating)
        {
            for (int i = 0; i < lightObjects.Length; i++)
            {
                // ���㵱ǰ�ƹ������Ŀ���������ת�Ƕ�
                Vector3 relativePosition = targetObject.transform.position - lightObjects[i].transform.position;
                Quaternion rotation = Quaternion.LookRotation(relativePosition);

                // Ӧ����ת����ǰ�ƹ����
                lightObjects[i].transform.rotation = rotation;

                // ��Y����ת��ǰ�ƹ�
                lightObjects[i].transform.RotateAround(targetObject.transform.position, Vector3.up, rotationSpeed * Time.deltaTime);
            }
        }
    }

    // �л���ת״̬�ķ���
    private void ToggleRotation()
    {
        isRotating = !isRotating;
    }
}
