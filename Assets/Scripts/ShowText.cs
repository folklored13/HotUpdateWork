using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowText : MonoBehaviour
{
    public GameObject textObj; // ������unity��ֵ����Ϊ�ýű����Ҳ������ص�����
    public string description;
    // Start is called before the first frame update
    void Start()
    {
        Text showText = textObj.GetComponent<Text>();
        showText.text = description; 
    }

    IEnumerator DisplayText()
    {
        textObj.SetActive(true);  // ������SetActive�������enable
        yield return new WaitForSeconds(3.0f); // ������ʾ2��
        textObj.SetActive(false); // ��������
    }

    private void OnTriggerEnter(Collider other)
    {
        StartCoroutine(DisplayText());
    }
}
