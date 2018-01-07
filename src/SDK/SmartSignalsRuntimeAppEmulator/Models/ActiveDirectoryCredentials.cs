using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Rest;

namespace Microsoft.Azure.Monitoring.SmartSignals.Emulator.Models
{
    public class ActiveDirectoryCredentials : ServiceClientCredentials
    {
        private string token;

        public ActiveDirectoryCredentials(string token)
        {
            this.token = token;
        }

        public override async Task ProcessHttpRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {       
            // Add the authentication header
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", this.token);
            await base.ProcessHttpRequestAsync(request, cancellationToken);
        }
    }
}
