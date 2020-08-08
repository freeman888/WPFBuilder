using System;
using System.IO;
using System.Diagnostics;
using System.Collections;
using System.Reflection;
using System.Xml;

namespace WPFCreater
{
    class Program
    {
        /// <summary>
        /// eg : --name Hello --path F:\Gasoline\Hello\platform\WPF\Hello --lib Hello
        /// </summary>
        /// <param name="name"></param>
        /// <param name="path"></param>
        /// <param name="lib"></param>
        static void Main(string name, string path, string[] lib)
        {
            //var r = Environment.GetEnvironmentVariable("dotnet");
            //foreach(DictionaryEntry i in Environment.)
            //{
            //    Console.WriteLine(i.Value);
            //}
            //创建项目
            ProcessStartInfo processinfo1 = new ProcessStartInfo("dotnet")
            {
                CreateNoWindow = false,
                Arguments = $"new wpf --name {name} --output {path}"
            };
            Process.Start(processinfo1).WaitForExit();
            //添加nuget system.io.compression包
            ProcessStartInfo processinfo2 = new ProcessStartInfo("dotnet")
            {
                CreateNoWindow = false,
                Arguments = $"add {path} package system.io.compression"
            };
            Process.Start(processinfo2).WaitForExit();
            //释放GI和GTWPF
            Assembly assembly = typeof(Program).Assembly;
            var gistream = assembly.GetManifestResourceStream("WPFCreater.GI.dll");
            byte[] b_gi = new byte[gistream.Length];
            gistream.Read(b_gi, 0, b_gi.Length);
            var gtwpfstream = assembly.GetManifestResourceStream("WPFCreater.GTWPF.dll");
            byte[] b_gt = new byte[gtwpfstream.Length];
            gtwpfstream.Read(b_gt, 0, b_gt.Length);
            FileStream fs_gi = new FileStream(path + "\\GI.dll", FileMode.OpenOrCreate);
            fs_gi.Write(b_gi, 0, b_gi.Length);
            FileStream fs_gt = new FileStream(path + "\\GTWPF.dll", FileMode.OpenOrCreate);
            fs_gt.Write(b_gt, 0, b_gt.Length);
            fs_gi.Close();
            fs_gt.Close();
            //设置启动代码
            var mwxcs_path = path + "\\MainWindow.xaml.cs";
            var mwxcs_code = @"using System.Windows;
using System.IO;
using System.IO.Compression;
using System.Xml;
using GI;
using System;
namespace __namespace__
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            Hide();
            string[] gaas = new string[]
            {
               __libs__
            };

            foreach(var i in gaas)
            {
                Stream stream = App.GetResourceStream(new Uri(""\\"" + i + "".gaa"", UriKind.Relative)).Stream;
                ZipArchive zipArchive = new ZipArchive(stream);
                
                var entry = zipArchive.GetEntry(i+""/information.xml"");

                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(entry.Open());
                var type =  xmlDocument.ChildNodes[0].GetAttribute(""source"");
                if(type == ""gas"")
                {
                    XmlDocument code = new XmlDocument();
                    code.Load(zipArchive.GetEntry(i + ""/source/code.xml"").Open());
                    GI.Gasoline.Loadgasxml(code);
                }
            }
            GTWPF.MainWindow mainWindow = new GTWPF.MainWindow();
            mainWindow.Show();

        }
    }
}";
            mwxcs_code = mwxcs_code.Replace("__namespace__", name);
            var _libs = "";
            foreach(var i in lib)
            {
                _libs +=$"\"{i}\",";
            }

            mwxcs_code = mwxcs_code.Replace("__libs__",_libs);
            using( StreamWriter streamWriter = new StreamWriter(mwxcs_path, false))
            {
                streamWriter.Write(mwxcs_code);
            }

            //设置引用
            var csproj_path = path + $"\\{name}.csproj";
            var csproj_code = @"<Project Sdk=""Microsoft.NET.Sdk.WindowsDesktop"">

  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>netcoreapp3.1</TargetFramework>
    <UseWPF>true</UseWPF>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include=""system.io.compression"" Version=""4.3.0"" />
  </ItemGroup>
  
  <ItemGroup>
    <Reference Include=""GI"">
      <HintPath>GI.dll</HintPath>
    </Reference>
    <Reference Include=""GTWPF"">
      <HintPath>GTWPF.dll</HintPath>
    </Reference>
  </ItemGroup>
  

</Project>";
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(csproj_code);
            
            var root = xmlDocument.FirstChild;
            var gaa_xml = xmlDocument.CreateElement("ItemGroup");
            foreach(var i in lib)
            {
                var res = xmlDocument.CreateElement("Resource");
                res.SetAttribute("Include", i + ".gaa");
                gaa_xml.AppendChild(res);
            }
            root.AppendChild(gaa_xml);

            xmlDocument.Save(csproj_path);

        }
    }
}

