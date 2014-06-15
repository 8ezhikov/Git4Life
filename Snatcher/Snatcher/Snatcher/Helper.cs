using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace Snatcher
{
    class Helper
    {
        public static HtmlDocument GetDocumentCustomMode(Encoding encoding, string url)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.2; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/32.0.1667.0 Safari/537.36";
            var resultingDocument = new HtmlDocument();

            Stream resultingStream = new MemoryStream();
            try
            {
                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    if (response.Headers["transfer-encoding"] == "chunked")
                    {
                        var sb = new StringBuilder();
                        var buf = new byte[8192];
                        var resStream = response.GetResponseStream();
                        var count = 0;
                        do
                        {
                            if (resStream != null) count = resStream.Read(buf, 0, buf.Length);
                            if (count != 0)
                            {
                                var tmpString = encoding.GetString(buf, 0, count);
                                sb.Append(tmpString);
                            }
                        } while (count > 0);
                        var resultingString = sb.ToString();
                        if (Equals(encoding, Encoding.GetEncoding("windows-1251")))
                        {
                            resultingString = resultingString.Replace("win-1251", "UTF-8");
                        }
                        resultingDocument.LoadHtml(resultingString);

                    }
                    else
                    {
                        using (var responseStream = response.GetResponseStream())
                        {
                            var buffer = new byte[0x1000];
                            int bytes;
                            while (responseStream != null && (bytes = responseStream.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                resultingStream.Write(buffer, 0, bytes);
                            }
                        }

                        resultingStream.Seek(0, SeekOrigin.Begin);
                        using (var sr = new StreamReader(resultingStream, encoding))
                        {
                            resultingDocument.LoadHtml(sr.ReadToEnd());
                        }

                    }
                }
                return resultingDocument;
            }
            catch (Exception)
            {
                return new HtmlDocument();
            }
        }

        public static HtmlDocument GetPageFromUrl(string url)
        {
            var htmlWeb = new HtmlWeb();

            HtmlDocument doc;

            try
            {
                doc = htmlWeb.Load(url);
                if (doc.ParseErrors.Any(error => error.Code == HtmlParseErrorCode.CharsetMismatch))
                {
                    doc = GetDocumentCustomMode(doc.Encoding, url);
                }
            }
            catch (Exception)
            {
                doc = GetDocumentCustomMode(Encoding.GetEncoding("windows-1251"), url);
            }

            return doc;
        }
    }
}
