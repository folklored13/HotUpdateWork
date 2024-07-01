using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallThrower : MonoBehaviour
{
    public float throwForce = 10f; // 抛射力度

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player")) // 检查碰撞对象是否为带有"Player"标签的对象
        {
            Rigidbody ballRigidbody = GetComponent<Rigidbody>(); // 获取球体的刚体组件
            if (ballRigidbody != null)
            {
                ballRigidbody.AddForce(transform.up * throwForce, ForceMode.Impulse); // 给球体施加向上的力，使其抛物线飞出去
            }
        }
    }
}
