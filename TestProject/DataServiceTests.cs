using Microsoft.VisualStudio.TestTools.UnitTesting;

using QuotationsWidgetProvider;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuotationsWidgetProvider.Tests
{
    [TestClass()]
    public class DataServiceTests
    {
        [TestMethod()]
        public void GetAQuotationTest()
        {
            DataService dataService =new QuotationsWidgetProvider.DataService();
            var s = dataService.RequsetQuotation();
        }

        [TestMethod()]
        public void DataServiceTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void RequsetQuotationTest()
        {
            Assert.Fail();
        }
    }
}