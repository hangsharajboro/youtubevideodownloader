using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YoutubeExplode.Videos.Streams;

namespace YoutubeDownloader.BLL
{
    internal class BusinessLogicLayer
    {
        public string url { get; set; }
        public string label { get; set; }
        public string quality { get; set; }
        public string filename { get; set; }
    }
}
