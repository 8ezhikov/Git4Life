using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Drawing;
using Microsoft.Office.Interop.Excel;

namespace Snatcher
{
    class SnatchEngine
    {

        public List<ProductDescriptor> Process()
        {
            var catalogDocument = Helper.GetPageFromUrl(SnatchSettings.catalogURL);


            var linksList = catalogDocument.DocumentNode.SelectNodes("//a[@href]");

            var filteredLinkList = linksList.Where(lr => lr.OuterHtml.Contains(SnatchSettings.categoryContains));
            var listOfProductUrls = new List<string>();
            if (!SnatchSettings.singleProduct)
            {
                foreach (var link in filteredLinkList)
                {
                    var attrib = link.Attributes["href"];
                    var rawLinkUrl = attrib.Value;
                    listOfProductUrls.Add(AppendSiteName(rawLinkUrl));
                }
            }
            else
            {
                var attrib = filteredLinkList.First().Attributes["href"];
                var rawLinkUrl = attrib.Value;
                listOfProductUrls.Add(AppendSiteName(rawLinkUrl));
            }
        
           
            var productList = GetDataFromLinks(listOfProductUrls);

            return productList;

        }

        private List<ProductDescriptor> GetDataFromLinks(IEnumerable<string> listOfProductUrls)
        {
            var listOfProductDescr = new List<ProductDescriptor>();
            int counter = 1;
            foreach (var prodUrl in listOfProductUrls)
            {
                var productDocument = Helper.GetPageFromUrl(prodUrl);
                var product = new ProductDescriptor();

                product.product_name = productDocument.DocumentNode.SelectSingleNode("/html[1]/body[1]/div[2]/div[1]/div[1]/div[1]/h1[1]").LastChild.OuterHtml.Replace(" &rarr;", "").Trim();
                product.name = product.product_name;
                product.sku = SnatchSettings.SKUStarter + counter.ToString("D4");
                product.description = productDocument.DocumentNode.SelectSingleNode("/html/body[@class='china']/div[@class='theme-wrapper']/div[@class='block clear']/div[@class='twocol-content-wrapper']/div[@class='content']/dl[@class='tea-options']").InnerHtml;
                product.short_description = productDocument.DocumentNode.SelectSingleNode("/html/body[@class='china']/div[@class='theme-wrapper']/div[@class='block clear']/div[@class='twocol-content-wrapper']/div[@class='content']/dl[@class='tea-options']/dd[3]").InnerHtml;

                try
                {
                    product.price = productDocument.DocumentNode.SelectNodes("//p[1]").First(t => t.InnerText.Contains("Цена")).InnerText.Replace("Цена:", "").Replace("руб.", "").Trim();

                }
                catch (Exception)
                {
                    product.price = 500.ToString();
                }
                var imgNode = productDocument.DocumentNode.SelectSingleNode("//div[@class='product-top clear']/div[@class='big-img']/img[@class='main-img']/@src");
                if (imgNode != null)
                {
                    var imgRawURL = imgNode.Attributes["src"].Value;

                    //var filePathCount = imgRawURL.Reverse().SkipWhile(str => str != '/').Count();
                    //var imgFileName = imgRawURL.Substring(filePathCount);

                    if (imgNode.Attributes["alt"] != null)
                    {
                        product.image_label = imgNode.Attributes["alt"].Value;

                    }
                    var extension = imgRawURL.Substring(imgRawURL.LastIndexOf('.'));
                    var imgname = prodUrl.Substring(prodUrl.LastIndexOf('/') + 1);
                    var imgWithExtension = imgname+ extension;

                    string localFilename = @"D:\Snatches\" + imgWithExtension;
                    using (WebClient client = new WebClient())
                    {
                        client.DownloadFile(new Uri(AppendSiteName(imgRawURL)), localFilename);
                    }
                    product.image = "/images/" + imgWithExtension;
                    product.small_image = "/images/" + imgWithExtension;
                    product.thumbnail = "/images/" + imgWithExtension;
                    product.image_label = product.product_name;
                    product.small_image_label =product.product_name;
                    product.thumbnail_label = product.product_name;
                }

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

                counter++;
            }
            return listOfProductDescr;
        }

        private string AppendSiteName(string rawLinkUrl)
        {
            if (rawLinkUrl.StartsWith("http") || rawLinkUrl.StartsWith("www"))
            {
                return rawLinkUrl;

            }
            return (SnatchSettings.BaseWebSiteURL + rawLinkUrl);
        }

        public Worksheet ExportToExcel(List<ProductDescriptor> snatchedProducts)
        {
            {
                var xlApp = new Application {Visible = true};
                var wb = xlApp.Workbooks.Add(XlWBATemplate.xlWBATWorksheet);
                var ws = (Worksheet) wb.Worksheets[1];

                if (ws == null)
                {
                    Console.WriteLine(
                        "Worksheet could not be created. Check that your office installation and project references are correct.");
                    return null;
                }

                var aRange = ws.Range["C1", "C7"];
                if (aRange == null)
                {
                    Console.WriteLine(
                        "Could not get a range. Check to be sure you have the correct versions of the office DLLs.");
                    return null;
                }


                const int firstColumn = 2;

                const int verticalPosition = 0;

                var productAtributes = typeof (ProductDescriptor).GetMembers().Skip(5).ToList();
                var attribCount = productAtributes.Count();

                var startingLevelCell = ws.Cells[verticalPosition + 1, firstColumn - 1];
                var endingLevelCell =
                    ws.Cells[verticalPosition  +1 + snatchedProducts.Count, attribCount + firstColumn ];

                var levelRange = ws.Range[startingLevelCell, endingLevelCell];
                var levelData = new object[snatchedProducts.Count+1, attribCount];

                for (var i = 0; i < attribCount; i++)
                {
                    levelData[0, i] = productAtributes[i].Name;
                    for (var j = 0; j < snatchedProducts.Count; j++)
                    {
                        var myobject = snatchedProducts[j];
                        levelData[j + 1, i] = myobject.GetType().GetField(productAtributes[i].Name).GetValue(myobject);
                       //var reflectedValue =  myobject.GetType().GetField(productAtributes[i].Name).GetValue(myobject);
                       // if (reflectedValue == null)
                       // {
                       //     levelData[j+1, i] = string.Empty;
                       // }
                       // else
                       // {
                       //     levelData[j+1, i] = reflectedValue.ToString();
                       // }
                      
                    }


                }


                levelRange.Value2 = levelData;
               // levelRange.AutoFormat();

                return ws;
            }
        }
    }


}
