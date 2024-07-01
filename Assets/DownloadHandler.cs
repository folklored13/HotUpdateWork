using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Unity�в������ı�������
/// </summary>
public enum ErrorCode
{
    DownloadFileEmpty,//��Ҫ���ص��ļ�����Ϊ��
    TempFileMissing,//��ʱ�ļ���ʧ
}

/// <summary>
/// �޲Σ��޷���ֵ��ί��
/// ί�е�ʵ�ʾ�������һ���ض��ķ���ֵ���ض������ĺ�������������ָ��ĳһ��
/// �κη��Ϲ���ĺ������ǿ�����ĳ��ί��
/// �κη��Ϲ���ĺ���������ί�и�ĳ��ί��ʾ����ί�б�����������
/// ������ʵ���˽�ĳ���ض�����ĺ��������һ�����ݵĹ���
/// ��ν�����Ĺ�����ʼ����һ��������ʲô���͵ķ���ֵ���;����ļ��������涨
/// </summary>
public delegate void SampleDelegate(string content);

/// <summary>
/// ���ش���ʱִ�лص�
/// </summary>
/// <param name="errorCode">������</param>
/// <param name="message">������Ϣ</param>
public delegate void ErrorEventHandler(ErrorCode errorCode, string message);
/// <summary>
/// �������ʱִ�лص�
/// </summary>
/// <param name="message">��ɵ���Ϣ</param>
public delegate void CompleteEventHandler(string fileName,string message);

/// <summary>
/// ���ؽ��ȸ���ʱִ�лص�
/// </summary>
/// <param name="prg">��ǰ����</param>
/// <param name="currLength">��ǰ������ɵĳ���</param>
/// <param name="totalLength">�ļ��ܳ���</param>
public delegate void ProgressEventHandler(float prg, long currLength, long totalLength);
public class DownloadHandler : DownloadHandlerScript
{
    /// <summary>
    /// ������ɺ󱣴��·��
    /// </summary>
    string SavePath;

    /// <summary>
    /// ��ʱ�ļ�����·��
    /// </summary>
    string TempPath;

    /// <summary>
    /// ������ʱ�ļ��Ĵ�С���ֽڳ��ȣ�
    /// </summary>
    long currentLength = 0;

    /// <summary>
    /// �ļ����ܴ�С(�ֽڳ���)
    /// </summary>
    long totalLength = 0;

    /// <summary>
    /// ������Ҫ���ص��ֽڳ���
    /// </summary>
    long contentLength = 0;

    /// <summary>
    /// �ļ��������������յ�������д���ļ�
    /// </summary>
    FileStream fileStream = null;

    /// <summary>
    /// ����ʱ��ص�
    /// ί������
    /// </summary>
    ErrorEventHandler OnError = null;

    /// <summary>
    /// �������ʱִ�еĻص�����
    /// </summary>
    CompleteEventHandler OnCompleted = null;

    /// <summary>
    /// ���ؽ��ȸ���ʱִ�еĻص�����
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
    /// ���ڴ����ļ����ص��Զ�����
    /// </summary>
    /// <param name="savePath">�����ļ��ı���·��</param>
    /// <param name="onCompeted"></param>
    /// <param name="onProgree"></param>
    /// <param name="onError"></param>
    public DownloadHandler(string savePath,CompleteEventHandler onCompeted,ProgressEventHandler onProgree,ErrorEventHandler onError)
    {
        //�ڹ��캯���У�this�ؼ��ִ�����ι��죬�����������Ķ������͵�ʵ��
        this.SavePath = savePath;
        //ԭ�����ļ�·���£����ⴴ��һ��.temp�ļ�
        this.TempPath = savePath + ".temp";

        this.OnCompleted = onCompeted;

        this.OnProgress = onProgree;

        this.OnError = onError;

        this.fileStream = new FileStream(this.TempPath, FileMode.OpenOrCreate, FileAccess.ReadWrite);

        //�����Create��������0
        this.currentLength = this.fileStream.Length;

        //��������֮�⣬д���ļ�ҲҪ���Ѿ�д�����󳤶ȼ�������д
        this.fileStream.Position = this.currentLength;
    }

    /// <summary>
    /// ������Headerʱ���ø÷���
    /// </summary>
    /// <param name="contentLength"></param>
    protected override void ReceiveContentLengthHeader(ulong contentLength)
    {
        this.contentLength = (long)contentLength;
        //һ���ļ����ܳ���=�Ѿ����س���+δ���س���
        this.totalLength = this.contentLength + currentLength;
    }

    /// <summary>
    /// ��ÿ�δӷ��������յ���Ϣʱ�����
    /// </summary>
    protected override bool  ReceiveData(byte[] datas,int dataLength)
    {
        if(contentLength<=0||datas==null||datas.Length<=0)
        {
            return false;
        }

        //����0��Length����ָ����datas��λ��
        this.fileStream.Write(datas, 0, dataLength);

        currentLength += dataLength;

        //����1.0f��Ϊ����ʽת����float����
        OnProgress?.Invoke(currentLength * 1.0f / totalLength, currentLength, totalLength);

        return true;
    }


    /// <summary>
    /// �������ʱ����õĺ���
    /// </summary>
    protected override void CompleteContent()
    {
        //�������������ݺ����ȹر��ļ���
        FileStreamClose();
        
        //����������ϲ����ڸ��ļ����������ص����ݳ��Ȼ�Ϊ0
        //������Ҫ���⴦���������
        if(contentLength<=0)
        {
            OnError.Invoke(ErrorCode.DownloadFileEmpty, "�������ݳ���Ϊ0");
            return;
        }

        //���������ɺ���ʱ�ļ����������ɾ���ˣ�Ҳ�׳�������ʾ
        if(!File.Exists(TempPath))
        {
            OnError.Invoke(ErrorCode.TempFileMissing, "������ʱ�����ļ���ʧ");
            return;
        }

        //������ص��ļ��Ѿ����ڣ���ɾ��ԭ�ļ�
        if(File.Exists(SavePath))
        {
            File.Delete(SavePath);
        }

        //ͨ�������ϵ�У��󣬾ͽ���ʱ�ļ��ƶ���Ŀ��·�������سɹ�
        //move����ͬʱҲ�������������Ĺ���
        //��Ϊpath��Ҫ��ָ�������ļ�����
        File.Move(TempPath, SavePath);

        FileInfo fileInfo = new FileInfo(SavePath);
        OnCompleted.Invoke(fileInfo.Name,"�����ļ����");
        
    }
    public override void Dispose()
    {
        Debug.Log(base.error);
        base.Dispose();
        FileStreamClose();
    }

    /// <summary>
    /// �ر��ļ���
    /// </summary>
    public void FileStreamClose()
    {
        if (fileStream == null)
            return;
        fileStream.Close();
        fileStream.Dispose();
        fileStream = null;
        Debug.Log("�ļ����ر�");
    }
}
