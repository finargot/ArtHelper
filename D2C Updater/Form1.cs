using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using Ionic.Zip;
using System.Diagnostics;
using Newtonsoft.Json;
using System.IO;

namespace D2C_Updater
{
    public partial class Form1 : Form
    {

        string changelog;
        private Version newVersion = null;
        private Version currentVersion = null;
        private WebClient updateClient = null;
        private bool startupUpdateCheck = true;
        public Form1()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            InitializeComponent();
           

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            checkForUpdates();
           // WebClient d2c = new WebClient();
           // d2c.DownloadFileAsync(new Uri("http://artifact.plus/HelperUpdate.zip"), "HelperUpdate.zip");
          //  d2c.DownloadProgressChanged += new DownloadProgressChangedEventHandler(Changed);
         //   d2c.DownloadFileCompleted += new AsyncCompletedEventHandler(Completed);
        }

        private void checkForUpdates()
        {
            string updateUrl = "https://api.github.com/repos/finargot/ArtHelper/releases/latest";
            currentVersion = new Version(Application.ProductVersion);
            updateClient = new WebClient();
            updateClient.DownloadStringCompleted += UpdateClient_DownloadStringCompleted;
            updateClient.Headers.Add("Content-Type", "application/json; charset=utf-8");
            updateClient.Headers.Add("Charset", "UTF-8");
            updateClient.Headers.Add("User-Agent", "Steam Desktop Authenticator");
            updateClient.DownloadStringAsync(new Uri(updateUrl));
        }
        private void UpdateClient_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
              try
               {
            dynamic resultObject = JsonConvert.DeserializeObject(e.Result);
                newVersion = new Version(resultObject.tag_name.Value);
                //changelog = resultObject.update_message.Value;
                //    currentVersion = new Version(Application.ProductVersion);
                using (FileStream fstream = File.OpenRead(@"version.txt"))
                {
                    // преобразуем строку в байты
                    byte[] array = new byte[fstream.Length];
                    // считываем данные
                    fstream.Read(array, 0, array.Length);
                    // декодируем байты в строку
                    string textFromFile = System.Text.Encoding.Default.GetString(array);
                    //  Console.WriteLine("Текст из файла: {0}", textFromFile);
                    currentVersion = new Version(textFromFile);
                }
                
                compareVersions();
           }
            catch (Exception)
            {
              MessageBox.Show("Не получилось проверить обновления.");
            } 
        }

        private void compareVersions()
        {
            if (newVersion > currentVersion)
            {
                DialogResult updateDialog = MessageBox.Show(String.Format("Доступна обновлённая версия, хотите скачать?\nВы обновитесь с версии {0} до {1}:\n{2}", currentVersion, newVersion.ToString(), changelog), "Новая версия", MessageBoxButtons.YesNo);
                if (updateDialog == DialogResult.Yes)
                {
                    
                        WebClient d2c = new WebClient();
                    try
                    {
                        d2c.DownloadFileAsync(new Uri("http://dota2vo.ru/HelperUpdate.zip"), "HelperUpdate.zip");
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Запустите программу от имени администратора!");
                    }
                    d2c.DownloadProgressChanged += new DownloadProgressChangedEventHandler(Changed);
                        d2c.DownloadFileCompleted += new AsyncCompletedEventHandler(Completed);
                  
                    
                    //  System.Diagnostics.Process.Start(@"Updater.exe");
                    //  Application.Exit();
                }
            }
            else
            {
                switch (MessageBox.Show((IWin32Window)this, "Вы используете последнюю версию! Запустить Artifact Помощник?", "Обновлений нет", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
                {
                    case DialogResult.Yes:
                        System.Diagnostics.Process.Start("Artifact Helper");
                        Application.Exit();
                        break;
                    case DialogResult.No:
                        Application.Exit();
                        break;
                }
                if (!startupUpdateCheck)
                {
                    MessageBox.Show(String.Format("You are using the latest version: {0}", Application.ProductVersion));
                }
            }

            newVersion = null; // Check the api again next time they check for updates
            updateClient = null; // Set to null to indicate it's done checking
            startupUpdateCheck = false; // Set when it's done checking on startup
        }
        private void Completed(object sender, AsyncCompletedEventArgs e)
        {
            if (this.progressBar1.Value == 100)
                this.label1.Text = "Загрузка и распаковка завершены";
            string startupPath = Application.StartupPath;
            using (ZipFile zipFile = new ZipFile("HelperUpdate.zip"))
            {
                zipFile.ProvisionalAlternateEncoding = Encoding.GetEncoding("cp866");
                foreach (ZipEntry zipEntry in zipFile)
                    zipEntry.Extract(startupPath, ExtractExistingFileAction.OverwriteSilently);
            }

            try
            {
                switch (MessageBox.Show((IWin32Window)this, "Ничего не скачалось? Перезапустите программу от имени администратора!\n\nЗапустить последнюю версию?", "Загрузка завершена", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
                {
                    case DialogResult.Yes:
                        System.Diagnostics.Process.Start("Artifact Helper");
                        Application.Exit();
                        break;
                    case DialogResult.No:
                        Application.Exit();
                        break;
                }
            }
            catch
            {
            }
        }

        private void Changed(object sender, DownloadProgressChangedEventArgs e)
        {
            this.progressBar1.Value = e.ProgressPercentage;
            this.label1.Text = string.Format("Загружено: {0} Кбайт / {1} Кбайт", (object)(e.BytesReceived / 1024L), (object)(e.TotalBytesToReceive / 1024L));
        }
    }
}
