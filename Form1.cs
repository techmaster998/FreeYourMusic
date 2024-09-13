using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using VideoLibrary;
using System.Diagnostics;
using LibVLCSharp.Shared;
using Vlc.DotNet.Core;
using Microsoft.Web.WebView2.Core;
using Vlc.DotNet.Core.Interops.Signatures;
using static System.Net.WebRequestMethods;
using Vlc.DotNet.Forms;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Net;
using FFMpegCore.Enums;
using FreeYourMusic.Properties;
using Xabe.FFmpeg;
using MediaToolkit;
using static MediaToolkit.Model.Metadata;
using File = System.IO.File;

namespace FreeYourMusic
{
    public partial class Form1 : Form
    {
        public string videoLink;
        public string currentAddress;
        public Uri resultsAddress;
        //public string videoPath;
        public string videoDirectory;
        public Form1()
        {
            InitializeComponent();
            Core.Initialize();
            FormClosing += OnProcessExit;
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyVideos), "FreeYourMusic");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            if (Directory.Exists(path))
            {
                videoDirectory = path;
            }
            currentAddress = webView21.Source.ToString();
            listView1.Items.Add(currentAddress);
            webView21.SourceChanged += webView21_SourceChanged;
            this.AcceptButton = button1;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void webView21_Click(object sender, EventArgs e)
        {


        }

        private void webView21_SourceChanged(object sender, CoreWebView2SourceChangedEventArgs e)
        {
            string uri = webView21.Source.ToString();

            if (e.IsNewDocument == false && uri.Contains("watch"))
            {
                currentAddress = webView21.Source.ToString();
                listView1.Items.Clear();
                listView1.Items.Add(currentAddress);
                SaveVideoToDiskAsync(currentAddress);
                //Run(currentAddress);
            }
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }



        public void SaveVideoToDiskAsync(string link)
        {
            var youTube = YouTube.Default; // starting point for YouTube actions
            var lowvideo = youTube.GetVideo(link);
            var videos = youTube.GetAllVideos(link);
            var maxResolution = videos.First(i => i.Resolution == videos.Max(j => j.Resolution));
            var video = maxResolution;
            string path = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
            var stream = video.Stream();
            listView1.Items.Clear();
            listView1.Items.Add("Downloading..."); 
            byte[] bytes = video.GetBytes();
            byte[] bytesL = lowvideo.GetBytes();
            File.WriteAllBytes(videoDirectory + "\\" + "video.mp4", bytes);
            File.WriteAllBytes(videoDirectory + "\\" + "audio.mp3", bytesL);
            if (File.Exists(videoDirectory + "\\" + "video.mp4"))
            {
                Uri searchUrl = new Uri("https://www.youtube.com/results?search_query=" + textBox1.Text);
                webView21.GoBack();
                string videoPath = videoDirectory + "\\" + "video.mp4";
                string audioPath = videoDirectory + "\\" + "audio.mp3";
                listView1.Items.Clear();
                listView1.Items.Add("Download complete.");
                ExtractAudio(audioPath, videoPath, video.FullName);

            }
        }

        public void ExtractAudio(string audioPath, string videoPath, string FVN)
        {
            //string audioName = audioPath.Substring(0, audioPath.Length - 4);
            var convertAudio = new NReco.VideoConverter.FFMpegConverter();
            convertAudio.ConvertMedia(audioPath, audioPath + ".mp3", "mp3");
            //File.Delete(audioPath);
            listView1.Items.Clear();
            listView1.Items.Add("Conversion complete.");
            MergeAudioVideo(audioPath + ".mp3", videoPath, FVN);
        }

        public void MergeAudioVideo(string audiopath, string videopath, string FVN)
        {
            Process proc = new Process();
            proc.StartInfo.Arguments = string.Format("-i {0} -i {1} -acodec copy -vcodec copy -map 0:0 -map 0:1 {2}", audiopath, videopath, FVN);
            PlayFinishedVideo(FVN);
            //using (var engine = new Engine())
            //{
            //    engine.CustomCommand("-i videopath -i audiopath -c copy FVN.mp4");
            //    listView1.Items.Clear();
            //    listView1.Items.Add("Merge complete.");
            //    PlayFinishedVideo(FVN);
            //}
        }

        public void PlayFinishedVideo(string FVN)
        {
            FileInfo fi = new FileInfo(FVN);
            vlcControl1.SetMedia(fi);
            vlcControl1.Play();
            vlcControl1.Focus();
            listView1.Items.Clear();
            listView1.Items.Add(FVN);
        }

        private void vlcControl1_Click(object sender, EventArgs e)
        {

        }

        private void OnProcessExit(object sender, FormClosingEventArgs e)
        {
            vlcControl1.Stop();
            DirectoryInfo di = new DirectoryInfo(videoDirectory);

            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }
        }
        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space)
            {
                e.SuppressKeyPress = e.Handled = true;
                MessageBox.Show("You pressed space.");
                button1_Click(this, new EventArgs());
            }
        }

        private void vlcControl1_controlRewind(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Left)
            {
                e.SuppressKeyPress = e.Handled = true;
                MessageBox.Show("You pressed left.");
                vlcControl1.Position = vlcControl1.Position - 10;
            }
        }
        private void vlcControl1_controlForward(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Right)
            {
                e.SuppressKeyPress = e.Handled = true;
                MessageBox.Show("You pressed right.");
                vlcControl1.Position = vlcControl1.Position - 10;
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            Uri searchUrl = new Uri("https://www.youtube.com/results?search_query=" + textBox1.Text);
            webView21.Source = searchUrl;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            
        }

        private void button3_Click(object sender, EventArgs e)
        {
            vlcControl1.Pause();
        }
    }
}
