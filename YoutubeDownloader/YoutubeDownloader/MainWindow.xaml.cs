using Microsoft.WindowsAPICodePack.Taskbar;
using netlibrary;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using YoutubeDownloader.BLL;
using YoutubeExplode;



namespace YoutubeDownloader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ObservableCollection<BusinessLogicLayer> listbox_list = new ObservableCollection<BusinessLogicLayer>();
        List<AudioBusinessLogicLayer> audio_list = new List<AudioBusinessLogicLayer>();
        string varUrl = "";
        string varYoutubeUrl = "";
        string varAudioUrl = "";
        string varFileName = "";
        string varAudioAppDataLocation = "";
        string varVideoAppDataLocation = "";
        string varVideoExtension = "";
        string varAudioExtension = "";
        CancellationTokenSource TokenSource = null;
        string strOutputDir = "";
        bool bAudioDownloadSuccess = false;
        bool bVideoDownloadSuccess = false;
        Thread backRun = null;
        bool bBackgroundRun = true;
        http header = new http();
        public MainWindow()
        {
            InitializeComponent();
            Combo1.ItemsSource = listbox_list;
            strOutputDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            strOutputDir = System.IO.Path.Combine(strOutputDir, "YoutubeDownloader");
            if (!Directory.Exists(strOutputDir))
            {
                System.IO.Directory.CreateDirectory(strOutputDir);
                
            }
            FolderLocation.Text = strOutputDir;
            string strAppDataLocation = (System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "YoutubeDownloader"));
            if (!Directory.Exists(strAppDataLocation))
            {
                System.IO.Directory.CreateDirectory(strAppDataLocation);
            }
            string strVideoAppDataLocation = System.IO.Path.Combine(strAppDataLocation, @"Video\parts");
            string strAudioAppDataLocation = System.IO.Path.Combine(strAppDataLocation, @"Audio\parts");
            if (Directory.Exists(strAppDataLocation))
            {
                if (!Directory.Exists(strVideoAppDataLocation))
                {
                    System.IO.Directory.CreateDirectory(strVideoAppDataLocation);
                }
                if (!Directory.Exists(strAudioAppDataLocation))
                {
                    System.IO.Directory.CreateDirectory(strAudioAppDataLocation);
                }

               
                varAudioAppDataLocation = strAudioAppDataLocation;
                varVideoAppDataLocation = strVideoAppDataLocation;
                
            }

            this.Dispatcher.Invoke(() => { DownloadBtn.IsEnabled = false; });

        }

        private async void GoBtn_Click(object sender, RoutedEventArgs e)
        {
           Top:;
            this.Dispatcher.Invoke(() =>
            {
                progress1.Value = 0;
                progress2.Value = 0;
                progress3.Value = 0;
                progress4.Value = 0;
                videoNameTextBlock.Text = "";
                VideoInfoTxt.Text = "";
                AudioInfoTxt.Text = "";
                MuxingInfoTxt.Text = "";
                txtBlockTransferRate.Text = "";
                txtBlockTimeLeft.Text = "";
                txtBlockVideoFileSize.Text = "";
                txtBlockAudioFileSize.Text = "";
                txtBlockStatus.Text = "";
                txtBlockDownloaded.Text = "";
            });
            TaskbarManager.Instance.SetProgressValue(0, 100);

            GoBtn.IsEnabled = false;
            varUrl = "";
            varFileName = "";
            varAudioUrl = "";
            varUrl = UrlText.Text;
            varUrl = varUrl.Trim();
            varYoutubeUrl = varUrl;
            if (varYoutubeUrl.Contains("https://www.youtube.com/watch?v=") || varYoutubeUrl.Contains("https://youtu.be/") || varYoutubeUrl.Contains("https://www.youtube.com/shorts/"))
            {

                listbox_list.Clear();
                audio_list.Clear();
                string videoTitle = "";
                img1.Source = new BitmapImage(new Uri("Images/white.png", UriKind.Relative));
                this.Dispatcher.Invoke(() => {
                    videoNameTextBlock.Text = "";
                    DownloadBtn.IsEnabled = false;
                });
                try
                {
                    var youtube = new YoutubeClient();
                    var video = await youtube.Videos.GetAsync(varUrl);
                    // Sanitize the video title to remove invalid characters from the file name
                    string sanitizedTitle = string.Join("_", video.Title.Split(System.IO.Path.GetInvalidFileNameChars()));

                    videoTitle = video.Title;


                    var streamManifest = await youtube.Videos.Streams.GetManifestAsync(video.Id);
                    var audioStreams = streamManifest.GetAudioOnlyStreams().OrderByDescending(s => s.Bitrate).ToList();
                    var distintAudio = audioStreams.DistinctBy(x => x.Bitrate);
                    foreach (var list in distintAudio)
                    {

                        audio_list.Add(new AudioBusinessLogicLayer { Bitrate = list.Bitrate.ToString(), Url = list.Url });

                    }

                    varAudioUrl = audio_list[0].Url;


                    var videoStreams = streamManifest.GetVideoOnlyStreams().OrderByDescending(s => s.VideoQuality).ToList();
                    var distinct = videoStreams.DistinctBy(x => x.VideoQuality.Label);
                    foreach (var list in distinct)
                    {
                        listbox_list.Add(new BusinessLogicLayer { label = "Quality - " + list.VideoQuality.Label + "                              Size - " + list.Size.ToString(), url = list.Url, quality = list.VideoQuality.Label, filename = sanitizedTitle });

                    }
                   

                }
                catch (Exception Ex)
                {
                    goto Top;
                }
                try
                {
                    Extract.YoutubeIdExtractor youtubeIdExtractor = new Extract.YoutubeIdExtractor();
                    string ThumbnailUrl = youtubeIdExtractor.ImageUrlConstructor(youtubeIdExtractor.IdExtract(varUrl));
                    Combo1.ItemsSource = listbox_list;
                    Combo1.SelectedIndex = 0;
                    GoBtn.IsEnabled = true;
                    img1.Source = new BitmapImage(new Uri(ThumbnailUrl));
                    this.Dispatcher.Invoke(() => {
                        videoNameTextBlock.Text = videoTitle;
                    });
                }
                catch (Exception Ex)
                {
                    MessageBox.Show(Ex.Message.ToString(), "GoBtn_Click Block-2 Error!");
                }
                finally
                {
                    this.Dispatcher.Invoke(() => { DownloadBtn.IsEnabled = true; });
                }
            }
            else
            {
                MessageBox.Show("Invalid youtube video url. Please check insterted url.");
                GoBtn.IsEnabled = true;
            }
        
        
           
        }
    
       private void DownloadBtn_Click(object sender, RoutedEventArgs e)
        {
            bBackgroundRun = true;
            backRun = new Thread(backgroundRun);
            backRun.Start();
            Globals.bVideoDownloadRunning = true;
            Globals.bAudioDownloadRunning = false;
            this.Dispatcher.Invoke(() =>
            {
                progress1.Value = 0;
                progress2.Value = 0;
                progress3.Value = 0;
                progress4.Value = 0;
                VideoInfoTxt.Text = "";
                AudioInfoTxt.Text = "";
                MuxingInfoTxt.Text = "";
                txtBlockTransferRate.Text = "";
                txtBlockTimeLeft.Text = "";
                txtBlockVideoFileSize.Text = "";
                txtBlockAudioFileSize.Text = "";
                txtBlockStatus.Text = "";
                txtBlockDownloaded.Text = "";
            });
            TaskbarManager.Instance.SetProgressValue(0, 100);
                     
            try
            {
                this.Dispatcher.Invoke(() => {
                    AudioInfoTxt.Text = "";
                    VideoInfoTxt.Text = "";
                    MuxingInfoTxt.Text = "";
                    DownloadBtn.IsEnabled = false;
                    GoBtn.IsEnabled = false;
                });
                TokenSource = new CancellationTokenSource();
                var Token = TokenSource.Token;
                string selectedVideo = Combo1.Text;
                if(selectedVideo!=null)
                {
                    foreach(var list in listbox_list)
                    {
                        if(list.label != null)
                        {
                            if (list.label.Contains(selectedVideo))
                            {
                                varUrl= list.url;
                                varFileName = "";
                                varFileName = list.filename + "_" +list.quality;
                             
                                try
                                {
                                   
                                    bVideoDownloadSuccess = true;
                                    header.GetHeaders(varUrl);
                                    Globals.strVideoExtension = "";
                                    Globals.strFileSize = "";
                                    Globals.strVideoExtension = header.strContentType;
                                    Globals.strFileSize = header.strContentLength;
                                    string strFileLength = Memory.FileSize(Globals.strFileSize);
                                    this.Dispatcher.Invoke(() => { txtBlockVideoFileSize.Text = "Video size: " + strFileLength; });
                                    Globals.strVideoExtension = StringManipulation.FilenameExtension(Globals.strVideoExtension);
                                    string strfileName = StringManipulation.ReplaceInvalidChars(varFileName);
                                    strfileName = System.IO.Path.Combine(varVideoAppDataLocation, strfileName);
                                    varVideoExtension = Globals.strVideoExtension;
                                    
                                    if (System.IO.File.Exists(strfileName+"_part1"+ varVideoExtension))
                                    {
                                        long localFileLength = new FileInfo(strfileName + "_part1" + varVideoExtension).Length;
                                        long remoteFileLength = Convert.ToInt64(header.strContentLength);
                                        if(localFileLength == remoteFileLength)
                                        {
                                            audioDownload();
                                            Globals.bVideoFileMergeSuccess = true;
                                            goto AlreadyExist;
                                        }
                                       
                                    }
                                    Downloader downloader = new Downloader();
                                    downloader.downloadFiles(varUrl, strfileName, varVideoExtension);
                             AlreadyExist:;

                                   

                                }
                                catch (Exception Ex)
                                {
                                    MessageBox.Show(Ex.Message.ToString());
                                    
                                }
                                finally
                                {
                                    TokenSource.Dispose();
                                    
                                }
                                Thread.Sleep(200);
                            }
                        }
                    }
                }
               
                
                Thread.Sleep(200);
            }
            catch(Exception Ex)
            {
                MessageBox.Show(Ex.Message.ToString());
            }
           
        }
        public void audioDownload()
        {
            Globals.bAudioDownloadRunning = true;
            Globals.bVideoDownloadRunning = false;
            this.Dispatcher.Invoke(() =>
            {
                progress1.Value = 0;
                progress2.Value = 0;
                progress3.Value = 0;
                progress4.Value = 0;
            });
            varUrl = varAudioUrl;
            //bAudiofile = true;
            //bVideofile = false;
                       
            try
            {
               
                bAudioDownloadSuccess = true;
                header.GetHeaders(varUrl);
                Globals.strAudioExtension = header.strContentType;
                Globals.strFileSize = header.strContentLength;
                string strFileLength = Memory.FileSize(Globals.strFileSize);
                this.Dispatcher.Invoke(() => { txtBlockAudioFileSize.Text = "Audio size: " + strFileLength; });
                Globals.strAudioExtension = StringManipulation.FilenameExtension(Globals.strAudioExtension);
                string strfileName = StringManipulation.ReplaceInvalidChars(varFileName);
                strfileName = System.IO.Path.Combine(varAudioAppDataLocation, strfileName);
                varAudioExtension = Globals.strAudioExtension;
                if (System.IO.File.Exists(strfileName + "_part1" + varAudioExtension))
                {
                    long localFileLength = new FileInfo(strfileName + "_part1" + varAudioExtension).Length;
                    long remoteFileLength = Convert.ToInt64(header.strContentLength);
                    if (localFileLength == remoteFileLength)
                    {
                        Globals.bAudioFileMergeSuccess = true;
                        goto AlreadyExist;
                    }
                    
                }
                Downloader downloader = new Downloader();
                downloader.downloadFiles(varUrl, strfileName, varAudioExtension);
            AlreadyExist:;
               

            }
            catch (Exception Ex)
            {
                MessageBox.Show(Ex.Message.ToString());

            }
            finally
            {
                TokenSource.Dispose();
                
            }
          
        }
        public async void Mux()
        {
            bBackgroundRun = false;
            string OutputFolderLocation = "";
            try
            {

               if(Globals.bAudioFileMergeSuccess && Globals.bVideoFileMergeSuccess)
                {
                    this.Dispatcher.Invoke(() => {
                        progress1.IsIndeterminate = true; progress1.UpdateLayout();
                        progress2.IsIndeterminate = true; progress2.UpdateLayout();
                        progress3.IsIndeterminate = true; progress3.UpdateLayout();
                        progress4.IsIndeterminate = true; progress4.UpdateLayout();
                        MuxingInfoTxt.Text = "Muxing............";
                    });
                    
                    string outputFileLocation =  strOutputDir + @"\" + varFileName+".mp4";

                    await Task.Run(() =>
                    {
                        if (System.IO.File.Exists(outputFileLocation))
                        {
                            bool bFileNameChanged = false;
                          
                            string NewName = "";
                            int i = 1;
                            do
                            {
                                NewName = strOutputDir + @"\" + varFileName + "(" + i.ToString() + ").mp4";
                                if (!System.IO.File.Exists(NewName))
                                {
                                    outputFileLocation = NewName;
                                    bFileNameChanged = true;
                                }
                                i++;
                            } while (!bFileNameChanged);
                        }
                    });
                    
                    if (System.IO.File.Exists( varVideoAppDataLocation+@"\"+varFileName+ "_part1"+varVideoExtension))
                    {
                      
                        if (System.IO.File.Exists(varAudioAppDataLocation+@"\"+varFileName+"_part1"+varAudioExtension))
                        {
                            await Task.Run(() => MuxAudioVideo(varVideoAppDataLocation +@"\"+ varFileName + "_part1" + varVideoExtension, varAudioAppDataLocation +@"\"+ varFileName + "_part1" + varAudioExtension, @outputFileLocation));
                        }
                    }
                   
                    OutputFolderLocation = outputFileLocation;
                    this.Dispatcher.Invoke(() => {
                        progress1.IsIndeterminate = false; progress1.UpdateLayout();
                        progress2.IsIndeterminate = false; progress2.UpdateLayout();
                        progress3.IsIndeterminate = false; progress3.UpdateLayout();
                        progress4.IsIndeterminate = false; progress4.UpdateLayout();
                        MuxingInfoTxt.Text = "Muxing completed.";
                    });
                    if (System.IO.File.Exists(varVideoAppDataLocation +@"\"+ varFileName + "_part1" + varVideoExtension))
                    {
                        System.IO.File.Delete(varVideoAppDataLocation + @"\" + varFileName + "_part1" + varVideoExtension);
                    }
                    if (System.IO.File.Exists(varAudioAppDataLocation + @"\" + varFileName + "_part1" + varAudioExtension))
                    {
                        System.IO.File.Delete(varAudioAppDataLocation + @"\" + varFileName + "_part1" + varAudioExtension);
                    }
                    if (Directory.Exists(varAudioAppDataLocation))
                    {
                        if (Directory.EnumerateFileSystemEntries(varAudioAppDataLocation).Any())
                        {
                            string[] Files = Directory.GetFiles(varAudioAppDataLocation);
                            for (int i = 0; i < Files.Count(); i++)
                            {
                                if (System.IO.File.Exists(Files[i]))
                                {
                                    System.IO.File.Delete(Files[i]);
                                }
                            }
                        }
                    }
                    if (Directory.Exists(varVideoAppDataLocation))
                    {
                        if (Directory.EnumerateFileSystemEntries(varVideoAppDataLocation).Any())
                        {
                            string[] Files = Directory.GetFiles(varVideoAppDataLocation);
                            for (int i = 0; i < Files.Count(); i++)
                            {
                                if (System.IO.File.Exists(Files[i]))
                                {
                                    System.IO.File.Delete(Files[i]);
                                }

                            }
                        }
                    }

                    
                    MessageBox.Show("Download Completed.", "YoutubeDownloader", MessageBoxButton.OK, MessageBoxImage.Exclamation, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                }


            }
            catch (Exception Ex)
            {
                MessageBox.Show(Ex.Message.ToString());
            }
            this.Dispatcher.Invoke(() => {
                DownloadBtn.IsEnabled = true;
                GoBtn.IsEnabled = true;
            });
           
            if (!bVideoDownloadSuccess | !bAudioDownloadSuccess)
            {
                MessageBox.Show("Download has not succeeded due to some error. Please redownload to resume.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            Globals.bVideoFileMergeSuccess = false;
            Globals.bAudioFileMergeSuccess = false;
        }
     
        private void MuxAudioVideo(string inputVideo,string inputAudio,string outputVideo)
        {
           
            try
            {
                FFMpegCore.FFMpeg.ReplaceAudio(inputVideo, inputAudio, outputVideo, true);
            }
            catch (Exception Ex)
            {
                MessageBox.Show(Ex.Message.ToString());
            }

        }

        private void FolderLocationButton_Click(object sender, RoutedEventArgs e)
        {
            string strFolder = FolderLocation.Text;
            Process.Start("Explorer", @strFolder);
        }
        public void concatVideoFiles()
        {
            fileConcate concateFiles = new fileConcate();
            concateFiles.VideoConcate(System.IO.Path.Combine(varVideoAppDataLocation, varFileName+"_part1"+varVideoExtension));
        }
        public void concatAudioFiles()
        {
            fileConcate concateFiles = new fileConcate();
            concateFiles.AudioConcate(System.IO.Path.Combine(varAudioAppDataLocation, varFileName + "_part1"+varAudioExtension));
        }
        private void backgroundRun()
        {
            while(bBackgroundRun)
            {
                if(Globals.bVideoDownloadSuccess)
                {
                    concatVideoFiles();
                    Globals.bVideoDownloadSuccess = false;
                    audioDownload();
                }
                if(Globals.bAudioDownloadSuccess)
                {
                    concatAudioFiles();
                    Globals.bAudioDownloadSuccess = false;
                }
                if(Globals.bVideoFileMergeSuccess && Globals.bAudioFileMergeSuccess)
                {
                    Mux();
                }
               
               
            }
        }

       

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Globals.bDownloadCanceled = true;
            bBackgroundRun = false;
            
        }

       
    }
}
