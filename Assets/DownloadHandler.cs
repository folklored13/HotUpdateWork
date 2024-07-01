using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Unity中不包括的报错类型
/// </summary>
public enum ErrorCode
{
    DownloadFileEmpty,//需要下载的文件内容为空
    TempFileMissing,//临时文件丢失
}

/// <summary>
/// 无参，无返回值的委托
/// 委托的实质就是声明一种特定的返回值，特定参数的函数，但不具体指定某一个
/// 任何符合规则的函数都是可以是某个委托
/// 任何符合规则的函数都可以委托给某个委托示例（委托变量）来调用
/// 这样就实现了将某种特定规则的函数像变量一样传递的功能
/// 所谓函数的规则起始就是一个函数由什么类型的返回值，和具体哪几个参数规定
/// </summary>
public delegate void SampleDelegate(string content);

/// <summary>
/// 下载错误时执行回调
/// </summary>
/// <param name="errorCode">错误码</param>
/// <param name="message">错误信息</param>
public delegate void ErrorEventHandler(ErrorCode errorCode, string message);
/// <summary>
/// 下载完成时执行回调
/// </summary>
/// <param name="message">完成的消息</param>
public delegate void CompleteEventHandler(string fileName,string message);

/// <summary>
/// 下载进度更新时执行回调
/// </summary>
/// <param name="prg">当前进度</param>
/// <param name="currLength">当前下载完成的长度</param>
/// <param name="totalLength">文件总长度</param>
public delegate void ProgressEventHandler(float prg, long currLength, long totalLength);
public class DownloadHandler : DownloadHandlerScript
{
    /// <summary>
    /// 下载完成后保存的路径
    /// </summary>
    string SavePath;

    /// <summary>
    /// 临时文件储存路径
    /// </summary>
    string TempPath;

    /// <summary>
    /// 代表临时文件的大小（字节长度）
    /// </summary>
    long currentLength = 0;

    /// <summary>
    /// 文件的总大小(字节长度)
    /// </summary>
    long totalLength = 0;

    /// <summary>
    /// 本次需要下载的字节长度
    /// </summary>
    long contentLength = 0;

    /// <summary>
    /// 文件流，用来将接收到的数据写入文件
    /// </summary>
    FileStream fileStream = null;

    /// <summary>
    /// 出错时候回调
    /// 委托类型
    /// </summary>
    ErrorEventHandler OnError = null;

    /// <summary>
    /// 下载完成时执行的回调函数
    /// </summary>
    CompleteEventHandler OnCompleted = null;

    /// <summary>
    /// 下载进度更新时执行的回调函数
    /// </summary>
    ProgressEventHandler OnProgress = null;

    public long CurrentLength 
    { 
        get { return currentLength; }
    }

    public long TotalLength
    {
        get { return totalLength; }
    }

    /// <summary>
    /// 用于处理文件下载的自定义类
    /// </summary>
    /// <param name="savePath">下载文件的保存路径</param>
    /// <param name="onCompeted"></param>
    /// <param name="onProgree"></param>
    /// <param name="onError"></param>
    public DownloadHandler(string savePath,CompleteEventHandler onCompeted,ProgressEventHandler onProgree,ErrorEventHandler onError)
    {
        //在构造函数中，this关键字代表这次构造，过程中声明的对用类型的实例
        this.SavePath = savePath;
        //原本的文件路径下，额外创建一个.temp文件
        this.TempPath = savePath + ".temp";

        this.OnCompleted = onCompeted;

        this.OnProgress = onProgree;

        this.OnError = onError;

        this.fileStream = new FileStream(this.TempPath, FileMode.OpenOrCreate, FileAccess.ReadWrite);

        //如果是Create，长度是0
        this.currentLength = this.fileStream.Length;

        //除了下载之外，写入文件也要从已经写入的最大长度继续往下写
        this.fileStream.Position = this.currentLength;
    }

    /// <summary>
    /// 当设置Header时调用该方法
    /// </summary>
    /// <param name="contentLength"></param>
    protected override void ReceiveContentLengthHeader(ulong contentLength)
    {
        this.contentLength = (long)contentLength;
        //一个文件的总长度=已经下载长度+未下载长度
        this.totalLength = this.contentLength + currentLength;
    }

    /// <summary>
    /// 再每次从服务器上收到消息时会调用
    /// </summary>
    protected override bool  ReceiveData(byte[] datas,int dataLength)
    {
        if(contentLength<=0||datas==null||datas.Length<=0)
        {
            return false;
        }

        //这里0和Length都是指的是datas的位置
        this.fileStream.Write(datas, 0, dataLength);

        currentLength += dataLength;

        //乘以1.0f是为了隐式转换成float类型
        OnProgress?.Invoke(currentLength * 1.0f / totalLength, currentLength, totalLength);

        return true;
    }


    /// <summary>
    /// 下载完成时会调用的函数
    /// </summary>
    protected override void CompleteContent()
    {
        //接受完所有数据后，首先关闭文件流
        FileStreamClose();
        
        //如果服务器上不存在该文件，请求下载的内容长度会为0
        //所以需要特殊处理这种情况
        if(contentLength<=0)
        {
            OnError.Invoke(ErrorCode.DownloadFileEmpty, "下载内容长度为0");
            return;
        }

        //如果下载完成后，临时文件如果被意外删除了，也抛出错误提示
        if(!File.Exists(TempPath))
        {
            OnError.Invoke(ErrorCode.TempFileMissing, "下载临时缓存文件丢失");
            return;
        }

        //如果下载的文件已经存在，就删掉原文件
        if(File.Exists(SavePath))
        {
            File.Delete(SavePath);
        }

        //通过了以上的校验后，就将临时文件移动到目的路径，下载成功
        //move方法同时也带有重新命名的功能
        //因为path中要求指定具体文件名称
        File.Move(TempPath, SavePath);

        FileInfo fileInfo = new FileInfo(SavePath);
        OnCompleted.Invoke(fileInfo.Name,"下载文件完成");
        
    }
    public override void Dispose()
    {
        Debug.Log(base.error);
        base.Dispose();
        FileStreamClose();
    }

    /// <summary>
    /// 关闭文件流
    /// </summary>
    public void FileStreamClose()
    {
        if (fileStream == null)
            return;
        fileStream.Close();
        fileStream.Dispose();
        fileStream = null;
        Debug.Log("文件流关闭");
    }
}
