using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TipsTriggle : MonoBehaviour
{
    public Text Tips;
    private int count = 0;
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        StartCoroutine(EnterTipsArea());

        count = Random.Range(0,2);
    }

    IEnumerator EnterTipsArea()
    {
        if(count == 0)
        {
            Tips.text = "�����������";
        }
        else
        {
            Tips.text = "��˵����������!";
        }
       
        yield return new WaitForSeconds(2f);

        Tips.text = null;
    }
}
