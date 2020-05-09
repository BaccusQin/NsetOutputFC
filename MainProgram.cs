﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NsetOutputFC
{
    class MainProgram
    {
        static void Main(string[] args)
        {
           
            int weeklyRange = Convert.ToInt32(ConfigurationManager.AppSettings["weeklyRange"]);
            int monthlyRange = Convert.ToInt32(ConfigurationManager.AppSettings["monthlyRange"]);
            int weeklyType = 0;
            int monthlyType = 1;
        

            CsvCreater weeklyCsv = new CsvCreater("Weekly_",weeklyType,weeklyRange);
            CsvCreater monthlyCsv = new CsvCreater("Monthly_",monthlyType,monthlyRange);
            weeklyCsv.OutPutCsv();
            monthlyCsv.OutPutCsv();
        
        }
      
       
       
    }
    /// <summary>
    /// DataSetを分割
    /// </summary>
    public class SplitDt
    {
        private int _range;
        private int _dataType;
        public SplitDt(int Range,int DataType)
        {
            _range = Range;
            _dataType = DataType;
        }
        public DataSet Change(DataTable dt)
        {
            try
            {
                DataSet ds = new DataSet();
                for (int i = 0; i < dt.Rows.Count;)
                {

                    DataTable tempDt = new DataTable();
                    tempDt.TableName = dt.Rows[i]["VENDOR_CD"].ToString() + "_" + dt.Rows[i]["VENDOR_DESC"].ToString();
                    tempDt.Columns.Add("Stock_Id");
                    tempDt.Columns.Add("Stock_Name");
                    for (int k = 0; k < _range; k++)
                    {
                        if (_dataType == 0)
                        {
                            DateTime dateTime = DateTime.Now.AddDays(k * 7);
                            DateTime startWeek = dateTime.AddDays(1 - Convert.ToInt32(dateTime.DayOfWeek.ToString("d")));
                            DateTime endWeek = startWeek.AddDays(6);
                            string colName = startWeek.ToString("MMdd") + "-" + endWeek.ToString("MMdd");
                            tempDt.Columns.Add(colName);
                        }
                        else
                        {
                            string colName = DateTime.Now.AddMonths(k).ToString("yyyyMM");
                            tempDt.Columns.Add(colName);
                        }

                    }
                    int j = 0;
                    do
                    {

                        tempDt.Rows.Add();
                        do
                        {
                            string DateNO = dt.Rows[i]["DATE_NO"].ToString();

                            tempDt.Rows[j]["Stock_Id"] = dt.Rows[i]["ITEM_NO"].ToString();
                            tempDt.Rows[j]["Stock_Name"] = dt.Rows[i]["ITEM_DESC"].ToString();
                            tempDt.Rows[j][DateNO] = dt.Rows[i]["PO_QTY"].ToString();
                            i++;


                        }
                        while (i < dt.Rows.Count && dt.Rows[i]["VENDOR_CD"].ToString() == dt.Rows[i - 1]["VENDOR_CD"].ToString() && dt.Rows[i]["ITEM_NO"].ToString() == dt.Rows[i - 1]["ITEM_NO"].ToString());
                        j++;


                    }
                    while (i < dt.Rows.Count && dt.Rows[i]["VENDOR_CD"].ToString() == dt.Rows[i - 1]["VENDOR_CD"].ToString());
                    ds.Tables.Add(tempDt);
                }
                return ds;

            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
          
        }
        
    }
    /// <summary>
    /// OutPut Csv File
    /// </summary>
    public class CsvCreater
    {
        

        int dataType;
        int range;
        string type;
        string allLog;
        string logFolderPath = ConfigurationManager.AppSettings["logFolderPath"];
        string targetFolderPath = ConfigurationManager.AppSettings["targetFolderPath"];

        public CsvCreater(string Type,int DataType,int Range)
        {
          
            type = Type;
            dataType = DataType;
            range = Range;
        }

        public void OutPutCsv()
        {
            try
            {
                DataTable dtBefore = new ConnDB(dataType).Get_Pur_FcstInqList(range).Tables[0];
                SplitDt Split = new SplitDt(range, dataType);
                DataSet ds = Split.Change(dtBefore);

                foreach (DataTable dt in ds.Tables)
                {
                    string strOutPutFolderPath = targetFolderPath + @"\" + dt.TableName.Split('_')[0];
                    string strOutPutCSVPath = strOutPutFolderPath + @"\" + type + dt.TableName.Split('_')[0] + "_" + DateTime.Now.ToString("yyyyMMdd") + @".csv";
                    if (!Directory.Exists(strOutPutFolderPath))
                    {
                        Directory.CreateDirectory(strOutPutFolderPath);
                        allLog += type + dt.TableName.Split('_')[0] + "  Folder was created.\n";
                    }
                    SaveCsv(dt, strOutPutCSVPath);
                    allLog += type + dt.TableName.Split('_')[0] + "_" + DateTime.Now.ToString("yyyyMMdd") + "  Csv was created.\n";

                }

            }
            catch(Exception ex)
            {
                allLog += ex.Message;
               
            }
            finally
            {
                File.AppendAllText(logFolderPath + @"\" + DateTime.Now.ToString("yyyy-MM-dd H-mm-ss") + "log.txt", "\r\n" + allLog);
            }
           
            
        }
        private void SaveCsv(DataTable dt, string filePath)
        {
            FileStream fs = null;
            StreamWriter sw = null;
            try
            {
                fs = new FileStream(filePath, FileMode.Create, FileAccess.Write);
                sw = new StreamWriter(fs, Encoding.Default);
                var data = string.Empty;       
                sw.WriteLine("," + "\"NIDEC SHIBAURA ELECTRONICS(THAILAND) CO.,LTD.\"");
                sw.WriteLine("," + "\"144 / 4 Moo.5 Tivanon Road,Bangkadi Industrial Park,\"");
                sw.WriteLine("," + "\"Tumbol Bangkadi,Amphur Muang,Patumthani 12000\"");
                sw.WriteLine("," + "\"Tel: (662) 831 - 9000 Fax: (662) 963 - 8235 , 963 - 8302\"");
                sw.WriteLine(" ");
                sw.WriteLine(","+"ORDER FORECAST");
                sw.WriteLine(" ");
                sw.WriteLine("To:,"+ dt.TableName.Split('_')[1]);
                sw.WriteLine("Date"+","+DateTime.Now.ToString("yy-MM-dd"));
                sw.WriteLine(" ");
                //Column Name
                for (var i = 0; i < dt.Columns.Count; i++)
                {
                    data += dt.Columns[i].ColumnName;
                    if (i < dt.Columns.Count - 1)
                    {
                        data += ",";
                    }
                }
                sw.WriteLine(data);
                //Row Data
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    data = string.Empty;
                    for (var j = 0; j < dt.Columns.Count; j++)
                    {
                        data += dt.Rows[i][j].ToString();
                        if (j < dt.Columns.Count - 1)
                        {
                            data += ",";
                        }
                    }
                    sw.WriteLine(data);
                }
            }
            catch (IOException ex)
            {
                throw new IOException(ex.Message, ex);
            }
            finally
            {
                if (sw != null) sw.Close();
                if (fs != null) fs.Close();
            }
        }

    }




}
