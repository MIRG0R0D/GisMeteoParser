using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace Parser
{
    public static class ParserEngine
    {
        private static string site = "http://www.gismeteo.ru";

        private static int taskCounter = 0;


        public static List<CityTemp> getCities(GisMeteo.ParserHandler notify = null)
        {
            taskCounter = 0;

            List<CityTemp> cities = new List<CityTemp>();
            HtmlDocument HD = new HtmlDocument();
            var web = new HtmlWeb
            {
                AutoDetectEncoding = false,
                OverrideEncoding = Encoding.UTF8,
            };
            HD = web.Load(site);

            HtmlNodeCollection citiesNodes = HD.DocumentNode.SelectNodes("//div[@class='cities_item']");
            foreach (HtmlNode cityNode in citiesNodes)
            {
                var nameNode = cityNode.SelectSingleNode(".//span[@class='cities_name']");
                string name = nameNode.InnerText.Trim();
                var linkNode = cityNode.SelectSingleNode(".//a[contains(@class, 'cities_link')]");
                string link = linkNode.GetAttributeValue("href", null);
                if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(link)) throw new ArgumentNullException("link or name not found");
                CityTemp city = new CityTemp();
                city.Name = name;
                city.Link = link;
                city.id = Regex.Match(link, @"\d+").Value;
                cities.Add(city);
            }

            HtmlNodeCollection favCities = HD.DocumentNode.SelectNodes("//noscript[@id='noscript']/a");
            foreach (HtmlNode cityNode in favCities)
            {
                CityTemp city = new CityTemp();
                string name = cityNode.GetAttributeValue("data-name", null);
                string link = cityNode.GetAttributeValue("href", null);
                if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(link)) throw new ArgumentNullException("link or name not found");
                city.Name = name;
                city.Link = link;
                city.id = Regex.Match(link, @"\d+").Value;
                cities.Add(city);
            }

            return cities;
        }

        public static void FillCity(CityTemp cityTemp, GisMeteo.ParserHandler notify = null)
        {
            //Console.WriteLine("getting details for {0}", cityTemp.Name);
            HtmlDocument HD = new HtmlDocument();
            var web = new HtmlWeb
            {
                AutoDetectEncoding = false,
                OverrideEncoding = Encoding.UTF8,
            };
            HD = web.Load(site + cityTemp.Link + "tomorrow/");
            var tempNextDay = HD.DocumentNode.SelectSingleNode("//div[contains(@class, 'forecast_frame')]/div[contains(@class, '_center')]/div//div[@class='values']");
            cityTemp.Date = DateTime.Now.Date.AddDays(1);
            cityTemp.NightTemp = tempNextDay.SelectSingleNode("div[1]/span[contains(@class, unit_temperature_c)]").InnerText;
            cityTemp.DayTemp = tempNextDay.SelectSingleNode("div[2]/span[contains(@class, unit_temperature_c)]").InnerText;

            if (cityTemp.DayTemp.Contains("minus")) { cityTemp.DayTemp = cityTemp.DayTemp.Replace("&minus;", "-"); }
            if (cityTemp.NightTemp.Contains("minus")) { cityTemp.NightTemp = cityTemp.NightTemp.Replace("&minus;", "-"); }

            notify?.Invoke(taskCounter);
            Interlocked.Increment(ref taskCounter);
            
            //Console.WriteLine(string.Format("{0,-20}{1,-30}{2,-10}{3,-10}", cityTemp.Name, cityTemp.Link, cityTemp.DayTemp, cityTemp.NightTemp));
        }

    }
}
