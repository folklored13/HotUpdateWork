using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 用于存储资源包版本差异的结果
/// </summary>
public class AssetBundleVersionDifference
{
    /// <summary>
    /// 新增资源包
    /// </summary>
    public List<string> AdditionAssetBundles = new List<string>();
    /// <summary>
    /// 减少资源包
    /// </summary>
    public List<string> ReducedAssetBundles = new List<string>();
}

/// <summary>
/// 因为Assets下的其他脚本会被编译到AssemblyCsharp.dll中
/// 跟随着包体打包出去（如APK）,所以不允许使用来自UnityEditor命名空间下的方法
/// </summary>
public class HelloWorld : MonoBehaviour
{

    public AssetBundlePattern LoadPattern;//声明加载模式

    public Button button;//点击进入场景按钮

    public string HTTPAddress = "http://192.168.203.48:808/";//服务器地址

    string RemoteVersionPath;//远端版本地址

    string DownloadVersionPath;//下载版本地址

    public Text progressTxt;//下载进度文本

    public Text versionTxt;//版本号文本

    public Text displayText;//展示是否存在

    public Text tipTxt;//提示

    void Start()
    {
        button.onClick.AddListener(LoadAsset);

        AssetManagerRuntime.AssetManagerInit(LoadPattern);

        if (LoadPattern == AssetBundlePattern.Remote)
        {
            StartCoroutine(GetRemoteVersion());
        }

    }

    /// <summary>
    /// 加载场景
    /// </summary>
    void LoadAsset()
    {
        AssetPackage assetPackage = AssetManagerRuntime.Instance.LoadPackage("A");

        Debug.Log(assetPackage.PackageName);

        assetPackage.LoadScene("Assets/Scenes/SchoolSceneDay.unity");
    }


    string FileURL;
    string FileSavePath;

    DownloadInfo CurrentDownloadInfo;
    string DownloadInfoFileName = "TempDownloadInfo";

    List<string> downloadMessages = new List<string>();

    /// <summary>
    /// 用于显示下载完成后的文件存在提示
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="message"></param>
    void OnCompleted(string fileName, string message)
    {

        if (!CurrentDownloadInfo.DownloadFileName.Contains(fileName))
        {
            CurrentDownloadInfo.DownloadFileName.Add(fileName);
            string downloadInfoString = JsonConvert.SerializeObject(CurrentDownloadInfo);
            string downloadSavePath = Path.Combine(AssetManagerRuntime.Instance.DownloadPath, DownloadInfoFileName);
            File.WriteAllText(downloadSavePath, downloadInfoString);
        }

        switch (fileName)
        {
            case "AllPackages":
                CreatePackagesDownloadList();
                break;
            case "AssetBundleHashs":
                CreateAssetBundleDownloadList();
                break;
        }
        downloadMessages.Add($"{fileName}:{message}");
        if (CurrentDownloadInfo.DownloadFileName.Count == RemoteBuildInfo.FileNames.Count)
        {
            CopyDownloadAssetsToLocalPath();

            AssetManagerRuntime.Instance.UpdataLocalAssetVersion();

            for (int i = 1; i < downloadMessages.Count; i++)
            {
                displayText.text += downloadMessages[i] + "\n\n";
                Debug.Log($"{fileName}:{message}");
            }
        }

    }
    /// <summary>
    /// 用于显示下载进度
    /// </summary>
    /// <param name="progress">进度值</param>
    /// <param name="currentLength">当前已下载长度</param>
    /// <param name="totalLength">文件总长度</param>
    void OnProgress(float progress, long currentLength, long totalLength)
    {
        progressTxt.text = $"下载进度：{progress * 100}%,当前下载长度{currentLength * 1.0f / 1024 / 1024}M，文件总长度：{totalLength * 1.0f / 1024 / 1024}M";
        Debug.Log($"下载进度：{progress * 100}%,当前下载长度{currentLength * 1.0f / 1024 / 1024}M，文件总长度：{totalLength * 1.0f / 1024 / 1024}M");
    }

    /// <summary>
    /// 用于打印错误信息
    /// </summary>
    /// <param name="errorCode"></param>
    /// <param name="message"></param>
    void OnError(ErrorCode errorCode, string message)
    {
        Debug.LogError(message);
    }

