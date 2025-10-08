using System;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace DentaQuestTestHarness
{
    /// <summary>
    /// Simple HTTP client for EDI services - works reliably across platforms
    /// </summary>
    public class SimpleEdiClient : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly string _serviceUrl;

        public SimpleEdiClient(string serviceUrl, X509Certificate2 clientCertificate = null)
        {
            _serviceUrl = serviceUrl;

#if NET451
            var handler = new WebRequestHandler();
#else
            var handler = new HttpClientHandler();
#endif

            if (clientCertificate != null)
            {
                handler.ClientCertificates.Add(clientCertificate);
            }

            _httpClient = new HttpClient(handler);
            _httpClient.DefaultRequestHeaders.Add("SOAPAction", "Process");
        }

        public async Task<string> ProcessAsync(string ediContent)
        {
            var soapEnvelope = CreateSoapEnvelope(ediContent);
            var content = new StringContent(soapEnvelope, Encoding.UTF8, "application/soap+xml");

            try
            {
                var response = await _httpClient.PostAsync(_serviceUrl, content);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                return ExtractEdiContentFromSoapResponse(responseContent);
            }
            catch (Exception ex)
            {
                throw new Exception($"EDI service call failed: {ex.Message}", ex);
            }
        }

        private string CreateSoapEnvelope(string ediContent)
        {
            return $@"<?xml version=""1.0"" encoding=""utf-8""?>
<soap12:Envelope xmlns:soap12=""http://www.w3.org/2003/05/soap-envelope""
                 xmlns:tem=""http://www.dentaquest.com/realtime"">
    <soap12:Header />
    <soap12:Body>
        <tem:Process>
            <tem:EDITRANSMISSION>{System.Security.SecurityElement.Escape(ediContent)}</tem:EDITRANSMISSION>
        </tem:Process>
    </soap12:Body>
</soap12:Envelope>";
        }

        private string ExtractEdiContentFromSoapResponse(string soapResponse)
        {
            try
            {
                var doc = new XmlDocument();
                doc.LoadXml(soapResponse);

                var namespaceManager = new XmlNamespaceManager(doc.NameTable);
                namespaceManager.AddNamespace("soap12", "http://www.w3.org/2003/05/soap-envelope");
                namespaceManager.AddNamespace("tem", "http://www.dentaquest.com/realtime");

                var responseNode = doc.SelectSingleNode("//tem:ProcessResponse/tem:ProcessResult", namespaceManager);
                return responseNode?.InnerText ?? soapResponse;
            }
            catch
            {
                // Fallback: return raw response if XML parsing fails
                return soapResponse;
            }
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}