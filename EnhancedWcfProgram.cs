using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Security;
using System.Threading.Tasks;
using System.Xml;

namespace DentaQuestTestHarness
{
    class EnhancedWcfProgram
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("DentaQuest EDI Enhanced WCF Test Harness (.NET 8)");
            Console.WriteLine("================================================");

            string testServiceUrl = "https://editest.dentaquest.com/TestEdiWcfRealTime/EdiWcfRealTime.svc";

            try
            {
                // Load certificates
                var clientCert = LoadClientCertificate();

                // Create enhanced WCF client with security
                var client = CreateSecureWcfClient(testServiceUrl, clientCert);

                // Get transaction
                string sampleEdiContent = GetEdiTransaction();

                Console.WriteLine($"\nSending EDI transaction to: {testServiceUrl}");
                Console.WriteLine($"Transaction length: {sampleEdiContent.Length} characters");

                // Create request
                var request = CreateEdiTransmission(sampleEdiContent);

                // Process transaction
                var response = await client.ProcessAsync(request);
                string responseContent = ExtractEdiContent(response);

                Console.WriteLine("\n=== RESPONSE RECEIVED ===");
                Console.WriteLine(responseContent);
                Console.WriteLine("==========================");

                if (client is ICommunicationObject commObj)
                {
                    try { commObj.Close(); } catch { commObj.Abort(); }
                }
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

        private static X509Certificate2 LoadClientCertificate()
        {
            var clientPfxPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "certificates", "airpay.pfx");
            if (File.Exists(clientPfxPath))
            {
                try
                {
                    var cert = new X509Certificate2(clientPfxPath, "changeit", X509KeyStorageFlags.Exportable);
                    Console.WriteLine($"✓ Client certificate loaded: {cert.Subject}");
                    return cert;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠ Could not load client certificate: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("⚠ Client certificate not found");
            }
            return null;
        }

        private static IEdiWcfRealTimeAsync CreateSecureWcfClient(string serviceUrl, X509Certificate2 clientCert)
        {
            // Create CustomBinding for SOAP 1.2 with HTTPS
            var binding = new CustomBinding();

            // Text message encoding with SOAP 1.2
            var textEncoding = new TextMessageEncodingBindingElement();
            textEncoding.MessageVersion = MessageVersion.Soap12WSAddressing10;
            textEncoding.WriteEncoding = System.Text.Encoding.UTF8;
            binding.Elements.Add(textEncoding);

            // HTTPS transport with client certificate
            var httpsTransport = new HttpsTransportBindingElement();
            httpsTransport.RequireClientCertificate = clientCert != null;
            binding.Elements.Add(httpsTransport);

            // Set timeouts
            binding.OpenTimeout = TimeSpan.FromMinutes(1);
            binding.CloseTimeout = TimeSpan.FromMinutes(1);
            binding.SendTimeout = TimeSpan.FromMinutes(1);
            binding.ReceiveTimeout = TimeSpan.FromMinutes(10);

            var endpointAddress = new EndpointAddress(serviceUrl);
            var factory = new ChannelFactory<IEdiWcfRealTimeAsync>(binding, endpointAddress);

            if (clientCert != null)
            {
                factory.Credentials.ClientCertificate.Certificate = clientCert;
            }

            // Ignore SSL certificate validation errors for testing
            factory.Credentials.ServiceCertificate.SslCertificateAuthentication =
                new X509ServiceCertificateAuthentication()
                {
                    CertificateValidationMode = X509CertificateValidationMode.None,
                    RevocationMode = X509RevocationMode.NoCheck
                };

            return factory.CreateChannel();
        }

        private static string GetEdiTransaction()
        {
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

            return TestTransactions.CleanEdiContent(sampleEdiContent);
        }

        private static XmlElement CreateEdiTransmission(string ediContent)
        {
            var doc = new XmlDocument();
            var element = doc.CreateElement("EDITRANSMISSION");
            element.InnerText = ediContent;
            return element;
        }

        private static string ExtractEdiContent(XmlElement ediTransmission)
        {
            return ediTransmission?.InnerText ?? string.Empty;
        }
    }

    // Async version of the service contract for .NET 8
    [ServiceContract(Namespace = "http://www.dentaquest.com/realtime")]
    public interface IEdiWcfRealTimeAsync
    {
        [OperationContract(Action = "Process", ReplyAction = "*")]
        Task<XmlElement> ProcessAsync(XmlElement EDITRANSMISSION);
    }
}