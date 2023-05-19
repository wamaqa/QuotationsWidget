using Microsoft.UI.Xaml.Controls;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace QuotationsWidgetProvider
{


    public class Config
    {
        public List<Market> Market { get; set; }
        public string Version { set; get; }
    }

    public class Market
    {
        public string Name { set; get; }
        public List<Scoket> List { set; get; }
    }

    public class Scoket
    {
        public string Code { set; get; }
        public string Name { set; get; }
        public string Price { set; get; }
        public string Rise { set; get; }
        /// <summary>
        // 换手率
        /// </summary>
        public double turnover_rate { set; get; }
        /// <summary>
        /// 成交量
        /// </summary>
        public double amount { set; get; }

        public string Image { set; get; }
    }



    public partial class DataService
    {

        const string baseUrl = "https://stock.xueqiu.com/v5/stock/realtime/quotec.json";//?symbol=SZ000001,SH601001";

        public Config? Config { get; }
        public string Layout { get; private set; }

        private HttpClient _httpClient;

        public static void GetAQuotation(Scoket scoket)
        {
            string url = $"{baseUrl}?symbol=SH{scoket.Code}";
        }

        public Dictionary<string, Action<Scoket>> actions = new Dictionary<string, Action<Scoket>>()
        {
            {"a股", GetAQuotation }
        };
        public DataService()
        {
            var list = System.IO.File.ReadAllText("C:\\ProgramData\\QuotationsWidget\\config.json");
            Config = JsonConvert.DeserializeObject<Config>(list);
            _httpClient = new HttpClient();
            initHttpHeader();
            initLayout();
        }

        private void initHttpHeader()
        {
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("accept", "text/html,application/xhtml+xml,application/json,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
            //_httpClient.DefaultRequestHeaders.Add("Content-Type", "application/json");
            _httpClient.DefaultRequestHeaders.Add("accept-encoding", "gzip, deflate, br");
            _httpClient.DefaultRequestHeaders.Add("accept-language", "zh-CN,zh;q=0.9,en;q=0.8,en-GB;q=0.7,en-US;q=0.6");
            _httpClient.DefaultRequestHeaders.Add("cache-control", "max-age=0");
            //_httpClient.DefaultRequestHeaders.Add("cookie", "device_id=7f671dbe5a682ae45d64f14cf32dff2f; Hm_lvt_1db88642e346389874251b5a1eded6e3=1680751377,1681708886");
            _httpClient.DefaultRequestHeaders.Add("sec-ch-ua", "\"Microsoft Edge\";v=\"113\", \"Chromium\";v=\"113\", \"Not-A.Brand\";v=\"24\"");
            _httpClient.DefaultRequestHeaders.Add("sec-ch-ua-mobile", "?0");
            _httpClient.DefaultRequestHeaders.Add("sec-ch-ua-platform", "\"Windows\"");
            _httpClient.DefaultRequestHeaders.Add("sec-fetch-dest", "document");
            _httpClient.DefaultRequestHeaders.Add("sec-fetch-mode", "navigate");
            _httpClient.DefaultRequestHeaders.Add("sec-fetch-site", "none");
            _httpClient.DefaultRequestHeaders.Add("sec-fetch-user", "?1");
            _httpClient.DefaultRequestHeaders.Add("upgrade-insecure-requests", "1");
            _httpClient.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/113.0.0.0 Safari/537.36 Edg/113.0.1774.42");

        }

        private void initLayout()
        {
            StringBuilder stringLayerBuilder = new StringBuilder();
            stringLayerBuilder.Append("\"rows\": [");
            string positonString = "\"rows\": []";
            string layer = QuotationsWidgetProvider.Resource.mainList;
            var count = Config?.Market.Sum(x => x.List.Count) ?? 0;
            for (int i = 1; i <= count; i++)
            {
                string row = QuotationsWidgetProvider.Resource.row.ToString()
                    .Replace("${$root.row1", $"${{$root.row{i}");
                stringLayerBuilder.Append(row);
                stringLayerBuilder.Append(',');
            }
            stringLayerBuilder.Remove(stringLayerBuilder.Length - 1, 1);
            stringLayerBuilder.Append("]");
            Layout = layer.Replace(positonString, stringLayerBuilder.ToString());
        }

        public string RequsetQuotation()
        {
            Config?.Market.ForEach(market =>
            {
                if (actions.ContainsKey(market.Name))
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    market.List.ForEach(item =>
                    {
                        stringBuilder.Append(item.Code);
                        stringBuilder.Append(",");
                    });
                    stringBuilder.Remove(stringBuilder.Length - 1, 1);
                                   var response = _httpClient.GetAsync(new Uri($"{baseUrl}?symbol={stringBuilder.ToString()}"));
                    response.Wait();
                    //var response = task.;
                    if (response.Result.IsSuccessStatusCode)
                    {
                        var stream = response.Result.Content.ReadAsStream();
                        GZipStream gZipStream = new GZipStream(stream, CompressionMode.Decompress);
                        StreamReader myStreamReader = new StreamReader(gZipStream, Encoding.UTF8);
                        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                        var retString = myStreamReader.ReadToEnd();
                        myStreamReader.Close();
                        gZipStream.Close();
                        stream.Close();

                        dynamic result = JsonConvert.DeserializeObject(retString);
                        market.List.ForEach(item =>
                        {
                            for (int i = result.data.Count - 1; i >= 0; i--)
                            {
                                if(result.data[i].symbol.Value == item.Code)
                                {
                                    item.Price = result.data[i].current.Value.ToString();
                                    item.Rise = $"{result.data[i].percent.Value}";
                                    item.amount = result.data[i].amount.Value ?? 0;
                                    item.turnover_rate = result.data[i].turnover_rate.Value ?? 0;
                                }
                            }
                        });
                    }
                }
            });
            StringBuilder stringDataBuilder     = new StringBuilder();
            stringDataBuilder.Append("{");
            int index = 1;
            Config?.Market.ForEach(item =>
            {
                item.List.ForEach(scoket =>
                {
                    var amount = scoket.amount> 10000 * 10000 ? $"{(scoket.amount / 10000 / 10000).ToString("f4")}亿" : $"{(scoket.amount / 10000).ToString("f4")}万";
                    stringDataBuilder.Append($"\"row{index}\":");
                    string attention = float.Parse(scoket.Rise ?? "") > 0 ? "Attention" : "Good";
                    string url = $"http://localhost:7090/{scoket.Code}/time.png";
                    string socketJson = $"{{\"name\":\"{scoket.Name}\"," +
                    $"\"code\":\"{scoket.Code}\"," +
                    $"\"price\":\"{scoket.Price}\"," +
                    $"\"rise\":\"{scoket.Rise}%\"," +
                    $"\"url\":\"{scoket.Image}\"," +
                    $"\"turnover_rate\":\"换{scoket.turnover_rate}%\"," +
                    $"\"amount\":\"{amount}\"," +
                    $"\"attention\":\"{attention}\" }},";
                    stringDataBuilder.Append(socketJson);
                    index++;
                });
            });
            stringDataBuilder.Remove(stringDataBuilder.Length - 1, 1);
            stringDataBuilder.Append("}");
            return stringDataBuilder.ToString();
        }

    }
}
