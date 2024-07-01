using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    private float MoveSpeed = 1.8f; // �ƶ��ٶ�
    private float RunSpeed = 5.0f;  // �ܲ��ٶ�
    private Animator ErikaAnimator; // ����������
    private bool isRuning; // �Ƿ���
    private float currentSpeed; // ��ǰ�ٶ�
    public bool isOpenDoor; // �Ƿ���
    private LayerMask DoorLayer;  // �㼶��outSide
    private float raycastDistance = 1f;  // ���߷���ľ���
    public RaycastHit hitInfo; // ����һ��RaycastHit�����������汻ײ�������Ϣ


    void Start()
    {
        DoorLayer = LayerMask.GetMask("Door"); // ���ŵĲ㼶��ΪDoor
        ErikaAnimator = transform.GetComponent<Animator>(); // ��ȡAnimator���
    }

    void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // ��ɫ����ת
        //transform.Rotate(new Vector3(0, horizontal * RotateSpeed, 0) * Time.deltaTime);

        if (Input.GetKeyDown(KeyCode.LeftShift)) // ����Shift��
        {
            isRuning = true;
        }

        if (Input.GetKeyUp(KeyCode.LeftShift)) // ̧��
        {
            isRuning = false;
        }

        // ��·���ܲ�
        if (!isRuning)
        {
            transform.Translate(new Vector3(horizontal, 0, vertical * MoveSpeed) * Time.deltaTime);
            currentSpeed = new Vector3(horizontal, 0, vertical).magnitude * MoveSpeed;
        }
        else
        {
            transform.Translate(new Vector3(horizontal, 0, vertical * RunSpeed) * Time.deltaTime);
            currentSpeed = new Vector3(horizontal, 0, vertical).magnitude * RunSpeed;
        }

        // ��ɫ�ƶ�ʱ�������ƶ�����
        Vector3 move = new Vector3(0, 0, vertical);
        ErikaAnimator.SetFloat("Speed", currentSpeed);

        // ��Ծ
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ErikaAnimator.SetTrigger("IsJump");
        }

        // ��������
        Vector3 offSet = new Vector3(0, 1, 0); // ƫ����
        Ray ray = new Ray();
        ray.origin = transform.position + offSet;   //�������
        ray.direction = transform.forward; //���߷���
        isOpenDoor = Physics.Raycast(ray, out hitInfo, raycastDistance, DoorLayer);
        if (isOpenDoor) // ���Ŷ���
        {
            ErikaAnimator.Play("OpenDoor");
        }
    }
}
