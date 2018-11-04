//-----------------------------------------------------------------------
// <copyright file="ObjectPoolTests.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace SmartDetectorsSharedTests
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartDetectors.Tools;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ObjectPoolTests
    {
        [TestMethod]
        public void WhenPoolNotFullAndAllObjectsLeasedNewOneCreated()
        {
            int counter = 0, maxPoolSize = 100;
            var pool = new ObjectPool<int>(() => counter++, maxPoolSize);
            for (int i = 0; i < maxPoolSize; i++)
            {
                var item = pool.LeaseItem();
                Assert.AreEqual(i, item.Item);
            }
        }

        [TestMethod]
        public void WhenPoolObjectAvailableNewOneNotCreated()
        {
            int counter = 0, maxPoolSize = 100;
            var pool = new ObjectPool<int>(() => counter++, maxPoolSize);

            using (pool.LeaseItem())
            {
            }

            var item = pool.LeaseItem();
            Assert.AreEqual(0, item.Item);
            Assert.AreEqual(1, counter);
        }

        [TestMethod]
        public void WhenPoolIsAtMaxCapacityAndEmptyNewObjectNotCreated()
        {
            int counter = 0, maxPoolSize = 10;
            var pool = new ObjectPool<int>(() => counter++, maxPoolSize);
            var rand = new Random();
            var itemToReleaseIndex = rand.Next(maxPoolSize);

            ILeasedItem<int> itemToRelease = null;
            for (int i = 0; i < maxPoolSize; i++)
            {
                var item = pool.LeaseItem();
                if (itemToReleaseIndex == i)
                {
                    itemToRelease = item;
                }
            }

            // Lease an item on another thread since it should block.
            var leasedTask = Task<int>.Run(() => pool.LeaseItem());
            Thread.Sleep(100);

            Assert.IsFalse(leasedTask.IsCompleted);
            itemToRelease.Dispose();

            if (!leasedTask.Wait(TimeSpan.FromMilliseconds(100)))
            {
                Assert.Fail("Item not leased successfully");
            }

            Assert.AreEqual(itemToReleaseIndex, leasedTask.Result.Item);
        }
    }
}
