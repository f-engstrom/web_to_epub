using System;
using System.Collections.Generic;
using System.IO;
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
            
            foreach (var article in articles.itemListElement)
            {
                driver.Navigate().GoToUrl(article.url);
            
                var articleParagraphs = driver.FindElements(By.ClassName("article__body-text"));
                string headline = driver.FindElement(By.ClassName("article__headline")).Text;
            
                string articleText = headline + "\n";
                foreach (var articleParagraph in articleParagraphs)
                {
                    articleText += articleParagraph.Text + "\n";
                }
            
                File.WriteAllText($"{headline}.txt", articleText);
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