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
    public class ConnDB
    {
		private int _date_type; //1--monthly　0--weekly
		private static string strNowDate=DateTime.Now.ToString("yyyyMMdd");
		private static string strCalenderDate = DateTime.Now.AddDays(1 - DateTime.Now.Day).ToString("yyyyMMdd");
		private string strTtlWeek;
		public ConnDB(int Date_type)
		{
			this.OpenDB();
			_date_type = Date_type;
			strTtlWeek = GetWeek();
		}
        private static string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();
        protected SqlConnection sqlConn = new SqlConnection(connectionString);
        public void OpenDB()
        {
            try
            {
                sqlConn.Open();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }
        public void CloseDB()
        {
            try
            {
                sqlConn.Close();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

		public string GetWeek()
		{
			try
			{
			
				string sql = "select TTL_WEEK from CALENDAR_MASTER where CALENDAR_DATE="+ strNowDate;
				SqlCommand cmd = new SqlCommand(sql, sqlConn);
				object objWeek = cmd.ExecuteScalar();
				return objWeek.ToString();
				

			}
			catch(Exception ex)
			{
				throw new Exception(ex.Message);
			}
			
		}

		public DataSet Get_Pur_FcstInqList(int range)
		{
			
			StringBuilder strSQL = new StringBuilder();
			DataSet ds = new DataSet();
			strSQL.Append("SELECT                                                                           ");
			strSQL.Append(" PO.VENDOR_CD, VENDOR_DESC, PO.ITEM_NO,         PO.ITEM_DESC,                                               ");
			strSQL.Append(" CASE PM.DELV_LT_TYPE WHEN 0 THEN convert(varchar,PM.DELV_LT) WHEN 1             ");
			strSQL.Append(" THEN convert(varchar,PM.DELV_LT_PROPORTION)   + '(LOT:' +                       ");
			strSQL.Append(" convert(varchar,PM.STD_LOT_SIZE) + ')' END DELV_LT ,SUM(PO.PO_QTY) PO_QTY,      ");
			if (this._date_type == 0)
			{
				//strSQL.Append(" CAL.TTL_WEEK DATE_NO                                                        ");
				strSQL.Append(" (SELECT　right(CONVERT(varchar, MIN(CALENDAR_DATE)),4)+'-'　+RIGHT(CONVERT(varchar, MAX(CALENDAR_DATE)),4)   ");

				strSQL.Append(" FROM[PM].[dbo].[CALENDAR_MASTER] WHERE TTL_WEEK = CAL.TTL_WEEK)   as DATE_NO                                 ");
			}
			else
			{
				strSQL.Append(" CAL.CALENDAR_CYM  DATE_NO                                                   ");
			}
			
			
			
			strSQL.Append(" FROM PLANNED_ORDER PO                                                    ");
			strSQL.Append(" LEFT JOIN CALENDAR_MASTER CAL ON CAL.CALENDAR_DATE  = PO.PO_DUE_DATE     ");
			if (this._date_type == 1)
			{
				strSQL.Append(" AND CAL.CALENDAR_DATE  >= (SELECT MIN(CALENDAR_DATE) from CALENDAR_MASTER WHERE CALENDAR_DATE >=" +strCalenderDate+")      ");
				strSQL.Append(" AND CAL.CALENDAR_DATE  <= (SELECT MAX(CALENDAR_DATE) from CALENDAR_MASTER WHERE CALENDAR_DATE <=                      ");
				strSQL.Append(" CONVERT(VARCHAR(8), DATEADD(DAY,-1,(DATEADD(MONTH,"+range.ToString()+",CONVERT(DATETIME,CONVERT(VARCHAR(8),"+strCalenderDate+"))))), 112))  ");
			}
			else
			{
				strSQL.Append(" AND CAL.CALENDAR_DATE  >= (SELECT MIN(CALENDAR_DATE) from CALENDAR_MASTER WHERE TTL_WEEK = "+strTtlWeek+")      ");
				strSQL.Append(" AND CAL.CALENDAR_DATE  <= (SELECT MAX(CALENDAR_DATE) from CALENDAR_MASTER WHERE TTL_WEEK =" +(Convert.ToInt32(strTtlWeek) + range - 1)+")  ");
			}
			strSQL.Append(" LEFT OUTER JOIN PURCHASE_MASTER PM ON PM.ITEM_NO  = PO.ITEM_NO                      ");
			strSQL.Append(" AND PO.VENDOR_CD = PM.VENDOR_CD                                                     ");
			strSQL.Append(" AND PO.PO_DUE_DATE >= PM.BEG_EFF_DATE                                               ");
			strSQL.Append("  AND PO.PO_DUE_DATE <= PM.END_EFF_DATE                                              ");
			strSQL.Append("LEFT JOIN VENDOR_MASTER ON VENDOR_MASTER.VENDOR_CD = po.VENDOR_CD                    ");
			strSQL.Append(" AND PO.ITEM_TYPE <> '99'                                                            ");
			strSQL.Append(" WHERE PO.SC_FLAG = 1                                                                ");



			if (this._date_type == 1)
			{
				strSQL.Append("AND PO.PO_DUE_DATE >=" +strCalenderDate+ " AND PO.PO_DUE_DATE <=            ");
				strSQL.Append("CONVERT(varchar(8), DATEADD(Month,"+range.ToString()+",convert(datetime,convert(varchar(8),+"+strCalenderDate+"))), 112)  ");
			
			}
			else
			{
				strSQL.Append(" AND PO.PO_DUE_DATE  >= (SELECT MIN(CALENDAR_DATE) from CALENDAR_MASTER WHERE TTL_WEEK = " + strTtlWeek + ") ");
				strSQL.Append(" AND PO.PO_DUE_DATE  <= (SELECT MAX(CALENDAR_DATE) from CALENDAR_MASTER WHERE TTL_WEEK = " + (Convert.ToInt32(strTtlWeek) + range - 1) + ")  ");
			
			}
			
			strSQL.Append(" GROUP BY PO.VENDOR_CD,VENDOR_DESC, PO.ITEM_NO,    PO.ITEM_DESC,                             ");
			strSQL.Append(" CASE PM.DELV_LT_TYPE WHEN 0 THEN convert(varchar,PM.DELV_LT) WHEN 1             ");
			strSQL.Append(" THEN convert(varchar,PM.DELV_LT_PROPORTION)   + '(LOT:' +                       ");
			strSQL.Append(" convert(varchar,PM.STD_LOT_SIZE) + ')' END,                                     ");
			if (this._date_type == 0)
			{
				strSQL.Append(" CAL.TTL_WEEK                                                ");
			}
			else
			{
				strSQL.Append(" CAL.CALENDAR_CYM                                            ");
			}
			strSQL.Append(" ORDER BY PO.VENDOR_CD,PO.ITEM_NO ");
			DataSet result=new DataSet();
			try
			{
				SqlDataAdapter myad = new SqlDataAdapter(strSQL.ToString(), sqlConn);
				myad.Fill(result, "mydata");
				
			}
			catch(Exception ex)
			{
				throw new Exception(ex.Message);
			}
			finally
			{
				this.CloseDB();
			}
			return result;
		}

	}
    
}
