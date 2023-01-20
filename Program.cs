using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;

namespace voiceCopy
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Green;

            Console.WriteLine("Downloading newest voice over");

            DownLoadFileInBackground("http://storage.adamko.tech/voiceover.bnk");

            Console.ReadKey();
            return;
        }

        private static string GetDestinationPath()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Classes\\.wotreplay\\shell\\open\\command"))
                {
                    if (key != null)
                    {
                        Object result = key.GetValue("");
                        if (result != null)
                        {

                            string destPath = result.ToString().Remove(result.ToString().Length - 3).Replace("\"", "")
                             .Replace("\\win64\\WorldOfTanks.exe", "")
                             .Trim();

                            destPath = Path.Combine(destPath, "res_mods");

                            string temp = destPath;
                            int i = 0;

                            //find newest version folder
                            while (!char.IsDigit(destPath[destPath.Length - 1]))
                            {

                                destPath = Path.Combine(temp, new DirectoryInfo(temp).GetDirectories().OrderByDescending(d => d.LastWriteTimeUtc).ElementAt(i).Name).Trim();
                                i++;
                            }


                            destPath = Path.Combine(destPath, "audioww");

                            Console.WriteLine("Copied to \""+destPath+"\"");
                            return (string)destPath;
                        }
                        else
                        {
                            Console.WriteLine("Could not find path (Value of key is null)");
                            return null;
                        }
                    }
                    else
                    {
                        Console.WriteLine("Could not find path (Key not found)");
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Something went wrong with search for WoT game folder path...\n\n" + ex);
                return null;
            }
        }

        private static string GetSourcePath() {
            string sourceFile = "voiceover.bnk";
            return (string)sourceFile;
        }

        public static void DownLoadFileInBackground(string address)
        {
            WebClient client = new WebClient();
            Uri uri = new Uri(address);

            // Specify a DownloadFileCompleted handler here...

            // Specify a progress notification handler.
            client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(DownloadProgressCallback);

            client.DownloadFileAsync(uri, "voiceover.bnk");
        }

        private static void DownloadProgressCallback(object sender, DownloadProgressChangedEventArgs e)
        {
            Console.Write("\r{0}% downloaded. Progress: {1} KB / {2} KB",
                e.ProgressPercentage,
                e.BytesReceived / 1024,
                e.TotalBytesToReceive / 1024);

            if (e.ProgressPercentage == 100)
            {
                Thread.Sleep(250);
                Console.Write("\n");
                MoveFile();
            }
        }

        private static void MoveFile()
        {

            string destPath = GetDestinationPath();
            string sourcePath = GetSourcePath();

            if (destPath == null)
            {
                Console.WriteLine("Could not find game folder, budzeš mušel ručne porobic :(");
                return;
            }

            try
            {
                if (!Directory.Exists(destPath))
                    Directory.CreateDirectory(destPath);

                //copy voiceover
                File.Move(sourcePath, Path.Combine(destPath, "voiceover.bnk"), true);
                Console.WriteLine("Done :)");
                Console.WriteLine("Press any key to close app.");
            }
            catch (Exception e)
            {
                Console.WriteLine("Chyba pri kopírovaní: " + e);
            }
        }
    }
}
