
using SharpPulsar.Interfaces;
using SharpPulsar.Utils;
using Xunit;

namespace SharpPulsar.Test
{
    public class MessageIdUtilsTest
    {
        [Theory]
        [InlineData(34, 5)]
        [InlineData(0, 78)]
        [InlineData(578, 0)]
        [InlineData(987, 123)]
        [InlineData(3, 500)]
        [InlineData(38, 50)]
        [InlineData(883, 60)]
        [InlineData(39, 5)]
        public void TestId(long ledger, long entry)
        {
            var id = new MessageId(ledger, entry, -1);
            var offset = MessageIdUtils.GetOffset(id);
            var id1 = MessageIdUtils.GetMessageId(offset);
            Assert.Equal(id, id1);
        }
    }
}
