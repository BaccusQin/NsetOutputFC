using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NsetOutputFC
{
    class MainProgram
    {
        static void Main(string[] args)
        {
            int weeklyRange = 2;
            int monthlyRange = 2;
            DataTable dtMyFcWeekly = new ConnDB(0).Get_Pur_FcstInqList(weeklyRange).Tables[0];
            DataTable dtMyFcMonthly = new ConnDB(1).Get_Pur_FcstInqList(monthlyRange).Tables[0];
            SplitDt mysplit = new SplitDt();
            DataSet test = mysplit.Change(dtMyFcWeekly);

        }
    }
    class SplitDt
    {
        public DataSet Change(DataTable dt)
        {
            DataSet ds = new DataSet();
            for(int i=0;i<dt.Rows.Count;)
            {
                int j = 0;
                DataTable tempDt = new DataTable();
                tempDt.TableName = dt.Rows[i]["VENDOR_CD"].ToString();
                tempDt.Columns.Add("Stock_Id");
                tempDt.Columns.Add("Stock_Name");
                string DateNO= dt.Rows[i]["DATE_NO"].ToString();
                do
                {
                    
                    tempDt.Columns.Add(DateNO);
                    tempDt.Rows.Add()
                    tempDt.Rows[j]["Stock_Id"] = dt.Rows[i]["ITEM_NO"].ToString();
                    tempDt.Rows[j]["Stock_Name"] = dt.Rows[i]["ITEM_DESC"].ToString();
                    tempDt.Rows[j][DateNO] = dt.Rows[i]["PO_QTY"].ToString();
            
                    j++;
                    i++;
                }
                while (dt.Rows[i]["VENDOR_CD"].ToString()== dt.Rows[i-1]["VENDOR_CD"].ToString());
                ds.Tables.Add(tempDt);
            }
            return ds;
        }
        
    }
    


}
