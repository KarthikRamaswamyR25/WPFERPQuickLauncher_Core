using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.DirectoryServices.AccountManagement;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using WPFERPQuickLauncher_Core.Model;

namespace WPFERPQuickLauncher_Core
{
    /// <summary>
    /// Interaction logic for ERPMain.xaml
    /// </summary>
    public partial class ERPMain : Window
    {
        string strServer;
        string strSharedDll;

        public ERPMain()
        {
            InitializeComponent();
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            lblTime.Content = DateTime.Now.ToString("hh:mm:ss tt");
            lblDate.Content = DateTime.Now.ToString("dd MMM yyyy");

            SqlConnectionStringBuilder connBuilder = new SqlConnectionStringBuilder();
            connBuilder.ConnectionString = ERPClass.MyConn;
            lblDatabase.Content = connBuilder.InitialCatalog;
            lblServer.Content = connBuilder.DataSource;

            string userFullName = Environment.UserName;

           if ( connBuilder.IntegratedSecurity)
            {
                lblUser.Content = userFullName;
            }
            else
            {
                lblUser.Content = connBuilder.UserID;
            }

            lblUProfile.Content = ERPClass.g_Profile;

            //SET SERVER FOLDER AND SHAREDDLL LOCATION
            SqlConnection oConn = new SqlConnection(ERPClass.g_Conn);
            System.Data.SqlClient.SqlDataReader oDR;
            System.Data.SqlClient.SqlCommand oCom;

            oConn.Open();
            oCom = new System.Data.SqlClient.SqlCommand();
            oCom.Connection = oConn;

            oCom.CommandText = "SELECT DotNetServerName, DotNetSharedDll From ERP_Path";
            oDR = oCom.ExecuteReader();

            if (oDR.HasRows)
            {
                while (oDR.Read())
                {
                    strServer =  oDR.GetString(0);
                    strSharedDll = oDR.GetString(1);
                }
            }

            oDR.Close();

            DataTable dt = GetData("Select Distinct MnuName,MnuBaseObject from QryMenu where MnuParent = 0 AND Username ='" + ERPClass.g_Profile  + "' And MnuVisible = 1");
            IEnumerable<QryMenu> summaries = ConvertToTankReadings(dt);

            this.radTxtModule.ItemsSource = summaries;

            DataTable dt1 = GetData("Select FMnuCode, FMnuName, FModuleName, FGroupname, FSlno from UR_ERPFavorites where FUserName = '" + ERPClass.g_Profile + "' order by FMnuCode");
            IEnumerable <UR_ERPFavorites> summaries1 = ConvertToIEnumerable(dt1);
            this.radGrdFav.ItemsSource = summaries1;  
        }

        private IEnumerable<UR_ERPFavorites> ConvertToIEnumerable(DataTable dataTable)
        {
            foreach (DataRow row in dataTable.Rows)
            {
                yield return new UR_ERPFavorites
                {
                    FMnuCode = Convert.ToString(row["FMnuCode"]),
                    FMnuName = Convert.ToString(row["FMnuName"]),
                    FModuleName = Convert.ToString(row["FModuleName"]),
                    FGroupname = Convert.ToString(row["FGroupname"]),
                    FSlno = Convert.ToString(row["FSlno"]),
                };
            }

        }


        private IEnumerable<QryMenu> ConvertToTankReadings(DataTable dataTable)
        {
            foreach (DataRow row in dataTable.Rows)
            {
                yield return new QryMenu
                {
                    MnuName = Convert.ToString(row["MnuName"]),
                    //MnuBaseObject = Convert.ToString(row["MnuBaseObject"]),
                    //MnuBaseObject = Convert.ToString(row["MnuBaseObject"]),
                };
            }

        }

        private static DataTable GetData(string query)
        {

            string strConnString = ERPClass.g_Conn;

            using (SqlConnection con = new SqlConnection(strConnString))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = query;
                    using (SqlDataAdapter sda = new SqlDataAdapter())
                    {
                        cmd.Connection = con;
                        sda.SelectCommand = cmd;
                        using (DataSet ds = new DataSet())
                        {
                            DataTable dt = new DataTable();
                            sda.Fill(dt);
                            return dt;
                        }
                    }
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }



