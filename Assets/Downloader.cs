using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;


/// <summary>
/// �Ѿ����ص��ļ���
/// </summary>
public class DownloadInfo
{
    public List<string> DownloadFileName = new List<string>();
}

/// <summary>
/// �������Լ���������ش�����
/// </summary>
public class Downloader : MonoBehaviour
{
    /// <summary>
    /// �ļ���������ַ
    /// </summary>
    string URL = null;

    /// <summary>
    /// �ļ�����·��
    /// </summary>
    string SavePath = null;

    /// <summary>
    /// ���������ʵ��
    /// </summary>
    UnityWebRequest request = null;

    /// <summary>
    /// �������Լ�ʵ�ֵ����ش�����
    /// </summary>
    DownloadHandler downloadHandler = null;

    ErrorEventHandler OnError = null;

    CompleteEventHandler OnCompleted = null;

    ProgressEventHandler OnProgress = null;

    /// <summary>
    /// ���캯��
    /// </summary>
    /// <param name="url">�ļ������ַ</param>
    /// <param name="savePath">�ļ����ر����ַ</param>
    /// <param name="onCompleted">��ɻص�</param>
    /// <param name="onProgress">����ʱ�ص�</param>
    /// <param name="onError">����ʱ�ص�</param>
    public Downloader(string url,string savePath,CompleteEventHandler onCompleted,ProgressEventHandler onProgress,
        ErrorEventHandler onError)
    {
        this.URL = url;
        this.SavePath = savePath;
        this.OnCompleted = onCompleted;
        this.OnProgress = onProgress;
        this.OnError = onError;
    }

    /// <summary>
    /// �����ļ���������
    /// </summary>
    public void StartDownload()
    {
        request = UnityWebRequest.Get(URL);

        if(!string.IsNullOrEmpty(SavePath))
        {
            //��ʱʱ��Ӧ��������ʱ��
            //�Ա���������Լ����������㹻������ʱ��
            //timeout������error
            request.timeout = 60;

            request.disposeDownloadHandlerOnDispose = true;

            downloadHandler = new DownloadHandler(SavePath, OnCompleted, OnProgress, OnError);

            //��ΪcurrentLength����ʵ�����Լ�д����ʱ�ļ�ʱ���£�����ʼ�տ��Ա����ʱ�ļ��ĳ���
            request.SetRequestHeader("range", $"bytes={downloadHandler.CurrentLength}-");

            request.downloadHandler = downloadHandler;
        }

        request.SendWebRequest();
    }


    public void Dispose()
    {
        OnError = null;
        OnCompleted = null;
        OnProgress = null;
        Debug.Log("�������ͷ�");
        if(request!=null)
        {
            //�������û����ɣ�����ֹ
            if(!request.isDone)
            {
                //������������
                request.Abort();
            }

            request.Dispose();
            request = null; 
        }
    }
}
