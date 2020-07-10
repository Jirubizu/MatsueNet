﻿using System.IO;

namespace MatsueNet.Utils
{
    public static class StreamUtils
    {
        public static Stream GetStreamFromUrl(string url)
        {
            byte[] imageData = null;

            using (var wc = new System.Net.WebClient())
            {
                imageData = wc.DownloadData(url);
            }
            return new MemoryStream(imageData);
        }
    }
}