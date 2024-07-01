using UnityEngine;
using Newtonsoft.Json;  // Json������
using System.IO;
using System.Collections.Generic;

public class Player : MonoBehaviour
{
    public static float moveSpeed = 1f; // ��ɫ�ƶ��ٶ�
    private float rotateSpeed = 60f; // ��ɫ��ת�ٶ�
    private List<float> speedList = new List<float>(); // �洢��ɫ������Ӧ���ٶ�
    private LayerMask DoorLayer;  // �㼶��outSide
    public bool isOpenDoor; // �Ƿ���
    private float raycastDistance = 1f;  // ���߷���ľ���
    public RaycastHit hitInfo; // ����һ��RaycastHit�����������汻ײ�������Ϣ
    private Animator PlayerAnim; // ���ﶯ��������

    void Start()
    {
        DoorLayer = LayerMask.GetMask("Door"); // ���ŵĲ㼶��ΪDoor
        PlayerAnim = transform.GetComponent<Animator>(); // ��ȡAnimator���
        
        // ��ȡjson���������ǵ�λ�ú���ת
        string pathJson = Application.dataPath + "/Json/Player.json";
        if (File.Exists(pathJson))
        {
            // ���ļ���������һ����ȡ���ļ�������
            StreamReader reader = File.OpenText(pathJson);
            string json = reader.ReadToEnd(); // ��ȡ�ļ���ȫ������
            // �����л�������ȡ����Json�ı�ת�������������
            PlayerData playerData = JsonConvert.DeserializeObject<PlayerData>(json);
            
            // �������ǵ�λ��
            transform.position = new Vector3(playerData.Position[0], playerData.Position[1], playerData.Position[2]);
            
            // ��ŷ��������Ԫ���ķ�ʽ��ֵ��transform.rotation, ����ŷ����ת������Ԫ��
            transform.rotation = Quaternion.Euler(playerData.Rotation[0], playerData.Rotation[1], playerData.Rotation[2]);
            reader.Close(); // �ر��ļ�������ֹһֱռ���ļ�
            reader.Dispose(); // �����ļ��������ͷ��ڴ�
        }

        // ��ȡCSV���������ǵ��ƶ��ٶ�
        string pathCSV = Application.dataPath + "/Resource/Resources/PlayerPropertyData.csv";
        string csvString = File.ReadAllText(pathCSV); // ��ȡCSV�ļ�
        string separateSign = ",";
        string lineFeedSign = "\r\n";

        string[] csvRowDatas = csvString.Split(lineFeedSign);
        
        for(int i = 1; i < csvRowDatas.Length; i++) // ����ÿһ������
        {
            // �ٶȴ����ڵ�����
            string[] csvRowData = csvRowDatas[i].Split(separateSign);
            speedList.Add(float.Parse(csvRowData[2]));
        }

    }

    void Update()
    {
        // ������ҵȼ������ƶ��ٶ�
        for(int i = 0; i < speedList.Count; i++)
        {
            if(HeadSculpture.playerLevel.text == i.ToString())
            {
                moveSpeed = speedList[i]; // �޸��ƶ��ٶ�
            }
        }
        // ����
        float horizontal = Input.GetAxis("Horizontal"); // ����1��-1
        float vertical = Input.GetAxis("Vertical");

        // ��ɫ���ƶ�����ת
        transform.Translate(new Vector3(0, 0, vertical* moveSpeed) * Time.deltaTime);
        transform.Rotate(new Vector3(0, horizontal * rotateSpeed, 0) * Time.deltaTime);

        // ��ɫ�ܶ�����ʱ���л��ܲ�����Running
        Vector3 move = new Vector3(0, 0, vertical);
        PlayerAnim.SetFloat("Speed", move.magnitude);

        // ��������
        Vector3 offSet = new Vector3(0, 1, 0); // ƫ����
        Ray ray = new Ray();
        ray.origin = transform.position + offSet;   //�������
        ray.direction = transform.forward; //���߷���
        isOpenDoor = Physics.Raycast(ray, out hitInfo, raycastDistance, DoorLayer);
        if (isOpenDoor) // ���Ŷ���
        {
            PlayerAnim.Play("Open_Door_Outwards");
        }
        // �������������
        //Debug.DrawRay(transform.position + offSet,
        //transform.forward * raycastDistance, Color.green);
    }

    private void OnTriggerEnter(Collider other) // ��������ҵȼ�+1
    {
        if(other.tag == "Upgrade")
        {
            int level = int.Parse(HeadSculpture.playerLevel.text) + 1;
            if(level < 10) // ���9��
            {
                HeadSculpture.playerLevel.text = level.ToString();
            }
            
        }
    }
    

    // ��¼����˳���Ϸʱ����Ϣ
    public class PlayerData {
        public float[] Position; // �洢���ǵ�λ����Ϣ
        public float[] Rotation; // �洢���ǵ���ת��Ϣ
        public string PlayerName; // ������Ϸ����
        public string PlayerLevel; // �������ǵȼ�
    }

    // ÿ���˳���Ϸ�����ᱣ��һ����Ϣ�������Ǹ���ʽ�ı���
    private void OnDestroy()
    {
        // ���ǵ�λ����Ϣ
        PlayerData playerData = new PlayerData();
        float[] position = new float[] { transform.position.x, transform.position.y, transform.position.z };
        playerData.Position = position; // ��¼λ����Ϣ

        // Unity�༭���������ʾ����תֵ��ŷ���ǵ���ʽ���֣�ͨ��Ϊ�����ᣨX��Y��Z���ϵĽǶ�ֵ��
        // transform.rotation����һ����Ԫ��������һ�ָ����ӵ�����Ч�ر�ʾ��ת�ķ�ʽ��
        Vector3 eulerRotation = transform.rotation.eulerAngles; // ����Ԫ��ת����ŷ����
        float[] rotation = new float[] { eulerRotation.x, eulerRotation.y, eulerRotation.z };
        playerData.Rotation = rotation; // ��¼��ת��Ϣ

        // �����ı���Ϣ
        playerData.PlayerName = HeadSculpture.playerName.text; // �������
        playerData.PlayerLevel = HeadSculpture.playerLevel.text;  // ��ҵȼ�

        // ����Json�ļ���
        if (!Directory.Exists(Application.dataPath + "/Json"))
        {
            Directory.CreateDirectory(Application.dataPath + "/Json");
        }
        //д������
        //���л��������ݶ���ת����Json�ı�
        string json = JsonConvert.SerializeObject(playerData);
        string path = Application.dataPath + "/Json/Player.json"; // �ļ�λ��

        //����һ���ı��ļ���������һ��д����ļ�������
       StreamWriter writer = File.CreateText(path); // ����Player.json�ı�
        writer.Write(json);  // ʹ�ļ�������Json��ʽ���ı�д���ļ���
        writer.Close(); // �ر��ļ�������ֹһֱռ���ļ�
        writer.Dispose(); // �����ļ��������ͷ��ڴ�
    }
}
