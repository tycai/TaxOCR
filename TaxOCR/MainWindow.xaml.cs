using System;
using System.Collections.Generic;
using System.Linq;
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
// using System;
using System.IO;
using System.Net;
// using System.Text;
using System.Web;
using Microsoft.Win32;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace TaxOCR
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        // 返回token示例
        public static string TOKEN = "24.df5fa4e6038541895e226f641640fe04.2592000.1625990364.282335-24326267";
        // 百度云中开通对应服务应用的 API Key 建议开通应用的时候多选服务
        private static string clientId = "IsG8QXesl4n1A8YDni13TieV";
        // 百度云中开通对应服务应用的 Secret Key
        private static String clientSecret = "HMPhBwqGcuKd3lFI9oMXBOCqDwIi3wpp";

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Ofd_Clicked(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            //fileDialog.DefaultExt = ".txt"; // Required file extension
            fileDialog.Title = "请选择要识别的发票的图片";
            fileDialog.Filter = "图片文件(*.jpg *.jpeg *.bmp *.png)|*.jpg;*.jpeg;*.bmp;*.png;"; // Optional file extensions 

            if ( fileDialog.ShowDialog() == true)
            {
                url_tb.Text = fileDialog.FileName;
            }
            else
            {
                url_tb.Text = "";
            }


        }
        /*
        */
        // 增值税发票识别
        public static string VatInvoice(string loc)
        {
            // string token = "[调用鉴权接口获取的token]";
            string host = "https://aip.baidubce.com/rest/2.0/ocr/v1/vat_invoice?access_token=" + TOKEN;
            Encoding encoding = Encoding.Default;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(host);
            request.Method = "post";
            request.KeepAlive = true;

            // 文件类别
            string ftype = loc.Substring(loc.LastIndexOf(".") + 1);

            // 图片的base64编码
            string base64 = GetFileBase64(loc);
            string base64pdf = Regex.Replace(base64, @"^data:application/[a-zA-Z]+;base64,", "");

            string str = "image=" + HttpUtility.UrlEncode(base64);
            byte[] buffer = encoding.GetBytes(str);
            request.ContentLength = buffer.Length;
            request.GetRequestStream().Write(buffer, 0, buffer.Length);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
            string result = reader.ReadToEnd();
            Console.WriteLine("增值税发票识别:");
            Console.WriteLine(result);
            return result;
        }

        public static string QuotaInvoice(string loc)
        {
            // string token = "[调用鉴权接口获取的token]";
            string host = "https://aip.baidubce.com/rest/2.0/ocr/v1/quota_invoice?access_token=" + TOKEN;
            Encoding encoding = Encoding.Default;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(host);
            request.Method = "post";
            request.KeepAlive = true;

            // 文件类别
            string ftype = loc.Substring(loc.LastIndexOf(".")+1);

            // 图片的base64编码
            string base64 = GetFileBase64(loc);
            string base64pdf = Regex.Replace(base64, @"^data:application/[a-zA-Z]+;base64,", "");


            String str = "image=" + HttpUtility.UrlEncode(base64);
            byte[] buffer = encoding.GetBytes(str);
            request.ContentLength = buffer.Length;
            request.GetRequestStream().Write(buffer, 0, buffer.Length);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
            string result = reader.ReadToEnd();
            Console.WriteLine("定额发票识别:");
            Console.WriteLine(result);
            return result;
        }

        public static string GetFileBase64(String fileName)
        {
            FileStream filestream = new FileStream(fileName, FileMode.Open);
            byte[] arr = new byte[filestream.Length];
            filestream.Read(arr, 0, (int)filestream.Length);
            string baser64 = Convert.ToBase64String(arr);
            filestream.Close();
            return baser64;

        }

        // 调用getAccessToken()获取的 access_token建议根据expires_in 时间 设置缓存
        public static String GetAccessToken()
        {
            String authHost = "https://aip.baidubce.com/oauth/2.0/token";
            HttpClient client = new HttpClient();
            List<KeyValuePair<String, String>> paraList = new List<KeyValuePair<string, string>>();
            paraList.Add(new KeyValuePair<string, string>("grant_type", "client_credentials"));
            paraList.Add(new KeyValuePair<string, string>("client_id", clientId));
            paraList.Add(new KeyValuePair<string, string>("client_secret", clientSecret));

            HttpResponseMessage response = client.PostAsync(authHost, new FormUrlEncodedContent(paraList)).Result;
            String result = response.Content.ReadAsStringAsync().Result;
            Console.WriteLine(result);
            return result;

        }

        private void Rec_Clicked(object sender, RoutedEventArgs e)
        {
            if (url_tb.Text != "")
            {
                //String result = GetAccessToken();
                //JObject jo = (JObject)JsonConvert.DeserializeObject(result);
                //TOKEN = jo["access_token"].ToString();
                //tbk.Text = QuotaInvoice(url_tb.Text);
                // tbk.Text = VatInvoice(url_tb.Text);

                // 增值税发票：
                string result = VatInvoice(url_tb.Text);
                JObject jo = (JObject)JsonConvert.DeserializeObject(result);
                string inv_num = jo["words_result"]["InvoiceNum"].ToString();
                string inv_type = jo["words_result"]["InvoiceType"].ToString();
                string inv_date = jo["words_result"]["InvoiceDate"].ToString();
                string purchaser = jo["words_result"]["PurchaserName"].ToString();
                string comm_name = jo["words_result"]["CommodityName"]["word"].ToString();
                string amount = jo["words_result"]["AmountInFiguers"].ToString();
            }
            else
            {
                /*
                String result = "{\"refresh_token\":\"25.643d0738f938d4d288625cd8f203cbf0.315360000.1938751582.282335-24326267\",\"expires_in\":2592000}";
                JObject jo = (JObject)JsonConvert.DeserializeObject(result);
                string token = jo["refresh_token"].ToString();
                string expires = jo["expires_in"].ToString();
                tbk.Text = token;
                */

            }

        }
    }
}



