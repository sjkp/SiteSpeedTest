using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace SJKP.SiteSpeedTest.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void ResourceGroupNameTest()
        {
            var name = Function1.MakeName(Region.AustraliaSouthEast, new System.Uri("http://veryverylong-url-thatneedstobeshortened.com"), Guid.NewGuid());
            Console.WriteLine(name);
            Assert.AreEqual(90, name.Length);
        }
    }
}
