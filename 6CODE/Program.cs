using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace _6CODE
{

    class Program
    {

        static void Main(string[] args)
        {
            //Employee emp = null;
            //var r = emp?.EmpName ?? "NoName";
            //Console.WriteLine(r);
            string str = System.Environment.CurrentDirectory;
            string[] lines = System.IO.File.ReadAllLines(str + @"\MAP.txt");
            List<string> listMAP = new List<string>();
            // string[] lines = { "24.84579622,120.927361249923", "24.84607369,120.926830172538" };
            listMAP.Add(lines[0]);
            for (int i = 0; i < lines.Count() - 1; i++)
            {
                var v1list = lines[i].Split(',');
                var v2list = lines[i + 1].Split(',');
                var lat1 = 0.0;
                var lng1 = 0.0;
                var lat2 = 0.0;
                var lng2 = 0.0;
                double.TryParse(v1list[0], out lat1);
                double.TryParse(v1list[1], out lng1);
                double.TryParse(v2list[0], out lat2);
                double.TryParse(v2list[1], out lng2);
                var m = GetDistance_Google(lat1, lng1, lat2, lng2);

                var msg = string.Format("{0}～{1}：{2}", lines[i], lines[i + 1], m);
                Console.WriteLine(msg);

                var d = m / 3;
                var dLat = (lat2 - lat1) / d;
                var dLng = (lng2 - lng1) / d;
                for (int j = 1; j <= d; j++)
                {
                    //m = GetDistance_Google(lat1, lng1, lat1 + dLat * j, lng1 + dLng * j);
                    //msg = string.Format("{0}～{1}：{2}", lat1 + "," + lng1, (lat1 + dLat * j) + "," + (lng1 + dLng * j), m);
                    //Console.WriteLine(msg);
                    listMAP.Add((lat1 + dLat * j) + "," + (lng1 + dLng * j));
                }
                listMAP.Add(lines[i + 1]);
                //s += listMAP[i] + "/";
                //gomap(lines[i], lines[i + 1]);
            }
            StreamWriter swGOOGLE = new StreamWriter(str + @"\NewMAPGOOGlE.txt");
            for (int i = 0; i < listMAP.Count() - 1; i++)
            {
                var m = gomap(listMAP[i], listMAP[i + 1]);
                //var m = GetDistance_Google(listMAP[i], listMAP[i + 1]);
                //var msg = string.Format("{0}～{1}：{2}", listMAP[i], listMAP[i + 1], m);
                swGOOGLE.WriteLine(m);// 寫入文字
                Console.WriteLine(m);
            }
            swGOOGLE.Close();
            //Console.WriteLine(s);
            StreamWriter sw = new StreamWriter(str + @"\NewMAP.txt");
            foreach (var item in listMAP)
            {
                sw.WriteLine(item);// 寫入文字
            }
            sw.Close();
        }

        private static string gomap(string v1, string v2)
        {
            var msg = "";
            using (var client = new WebClient())
            {
                string url = "https://maps.googleapis.com/maps/api/distancematrix/json?origins=" + v1 + "&destinations=" + v2 + "&language=zh_TW&sensor=false&mode=walking";
                var result = client.DownloadData(url);
                var json = Encoding.UTF8.GetString(result);
                var serializer = new JavaScriptSerializer();
                var distanceResponse = serializer.Deserialize<DistanceResponse>(json);
                if (string.Equals("ok", distanceResponse.Status, StringComparison.OrdinalIgnoreCase))
                {
                    foreach (var row in distanceResponse.Rows)
                    {
                        foreach (var element in row.Elements)
                        {
                            if (string.Equals("ok", element.Status, StringComparison.OrdinalIgnoreCase))
                            {
                                //list.Add( element.Distance.Text + element.Distance.Value);
                                //  Label1.Text = "Distance: " + element.Distance.Text + element.Distance.Value;
                                //if (element.Distance.Value <= 50000)//超過50公里
                                //{

                                //}
                                msg = string.Format("{0}～{1}：{2}", distanceResponse.Origin_Addresses[0], distanceResponse.Destination_Addresses[0], element.Distance.Text);
                                Console.WriteLine(msg);
                            }
                            else
                            {
                                msg = string.Format("{0}～{1}：{2}", distanceResponse.Origin_Addresses[0], distanceResponse.Destination_Addresses[0], element.Distance.Text);
                                //list.Add("Error");
                                //// Label1.Text = "地址錯惹！！";
                            }
                        }
                    }
                }
                else if (string.Equals("OVER_QUERY_LIMIT", distanceResponse.Status, StringComparison.OrdinalIgnoreCase))
                {
                    Thread.Sleep(4000);//延遲4000毫秒才處理下一筆
                    msg = gomap( v1,  v2);
                }
                else
                {
                    msg = string.Format("{0}～{1}：{2}", v1, v2, distanceResponse.Status);
                }

            }
            return msg;
        }

        public static double GetDistance_Google(string v1, string v2)
        {
            var v1list = v1.Split(',');
            var v2list = v2.Split(',');
            var lat1 = 0.0;
            var lng1 = 0.0;
            var lat2 = 0.0;
            var lng2 = 0.0;
            double.TryParse(v1list[0], out lat1);
            double.TryParse(v1list[1], out lng1);
            double.TryParse(v2list[0], out lat2);
            double.TryParse(v2list[1], out lng2);
            return GetDistance_Google(lat1, lng1, lat2, lng2);
        }
        /// <summary>
        /// from Google Map 腳本
        /// <para>出處：http://windperson.wordpress.com/2011/11/01/由兩點經緯度數值計算實際距離的方法/ </para>
        /// </summary>
        /// <param name="lat1"></param>
        /// <param name="lng1"></param>
        /// <param name="lat2"></param>
        /// <param name="lng2"></param>
        /// <returns>回傳單位 公尺</returns>
        public static double GetDistance_Google(double lat1, double lng1, double lat2, double lng2)
        {
            var earthRadius = 6371; //appxoximate radius in miles
            var factor = Math.PI / 180;
            var dLat = (lat2 - lat1) * factor;
            var dLon = (lng2 - lng1) * factor;
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) + Math.Cos(lat1 * factor)
              * Math.Cos(lat2 * factor) * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            double d = earthRadius * c * 1000;

            return d;
        }

    }
    class Employee
    {
        public string EmpName;
    }
    public class DistanceResponse
    {
        /// <summary>
        /// 狀態
        /// </summary>
        public string Status { get; set; }
        /// <summary>
        /// 起點
        /// </summary>
        public string[] Origin_Addresses { get; set; }
        /// <summary>
        /// 終點
        /// </summary>
        public string[] Destination_Addresses { get; set; }
        /// <summary>
        /// 回傳資料
        /// </summary>
        public Row[] Rows { get; set; }
        public string Error_message { get; set; }
    }
    public class Row
    {
        /// <summary>
        /// 資料內容
        /// </summary>
        public Element[] Elements { get; set; }
    }
    public class Element
    {
        /// <summary>
        /// 狀態
        /// </summary>
        public string Status { get; set; }
        /// <summary>
        /// 時間
        /// </summary>
        public Item Duration { get; set; }
        /// <summary>
        /// 距離
        /// </summary>
        public Item Distance { get; set; }
    }
    public class Item
    {
        /// <summary>
        /// 原始數值
        /// </summary>
        public int Value { get; set; }
        /// <summary>
        /// 單位換算後的數值
        /// </summary>
        public string Text { get; set; }
    }
}
