﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.ExceptionServices;
using System.Text;
using Microsoft.Office.Interop.Excel;

namespace Snatcher
{
    class SnatchEngine
    {

        public List<MakeuploveProductDescriptor> Process()
        {
            var catalogDocument = Helper.GetPageFromUrl(SnatchSettings.catalogURL);


            var linksList = catalogDocument.DocumentNode.SelectNodes("//a[@href]");

            var filteredLinkList = linksList.Where(lr => lr.OuterHtml.Contains(SnatchSettings.categoryContains));
            var listOfProductUrls = new List<string>();
            if (!SnatchSettings.singleProduct)
            {
                foreach (var link in filteredLinkList)
                {
                    //var attrib = link.Attributes["href"];
                    //var rawLinkUrl = attrib.Value;
                   var linkURL= link.Attributes["href"].Value;
                   if (!link.InnerHtml.Contains("img") && !linkURL.Contains("page") && linkURL != SnatchSettings.catalogURL
                       && !linkURL.Contains("sort") && linkURL != SnatchSettings.BadURL)
                    {
                        listOfProductUrls.Add(AppendSiteName(linkURL));
                    }
                }
            }
            else
            {
                var attrib = filteredLinkList.First().Attributes["href"];
                var rawLinkUrl = attrib.Value;
                listOfProductUrls.Add(AppendSiteName(rawLinkUrl));
            }

            //listOfProductUrls = listOfProductUrls.Distinct().ToList();
            //listOfProductUrls.RemoveAt(0);
            //listOfProductUrls.RemoveAt(listOfProductUrls.Count-1);

            var productList = GetDataFromLinks(listOfProductUrls);

            return productList;

        }
        public static string RemoveSpecialCharacters(string str)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in str)
            {
                if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == '.' || c == '_' || c == ')' || c == '(' || c == '№' || c == '!' || (c >= 'А' && c <= 'Я') || (c >= 'а' && c <= 'я'))
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        private List<MakeuploveProductDescriptor> GetDataFromLinks(List<string> listOfProductUrls)
        {
            var listOfProductDescr = new List<MakeuploveProductDescriptor>();
            int counter = 1;
          //listOfProductUrls.RemoveAll(str => str.Contains('?'));
            listOfProductUrls = listOfProductUrls.Distinct().ToList();
            foreach (var prodUrl in listOfProductUrls)
            {
                var productDocument = Helper.GetPageFromUrl(prodUrl);
                var product = new MakeuploveProductDescriptor();

                product.product_name = productDocument.DocumentNode.SelectSingleNode("/html/body/div[@class='l-page l-page-rubber']/div[@class='l-page-holder']/div[@class='l-wrapper g-center-width']/div[@class='l-page-content']/div[@class='l-content']/div[@class='name']").LastChild.OuterHtml.Replace(" &rarr;", "").Trim();
                product.product_name =  WebUtility.HtmlDecode(product.product_name);
                product.name = product.product_name;
                //product.sku = SnatchSettings.SKUStarter + counter.ToString("D4");
                product.description = productDocument.DocumentNode.SelectSingleNode("/html/body/div[@class='l-page l-page-rubber']/div[@class='l-page-holder']/div[@class='l-wrapper g-center-width']/div[@class='l-page-content']/div[@class='l-content']/div[@class='content']/div[@class='uss_shop_detail']/div[@class='uss_shop_full_description']").InnerHtml;
                product.description = WebUtility.HtmlDecode(product.description);
                //product.short_description = productDocument.DocumentNode.SelectSingleNode("/html/body[@class='china']/div[@class='theme-wrapper']/div[@class='block clear']/div[@class='twocol-content-wrapper']/div[@class='content']/dl[@class='tea-options']/dd[1]").InnerHtml;

                try
                {
                    product.price = productDocument.DocumentNode.SelectNodes("//span[@class='price']").First().InnerText.Replace("руб.", "").Trim();

                }
                catch (Exception)
                {
                    product.price = 500.ToString();
                }
                var imgNode = productDocument.DocumentNode.SelectSingleNode("/html/body/div[@class='l-page l-page-rubber']/div[@class='l-page-holder']/div[@class='l-wrapper g-center-width']/div[@class='l-page-content']/div[@class='l-content']/div[@class='content']/div[@class='uss_shop_detail']/div[@class='uss_img']/img[@class='big_image']/@src");
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
                    imgname = imgname.Replace("?pos=", "");
                    var imgWithExtension = imgname+ extension;

                    string localFilename = @"D:\MimiSnatches\" + imgWithExtension;
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

        public Worksheet ExportToExcel(List<MakeuploveProductDescriptor> snatchedProducts)
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

                var productAtributes = typeof(MakeuploveProductDescriptor).GetMembers().Skip(5).ToList();
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
