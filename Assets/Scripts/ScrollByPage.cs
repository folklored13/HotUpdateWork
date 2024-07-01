using System.Collections;
using System.Collections.Generic;
using UnityEngine;    // ͨ��������ҳ��
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ScrollByPage : MonoBehaviour,IEndDragHandler
{                                         // ���ýӿ�,������קʱ����Ϣ

    private ScrollRect scrollRect;  // ��ȡScrollRect���

    // �ܹ�4ҳ��ÿһҳ��λ�ã���λ��
    private float[] PagePotision = new float[4] { 0, 0.3333f, 0.6666f, 1 };
    private float TargetPotision = 0;  // Ŀ��λ��
    private bool IsMove = false; // �Ƿ񻬶�, ������
    private float ScollSpeed = 7; // �����ٶ�

    private Toggle[] PageToggles; // �������е�Toggle

    void Start()
    {
        // ����һ��Scoll View�����ű���������
        scrollRect = GetComponent<ScrollRect>(); // ��ȡ������ű�����

        // ��ȡѡ��ҳ�������Toggle���
        PageToggles = new Toggle[4];
        for(int i = 0; i < 4; i++)
        {
            string name = "PageToggle" + (i + 1).ToString(); // 4��Toggle����ΪPageToggle1 2 3 4
            PageToggles[i] = GameObject.Find(name).GetComponent<Toggle>(); // ��ȡToggle���
        }

        // ��Ӽ����¼������Toggle�ƶ�����Ӧ��ҳ��
        PageToggles[0].onValueChanged.AddListener((isOn) => MoveToPage(isOn, 0)); // ��һҳ
        PageToggles[1].onValueChanged.AddListener((isOn) => MoveToPage(isOn, 1)); // �ڶ�ҳ
        PageToggles[2].onValueChanged.AddListener((isOn) => MoveToPage(isOn, 2)); // ����ҳ
        PageToggles[3].onValueChanged.AddListener((isOn) => MoveToPage(isOn, 3)); // ����ҳ
    }

    void Update()
    {
        if (IsMove) // isMoveΪ��ʱ������
        {
            // ����������Mathf.Lerp�����ã���scrollRect.horizontalNormalizedPosition 
            // �� TargetPotision �ĳ��ȣ��ֳ�speed * Time.deltaTime ��
            scrollRect.horizontalNormalizedPosition = Mathf.Lerp(scrollRect.horizontalNormalizedPosition, 
                                             TargetPotision, ScollSpeed * Time.deltaTime);
            
            // ���ﻬ��Ŀ�ĵأ�ֹͣ����
            if(Mathf.Abs(scrollRect.horizontalNormalizedPosition - TargetPotision) < 0.001f)
            {
                IsMove = false; // ��Ϊ������
                scrollRect.horizontalNormalizedPosition = TargetPotision; // ��λ�ø�Ϊ��Ӧҳ��λ��
            }
        }
    }

    // ��ȡ������קʱ����Ϣ�����ȽϾ�����һҳ����������������һҳ
    public void OnEndDrag(PointerEventData eventData)
    {
        // �����ݵ�ˮƽ����ĳ���Width��λ��
        float curPosition = scrollRect.horizontalNormalizedPosition;

        // �������һҳ���
        int index = 0; // ��¼�±�

        // ��ǰλ�����һҳλ�õľ���
        float offset = curPosition - PagePotision[0]; 

        // ����ÿһҳ�뵱ǰλ�õľ��룬��¼�뵱ǰλ�������ҳ��
        for (int i = 1; i < 4; i++) // �ӵڶ�ҳ��ʼ
        {
            // �и����ģ����¼��ҳ����Ϣ�Լ�����ľ���
            if (Mathf.Abs(curPosition - PagePotision[i]) < offset)
            {
                index = i; // ��¼�±�
                offset = Mathf.Abs(curPosition - PagePotision[i]); // ��¼����ļ�¼
            }
        }

        // ������ľ��븳ֵ��Ŀ��Ϊ
        TargetPotision = PagePotision[index]; 
        IsMove = true; // ��ʼ�ƶ�
        PageToggles[index].isOn = true; // ѡ�и�toggle
    }

    // ͨ�����Toggle���ƶ���ָ����ҳ��
    private void MoveToPage(bool isOn, int index)
    {
        if (isOn)
        {
            IsMove = true; // �ƶ�
            TargetPotision = PagePotision[index]; // �ƶ���ָ��ҳ��
        }
    }
}
