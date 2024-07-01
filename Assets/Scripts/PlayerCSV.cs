using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using UnityEngine;

public class PlayerCSV : MonoBehaviour
{
    
    void Start()
    {
        SaveCSV();
    }

   
    void Update()
    {
        
    }

    void SaveCSV()
    {
        DataTable dataTable = new DataTable();

        // ����������
        dataTable.Columns.Add("�ȼ�");
        dataTable.Columns.Add("����");
        dataTable.Columns.Add("�ƶ��ٶ�");

        for(int i = 0; i < 10; i++)
        {
            DataRow dataRow = dataTable.NewRow();
            dataRow[0] = i; // �ȼ�
            dataRow[1] = i+1; // ����
            dataRow[2] = Player.moveSpeed * i + 1 * 1.3;   // �ٶ�

            dataTable.Rows.Add(dataRow);
        }

        StringBuilder csvString = new StringBuilder();
        string separateSign = ",";
        string lineFeedSign = "\r\n";
        for(int i = 0; i < dataTable.Columns.Count; i++)
        {
            csvString.Append(dataTable.Columns[i].ColumnName);
            if (i < dataTable.Columns.Count - 1)
            {
                csvString.Append(separateSign);
            }
        }

        for(int i = 0; i < dataTable.Rows.Count; i++)
        {
            csvString.Append(lineFeedSign);
            for(int j = 0; j < dataTable.Columns.Count; j++)
            {
                csvString.Append(dataTable.Rows[i][j].ToString());
                if (j < dataTable.Columns.Count - 1)
                {
                    csvString.Append(separateSign);
                }
            }
        }

        string path = Application.dataPath + "/Resource/Resources/PlayerPropertyData.csv";
        File.WriteAllText(path, csvString.ToString(), Encoding.UTF8);
    }
}
