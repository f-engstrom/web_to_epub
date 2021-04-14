using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;

namespace web_to_epub_console_app
{
    class Program
    {
        static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false);

            IConfiguration configuration = builder.Build();

            // Print connection string to demonstrate configuration object is populated
            string email = configuration.GetSection("email").Value;
            string password = configuration.GetSection("password").Value;
            Console.WriteLine(password);

            IWebDriver driver;
            FirefoxOptions options = new FirefoxOptions();
            options.AddArguments("--headless");
            driver = new FirefoxDriver(options);
            // using IWebDriver driver = new FirefoxDriver(new FirefoxOptions().AddArguments("--headless"));
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            driver.Navigate().GoToUrl("https://www.economist.com/");
            driver.FindElement(By.XPath("/html/body/div[1]/div/div/div[4]/header/div[1]/div[1]/div/div[2]/a")).Click();
            wait.Until(webDriver => webDriver.FindElement(By.Name("username")).Displayed);
            driver.FindElement(By.Name("username")).SendKeys(email + Keys.Enter);
            //  wait.Until(webDriver => webDriver.FindElement(By.Name("username")).Displayed);
            wait.Until(webDriver => webDriver.FindElement(By.Name("password")).Displayed);

            //driver.FindElement(By.Name("username")).SendKeys("frans.engstrom@gmail.com");
            driver.FindElement(By.Name("password")).SendKeys(password + Keys.Enter);
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(4);
            wait.Until(webDriver => webDriver.FindElement(By.TagName("body")).Displayed);
            driver.FindElement(By.XPath("/html/body/div[1]/div/div/div[4]/header/div[1]/div[1]/nav/ul/li[2]/a"))
                .Click();

            //  driver.Navigate().GoToUrl("https://www.economist.com/weeklyedition/");
            var scriptTag = driver.FindElement((By.XPath("/html/body/div/div/div/div[5]/main/script")));
            var jsonData = scriptTag.GetAttribute("innerHTML");
            var articles = JsonConvert.DeserializeObject<Articles>(jsonData);

            var articlesHTMLString = "";
            int index = 0;

            foreach (var article in articles.itemListElement)
            {
                Directory.CreateDirectory("/Users/fransengstrom/Desktop/economist/testArticle");

                driver.Navigate().GoToUrl(article.url);
                IJavaScriptExecutor js = (IJavaScriptExecutor) driver;

                
                
                var mainNode = driver.FindElement((By.Id("content")));

                try
                {
                    var articleNode = mainNode.FindElement(By.TagName("article"));
                    js.ExecuteScript("arguments[0].classList.add('chapter');", articleNode);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                
                try
                {
                    var audioPlayer = mainNode.FindElement(By.ClassName("article-audio-player"));
                    js.ExecuteScript("arguments[0].remove();", audioPlayer);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

                
                try
                {

                    var recommendedSection = mainNode.FindElement(By.ClassName("css-1j6rk0a"));
                    js.ExecuteScript("arguments[0].remove();", recommendedSection);

                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                
                
                try
                {

                    var articleSection = mainNode.FindElement(By.ClassName("article__section"));
                    js.ExecuteScript("arguments[0].remove();", articleSection);

                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

                try
                {
                    var articleAside = mainNode.FindElement(By.ClassName("article__aside"));
                    js.ExecuteScript("arguments[0].remove();", articleAside);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            

                try
                {
                    var adverts = mainNode.FindElements(By.ClassName("advert"));
                    foreach (var advert in adverts)
                    {
                        js.ExecuteScript("arguments[0].remove();", advert);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
              

                
                
                try
                {
                    var images = mainNode.FindElements(By.TagName("img"));

                    foreach (var image in images)
                    {
                        var imageSrc = (string) js.ExecuteScript(" return arguments[0].src;", image);
                        download(imageSrc, index.ToString());
                        js.ExecuteScript(" return arguments[0].src =arguments[1].toString()+'.png';", image, index);
                        index++;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
             

                
                
                try
                {
                    var layoutGrid =
                        mainNode.FindElement(By.XPath("/html/body/div/div/div/div[5]/main/article/div[3]"));
                    js.ExecuteScript("arguments[0].remove();", layoutGrid);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

                try
                {
                    var promoLinks = mainNode.FindElement(By.ClassName("layout-article-links"));

                    js.ExecuteScript("arguments[0].remove();", promoLinks);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
              
                
                
                try
                {
                    var relatedArticles = mainNode.FindElement(By.ClassName("related-articles"));
                    js.ExecuteScript("arguments[0].remove();", relatedArticles);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }


                articlesHTMLString += (string) js.ExecuteScript("return arguments[0].innerHTML;", mainNode);
            }

            File.WriteAllText($"/Users/fransengstrom/Desktop/economist/testArticle/articles.txt",
                articlesHTMLString);
        }


        static void download(string url, string imageName)
        {
            using (WebClient client = new WebClient())
            {
                client.DownloadFile(new Uri(url),
                    $"/Users/fransengstrom/Desktop/economist/testArticle/{imageName}.png");
            }
        }
    }


    class Articles
    {
        public string @context { get; set; }
        public string @type { get; set; }
        public List<ArticleData> itemListElement { get; set; } = new List<ArticleData>();
    }

    class ArticleData
    {
        public string @type { get; set; }
        public int position { get; set; }
        public string url { get; set; }
    }
}