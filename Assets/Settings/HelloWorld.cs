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

public class AssetBundleVersionDifference
{
    /// <summary>
    /// ������Դ��
    /// </summary>
    public List<string> AdditionAssetBundles = new List<string>();
    /// <summary>
    /// ������Դ��
    /// </summary>
    public List<string> ReducedAssetBundles = new List<string>();
}

/// <summary>
/// ��ΪAssets�µ������ű��ᱻ���뵽AssemblyCsharp.dll��
/// �����Ű�������ȥ����APK��,���Բ�����ʹ������UnityEditor�����ռ��µķ���
/// </summary>
public class HelloWorld : MonoBehaviour
{

    public AssetBundlePattern LoadPattern;

    public Button button;

    public string HTTPAddress = "http://192.168.203.48:808/";

    string RemoteVersionPath;

    string DownloadVersionPath;

    public Text progressTxt;

    public Text versionTxt;

    public Text displayText;

    public Text tipTxt;



    void Start()
    {
        button.onClick.AddListener(LoadAsset);

        AssetManagerRuntime.AssetManagerInit(LoadPattern);

        if (LoadPattern == AssetBundlePattern.Remote)
        {
            StartCoroutine(GetRemoteVersion());
        }

    }

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

    void OnProgress(float progress, long currentLength, long totalLength)
    {
        progressTxt.text = $"���ؽ��ȣ�{progress * 100}%,��ǰ���س���{currentLength * 1.0f / 1024 / 1024}M���ļ��ܳ��ȣ�{totalLength * 1.0f / 1024 / 1024}M";
        Debug.Log($"���ؽ��ȣ�{progress * 100}%,��ǰ���س���{currentLength * 1.0f / 1024 / 1024}M���ļ��ܳ��ȣ�{totalLength * 1.0f / 1024 / 1024}M");
    }

    void OnError(ErrorCode errorCode, string message)
    {
        Debug.LogError(message);
    }

