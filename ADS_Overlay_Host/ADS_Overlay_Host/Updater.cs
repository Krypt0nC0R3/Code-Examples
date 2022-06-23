using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

namespace Krypt0n.Tools
{
    public class Updater
    {
        private string _vlink = null;
        private System.Version _currentVersion = null;

        private string _fileLink = null;
        private System.Version _newestVersion = null;
        private string _pathcnote = null;
        private string _md5 = null;

        public string Pathcnote { get { return _pathcnote; } }
        public System.Version FindedVersion { get { return _newestVersion; } }

        public DownloadProgressChanged OnDownloadProgressChanged;
        public DownloadCompleted OnDownloadCompleted;

        public delegate void DownloadProgressChanged(int Percentage);
        public delegate void DownloadCompleted(string Filename);

        public Updater()
        {
            _currentVersion = new Version("0.0.1");
        }

        public Updater(string VersionsLink,System.Version CurrentVersion = null)
        {
            if (CurrentVersion != null)
                _currentVersion = CurrentVersion;
            else
                _currentVersion = new Version("0.0.1");

            _vlink = VersionsLink;
        }

        public bool UpdatesAvailable()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var response = client.GetAsync(_vlink).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = response.Content;

                        // by calling .Result you are synchronously reading the result
                        string responseString = responseContent.ReadAsStringAsync().Result;

                        JArray versions = JArray.Parse(responseString);
                        _newestVersion = _currentVersion;
                        for(int i = 0; i < versions.Count; i++)
                        {
                            System.Version _newVersion = new System.Version((string)((JObject)versions[i]).GetValue("version"));
                            if (_newVersion > _newestVersion)
                            {
                                _newestVersion = _newVersion;
                                _fileLink = (string)((JObject)versions[i]).GetValue("file");
                                _pathcnote = (string)((JObject)versions[i]).GetValue("pacthnote");
                                _md5 = (string)((JObject)versions[i]).GetValue("md5");
                            }
                        }
                        return _newestVersion > _currentVersion;
                    }
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        private string CalculateMD5(string filename)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filename))
                {
                    var hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }
        public void DownloadLastestVersion()
        {
            WebClient client = new WebClient();

            string filename = _newestVersion.ToString() + ".update";

            client.DownloadProgressChanged += (sndr, args) => {
                OnDownloadProgressChanged?.Invoke(args.ProgressPercentage);
            };
            client.DownloadFileCompleted += (sndr, args) =>
            {
                try
                {
                    if (!CalculateMD5(filename).ToLower().Equals(_md5.ToLower()))
                    {
                        throw new Exception();
                    }
                    OnDownloadCompleted?.Invoke(filename);
                }
                catch
                {
                    OnDownloadCompleted?.Invoke(null);
                    return;
                }
            };
            client.DownloadFileAsync(new Uri(_fileLink), filename);
        }

        
    }
}
