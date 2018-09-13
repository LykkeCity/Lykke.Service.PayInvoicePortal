using System;
using System.Collections.Generic;
using System.Text;
using Lykke.Service.PayInvoicePortal.Core.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Lykke.Service.PayInvoicePortal.Services.Tests
{
    [TestClass]
    public class SignupServiceTests
    {
        private ISignupService _signupService;

        [TestInitialize]
        public void TestInitialize()
        {
            _signupService = new SignupService(true);
        }

        [DataTestMethod]
        [DataRow("New Merchant Name")]
        [DataRow("new merchant name")]
        [DataRow("nEW mERCHANT nAME")]
        [DataRow("   New   Merchant   Name   ")]
        public void Test_GetIdFromName_Valid(string name)
        {
            var id = _signupService.GetIdFromName(name);

            Assert.AreEqual("NewMerchantName", id);
        }

        [DataTestMethod]
        [DataRow("")]
        [DataRow("   ")]
        public void Test_GetIdFromName_Empty(string name)
        {
            var id = _signupService.GetIdFromName(name);

            Assert.AreEqual(string.Empty, id);
        }

        [DataTestMethod]
        [DataRow("New_Merchant_Name")]
        public void Test_GetIdFromName_Invalid(string name)
        {
            var id = _signupService.GetIdFromName(name);

            Assert.AreNotEqual("NewMerchantName", id);
        }
    }
}
