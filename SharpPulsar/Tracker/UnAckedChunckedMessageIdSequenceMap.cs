using System.Collections.Generic;
using Akka.Actor;
using SharpPulsar.Batch;
using SharpPulsar.Tracker.Messages;

namespace SharpPulsar.Tracker
{
    public class UnAckedChunckedMessageIdSequenceMap: ReceiveActor, IWithUnboundedStash
    {
        private readonly Dictionary<MessageId, MessageId[]> _unAckedChunckedMessageIdSequenceMap;
        public UnAckedChunckedMessageIdSequenceMap()
        {
            _unAckedChunckedMessageIdSequenceMap = new Dictionary<MessageId, MessageId[]>();
            Receive<UnAckedChunckedMessageIdSequenceMapCmd>(r =>
            {
                var ids = new List<MessageId>();
                var cmd = r.Command;
                var messageIds = r.MessageId;
                foreach (var msgId in messageIds)
                {
                    MessageId msgid;
                    if (msgId is BatchMessageId id)
                        msgid = new MessageId(id.LedgerId, id.EntryId, id.PartitionIndex);
                    else
                        msgid = (MessageId)msgId;

                    if (cmd == UnAckedCommand.Remove)
                    {
                        if (_unAckedChunckedMessageIdSequenceMap.ContainsKey(msgid))
                            _unAckedChunckedMessageIdSequenceMap.Remove(msgid);
                        continue;
                    }
                    if (cmd == UnAckedCommand.GetRemoved && _unAckedChunckedMessageIdSequenceMap.TryGetValue(msgid, out var removed))
                    {
                        _unAckedChunckedMessageIdSequenceMap.Remove(msgid);
                        ids.AddRange(removed);
                    }
                    if (cmd == UnAckedCommand.Get && _unAckedChunckedMessageIdSequenceMap.ContainsKey(msgid))
                    {
                        var mIds = _unAckedChunckedMessageIdSequenceMap[msgid];
                        ids.AddRange(mIds);
                    }
                }
                if(cmd == UnAckedCommand.Get || cmd == UnAckedCommand.GetRemoved)
                    Sender.Tell(new UnAckedChunckedMessageIdSequenceMapCmdResponse(ids.ToArray()));
            });
            Receive<Clear>(_=> _unAckedChunckedMessageIdSequenceMap.Clear());
            Receive<AddMessageIds>(a=> _unAckedChunckedMessageIdSequenceMap.Add(a.MessageId, a.MessageIds));
        }
        
        public static Props Prop()
        {
            return Props.Create(()=> new UnAckedChunckedMessageIdSequenceMap());
        }
        public IStash Stash { get; set; }
    }

    public sealed class AddMessageIds
    {
        public MessageId MessageId { get; }
        public MessageId[] MessageIds { get; }

        public AddMessageIds(MessageId id, MessageId[] ids)
        {
            MessageId = id;
            MessageIds = ids;
        }
    }
}
