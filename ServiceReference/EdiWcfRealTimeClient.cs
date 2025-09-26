using System;
using System.ComponentModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading.Tasks;
using System.Xml;

namespace DentaQuestTestHarness.ServiceReference
{
    [ServiceContract(Namespace = "http://www.dentaquest.com/realtime", ConfigurationName = "EdiWcfRealTime")]
    public interface IEdiWcfRealTime
    {
        [OperationContract(Action = "Process", ReplyAction = "*")]
        XmlElement Process(XmlElement EDITRANSMISSION);

        [OperationContract(Action = "Process", ReplyAction = "*")]
        Task<XmlElement> ProcessAsync(XmlElement EDITRANSMISSION);
    }

    public partial class EdiWcfRealTimeClient : ClientBase<IEdiWcfRealTime>, IEdiWcfRealTime
    {
        public EdiWcfRealTimeClient()
        {
        }

        public EdiWcfRealTimeClient(string endpointConfigurationName) : base(endpointConfigurationName)
        {
        }

        public EdiWcfRealTimeClient(string endpointConfigurationName, string remoteAddress) : base(endpointConfigurationName, remoteAddress)
        {
        }

        public EdiWcfRealTimeClient(string endpointConfigurationName, EndpointAddress remoteAddress) : base(endpointConfigurationName, remoteAddress)
        {
        }

        public EdiWcfRealTimeClient(Binding binding, EndpointAddress remoteAddress) : base(binding, remoteAddress)
        {
        }

        public XmlElement Process(XmlElement EDITRANSMISSION)
        {
            return base.Channel.Process(EDITRANSMISSION);
        }

        public Task<XmlElement> ProcessAsync(XmlElement EDITRANSMISSION)
        {
            return base.Channel.ProcessAsync(EDITRANSMISSION);
        }
    }

    public static class EdiTransmissionHelper
    {
        public static XmlElement CreateEdiTransmission(string ediContent)
        {
            var doc = new XmlDocument();
            var element = doc.CreateElement("EDITRANSMISSION");
            element.InnerText = ediContent;
            return element;
        }

        public static string ExtractEdiContent(XmlElement ediTransmission)
        {
            return ediTransmission?.InnerText ?? string.Empty;
        }
    }
}