    /// <summary>
    /// 创建一个AssetBundle的下载列表，并根据这个列表来下载需要更新的资源包
    /// </summary>
    void CreateAssetBundleDownloadList()
    {
        //本地表读取路径
        string assetBundleHashsLoadPath = Path.Combine(AssetManagerRuntime.Instance.AssetBundleLoadPath, "AssetBundleHashs");
        string[] localAssetBundleHashs = null;

        if (File.Exists(assetBundleHashsLoadPath))
        {
            string assetBundleHashsString = File.ReadAllText(assetBundleHashsLoadPath);
            localAssetBundleHashs = JsonConvert.DeserializeObject<string[]>(assetBundleHashsString);
        }

        //远端包列表
        string romoteHashPath = Path.Combine(DownloadVersionPath, "AssetBundleHashs");
        string remoteHashString = File.ReadAllText(romoteHashPath);
        string[] remoteAssetBundleHashs = JsonConvert.DeserializeObject<string[]>(remoteHashString);


        //本次更新需要下载的AB包
        List<string> downloadAssetNames = null;
        if (localAssetBundleHashs == null)
        {
            Debug.Log("本地读取失败，直接下载远端表");
            downloadAssetNames = remoteAssetBundleHashs.ToList();
        }
        else
        {
            AssetBundleVersionDifference difference = ContrastAssetBundleVersion(localAssetBundleHashs, remoteAssetBundleHashs);
            downloadAssetNames = difference.AdditionAssetBundles;
        }

        //添加主包包名
        downloadAssetNames.Add("LocalAssets");
        Downloader downloader = null;

        //实际上的AssetBundle下载请求在这声明
        foreach (string assetBundleName in downloadAssetNames)
        {
            string fileName = assetBundleName;
            if (assetBundleName.Contains("_"))
            {
                //下化线最后一位才是AssetbundleName
                int startIndex = assetBundleName.IndexOf("_") + 1;

                fileName = assetBundleName.Substring(startIndex);
            }
            if (!CurrentDownloadInfo.DownloadFileName.Contains(fileName))
            {
                string fileURL = Path.Combine(RemoteVersionPath, fileName);
                string fileSavePath = Path.Combine(DownloadVersionPath, fileName);
                try
                {
                    downloader = new Downloader(fileURL, fileSavePath, OnCompleted, OnProgress, OnError);
                    downloader.StartDownload();
                }
                catch(Exception ex)
                {
                    Debug.Log(ex);
                }
            }
            else
            {
                OnCompleted(fileName, "本地已存在");
            }
        }
    }

    /// <summary>
    /// 生成一个下载列表，并根据这个列表下载所需的资源包
    /// </summary>
    void CreatePackagesDownloadList()
    {
        string allPackagesPath = Path.Combine(DownloadVersionPath, "AllPackages");//存储了包含所有包名的文件路径
        string allPackagesString = File.ReadAllText(allPackagesPath);//allPackagesPath 路径的文件中读取所有包名的字符串
        List<string> allPackages = JsonConvert.DeserializeObject<List<string>>(allPackagesString);//存储反序列化后的包名

        Downloader downloader = null;

        foreach (string packageName in allPackages)
        {
            if (!CurrentDownloadInfo.DownloadFileName.Contains(packageName))
            {
                string remoteFilePath = Path.Combine(RemoteVersionPath, packageName);//远程文件路径 
                string remotePackageSavePath = Path.Combine(DownloadVersionPath, packageName);//本地保存路径
                downloader = new Downloader(remoteFilePath, remotePackageSavePath, OnCompleted, OnProgress, OnError);
                downloader.StartDownload();
            }
            else
            {
                OnCompleted(packageName, "本地已经存在");
            }
        }

        if (!CurrentDownloadInfo.DownloadFileName.Contains("AssetBundleHashs"))
        {
            string remoteFilePath = Path.Combine(RemoteVersionPath, "AssetBundleHashs");
            string remoteFileSavePath = Path.Combine(DownloadVersionPath, "AssetBundleHashs");
            downloader = new Downloader(remoteFilePath, remoteFileSavePath, OnCompleted, OnProgress, OnError);
            downloader.StartDownload();
        }
        else
        {
            OnCompleted("AssetBundleHashs", "本地已经存在");
        }
    }

    BuildInfo RemoteBuildInfo;//存储从远程服务器获取的构建信息

