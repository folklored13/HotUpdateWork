using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Eating : MonoBehaviour
{
    public Text TipsChara;
    private int HungerValue = 60;

    void Start()
    {

        StartCoroutine(EnegerIndece());  // ���õ���ʱ

    } 
    IEnumerator EnegerIndece() // ��ʱ��
    {

        while (HungerValue > 0)
        {
            HungerValue--;
            yield return new WaitForSeconds(1f);
        }

        if(HungerValue == 0)
        {
            TipsChara.text = "̫���ˣ���ȥ�Է���";
        }
    }

    IEnumerator EnegerAdd()
    {
        if (HungerValue >= 60)
        {
            HungerValue = 60;
            TipsChara.text = "�Ա��ˣ�";
            yield return new WaitForSeconds(1f);
            TipsChara.text = null;

            StartCoroutine(EnegerIndece()); // ���õ���ʱ
        }
    }

    public void ClickEatFood()
    {
        HungerValue += 30;
        StartCoroutine(EnegerAdd()); 
    }  
}
