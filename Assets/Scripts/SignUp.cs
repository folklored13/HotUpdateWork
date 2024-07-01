using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// ����һ���û��࣬���������û���Ӧ����Ϣ
public class UserInfos
{
    public string UserName; // �û�����
    public string Account; // �˺�
    public string Password; // ����
}

public class SignUp : MonoBehaviour
{
    private Text TipsSignUp; // ע����ʾ
    private InputField InputUserName;  // ������Ϸ��
    private InputField InputAccount; // �û���
    private InputField InputPassword; // ��������

    private Button BtnSignUp; // ���밴ť
    private Button BtnReturnLogIn; // ���ص�����水ť

    private UserInfos userInfo; // �û���Ϣ
    private List<UserInfos> ListUser = new List<UserInfos>(); // �û����б�

    void Start()
    {
        // �ȼ����ļ����Ѵ��ڵ��û�
        AddLoadUserInfos();

        // ��ȡInput���
        InputUserName = GameObject.Find("InputUserName").GetComponent<InputField>();
        InputAccount = GameObject.Find("InputAccount").GetComponent<InputField>();
        InputPassword = GameObject.Find("InputPassword").GetComponent<InputField>();

        // ע�ᰴť
        BtnSignUp = GameObject.Find("BtnSignUp").GetComponent<Button>();
        BtnSignUp.onClick.AddListener(ClickSignUp);

        // ���ص��밴ť
        BtnReturnLogIn = GameObject.Find("BtnReturnSignIn").GetComponent<Button>();
        BtnReturnLogIn.onClick.AddListener(ClickReturnBtn);

        // ע����ʾ
        TipsSignUp = GameObject.Find("SignUpTips").GetComponent<Text>();
    }

    // ע��
    void ClickSignUp()
    {
        // �½�һ���û���Ϣ����input���������Ϣ�����浽�û�����
        userInfo = new UserInfos();
        userInfo.UserName = InputUserName.text;
        userInfo.Account = InputAccount.text;
        userInfo.Password = InputPassword.text;

        // �����û�����Ϣ��ӵ��û����б���
        ListUser.Add(userInfo);

        // �����û���Ϣ
        SaveUserInfos();
    }

    // �����û���Ϣ
    void SaveUserInfos()
    {
        // ����Json�ļ���
        if (!Directory.Exists(Application.dataPath + "/Json"))
        {
            Directory.CreateDirectory(Application.dataPath + "/Json");
        }

        // �ļ������·��
        string userInfosPath = Application.dataPath + "/Json/UserInfos.Json";

        // �� �û����б� ���л��� �ַ���
        string userInfosString = JsonConvert.SerializeObject(ListUser);

        // �ж��ļ��Ƿ����
        if (Directory.Exists(userInfosPath))
        {
            Directory.CreateDirectory(userInfosPath);
        }

        // ������д���ļ�����
        File.WriteAllText(userInfosPath, userInfosString);
        Debug.Log($"UserInfos�Ĵ��·��Ϊ��{userInfosPath}");

        // ��ʾע��ɹ�����ʾ
        StartCoroutine(DisplayText());
    }

    // ���ļ������е���Ϣ��ӵ��û����б�ListUser����
    void AddLoadUserInfos()
    {
        // UserInfos.Json �ļ�·��
        string userInfosPath = Application.dataPath + "/Json/UserInfos.Json";

        // File.ReadAllText ���������������ַ�����ʽ
        string userInfosString = File.ReadAllText(userInfosPath);

        // ���ַ��������л�Ϊ��List<UserInfos>����
        List<UserInfos> listUser = JsonConvert.DeserializeObject<List<UserInfos>>(userInfosString);

        // ���ļ����Ѵ��ڵ��û���ӵ��û��б�ListUser��
        foreach (UserInfos userInfo in listUser)
        {
            ListUser.Add(userInfo); // ��ӵ��û��б���
        }
    }

    // �������������
    void ClickReturnBtn()
    {
        SceneManager.LoadScene("LogIn");   // ���ص��볡��
    }

    // ע��ɹ�����ʾ����ʾ2�����ʧ
    IEnumerator DisplayText()
    {
        TipsSignUp.text = "�˺Ŵ����ɹ�";
        yield return new WaitForSeconds(2.0f); // ������ʾ2��
        TipsSignUp.text = null;
    }
}
