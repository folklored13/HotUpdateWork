using System.Collections;
using System.Collections.Generic;
using UnityEngine;  // ���������������ת����

public class CameraController : MonoBehaviour
{
    public Transform Player; // ����
    private float MouseX, MouseY; //��ȡ����ƶ���ֵ
    private float MouseSensitivity; // ���������

    private float XRotation; // ������ת����Χ��X����ת

    private bool MouseController = true;
    void Start()
    {
        MouseSensitivity = 100f; // �����������Ϊ200f

        
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            MouseController = !MouseController;
        }

        if (MouseController)
        {
            // Mouse X ��ʾ�����ᣬ��������ƶ����� -1����������ƶ����� 1
            MouseX = Input.GetAxis("Mouse X") * MouseSensitivity * Time.deltaTime;

            // Mouse Y ��ʾ������ᣬ��������ƶ����� -1����������ƶ����� 1
            MouseY = Input.GetAxis("Mouse Y") * MouseSensitivity * Time.deltaTime;

            // XRotationΪ��ֵʱ������̧����գ�����MouseYΪ��ʱ����������ƶ������ö�ӦXRotationΪ��
            XRotation -= MouseY; // ���Y����ƶ���

            // ��XRotationd��ֵ������ -70f �� 70f֮��
            XRotation = Mathf.Clamp(XRotation, -70f, 70f);

            // ����������ת
            Player.Rotate(Vector3.up * MouseX);

            // ���������ת
            transform.localRotation = Quaternion.Euler(XRotation, 0, 0);
        }
    }
        
}
