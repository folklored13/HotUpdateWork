using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class HeadSculpture : MonoBehaviour
{
    // ��̬�����������������ű��з���
    public static Text playerName;  // ��Ϸ����
    public static Text playerLevel; // ���ǵȼ�
    private Image headSculpture; // ͷ��ͼƬ
    
    
    void Start()
    {
        playerName = GameObject.Find("Name").GetComponent<Text>(); // ��ȡText���
        playerName.horizontalOverflow = HorizontalWrapMode.Overflow; // �����������ı���
        //playerName.text = Login.Instance.PlayerName.text; // �������������ָ��Ƶ���Ϸ���� 

        playerLevel = GameObject.Find("Level").GetComponent<Text>(); // ��ȡText���
        playerLevel.text = "0";  // ��ʼ�ȼ�Ϊ0
        string path = Application.dataPath + "/Json/Player.json";
        if (File.Exists(path))
        {
            StreamReader reader = File.OpenText(path);
            string json = reader.ReadToEnd();
            Player.PlayerData playerData = JsonConvert.DeserializeObject<Player.PlayerData>(json);
            if(playerName.text == "")
            {
                playerName.text = playerData.PlayerName;  // �������������Ƹ�ֵ���ı�
                playerLevel.text = playerData.PlayerLevel;// ���������ҵȼ���ֵ���ı�
            }   
        }

        // ��ȡ���
        headSculpture = GameObject.Find("head_img").GetComponent<Image>();
        string index = Random.Range(1, 6).ToString(); // �������һ����
        
        // �첽������Դ�����������ں�̨������Դ�������������߳�ִ�С�
        // ��Resources �ļ����У�ͼƬΪSprite��ͼƬ����Ϊ1,2,3,4,5
        ResourceRequest resourceRequest = Resources.LoadAsync<Sprite>(index);
        headSculpture.sprite = resourceRequest.asset as Sprite; // ��ֵ
    }
}
