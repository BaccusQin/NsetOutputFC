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

        }
    }
    class SplitDt
    {
        public DataSet Change(DataTable dt)
        {
            DataSet ds = new DataSet();
            for(int i=0;i<dt.Rows.Count;)
            {
                DataTable tempDt = new DataTable();
                tempDt.TableName = dt.Rows[i]["VENDOR_CD"].ToString();
                tempDt.Columns.Add("Stock_Id");
                tempDt.Columns.Add("Stock_Name");

                do
                {
                    int j = 0;
                    tempDt.Columns.Add(dt.Rows[i]["DATE_NO"].ToString());
                    tempDt.Rows.Add dt.Rows[i]["VENDOR_CD"].ToString();
                    tempDt.Rows[j]["Stock_Name"] = dt.Rows[i]["VENDOR_CD"].ToString();
                    do
                    {

                    }
                    while (dt.Rows[i]["ITEM_NO"].ToString() == dt.Rows[i - 1]["ITEM_NO"].ToString());

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
