using Microsoft.WindowsAPICodePack.Taskbar;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows;


namespace YoutubeDownloader
{
    internal class Downloader
    {
      
       
        CancellationTokenSource TokenSource = new CancellationTokenSource();
        bool bDownloadPartOneSuccess = false;
        bool bDownloadPartTwoSuccess = false;
        bool bDownloadPartThreeSuccess = false;
        bool bDownloadPartFourSuccess = false;
        double totalByteReceived = 0;
        long remoteFileLength = 0;
        long lTotalLocalFileLength = 0;
       
        bool bLocalFileAdd_1 = false;
        bool bLocalFileAdd_2 = false;
        bool bLocalFileAdd_3 = false;
        bool bLocalFileAdd_4 = false;
        public Downloader()
        {
            totalByteReceived = 0;
            remoteFileLength = 0;
            lTotalLocalFileLength = 0;
            bLocalFileAdd_1 = false;
            bLocalFileAdd_2 = false;
            bLocalFileAdd_3 = false;
            bLocalFileAdd_4 = false;
        }
       
        public void downloadFiles(string url, string fileName, string extension)
        {
            try
            {
                if (Application.Current.Dispatcher.CheckAccess())
                {
                    ((MainWindow)System.Windows.Application.Current.MainWindow).txtBlockStatus.Text = "Connecting....";
                }
                else
                {
                    Application.Current.Dispatcher.Invoke(new System.Action(() =>
                    {                        
                        ((MainWindow)System.Windows.Application.Current.MainWindow).txtBlockStatus.Text = "Connecting....";
                    }));
                }
                              
                remoteFileLength = Convert.ToInt64(Globals.strFileSize);
                long eachPart = remoteFileLength / 4;
                var token = TokenSource.Token;
                Thread t1 = new Thread(() => downloadPartOne(token, url, 0, eachPart, fileName + "_part1" + extension));
                Thread t2 = new Thread(() => downloadPartTwo(token, url, eachPart + 1, eachPart * 2, fileName + "_part2" + extension));
                Thread t3 = new Thread(() => downloadPartThree(token, url, (eachPart * 2) + 1, eachPart * 3, fileName + "_part3" + extension));
                Thread t4 = new Thread(() => downloadPartFour(token, url, (eachPart * 3) + 1, remoteFileLength, fileName + "_part4" + extension));
               
                t1.Start();
                t2.Start();
                t3.Start();
                t4.Start();
            }
            catch(Exception Ex)
            {
                MessageBox.Show(Ex.Message.ToString());
            }
            finally
            {

            }
           
        }
        private void downloadPartOne(CancellationToken token, string url, long startRange, long endRange, string fileName)
        {
            token = TokenSource.Token;
            long prevFileLength = 0;
        Top:
            if(Globals.bVideoDownloadRunning)
            {
                
                    if (Application.Current.Dispatcher.CheckAccess())
                    {
                        ((MainWindow)System.Windows.Application.Current.MainWindow).Dispatcher.Invoke(() => { ((MainWindow)System.Windows.Application.Current.MainWindow).VideoInfoTxt.Text = "Video downloading........."; });
                    }
                    else
                    {
                        Application.Current.Dispatcher.Invoke(new System.Action(() =>
                        {
                            ((MainWindow)System.Windows.Application.Current.MainWindow).VideoInfoTxt.Text = "Video downloading.........";
                        }));
                    }
                
            }
            if (Globals.bAudioDownloadRunning)
            {
                

                    if (Application.Current.Dispatcher.CheckAccess())
                    {
                        ((MainWindow)System.Windows.Application.Current.MainWindow).Dispatcher.Invoke(() => { ((MainWindow)System.Windows.Application.Current.MainWindow).AudioInfoTxt.Text = "Audio downloading........."; });
                    }
                    else
                    {
                        Application.Current.Dispatcher.Invoke(new System.Action(() =>
                        {
                            ((MainWindow)System.Windows.Application.Current.MainWindow).AudioInfoTxt.Text = "Audio downloading.........";
                        }));
                    }
                
            }
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            bDownloadPartOneSuccess = false;
            Stream remoteStream = null;
            Stream localStream = null;
            HttpWebRequest request;
            WebResponse response = null;
            
            long lnFileLength = 0;
            bool bFileExist = false;
            if (File.Exists(fileName))
            {
                lnFileLength = new FileInfo(fileName).Length;
                if (!bLocalFileAdd_1)
                {
                    lTotalLocalFileLength += lnFileLength;
                    prevFileLength = lnFileLength;
                    bLocalFileAdd_1 = true;
                }
                if(prevFileLength < lnFileLength)
                {
                    lTotalLocalFileLength += (lnFileLength - prevFileLength);
                    prevFileLength = lnFileLength;
                }
                if (lnFileLength >= (endRange - startRange) + 1)
                {
                    goto Complete;
                }
                bFileExist = true;
                
            }
            try
            {


                request = (HttpWebRequest)WebRequest.Create(url);
                request.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)";
                request.CookieContainer = new CookieContainer();

                if (bFileExist)
                {
                    request.AddRange(startRange + lnFileLength, endRange);
                }
                else
                {
                    request.AddRange(startRange, endRange);
                }



                if (request != null)
                {

                    response = request.GetResponse();

                    if (response != null)
                    {
                        int byteProcessed = 0;
                        double receivedBytes = 0;
                        double percentage = 0;
                        double totalPercentage = 0;
                        double averageSpeed = 0;
                        remoteStream = response.GetResponseStream();
                        localStream = new FileStream(fileName, FileMode.Append);
                        byte[] buffer = new byte[1024];
                        double TotalBytesToBeReceived = response.ContentLength;
                        int bytesRead;
                        do
                        {
                            if (!Globals.bDownloadCanceled)
                            {

                                //Read data (up to 1k) from the stream
                                bytesRead = remoteStream.Read(buffer, 0, buffer.Length);
                                byteProcessed += bytesRead;
                                receivedBytes = Convert.ToDouble(byteProcessed + lnFileLength);
                                totalByteReceived += bytesRead;
                                totalPercentage = ((totalByteReceived + lTotalLocalFileLength) / remoteFileLength) * 100;
                                totalPercentage = Math.Round(totalPercentage);
                                percentage = (receivedBytes / TotalBytesToBeReceived) * 100;
                                percentage = Math.Round(percentage);
                                averageSpeed = (totalByteReceived / 1024d / stopWatch.Elapsed.TotalSeconds);
                                if (Application.Current.Dispatcher.CheckAccess())
                                    {
                                        ((MainWindow)System.Windows.Application.Current.MainWindow).Dispatcher.Invoke(() => {
                                            ((MainWindow)System.Windows.Application.Current.MainWindow).progress1.Value = percentage;
                                            ((MainWindow)System.Windows.Application.Current.MainWindow).txtBlockDownloaded.Text = Memory.FileSize((totalByteReceived + lTotalLocalFileLength).ToString()) + " ( " + totalPercentage.ToString() + "% )";
                                            ((MainWindow)System.Windows.Application.Current.MainWindow).txtBlockTransferRate.Text = Memory.FileSize( (totalByteReceived / stopWatch.Elapsed.TotalSeconds).ToString()) + "/sec";
                                            ((MainWindow)System.Windows.Application.Current.MainWindow).txtBlockTimeLeft.Text = StringManipulation.TimeLeft((((remoteFileLength / 1024d) - ((totalByteReceived + lTotalLocalFileLength) / 1024d)) / averageSpeed));
                                            ((MainWindow)System.Windows.Application.Current.MainWindow).txtBlockStatus.Text = "Receiving data....";
                                            TaskbarManager.Instance.SetProgressValue((int)totalPercentage, 100);
                                        });

                                    }
                                    else
                                    {
                                        Application.Current.Dispatcher.Invoke(new System.Action(() =>
                                        {
                                            ((MainWindow)System.Windows.Application.Current.MainWindow).progress1.Value = percentage;
                                            ((MainWindow)System.Windows.Application.Current.MainWindow).txtBlockDownloaded.Text = Memory.FileSize((totalByteReceived + lTotalLocalFileLength).ToString()) + " ( " + totalPercentage.ToString() + "% )";
                                            ((MainWindow)System.Windows.Application.Current.MainWindow).txtBlockTransferRate.Text = Memory.FileSize((totalByteReceived / stopWatch.Elapsed.TotalSeconds).ToString()) + "/sec";
                                            ((MainWindow)System.Windows.Application.Current.MainWindow).txtBlockTimeLeft.Text = StringManipulation.TimeLeft((((remoteFileLength / 1024d) - ((totalByteReceived + lTotalLocalFileLength) / 1024d)) / averageSpeed));
                                            ((MainWindow)System.Windows.Application.Current.MainWindow).txtBlockStatus.Text = "Receiving data....";
                                            TaskbarManager.Instance.SetProgressValue((int)totalPercentage, 100);
                                        }));
                                    }
                                
                             
                                //Write the data to the local file
                                localStream.Write(buffer, 0, bytesRead);
                                //Increment total bytes processed
                            }
                            else
                            {
                                
                                if (response != null) response.Close();
                                if (remoteStream != null) remoteStream.Close();
                                if (localStream != null) localStream.Close();
                                
                                stopWatch.Stop();
                                return;
                            }
                            if (token.IsCancellationRequested)
                            {
                                //todo:clean
                                token.ThrowIfCancellationRequested();
                            }
                        } while (bytesRead > 0);


                    }

                }
            }

