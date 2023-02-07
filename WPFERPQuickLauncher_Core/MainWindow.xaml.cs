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

namespace WPFERPQuickLauncher_Core
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
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

            if (ERPClass.strParamModule != null & ERPClass.strParamModule != "")
            {
                if (ERPClass.strParamForm != null & ERPClass.strParamForm != "")
                {

                        string assemblyName = string.Format("{0}\\" + ERPClass.strParamModule + ".dll", new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName);
                        System.Windows.Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                        {
                            Window wnd = LoadAssembly(assemblyName, ERPClass.strParamForm);
                            wnd.Show();

                            this.Close();
                        }));
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
                SqlConnection conn = new SqlConnection();
                if (chkDefault.IsChecked == (bool?)true)
                {
                    // TRUSTED CONNECTION
                    conn.ConnectionString =
                      "Data Source=" + txtServer.Text + ";" +
                      "Initial Catalog=" + txtDatabase.Text + ";" +
                      "Integrated Security=SSPI;";
                    conn.Open();

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

                    //MessageBoxResult resultc = MessageBox.Show(conn.ConnectionString);
                }
                MessageBoxResult result1 = MessageBox.Show("Login Success");
                ERPClass.MyConn = conn.ConnectionString;

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
    }
}
