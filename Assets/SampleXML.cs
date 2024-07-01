using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using UnityEngine;
using System;

public class SampleXML : MonoBehaviour
{

    public SampleScriptableObject Sample;
    // Start is called before the first frame update
    void Start()
    {
        SaveXML();
        //LoadXML();

        Sample = ScriptableObject.CreateInstance<SampleScriptableObject>();
        Sample.Index++;
        Debug.Log(Sample.Index);
    }

    void SaveXML()
    {
        XmlDocument xmlDoc = new XmlDocument();
        XmlElement xmlRootElment = xmlDoc.CreateElement("�����Ͽ�ѧ��"); 
        xmlRootElment.InnerText = "Unity3D�γ�";
        xmlRootElment.SetAttribute("ʱ��", DateTime.Now.ToShortDateString());

        XmlElement xmlStudentElement = xmlDoc.CreateElement("����");
        for (int i = 0210500; i < 0210506; i++)
        {
            xmlStudentElement.SetAttribute("�Ա�", "��");
            XmlElement xmlStudentIndexElement = xmlDoc.CreateElement("ѧ��");
            xmlStudentIndexElement.InnerText = i.ToString();

            xmlStudentElement.AppendChild(xmlStudentIndexElement);
        }

        xmlRootElment.AppendChild(xmlStudentElement);
        XmlElement xmlTeacherElement = xmlDoc.CreateElement("Ů��");
        for (int i = 0; i < 8; i++)
        {
            xmlTeacherElement.SetAttribute("�Ա�", "Ů");
            XmlElement xmlTeacherIndexElement = xmlDoc.CreateElement("ѧ��");
            xmlTeacherIndexElement.InnerText = i.ToString();

            xmlTeacherElement.AppendChild(xmlTeacherIndexElement);
        }
        xmlRootElment.AppendChild(xmlTeacherElement);
        xmlDoc.AppendChild(xmlRootElment);

        string xmlPath = Path.Combine(Application.dataPath, "xmlDocSample.Sample");
        xmlDoc.Save(xmlPath);
    }
    void LoadXML()
    {
        string xmlPath = Path.Combine(Application.dataPath, "xmlDocSample.Sample");
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.Load(xmlPath);

        //node�ǽڵ��ȡ���ͣ������������޷�����
        foreach(XmlNode node in xmlDoc.ChildNodes)
        {
            Debug.Log(node.Name);
            XmlElement element = (XmlElement)node;

            element.SetAttribute("��̬���", "����ʱ");
            foreach(XmlNode childNode in node.ChildNodes)
            {
                Debug.Log(childNode.Name);
            }
        }
        xmlDoc.Save(xmlPath);
    }
    void Update()
    {
        
    }
}
