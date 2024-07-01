using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum AssetBundlePattern
{
    /// <summary>
    /// �༭��ģ����أ�Ӧ��ʹ��AssetDataBase������Դ���أ������ý��д��
    /// </summary>
    EditorSimulation,
    /// <summary>
    /// ���ؼ���ģʽ��Ӧ���������·����StreamingAssets·���£��Ӹ�·������
    /// </summary>
    Local,
    /// <summary>
    /// Զ�˼���ģʽ��Ӧ�����������Դ��������ַ��Ȼ��ͨ�������������
    /// ���ص�ɳ��·��persistentDataPath���ٽ��м���
    /// </summary>
    Remote
}

/// <summary>
/// ѹ��ģʽ
/// </summary>
public enum AssetBundleCompressionPattern
{
    LZMA,
    LZ4,
    None
}

/// <summary>
/// �κ�BuildOption���ڷ�forceRebuildѡ���¶�Ĭ��Ϊ�������
/// </summary>
public enum IncrementalBuildMode
{
    None,
    IncrementalBuild,
    ForceRebuild
}

/// <summary>
/// Package������¼����Ϣ
/// </summary>
/// 

//�洢�йص�����Դ�������ơ���Դ�б�������ϵ���Ƿ�Ϊ��ʼ������Ϣ
public class PackageBuildInfo
{
    public string PackageName;
    public List<AssetBuildInfo> AssetIfos = new List<AssetBuildInfo>();
    public List<string> PackageDependecies = new List<string>();
    /// <summary>
    /// �����Ƿ��ǳ�ʼ��
    /// </summary>
    public bool IsSourcePackage = false;

}
/// <summary>
/// Package�е�Asset���֮���¼����Ϣ
/// </summary>
public class AssetBuildInfo
{
    /// <summary>
    /// ��Դ���ƣ�����Ҫ������Դ�ǣ�Ӧ�ú͸��ַ�����ͬ
    /// </summary>
    public string AssetName;
    /// <summary>
    /// ����Դ�����ĸ�AssetBundle
    /// </summary>
    public string AssetBundleName;
}

/// <summary>
/// ��װ����Դ���ļ����߼�
/// </summary>
public class AssetPackage
{
    public PackageBuildInfo PackageInfo;
    public string PackageName {get{ return PackageInfo.PackageName; } }//���� PackageInfo �еİ�����

    Dictionary<string, Object> LoadedAssets = new Dictionary<string, Object>();//���ڴ洢�Ѿ����ص���Դ�����м�����Դ���ƣ�ֵ����Դ����

    /// <summary>
    /// ���ڼ���ָ�����Ƶĳ���
    /// </summary>
    /// <param name="SceneName">������</param>
    public void LoadScene(string SceneName)
    {
        bool isHasScene = false;//�Ƿ��ҵ���ָ���ĳ���

        //���ÿ�� AssetBuildInfo ����� AssetName �Ƿ��봫��� SceneName ƥ�䡣
        foreach (AssetBuildInfo info in PackageInfo.AssetIfos)
        {
            if (info.AssetName == SceneName)
            {
                foreach (string dependAssetName in AssetManagerRuntime.Instance.Mainfest.GetAllDependencies(info.AssetBundleName))
                {
                    string dependAssetBundlePath = Path.Combine(AssetManagerRuntime.Instance.AssetBundleLoadPath, dependAssetName);

                    AssetBundle.LoadFromFile(dependAssetBundlePath);
                }

                string assetBundlePath = Path.Combine(AssetManagerRuntime.Instance.AssetBundleLoadPath, info.AssetBundleName);

                AssetBundle bundle = AssetBundle.LoadFromFile(assetBundlePath);
                isHasScene = true;
            }
        }
        if (isHasScene)
        {
            SceneManager.LoadScene(SceneName);
        }
    }

