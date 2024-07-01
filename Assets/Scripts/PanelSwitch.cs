using UnityEngine;
using UnityEngine.UI;

public class PanelSwitch : MonoBehaviour
{
    public GameObject panel1; // ��Panel1��ק�����������
    public GameObject panel2; // ��Panel2��ק�����������
    public GameObject panel3; // ��Panel3��ק�����������

    public void TogglePanel1()
    {
        panel1.SetActive(!panel1.activeSelf); // �л� panel1 ��״̬
        panel2.SetActive(!panel2.activeSelf);
    }

    public void TogglePanel2()
    {
        panel2.SetActive(!panel2.activeSelf); // �л� panel1 ��״̬
        panel3.SetActive(!panel3.activeSelf); // �л� panel2 ��״̬
    }

    public void TogglePanel3()
    {
        panel3.SetActive(!panel3.activeSelf); // �л� panel2 ��״̬
        panel1.SetActive(!panel1.activeSelf); // �л� panel3 ��״̬
    }
}