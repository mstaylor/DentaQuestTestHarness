using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace DentaQuestTestHarness
{
    class SimpleProgram
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("DentaQuest EDI Simple HTTP Test Harness");
            Console.WriteLine("=======================================");

            // Test environment URL
            string testServiceUrl = "https://editest.dentaquest.com/TestEdiWcfRealTime/EdiWcfRealTime.svc";

            try
            {
                // Load client certificate if available
                X509Certificate2 clientCert = null;
                var clientPfxPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "certificates", "airpay.pfx");
                if (File.Exists(clientPfxPath))
                {
                    try
                    {
                        clientCert = new X509Certificate2(clientPfxPath, "changeit", X509KeyStorageFlags.Exportable);
                        Console.WriteLine($"✓ Client certificate loaded: {clientCert.Subject}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"⚠ Could not load client certificate: {ex.Message}");
                        Console.WriteLine("Proceeding without client certificate...");
                    }
                }
                else
                {
                    Console.WriteLine("⚠ Client certificate not found, proceeding without...");
                }

                using var client = new SimpleEdiClient(testServiceUrl, clientCert);

                // Choose transaction type
                Console.WriteLine("\nSelect transaction type:");
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

                Console.WriteLine($"\nSending EDI transaction to: {testServiceUrl}");
                Console.WriteLine($"Transaction length: {sampleEdiContent.Length} characters");

                var response = await client.ProcessAsync(sampleEdiContent);

                Console.WriteLine("\n=== RESPONSE RECEIVED ===");
                Console.WriteLine(response);
                Console.WriteLine("==========================");
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
}