using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;


/// <summary>
/// 已经下载的文件名
/// </summary>
public class DownloadInfo
{
    public List<string> DownloadFileName = new List<string>();
}

/// <summary>
/// 由我们自己定义的下载处理类
/// </summary>
public class Downloader : MonoBehaviour
{
    /// <summary>
    /// 文件服务器地址
    /// </summary>
    string URL = null;

    /// <summary>
    /// 文件保存路径
    /// </summary>
    string SavePath = null;

    /// <summary>
    /// 具体的下载实例
    /// </summary>
    UnityWebRequest request = null;

    /// <summary>
    /// 由我们自己实现的下载处理类
    /// </summary>
    DownloadHandler downloadHandler = null;

    ErrorEventHandler OnError = null;

    CompleteEventHandler OnCompleted = null;

    ProgressEventHandler OnProgress = null;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="url">文件网络地址</param>
    /// <param name="savePath">文件本地保存地址</param>
    /// <param name="onCompleted">完成回调</param>
    /// <param name="onProgress">下载时回调</param>
    /// <param name="onError">报错时回调</param>
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
    /// 启动文件下载任务
    /// </summary>
    public void StartDownload()
    {
        request = UnityWebRequest.Get(URL);

        if(!string.IsNullOrEmpty(SavePath))
        {
            //超时时间应包含下载时间
            //以便给服务器以及本地留出足够的冗余时间
            //timeout不触发error
            request.timeout = 60;

            request.disposeDownloadHandlerOnDispose = true;

            downloadHandler = new DownloadHandler(SavePath, OnCompleted, OnProgress, OnError);

            //因为currentLength会在实例化以及写入临时文件时更新，所以始终可以表达临时文件的长度
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
        Debug.Log("下载器释放");
        if(request!=null)
        {
            //如果下载没有完成，就中止
            if(!request.isDone)
            {
                //放弃本次请求
                request.Abort();
            }

            request.Dispose();
            request = null; 
        }
    }
}
