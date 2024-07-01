using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallThrower : MonoBehaviour
{
    public float throwForce = 10f; // ��������

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player")) // �����ײ�����Ƿ�Ϊ����"Player"��ǩ�Ķ���
        {
            Rigidbody ballRigidbody = GetComponent<Rigidbody>(); // ��ȡ����ĸ������
            if (ballRigidbody != null)
            {
                ballRigidbody.AddForce(transform.up * throwForce, ForceMode.Impulse); // ������ʩ�����ϵ�����ʹ�������߷ɳ�ȥ
            }
        }
    }
}
