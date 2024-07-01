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


public class AssetPackage
{
    public PackageBuildInfo PackageInfo;
    public string PackageName {get{ return PackageInfo.PackageName; } }

    Dictionary<string, Object> LoadedAssets = new Dictionary<string, Object>();


    public void LoadScene(string SceneName)
    {
        bool isHasScene = false;
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


    public AssetBundleManifest Mainfest;

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

    void CheckAssetBundleLoadPath()
    {
        AssetBundleLoadPath = Path.Combine(LocalAssetPath, LocalAssetVersion.ToString());
    }
    public void UpdataLocalAssetVersion()
    {
        LocalAssetVersion = RemoteAssetVersion;
        string versionFilePath = Path.Combine(LocalAssetPath, "LocalVersion.version");
        File.WriteAllText(versionFilePath, LocalAssetVersion.ToString());
        CheckAssetBundleLoadPath();
        Debug.Log($"���ذ汾�������{LocalAssetVersion}");
    }

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
