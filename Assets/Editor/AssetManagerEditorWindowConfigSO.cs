using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

//�����˵�
[CreateAssetMenu(fileName = "AssetManagerEditorWindowConfig", menuName = "AssetManager/CreateWindowConfig")]

//������ScriptableObject����
//���úʹ洢��༭��������ص���ʽ����Դ
public class AssetManagerEditorWindowConfigSO : ScriptableObject
{
    public GUIStyle TitleTextStyle;//���ڶ���༭�����ڱ�����ı���ʽ
    public GUIStyle VersionTextStyle;//���ڶ���汾�ı�����ʽ
    public Texture2D LogoTexture;//���ڴ洢�༭�����ڵı���ͼ��
    public GUIStyle LogoTextureStyle;//���ڶ��屳��ͼ�����ʽ
}