        private void cmdInvBalance_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string strLoc = "\\\\" + strServer + "\\" + strSharedDll + "\\";
                //string assemblyName = string.Format("{0}\\WPFcomInventory_Core.dll", new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName);
                string assemblyName = string.Format(strLoc + "\\WPFcomInventory_Core.dll", new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName);

                bool bAllow = IsUserAuthorized(lblUProfile.Content.ToString(), "592");

                if (bAllow == true)
                {
                    System.Windows.Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                    {
                        Window wnd = LoadAssembly(assemblyName, "frmInvBalance");
                        wnd.Show();
                    }));
                }
                else
                {
                    MessageBoxResult resultc = MessageBox.Show("Sorry!.... Menu Code: 592 ....  Not Authorized ....");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(string.Format("Failed to load window from{0} - {1}", "OtherWindow", ex.Message));
                throw new Exception(String.Format("Failed to load window from{0} - {1}", "OtherWindow", ex.Message), ex);
            }
        }


    private static Window LoadAssembly(String assemblyName, String typeName)
        {
            try
            {
                Assembly assemblyInstance = Assembly.LoadFrom(assemblyName);
                foreach (Type t in assemblyInstance.GetTypes().Where(t => String.Equals(t.Name, typeName, StringComparison.OrdinalIgnoreCase)))
                {
                    var wnd = assemblyInstance.CreateInstance(t.FullName) as Window;
                    return wnd;
                }
                throw new Exception("Unable to load external window");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(string.Format("Failed to load window from{0}{1}", assemblyName, ex.Message));
                throw new Exception(string.Format("Failed to load external window{0}", assemblyName), ex);
            }
        }

        private bool IsUserAuthorized(string strUserName, string strMenuCode)
        {
            //SET SERVER FOLDER AND SHAREDDLL LOCATION
            SqlConnection oConn = new SqlConnection(ERPClass.g_Conn);
            System.Data.SqlClient.SqlDataReader oDR;
            System.Data.SqlClient.SqlCommand oCom;

            oConn.Open();
            oCom = new System.Data.SqlClient.SqlCommand();
            oCom.Connection = oConn;

            oCom.CommandText = "select * from QryMenu where MnuCode='" + strMenuCode + "' AND UserName='" + strUserName.Replace(@"\\", @"\") + "'";
            oDR = oCom.ExecuteReader();

            if (oDR.HasRows)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void cmdOpen_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SqlConnection oConn = new SqlConnection(ERPClass.g_Conn);
                System.Data.SqlClient.SqlDataReader oDR;
                System.Data.SqlClient.SqlCommand oCom;

                string strModuleName;
                string strFormName;

                strModuleName = "";
                strFormName = "";

                if (txtMenuCode.Text != "") 
                {
                    bool bAllow = IsUserAuthorized(lblUProfile.Content.ToString(), txtMenuCode.Text);

                    if (bAllow == true)
                    {

                        oConn.Open();
                        oCom = new System.Data.SqlClient.SqlCommand();
                        oCom.Connection = oConn;

                        oCom.CommandText = "select DotNetModuleName, DotNetFormName from QryMenu where MnuCode='" + txtMenuCode.Text + "' AND UserName='" + lblUProfile.Content.ToString().Replace(@"\\", @"\") + "'";
                        oDR = oCom.ExecuteReader();

                        if (oDR.HasRows)
                        {
                            while (oDR.Read())
                            {
                                strModuleName = oDR.GetString(0);
                                strFormName = oDR.GetString(1);
                            }
                        }

                        string strLoc = "\\\\" + strServer + "\\" + strSharedDll + "\\";
                        string assemblyName = string.Format(strLoc + "\\" + strModuleName + ".dll", new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName);

                        System.Windows.Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                        {
                            Window wnd = LoadAssembly(assemblyName, strFormName);
                            wnd.Show();
                        }));
                    }
                    else
                    {
                        MessageBoxResult resultc = MessageBox.Show("Sorry!.... Menu Code: 592 ....  Not Authorized ....");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(string.Format("Failed to load window from{0} - {1}", "OtherWindow", ex.Message));
                throw new Exception(String.Format("Failed to load window from{0} - {1}", "OtherWindow", ex.Message), ex);
            }
        }
    }
}