    void CreateAssetBundleDownloadList()
    {
        //���ر��ȡ·��
        string assetBundleHashsLoadPath = Path.Combine(AssetManagerRuntime.Instance.AssetBundleLoadPath, "AssetBundleHashs");
        string[] localAssetBundleHashs = null;

        if (File.Exists(assetBundleHashsLoadPath))
        {
            string assetBundleHashsString = File.ReadAllText(assetBundleHashsLoadPath);
            localAssetBundleHashs = JsonConvert.DeserializeObject<string[]>(assetBundleHashsString);
        }

        //Զ�˰��б�
        string romoteHashPath = Path.Combine(DownloadVersionPath, "AssetBundleHashs");
        string remoteHashString = File.ReadAllText(romoteHashPath);
        string[] remoteAssetBundleHashs = JsonConvert.DeserializeObject<string[]>(remoteHashString);


        //���θ�����Ҫ���ص�AB��
        List<string> downloadAssetNames = null;
        if (localAssetBundleHashs == null)
        {
            Debug.Log("���ض�ȡʧ�ܣ�ֱ������Զ�˱�");
            downloadAssetNames = remoteAssetBundleHashs.ToList();
        }
        else
        {
            AssetBundleVersionDifference difference = ContrastAssetBundleVersion(localAssetBundleHashs, remoteAssetBundleHashs);
            downloadAssetNames = difference.AdditionAssetBundles;
        }

        //�����������
        downloadAssetNames.Add("LocalAssets");
        Downloader downloader = null;

        //ʵ���ϵ�AssetBundle����������������
        foreach (string assetBundleName in downloadAssetNames)
        {
            string fileName = assetBundleName;
            if (assetBundleName.Contains("_"))
            {
                //�»������һλ����AssetbundleName
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

                OnCompleted(fileName, "�����Ѵ���");
            }
        }


    }
    void CreatePackagesDownloadList()
    {
        string allPackagesPath = Path.Combine(DownloadVersionPath, "AllPackages");
        string allPackagesString = File.ReadAllText(allPackagesPath);
        List<string> allPackages = JsonConvert.DeserializeObject<List<string>>(allPackagesString);

        Downloader downloader = null;

        foreach (string packageName in allPackages)
        {
            if (!CurrentDownloadInfo.DownloadFileName.Contains(packageName))
            {
                string remoteFilePath = Path.Combine(RemoteVersionPath, packageName);
                string remotePackageSavePath = Path.Combine(DownloadVersionPath, packageName);
                downloader = new Downloader(remoteFilePath, remotePackageSavePath, OnCompleted, OnProgress, OnError);
                downloader.StartDownload();
            }
            else
            {

                OnCompleted(packageName, "�����Ѿ�����");
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

            OnCompleted("AssetBundleHashs", "�����Ѿ�����");
        }
    }
    BuildInfo RemoteBuildInfo;
    IEnumerator GetRemoteVersion()
    {
        #region ��ȡԶ�˰汾��
        string remoteVersionFilePath = Path.Combine(HTTPAddress, "BuildOutput", "BuildVersion.version");

        UnityWebRequest request = UnityWebRequest.Get(remoteVersionFilePath);

        request.SendWebRequest();

        while (!request.isDone)
        {
            //����null����ȴ�һ֡
            yield return null;
        }
        if (!string.IsNullOrEmpty(request.error))
        {
            Debug.LogError(request.error);
            yield break;
        }

        int version = int.Parse(request.downloadHandler.text);
        if (AssetManagerRuntime.Instance.LocalAssetVersion == version)
        {
            Debug.Log("�汾һ�����ý�������");
            tipTxt.text = "�汾һ�����ý�������";
            if (button)
            {

                //LoadAsset();
            }

            yield break;
        }
        //ʹ�ñ�������Զ�˰汾
        AssetManagerRuntime.Instance.RemoteAssetVersion = version;
        #endregion
        RemoteVersionPath = Path.Combine(HTTPAddress, "BuildOutput", AssetManagerRuntime.Instance.RemoteAssetVersion.ToString());
        DownloadVersionPath = Path.Combine(AssetManagerRuntime.Instance.DownloadPath, AssetManagerRuntime.Instance.RemoteAssetVersion.ToString());

        if (!Directory.Exists(DownloadVersionPath))
        {
            Directory.CreateDirectory(DownloadVersionPath);
        }
        Debug.Log(DownloadVersionPath);

        versionTxt.text = "Զ����Դ�汾��Ϊ" + version;
        Debug.Log($"Զ����Դ�汾{version}");

        #region ��ȡԶ��BuildInfo

        string remoteBuildInfoPath = Path.Combine(HTTPAddress, "BuildOutput", version.ToString(), "BuildInfo");

        request = UnityWebRequest.Get(remoteBuildInfoPath);

        request.SendWebRequest();

        while (!request.isDone)
        {
            //����null����ȴ�һ֡
            yield return null;
        }
        if (!string.IsNullOrEmpty(request.error))
        {
            Debug.LogError(request.error);
            yield break;
        }

        string buildInfoString = request.downloadHandler.text;
        RemoteBuildInfo = JsonConvert.DeserializeObject<BuildInfo>(buildInfoString);

        if (RemoteBuildInfo == null || RemoteBuildInfo.FileTotalSize <= 0)
        {
            yield break;
        }
        #endregion
        CreateDownloadList();


    }


    void CreateDownloadList()
    {
        //���ȶ�ȡ���������б�
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

        //���Ȼ���Ҫ����AllPackages�Լ�Packages
        //������Ҫ�����ж�AllPackages�Ƿ��Ѿ����أ�
        if (CurrentDownloadInfo.DownloadFileName.Contains("AllPackages"))
        {

            OnCompleted("AllPackages", "�����Ѵ���");
        }
        else
        {
            string filePath = Path.Combine(RemoteVersionPath, "AllPackages");
            string savePath = Path.Combine(DownloadVersionPath, "AllPackages");
            Downloader downloader = new Downloader(filePath, savePath, OnCompleted, OnProgress, OnError);
            downloader.StartDownload();
        }

    }

    void CopyDownloadAssetsToLocalPath()
    {
        DirectoryInfo directoryInfo = new DirectoryInfo(DownloadVersionPath);

        string localVersionPath = Path.Combine(AssetManagerRuntime.Instance.LocalAssetPath, AssetManagerRuntime.Instance.RemoteAssetVersion.ToString());

        directoryInfo.MoveTo(localVersionPath);

        string downoadInfoPath = Path.Combine(AssetManagerRuntime.Instance.DownloadPath, DownloadInfoFileName);
        File.Delete(downoadInfoPath);
    }
    AssetBundleVersionDifference ContrastAssetBundleVersion(string[] oldVersionAssets, string[] newVersionAssets)
    {
        AssetBundleVersionDifference difference = new AssetBundleVersionDifference();
        foreach (var assetName in oldVersionAssets)
        {
            if (newVersionAssets.Contains(assetName))
            {
                difference.ReducedAssetBundles.Add(assetName);
            }
        }

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




