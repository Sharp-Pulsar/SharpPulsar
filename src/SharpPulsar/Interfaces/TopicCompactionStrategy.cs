using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpPulsar.Interfaces
{
	/// <summary>
	/// Defines a custom strategy to compact messages in a topic.
	/// This strategy can be passed to Topic Compactor and Table View to compact messages in a custom way.
	/// 
	/// Examples:
	/// 
	/// TopicCompactionStrategy strategy = new MyTopicCompactionStrategy();
	/// 
	/// // Run topic compaction by the compaction strategy.
	/// // While compacting messages for each key,
	/// //   it will choose messages only if TopicCompactionStrategy.shouldKeepLeft(prev, cur) returns false.
	/// StrategicTwoPhaseCompactor compactor = new StrategicTwoPhaseCompactor(...);
	/// compactor.compact(topic, strategy);
	/// 
	/// // Run table view by the compaction strategy.
	/// // While updating messages in the table view <key,value> map,
	/// //   it will choose messages only if TopicCompactionStrategy.shouldKeepLeft(prev, cur) returns false.
	/// TableView tableView = pulsar.getClient().newTableViewBuilder(strategy.getSchema())
	///                 .topic(topic)
	///                 .loadConf(Map.of(
	///                         "topicCompactionStrategyClassName", strategy.getClass().getCanonicalName()))
	///                 .create();
	/// </summary>
	public interface ITopicCompactionStrategy<T>
    {

        /// <summary>
        /// Returns the schema object for this strategy.
        /// @return
        /// </summary>
        ISchema<T> Schema { get; }
        /// <summary>
        /// Tests if the compaction needs to keep the left(previous message)
        /// compared to the right(current message) for the same key.
        /// </summary>
        /// <param name="prev"> previous message value </param>
        /// <param name="cur"> current message value </param>
        /// <returns> True if it needs to keep the previous message and ignore the current message. Otherwise, False. </returns>
        bool ShouldKeepLeft(T prev, T cur);

        static ITopicCompactionStrategy<T> Load(string topicCompactionStrategyClassName)
        {
            if (topicCompactionStrategyClassName == null)
            {
                return null;
            }
            try
            {
                var x = Activator.CreateInstance(null, topicCompactionStrategyClassName);
                var instance = (ITopicCompactionStrategy<T>)x.Unwrap();
                return instance;
            }
            catch (Exception e)
            {
                throw new ArgumentException("Error when loading topic compaction strategy: " + topicCompactionStrategyClassName, e);
            }
        }
    }

}
