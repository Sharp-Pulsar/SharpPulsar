// <auto-generated>
// Code generated by Microsoft (R) AutoRest Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>

namespace PulsarAdmin.Models
{
    using Newtonsoft.Json;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    public partial class PersistentOfflineTopicStats
    {
        /// <summary>
        /// Initializes a new instance of the PersistentOfflineTopicStats
        /// class.
        /// </summary>
        public PersistentOfflineTopicStats()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the PersistentOfflineTopicStats
        /// class.
        /// </summary>
        public PersistentOfflineTopicStats(long? storageSize = default(long?), long? totalMessages = default(long?), long? messageBacklog = default(long?), string brokerName = default(string), string topicName = default(string), IList<LedgerDetails> dataLedgerDetails = default(IList<LedgerDetails>), IDictionary<string, CursorDetails> cursorDetails = default(IDictionary<string, CursorDetails>), System.DateTime? statGeneratedAt = default(System.DateTime?))
        {
            StorageSize = storageSize;
            TotalMessages = totalMessages;
            MessageBacklog = messageBacklog;
            BrokerName = brokerName;
            TopicName = topicName;
            DataLedgerDetails = dataLedgerDetails;
            CursorDetails = cursorDetails;
            StatGeneratedAt = statGeneratedAt;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "storageSize")]
        public long? StorageSize { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "totalMessages")]
        public long? TotalMessages { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "messageBacklog")]
        public long? MessageBacklog { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "brokerName")]
        public string BrokerName { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "topicName")]
        public string TopicName { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "dataLedgerDetails")]
        public IList<LedgerDetails> DataLedgerDetails { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "cursorDetails")]
        public IDictionary<string, CursorDetails> CursorDetails { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "statGeneratedAt")]
        public System.DateTime? StatGeneratedAt { get; set; }

    }
}
