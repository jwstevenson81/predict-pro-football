using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PPF.Tests.Services
{
    [TestClass]
    public class SetupNewUserTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            var ppf = new PPF.Models.SeasonService();
            ppf.SetupNewUserMidSeason("lllo");
        }
    }
}
