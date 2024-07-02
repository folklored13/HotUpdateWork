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
    /// 编辑器模拟加载，应该使用AssetDataBase进行资源加载，而不用进行打包
    /// </summary>
    EditorSimulation,
    /// <summary>
    /// 本地加载模式，应打包到本地路径或StreamingAssets路径下，从该路径加载
    /// </summary>
    Local,
    /// <summary>
    /// 远端加载模式，应打包到任意资源服务器地址，然后通过网络进行下载
    /// 下载到沙盒路径persistentDataPath后，再进行加载
    /// </summary>
    Remote
}

/// <summary>
/// 压缩模式
/// </summary>
public enum AssetBundleCompressionPattern
{
    LZMA,
    LZ4,
    None
}

/// <summary>
/// 任何BuildOption处于非forceRebuild选项下都默认为增量打包
/// </summary>
public enum IncrementalBuildMode
{
    None,
    IncrementalBuild,
    ForceRebuild
}

/// <summary>
/// Package打包后记录的信息
/// </summary>
/// 

//存储有关单个资源包的名称、资源列表、依赖关系和是否为初始包的信息
public class PackageBuildInfo
{
    public string PackageName;
    public List<AssetBuildInfo> AssetIfos = new List<AssetBuildInfo>();
    public List<string> PackageDependecies = new List<string>();
    /// <summary>
    /// 代表是否是初始包
    /// </summary>
    public bool IsSourcePackage = false;

}
/// <summary>
/// Package中的Asset打包之后记录的信息
/// </summary>
public class AssetBuildInfo
{
    /// <summary>
    /// 资源名称，当需要加载资源是，应该和该字符串相同
    /// </summary>
    public string AssetName;
    /// <summary>
    /// 该资源属于哪个AssetBundle
    /// </summary>
    public string AssetBundleName;
}

/// <summary>
/// 封装了资源包的加载逻辑
/// </summary>
public class AssetPackage
{
    public PackageBuildInfo PackageInfo;
    public string PackageName {get{ return PackageInfo.PackageName; } }//返回 PackageInfo 中的包名称

    Dictionary<string, Object> LoadedAssets = new Dictionary<string, Object>();//用于存储已经加载的资源，其中键是资源名称，值是资源对象。

    /// <summary>
    /// 用于加载指定名称的场景
    /// </summary>
    /// <param name="SceneName">场景名</param>
    public void LoadScene(string SceneName)
    {
        bool isHasScene = false;//是否找到了指定的场景

        //检查每个 AssetBuildInfo 对象的 AssetName 是否与传入的 SceneName 匹配。
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

    //定义了一个泛型方法 LoadAsset，用于加载指定名称的资源，并返回该资源
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
            Debug.LogError($"{assetName}未在{PackageName}中找到");
        }
        return assetObject;
    }
    
}

/// <summary>
/// 定义了一个名为 BuildInfo的类
/// 作用是存储有关构建版本的信息，包括文件名及其哈希值、总文件大小
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
    /// 当前类的单例
    /// </summary>
    public static AssetManagerRuntime Instance;
    /// <summary>
    /// 当前资源包模式
    /// </summary>
    AssetBundlePattern CurrentPattern;

    /// <summary>
    /// 所有本地Asset所处的路径应为AssetBundleLoadPath的上一层
    /// </summary>
    public string LocalAssetPath;

    /// <summary>
    /// AssetBundle记载路径
    /// </summary>
   public  string AssetBundleLoadPath;

    /// <summary>
    /// 资源下载路径，下载完成后应该将资源放置到LocalAssetPath中
    /// </summary>
    public string DownloadPath;

    /// <summary>
    /// 用于对比本地资源版本的远端资源版本号
    /// </summary>
    public int LocalAssetVersion;

    /// <summary>
    /// 本次运行访问到的资源服务器版本号
    /// </summary>
    public int RemoteAssetVersion;
    /// <summary>
    /// 本地所有的Package信息
    /// </summary>
    List<string> PackageNames ;

    /// <summary>
    /// 代表所有已经加载的Package
    /// </summary>
    Dictionary<string,AssetPackage> LoadedAssetPackages = new Dictionary<string, AssetPackage>();


    public AssetBundleManifest Mainfest; //用于存储资源包的清单信息

    /// <summary>
    /// 初始化资源管理器实例，并设置资源加载模式
    /// </summary>
    /// <param name="pattern">加载模式</param>
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
    /// 根据不同的资源加载模式设置本地资源路径和下载路径
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
    /// 检查本地资源版本，并在本地版本文件不存在时创建它
    /// </summary>
    void CheckLocalAssetVersion()
    {
        //asset.version是由我们自定义拓展名的文本文件
        string versionFilePath = Path.Combine(LocalAssetPath, "LocalVersion.version");

        // 确保 LocalAssetPath 存在
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
            // 处理 DirectoryNotFoundException 异常
            System.Console.WriteLine("指定的路径不存在，请检查路径是否正确。");
        }
    }

    /// <summary>
    /// 设置资源包加载路径
    /// </summary>
    void CheckAssetBundleLoadPath()
    {
        AssetBundleLoadPath = Path.Combine(LocalAssetPath, LocalAssetVersion.ToString());
    }

    /// <summary>
    /// 用于更新本地资源版本号，并更新本地版本文件
    /// </summary>
    public void UpdataLocalAssetVersion()
    {
        LocalAssetVersion = RemoteAssetVersion;
        string versionFilePath = Path.Combine(LocalAssetPath, "LocalVersion.version");
        File.WriteAllText(versionFilePath, LocalAssetVersion.ToString());
        CheckAssetBundleLoadPath();
        Debug.Log($"本地版本更新完成{LocalAssetVersion}");
    }

    /// <summary>
    /// 用于加载指定名称的资源包
    /// </summary>
    /// <param name="packageName"></param>
    /// <returns></returns>
    public AssetPackage LoadPackage(string packageName)
    {
        string packagePath = null;//资源包路径
        string packageString = null;//资源包内容
        if (PackageNames == null)
        {
            packagePath = Path.Combine(AssetBundleLoadPath, "AllPackages");
            packageString = File.ReadAllText(packagePath);
            PackageNames = JsonConvert.DeserializeObject<List<string>>(packageString);
        }
        //检查包是否存在
        if (!PackageNames.Contains(packageName))
        {
            Debug.LogError($"{packageName}本地包列表中不存在该包");
            return null;
        }
        //加载资源包清单
        if(Mainfest==null)
        {
            string mainBundlePath = Path.Combine(AssetBundleLoadPath, "LocalAssets");
            AssetBundle mainBundle = AssetBundle.LoadFromFile(mainBundlePath);
            Mainfest= mainBundle.LoadAsset<AssetBundleManifest>(nameof(AssetBundleManifest));
        }

        AssetPackage assetPackage=null;
        //检查是否已经加载
        if(LoadedAssetPackages.ContainsKey(packageName))
        {
            assetPackage = LoadedAssetPackages[packageName];
            Debug.LogWarning($"{packageName}已经加载");
            return assetPackage;
        }
        assetPackage = new AssetPackage();

        //加载包信息
        packagePath = Path.Combine(AssetBundleLoadPath, packageName);
        packageString = File.ReadAllText(packagePath);
        assetPackage.PackageInfo = JsonConvert.DeserializeObject<PackageBuildInfo>(packageString);

        LoadedAssetPackages.Add(assetPackage.PackageName, assetPackage);
        //加载依赖包
        foreach(string dependName in assetPackage.PackageInfo.PackageDependecies)
        {
            LoadPackage(dependName);
        }
        return assetPackage;
    }
}
