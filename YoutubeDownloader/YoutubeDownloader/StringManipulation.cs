using System.Windows;
namespace YoutubeDownloader
{
    class StringManipulation
    {
        public static string Reverse(string inputString)
        {
            string outputString = "";
            if (!(inputString.Length < 0))
            {

                for (int count = inputString.Length - 1; count >= 0; count--)
                {
                    outputString += inputString[count];
                }
                return outputString;
            }
            else
            {
                return outputString;
            }


        }
        public static string FilenameExtension(string strFileName)
        {
            strFileName = Uri.UnescapeDataString(strFileName);
            if (strFileName != null)
            {
                if(strFileName.Contains("/"))
                {
                    string reversedFilename = StringManipulation.Reverse(strFileName);
                    int start, end;
                    string strStart = "", strEnd = "/";
                    start = reversedFilename.IndexOf(strStart, 0) + strStart.Length;
                    end = reversedFilename.IndexOf(strEnd, start);
                    strFileName = reversedFilename.Substring(start, end - start);
                    strFileName += ".";
                    strFileName = StringManipulation.Reverse(strFileName);
                }
                else
                {
                    MessageBox.Show("Filename don't have / ");
                }
                
            }
            else
            {
                MessageBox.Show("File name is null.");
            }

            return strFileName;

        }
       
        public static string TimeLeft(double secondLeft)
        {
            string strSize = "";
            if (secondLeft > 0)
            {
               
                if (secondLeft < 60)
                {
                    secondLeft = Math.Round(secondLeft);
                    return secondLeft.ToString() + " Second";
                }
                else
                {
                    secondLeft = secondLeft / 60;
                    if (secondLeft < 60)
                    {
                        secondLeft = Math.Round(secondLeft);
                        return secondLeft.ToString() + " Minute";
                    }
                    else if (secondLeft >= 60)
                    {
                        secondLeft = secondLeft / 60;
                        if (secondLeft < 24)
                        {
                            secondLeft = Math.Round(secondLeft);//Rounding figure with 2 precision point
                            return secondLeft.ToString() + " Hours";
                        }
                        else if (secondLeft >= 24)
                        {
                            secondLeft = secondLeft / 24;
                            if (secondLeft < 7)
                            {
                                secondLeft = Math.Round(secondLeft);//Rounding figure with 2 precision point
                                return secondLeft.ToString() + " Days";
                            }
                            else if (secondLeft >= 7)
                            {
                                secondLeft = secondLeft / 7;
                                secondLeft = Math.Round(secondLeft);//Rounding figure with 2 precision point
                                return secondLeft.ToString() + " Weeks";
                            }
                        }
                    }

                }

            }
            return strSize;

        }
        public static string ReplaceInvalidChars(string filename)
        {
            return string.Join("_", filename.Split(System.IO.Path.GetInvalidFileNameChars()));
        }
    }
}
