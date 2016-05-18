using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class TT : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {

    }
    protected void Button1_Click(object sender, EventArgs e)
    {
        int intActionType = Convert.ToInt32(txtActionType.Text) ;
        string strColumns = txtColumns.Text ;
        string[] arrColumns = strColumns.Split(new string[]{","},StringSplitOptions.RemoveEmptyEntries) ;
        string strValuess = txtValues.Text ;
        string[] arrValuess = strValuess.Split(new string[]{","},StringSplitOptions.RemoveEmptyEntries) ;
        string strTargetTable = txtTable.Text ;
        string strWhere = txtWhere.Text;
        string strSortBy = txtSortBy.Text;
        Boolean primary = false;
        string strDbConnection = "InformationRetrieval";

        Service s = new Service();
        Label1.Text = s.DBService_Basic_Operate(intActionType, arrColumns, arrValuess, strTargetTable, strWhere, strSortBy, strDbConnection);
        //Label1.Text = s.PerformDatabaseAction3(intActionType, arrColumns, arrValuess, strTargetTable, strWhere, strSortBy, primary);
        lbResult.Text = s.DBService_DBList();
    }
}