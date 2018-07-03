using HtmlAgilityPack;
using System;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AmazonPrice
{
    class Program
    {
        

        static void Main()
        {
            Console.OutputEncoding = Encoding.Unicode;
            ProcessRepositories().Wait();
        }

        private static async Task ProcessRepositories()
        {
            try
            {
                var Products = new Product[]
                {
                    new Product{ ProductUri = $"https://www.amazon.com/Dell-FR3PK-34-Inch-Led-Lit-Monitor/dp/B01IOO4TIM", ExpectedPrice=729M },
                };

                using (var hc = new HttpClient())
                {
                    foreach (var product in Products)
                    {
                        await GetProductDetailsAsync(hc, product);
                        PrintProduct(product);
                    }
                    //string month = DateTime.Today.AddMonths(1).ToString("MMMM").ToLower();                    
                    //string visabulletinUrl = $"https://travel.state.gov/content/travel/en/legal/visa-law0/visa-bulletin/2018/visa-bulletin-for-{month}-2018.html";
                    //var result = await hc.GetAsync(visabulletinUrl);
                    //if (result.StatusCode != System.Net.HttpStatusCode.OK)
                    //{
                    //    Console.Write($"\n===========================================================================================================\n\n");
                    //    PrintBad($"\t\tNo visa bulletin for {month}");
                    //    Console.Write($"\n\n===========================================================================================================\n");
                    //}
                    //else
                    //{
                    //    System.Diagnostics.Process.Start(@"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe", visabulletinUrl);

                    //}

                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
            Console.ReadKey();
        }

        private static void PrintWithLines(string value)
        {
            Console.Write($"\n===========================================================================================================\n\n");
            Console.WriteLine($"\t\t{value}");
            Console.Write($"\n\n===========================================================================================================\n");
        }

        private static async Task<Product> GetProductDetailsAsync(HttpClient hc, Product product)
        {
            if (hc == null) throw new Exception("HttpClient is null");
            var result = await hc.GetAsync(product.ProductUri);
            if (result == null) throw new Exception("result is null");
            var stream = await result.Content.ReadAsStreamAsync();
            if (stream == null) throw new Exception("stream is null");

            var doc = new HtmlDocument();
            doc.Load(stream);

            var productNode = doc.DocumentNode.SelectSingleNode("//span[@id='productTitle']");
            if (productNode == null) throw new Exception("ProductNodes are null or empty");
            product.ProductTitle = productNode.InnerText.Trim();
            var price = doc.DocumentNode.SelectSingleNode("//span[@id='priceblock_ourprice']");
            if (price == null) throw new Exception("PriceColleciton are null or empty");
            product.Price = decimal.Parse(price.InnerHtml, NumberStyles.Currency);
            return product;
        }

        private static void PrintProduct(Product product)
        {
            Console.Write($"\n===========================================================================================================\n");
            Console.Write($"[{product.ProductTitle}] price is ");
            PrintGood("$"+product.Price.ToString());
            var diff = product.ExpectedPrice - product.Price;
            
            Console.Write($" compare to ");
            PrintGood($"${ product.ExpectedPrice}");
            Console.Write($" i.e. ");
            string absDiff = Math.Abs(diff).ToString();
            if (diff > 0)
                PrintGood("$" + absDiff);
            else
                PrintBad("$" + absDiff);
            
            Console.Write($" {(diff > 0 ? "less :)" : "more :(")}\n");
            Console.Write($"\n===========================================================================================================\n");
        }

        private static void PrintGood(string value)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($"{value}");
            Console.ResetColor();
        }
        private static void PrintBad(string value)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write($"{value}");
            Console.ResetColor();
        }

    }
    class Product
    {
        public string ProductTitle { get; set; }
        public decimal Price { get; set; }
        public string ProductUri { get; set; }
        public decimal ExpectedPrice { get; set; }
    }
}
