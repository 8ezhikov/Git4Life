using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Drawing;

namespace Snatcher
{
    class SnatchEngine
    {

        public List<ProductDescriptor> Process()
        {

            var URL = "http://geotea.ru/catalog/dishes/ceremony";

            var catalogDocument = GetPageFromURL(SnatchSettings.catalogURL);


            var linksList = catalogDocument.DocumentNode.SelectNodes("//a[@href]");

            var filteredLinkList = linksList.Where(lr => lr.OuterHtml.Contains(SnatchSettings.categoryContains));
            var listOfProductUrls = new List<string>();
            foreach (var link in filteredLinkList)
            {
                var attrib = link.Attributes["href"];
                var rawLinkUrl = attrib.Value;
                listOfProductUrls.Add( AppendSiteName(rawLinkUrl));
           

            }
            var listOfProductDescr = new List<ProductDescriptor>();
            foreach (var prodUrl in listOfProductUrls)
            {
                var productDocument = GetPageFromURL(prodUrl);
                var product = new ProductDescriptor();

                product.product_name = productDocument.DocumentNode.SelectSingleNode("/html[1]/body[1]/div[2]/div[1]/div[1]/div[1]/h1[1]").LastChild.OuterHtml.Replace(" &rarr;", "").Trim();

                product.description = productDocument.DocumentNode.SelectSingleNode("/html/body/div[@class='product-page-wrapper']/div[@class='block clear']/div[@class='twocol-content-wrapper']/div[@class='content']/dl[@class='tea-options']").InnerHtml;
                product.price = productDocument.DocumentNode.SelectNodes("//p[1]").First(t => t.InnerText.Contains("Цена:")).InnerText.Replace("Цена:","").Trim();
                var imgNode =productDocument.DocumentNode.SelectSingleNode("//div[@class='product-top clear']/div[@class='big-img']/img[@class='main-img']/@src");
                if (imgNode != null)
                {
                    var imgRawURL = imgNode.Attributes["src"].Value;

                    var filePathCount = imgRawURL.Reverse().SkipWhile(str => str != '/').Count();
                    var imgFileName = imgRawURL.Substring( filePathCount);

                   if(imgNode.Attributes["alt"]!=null)
                    {
                        product.image_label = imgNode.Attributes["alt"].Value;
                        
                    }
                    string localFilename = @"D:\Snatches\"+imgFileName;
                    using (WebClient client = new WebClient())
                    {
                        client.DownloadFile(new Uri(AppendSiteName(imgRawURL)) , localFilename);
                    }
                }

                //WebRequest req = WebRequest.Create("https://appharbor.com/assets/images/stackoverflow-logo.png");
                //WebResponse response = req.GetResponse();
                //Stream stream = response.GetResponseStream();
                //System.Drawing.Image image = System.Drawing.Image.FromStream(stream);
                //using (MemoryStream ms = new MemoryStream())
                //{
                //    image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                //    ms.WriteTo(Response.OutputStream);
                //}
                //string localFilename = @"c:\localpath\tofile.jpg";
                //using (WebClient client = new WebClient())
                //{
                //    client.DownloadFile("http://www.example.com/image.jpg", localFilename);
                //}
                
                
                #region meta
                var descNode = productDocument.DocumentNode.SelectSingleNode("//meta[@name='description']");

                if (descNode != null)
                {
                    var desc = descNode.Attributes["content"];
                    product.meta_description = desc.Value;
                }

                var keywordsNode = productDocument.DocumentNode.SelectSingleNode("//meta[@name='keywords']");

                if (keywordsNode != null)
                {
                    var desc = keywordsNode.Attributes["content"];
                    product.meta_keyword = desc.Value;
                }

                var titleNode = productDocument.DocumentNode.SelectSingleNode("//meta[@name='title']");

                if (titleNode != null)
                {
                    var desc = titleNode.Attributes["content"];
                    product.meta_title = desc.Value;
                }
                listOfProductDescr.Add(product);
                #endregion
            }

            //foreach (var VARIABLE in COLLECTION)
            //{

            //}

            return null;

        }

        private string AppendSiteName(string rawLinkUrl)
        {
            if (rawLinkUrl.StartsWith("http") || rawLinkUrl.StartsWith("www"))
            {
                return rawLinkUrl;

            }
            return (SnatchSettings.BaseWebSiteURL + rawLinkUrl);
        }

        private HtmlDocument GetPageFromURL(string URL)
        {
            var htmlWeb = new HtmlWeb();

            HtmlDocument doc;

            try
            {
                doc = htmlWeb.Load(URL);
                if (doc.ParseErrors.Any(error => error.Code == HtmlParseErrorCode.CharsetMismatch))
                {
                    doc = Helper.GetDocumentCustomMode(doc.Encoding, URL);
                }
            }
            catch (Exception)
            {
                doc = Helper.GetDocumentCustomMode(Encoding.GetEncoding("windows-1251"), URL);
            }

            return doc;
        }
    }
}
