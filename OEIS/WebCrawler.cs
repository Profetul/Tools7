using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace OEIS
{
    public static class WebCrawler
    {
        static object ioLock = new object();
        static void Crawl(string targetFolder)
        {
            //List<Task> tasks = new List<Task>();
            //foreach (int indx in list)
            //{
            //    string id = indx.ToString("000000");
            //    string folder = folder + "\\" + ((int)(indx / 10000) * 10000).ToString("000000");
            //    var filePath = String.Format("{0}\\b{1}.txt", folder, id);
            //    if (File.Exists(filePath))
            //    {
            //        continue;
            //    }

            //    if (!Directory.Exists(folder))
            //    {
            //        Directory.CreateDirectory(folder);
            //    }

            //    Task task = DownloadOeis(id, folder);
            //    tasks.Add(task);
            //    if ((indx + 1) % 100 == 0)
            //    {
            //        Task.WaitAll(tasks.ToArray());
            //        tasks.Clear();
            //    }
            //}
            //Task.WaitAll(tasks.ToArray());
        }

        private static Task DownloadOeis(string id, string folder, int iteration = 0)
        {
            return Task.Factory.StartNew(() =>
            {

                var fileName = String.Format("b{0}.txt", id);
                var filePath = String.Format("{0}\\{1}", folder, fileName);
                if (!File.Exists(filePath))
                {
                    if (iteration > 50)
                    {
                        return;
                    }
                    Task.Delay((new Random()).Next() % 500).Wait();
                    HttpClient client = new HttpClient();
                    try
                    {
                        var fileTask = client.GetStringAsync(new Uri(String.Format("https://oeis.org/A{0}/{1}", id, fileName)));
                        fileTask.Wait();
                        var file = fileTask.Result;
                        if (!String.IsNullOrWhiteSpace(file))
                        {
                            lock (ioLock)
                            {
                                File.WriteAllText(filePath, file);
                            }
                        }
                    }
                    catch (AggregateException ex)
                    {
                        if (!ex.InnerExceptions.Any(e => e.Message.Contains("404")))
                        {
                            Task.Delay((new Random()).Next() % 5000).Wait();
                            DownloadOeis(id, folder, iteration + 1).Wait();
                        }
                    }
                    catch (Exception ex1)
                    {

                    }
                }
            });
        }
    }
}
