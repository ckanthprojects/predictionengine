using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace PredictionEngine
{
    [DataContract]
    public class WordsService
    {
        [DataMember]
        public string Value { get; set; }
    }
}
