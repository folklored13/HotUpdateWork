using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

//创建菜单
[CreateAssetMenu(fileName = "AssetManagerEditorWindowConfig", menuName = "AssetManager/CreateWindowConfig")]

//派生自ScriptableObject的类
//配置和存储与编辑器窗口相关的样式和资源
public class AssetManagerEditorWindowConfigSO : ScriptableObject
{
    public GUIStyle TitleTextStyle;//用于定义编辑器窗口标题的文本样式
    public GUIStyle VersionTextStyle;//用于定义版本文本的样式
    public Texture2D LogoTexture;//用于存储编辑器窗口的背景图像
    public GUIStyle LogoTextureStyle;//用于定义背景图像的样式
}
