using System;
using System.Runtime.Serialization;

namespace PDFRendererModels
{
    [DataContract]
    public class Message<TPayload>
    {
        [DataMember]
        public Guid Id { get; set; }
        [DataMember]
        public TPayload Payload { get; set; }
    }
}
