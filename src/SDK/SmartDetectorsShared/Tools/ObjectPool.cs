//-----------------------------------------------------------------------
// <copyright file="ObjectPool.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.Tools
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading;

    /// <summary>
    /// Pool of objects that can be leased.
    /// </summary>
    /// <typeparam name="T">Type of objects in pool</typeparam>
    public class ObjectPool<T> : IDisposable
    {
        private readonly int maxPoolSize;
        private readonly Func<T> objectGenerator;
        private readonly BlockingCollection<T> pool = new BlockingCollection<T>();
        private long poolSize = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectPool{T}"/> class.
        /// </summary>
        /// <param name="objectGenerator">Function that generates a new object in the case the pool is empty</param>
        /// <param name="maxPoolSize">Maximum amount of objects in pool.  Default is 100.</param>
        public ObjectPool(Func<T> objectGenerator, int maxPoolSize = 100)
        {
            this.objectGenerator = objectGenerator;
            this.maxPoolSize = maxPoolSize;
        }

        /// <summary>
        /// Returns an item from the pool.  If no items exist in the pool either:
        /// 1. A new item is created if the number of items created is less than the max.
        /// 2. Blocks until an item becomes available.
        /// </summary>
        /// <returns>Item from the pool</returns>
        public ILeasedItem<T> LeaseItem()
        {
            if (this.pool.TryTake(out T retval))
            {
                return new LeasedItem<T>(this, retval);
            }

            if (Interlocked.Increment(ref this.poolSize) > this.maxPoolSize)
            {
                Interlocked.Decrement(ref this.poolSize);
                return new LeasedItem<T>(this, this.pool.Take());
            }

            retval = this.objectGenerator();
            return new LeasedItem<T>(this, retval);
        }

        /// <summary>
        /// Dispose method
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposing helper method
        /// </summary>
        /// <param name="disposing">True if disposing, false if finalizing</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.pool?.Dispose();
            }
        }

        /// <summary>
        /// Return the item to the pool.
        /// </summary>
        /// <param name="item">The item to be returned to the pool</param>
        private void ReleaseItem(T item)
        {
            this.pool.Add(item);
        }

        /// <summary>
        /// A wrapper for a leased item that is returned to the pool when disposed
        /// </summary>
        /// <typeparam name="TItem">Type of the item</typeparam>
        private class LeasedItem<TItem> : ILeasedItem<TItem>
        {
            private readonly ObjectPool<TItem> pool;

            /// <summary>
            /// Initializes a new instance of the <see cref="LeasedItem{TItem}"/> class
            /// </summary>
            /// <param name="pool">The pool the item was taken from and should be returned to</param>
            /// <param name="item">The item itself</param>
            public LeasedItem(ObjectPool<TItem> pool, TItem item)
            {
                this.pool = pool;
                this.Item = item;
            }

            /// <summary>
            /// Gets the leased item.
            /// </summary>
            public TItem Item { get; }

            /// <summary>
            /// Returns the item to the pool
            /// </summary>
            public void Dispose()
            {
                this.pool.ReleaseItem(this.Item);
            }
        }
    }
}
