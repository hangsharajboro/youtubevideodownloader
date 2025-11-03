
using System.IO;

using System.Windows;

namespace YoutubeDownloader
{
    internal class fileConcate
    {
        bool bVideoMergeSuccess = false;
        bool bAudioMergeSuccess = false;
        public void VideoConcate(string outputFile)
        {
            bVideoMergeSuccess = false;
            string extension = System.IO.Path.GetExtension(outputFile);
            string filename = System.IO.Path.GetFileName(outputFile);
            string finishedFileName = pureFilename(filename, extension);
            string directory = System.IO.Path.GetDirectoryName(outputFile);
            if (Directory.Exists(directory))
            {

                for (int i = 2; i <= 4; i++)
                {

                    if (File.Exists(directory + @"\" + finishedFileName + "_part1" + extension))
                    {


                        if (File.Exists(directory + @"\" + finishedFileName + "_part" + i.ToString() + extension))
                        {
                            mergeFiles(directory + @"\" + finishedFileName + "_part1" + extension, directory + @"\" + finishedFileName + "_part" + i.ToString() + extension);
                            bVideoMergeSuccess = true;
                        }

                    }

                }
                for (int i = 2; i <= 4; i++)
                {

                    if (File.Exists(directory + @"\" + finishedFileName + "_part" + i.ToString() + extension))
                    {
                        File.Delete(directory + @"\" + finishedFileName + "_part" + i.ToString() + extension);
                    }



                }

            }
            if (bVideoMergeSuccess)
            {
                Globals.bVideoFileMergeSuccess = true;
            }

        }
        public void AudioConcate(string outputFile)
        {
            bAudioMergeSuccess = false;
            string extension = System.IO.Path.GetExtension(outputFile);
            string filename = System.IO.Path.GetFileName(outputFile);
            string finishedFileName = pureFilename(filename, extension);
            string directory = System.IO.Path.GetDirectoryName(outputFile);
            if (Directory.Exists(directory))
            {

                for (int i = 2; i <= 4; i++)
                {

                    if (File.Exists(directory + @"\" + finishedFileName + "_part1" + extension))
                    {


                        if (File.Exists(directory + @"\" + finishedFileName + "_part" + i.ToString() + extension))
                        {
                            mergeFiles(directory + @"\" + finishedFileName + "_part1" + extension, directory + @"\" + finishedFileName + "_part" + i.ToString() + extension);
                            bAudioMergeSuccess = true;
                        }

                    }

                }
                for (int i = 2; i <= 4; i++)
                {

                    if (File.Exists(directory + @"\" + finishedFileName + "_part" + i.ToString() + extension))
                    {
                        File.Delete(directory + @"\" + finishedFileName + "_part" + i.ToString() + extension);
                    }



                }

            }
            if (bAudioMergeSuccess)
            {
                Globals.bAudioFileMergeSuccess = true;
            }

        }
        private void mergeFiles(string ouputFile, string inputFile)
        {

            Stream finalFile = null;
            Stream rawFile = null;
            rawFile = new FileStream(inputFile, FileMode.Open, FileAccess.Read);
            finalFile = new FileStream(ouputFile, FileMode.Append);
            byte[] buff = new byte[rawFile.Length];
            int byteRead;
            byteRead = rawFile.Read(buff, 0, buff.Length);
            finalFile.Write(buff, 0, byteRead);
            rawFile.Close();
            finalFile.Close();
        }
        public static string pureFilename(string strFileName, string extension)
        {
            strFileName = Uri.UnescapeDataString(strFileName);
            if (strFileName != null)
            {
                if (strFileName.Contains(extension))
                {
                    string reversedFilename = StringManipulation.Reverse(strFileName);
                    int start, end;
                    string strStart = extension.Trim();
                    start = reversedFilename.IndexOf(strStart, 0) + strStart.Length + 1;
                    end = reversedFilename.Length;
                    strFileName = reversedFilename.Substring(start, end - start);
                    start = 0; end = 0;
                    strStart = "";
                    strStart = "_part1";
                    end = strFileName.Length;
                    start = strFileName.IndexOf(strStart, 0) + strStart.Length + 1;
                    strFileName = strFileName.Substring(start, end - start);
                    strFileName = StringManipulation.Reverse(strFileName);
                }
                else
                {
                    MessageBox.Show("Filename don't have extension ");
                }

            }
            else
            {
                MessageBox.Show("File name is null.");
            }

            return strFileName;

        }
    }
}