    //������һ�����ͷ��� LoadAsset�����ڼ���ָ�����Ƶ���Դ�������ظ���Դ
    public T LoadAsset<T>(string assetName) where T:Object
    {
        T assetObject = default;
        foreach (AssetBuildInfo info in PackageInfo.AssetIfos)
        {
            if(info.AssetName==assetName)
            {
                if(LoadedAssets.ContainsKey(assetName))
                {
                    assetObject = LoadedAssets[assetName] as T;
                    return assetObject;
                }

                foreach(string dependAssetName in AssetManagerRuntime.Instance.Mainfest.GetAllDependencies(info.AssetBundleName))
                {
                    string dependAssetBundlePath = Path.Combine(AssetManagerRuntime.Instance.AssetBundleLoadPath,dependAssetName);

                    AssetBundle.LoadFromFile(dependAssetBundlePath);
                }

                string assetBundlePath= Path.Combine(AssetManagerRuntime.Instance.AssetBundleLoadPath, info.AssetBundleName);

                AssetBundle bundle = AssetBundle.LoadFromFile(assetBundlePath);
                assetObject = bundle.LoadAsset<T>(assetName);
            }
        }

        if (assetObject==null)
        {
            Debug.LogError($"{assetName}δ��{PackageName}���ҵ�");
        }
        return assetObject;
    }
    
}

/// <summary>
/// ������һ����Ϊ BuildInfo����
/// �����Ǵ洢�йع����汾����Ϣ�������ļ��������ϣֵ�����ļ���С
/// </summary>
public class BuildInfo
{
    public int BuildVersion;
    public Dictionary<string, ulong> FileNames = new Dictionary<string, ulong>();
    public ulong FileTotalSize;
}

public class AssetManagerRuntime 
{
    /// <summary>
    /// ��ǰ��ĵ���
    /// </summary>
    public static AssetManagerRuntime Instance;
    /// <summary>
    /// ��ǰ��Դ��ģʽ
    /// </summary>
    AssetBundlePattern CurrentPattern;

    /// <summary>
    /// ���б���Asset������·��ӦΪAssetBundleLoadPath����һ��
    /// </summary>
    public string LocalAssetPath;

    /// <summary>
    /// AssetBundle����·��
    /// </summary>
   public  string AssetBundleLoadPath;

    /// <summary>
    /// ��Դ����·����������ɺ�Ӧ�ý���Դ���õ�LocalAssetPath��
    /// </summary>
    public string DownloadPath;


    /// <summary>
    /// ���ڶԱȱ�����Դ�汾��Զ����Դ�汾��
    /// </summary>
    public int LocalAssetVersion;

    /// <summary>
    /// �������з��ʵ�����Դ�������汾��
    /// </summary>
    public int RemoteAssetVersion;
    /// <summary>
    /// �������е�Package��Ϣ
    /// </summary>
    List<string> PackageNames ;

    /// <summary>
    /// ���������Ѿ����ص�Package
    /// </summary>
    Dictionary<string,AssetPackage> LoadedAssetPackages = new Dictionary<string, AssetPackage>();


    public AssetBundleManifest Mainfest; //���ڴ洢��Դ�����嵥��Ϣ

    /// <summary>
    /// ��ʼ����Դ������ʵ������������Դ����ģʽ
    /// </summary>
    /// <param name="pattern">����ģʽ</param>
    public static void AssetManagerInit(AssetBundlePattern pattern)
    {
        if(Instance==null)
        {
            Instance = new AssetManagerRuntime();
            Instance.CurrentPattern = pattern;
            Instance.CheckLoadAssetPath();
            Instance.CheckLocalAssetVersion();
            Instance.CheckAssetBundleLoadPath();
        }
    }

    /// <summary>
    /// ���ݲ�ͬ����Դ����ģʽ���ñ�����Դ·��������·��
    /// </summary>
    void CheckLoadAssetPath()
    {
        switch(CurrentPattern)
        {
            case AssetBundlePattern.EditorSimulation:
                break;
            case AssetBundlePattern.Local:
                LocalAssetPath = Path.Combine(Application.streamingAssetsPath,"BuildOutput");
                break;
            case AssetBundlePattern.Remote:
                DownloadPath = Path.Combine(Application.persistentDataPath, "DownloadAssets");
                LocalAssetPath = Path.Combine(Application.persistentDataPath, "BuildOutput");
                break;
        }
    }

