

namespace YoutubeDownloader
{
    public class Memory
    {
        public static string FileSize(string strBytes)
        {
            string strSize = "";
            if (strBytes != "")
            {
                double dSize = Convert.ToDouble(strBytes);
                if (dSize < 1024)
                {
                    return dSize.ToString() + " Bytes";
                }
                else
                {
                    dSize = dSize / 1024;
                    if (dSize < 1024)
                    {
                        dSize = Math.Round(dSize, 2);//Rounding figure with 2 precision point
                        return dSize.ToString() + " KB";
                    }
                    else if (dSize >= 1024)
                    {
                        dSize = dSize / 1024;
                        if (dSize < 1024)
                        {
                            dSize = Math.Round(dSize, 2);//Rounding figure with 2 precision point
                            return dSize.ToString() + " MB";
                        }
                        else if (dSize >= 1024)
                        {
                            dSize = dSize / 1024;
                            if (dSize < 1024)
                            {
                                dSize = Math.Round(dSize, 2);//Rounding figure with 2 precision point
                                return dSize.ToString() + " GB";
                            }
                            else if (dSize >= 1024)
                            {
                                dSize = dSize / 1024;
                                dSize = Math.Round(dSize, 2);//Rounding figure with 2 precision point
                                return dSize.ToString() + " TB";
                            }
                        }
                    }

                }

            }
            return strSize;

        }
    }
}
