using IdentityModel.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PredictionEngine
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region public declarations

        string dbConnstring = @"data source=c:\Dictionary.db; Version=3;"; //custom path to the db file
        List<Words> lstWords = new List<Words>();

        #endregion
        public MainWindow()
        {
            InitializeComponent();
            InitializeDictionary();
        }


        #region Dictionary Service


        public void InitializeDictionary()
        {
            SQLiteConnection sqliteCon = new SQLiteConnection(dbConnstring);

            try
            {
                sqliteCon.Open();
                string getData = "select * from Words"; //query to fetch the data
                SQLiteCommand sqlcmd = new SQLiteCommand(getData, sqliteCon);
                sqlcmd.ExecuteNonQuery();
                SQLiteDataReader dr = sqlcmd.ExecuteReader();

                while (dr.Read())
                {
                    Words obj = new Words();
                    obj.Id = (long)dr[0];
                    obj.Value = (string)dr[1];

                    lstWords.Add(obj);
                }
            }
            catch (Exception ex)
            {
                throw ex; //can refine the exception handling
            }
        }


        #endregion


        #region Webservice

        public static async Task<List<string>> GetInfo(string input)
        {
            List<string> lstResponse = new List<string>();

            using (var client = new HttpClient())
            {
                string authorization = "MjAyMS0wNi0xMQ==.Y2thbnRoLnByb2plY3RzQGdtYWlsLmNvbQ==.ZTk5NjMxZmE4OGNjNTgwMmJmMGY0ZGJhMzY2ZGQ3YmU=";
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authorization);
                client.BaseAddress = new Uri("https://services.lingapps.dk/misc/getPredictions");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                HttpResponseMessage response = new HttpResponseMessage();

                response = await client.GetAsync("https://services.lingapps.dk/misc/getPredictions?locale=en-GB&text=" + input).ConfigureAwait(false);

                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    lstResponse = JsonConvert.DeserializeObject<List<string>>(jsonString);
                }
            }

            return lstResponse;

        }
        #endregion


        private async void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            lstPrediction.Items.Clear();

            if (txtInput.Text.Trim() != "")
            {

                var results = from result in lstWords
                              where result.Value.StartsWith(txtInput.Text.Trim().ToLower())
                              select result;

                foreach (var result in results)
                {
                    lstPrediction.Items.Add(result.Value.ToString());
                    lstPrediction.Visibility = Visibility.Visible;
                }

                ///For Webservice
                lstWebService.Items.Clear();
                List<string> lstResult = new List<string>();
                lstResult = await GetInfo(txtInput.Text.Trim());

                foreach (var result in lstResult)
                {
                    lstWebService.Items.Add(result.ToString());
                    lstWebService.Visibility = Visibility.Visible;
                }

            }

            if (lstPrediction.Items.IsEmpty || lstPrediction.Items.Count == 119)
            {
                lstPrediction.Visibility = Visibility.Collapsed;
                if (lstPrediction.Items.Count == 119) lstPrediction.Items.Clear();
            }
 
            if (lstWebService.Items.IsEmpty || lstWebService.Items.Count == 119)
            {
                lstWebService.Visibility = Visibility.Collapsed;
                if (lstWebService.Items.Count == 119) lstWebService.Items.Clear();
            }

        }


        private void lstPrediction_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstPrediction.SelectedItem != null)
            {
                txtInput.Text = lstPrediction.SelectedItem.ToString();
            }
        }

        private void lstWebService_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstWebService.SelectedItem != null)
            {
                txtInput.Text = lstWebService.SelectedItem.ToString();
            }
        }
    }
}
