using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Security;

namespace DentaQuestTestHarness
{
    public static class AlternativeBindingHelper
    {
        /// <summary>
        /// Creates a WSHttpBinding with certificate security - simpler approach for .NET Core
        /// </summary>
        public static WSHttpBinding CreateWSHttpBinding()
        {
            var binding = new WSHttpBinding(SecurityMode.Transport);

            // Set security mode to Transport with client certificates
            binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Certificate;

            // Set timeouts to match specification
            binding.CloseTimeout = TimeSpan.FromMinutes(1);
            binding.OpenTimeout = TimeSpan.FromMinutes(1);
            binding.ReceiveTimeout = TimeSpan.FromMinutes(70);
            binding.SendTimeout = TimeSpan.FromMinutes(1);

            // Set message encoding
            binding.MessageEncoding = WSMessageEncoding.Text;

            // Set maximum message size
            binding.MaxReceivedMessageSize = 1048576;
            binding.ReaderQuotas.MaxStringContentLength = 1048576;
            binding.ReaderQuotas.MaxArrayLength = 16384;
            binding.ReaderQuotas.MaxBytesPerRead = 65536;
            binding.ReaderQuotas.MaxDepth = 32;
            binding.ReaderQuotas.MaxNameTableCharCount = 16384;

            return binding;
        }

        /// <summary>
        /// Creates a BasicHttpsBinding for simple HTTPS communication - fallback option
        /// </summary>
        public static BasicHttpsBinding CreateBasicHttpsBinding()
        {
            var binding = new BasicHttpsBinding();
            binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Certificate;

            // Set timeouts
            binding.CloseTimeout = TimeSpan.FromMinutes(1);
            binding.OpenTimeout = TimeSpan.FromMinutes(1);
            binding.ReceiveTimeout = TimeSpan.FromMinutes(70);
            binding.SendTimeout = TimeSpan.FromMinutes(1);

            return binding;
        }
    }
}