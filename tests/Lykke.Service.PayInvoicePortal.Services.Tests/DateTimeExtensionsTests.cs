using System;
using System.Collections.Generic;
using Lykke.Service.PayInvoicePortal.Services.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Lykke.Service.PayInvoicePortal.Services.Tests
{
    [TestClass]
    public class DateTimeExtensionsTests
    {
        private static DateTime Sunday { get; } = new DateTime(2018, 9, 30);
        private static DateTime Saturday { get; } = new DateTime(2018, 9, 29);
        private static DateTime Friday { get; } = new DateTime(2018, 9, 28);
        private static DateTime Thursday { get; } = new DateTime(2018, 9, 27);
        private static DateTime Wednesday { get; } = new DateTime(2018, 9, 26);
        private static DateTime Tuesday { get; } = new DateTime(2018, 9, 25);
        private static DateTime Monday { get; } = new DateTime(2018, 9, 24);

        public static IEnumerable<object[]> DaysOfWeek
        {
            get
            {
                yield return new object[] { Sunday };
                yield return new object[] { Saturday };
                yield return new object[] { Friday };
                yield return new object[] { Thursday };
                yield return new object[] { Wednesday };
                yield return new object[] { Tuesday };
                yield return new object[] { Monday };
            }
        }

        private static DateTime FirstDayOfMonth { get; } = new DateTime(2018, 9, 1);
        private static DateTime NotFirstDayOfMonth { get; } = new DateTime(2018, 9, 2);

        public static IEnumerable<object[]> DaysOfMonth
        {
            get
            {
                yield return new object[] { FirstDayOfMonth };
                yield return new object[] { NotFirstDayOfMonth };
            }
        }

        [DataTestMethod]
        [DynamicData(nameof(DaysOfWeek), DynamicDataSourceType.Property)]
        public void Test_GetBeginOfWeek_Valid(DateTime date)
        {
            // add some time
            date = date.AddHours(12).AddMinutes(30).AddSeconds(30);

            var result = date.GetBeginOfWeek();

            Assert.AreEqual(result.DayOfWeek, DayOfWeek.Monday);
            Assert.AreEqual(result.TimeOfDay, TimeSpan.Zero);
        }

        [DataTestMethod]
        [DynamicData(nameof(DaysOfMonth), DynamicDataSourceType.Property)]
        public void Test_GetBeginOfMonth_Valid(DateTime date)
        {
            // add some time
            date = date.AddHours(12).AddMinutes(30).AddSeconds(30);

            var result = date.GetBeginOfMonth();

            Assert.AreEqual(result.Day, 1);
            Assert.AreEqual(result.TimeOfDay, TimeSpan.Zero);
        }
    }
}
