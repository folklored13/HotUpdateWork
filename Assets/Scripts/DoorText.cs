using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DoorText : MonoBehaviour,IPointerEnterHandler,IPointerExitHandler
{
    private Text text;
    void Start()
    {
        text = transform.Find("DoorText").GetComponent<Text>();
    }

    // ����Canvas��Render Mode ��Ϊ World Spaceʱ��z����볯����Ļ����������Ļ��
    public void OnPointerEnter(PointerEventData eventData)
    {
        text.enabled = true;
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        text.enabled = false;
    }
}
