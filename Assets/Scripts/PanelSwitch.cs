using UnityEngine;
using UnityEngine.UI;

public class PanelSwitch : MonoBehaviour
{
    public GameObject panel1; // 将Panel1拖拽到这个变量上
    public GameObject panel2; // 将Panel2拖拽到这个变量上
    public GameObject panel3; // 将Panel3拖拽到这个变量上

    public void TogglePanel1()
    {
        panel1.SetActive(!panel1.activeSelf); // 切换 panel1 的状态
        panel2.SetActive(!panel2.activeSelf);
    }

    public void TogglePanel2()
    {
        panel2.SetActive(!panel2.activeSelf); // 切换 panel1 的状态
        panel3.SetActive(!panel3.activeSelf); // 切换 panel2 的状态
    }

    public void TogglePanel3()
    {
        panel3.SetActive(!panel3.activeSelf); // 切换 panel2 的状态
        panel1.SetActive(!panel1.activeSelf); // 切换 panel3 的状态
    }
}