    /// <summary>
    /// ��鱾����Դ�汾�����ڱ��ذ汾�ļ�������ʱ������
    /// </summary>
    void CheckLocalAssetVersion()
    {
        //asset.version���������Զ�����չ�����ı��ļ�
        string versionFilePath = Path.Combine(LocalAssetPath, "LocalVersion.version");

        // ȷ�� LocalAssetPath ����
        if (!Directory.Exists(LocalAssetPath))
        {
            Directory.CreateDirectory(LocalAssetPath);
        }

        try
        {
            if (!File.Exists(versionFilePath))
            {
                LocalAssetVersion = 100;
                File.WriteAllText(versionFilePath, LocalAssetVersion.ToString());
                return;
            }
            LocalAssetVersion = int.Parse(File.ReadAllText(versionFilePath));
        }
        catch (DirectoryNotFoundException)
        {
            // ���� DirectoryNotFoundException �쳣
            System.Console.WriteLine("ָ����·�������ڣ�����·���Ƿ���ȷ��");
        }
    }

    /// <summary>
    /// ������Դ������·��
    /// </summary>
    void CheckAssetBundleLoadPath()
    {
        AssetBundleLoadPath = Path.Combine(LocalAssetPath, LocalAssetVersion.ToString());
    }

    /// <summary>
    /// ���ڸ��±�����Դ�汾�ţ������±��ذ汾�ļ�
    /// </summary>
    public void UpdataLocalAssetVersion()
    {
        LocalAssetVersion = RemoteAssetVersion;
        string versionFilePath = Path.Combine(LocalAssetPath, "LocalVersion.version");
        File.WriteAllText(versionFilePath, LocalAssetVersion.ToString());
        CheckAssetBundleLoadPath();
        Debug.Log($"���ذ汾�������{LocalAssetVersion}");
    }

    /// <summary>
    /// ���ڼ���ָ�����Ƶ���Դ��
    /// </summary>
    /// <param name="packageName"></param>
    /// <returns></returns>
    public AssetPackage LoadPackage(string packageName)
    {
        string packagePath = null;
        string packageString = null;
        if (PackageNames == null)
        {
            packagePath = Path.Combine(AssetBundleLoadPath, "AllPackages");
            packageString = File.ReadAllText(packagePath);
            PackageNames = JsonConvert.DeserializeObject<List<string>>(packageString);
        }

        if(!PackageNames.Contains(packageName))
        {
            Debug.LogError($"{packageName}���ذ��б��в����ڸð�");
            return null;
        }

        if(Mainfest==null)
        {
            string mainBundlePath = Path.Combine(AssetBundleLoadPath, "LocalAssets");
            AssetBundle mainBundle = AssetBundle.LoadFromFile(mainBundlePath);
            Mainfest= mainBundle.LoadAsset<AssetBundleManifest>(nameof(AssetBundleManifest));
        }

        AssetPackage assetPackage=null;
        if(LoadedAssetPackages.ContainsKey(packageName))
        {
            assetPackage = LoadedAssetPackages[packageName];
            Debug.LogWarning($"{packageName}�Ѿ�����");
            return assetPackage;
        }
        assetPackage = new AssetPackage();

        packagePath= Path.Combine(AssetBundleLoadPath, packageName);
        packageString = File.ReadAllText(packagePath);
        assetPackage.PackageInfo = JsonConvert.DeserializeObject<PackageBuildInfo>(packageString);

        LoadedAssetPackages.Add(assetPackage.PackageName, assetPackage);

        foreach(string dependName in assetPackage.PackageInfo.PackageDependecies)
        {
            LoadPackage(dependName);
        }
        return assetPackage;
    }


}
