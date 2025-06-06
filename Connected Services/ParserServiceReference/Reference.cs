﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SML.ParserServiceReference {
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="ParserServiceReference.IParserService")]
    public interface IParserService {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IParserService/GetData", ReplyAction="http://tempuri.org/IParserService/GetDataResponse")]
        string GetData(string filePath);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IParserService/GetData", ReplyAction="http://tempuri.org/IParserService/GetDataResponse")]
        System.Threading.Tasks.Task<string> GetDataAsync(string filePath);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IParserServiceChannel : SML.ParserServiceReference.IParserService, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class ParserServiceClient : System.ServiceModel.ClientBase<SML.ParserServiceReference.IParserService>, SML.ParserServiceReference.IParserService {
        
        public ParserServiceClient() {
        }
        
        public ParserServiceClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public ParserServiceClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public ParserServiceClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public ParserServiceClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public string GetData(string filePath) {
            return base.Channel.GetData(filePath);
        }
        
        public System.Threading.Tasks.Task<string> GetDataAsync(string filePath) {
            return base.Channel.GetDataAsync(filePath);
        }
    }
}
