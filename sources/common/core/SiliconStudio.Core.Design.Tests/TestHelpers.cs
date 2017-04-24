// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Collections.Generic;
using NUnit.Framework;
using SiliconStudio.Core.Windows;

namespace SiliconStudio.Core.Design.Tests
{
    [TestFixture]
    class TestHelpers
    {
        private static void ThrowTestInner(string msg)
        {
            throw new InvalidOperationException(msg);
        }

        private static void ThrowTest1()
        {
            try
            {
                ThrowTestInner("Exception1 - Inner");
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Exception1", e);
            }
        }

        private static void ThrowTest2()
        {
            try
            {
                ThrowTestInner("Exception2 - Inner");
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Exception2", e);
            }

        }

        private static void ThrowTest()
        {
            var exceptions = new List<Exception>();
            try
            {
                ThrowTest1();
            }
            catch (Exception e)
            {
                exceptions.Add(e);
            }
            try
            {
                ThrowTest2();
            }
            catch (Exception e)
            {
                exceptions.Add(e);
            }
            if (exceptions.Count > 0)
                throw new AggregateException("Aggregate exceptions!", exceptions);

        }
        [Test]
        public void ExceptionLogTest()
        {
            try
            {
                ThrowTest();
            }
            catch (Exception e)
            {
                var message = AppHelper.BuildErrorMessage(e);
                Console.WriteLine(message);
            }
        }
    }
}
