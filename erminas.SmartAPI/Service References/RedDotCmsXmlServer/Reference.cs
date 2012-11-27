﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.544
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace erminas.SmartAPI.RedDotCmsXmlServer
{
    [GeneratedCode("System.ServiceModel", "4.0.0.0")]
    [ServiceContract(Namespace = "http://tempuri.org/RDCMSXMLServer/webservice/",
        ConfigurationName = "RedDotCmsXmlServer.XmlServerSoapPort")]
    public interface XmlServerSoapPort
    {
        // CODEGEN: Generating message contract since the wrapper namespace (http://tempuri.org/RDCMSXMLServer/message/) of message ExecuteRequest does not match the default value (http://tempuri.org/RDCMSXMLServer/webservice/)
        [OperationContract(Action = "http://tempuri.org/RDCMSXMLServer/action/XmlServer.Execute", ReplyAction = "*")]
        [XmlSerializerFormat(Style = OperationFormatStyle.Rpc, SupportFaults = true, Use = OperationFormatUse.Encoded)]
        ExecuteResponse Execute(ExecuteRequest request);
    }

    [DebuggerStepThrough]
    [GeneratedCode("System.ServiceModel", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    [MessageContract(WrapperName = "Execute", WrapperNamespace = "http://tempuri.org/RDCMSXMLServer/message/",
        IsWrapped = true)]
    public class ExecuteRequest
    {
        [MessageBodyMember(Namespace = "", Order = 1)] public object sErrorA;
        [MessageBodyMember(Namespace = "", Order = 0)] public string sParamA;

        [MessageBodyMember(Namespace = "", Order = 2)] public object sResultInfoA;

        public ExecuteRequest()
        {
        }

        public ExecuteRequest(string sParamA, object sErrorA, object sResultInfoA)
        {
            this.sParamA = sParamA;
            this.sErrorA = sErrorA;
            this.sResultInfoA = sResultInfoA;
        }
    }

    [DebuggerStepThrough]
    [GeneratedCode("System.ServiceModel", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    [MessageContract(WrapperName = "ExecuteResponse", WrapperNamespace = "http://tempuri.org/RDCMSXMLServer/message/",
        IsWrapped = true)]
    public class ExecuteResponse
    {
        [MessageBodyMember(Namespace = "", Order = 0)] public string Result;

        [MessageBodyMember(Namespace = "", Order = 1)] public object sErrorA;

        [MessageBodyMember(Namespace = "", Order = 2)] public object sResultInfoA;

        public ExecuteResponse()
        {
        }

        public ExecuteResponse(string Result, object sErrorA, object sResultInfoA)
        {
            this.Result = Result;
            this.sErrorA = sErrorA;
            this.sResultInfoA = sResultInfoA;
        }
    }

    [GeneratedCode("System.ServiceModel", "4.0.0.0")]
    public interface XmlServerSoapPortChannel : XmlServerSoapPort, IClientChannel
    {
    }

    [DebuggerStepThrough]
    [GeneratedCode("System.ServiceModel", "4.0.0.0")]
    public class XmlServerSoapPortClient : ClientBase<XmlServerSoapPort>, XmlServerSoapPort
    {
        public XmlServerSoapPortClient()
        {
        }

        public XmlServerSoapPortClient(string endpointConfigurationName) :
            base(endpointConfigurationName)
        {
        }

        public XmlServerSoapPortClient(string endpointConfigurationName, string remoteAddress) :
            base(endpointConfigurationName, remoteAddress)
        {
        }

        public XmlServerSoapPortClient(string endpointConfigurationName, EndpointAddress remoteAddress) :
            base(endpointConfigurationName, remoteAddress)
        {
        }

        public XmlServerSoapPortClient(Binding binding, EndpointAddress remoteAddress) :
            base(binding, remoteAddress)
        {
        }

        #region XmlServerSoapPort Members

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        ExecuteResponse XmlServerSoapPort.Execute(ExecuteRequest request)
        {
            return base.Channel.Execute(request);
        }

        #endregion

        public string Execute(string sParamA, ref object sErrorA, ref object sResultInfoA)
        {
            ExecuteRequest inValue = new ExecuteRequest();
            inValue.sParamA = sParamA;
            inValue.sErrorA = sErrorA;
            inValue.sResultInfoA = sResultInfoA;
            ExecuteResponse retVal = ((XmlServerSoapPort) (this)).Execute(inValue);
            sErrorA = retVal.sErrorA;
            sResultInfoA = retVal.sResultInfoA;
            return retVal.Result;
        }
    }
}