using System;
using System.Collections.Generic;
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

namespace WPFERPQuickLauncher_Core
{
    /// <summary>
    /// Interaction logic for ERPMain.xaml
    /// </summary>
    public partial class ERPMain : Window
    {
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
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string assemblyName = string.Format("{0}\\WPFcomInventory_Core.dll", new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName);
                System.Windows.Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                {
                    Window wnd = LoadAssembly(assemblyName, "frmInvBalance");
                    wnd.Show();
                }));
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
    }
}
