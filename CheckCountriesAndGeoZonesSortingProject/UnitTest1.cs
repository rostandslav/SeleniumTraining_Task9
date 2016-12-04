using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.Chrome;
using System.Collections;
using System.Collections.Generic;

namespace CheckCountriesAndGeoZonesSortingProject
{
    [TestClass]
    public class UnitTest1
    {
        private IWebDriver driver;
        private WebDriverWait wait;


        [TestInitialize]
        public void Init()
        {
            driver = new ChromeDriver();
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(60));
        }


        private void CheckStringListIsSorted(List<string> ls)
        {
            var sortedLS = new List<string>(ls);
            sortedLS.Sort();
            CollectionAssert.AreEqual(ls, sortedLS);
        }

        private int GetColumnIndexByName(string headerSelector, string columnName)
        {
            var header = driver.FindElements(By.CssSelector(headerSelector));
            for (int i = 0; i < header.Count; i++)
            {
                if (string.Compare(header[i].Text, columnName, StringComparison.OrdinalIgnoreCase) == 0)
                    return i + 1;
            }
            Assert.Fail("Столбец " + columnName + " не найден среди заголовков " + headerSelector);
            return 0;
        }


        [TestMethod]
        public void CheckCountriesSorting()
        {
            driver.Url = "http://litecart/admin/?app=countries&doc=countries";

            driver.FindElement(By.Name("username")).SendKeys("admin");
            driver.FindElement(By.Name("password")).SendKeys("admin");
            driver.FindElement(By.Name("login")).Click();

            wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("table.dataTable")));

            var countries = driver.FindElements(By.CssSelector("table.dataTable tr.row")); // страны

            var countriesNames = new List<string>(); // названия стран
            var zonesNames = new List<string>(); // названия зон страны (если они есть)

            int nameIndex = GetColumnIndexByName("table.dataTable th", "Name");
            int zonesIndex = GetColumnIndexByName("table.dataTable th", "Zones");
            string zonesText = "0";
            int zonesCount = 0; // число зон страны

            for (int i = 0; i < countries.Count; i++)
            {
                countriesNames.Add(countries[i].FindElement(By.CssSelector("td:nth-child(" + nameIndex + ") a")).Text);
                zonesText = countries[i].FindElement(By.CssSelector("td:nth-child(" + zonesIndex + ")")).Text;
                zonesCount = Convert.ToInt32(zonesText);

                if (zonesCount > 0)
                {
                    countries[i].FindElement(By.CssSelector("td:nth-child(" + nameIndex + ") a")).Click();

                    var zones = driver.FindElements(By.CssSelector("table#table-zones tr:not(.header)")); // зоны
                    for (int j = 0; j < zones.Count - 1; j++)
                    {
                        zonesNames.Add(zones[j].FindElement(By.CssSelector("td:nth-child(3)")).GetAttribute("innerText"));
                    }
                    CheckStringListIsSorted(zonesNames); // проверка упорядоченности зон                   
                    zonesNames.Clear();
                    driver.Navigate().Back();

                    countries = driver.FindElements(By.CssSelector("table.dataTable tr.row")); // страны
                }
            }

            CheckStringListIsSorted(countriesNames); // проверка упорядоченности стран
        }


        [TestMethod]
        public void CheckGeoZonesSorting()
        {
            driver.Url = "http://litecart/admin/?app=geo_zones&doc=geo_zones";

            driver.FindElement(By.Name("username")).SendKeys("admin");
            driver.FindElement(By.Name("password")).SendKeys("admin");
            driver.FindElement(By.Name("login")).Click();

            string tableGeoZonesSelector = "form[name='geo_zones_form'] table.dataTable";
            string tableZonesSelector = "form[name='form_geo_zone'] table#table-zones";

            wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(tableGeoZonesSelector)));

            var geoZones = driver.FindElements(By.CssSelector(tableGeoZonesSelector + " tr.row")); // гео-зоны
            int nameIndex = GetColumnIndexByName(tableGeoZonesSelector + " th", "Name");
            var zonesNames = new List<string>(); // названия зон страны

            for (int i = 0; i < geoZones.Count; i++)
            {
                geoZones[i].FindElement(By.CssSelector("td:nth-child(" + nameIndex + ") a")).Click();

                wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(tableZonesSelector)));
                var zones = driver.FindElements(By.CssSelector(tableZonesSelector + " tr:not(.header)")); // зоны
                int zoneIndex = GetColumnIndexByName(tableZonesSelector + " th", "Zone");
                for (int j = 0; j < zones.Count - 1; j++)
                {
                    zonesNames.Add(zones[j].FindElement(By.CssSelector("td:nth-child(" + zoneIndex + ") option[selected='selected']")).Text);
                }
                CheckStringListIsSorted(zonesNames);
                zonesNames.Clear();
                driver.Navigate().Back();

                geoZones = driver.FindElements(By.CssSelector(tableGeoZonesSelector + " tr.row")); // гео-зоны
            }
        }


        [TestCleanup]
        public void Finish()
        {
            driver.Quit();
            //driver = null;
        }
    }
}
