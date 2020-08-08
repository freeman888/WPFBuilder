using System;
using System.IO;
using System.Diagnostics;

namespace WPFBuilder
{
    class Program
    {

        /// <summary>
        /// build a wpf application
        /// </summary>
        /// <param name="file">the csproj path</param>
        /// <param name="outpath">the output path</param>
        static void Main(string file ,string outpath)
        {
            try
            {
                FileInfo fileInfo = new FileInfo(file);
                if (fileInfo.Extension != ".csproj")
                    throw new Exception("this is not a right wpf project");
                ProcessStartInfo process = new ProcessStartInfo("dotnet");
                process.CreateNoWindow = false;
                process.Arguments = $"publish {file} --self-contained true --runtime win-x86 -p:PublishSingleFile=true -p:PublishTrimmed=true --output {outpath}";
                Process.Start(process).WaitForExit();
                //process.Arguments = "dotnet publish wpftest.csproj --self-contained true --runtime win-x86 -p:PublishSingleFile=true -p:PublishTrimmed=true --output f:\";


            }
            catch(Exception e)
            {
                Console.Write(e);
            }
        }
    }
}
