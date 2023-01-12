using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Updater
{
    public class GithubRelease
    {
        public class VersionInfo
        {
            /// <summary>
            /// 版本标题
            /// </summary>
            public string Title { get; set; }
            /// <summary>
            /// 版本号
            /// </summary>
            public string Version { get; set; }
            /// <summary>
            /// 是否是预览版本
            /// </summary>
            public bool IsPre { get; set; }
            /// <summary>
            /// 下载路径
            /// </summary>
            public string DownloadUrl { get; set; }
            /// <summary>
            /// 版本更新内容网页链接
            /// </summary>
            public string HtmlUrl { get; set; }
        }
        public class GithubModel
        {
            public string tag_name { get; set; }
            public string html_url { get; set; }
            public string name { get; set; }
            public bool prerelease { get; set; }

            public List<GithubAssetsModel> assets { get; set; }
        }
        public class GithubAssetsModel
        {
            public string browser_download_url { get; set; }
        }
        private string githubUrl;
        private string nowVersion;
        public VersionInfo Info { get; set; }

        //public event UpdaterEventHandler RequestCompleteEvent;
        //public event UpdaterEventHandler RequestErrorEvent;
        //public delegate void UpdaterEventHandler(object sender, object value);
        public GithubRelease(string githubUrl, string nowVersion)
        {
            this.githubUrl = githubUrl;
            this.nowVersion = nowVersion;
            Info = new VersionInfo();
        }
        public bool IsCanUpdate()
        {
            return !(nowVersion == Info.Version);
        }
        public async Task<VersionInfo> GetRequest()
        {
            var result = await Task.Run(() =>
            {
                HttpWebResponse httpWebRespones = null;
                try
                {
                    System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    HttpWebRequest httpWebRequest = WebRequest.Create(githubUrl) as HttpWebRequest;
                    httpWebRequest.Timeout = 60 * 1000;
                    httpWebRequest.ReadWriteTimeout = 60000;
                    httpWebRequest.AllowAutoRedirect = true;
                    httpWebRequest.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/68.0.3440.106 Safari/537.36";
                    httpWebRespones = (HttpWebResponse)httpWebRequest.GetResponse();


                    using (Stream stream = httpWebRespones.GetResponseStream())
                    {
                        List<byte> lst = new List<byte>();
                        int nRead = 0;
                        while ((nRead = stream.ReadByte()) != -1) lst.Add((byte)nRead);
                        byte[] bodyBytes = lst.ToArray();

                        string body = Encoding.UTF8.GetString(bodyBytes, 0, bodyBytes.Length);

                        var data = JsonConvert.DeserializeObject<GithubModel>(body);
                        Info.IsPre = data.prerelease;
                        Info.Title = data.name;
                        Info.Version = data.tag_name;
                        Info.DownloadUrl = data.assets[0].browser_download_url;
                        Info.HtmlUrl = data.html_url;
                        return Info;
                    }

                }
                catch (Exception)
                {
                    return null;
                }

            });
            return result;
        }
    }
}
