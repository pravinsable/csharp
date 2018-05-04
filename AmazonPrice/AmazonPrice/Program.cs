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
            ProcessRepositories().Wait();
        }

        private static async Task ProcessRepositories()
        {
            try
            {
                var Products = new Product[]
                {
                    new Product{ ProductUri = $"https://www.amazon.com/Dell-FR3PK-34-Inch-Led-Lit-Monitor/dp/B01IOO4TIM", ExpectedPrice=729M },
                    new Product{ ProductUri = $"https://www.amazon.com/gp/product/B00SMLJPIC",ExpectedPrice=211.10M }
                };

                using (var hc = new HttpClient())
                {
                    foreach (var product in Products)
                    {
                        await GetProductDetailsAsync(hc, product);
                        PrintProduct(product);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: "+ex.Message);
            }
            Console.ReadKey();
        }

        private static async Task<Product> GetProductDetailsAsync(HttpClient hc, Product product)
        {
            if (hc == null) throw new Exception("HttpClient is null");
            var result = await hc.GetAsync(product.ProductUri);
            if(result==null) throw new Exception("result is null");
            var stream = await result.Content.ReadAsStreamAsync();
            if(stream==null) throw new Exception("stream is null");

            var doc = new HtmlDocument();
            doc.Load(stream);
            
            var productNode  = doc.DocumentNode.SelectSingleNode("//span[@id='productTitle']");
            if (productNode==null) throw new Exception("ProductNodes are null or empty");
            product.ProductTitle = productNode.InnerText.Trim();
            var price = doc.DocumentNode.SelectSingleNode("//span[@id='priceblock_ourprice']");
            if (price==null) throw new Exception("PriceColleciton are null or empty");
            product.Price = decimal.Parse(price.InnerHtml, NumberStyles.Currency);
            return product;
        }

        private static void PrintProduct(Product product)
        {
            Console.Write($"\n===========================================================================================================\n");
            Console.OutputEncoding = Encoding.Unicode;
            Console.Write($"[{product.ProductTitle}] price is ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($"${product.Price}");
            var diff = product.ExpectedPrice - product.Price;
            Console.ResetColor();
            Console.Write($" compare to ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($"${ product.ExpectedPrice}");
            Console.ResetColor();
            Console.Write($" i.e. ");
            Console.ForegroundColor = (diff > 0 ? ConsoleColor.Green : ConsoleColor.Red);
            Console.Write($"${ Math.Abs(diff)}");
            Console.ResetColor();
            Console.Write($" {(diff > 0 ? "less :)" : "more :(")}\n");
            Console.Write($"\n===========================================================================================================\n");
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
