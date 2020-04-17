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

    public partial class CursorStats
    {
        /// <summary>
        /// Initializes a new instance of the CursorStats class.
        /// </summary>
        public CursorStats()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the CursorStats class.
        /// </summary>
        public CursorStats(string markDeletePosition = default(string), string readPosition = default(string), bool? waitingReadOp = default(bool?), int? pendingReadOps = default(int?), long? messagesConsumedCounter = default(long?), long? cursorLedger = default(long?), long? cursorLedgerLastEntry = default(long?), string individuallyDeletedMessages = default(string), string lastLedgerSwitchTimestamp = default(string), string state = default(string), long? numberOfEntriesSinceFirstNotAckedMessage = default(long?), int? totalNonContiguousDeletedMessagesRange = default(int?), IDictionary<string, long?> properties = default(IDictionary<string, long?>))
        {
            MarkDeletePosition = markDeletePosition;
            ReadPosition = readPosition;
            WaitingReadOp = waitingReadOp;
            PendingReadOps = pendingReadOps;
            MessagesConsumedCounter = messagesConsumedCounter;
            CursorLedger = cursorLedger;
            CursorLedgerLastEntry = cursorLedgerLastEntry;
            IndividuallyDeletedMessages = individuallyDeletedMessages;
            LastLedgerSwitchTimestamp = lastLedgerSwitchTimestamp;
            State = state;
            NumberOfEntriesSinceFirstNotAckedMessage = numberOfEntriesSinceFirstNotAckedMessage;
            TotalNonContiguousDeletedMessagesRange = totalNonContiguousDeletedMessagesRange;
            Properties = properties;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "markDeletePosition")]
        public string MarkDeletePosition { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "readPosition")]
        public string ReadPosition { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "waitingReadOp")]
        public bool? WaitingReadOp { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "pendingReadOps")]
        public int? PendingReadOps { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "messagesConsumedCounter")]
        public long? MessagesConsumedCounter { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "cursorLedger")]
        public long? CursorLedger { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "cursorLedgerLastEntry")]
        public long? CursorLedgerLastEntry { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "individuallyDeletedMessages")]
        public string IndividuallyDeletedMessages { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "lastLedgerSwitchTimestamp")]
        public string LastLedgerSwitchTimestamp { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "state")]
        public string State { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "numberOfEntriesSinceFirstNotAckedMessage")]
        public long? NumberOfEntriesSinceFirstNotAckedMessage { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "totalNonContiguousDeletedMessagesRange")]
        public int? TotalNonContiguousDeletedMessagesRange { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "properties")]
        public IDictionary<string, long?> Properties { get; set; }

    }
}
