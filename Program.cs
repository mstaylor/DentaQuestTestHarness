using System;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Security;
using System.ServiceModel.Security.Tokens;
using System.Xml;
using DentaQuestTestHarness.ServiceReference;

namespace DentaQuestTestHarness
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("DentaQuest EDI WCF Test Harness");
            Console.WriteLine("==============================");

            // Test environment URL
            string testServiceUrl = "https://editest.dentaquest.com/TestEdiWcfRealTime/EdiWcfRealTime.svc";
            // Production environment URL
            // string prodServiceUrl = "https://ediprod.dentaquest.com/EdiWcfRealTime/EdiWcfRealTime.svc";

            try
            {
                var testHarness = new EdiTestHarness();
                var client = testHarness.SetWebServiceClient(testServiceUrl);
                Console.WriteLine("WCF client configured successfully!");

                // Choose transaction type
                Console.WriteLine("Select transaction type:");
                Console.WriteLine("1. 270 (Eligibility Inquiry)");
                Console.WriteLine("2. 276 (Claim Status Inquiry)");
                Console.Write("Enter choice (1 or 2): ");

                string choice = Console.ReadLine();
                string sampleEdiContent;

                switch (choice)
                {
                    case "1":
                        sampleEdiContent = TestTransactions.GetSample270Transaction();
                        Console.WriteLine("Using 270 (Eligibility Inquiry) transaction");
                        break;
                    case "2":
                        sampleEdiContent = TestTransactions.GetSample276Transaction();
                        Console.WriteLine("Using 276 (Claim Status Inquiry) transaction");
                        break;
                    default:
                        sampleEdiContent = TestTransactions.GetSample270Transaction();
                        Console.WriteLine("Default: Using 270 (Eligibility Inquiry) transaction");
                        break;
                }

                // Clean EDI content (remove CRLF as required by DentaQuest)
                sampleEdiContent = TestTransactions.CleanEdiContent(sampleEdiContent);

                // Process the EDI transaction
                var request = EdiTransmissionHelper.CreateEdiTransmission(sampleEdiContent);
                Console.WriteLine($"\nSending EDI transaction to: {testServiceUrl}");
                Console.WriteLine($"Transaction length: {sampleEdiContent.Length} characters");

                var response = client.Process(request);
                string responseContent = EdiTransmissionHelper.ExtractEdiContent(response);

                Console.WriteLine("\n=== RESPONSE RECEIVED ===");
                Console.WriteLine(responseContent);
                Console.WriteLine("==========================");

                client.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }

    public class EdiTestHarness
    {
        // Configuration - set these paths to your PFX files
        private readonly string clientPfxPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "certificates", "airpay.pfx");
        private readonly string clientPfxPassword = "changeit";
        private readonly string testServerCertPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "certificates", "dentaquest-test.crt");
        private readonly string prodServerCertPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "certificates", "dentaquest-prod.crt");

        public EdiWcfRealTimeClient SetWebServiceClient(string url)
        {
            string dnsIdentity = string.Empty;
            string serverCertPath = string.Empty;
            bool isProduction = false;

            if ((url.ToLower().Contains("dq.ad") || url.ToLower().Contains("ediprod")) && !url.ToLower().Contains(System.Environment.MachineName.ToLower()))
            {
                dnsIdentity = "www.ediprod.dentaquest.com";
                serverCertPath = prodServerCertPath;
                isProduction = true;
            }
            else
            {
                dnsIdentity = "www.editest.dentaquest.com";
                serverCertPath = testServerCertPath;
                isProduction = false;
            }

            Console.WriteLine($"Environment: {(isProduction ? "Production" : "Test")}");
            Console.WriteLine($"DNS Identity: {dnsIdentity}");

            // Check web service reachable
            try
            {
                var myRequest = (HttpWebRequest)WebRequest.Create(url);

                var response = (HttpWebResponse)myRequest.GetResponse();

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    throw new Exception(string.Format("Web service {0}, but with status: {1}",
                        url, response.StatusDescription));
                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Web service {0} unavailable: {1}", url, ex.Message));
            }

            // Load certificates from PFX files
            Console.WriteLine("Loading client certificate from PFX...");
            X509Certificate2 clientCertificate = PfxCertificateLoader.LoadClientCertificateFromPfx(clientPfxPath, clientPfxPassword);

            Console.WriteLine("Loading server certificate from file...");
            X509Certificate2 serverCertificate = PfxCertificateLoader.LoadServerCertificateFromFile(serverCertPath);

            // Build endpoint and binding
            EndpointIdentity identity = EndpointIdentity.CreateDnsIdentity(dnsIdentity);
            EndpointAddress address = new EndpointAddress(new Uri(url), identity);

            if (url.ToUpper().Contains(System.Environment.MachineName.ToUpper()))
            {
                // Local testing - use WSHttpBinding without security
                WSHttpBinding httpBinding = new WSHttpBinding();
                httpBinding.Security.Mode = SecurityMode.None;
                httpBinding.Security.Transport.ClientCredentialType = HttpClientCredentialType.None;
                EdiWcfRealTimeClient httpClient = new EdiWcfRealTimeClient(httpBinding, address);
                return httpClient;
            }

            // Create binding - try different approaches based on .NET Core compatibility
            Binding binding;

            try
            {
                // Option 1: Custom binding (most accurate to WSDL)
                binding = CreateCustomBinding();
                Console.WriteLine("Using CustomBinding approach");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CustomBinding failed: {ex.Message}");
                // Option 2: WSHttpBinding fallback
                binding = AlternativeBindingHelper.CreateWSHttpBinding();
                Console.WriteLine("Using WSHttpBinding fallback");
            }

            EdiWcfRealTimeClient client = new EdiWcfRealTimeClient(binding, address);

            // Set client certificate from PFX file
            client.ClientCredentials.ClientCertificate.Certificate = clientCertificate;

            // Set server certificate from file
            client.ClientCredentials.ServiceCertificate.DefaultCertificate = serverCertificate;

            return client;
        }

        private CustomBinding CreateCustomBinding()
        {
            // Create custom binding based on WSDL specification - works with .NET Framework
            var binding = new CustomBinding();

            // Security binding element for mutual certificate authentication
            var security = new AsymmetricSecurityBindingElement();
            security.MessageSecurityVersion = MessageSecurityVersion.WSSecurity10WSTrustFebruary2005WSSecureConversationFebruary2005WSSecurityPolicy11BasicSecurityProfile10;
            security.InitiatorTokenParameters = new X509SecurityTokenParameters { InclusionMode = SecurityTokenInclusionMode.AlwaysToRecipient };
            security.RecipientTokenParameters = new X509SecurityTokenParameters { InclusionMode = SecurityTokenInclusionMode.Never };
            security.MessageProtectionOrder = MessageProtectionOrder.EncryptBeforeSign;
            security.SecurityHeaderLayout = SecurityHeaderLayout.LaxTimestampFirst;
            security.IncludeTimestamp = true;
            security.DefaultAlgorithmSuite = SecurityAlgorithmSuite.Basic256Sha256Rsa15;
            security.KeyEntropyMode = SecurityKeyEntropyMode.ServerEntropy;

            binding.Elements.Add(security);

            // Text message encoding (SOAP 1.2, UTF-8)
            var textEncoding = new TextMessageEncodingBindingElement();
            textEncoding.MessageVersion = MessageVersion.Soap12;
            textEncoding.WriteEncoding = System.Text.Encoding.UTF8;
            textEncoding.ReaderQuotas.MaxDepth = 32;
            textEncoding.ReaderQuotas.MaxStringContentLength = 1048576;
            textEncoding.ReaderQuotas.MaxArrayLength = 16384;
            textEncoding.ReaderQuotas.MaxBytesPerRead = 65536;
            textEncoding.ReaderQuotas.MaxNameTableCharCount = 16384;

            binding.Elements.Add(textEncoding);

            // HTTPS transport
            var httpsTransport = new HttpsTransportBindingElement();
            binding.Elements.Add(httpsTransport);

            // Set timeouts to match configuration
            binding.CloseTimeout = TimeSpan.FromMinutes(1);
            binding.OpenTimeout = TimeSpan.FromMinutes(1);
            binding.ReceiveTimeout = TimeSpan.FromMinutes(70);
            binding.SendTimeout = TimeSpan.FromMinutes(1);

            return binding;
        }
    }
}