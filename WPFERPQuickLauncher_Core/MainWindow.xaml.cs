using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
//using WPFcomInventory_Core;

namespace WPFERPQuickLauncher_Core
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string strServer;
        string strSharedDll;

        public MainWindow()
        {
            try
            {
                InitializeComponent();
                WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                Application.Current.MainWindow.Height = 225;
                this.chkDefault.IsChecked = true;

                txtServer.Text = "172.17.71.30";
                txtDatabase.Text = "URDB";

                CreateConn();
                GetLocDetails();

                if (ERPClass.strConn != null & ERPClass.strConn != "") 
                {
                    string userFullName = Environment.UserName;
                    lblUser.Content = userFullName;
                    ERPClass.g_Profile = GetProfileName(userFullName);

                    ERPClass.g_Conn = ERPClass.strConn;
                    ERPClass.MyConn = ERPClass.strConn;
                }

                if (ERPClass.strParamModule != null & ERPClass.strParamModule != "")
                {
                    if (ERPClass.strParamForm != null & ERPClass.strParamForm != "")
                    {

                        //string assemblyName = string.Format("{0}\\" + ERPClass.strParamModule + ".dll", new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName);
                        string strLoc = "\\\\" + strServer + "\\" + strSharedDll + "\\";
                        string assemblyName = string.Format(strLoc + "\\" + ERPClass.strParamModule + ".dll", new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName);

                        //MessageBoxResult result1 = MessageBox.Show(ERPClass.strParamModule);

                        bool bAllow = IsUserAuthorized((ERPClass.g_Profile).ToString(), ERPClass.strMenuCode);

                        if (bAllow == true)
                        {

                            System.Windows.Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                            {
                                Window wnd = LoadAssembly(assemblyName, ERPClass.strParamForm);
                                wnd.Show();

                                this.Close();
                            }));
                        }
                        else
                        {
                            MessageBoxResult resultc = MessageBox.Show("Sorry!.... Menu Code: " + ERPClass.strMenuCode + " ....  Not Authorized for " + (ERPClass.g_Profile).ToString() + "....");
                            this.Close();
                        }
                    }
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

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.Height = 225;
        }

        private void CheckBox_UnChecked(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.Height = 300;
        }

        private void txUserName_TextChanged(object sender, TextChangedEventArgs e)
        {
        }

        private void cmdCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void cmdOK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CreateConn();

                ERPMain Win2 = new ERPMain();
                Win2.Show();

                this.Close();
            }
            catch (System.IO.IOException ex)
            {
                MessageBoxResult result = MessageBox.Show(ex.Message);
            }
            finally
            {
            }
        }

        private void GetLocDetails()
        {
            //SET SERVER FOLDER AND SHAREDDLL LOCATION
            SqlConnection oConn = new SqlConnection(ERPClass.MyConn);
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
                    strServer = oDR.GetString(0);
                    strSharedDll = oDR.GetString(1);
                }
            }
        }

        private void CreateConn()
        {
            try
            {
                string userFullName = Environment.UserName;
                //string userFullName = "Praveen.Gladwin";

                SqlConnection conn = new SqlConnection();
                if (chkDefault.IsChecked == (bool?)true)
                {
                    // TRUSTED CONNECTION
                    conn.ConnectionString =
                      "Data Source=" + txtServer.Text + ";" +
                      "Initial Catalog=" + txtDatabase.Text + ";" +
                      "Integrated Security=SSPI;";
                    conn.Open();

                    ERPClass.g_Conn = "Data Source=" + txtServer.Text + ";" +
                      "Initial Catalog=" + txtDatabase.Text + ";" +
                      "Integrated Security=SSPI;";

                    lblUser.Content = userFullName;
                    ERPClass.g_Profile = GetProfileName(userFullName);

                    //MessageBoxResult resultc = MessageBox.Show(conn.ConnectionString);
                }
                else
                {
                    //USERNAME PASSWORD
                    conn.ConnectionString =
                      "Data Source=" + txtServer.Text + ";" +
                      "Initial Catalog=" + txtDatabase.Text + ";" +
                      "User id=" + txtUserName.Text + ";" +
                      "Password=" + txtPassword.Password + ";";
                    conn.Open();

                    ERPClass.g_Conn = "Data Source=" + txtServer.Text + ";" +
                      "Initial Catalog=" + txtDatabase.Text + ";" +
                      "User id=" + txtUserName.Text + ";" +
                      "Password=" + txtPassword.Password + ";";

                    lblUser.Content = txtUserName.Text;
                    ERPClass.g_Profile = txtUserName.Text;

                    //MessageBoxResult resultc = MessageBox.Show(conn.ConnectionString);
                }
                //MessageBoxResult result1 = MessageBox.Show("Login Success");
                ERPClass.MyConn = conn.ConnectionString;

                
            }
            catch (System.IO.IOException ex)
            {
                MessageBoxResult result = MessageBox.Show(ex.Message);
            }
            finally
            {
            }
        }

        private string GetProfileName(string strUserName)
        {
            string strProfile;
            string strProfileTmp;
            
            strProfile = "";
            strProfileTmp = "";

            //SET SERVER FOLDER AND SHAREDDLL LOCATION
            SqlConnection oConn = new SqlConnection(ERPClass.g_Conn);
            System.Data.SqlClient.SqlDataReader oDR;
            System.Data.SqlClient.SqlCommand oCom;

            oConn.Open();
            oCom = new System.Data.SqlClient.SqlCommand();
            oCom.Connection = oConn;

            oCom.CommandText = "select UserName from DomainLoginMap Where NewUserName ='" + strUserName + "'";
            oDR = oCom.ExecuteReader();

            if (oDR.HasRows)
            {
                while (oDR.Read())
                {
                    strProfileTmp = oDR.GetString(0);
                    strProfile = strProfileTmp.Replace(@"\\", @"\");
                }
            }
            else
            {
                strProfile= strUserName;
            }

            return strProfile;
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


    }
}
