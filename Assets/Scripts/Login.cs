using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Login : MonoBehaviour
{
    public string HeadSculptureUserName; // �û���
    private InputField AccountInput; // �˺�
    private InputField PasswordInput; // ��������
    private Button LoginBtn; // ���밴ť
    private Text LogInTips; // ��ʾ�ı�

    // ʹ�����ű����Ե���Login�ű��еı���
    private static Login _instance;
    public static Login Instance
    {
        get { return _instance; }
    }

    private void Awake()  // ��ʼ��
    {
        _instance = this;
    }

    void Start()
    {
        // ��ʼ��
        AccountInput = GameObject.Find("InputAccount").GetComponent<InputField>();
        PasswordInput = GameObject.Find("InputPassword").GetComponent<InputField>();
        PasswordInput.contentType = InputField.ContentType.Password; // ��������

        LogInTips = GameObject.Find("SignUpTips").GetComponent<Text>();

        // ���밴ť
        LoginBtn = GameObject.Find("LoginBtn").GetComponent<Button>();
        LoginBtn.onClick.AddListener(ClickLogIn); // �󶨼����¼�

        // ע�ᰴť
        LoginBtn = GameObject.Find("SignUpBtn").GetComponent<Button>();
        LoginBtn.onClick.AddListener(ClickSignUp); // �󶨼����¼�
    }

    // ����
    void ClickLogIn()
    {
        List<UserInfos> listUser = LoadUserInfos(); // �ȼ����ļ����Ѿ����ڵ��û�
        bool userNoExist = true; // ��¼�û��Ƿ����
        bool passwordError = false; // ��¼�����Ƿ���ȷ


        // �ж��û����������Ƿ���ȷ
        foreach (UserInfos userInfos in listUser)
        {
            // �ļ��е��˺ŵ���������˺ţ����˺���ȷ
            if (userInfos.Account == AccountInput.text) // �˺���ȷ
            {
                userNoExist = false; // �˺���ȷ����˵���û�����

                if (userInfos.Password == PasswordInput.text) // ������ȷ
                {
                    HeadSculptureUserName = userInfos.UserName; // ����Ϸ�����е�ͷ�����Ƹ�ֵ
                    StartCoroutine(LoadMainScene()); // ������Ϸ����
                }
                else // �������
                {
                    passwordError = true;
                }
            }
        }

        // �û������ڣ������Ӧ��ʾ
        if (userNoExist)
        {
            StartCoroutine(TipsUserNoExist());
        }

        // ������������Ӧ��ʾ
        if (passwordError)
        {
            StartCoroutine(TipsPasswordError());
        }
    }

    // ��ȡ�ļ����е��û���Ϣ��������
    List<UserInfos> LoadUserInfos()
    {

        // UserInfos.Json �ļ�·��
        string UserInfosPath = Application.dataPath + "/Json/UserInfos.Json";

        // File.ReadAllText ���������������ַ�����ʽ
        string UserInfosString = File.ReadAllText(UserInfosPath);

        // ���ַ��������л�Ϊ��List<UserInfos>����
        List<UserInfos> listUser = JsonConvert.DeserializeObject<List<UserInfos>>(UserInfosString);

        return listUser;
    }

    // ���ע�ᣬ��תע�᳡��
    void ClickSignUp()
    {
        SceneManager.LoadScene("SignUp"); // ���ص������
    }

    // ����ɹ�����������
    IEnumerator LoadMainScene()
    {
        LogInTips.text = "����ɹ���";
        yield return new WaitForSeconds(0.1f); // ������ʾ1�����ʧ
        SceneManager.LoadScene("SelectCharacter"); // ������Ϸ����
    }

    // ���������ʾ
    IEnumerator TipsPasswordError()
    {
        LogInTips.text = "�����������";
        yield return new WaitForSeconds(2f); // ������ʾ2�����ʧ
        LogInTips.text = null;
    }

    // �û���������ʾ
    IEnumerator TipsUserNoExist()
    {
        LogInTips.text = "�û������ڣ�";
        yield return new WaitForSeconds(2f); // ������ʾ2�����ʧ
        LogInTips.text = null;
    }
}
