//-----------------------------------------------------------------------
// <copyright file="DisposableSemaphoreSlim.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.Tools
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// A wrapper to the SemaphoreSlim class that employs the Dispose() pattern for releasing
    /// </summary>
    public class DisposableSemaphoreSlim : IDisposable
    {
        private readonly SemaphoreSlim semaphore;

        /// <summary>
        /// Initializes a new instance of the <see cref="DisposableSemaphoreSlim"/> class.
        /// </summary>
        /// <param name="semaphore">The actual semaphore</param>
        public DisposableSemaphoreSlim(SemaphoreSlim semaphore)
        {
            Diagnostics.EnsureArgumentNotNull(() => semaphore);
            this.semaphore = semaphore;
        }

        /// <summary>
        /// Wait on the semaphore
        /// </summary>
        /// <returns>Returns the instance so it can be used in a using() statement</returns>
        public DisposableSemaphoreSlim Wait()
        {
            this.semaphore.Wait();
            return this;
        }

        /// <summary>
        /// Wait on the semaphore
        /// </summary>
        /// <param name="cancellationToken">A cancellationToken</param>
        /// <returns>Returns the instance so it can be used in a using() statement</returns>
        public async Task<DisposableSemaphoreSlim> WaitAsync(CancellationToken cancellationToken)
        {
            await this.semaphore.WaitAsync(cancellationToken);
            return this;
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
                this.semaphore.Release();
            }
        }
    }
}