            catch (Exception Ex)
            {

                if (response != null) response.Close();
                if (remoteStream != null) remoteStream.Close();
                if (localStream != null) localStream.Close();
                
                stopWatch.Stop();
                Thread.Sleep(200);
                if (!Globals.bDownloadCanceled)
                {
                    goto Top;
                }
                else
                {
                    goto Exit;
                }
                    
            }
            finally
            {
                if (response != null) response.Close();
                if (remoteStream != null) remoteStream.Close();
                if (localStream != null) localStream.Close();
                
                stopWatch.Stop();
            }
        Complete:;
           
                if (Application.Current.Dispatcher.CheckAccess())
                {
                    ((MainWindow)System.Windows.Application.Current.MainWindow).progress1.Dispatcher.Invoke(() => { ((MainWindow)System.Windows.Application.Current.MainWindow).progress1.Value = 100; });
                }
                else
                {
                    Application.Current.Dispatcher.Invoke(new System.Action(() =>
                    {
                        ((MainWindow)System.Windows.Application.Current.MainWindow).progress1.Value = 100;
                    }));
                }
            
            
            bDownloadPartOneSuccess = true;
            if(bDownloadPartOneSuccess && bDownloadPartTwoSuccess && bDownloadPartThreeSuccess && bDownloadPartFourSuccess)
            {
               if(Globals.bVideoDownloadRunning)
                {
                    Globals.bVideoDownloadRunning = false;
                    Globals.bVideoDownloadSuccess = true;
                   
                        if (Application.Current.Dispatcher.CheckAccess())
                        {
                            ((MainWindow)System.Windows.Application.Current.MainWindow).Dispatcher.Invoke(() => { ((MainWindow)System.Windows.Application.Current.MainWindow).VideoInfoTxt.Text = "Video download completed."; });
                        }
                        else
                        {
                            Application.Current.Dispatcher.Invoke(new System.Action(() =>
                            {
                                ((MainWindow)System.Windows.Application.Current.MainWindow).VideoInfoTxt.Text = "Video download completed.";
                            }));
                        }
                    
                }
               if(Globals.bAudioDownloadRunning)
                {
                    Globals.bAudioDownloadRunning = false;
                    Globals.bAudioDownloadSuccess = true;
                    
                        if (Application.Current.Dispatcher.CheckAccess())
                        {
                            ((MainWindow)System.Windows.Application.Current.MainWindow).Dispatcher.Invoke(() => { ((MainWindow)System.Windows.Application.Current.MainWindow).AudioInfoTxt.Text = "Audio download completed."; });
                        }
                        else
                        {
                            Application.Current.Dispatcher.Invoke(new System.Action(() =>
                            {
                                ((MainWindow)System.Windows.Application.Current.MainWindow).AudioInfoTxt.Text = "Audio download completed.";
                            }));
                        }
                    
                }
                bLocalFileAdd_1 = false;
                
            }
            Exit:;

        }
        private void downloadPartTwo(CancellationToken token, string url, long startRange, long endRange, string fileName)
        {
            token = TokenSource.Token;
            long prevFileLength = 0;
        Top:
            if (Globals.bVideoDownloadRunning)
            {
                

                    if (Application.Current.Dispatcher.CheckAccess())
                    {
                        ((MainWindow)System.Windows.Application.Current.MainWindow).Dispatcher.Invoke(() => { ((MainWindow)System.Windows.Application.Current.MainWindow).VideoInfoTxt.Text = "Video downloading........."; });
                    }
                    else
                    {
                        Application.Current.Dispatcher.Invoke(new System.Action(() =>
                        {
                            ((MainWindow)System.Windows.Application.Current.MainWindow).VideoInfoTxt.Text = "Video downloading.........";
                        }));
                    }
                
            }
            if (Globals.bAudioDownloadRunning)
            {
               
                    if (Application.Current.Dispatcher.CheckAccess())
                    {
                        ((MainWindow)System.Windows.Application.Current.MainWindow).Dispatcher.Invoke(() => { ((MainWindow)System.Windows.Application.Current.MainWindow).AudioInfoTxt.Text = "Audio downloading........."; });
                    }
                    else
                    {
                        Application.Current.Dispatcher.Invoke(new System.Action(() =>
                        {
                            ((MainWindow)System.Windows.Application.Current.MainWindow).AudioInfoTxt.Text = "Audio downloading.........";
                        }));
                    }
                
            }
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            bDownloadPartTwoSuccess = false;
            Stream remoteStream = null;
            Stream localStream = null;
            HttpWebRequest request;
            WebResponse response = null;
           
           
            long lnFileLength = 0;
            bool bFileExist = false;
            if (File.Exists(fileName))
            {
                lnFileLength = new FileInfo(fileName).Length;
                if (!bLocalFileAdd_2)
                {
                    lTotalLocalFileLength += lnFileLength;
                    prevFileLength = lnFileLength;
                    bLocalFileAdd_2 = true;
                }
                if (prevFileLength < lnFileLength)
                {
                    lTotalLocalFileLength += (lnFileLength - prevFileLength);
                    prevFileLength = lnFileLength;
                }
                if (lnFileLength >= (endRange - startRange) + 1)
                {
                    goto Complete;
                }
                bFileExist = true;
                
            }
            try
            {



                request = (HttpWebRequest)WebRequest.Create(url);
                request.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)";
                if (bFileExist)
                {
                    request.AddRange(startRange + lnFileLength, endRange);
                }
                else
                {
                    request.AddRange(startRange, endRange);
                }
                request.CookieContainer = new CookieContainer();


                if (request != null)
                {

                    response = request.GetResponse();

                    if (response != null)
                    {
                        int byteProcessed = 0;
                        double receivedBytes = 0;
                        double percentage = 0;
                        double totalPercentage = 0;
                        double averageSpeed = 0;
                        remoteStream = response.GetResponseStream();
                        localStream = new FileStream(fileName, FileMode.Append);
                        byte[] buffer = new byte[1024];
                        double TotalBytesToBeReceived = response.ContentLength;
                        int bytesRead;
                        do
                        {
                            if (!Globals.bDownloadCanceled)
                            {
                                //Read data (up to 1k) from the stream
                                bytesRead = remoteStream.Read(buffer, 0, buffer.Length);
                                byteProcessed += bytesRead;
                                receivedBytes = Convert.ToDouble(byteProcessed + lnFileLength);
                                totalByteReceived += bytesRead;
                                totalPercentage = ((totalByteReceived + lTotalLocalFileLength) / remoteFileLength) * 100;
                                totalPercentage = Math.Round(totalPercentage);
                                percentage = (receivedBytes / TotalBytesToBeReceived) * 100;
                                percentage = Math.Round(percentage);
                                averageSpeed = (totalByteReceived / 1024d / stopWatch.Elapsed.TotalSeconds);
                                if (Application.Current.Dispatcher.CheckAccess())
                                    {
                                        ((MainWindow)System.Windows.Application.Current.MainWindow).Dispatcher.Invoke(() =>
                                        {
                                            ((MainWindow)System.Windows.Application.Current.MainWindow).progress2.Value = percentage;
                                        });
                                        ((MainWindow)System.Windows.Application.Current.MainWindow).txtBlockDownloaded.Text = Memory.FileSize((totalByteReceived + lTotalLocalFileLength).ToString()) + " ( " + totalPercentage.ToString() + "% )";
                                        ((MainWindow)System.Windows.Application.Current.MainWindow).txtBlockTransferRate.Text = Memory.FileSize((totalByteReceived / stopWatch.Elapsed.TotalSeconds).ToString()) + "/sec";
                                    ((MainWindow)System.Windows.Application.Current.MainWindow).txtBlockTimeLeft.Text = StringManipulation.TimeLeft((((remoteFileLength / 1024d) - ((totalByteReceived + lTotalLocalFileLength) / 1024d)) / averageSpeed));
                                        ((MainWindow)System.Windows.Application.Current.MainWindow).txtBlockStatus.Text = "Receiving data....";
                                        TaskbarManager.Instance.SetProgressValue((int)totalPercentage, 100);
                                }
                                    else
                                    {
                                        Application.Current.Dispatcher.Invoke(new System.Action(() =>
                                        {
                                            ((MainWindow)System.Windows.Application.Current.MainWindow).progress2.Value = percentage;
                                            ((MainWindow)System.Windows.Application.Current.MainWindow).txtBlockDownloaded.Text = Memory.FileSize((totalByteReceived + lTotalLocalFileLength).ToString()) + " ( " + totalPercentage.ToString() + "% )";
                                            ((MainWindow)System.Windows.Application.Current.MainWindow).txtBlockTransferRate.Text = Memory.FileSize((totalByteReceived / stopWatch.Elapsed.TotalSeconds).ToString()) + "/sec";
                                            ((MainWindow)System.Windows.Application.Current.MainWindow).txtBlockTimeLeft.Text = StringManipulation.TimeLeft((((remoteFileLength / 1024d) - ((totalByteReceived + lTotalLocalFileLength) / 1024d)) / averageSpeed));
                                            ((MainWindow)System.Windows.Application.Current.MainWindow).txtBlockStatus.Text = "Receiving data....";
                                            TaskbarManager.Instance.SetProgressValue((int)totalPercentage, 100);
                                        }));
                                    }
                                
                                //Write the data to the local file
                                 localStream.Write(buffer, 0, bytesRead);
                                //Increment total bytes processed
                            }
                            else
                            {

                                if (response != null) response.Close();
                                if (remoteStream != null) remoteStream.Close();
                                if (localStream != null) localStream.Close();
                               
                                stopWatch.Stop();
                                return;
                            }
                            if (token.IsCancellationRequested)
                            {
                                //todo:clean
                                token.ThrowIfCancellationRequested();
                            }
                        } while (bytesRead > 0);


                    }

                }
            }

