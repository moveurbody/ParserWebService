using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.IO;

/// <summary>
/// ===2015-01-23=== Modify by Yuhsuan
///  
/// 1. For PerformDatabaseAction3 ,after insert new value to DB, it will return last UID.
///    Insert success ReturnCode=1 , duplicate ReturnCode=0 , other failed ReturnCode=-1
///    If there is no SCOPE_IDENTITY in table, the ReturnCode is 1 and UID is null.
///    
/// 2. Add event log to show return result. It can be setting by webconfig.
/// 
/// </summary>

[WebService(Namespace = "http://tempuri.org/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]

//For overloading webservice setting
//[WebServiceBinding(ConformsTo = WsiProfiles.None)]

// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
// [System.Web.Script.Services.ScriptService]
public class Service : System.Web.Services.WebService {

    public Service () {

        //Uncomment the following line if using designed components 
        //InitializeComponent(); 
    }

    //It's asp sample
    [WebMethod]
    public string HelloWorld() {
        return "Hello World (" + Environment.MachineName +")";
    }

    
    [WebMethod]
    public string BatchInsert(string[] targetColumns, List<string[]> targetValue, string targetTable, Boolean IsUsePrimaryDB)
    {
        EventLog.Write("[BatchInsert] +++");
        //System.Diagnostics.Stopwatch TotalSW = new System.Diagnostics.Stopwatch();
        //System.Diagnostics.Stopwatch InsertSW = new System.Diagnostics.Stopwatch();
        //System.Diagnostics.Stopwatch QueryTotalSW = new System.Diagnostics.Stopwatch();
        //System.Diagnostics.Stopwatch QueryEachSW = new System.Diagnostics.Stopwatch();
        //System.Diagnostics.Stopwatch QueryResponSW = new System.Diagnostics.Stopwatch();
        //
        //TotalSW.Restart();

        SqlConnection Conn = null;
        try
        {
            //Defined return format
            DataTable table = new DataTable("InsertResult");
            table.Columns.Add("InsertNumber", typeof(Int32));
            table.Columns.Add("UID", typeof(String));
            table.Columns.Add("ErrorCode", typeof(String));
            DataSet set = new DataSet();

            //Definde Insert Column string
            StringBuilder InsertColumns = new StringBuilder();
            //Definde Insert Value string
            StringBuilder InsertValues = new StringBuilder();

            for (int i = 0; i < targetColumns.Length; i++)
            {
                if (InsertColumns.Length == 0)
                {
                    InsertColumns.Append(targetColumns[i].Trim());
                }
                else
                {
                    InsertColumns.Append(string.Format(",{0}", targetColumns[i].Trim()));
                }
            }

            int ListCount = targetValue.Count;
            int ListRecord = 0;
            int ArrayCount = 0;
            //defined SQL statment
            foreach (string[] listout in targetValue)
            {
                InsertValues.Append("(");
                ArrayCount = listout.Length;
                for (int i = 0; i < ArrayCount - 1; i++)
                {
                    InsertValues.Append("@" + ListRecord.ToString() + "A" +i.ToString().Trim() + ",");   
                }
                InsertValues.Append("@" + ListRecord.ToString() + "A" + (ArrayCount - 1).ToString().Trim());
                InsertValues.Append("),");
                ListRecord++;
            }
            InsertValues.Remove(InsertValues.Length - 1, 1);

            string strSQL_Insert = string.Format("Insert into {0}({1}) values{2}", targetTable, InsertColumns.ToString(), InsertValues.ToString());
            //EventLog.Write(strSQL_Insert);
            if (IsUsePrimaryDB)
            {
                Conn = new SqlConnection(ConfigurationManager.ConnectionStrings["EagleVision_NewTable"].ConnectionString);
            }
            else
            {
                Conn = new SqlConnection(ConfigurationManager.ConnectionStrings["EagleVision_SCDDB6"].ConnectionString);
            }

            SqlCommand Comm_Insert = new SqlCommand(strSQL_Insert, Conn);

            //Defind each parameters values

            ListRecord = 0;
            foreach (string[] listout in targetValue)
            {
                ArrayCount = listout.Length;
                for (int i = 0; i < ArrayCount; i++)
                {
                    Comm_Insert.Parameters.Add(new SqlParameter("@" + ListRecord.ToString()+"A"+ i.ToString().Trim(), listout[i].Trim()));
                    EventLog.Write("@" + ListRecord.ToString() + "A" +i.ToString().Trim() + "=" + listout[i].Trim());
                }
                ListRecord++;
            }

            Conn.Open();
            //InsertSW.Restart();
            try
            {
                int intFlag = Comm_Insert.ExecuteNonQuery();
                //InsertSW.Stop();
                Conn.Close();
                //EventLog.Write("ConnectSW" + InsertSW.ElapsedMilliseconds.ToString());
				
				//Insert Data To Return Table
				DataRow row = table.NewRow();
                row["InsertNumber"] = ListCount; row["UID"] = 0;row["ErrorCode"] =0;table.Rows.Add(row);
				
                //Update Return Table
                set.Tables.Add(table);
                string insert_result;
                using (StringWriter sw = new StringWriter())
                {
                    set.Tables[0].WriteXml(sw);
                    insert_result = sw.ToString();
                }

                //TotalSW.Stop();
                //EventLog.Write("TotalSW : " + TotalSW.ElapsedMilliseconds.ToString());
                return insert_result;
            }
            catch (Exception ex)
            {
				
				//Insert Data To Return Table
				DataRow row = table.NewRow();
				row["InsertNumber"] = ListCount; row["UID"] = -1;row["ErrorCode"] ="[ERROR][MSG] " + ex.Message + "[Stack]" + ex.StackTrace;table.Rows.Add(row);
				
                //Update Return Table
                set.Tables.Add(table);
                string insert_result;
                using (StringWriter sw = new StringWriter())
                {
                    set.Tables[0].WriteXml(sw);
                    insert_result = sw.ToString();
                }

                //TotalSW.Stop();
                //EventLog.Write("TotalSW : " + TotalSW.ElapsedMilliseconds.ToString());
                return insert_result;
            }
        }
        catch (Exception ex)
        {
            //Defined return format
            DataTable table = new DataTable("InsertResult");
            table.Columns.Add("InsertNumber", typeof(Int32));
            table.Columns.Add("UID", typeof(String));
            table.Columns.Add("ErrorCode", typeof(String));
            DataSet set = new DataSet();

            //return "[ERROR][MSG] " + ex.Message + "[Stack]" + ex.StackTrace;
            int ListCount = targetValue.Count;
			DataRow row = table.NewRow();
			row["InsertNumber"] = ListCount; row["UID"] = -1;row["ErrorCode"] ="[ERROR][MSG] " + ex.Message + "[Stack]" + ex.StackTrace;table.Rows.Add(row);
			
            //Update Return Table
            set.Tables.Add(table);
            string insert_result;
            using (StringWriter sw = new StringWriter())
            {
                set.Tables[0].WriteXml(sw);
                insert_result = sw.ToString();
            }

            //TotalSW.Stop();
            //EventLog.Write("TotalSW : " + TotalSW.ElapsedMilliseconds.ToString());
            return insert_result;
        }
        finally
        {
            Conn = null;
        }

    }

    //Created by Austin Hsu, it's first version webservice
    [WebMethod]
    public string PerformDatabaseAction(int actionType, string[] targetColumns, string[] targetValues, string targetTable, string queryFilter, string sortBy)
    {
        EventLog.Write("[PerformDatabaseAction]+++");
        SqlConnection Conn = null;
        try
        {
            #region check input
            if (actionType == 0 || actionType == 1)
            {
                if (actionType == 0)
                {
                    if (targetColumns.Length == 0)
                    {
                        return string.Format("(actionType:{0}) targetColumns must have data.", actionType);
                    }
                }
                else
                {
                    if (targetColumns.Length == 0)
                    {
                        return string.Format("(actionType:{0}) targetColumns must have data.", actionType);
                    }
                    else
                    {
                        if (targetValues.Length == 0)
                        {
                            return string.Format("(actionType:{0}) targetValues must have data for insert intent.", actionType);
                        }
                        else
                        {
                            if (targetColumns.Length != targetValues.Length)
                            {
                                return string.Format("(actionType:{0}) Length of targetValues must be equal to targetColumns.", actionType);
                            }
                        }
                    }
                }
            }
            else
            {
                return "incorrect actionType";
            }
            #endregion

            switch (actionType) //0:query ; 1:insert
            {
                case 1:
                    StringBuilder objSB_Insert = new StringBuilder();
                    StringBuilder objSB_InsertP = new StringBuilder();
                    for (int i = 0; i < targetColumns.Length; i++)
                    {
                        if (objSB_Insert.Length == 0)
                        {
                            objSB_Insert.Append(targetColumns[i].Trim());
                            objSB_InsertP.Append("@" + targetColumns[i].Trim());
                        }
                        else
                        {
                            objSB_Insert.Append(string.Format(",{0}", targetColumns[i].Trim()));
                            objSB_InsertP.Append(string.Format(",@{0}", targetColumns[i].Trim()));
                        }
                    }
                    string strSQL_Insert = string.Format("Insert into {0}({1}) values({2})", targetTable, objSB_Insert.ToString(), objSB_InsertP.ToString());
                    Conn = new SqlConnection(ConfigurationManager.ConnectionStrings["EagleVision"].ConnectionString);
                    SqlCommand Comm_Insert = new SqlCommand(strSQL_Insert, Conn);
                    for (int i = 0; i < targetColumns.Length; i++)
                    {
                        Comm_Insert.Parameters.Add(new SqlParameter(targetColumns[i].Trim(), targetValues[i].Trim()));
                    }
                    Conn.Open();
                    int intFlag = Comm_Insert.ExecuteNonQuery();
                    Conn.Close();

                    return intFlag.ToString();
                    

                case 0: //query
                default:
                    StringBuilder objSB = new StringBuilder();
                    for (int i = 0; i < targetColumns.Length; i++)
                    {
                        if (objSB.Length == 0)
                        {
                            objSB.Append(targetColumns[i].Trim());
                        }
                        else
                        {
                            objSB.Append(string.Format(",{0}", targetColumns[i].Trim()));
                        }
                    }

                    string strWhere = "";
                    if (queryFilter.Trim().Length > 0)
                    {
                        strWhere = string.Format(" where {0} ", queryFilter);
                    }

                    string strOrderBy = "";
                    if (sortBy.Trim().Length > 0)
                    {
                        strOrderBy = string.Format(" order by {0} ", sortBy);
                    }

                    string strSQL = string.Format("Select {0} from {1} {2} {3}", objSB.ToString(), targetTable, strWhere, strOrderBy);
                    objSB = null;
                    Conn = new SqlConnection(ConfigurationManager.ConnectionStrings["EagleVision_Read"].ConnectionString);
                    SqlDataAdapter da = new SqlDataAdapter(strSQL, Conn);
                    DataSet ds = new DataSet();
                    Conn.Open();
                    da.Fill(ds);
                    Conn.Close();

                    string result;
                    using (StringWriter sw = new StringWriter())
                    {
                        ds.Tables[0].WriteXml(sw);
                        result = sw.ToString();
                    }
                    return result;
            }
        }
        catch (Exception ex)
        {
            return "[ERROR][MSG] " + ex.Message + "[Stack]" + ex.StackTrace;
        }
        finally
        {
            Conn = null;
        }

    }
    
    //Created by Austin Hsu, it's first version webservice
    [WebMethod]
    public string PerformDatabaseAction2(int actionType, string[] targetColumns, string[] targetValues, string targetTable, string queryFilter, string sortBy, Boolean IsUsePrimaryDB)
    {
        EventLog.Write("[PerformDatabaseAction2]+++");
        SqlConnection Conn = null;
        try
        {
            #region check input
            if (actionType == 0 || actionType == 1)
            {
                if (actionType == 0)
                {
                    if (targetColumns.Length == 0)
                    {
                        return string.Format("(actionType:{0}) targetColumns must have data.", actionType);
                    }
                }
                else
                {
                    if (targetColumns.Length == 0)
                    {
                        return string.Format("(actionType:{0}) targetColumns must have data.", actionType);
                    }
                    else
                    {
                        if (targetValues.Length == 0)
                        {
                            return string.Format("(actionType:{0}) targetValues must have data for insert intent.", actionType);
                        }
                        else
                        {
                            if (targetColumns.Length != targetValues.Length)
                            {
                                return string.Format("(actionType:{0}) Length of targetValues must be equal to targetColumns.", actionType);
                            }
                        }
                    }
                }
            }
            else
            {
                return "incorrect actionType";
            }
            #endregion

            switch (actionType) //0:query ; 1:insert
            {
                case 1:
                    StringBuilder objSB_Insert = new StringBuilder();
                    StringBuilder objSB_InsertP = new StringBuilder();
                    for (int i = 0; i < targetColumns.Length; i++)
                    {
                        if (objSB_Insert.Length == 0)
                        {
                            objSB_Insert.Append(targetColumns[i].Trim());
                            objSB_InsertP.Append("@" + targetColumns[i].Trim());
                        }
                        else
                        {
                            objSB_Insert.Append(string.Format(",{0}", targetColumns[i].Trim()));
                            objSB_InsertP.Append(string.Format(",@{0}", targetColumns[i].Trim()));
                        }
                    }
                    string strSQL_Insert = string.Format("Insert into {0}({1}) values({2})", targetTable, objSB_Insert.ToString(), objSB_InsertP.ToString());
                    Conn = new SqlConnection(ConfigurationManager.ConnectionStrings["EagleVision"].ConnectionString);
                    SqlCommand Comm_Insert = new SqlCommand(strSQL_Insert, Conn);
                    for (int i = 0; i < targetColumns.Length; i++)
                    {
                        Comm_Insert.Parameters.Add(new SqlParameter(targetColumns[i].Trim(), targetValues[i].Trim()));
                    }
                    Conn.Open();
                    int intFlag = Comm_Insert.ExecuteNonQuery();
                    Conn.Close();

                    return intFlag.ToString();

                case 0: //query
                default:
                    StringBuilder objSB = new StringBuilder();
                    for (int i = 0; i < targetColumns.Length; i++)
                    {
                        if (objSB.Length == 0)
                        {
                            objSB.Append(targetColumns[i].Trim());
                        }
                        else
                        {
                            objSB.Append(string.Format(",{0}", targetColumns[i].Trim()));
                        }
                    }

                    string strWhere = "";
                    if (queryFilter.Trim().Length > 0)
                    {
                        strWhere = string.Format(" where {0} ", queryFilter);
                    }

                    string strOrderBy = "";
                    if (sortBy.Trim().Length > 0)
                    {
                        strOrderBy = string.Format(" order by {0} ", sortBy);
                    }

                    string strSQL = string.Format("Select {0} from {1} {2} {3}", objSB.ToString(), targetTable, strWhere, strOrderBy);
                    objSB = null;
                    if (IsUsePrimaryDB)
                    {
                        Conn = new SqlConnection(ConfigurationManager.ConnectionStrings["EagleVision"].ConnectionString);
                    }
                    else
                    {
                        Conn = new SqlConnection(ConfigurationManager.ConnectionStrings["EagleVision_Read"].ConnectionString);
                    }
                    SqlDataAdapter da = new SqlDataAdapter(strSQL, Conn);
                    DataSet ds = new DataSet();
                    Conn.Open();
                    da.Fill(ds);
                    Conn.Close();

                    string result;
                    using (StringWriter sw = new StringWriter())
                    {
                        ds.Tables[0].WriteXml(sw);
                        result = sw.ToString();
                    }
                    return result;
            }
        }
        catch (Exception ex)
        {
            return "[ERROR][MSG] " + ex.Message + "[Stack]" + ex.StackTrace;
        }
        finally
        {
            Conn = null;
        }

    }

    //Created by Yuhsuan Chen on 2015/04/07, it's can response insert / query uid status
    [WebMethod]
    public string PerformDatabaseAction3(int actionType, string[] targetColumns, string[] targetValues, string targetTable, string queryFilter, string sortBy, Boolean IsUsePrimaryDB)
    {
        EventLog.Write("[PerformDatabaseAction3]+++");
        SqlConnection Conn = null;
        try
        {
            #region check input
            if (actionType == 0 || actionType == 1)
            {
                if (actionType == 0)
                {
                    if (targetColumns.Length == 0)
                    {
                        return string.Format("(actionType:{0}) targetColumns must have data.", actionType);
                    }
                }
                else
                {
                    if (targetColumns.Length == 0)
                    {
                        return string.Format("(actionType:{0}) targetColumns must have data.", actionType);
                    }
                    else
                    {
                        if (targetValues.Length == 0)
                        {
                            return string.Format("(actionType:{0}) targetValues must have data for insert intent.", actionType);
                        }
                        else
                        {
                            if (targetColumns.Length != targetValues.Length)
                            {
                                return string.Format("(actionType:{0}) Length of targetValues must be equal to targetColumns.", actionType);
                            }
                        }
                    }
                }
            }
            else
            {
                return "incorrect actionType";
            }
            #endregion

            switch (actionType) //0:query ; 1:insert
            {
                case 1:
                    //Create data set for insert result
                    DataTable table = new DataTable("InsertResult");
                    table.Columns.Add("ReturnCode", typeof(Int32));
                    table.Columns.Add("UID", typeof(String));
                    table.Columns.Add("ErrorCode", typeof(String));

                    DataSet set = new DataSet();
                    StringBuilder objSB_Insert = new StringBuilder();
                    StringBuilder objSB_InsertP = new StringBuilder();
                    for (int i = 0; i < targetColumns.Length; i++)
                    {
                        if (objSB_Insert.Length == 0)
                        {
                            objSB_Insert.Append(targetColumns[i].Trim());
                            objSB_InsertP.Append("@" + targetColumns[i].Trim());
                        }
                        else
                        {
                            objSB_Insert.Append(string.Format(",{0}", targetColumns[i].Trim()));
                            objSB_InsertP.Append(string.Format(",@{0}", targetColumns[i].Trim()));
                        }
                    }
                    string strSQL_Insert = string.Format("Insert into {0}({1}) values({2})", targetTable, objSB_Insert.ToString(), objSB_InsertP.ToString()) + "SELECT SCOPE_IDENTITY();";
                    Conn = new SqlConnection(ConfigurationManager.ConnectionStrings["EagleVision_NewTable"].ConnectionString);
                    SqlCommand Comm_Insert = new SqlCommand(strSQL_Insert, Conn);
                    for (int i = 0; i < targetColumns.Length; i++)
                    {
                        Comm_Insert.Parameters.Add(new SqlParameter(targetColumns[i].Trim(), targetValues[i].Trim()));
                    }
                    try
                    {
                        Conn.Open();
                        //Get last uid from server
                        string intFlag = Comm_Insert.ExecuteScalar().ToString();
                        Conn.Close();

                        DataRow row = table.NewRow();
                        row["ReturnCode"] = 1;row["UID"] = intFlag;row["ErrorCode"] = "No Error";
                        table.Rows.Add(row);
                        set.Tables.Add(table);

                        string insert_result;
                        using (StringWriter sw = new StringWriter())
                        {
                            set.Tables[0].WriteXml(sw);
                            insert_result = sw.ToString();
                        }
                        //return intFlag.ToString();
                        return insert_result;
                    }
                    catch (Exception ex)
                    {
                        if (ex.Message.IndexOf("The duplicate key value is") >= 0)
                        {
                            DataRow row = table.NewRow();
                            row["ReturnCode"] = 0;row["UID"] = 0;row["ErrorCode"] = "Some value is duplicate"+ex.Message;
                            table.Rows.Add(row);

                            set.Tables.Add(table);

                            string insert_result;
                            using (StringWriter sw = new StringWriter())
                            {
                                set.Tables[0].WriteXml(sw);
                                insert_result = sw.ToString();
                            }
                            return insert_result;
                        }
                        else
                        {
                            DataRow row = table.NewRow();
                            row["ReturnCode"] = -1;row["UID"] = 0;row["ErrorCode"] = "[ERROR][MSG] " + ex.Message + "[Stack]" + ex.StackTrace;
                            table.Rows.Add(row);

                            set.Tables.Add(table);

                            string insert_result;
                            using (StringWriter sw = new StringWriter())
                            {
                                set.Tables[0].WriteXml(sw);
                                insert_result = sw.ToString();
                            }
                            return insert_result;
                        }
                    }
                case 0: //query
                default:
                    StringBuilder objSB = new StringBuilder();
                    for (int i = 0; i < targetColumns.Length; i++)
                    {
                        if (objSB.Length == 0)
                        {
                            objSB.Append(targetColumns[i].Trim());
                        }
                        else
                        {
                            objSB.Append(string.Format(",{0}", targetColumns[i].Trim()));
                        }
                    }

                    string strWhere = "";
                    if (queryFilter.Trim().Length > 0)
                    {
                        strWhere = string.Format(" where {0} ", queryFilter);
                    }

                    string strOrderBy = "";
                    if (sortBy.Trim().Length > 0)
                    {
                        strOrderBy = string.Format(" order by {0} ", sortBy);
                    }

                    string strSQL = string.Format("Select {0} from {1} {2} {3}", objSB.ToString(), targetTable, strWhere, strOrderBy);
                    objSB = null;
                    if (IsUsePrimaryDB)
                    {
                        Conn = new SqlConnection(ConfigurationManager.ConnectionStrings["EagleVision_NewTable"].ConnectionString);
                    }
                    else
                    {
                        Conn = new SqlConnection(ConfigurationManager.ConnectionStrings["EagleVision_NewTable_Read"].ConnectionString);
                    }
                    SqlDataAdapter da = new SqlDataAdapter(strSQL, Conn);
                    DataSet ds = new DataSet();
                    Conn.Open();
                    da.Fill(ds);
                    Conn.Close();

                    string result;
                    using (StringWriter sw = new StringWriter())
                    {
                        ds.Tables[0].WriteXml(sw);
                        result = sw.ToString();
                    }
                    return result;
            }
        }
        catch (Exception ex)
        {
            return "[ERROR][MSG] " + ex.Message + "[Stack]" + ex.StackTrace;
        }
        finally
        {
            Conn = null;
        }

    }

	[WebMethod]
    public string PerformDatabaseAction4(int actionType, string[] targetColumns, string[] targetValues, string targetTable, string queryFilter, string sortBy, Boolean IsUsePrimaryDB)
    {
        EventLog.Write("[PerformDatabaseAction4]+++");
        System.Diagnostics.Stopwatch TotalSW = new System.Diagnostics.Stopwatch();
        System.Diagnostics.Stopwatch ConnectSW = new System.Diagnostics.Stopwatch();

        //Inital the total execution time
        ConnectSW.Reset();
        TotalSW.Reset();
        TotalSW.Start();

        SqlConnection Conn = null;
        try
        {
            #region check input
            if (actionType == 0 || actionType == 1)
            {
                if (actionType == 0)
                {
                    if (targetColumns.Length == 0)
                    {
                        return string.Format("(actionType:{0}) targetColumns must have data.", actionType);
                    }
                }
                else
                {
                    if (targetColumns.Length == 0)
                    {
                        return string.Format("(actionType:{0}) targetColumns must have data.", actionType);
                    }
                    else
                    {
                        if (targetValues.Length == 0)
                        {
                            return string.Format("(actionType:{0}) targetValues must have data for insert intent.", actionType);
                        }
                        else
                        {
                            if (targetColumns.Length != targetValues.Length)
                            {
                                return string.Format("(actionType:{0}) Length of targetValues must be equal to targetColumns.", actionType);
                            }
                        }
                    }
                }
            }
            else
            {
                return "incorrect actionType";
            }
            #endregion

            switch (actionType) //0:query ; 1:insert
            {
                case 1:
                    //Create data set for insert result
                    DataTable table = new DataTable("InsertResult");
                    table.Columns.Add("ReturnCode", typeof(Int32));
                    table.Columns.Add("UID", typeof(String));
                    table.Columns.Add("ErrorCode", typeof(String));

                    DataSet set = new DataSet();
                    StringBuilder objSB_Insert = new StringBuilder();
                    StringBuilder objSB_InsertP = new StringBuilder();
                    for (int i = 0; i < targetColumns.Length; i++)
                    {
                        if (objSB_Insert.Length == 0)
                        {
                            objSB_Insert.Append(targetColumns[i].Trim());
                            objSB_InsertP.Append("@" + targetColumns[i].Trim());
                        }
                        else
                        {
                            objSB_Insert.Append(string.Format(",{0}", targetColumns[i].Trim()));
                            objSB_InsertP.Append(string.Format(",@{0}", targetColumns[i].Trim()));
                        }
                    }
                    string strSQL_Insert = string.Format("Insert into {0}({1}) values({2})", targetTable, objSB_Insert.ToString(), objSB_InsertP.ToString()) + "SELECT SCOPE_IDENTITY();";
                    Conn = new SqlConnection(ConfigurationManager.ConnectionStrings["EagleVision_SCDDB6"].ConnectionString);
                    SqlCommand Comm_Insert = new SqlCommand(strSQL_Insert, Conn);
                    for (int i = 0; i < targetColumns.Length; i++)
                    {
                        Comm_Insert.Parameters.Add(new SqlParameter(targetColumns[i].Trim(), targetValues[i].Trim()));
                    }
                    try
                    {
                        ConnectSW.Start();
                        Conn.Open();
                        //Get last uid from server
                        string intFlag = Comm_Insert.ExecuteScalar().ToString();
                        Conn.Close();
                        ConnectSW.Stop();

                        DataRow row = table.NewRow();
                        row["ReturnCode"] = 1; row["UID"] = intFlag; row["ErrorCode"] = "No Error";
                        table.Rows.Add(row);
                        set.Tables.Add(table);

                        string insert_result;
                        using (StringWriter sw = new StringWriter())
                        {
                            set.Tables[0].WriteXml(sw);
                            insert_result = sw.ToString();
                        }
                        //Calculate the execution time
                        TotalSW.Stop();
                        EventLog.Write(", Insert: Total Time, " + Math.Round(TotalSW.Elapsed.TotalMilliseconds, 2).ToString() + ", SQL Excute Time, " + Math.Round(ConnectSW.Elapsed.TotalMilliseconds, 2).ToString() + ", WebService Excute Time, " + Math.Round((TotalSW.Elapsed.TotalMilliseconds - ConnectSW.Elapsed.TotalMilliseconds), 2).ToString() + ", UID, " + intFlag);
                        //return intFlag.ToString();
                        return insert_result;
                    }
                    catch (Exception ex)
                    {
                        if (ex.Message.IndexOf("The duplicate key value is") >= 0)
                        {
                            DataRow row = table.NewRow();
                            row["ReturnCode"] = 0; row["UID"] = 0; row["ErrorCode"] = "Some value is duplicate" + ex.Message;
                            table.Rows.Add(row);

                            set.Tables.Add(table);

                            string insert_result;
                            using (StringWriter sw = new StringWriter())
                            {
                                set.Tables[0].WriteXml(sw);
                                insert_result = sw.ToString();
                            }
                            TotalSW.Stop();
                            EventLog.Write(", InsertFail: Total Time, " + Math.Round(TotalSW.Elapsed.TotalMilliseconds, 2).ToString() + ", SQL Excute Time, " + Math.Round(ConnectSW.Elapsed.TotalMilliseconds, 2).ToString() + ", WebService Excute Time, " + Math.Round((TotalSW.Elapsed.TotalMilliseconds - ConnectSW.Elapsed.TotalMilliseconds), 2).ToString() + ", UID, 0");
                            return insert_result;
                        }
                        else
                        {
                            DataRow row = table.NewRow();
                            row["ReturnCode"] = -1; row["UID"] = 0; row["ErrorCode"] = "[ERROR][MSG] " + ex.Message + "[Stack]" + ex.StackTrace;
                            table.Rows.Add(row);

                            set.Tables.Add(table);

                            string insert_result;
                            using (StringWriter sw = new StringWriter())
                            {
                                set.Tables[0].WriteXml(sw);
                                insert_result = sw.ToString();
                            }
                            TotalSW.Stop();
                            EventLog.Write(", InsertFail: Total Time, " + Math.Round(TotalSW.Elapsed.TotalMilliseconds, 2).ToString() + ", SQL Excute Time, " + Math.Round(ConnectSW.Elapsed.TotalMilliseconds, 2).ToString() + ", WebService Excute Time, " + Math.Round((TotalSW.Elapsed.TotalMilliseconds - ConnectSW.Elapsed.TotalMilliseconds), 2).ToString() + ", UID, 0");
                            return insert_result;
                        }
                    }
                case 0: //query
                default:
                    StringBuilder objSB = new StringBuilder();
                    for (int i = 0; i < targetColumns.Length; i++)
                    {
                        if (objSB.Length == 0)
                        {
                            objSB.Append(targetColumns[i].Trim());
                        }
                        else
                        {
                            objSB.Append(string.Format(",{0}", targetColumns[i].Trim()));
                        }
                    }

                    string strWhere = "";
                    if (queryFilter.Trim().Length > 0)
                    {
                        strWhere = string.Format(" where {0} ", queryFilter);
                    }

                    string strOrderBy = "";
                    if (sortBy.Trim().Length > 0)
                    {
                        strOrderBy = string.Format(" order by {0} ", sortBy);
                    }

                    string strSQL = string.Format("Select {0} from {1} {2} {3}", objSB.ToString(), targetTable, strWhere, strOrderBy);
                    objSB = null;
                    if (IsUsePrimaryDB)
                    {
                        Conn = new SqlConnection(ConfigurationManager.ConnectionStrings["EagleVision_SCDDB6"].ConnectionString);
                    }
                    else
                    {
                        Conn = new SqlConnection(ConfigurationManager.ConnectionStrings["EagleVision_SCDDB6_Read"].ConnectionString);
                    }
                    SqlDataAdapter da = new SqlDataAdapter(strSQL, Conn);
                    DataSet ds = new DataSet();
                    ConnectSW.Start();
                    Conn.Open();
                    da.Fill(ds);
                    Conn.Close();
                    ConnectSW.Stop();

                    string result;
                    using (StringWriter sw = new StringWriter())
                    {
                        ds.Tables[0].WriteXml(sw);
                        result = sw.ToString();
                    }
                    //Calculate the execution time
                    TotalSW.Stop();
                    EventLog.Write(", Query: Total Time, " + Math.Round(TotalSW.Elapsed.TotalMilliseconds, 2).ToString() + ", SQL Excute Time, " + Math.Round(ConnectSW.Elapsed.TotalMilliseconds, 2).ToString() + ", WebService Excute Time, " + Math.Round((TotalSW.Elapsed.TotalMilliseconds - ConnectSW.Elapsed.TotalMilliseconds), 2).ToString());
                    return result;
            }
        }
        catch (Exception ex)
        {
            TotalSW.Stop();
            EventLog.Write(", Error: Total Time, " + Math.Round(TotalSW.Elapsed.TotalMilliseconds, 2).ToString());
            return "[ERROR][MSG] " + ex.Message + "[Stack]" + ex.StackTrace;
        }
        finally
        {
            Conn = null;
        }

    }
	
    //Created by Yuhsuan Chen on 2015/10/02, user can chnage connection DB
    //[WebMethod(MessageName = "BasicOperate")]
    //public string DBService(int actionType, string[] targetColumns, string[] targetValues, string targetTable, string queryFilter, string sortBy, string DBConnection)
    [WebMethod]
    public string DBService_Basic_Operate(int actionType, string[] targetColumns, string[] targetValues, string targetTable, string queryFilter, string sortBy, string DBConnection)
    {
        EventLog.Write("[BasicOperate]+++");
        System.Diagnostics.Stopwatch TotalSW = new System.Diagnostics.Stopwatch();
        System.Diagnostics.Stopwatch ConnectSW = new System.Diagnostics.Stopwatch();

        //Inital the total execution time
        ConnectSW.Reset();
        TotalSW.Reset();
        TotalSW.Start();

        SqlConnection Conn = null;
        try
        {
            EventLog.Write("[BasicOperate]+++");
            #region check input
            if (actionType == 0 || actionType == 1)
            {
                if (actionType == 0)
                {
                    if (targetColumns.Length == 0)
                    {
                        return string.Format("(actionType:{0}) targetColumns must have data.", actionType);
                    }
                }
                else
                {
                    if (targetColumns.Length == 0)
                    {
                        return string.Format("(actionType:{0}) targetColumns must have data.", actionType);
                    }
                    else
                    {
                        if (targetValues.Length == 0)
                        {
                            return string.Format("(actionType:{0}) targetValues must have data for insert intent.", actionType);
                        }
                        else
                        {
                            if (targetColumns.Length != targetValues.Length)
                            {
                                return string.Format("(actionType:{0}) Length of targetValues must be equal to targetColumns.", actionType);
                            }
                        }
                    }
                }
            }
            else
            {
                return "incorrect actionType";
            }
            #endregion

            switch (actionType) //0:query ; 1:insert
            {
                #region InsertData
                case 1:
                    //Create data set for insert result
                    EventLog.Write("[DBService_Basic_Operate][InsertData] Create data set for insert result");
                    DataTable table = new DataTable("InsertResult");
                    table.Columns.Add("ReturnCode", typeof(Int32));
                    table.Columns.Add("UID", typeof(String));
                    table.Columns.Add("ErrorCode", typeof(String));

                    DataSet set = new DataSet();
                    StringBuilder objSB_Insert = new StringBuilder();
                    StringBuilder objSB_InsertP = new StringBuilder();
                    for (int i = 0; i < targetColumns.Length; i++)
                    {
                        if (objSB_Insert.Length == 0)
                        {
                            objSB_Insert.Append(targetColumns[i].Trim());
                            objSB_InsertP.Append("@" + targetColumns[i].Trim());
                        }
                        else
                        {
                            objSB_Insert.Append(string.Format(",{0}", targetColumns[i].Trim()));
                            objSB_InsertP.Append(string.Format(",@{0}", targetColumns[i].Trim()));
                        }
                    }
                    string strSQL_Insert = string.Format("Insert into {0}({1}) values({2})", targetTable, objSB_Insert.ToString(), objSB_InsertP.ToString()) + "SELECT SCOPE_IDENTITY();";
                    EventLog.Write(strSQL_Insert);
                    Conn = new SqlConnection(ConfigurationManager.ConnectionStrings[DBConnection].ConnectionString);
                    EventLog.Write(Conn.ToString());
                    SqlCommand Comm_Insert = new SqlCommand(strSQL_Insert, Conn);
                    for (int i = 0; i < targetColumns.Length; i++)
                    {
                        Comm_Insert.Parameters.Add(new SqlParameter(targetColumns[i].Trim(), targetValues[i].Trim()));
                    }
                    try
                    {
                        ConnectSW.Start();
                        Conn.Open();
                        //Get last uid from server
                        string intFlag = Comm_Insert.ExecuteScalar().ToString();
                        Conn.Close();
                        ConnectSW.Stop();

                        DataRow row = table.NewRow();
                        row["ReturnCode"] = 1; row["UID"] = intFlag; row["ErrorCode"] = "No Error";
                        table.Rows.Add(row);
                        set.Tables.Add(table);

                        string insert_result;
                        using (StringWriter sw = new StringWriter())
                        {
                            set.Tables[0].WriteXml(sw);
                            insert_result = sw.ToString();
                        }
                        //Calculate the execution time
                        TotalSW.Stop();
                        EventLog.Write(", Insert: Total Time, " + Math.Round(TotalSW.Elapsed.TotalMilliseconds, 2).ToString() + ", SQL Excute Time, " + Math.Round(ConnectSW.Elapsed.TotalMilliseconds, 2).ToString() + ", WebService Excute Time, " + Math.Round((TotalSW.Elapsed.TotalMilliseconds - ConnectSW.Elapsed.TotalMilliseconds), 2).ToString() + ", UID, " + intFlag);
                        //return intFlag.ToString();

                        EventLog.Write("--- BasicOperate");
                        return insert_result;
                    }
                    catch (Exception ex)
                    {
                        if (ex.Message.IndexOf("The duplicate key value is") >= 0)
                        {
                            DataRow row = table.NewRow();
                            row["ReturnCode"] = 0; row["UID"] = 0; row["ErrorCode"] = "Some value is duplicate" + ex.Message;
                            table.Rows.Add(row);

                            set.Tables.Add(table);

                            string insert_result;
                            using (StringWriter sw = new StringWriter())
                            {
                                set.Tables[0].WriteXml(sw);
                                insert_result = sw.ToString();
                            }
                            TotalSW.Stop();
                            EventLog.Write(", InsertFail: Total Time, " + Math.Round(TotalSW.Elapsed.TotalMilliseconds, 2).ToString() + ", SQL Excute Time, " + Math.Round(ConnectSW.Elapsed.TotalMilliseconds, 2).ToString() + ", WebService Excute Time, " + Math.Round((TotalSW.Elapsed.TotalMilliseconds - ConnectSW.Elapsed.TotalMilliseconds), 2).ToString() + ", UID, 0");
                            EventLog.Write("--- BasicOperate");
                            return insert_result;
                        }
                        else
                        {
                            DataRow row = table.NewRow();
                            row["ReturnCode"] = -1; row["UID"] = 0; row["ErrorCode"] = "[ERROR][MSG] " + ex.Message + "[Stack]" + ex.StackTrace;
                            table.Rows.Add(row);

                            set.Tables.Add(table);

                            string insert_result;
                            using (StringWriter sw = new StringWriter())
                            {
                                set.Tables[0].WriteXml(sw);
                                insert_result = sw.ToString();
                            }
                            TotalSW.Stop();
                            EventLog.Write(", InsertFail: Total Time, " + Math.Round(TotalSW.Elapsed.TotalMilliseconds, 2).ToString() + ", SQL Excute Time, " + Math.Round(ConnectSW.Elapsed.TotalMilliseconds, 2).ToString() + ", WebService Excute Time, " + Math.Round((TotalSW.Elapsed.TotalMilliseconds - ConnectSW.Elapsed.TotalMilliseconds), 2).ToString() + ", UID, 0");
                            EventLog.Write("--- BasicOperate");
                            return insert_result;
                        }
                    }
                #endregion
                #region QueryData
                case 0: //query
                default:
                    StringBuilder objSB = new StringBuilder();
                    for (int i = 0; i < targetColumns.Length; i++)
                    {
                        if (objSB.Length == 0)
                        {
                            objSB.Append(targetColumns[i].Trim());
                        }
                        else
                        {
                            objSB.Append(string.Format(",{0}", targetColumns[i].Trim()));
                        }
                    }

                    string strWhere = "";
                    if (queryFilter.Trim().Length > 0)
                    {
                        strWhere = string.Format(" where {0} ", queryFilter);
                    }

                    string strOrderBy = "";
                    if (sortBy.Trim().Length > 0)
                    {
                        strOrderBy = string.Format(" order by {0} ", sortBy);
                    }

                    string strSQL = string.Format("Select {0} from {1} {2} {3}", objSB.ToString(), targetTable, strWhere, strOrderBy);
                    objSB = null;

                    Conn = new SqlConnection(ConfigurationManager.ConnectionStrings[DBConnection].ConnectionString);
                    
                    SqlDataAdapter da = new SqlDataAdapter(strSQL, Conn);
                    DataSet ds = new DataSet();
                    ConnectSW.Start();
                    Conn.Open();
                    da.Fill(ds);
                    Conn.Close();
                    ConnectSW.Stop();

                    string result;
                    using (StringWriter sw = new StringWriter())
                    {
                        ds.Tables[0].WriteXml(sw);
                        result = sw.ToString();
                    }
                    //Calculate the execution time
                    //TotalSW.Stop();
                    //EventLog.Write(", Query: Total Time, " + Math.Round(TotalSW.Elapsed.TotalMilliseconds, 2).ToString() + ", SQL Excute Time, " + Math.Round(ConnectSW.Elapsed.TotalMilliseconds, 2).ToString() + ", WebService Excute Time, " + Math.Round((TotalSW.Elapsed.TotalMilliseconds - ConnectSW.Elapsed.TotalMilliseconds), 2).ToString());
                    EventLog.Write("--- BasicOperate");
                    return result;
                #endregion
            }
        }
        catch (Exception ex)
        {
            //TotalSW.Stop();
            //EventLog.Write(", Error: Total Time, " + Math.Round(TotalSW.Elapsed.TotalMilliseconds, 2).ToString());
            EventLog.Write("--- BasicOperate");
            return "[ERROR][MSG] " + ex.Message + "[Stack]" + ex.StackTrace;
        }
        finally
        {
            Conn = null;
        }

    }

    //[WebMethod(MessageName = "CommonBatchInsert")]
    //public string DBService(string[] targetColumns, List<string[]> targetValue, string targetTable, string DBConnection)
    [WebMethod]
    public string DBService_Common_Batch_Insert(string[] targetColumns, List<string[]> targetValue, string targetTable, string DBConnection)
    {
        //System.Diagnostics.Stopwatch TotalSW = new System.Diagnostics.Stopwatch();
        //System.Diagnostics.Stopwatch InsertSW = new System.Diagnostics.Stopwatch();
        //System.Diagnostics.Stopwatch QueryTotalSW = new System.Diagnostics.Stopwatch();
        //System.Diagnostics.Stopwatch QueryEachSW = new System.Diagnostics.Stopwatch();
        //System.Diagnostics.Stopwatch QueryResponSW = new System.Diagnostics.Stopwatch();
        //
        //TotalSW.Restart();

        SqlConnection Conn = null;
        try
        {
            EventLog.Write("+++ CommonBatchInsert");
            //Defined return format
            DataTable table = new DataTable("InsertResult");
            table.Columns.Add("InsertNumber", typeof(Int32));
            table.Columns.Add("UID", typeof(String));
            table.Columns.Add("ErrorCode", typeof(String));
            DataSet set = new DataSet();

            //Definde Insert Column string
            StringBuilder InsertColumns = new StringBuilder();
            //Definde Insert Value string
            StringBuilder InsertValues = new StringBuilder();

            for (int i = 0; i < targetColumns.Length; i++)
            {
                if (InsertColumns.Length == 0)
                {
                    InsertColumns.Append(targetColumns[i].Trim());
                }
                else
                {
                    InsertColumns.Append(string.Format(",{0}", targetColumns[i].Trim()));
                }
            }

            int ListCount = targetValue.Count;
            int ListRecord = 0;
            int ArrayCount = 0;
            //defined SQL statment
            foreach (string[] listout in targetValue)
            {
                InsertValues.Append("(");
                ArrayCount = listout.Length;
                for (int i = 0; i < ArrayCount - 1; i++)
                {
                    InsertValues.Append("@" + ListRecord.ToString() + "A" +i.ToString().Trim() + ",");
                    //InsertValues.Append(listout[i].Trim() + ",");
                }
                InsertValues.Append("@" + ListRecord.ToString() + "A" + (ArrayCount - 1).ToString().Trim());
                //InsertValues.Append(listout[ArrayCount - 1].Trim());
                InsertValues.Append("),");
                ListRecord++;
            }
            InsertValues.Remove(InsertValues.Length - 1, 1);
            EventLog.Write(InsertValues.ToString());

            string strSQL_Insert = string.Format("Insert into {0}({1}) values{2}", targetTable, InsertColumns.ToString(), InsertValues.ToString());
            EventLog.Write(strSQL_Insert);
            Conn = new SqlConnection(ConfigurationManager.ConnectionStrings[DBConnection].ConnectionString);            

            SqlCommand Comm_Insert = new SqlCommand(strSQL_Insert, Conn);

            //Defind each parameters values

            ListRecord = 0;
            foreach (string[] listout in targetValue)
            {
                ArrayCount = listout.Length;
                for (int i = 0; i < ArrayCount; i++)
                {
                    Comm_Insert.Parameters.Add(new SqlParameter("@" + ListRecord.ToString() + "A" + i.ToString().Trim(), listout[i].Trim()));
                    EventLog.Write("@" + ListRecord.ToString() +"A"+ i.ToString().Trim() + "=" + listout[i].Trim());
                }
                ListRecord++;
            }

            Conn.Open();
            //InsertSW.Restart();
            try
            {
                int intFlag = Comm_Insert.ExecuteNonQuery();
                //InsertSW.Stop();
                Conn.Close();
                //EventLog.Write("ConnectSW" + InsertSW.ElapsedMilliseconds.ToString());

                //Insert Data To Return Table for all insert data
                //for (int i = 1; i <= ListCount; i++)
                //{
                //    DataRow row = table.NewRow();
                //    row["InsertNumber"] = i; row["UID"] = 0; table.Rows.Add(row);
                //}

                //Insert Data To Return Table
                DataRow row = table.NewRow();
                row["InsertNumber"] = ListCount; row["UID"] = 0; row["ErrorCode"] = 0; table.Rows.Add(row);

                //Update Return Table
                set.Tables.Add(table);
                string insert_result;
                using (StringWriter sw = new StringWriter())
                {
                    set.Tables[0].WriteXml(sw);
                    insert_result = sw.ToString();
                }

                //TotalSW.Stop();
                //EventLog.Write("TotalSW : " + TotalSW.ElapsedMilliseconds.ToString());
                EventLog.Write("--- CommonBatchInsert");
                return insert_result;
            }
            catch (Exception ex)
            {
                //Insert Data To Return Table for all insert data
                //for (int i = 1; i <= ListCount; i++)
                //{
                //    DataRow row = table.NewRow();
                //    row["InsertNumber"] = i; row["UID"] = -1; table.Rows.Add(row);
                //}

                //Insert Data To Return Table
                DataRow row = table.NewRow();
                row["InsertNumber"] = ListCount; row["UID"] = -1; row["ErrorCode"] = "[ERROR][MSG] " + ex.Message + "[Stack]" + ex.StackTrace; table.Rows.Add(row);

                //Update Return Table
                set.Tables.Add(table);
                string insert_result;
                using (StringWriter sw = new StringWriter())
                {
                    set.Tables[0].WriteXml(sw);
                    insert_result = sw.ToString();
                }

                //TotalSW.Stop();
                //EventLog.Write("TotalSW : " + TotalSW.ElapsedMilliseconds.ToString());
                EventLog.Write("--- CommonBatchInsert");
                return insert_result;
            }
        }
        catch (Exception ex)
        {
            //Defined return format
            DataTable table = new DataTable("InsertResult");
            table.Columns.Add("InsertNumber", typeof(Int32));
            table.Columns.Add("UID", typeof(String));
            table.Columns.Add("ErrorCode", typeof(String));
            DataSet set = new DataSet();

            return "[ERROR][MSG] " + ex.Message + "[Stack]" + ex.StackTrace;
            int ListCount = targetValue.Count;
            DataRow row = table.NewRow();
            row["InsertNumber"] = ListCount; row["UID"] = -1; row["ErrorCode"] = "[ERROR][MSG] " + ex.Message + "[Stack]" + ex.StackTrace; table.Rows.Add(row);

            //Update Return Table
            set.Tables.Add(table);
            string insert_result;
            using (StringWriter sw = new StringWriter())
            {
                set.Tables[0].WriteXml(sw);
                insert_result = sw.ToString();
            }

            //TotalSW.Stop();
            //EventLog.Write("TotalSW : " + TotalSW.ElapsedMilliseconds.ToString());
            EventLog.Write("--- CommonBatchInsert");
            return insert_result;
        }
        finally
        {
            Conn = null;
        }

    }

    //User can check all DBList
    //[WebMethod(MessageName = "DBList")]
    //public string DBService()
    [WebMethod]
    public string DBService_DBList()
    {
        try
        {
            EventLog.Write("+++ DBList");
            string DBList = "";

            //Create container
            DataSet set = new DataSet();
            
            DataTable table = new DataTable("DBList");
            table.Columns.Add("List", typeof(String));
            table.Columns.Add("ConnectionString", typeof(String));

            ConnectionStringSettingsCollection settings = ConfigurationManager.ConnectionStrings;

            if (settings != null)
            {
                foreach (ConnectionStringSettings cs in settings)
                {
                    table.Rows.Add(cs.Name,cs.ConnectionString.Substring(0, (cs.ConnectionString.LastIndexOf("User"))));
                    
                    //EventLog.Write(cs.Name);
                }
            }
            set.Tables.Add(table);

            using (StringWriter sw = new StringWriter())
            {
                set.Tables[0].WriteXml(sw);
                DBList = sw.ToString();
            }
            EventLog.Write(DBList);
            EventLog.Write("--- DBList");
            return DBList;
        }
        catch (Exception ex)
        {
            EventLog.Write("--- DBList");
            EventLog.Write(ex.Message);
            return ex.Message;
        }
    }
}
