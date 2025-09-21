using System.Collections.Generic;
using Akka.Actor;
using SharpPulsar.Batch;
using SharpPulsar.Tracker.Messages;

namespace SharpPulsar.Tracker
{
    public class UnAckedChunckedMessageIdSequenceMap: ReceiveActor
    {
        private readonly Dictionary<IMessageIdAdv, MessageIdAdv[]> _unAckedChunckedMessageIdSequenceMap;
        public UnAckedChunckedMessageIdSequenceMap()
        {
            _unAckedChunckedMessageIdSequenceMap = new Dictionary<IMessageIdAdv, MessageIdAdv[]>();
            Receive<UnAckedChunckedMessageIdSequenceMapCmd>(r =>
            {
                var ids = new List<MessageIdAdv>();
                var cmd = r.Command;
                var messageIds = r.MessageId;
                foreach (var msgId in messageIds)
                {
                    MessageIdAdv msgid;
                    if (msgId is BatchMessageId id)
                        msgid = new MessageIdAdv(id.LedgerId, id.EntryId, id.PartitionIndex);
                    else if (msgId is TopicMessageId tmid)
                        msgid = (MessageIdAdv)tmid.MessageId;
                    else
                        msgid = (MessageIdAdv)msgId;

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

    }

    public sealed class AddMessageIds
    {
        public MessageIdAdv MessageId { get; }
        public MessageIdAdv[] MessageIds { get; }

        public AddMessageIds(MessageIdAdv id, MessageIdAdv[] ids)
        {
            MessageId = id;
            MessageIds = ids;
        }
    }
}