            catch (Exception Ex)
            {
                if (response != null) response.Close();
                if (remoteStream != null) remoteStream.Close();
                if (localStream != null) localStream.Close();
               
                stopWatch.Stop();
                Thread.Sleep(200);
                if (!Globals.bDownloadCanceled)
                {
                    goto Top;
                }
                else
                {
                    goto Exit;
                }
            }
            finally
            {
                if (response != null) response.Close();
                if (remoteStream != null) remoteStream.Close();
                if (localStream != null) localStream.Close();
                
                stopWatch.Stop();
            }
        Complete:;
            
                if (Application.Current.Dispatcher.CheckAccess())
                {
                    ((MainWindow)System.Windows.Application.Current.MainWindow).progress2.Dispatcher.Invoke(() => { ((MainWindow)System.Windows.Application.Current.MainWindow).progress2.Value = 100; });
                }
                else
                {
                    Application.Current.Dispatcher.Invoke(new System.Action(() =>
                    {
                        ((MainWindow)System.Windows.Application.Current.MainWindow).progress2.Value = 100;
                    }));
                }
            
            
            bDownloadPartTwoSuccess = true;
            if (bDownloadPartOneSuccess && bDownloadPartTwoSuccess && bDownloadPartThreeSuccess && bDownloadPartFourSuccess)
            {
                if (Globals.bVideoDownloadRunning)
                {
                    Globals.bVideoDownloadRunning = false;
                    Globals.bVideoDownloadSuccess = true;
                   
                        if (Application.Current.Dispatcher.CheckAccess())
                        {
                            ((MainWindow)System.Windows.Application.Current.MainWindow).Dispatcher.Invoke(() => { ((MainWindow)System.Windows.Application.Current.MainWindow).VideoInfoTxt.Text = "Video download completed."; });
                        }
                        else
                        {
                            Application.Current.Dispatcher.Invoke(new System.Action(() =>
                            {
                                ((MainWindow)System.Windows.Application.Current.MainWindow).VideoInfoTxt.Text = "Video download completed.";
                            }));
                        }
                    
                }
                if (Globals.bAudioDownloadRunning)
                {
                    Globals.bAudioDownloadRunning = false;
                    Globals.bAudioDownloadSuccess = true;
                   
                        if (Application.Current.Dispatcher.CheckAccess())
                        {
                            ((MainWindow)System.Windows.Application.Current.MainWindow).Dispatcher.Invoke(() => { ((MainWindow)System.Windows.Application.Current.MainWindow).AudioInfoTxt.Text = "Audio download completed."; });
                        }
                        else
                        {
                            Application.Current.Dispatcher.Invoke(new System.Action(() =>
                            {
                                ((MainWindow)System.Windows.Application.Current.MainWindow).AudioInfoTxt.Text = "Audio download completed.";
                            }));
                        }
                    
                }
                bLocalFileAdd_2 = false;
               
            }
           Exit:;
        }
        private void downloadPartThree(CancellationToken token, string url, long startRange, long endRange, string fileName)
        {
            token = TokenSource.Token;
            long prevFileLength = 0;
        Top:
            if (Globals.bVideoDownloadRunning)
            {
               
                    if (Application.Current.Dispatcher.CheckAccess())
                    {
                        ((MainWindow)System.Windows.Application.Current.MainWindow).Dispatcher.Invoke(() => { ((MainWindow)System.Windows.Application.Current.MainWindow).VideoInfoTxt.Text = "Video downloading........."; });
                    }
                    else
                    {
                        Application.Current.Dispatcher.Invoke(new System.Action(() =>
                        {
                            ((MainWindow)System.Windows.Application.Current.MainWindow).VideoInfoTxt.Text = "Video downloading.........";
                        }));
                    }
                
            }
            if (Globals.bAudioDownloadRunning)
            {
                
                    if (Application.Current.Dispatcher.CheckAccess())
                    {
                        ((MainWindow)System.Windows.Application.Current.MainWindow).Dispatcher.Invoke(() => { ((MainWindow)System.Windows.Application.Current.MainWindow).AudioInfoTxt.Text = "Audio downloading........."; });
                    }
                    else
                    {
                        Application.Current.Dispatcher.Invoke(new System.Action(() =>
                        {
                            ((MainWindow)System.Windows.Application.Current.MainWindow).AudioInfoTxt.Text = "Audio downloading.........";
                        }));
                    }
                
            }
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            bDownloadPartThreeSuccess = false;
            Stream remoteStream = null;
            Stream localStream = null;
            HttpWebRequest request;
            WebResponse response = null;
            
            long lnFileLength = 0;
            bool bFileExist = false;
            if (File.Exists(fileName))
            {
                lnFileLength = new FileInfo(fileName).Length;
                if (!bLocalFileAdd_3)
                {
                    lTotalLocalFileLength += lnFileLength;
                    prevFileLength = lnFileLength;
                    bLocalFileAdd_3 = true;
                }
                if (prevFileLength < lnFileLength)
                {
                    lTotalLocalFileLength += (lnFileLength - prevFileLength);
                    prevFileLength = lnFileLength;
                }
                if (lnFileLength >= (endRange - startRange) + 1)

                {
                    goto Complete;
                }
                bFileExist = true;
                
            }
            try
            {


                request = (HttpWebRequest)WebRequest.Create(url);
                request.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)";
                if (bFileExist)
                {
                    request.AddRange(startRange + lnFileLength, endRange);
                }
                else
                {
                    request.AddRange(startRange, endRange);
                }
                request.CookieContainer = new CookieContainer();


                if (request != null)
                {

                    response = request.GetResponse();

                    if (response != null)
                    {
                        int byteProcessed = 0;
                        double receivedBytes = 0;
                        double percentage = 0;
                        double totalPercentage = 0;
                        double averageSpeed = 0;
                        remoteStream = response.GetResponseStream();
                        localStream = new FileStream(fileName, FileMode.Append);
                        byte[] buffer = new byte[1024];
                        double TotalBytesToBeReceived = response.ContentLength;
                        int bytesRead;
                        do
                        {
                            if (!Globals.bDownloadCanceled)
                            {


                                //Read data (up to 1k) from the stream
                                bytesRead = remoteStream.Read(buffer, 0, buffer.Length);
                                byteProcessed += bytesRead;
                                receivedBytes = Convert.ToDouble(byteProcessed + lnFileLength);
                                totalByteReceived += bytesRead;
                                totalPercentage = ((totalByteReceived + lTotalLocalFileLength) / remoteFileLength) * 100;
                                totalPercentage = Math.Round(totalPercentage);
                                percentage = (receivedBytes / TotalBytesToBeReceived) * 100;
                                percentage = Math.Round(percentage);
                                averageSpeed = (totalByteReceived / 1024d / stopWatch.Elapsed.TotalSeconds);
                                if (Application.Current.Dispatcher.CheckAccess())
                                    {
                                        ((MainWindow)System.Windows.Application.Current.MainWindow).Dispatcher.Invoke(() => { ((MainWindow)System.Windows.Application.Current.MainWindow).progress3.Value = percentage; });
                                        ((MainWindow)System.Windows.Application.Current.MainWindow).txtBlockDownloaded.Text = Memory.FileSize((totalByteReceived + lTotalLocalFileLength).ToString()) + " ( " + totalPercentage.ToString() + "% )";
                                        ((MainWindow)System.Windows.Application.Current.MainWindow).txtBlockTransferRate.Text = Memory.FileSize((totalByteReceived / stopWatch.Elapsed.TotalSeconds).ToString()) + "/sec";
                                    ((MainWindow)System.Windows.Application.Current.MainWindow).txtBlockTimeLeft.Text = StringManipulation.TimeLeft((((remoteFileLength / 1024d) - ((totalByteReceived + lTotalLocalFileLength) / 1024d)) / averageSpeed));
                                        ((MainWindow)System.Windows.Application.Current.MainWindow).txtBlockStatus.Text = "Receiving data....";
                                        TaskbarManager.Instance.SetProgressValue((int)totalPercentage, 100);
                                }
                                    else
                                    {
                                        Application.Current.Dispatcher.Invoke(new System.Action(() =>
                                        {
                                            ((MainWindow)System.Windows.Application.Current.MainWindow).progress3.Value = percentage;
                                            ((MainWindow)System.Windows.Application.Current.MainWindow).txtBlockDownloaded.Text = Memory.FileSize((totalByteReceived + lTotalLocalFileLength).ToString()) + " ( " + totalPercentage.ToString() + "% )";
                                            ((MainWindow)System.Windows.Application.Current.MainWindow).txtBlockTransferRate.Text = Memory.FileSize((totalByteReceived / stopWatch.Elapsed.TotalSeconds).ToString()) + "/sec";
                                            ((MainWindow)System.Windows.Application.Current.MainWindow).txtBlockTimeLeft.Text = StringManipulation.TimeLeft((((remoteFileLength / 1024d) - ((totalByteReceived + lTotalLocalFileLength) / 1024d)) / averageSpeed));
                                            ((MainWindow)System.Windows.Application.Current.MainWindow).txtBlockStatus.Text = "Receiving data....";
                                            TaskbarManager.Instance.SetProgressValue((int)totalPercentage, 100);
                                        }));
                                    }
                                
                                //Write the data to the local file
                                localStream.Write(buffer, 0, bytesRead);
                                //Increment total bytes processed
                            }
                            else
                            {

                                if (response != null) response.Close();
                                if (remoteStream != null) remoteStream.Close();
                                if (localStream != null) localStream.Close();
                               
                                stopWatch.Stop();
                                return;
                            }
                            if (token.IsCancellationRequested)
                            {
                                //todo:clean
                                token.ThrowIfCancellationRequested();
                            }
                        } while (bytesRead > 0);


                    }

                }
            }

            catch (Exception Ex)
            {
               
                if (response != null) response.Close();
                if (remoteStream != null) remoteStream.Close();
                if (localStream != null) localStream.Close();
                
                stopWatch.Stop();
                Thread.Sleep(200);
                if (!Globals.bDownloadCanceled)
                {
                    goto Top;
                }
                else
                {
                    goto Exit;
                }
            }
            finally
            {
                if (response != null) response.Close();
                if (remoteStream != null) remoteStream.Close();
                if (localStream != null) localStream.Close();
              
                stopWatch.Stop();
            }
        Complete:;
            

                if (Application.Current.Dispatcher.CheckAccess())
                {
                    ((MainWindow)System.Windows.Application.Current.MainWindow).progress3.Dispatcher.Invoke(() => { ((MainWindow)System.Windows.Application.Current.MainWindow).progress3.Value = 100; });
                }
                else
                {
                    Application.Current.Dispatcher.Invoke(new System.Action(() =>
                    {
                        ((MainWindow)System.Windows.Application.Current.MainWindow).progress3.Value = 100;
                    }));
                }
            
            
            bDownloadPartThreeSuccess = true;
            if (bDownloadPartOneSuccess && bDownloadPartTwoSuccess && bDownloadPartThreeSuccess && bDownloadPartFourSuccess)
            {
                if (Globals.bVideoDownloadRunning)
                {
                    Globals.bVideoDownloadRunning = false;
                    Globals.bVideoDownloadSuccess = true;
                    if (Application.Current.Dispatcher.CheckAccess())
                    {
                        ((MainWindow)System.Windows.Application.Current.MainWindow).Dispatcher.Invoke(() => { ((MainWindow)System.Windows.Application.Current.MainWindow).VideoInfoTxt.Text = "Video download completed."; });
                    }
                    else
                    {
                        Application.Current.Dispatcher.Invoke(new System.Action(() =>
                        {
                            ((MainWindow)System.Windows.Application.Current.MainWindow).VideoInfoTxt.Text = "Video download completed.";
                        }));
                    }
                }
                if (Globals.bAudioDownloadRunning)
                {
                    Globals.bAudioDownloadRunning = false;
                    Globals.bAudioDownloadSuccess = true;
                    if (Application.Current.Dispatcher.CheckAccess())
                    {
                        ((MainWindow)System.Windows.Application.Current.MainWindow).Dispatcher.Invoke(() => { ((MainWindow)System.Windows.Application.Current.MainWindow).AudioInfoTxt.Text = "Audio download completed."; });
                    }
                    else
                    {
                        Application.Current.Dispatcher.Invoke(new System.Action(() =>
                        {
                            ((MainWindow)System.Windows.Application.Current.MainWindow).AudioInfoTxt.Text = "Audio download completed.";
                        }));
                    }
                }
                bLocalFileAdd_3 = false;
                
            }
            Exit:;
        }
        private void downloadPartFour(CancellationToken token, string url, long startRange, long endRange, string fileName)
        {
            token = TokenSource.Token;
            long prevFileLength = 0;
        Top:
            if (Globals.bVideoDownloadRunning)
            {
                
                    if (Application.Current.Dispatcher.CheckAccess())
                    {
                        ((MainWindow)System.Windows.Application.Current.MainWindow).Dispatcher.Invoke(() => { ((MainWindow)System.Windows.Application.Current.MainWindow).VideoInfoTxt.Text = "Video downloading........."; });
                    }
                    else
                    {
                        Application.Current.Dispatcher.Invoke(new System.Action(() =>
                        {
                            ((MainWindow)System.Windows.Application.Current.MainWindow).VideoInfoTxt.Text = "Video downloading.........";
                        }));
                    }
                

              
            }
            if (Globals.bAudioDownloadRunning)
            {
               
                    if (Application.Current.Dispatcher.CheckAccess())
                    {
                        ((MainWindow)System.Windows.Application.Current.MainWindow).Dispatcher.Invoke(() => { ((MainWindow)System.Windows.Application.Current.MainWindow).AudioInfoTxt.Text = "Audio downloading........."; });
                    }
                    else
                    {
                        Application.Current.Dispatcher.Invoke(new System.Action(() =>
                        {
                            ((MainWindow)System.Windows.Application.Current.MainWindow).AudioInfoTxt.Text = "Audio downloading.........";
                        }));
                    }
                
            }
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            bDownloadPartFourSuccess = false;
            Stream remoteStream = null;
            Stream localStream = null;
            HttpWebRequest request;
            WebResponse response = null;
            
            long lnFileLength = 0;
            bool bFileExist = false;
            if (File.Exists(fileName))
            {
                lnFileLength = new FileInfo(fileName).Length;
                if (!bLocalFileAdd_4)
                {
                    lTotalLocalFileLength += lnFileLength;
                    prevFileLength = lnFileLength;
                    bLocalFileAdd_4 = true;
                }
                if (prevFileLength < lnFileLength)
                {
                    lTotalLocalFileLength += (lnFileLength - prevFileLength);
                    prevFileLength = lnFileLength;
                }
                if (lnFileLength >= (endRange - startRange))
                {

                    goto Complete;

                }
                bFileExist = true;
               
            }
            try
            {

                request = (HttpWebRequest)WebRequest.Create(url);
                request.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)";
                if (bFileExist)
                {
                    request.AddRange(startRange + lnFileLength, endRange);
                }
                else
                {
                    request.AddRange(startRange, endRange);
                }
                request.CookieContainer = new CookieContainer();


                if (request != null)
                {

                    response = request.GetResponse();

                    if (response != null)
                    {
                        int byteProcessed = 0;
                        double receivedBytes = 0;
                        double percentage = 0;
                        double totalPercentage = 0;
                        double averageSpeed = 0;
                        remoteStream = response.GetResponseStream();
                        localStream = new FileStream(fileName, FileMode.Append);
                        byte[] buffer = new byte[1024];
                        double TotalBytesToBeReceived = response.ContentLength;
                        int bytesRead;
                        do
                        {
                            if (!Globals.bDownloadCanceled)
                            {

                                //Read data (up to 1k) from the stream
                                bytesRead = remoteStream.Read(buffer, 0, buffer.Length);
                                byteProcessed += bytesRead;
                                receivedBytes = Convert.ToDouble(byteProcessed + lnFileLength);
                                totalByteReceived += bytesRead;
                                totalPercentage = ((totalByteReceived + lTotalLocalFileLength) / remoteFileLength) * 100;
                                totalPercentage = Math.Round(totalPercentage);
                                percentage = (receivedBytes / TotalBytesToBeReceived) * 100;
                                percentage = Math.Round(percentage);
                                averageSpeed = (totalByteReceived / 1024d / stopWatch.Elapsed.TotalSeconds);

                                if (Application.Current.Dispatcher.CheckAccess())
                                    {
                                        ((MainWindow)System.Windows.Application.Current.MainWindow).Dispatcher.Invoke(() => { ((MainWindow)System.Windows.Application.Current.MainWindow).progress4.Value = percentage; });
                                        ((MainWindow)System.Windows.Application.Current.MainWindow).txtBlockDownloaded.Text = Memory.FileSize((totalByteReceived + lTotalLocalFileLength).ToString()) + " ( " + totalPercentage.ToString() + "% )";
                                        ((MainWindow)System.Windows.Application.Current.MainWindow).txtBlockTransferRate.Text = Memory.FileSize((totalByteReceived / stopWatch.Elapsed.TotalSeconds).ToString()) + "/sec";
                                    ((MainWindow)System.Windows.Application.Current.MainWindow).txtBlockTimeLeft.Text = StringManipulation.TimeLeft((((remoteFileLength / 1024d) - ((totalByteReceived + lTotalLocalFileLength) / 1024d)) / averageSpeed));
                                        ((MainWindow)System.Windows.Application.Current.MainWindow).txtBlockStatus.Text = "Receiving data....";
                                        TaskbarManager.Instance.SetProgressValue((int)totalPercentage, 100);
                                }
                                    else
                                    {
                                        Application.Current.Dispatcher.Invoke(new System.Action(() =>
                                            {
                                                ((MainWindow)System.Windows.Application.Current.MainWindow).progress4.Value = percentage;
                                                ((MainWindow)System.Windows.Application.Current.MainWindow).txtBlockDownloaded.Text = Memory.FileSize((totalByteReceived + lTotalLocalFileLength).ToString()) + " ( " + totalPercentage.ToString() + "% )";
                                                ((MainWindow)System.Windows.Application.Current.MainWindow).txtBlockTransferRate.Text = Memory.FileSize((totalByteReceived / stopWatch.Elapsed.TotalSeconds).ToString()) + "/sec";
                                                ((MainWindow)System.Windows.Application.Current.MainWindow).txtBlockTimeLeft.Text = StringManipulation.TimeLeft((((remoteFileLength / 1024d) - ((totalByteReceived + lTotalLocalFileLength) / 1024d)) / averageSpeed));
                                                ((MainWindow)System.Windows.Application.Current.MainWindow).txtBlockStatus.Text = "Receiving data....";
                                                TaskbarManager.Instance.SetProgressValue((int)totalPercentage, 100);
                                            }));
                                    }
                            
                                //Write the data to the local file
                                localStream.Write(buffer, 0, bytesRead);
                                //Increment total bytes processed
                            }
                            else
                            {

                                if (response != null) response.Close();
                                if (remoteStream != null) remoteStream.Close();
                                if (localStream != null) localStream.Close();
                                
                                stopWatch.Stop();
                                return;
                            }
                            if (token.IsCancellationRequested)
                            {
                                //todo:clean
                                token.ThrowIfCancellationRequested();
                            }
                        } while (bytesRead > 0);


                    }

                }
            }

            catch (Exception Ex)
            {
               
                if (response != null) response.Close();
                if (remoteStream != null) remoteStream.Close();
                if (localStream != null) localStream.Close();
               
                stopWatch.Stop();
                Thread.Sleep(200);
                if (!Globals.bDownloadCanceled)
                {
                    goto Top;
                }
                else
                {
                    goto Exit;
                }
            }
            finally
            {
                if (response != null) response.Close();
                if (remoteStream != null) remoteStream.Close();
                if (localStream != null) localStream.Close();
               
                stopWatch.Stop();
            }
        Complete:;
           
                if (Application.Current.Dispatcher.CheckAccess())
                {
                    ((MainWindow)System.Windows.Application.Current.MainWindow).progress4.Dispatcher.Invoke(() => { ((MainWindow)System.Windows.Application.Current.MainWindow).progress4.Value = 100; });
                }
                else
                {
                    Application.Current.Dispatcher.Invoke(new System.Action(() =>
                    {
                        ((MainWindow)System.Windows.Application.Current.MainWindow).progress4.Value = 100;
                    }));
                }
            

            bDownloadPartFourSuccess = true;
            if (bDownloadPartOneSuccess && bDownloadPartTwoSuccess && bDownloadPartThreeSuccess && bDownloadPartFourSuccess)
            {
                if (Globals.bVideoDownloadRunning)
                {
                    Globals.bVideoDownloadRunning = false;
                    Globals.bVideoDownloadSuccess = true;
                    

                        if (Application.Current.Dispatcher.CheckAccess())
                        {
                            ((MainWindow)System.Windows.Application.Current.MainWindow).Dispatcher.Invoke(() => { ((MainWindow)System.Windows.Application.Current.MainWindow).VideoInfoTxt.Text = "Video download completed."; });
                        }
                        else
                        {
                            Application.Current.Dispatcher.Invoke(new System.Action(() =>
                            {
                                ((MainWindow)System.Windows.Application.Current.MainWindow).VideoInfoTxt.Text = "Video download completed.";
                            }));
                        }
                    
                    
                }
                if (Globals.bAudioDownloadRunning)
                {
                    Globals.bAudioDownloadRunning = false;
                    Globals.bAudioDownloadSuccess = true;
                   
                        if (Application.Current.Dispatcher.CheckAccess())
                        {
                            ((MainWindow)System.Windows.Application.Current.MainWindow).Dispatcher.Invoke(() => { ((MainWindow)System.Windows.Application.Current.MainWindow).AudioInfoTxt.Text = "Audio download completed."; });
                        }
                        else
                        {
                            Application.Current.Dispatcher.Invoke(new System.Action(() =>
                            {
                                ((MainWindow)System.Windows.Application.Current.MainWindow).AudioInfoTxt.Text = "Audio download completed.";
                            }));
                        }
                    
                    
                }
                bLocalFileAdd_4 = false;
            }
        Exit:;
        }
        
    }
}
