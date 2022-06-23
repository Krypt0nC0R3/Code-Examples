using System;
using System.IO;
using Newtonsoft.Json.Linq;
using VkNet;
using VkNet.Model;

namespace VKPoster
{
    class Program
    {
        static string path = AppDomain.CurrentDomain.BaseDirectory;
        static void Main(string[] args)
        {
            Log("VKPoster started!");
            try
            {
                if (args.Length > 0)
                {
                    path = args[0];
                }

                FileInfo[] files = new DirectoryInfo(path).GetFiles();

                Log($"{files.Length} files found");
                if (files.Length > 0)
                {
                    foreach (FileInfo file in files)
                    {
                        try
                        {
                            StreamReader sr = new StreamReader(file.FullName);
                            JObject json = JObject.Parse(sr.ReadToEnd());
                            sr.Close();
                            DateTime postTime = DateTime.Parse(json["date"].ToString());
                            TimeSpan span = postTime - DateTime.Now;
                            if (span.TotalSeconds <= 2 && span.TotalSeconds > 0)
                            {
                                VkApi api = new VkApi();
                                api.Authorize(new ApiAuthParams
                                {
                                    AccessToken = json["oauth"].ToString()
                                });

                                api.Messages.Send(new VkNet.Model.RequestParams.MessagesSendParams
                                {
                                    ChatId = Convert.ToInt64(json["id"]),
                                    Message = json["message"].ToString()
                                });
                            }
                        }
                        catch (Exception exp)
                        {
                            Log($"Error while parsing {file.FullName}: {exp.Message}\n{exp.StackTrace}");
                        }
                    }
                }
            }
            catch (Exception exp)
            {
                Log($"Error: {exp.Message}\n{exp.StackTrace}");
            }
        }

        static void Log(string message)
        {
            Console.WriteLine($"{DateTime.Now.ToString("dd.MM.yy HH:mm:ss")}:\t {message}");
        }
    }
}