    /// <summary>
    /// 获取远程服务器上的版本信息，比较本地版本和远程版本，决定是否需要进行更新
    /// </summary>
    /// <returns></returns>
    IEnumerator GetRemoteVersion()
    {
        #region 获取远端版本号
        string remoteVersionFilePath = Path.Combine(HTTPAddress, "BuildOutput", "BuildVersion.version");//存储了远程版本文件的路径

        UnityWebRequest request = UnityWebRequest.Get(remoteVersionFilePath);//发起一个GET请求到远程版本文件的URL

        request.SendWebRequest();//发送网络请求

        while (!request.isDone)
        {
            //返回null代表等待一帧
            yield return null;
        }
        if (!string.IsNullOrEmpty(request.error))
        {
            Debug.LogError(request.error);
            yield break;
        }

        //如果请求成功，解析返回的文本为整型版本号
        int version = int.Parse(request.downloadHandler.text);
        if (AssetManagerRuntime.Instance.LocalAssetVersion == version)
        {
            Debug.Log("版本一样不用进行下载");
            tipTxt.text = "版本一样不用进行下载";
            if (button)
            {
                //LoadAsset();
            }
            yield break;
        }

        //使用变量保存远端版本
        AssetManagerRuntime.Instance.RemoteAssetVersion = version;
        #endregion
        RemoteVersionPath = Path.Combine(HTTPAddress, "BuildOutput", AssetManagerRuntime.Instance.RemoteAssetVersion.ToString());//远程路径
        DownloadVersionPath = Path.Combine(AssetManagerRuntime.Instance.DownloadPath, AssetManagerRuntime.Instance.RemoteAssetVersion.ToString());//本地下载路径

        if (!Directory.Exists(DownloadVersionPath))
        {
            Directory.CreateDirectory(DownloadVersionPath);
        }
        Debug.Log(DownloadVersionPath);

        versionTxt.text = "远端资源版本现为" + version;
        Debug.Log($"远端资源版本{version}");

        #region 获取远端BuildInfo

        string remoteBuildInfoPath = Path.Combine(HTTPAddress, "BuildOutput", version.ToString(), "BuildInfo");//远端BuildInfo路径

        request = UnityWebRequest.Get(remoteBuildInfoPath);

        request.SendWebRequest();

        while (!request.isDone)
        {
            //返回null代表等待一帧
            yield return null;
        }
        if (!string.IsNullOrEmpty(request.error))
        {
            Debug.LogError(request.error);
            yield break;
        }

        string buildInfoString = request.downloadHandler.text;
        RemoteBuildInfo = JsonConvert.DeserializeObject<BuildInfo>(buildInfoString);//将返回的文本反序列化为 BuildInfo 对象

        if (RemoteBuildInfo == null || RemoteBuildInfo.FileTotalSize <= 0)
        {
            yield break;
        }
        #endregion
        CreateDownloadList();
    }

    /// <summary>
    /// 作用是初始化下载列表，并根据这个列表开始下载必要的文件
    /// </summary>
    void CreateDownloadList()
    {
        //首先读取本地下载列表

        //本地下载信息文件的路径，这个文件存储了之前下载的文件信息
        string downoadInfoPath = Path.Combine(AssetManagerRuntime.Instance.DownloadPath, DownloadInfoFileName);
        if (File.Exists(downoadInfoPath))
        {
            string downloadInfoString = File.ReadAllText(downoadInfoPath);
            CurrentDownloadInfo = JsonConvert.DeserializeObject<DownloadInfo>(downloadInfoString);
        }
        else
        {
            CurrentDownloadInfo = new DownloadInfo();
        }

        //首先还是要下载AllPackages以及Packages
        //所以需要首先判断AllPackages是否已经下载；
        if (CurrentDownloadInfo.DownloadFileName.Contains("AllPackages"))
        {

            OnCompleted("AllPackages", "本地已存在");
        }
        else
        {
            string filePath = Path.Combine(RemoteVersionPath, "AllPackages");
            string savePath = Path.Combine(DownloadVersionPath, "AllPackages");
            Downloader downloader = new Downloader(filePath, savePath, OnCompleted, OnProgress, OnError);//创建 Downloader 类的实例，设置下载完成、下载进度和错误处理的回调函数
            downloader.StartDownload();
        }

    }

    /// <summary>
    /// 将下载的资源从临时下载路径复制到本地资源路径，并清理下载信息文件
    /// </summary>
    void CopyDownloadAssetsToLocalPath()
    {
        DirectoryInfo directoryInfo = new DirectoryInfo(DownloadVersionPath);//表示下载资源所在的目录

        string localVersionPath = Path.Combine(AssetManagerRuntime.Instance.LocalAssetPath, AssetManagerRuntime.Instance.RemoteAssetVersion.ToString());//存储更新后的资源

        directoryInfo.MoveTo(localVersionPath);

        string downoadInfoPath = Path.Combine(AssetManagerRuntime.Instance.DownloadPath, DownloadInfoFileName);//记录了下载过程中的文件名信息
        File.Delete(downoadInfoPath);//删除下载信息文件
    }

    /// <summary>
    /// 识别出哪些资源包在新版本中被添加或删除了
    /// </summary>
    /// <param name="oldVersionAssets">旧版本的资源包名称数组</param>
    /// <param name="newVersionAssets">新版本的资源包名称数组</param>
    /// <returns></returns>
    AssetBundleVersionDifference ContrastAssetBundleVersion(string[] oldVersionAssets, string[] newVersionAssets)
    {
        AssetBundleVersionDifference difference = new AssetBundleVersionDifference();//创建实例difference，用于存储比较结果

        //遍历旧版本的资源包列表
        foreach (var assetName in oldVersionAssets)
        {
            if (newVersionAssets.Contains(assetName))
            {
                difference.ReducedAssetBundles.Add(assetName);
            }
        }
        //循环遍历新版本的资源包列表
        foreach (var assetName in newVersionAssets)
        {
            if (!oldVersionAssets.Contains(assetName))
            {
                difference.AdditionAssetBundles.Add(assetName);
            }
        }
        return difference;
    }
}




