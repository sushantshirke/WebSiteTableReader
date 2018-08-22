using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Text;
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

namespace WpfApp4
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        DataTable dataTable = new DataTable();
        public MainWindow()
        {
            InitializeComponent();
           
        }

        public string baseURL { get; set; }

        public string dateString { get; set; }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                dataTable = new DataTable();
                WebClient client = new WebClient();
                // String htmlCode = client.DownloadString("https://www.google.com/url?hl=en&q=https://www.nseindia.com/live_market/dynaContent/live_watch/option_chain/optionKeys.jsp?symbolCode%3D-9999%26symbol%3DBANKNIFTY%26symbol%3DBANKNIFTY%26instrument%3DOPTIDX%26date%3D-%26segmentLink%3D17%26segmentLink%3D17&source=gmail&ust=1535004180169000&usg=AFQjCNF6l9oo7vPCyhmi3egy6qyVKSY55Q");

                //String htmlCode = client.DownloadString("https://www.nseindia.com/live_market/dynaContent/live_watch/option_chain/optionKeys.jsp?segmentLink=17&instrument=OPTIDX&symbol=BANKNIFTY&date=30AUG2018");

                string htmlCode = client.DownloadString(baseURL + dateValue.Text.ToString());







                StringBuilder stringBuilder = new StringBuilder();
                string a = htmlCode.Substring(htmlCode.IndexOf("<table"), htmlCode.LastIndexOf("</table") - htmlCode.IndexOf("<table"));

                string[] seperator = new string[] { "</table" };
                string[] tableArray = a.Split(seperator, StringSplitOptions.RemoveEmptyEntries); // read array of 2

                string[] tableRows = tableArray[2].Split(new string[] { "</tr" }, StringSplitOptions.RemoveEmptyEntries); //array of 1 for header

                string[] tableHeader = tableRows[1].Split(new string[] { "title" }, StringSplitOptions.RemoveEmptyEntries);

                string columPrefix = "Call_";

                // foreach (string tablcolumn in tableHeader)
                {

                    for (int i = 1; i < tableHeader.Length; i++)



                    //if (!tablcolumn.Contains("<!--"))
                    {
                        string tablcolumn = string.Empty;
                        tablcolumn = tableHeader[i];

                        string columnName = tablcolumn.Substring(2, tablcolumn.IndexOf(">") - 3).Replace(" ", "");

                        if (columnName.Contains("StrikePrice"))
                        {
                            columPrefix = "";
                        }
                        DataColumn dataColumn = new DataColumn(columPrefix + columnName);

                        dataTable.Columns.Add(dataColumn);

                        if (columnName.Contains("StrikePrice"))
                        {
                            columPrefix = "Put_";
                        }
                    }
                }


                for (int i = 2; i < tableRows.Length; i++)
                {
                    string[] data = tableRows[i].Split(new string[] { "</td" }, StringSplitOptions.RemoveEmptyEntries);

                    DataRow dr = dataTable.NewRow();


                    if (data[0].Contains("Total"))
                    {

                        Func<string, string> getTotal = str =>
                        {

                            string[] totalarr = str.Split(new string[] { "</b" }, StringSplitOptions.RemoveEmptyEntries);
                            return totalarr[0].Substring(totalarr[0].LastIndexOf(">") + 1);
                        };

                        double callUI = 0;
                        double callOIchange = 0;
                        double putOI = 0;
                        double putOIcahnge = 0;


                        double.TryParse(getTotal(data[1]), out callUI);  //call OI Total
                        dr[1] = callUI;

                        double.TryParse(getTotal(data[3]), out callOIchange); //call change in oi total
                        dr[3] = callOIchange;

                        double.TryParse(getTotal(data[5]), out putOI);
                        dr[19] = putOI;

                        double.TryParse(getTotal(data[7]), out putOIcahnge);
                        dr[21] = putOIcahnge;

                        dataTable.Rows.Add(dr);


                        break; ;
                    }


                    int columnIndex = 0;

                    for (int j = 2; j < dataTable.Columns.Count + 1; j++)
                    {
                        string columnWiseData = string.Empty;

                        //Last row total

                        if (data[j].Contains("href"))
                        {
                            if (data[j].Contains("</b>"))
                            {
                                string[] insideAnchor = data[j].Split(new string[] { "</b" }, StringSplitOptions.RemoveEmptyEntries);
                                columnWiseData = insideAnchor[0].Substring(insideAnchor[0].LastIndexOf(">") + 1);
                            }
                            else
                            {
                                string[] insideAnchor = data[j].Split(new string[] { "</a" }, StringSplitOptions.RemoveEmptyEntries);
                                columnWiseData = insideAnchor[0].Substring(insideAnchor[0].LastIndexOf(">") + 1);
                            }


                            //columnWiseData = data[j].Substring(data[j].LastIndexOf(">") + 1);
                        }
                        else
                        {

                            columnWiseData = data[j].Substring(data[j].LastIndexOf(">") + 1);
                        }

                        double intcolumnWiseData = 0;
                        double.TryParse(columnWiseData, out intcolumnWiseData);
                        dr[columnIndex++] = intcolumnWiseData;
                    }
                    dataTable.Rows.Add(dr);

                }

                dataGrid.DataContext = dataTable.DefaultView;
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.StackTrace);
            }

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // txtBaseUrl.Text = this.baseURL;

            txtBaseUrl.Text= "https://www.nseindia.com/live_market/dynaContent/live_watch/option_chain/optionKeys.jsp?segmentLink=17&instrument=OPTIDX&symbol=BANKNIFTY&date=";
            baseURL = "https://www.nseindia.com/live_market/dynaContent/live_watch/option_chain/optionKeys.jsp?segmentLink=17&instrument=OPTIDX&symbol=BANKNIFTY&date=";
        }


        public  string DataTableToCSV( DataTable datatable, char seperator=',')
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < datatable.Columns.Count; i++)
            {
                sb.Append(datatable.Columns[i]);
                if (i < datatable.Columns.Count - 1)
                    sb.Append(seperator);
            }
            sb.AppendLine();
            foreach (DataRow dr in datatable.Rows)
            {
                for (int i = 0; i < datatable.Columns.Count; i++)
                {
                    sb.Append(dr[i].ToString());

                    if (i < datatable.Columns.Count - 1)
                        sb.Append(seperator);
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }

        private void Export_click(object sender, RoutedEventArgs e)
        {
            try
            {

            
            string sbFile = string.Empty;
              sbFile =   DataTableToCSV(dataTable, ',');

            string appPath = Environment.CurrentDirectory + @"\" + dateValue.Text.ToString() + ".csv";

            System.IO.File.WriteAllText(appPath, sbFile);

            MessageBox.Show("File Exported to :" + appPath);
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.StackTrace);
            }

        }

        
    }
}