namespace com.baidu.ai
{
    public class QuotaInvoice
    {
        // 定额发票识别
        public static string quotaInvoice()
        {
            string token = "[调用鉴权接口获取的token]";
            string host = "https://aip.baidubce.com/rest/2.0/ocr/v1/quota_invoice?access_token=" + token;
            Encoding encoding = Encoding.Default;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(host);
            request.Method = "post";
            request.KeepAlive = true;
            // 图片的base64编码
            string base64 = getFileBase64("[本地图片文件]");
            String str = "image=" + HttpUtility.UrlEncode(base64);
            byte[] buffer = encoding.GetBytes(str);
            request.ContentLength = buffer.Length;
            request.GetRequestStream().Write(buffer, 0, buffer.Length);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.Default);
            string result = reader.ReadToEnd();
            Console.WriteLine("定额发票识别:");
            Console.WriteLine(result);
            return result;
        }

        public static String getFileBase64(String fileName)
        {
            FileStream filestream = new FileStream(fileName, FileMode.Open);
            byte[] arr = new byte[filestream.Length];
            filestream.Read(arr, 0, (int)filestream.Length);
            string baser64 = Convert.ToBase64String(arr);
            filestream.Close();
            return baser64;
            
        }
    }

    public static class AccessToken

    {
        // 调用getAccessToken()获取的 access_token建议根据expires_in 时间 设置缓存
        // 返回token示例
        // public static String TOKEN = "24.adda70c11b9786206253ddb70affdc46.2592000.1493524354.282335-1234567";

        // 百度云中开通对应服务应用的 API Key 建议开通应用的时候多选服务
        private static String clientId = "百度云应用的AK";
        // 百度云中开通对应服务应用的 Secret Key
        private static String clientSecret = "百度云应用的SK";

        public static String getAccessToken()
        {
            String authHost = "https://aip.baidubce.com/oauth/2.0/token";
            HttpClient client = new HttpClient();
            List<KeyValuePair<String, String>> paraList = new List<KeyValuePair<string, string>>();
            paraList.Add(new KeyValuePair<string, string>("grant_type", "client_credentials"));
            paraList.Add(new KeyValuePair<string, string>("client_id", clientId));
            paraList.Add(new KeyValuePair<string, string>("client_secret", clientSecret));

            HttpResponseMessage response = client.PostAsync(authHost, new FormUrlEncodedContent(paraList)).Result;
            String result = response.Content.ReadAsStringAsync().Result;
            Console.WriteLine(result);
            return result;
        }
    }
}