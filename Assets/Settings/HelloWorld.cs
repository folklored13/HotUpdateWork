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
/// ���ڴ洢��Դ���汾����Ľ��
/// </summary>
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

    public AssetBundlePattern LoadPattern;//��������ģʽ

    public Button button;//������볡����ť

    public string HTTPAddress = "http://192.168.203.48:808/";//��������ַ

    string RemoteVersionPath;//Զ�˰汾��ַ

    string DownloadVersionPath;//���ذ汾��ַ

    public Text progressTxt;//���ؽ����ı�

    public Text versionTxt;//�汾���ı�

    public Text displayText;//չʾ�Ƿ����

    public Text tipTxt;//��ʾ

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
    /// ���س���
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
    /// ������ʾ������ɺ���ļ�������ʾ
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
    /// ������ʾ���ؽ���
    /// </summary>
    /// <param name="progress">����ֵ</param>
    /// <param name="currentLength">��ǰ�����س���</param>
    /// <param name="totalLength">�ļ��ܳ���</param>
    void OnProgress(float progress, long currentLength, long totalLength)
    {
        progressTxt.text = $"���ؽ��ȣ�{progress * 100}%,��ǰ���س���{currentLength * 1.0f / 1024 / 1024}M���ļ��ܳ��ȣ�{totalLength * 1.0f / 1024 / 1024}M";
        Debug.Log($"���ؽ��ȣ�{progress * 100}%,��ǰ���س���{currentLength * 1.0f / 1024 / 1024}M���ļ��ܳ��ȣ�{totalLength * 1.0f / 1024 / 1024}M");
    }

    /// <summary>
    /// ���ڴ�ӡ������Ϣ
    /// </summary>
    /// <param name="errorCode"></param>
    /// <param name="message"></param>
    void OnError(ErrorCode errorCode, string message)
    {
        Debug.LogError(message);
    }

    /// <summary>
    /// ����һ��AssetBundle�������б�����������б���������Ҫ���µ���Դ��
    /// </summary>
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

    /// <summary>
    /// ����һ�������б�����������б������������Դ��
    /// </summary>
    void CreatePackagesDownloadList()
    {
        string allPackagesPath = Path.Combine(DownloadVersionPath, "AllPackages");//�洢�˰������а������ļ�·��
        string allPackagesString = File.ReadAllText(allPackagesPath);//allPackagesPath ·�����ļ��ж�ȡ���а������ַ���
        List<string> allPackages = JsonConvert.DeserializeObject<List<string>>(allPackagesString);//�洢�����л���İ���

        Downloader downloader = null;

        foreach (string packageName in allPackages)
        {
            if (!CurrentDownloadInfo.DownloadFileName.Contains(packageName))
            {
                string remoteFilePath = Path.Combine(RemoteVersionPath, packageName);//Զ���ļ�·�� 
                string remotePackageSavePath = Path.Combine(DownloadVersionPath, packageName);//���ر���·��
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

    BuildInfo RemoteBuildInfo;//�洢��Զ�̷�������ȡ�Ĺ�����Ϣ

    /// <summary>
    /// ��ȡԶ�̷������ϵİ汾��Ϣ���Ƚϱ��ذ汾��Զ�̰汾�������Ƿ���Ҫ���и���
    /// </summary>
    /// <returns></returns>
    IEnumerator GetRemoteVersion()
    {
        #region ��ȡԶ�˰汾��
        string remoteVersionFilePath = Path.Combine(HTTPAddress, "BuildOutput", "BuildVersion.version");//�洢��Զ�̰汾�ļ���·��

        UnityWebRequest request = UnityWebRequest.Get(remoteVersionFilePath);//����һ��GET����Զ�̰汾�ļ���URL

        request.SendWebRequest();//������������

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

        //�������ɹ����������ص��ı�Ϊ���Ͱ汾��
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
        RemoteVersionPath = Path.Combine(HTTPAddress, "BuildOutput", AssetManagerRuntime.Instance.RemoteAssetVersion.ToString());//Զ��·��
        DownloadVersionPath = Path.Combine(AssetManagerRuntime.Instance.DownloadPath, AssetManagerRuntime.Instance.RemoteAssetVersion.ToString());//��������·��

        if (!Directory.Exists(DownloadVersionPath))
        {
            Directory.CreateDirectory(DownloadVersionPath);
        }
        Debug.Log(DownloadVersionPath);

        versionTxt.text = "Զ����Դ�汾��Ϊ" + version;
        Debug.Log($"Զ����Դ�汾{version}");

        #region ��ȡԶ��BuildInfo

        string remoteBuildInfoPath = Path.Combine(HTTPAddress, "BuildOutput", version.ToString(), "BuildInfo");//Զ��BuildInfo·��

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
        RemoteBuildInfo = JsonConvert.DeserializeObject<BuildInfo>(buildInfoString);//�����ص��ı������л�Ϊ BuildInfo ����

        if (RemoteBuildInfo == null || RemoteBuildInfo.FileTotalSize <= 0)
        {
            yield break;
        }
        #endregion
        CreateDownloadList();
    }

    /// <summary>
    /// �����ǳ�ʼ�������б�����������б�ʼ���ر�Ҫ���ļ�
    /// </summary>
    void CreateDownloadList()
    {
        //���ȶ�ȡ���������б�

        //����������Ϣ�ļ���·��������ļ��洢��֮ǰ���ص��ļ���Ϣ
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
            Downloader downloader = new Downloader(filePath, savePath, OnCompleted, OnProgress, OnError);//���� Downloader ���ʵ��������������ɡ����ؽ��Ⱥʹ�����Ļص�����
            downloader.StartDownload();
        }

    }

    /// <summary>
    /// �����ص���Դ����ʱ����·�����Ƶ�������Դ·����������������Ϣ�ļ�
    /// </summary>
    void CopyDownloadAssetsToLocalPath()
    {
        DirectoryInfo directoryInfo = new DirectoryInfo(DownloadVersionPath);//��ʾ������Դ���ڵ�Ŀ¼

        string localVersionPath = Path.Combine(AssetManagerRuntime.Instance.LocalAssetPath, AssetManagerRuntime.Instance.RemoteAssetVersion.ToString());//�洢���º����Դ

        directoryInfo.MoveTo(localVersionPath);

        string downoadInfoPath = Path.Combine(AssetManagerRuntime.Instance.DownloadPath, DownloadInfoFileName);//��¼�����ع����е��ļ�����Ϣ
        File.Delete(downoadInfoPath);//ɾ��������Ϣ�ļ�
    }

    /// <summary>
    /// ʶ�����Щ��Դ�����°汾�б���ӻ�ɾ����
    /// </summary>
    /// <param name="oldVersionAssets">�ɰ汾����Դ����������</param>
    /// <param name="newVersionAssets">�°汾����Դ����������</param>
    /// <returns></returns>
    AssetBundleVersionDifference ContrastAssetBundleVersion(string[] oldVersionAssets, string[] newVersionAssets)
    {
        AssetBundleVersionDifference difference = new AssetBundleVersionDifference();//����ʵ��difference�����ڴ洢�ȽϽ��

        //�����ɰ汾����Դ���б�
        foreach (var assetName in oldVersionAssets)
        {
            if (newVersionAssets.Contains(assetName))
            {
                difference.ReducedAssetBundles.Add(assetName);
            }
        }
        //ѭ�������°汾����Դ���б�
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




