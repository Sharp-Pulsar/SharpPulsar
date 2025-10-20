using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Akka.Actor;
using SharpPulsar.Messages.Consumer;
using SharpPulsar.Messages.Requests;

namespace SharpPulsar.Internal.Consumer
{
    internal class PatternConsumerUpdateQueue : ReceiveActor, IWithTimers
    {
        private enum UpdateSubscriptionType
        {
            /// <summary>
            /// A marker that indicates the consumer's subscribe task.* </summary>
            CONSUMER_INIT,
            /// <summary>
            /// Triggered by <seealso cref="PatternMultiTopicsConsumerImpl.topicsChangeListener"/>.* </summary>
            TOPICS_ADDED,
            /// <summary>
            /// Triggered by <seealso cref="PatternMultiTopicsConsumerImpl.topicsChangeListener"/>.* </summary>
            TOPICS_REMOVED,
            /// <summary>
            /// A fully check for pattern consumer. * </summary>
            RECHECK
        }

        private static readonly KeyValuePair<UpdateSubscriptionType, ICollection<string>> RECHECK_OP = new KeyValuePair<UpdateSubscriptionType, ICollection<string>>(UpdateSubscriptionType.RECHECK, null);

        private readonly BlockingCollection<KeyValuePair<UpdateSubscriptionType, ICollection<string>>> _pendingTasks;

        private readonly IActorRef _patternConsumer;

        //private readonly PatternMultiTopicsConsumerImpl.TopicsChangedListener topicsChangeListener;

        /// <summary>
        /// Whether there is a task is in progress, this variable is used to confirm whether a next-task triggering is
        /// needed.
        /// </summary>
        private KeyValuePair<UpdateSubscriptionType, ValueTask> _taskInProgress = default;

        /// <summary>
        /// Whether there is a recheck task in queue.
        /// - Since recheck task will do all changes, it can be used to compress multiple tasks to one.
        /// - To avoid skipping the newest changes, once the recheck task is starting to work, this variable will be set
        ///   to "false".
        /// </summary>
        private bool _recheckTaskInQueue = false;

        private long _lastRecheckTaskStartingTimestamp = 0;

        private bool _closed;
        private readonly ILoggingAdapter _log;
        private bool _taskInProgressBool;
        public ITimerScheduler Timers { get; set; }

        /// <summary>
        /// This constructor is only for test. * </summary>
        public PatternConsumerUpdateQueue(IActorRef patternConsumer)
        {
            _log = Context.GetLogger();
            _patternConsumer = patternConsumer;
            _pendingTasks = new BlockingCollection<KeyValuePair<UpdateSubscriptionType, ICollection<string>>>();
            // To avoid subscribing and topics changed events execute concurrently, let the change events starts after the
            // subscribing task.
            ReceiveAsync<CancelAllAndWaitForTheRunningTask>(async _ => await CancelAllAndWaitForTheRunningTask());
            Receive<LastRecheckTaskStartingTimestamp>(_ =>
            {
                var failedTime = DateTimeHelper.CurrentUnixTimeMillis();
                if (_lastRecheckTaskStartingTimestamp <= failedTime)
                {
                    AppendRecheckOp();
                }
            });
            Receive<TriggerNext>(_ => TriggerNextTask());
            Receive<AppendRecheckOp>(_ =>
            {
                AppendRecheckOp();
            });
            Receive<AppendTopicsRemovedOp>(a =>
            {
                AppendTopicsRemovedOp(a.DeletedTopics);
            });
            Receive<AppendTopicsAddedOp>(a =>
            {
                AppendTopicsAddedOp(a.NewTopics);
            });
            DoAppend(new KeyValuePair<UpdateSubscriptionType, ICollection<string>>(UpdateSubscriptionType.CONSUMER_INIT, null));
        }
        public static Props Prop(IActorRef patternConsumer)
        {
            return Props.Create(() => new PatternConsumerUpdateQueue(patternConsumer));
        }
        private void AppendTopicsAddedOp(ICollection<string> topics)
        {
            if (topics == null || topics.Count == 0)
            {
                return;
            }
            DoAppend(new KeyValuePair<UpdateSubscriptionType, ICollection<string>>(UpdateSubscriptionType.TOPICS_ADDED, topics));
        }

        private void AppendTopicsRemovedOp(ICollection<string> topics)
        {
            if (topics == null || topics.Count == 0)
            {
                return;
            }
            DoAppend(new KeyValuePair<UpdateSubscriptionType, ICollection<string>>(UpdateSubscriptionType.TOPICS_REMOVED, topics));
        }

        private void AppendRecheckOp()
        {
            DoAppend(RECHECK_OP);
        }


        private void DoAppend(KeyValuePair<UpdateSubscriptionType, ICollection<string>> task)
        {
            if (_log.IsDebugEnabled)
            {
                var sub = _patternConsumer.Ask<string>(new GetSubscription()).GetAwaiter().GetResult();
                var v = task.Value == null ? "" : task.Value.ToList().ToString();
                _log.Debug($"Pattern consumer [{sub}] try to append task. {task.Key} {v}");
            }
            // Once there is a recheck task in queue, it means other tasks can be skipped.
            if (_recheckTaskInQueue)
            {
                return;
            }

            // Once there are too many tasks in queue, compress them as a recheck task.
            if (_pendingTasks.Count >= 30 && !task.Key.Equals(UpdateSubscriptionType.RECHECK))
            {
                AppendRecheckOp();
                return;
            }

            _pendingTasks.Add(task);
            if (task.Key.Equals(UpdateSubscriptionType.RECHECK))
            {
                _recheckTaskInQueue = true;
            }

            // If no task is in-progress, trigger a task execution.
            if (!_taskInProgressBool)
            {
                TriggerNextTask();
                _taskInProgressBool = true;
            }
        }
        private void TriggerNextTask()
        {
            if (_closed)
            {
                return;
            }

            var task = _pendingTasks.Take();

            // No pending task.
            /*if (task.Value == null)
            {
                _taskInProgress = default;
                return;
            }*/

            // If there is a recheck task in queue, skip others and only call the recheck task.
            if (_recheckTaskInQueue && !task.Key.Equals(UpdateSubscriptionType.RECHECK))
            {
                Self.Tell(TriggerNext.Instance);
                //TriggerNextTask();
                return;
            }

            // Execute pending task.
            var sub1 = _patternConsumer.Ask<string>(new GetSubscription()).GetAwaiter().GetResult();
            if (_log.IsDebugEnabled)
            {
                _log.Debug($"Pattern consumer [{sub1}] starting task. {task.Key} {task.Value}");
            }
            try
            {
                var sub = "";
                switch (task.Key)
                {
                    case UpdateSubscriptionType.CONSUMER_INIT:
                        {

                            try
                            {
                                sub = _patternConsumer.Ask<string>(new GetSubscription()).GetAwaiter().GetResult();
                            }
                            catch (Exception ex)
                            {
                                // If the subscribe future was failed, the consumer will be closed.
                                _closed = true;
                                var ask = _patternConsumer.Ask<AskResponse>(Close.Instance).GetAwaiter().GetResult();
                                _log.Error($"Pattern consumer failed to close, this error may left orphan consumers. Subscription: {sub}. Exception: {ask.ToString}");
                            }
                            break;
                        }

                    case UpdateSubscriptionType.TOPICS_ADDED:
                        {
                            _patternConsumer.Tell(new TopicsAdded(task.Value));
                            break;
                        }
                    case UpdateSubscriptionType.TOPICS_REMOVED:
                        {
                            _patternConsumer.Tell(new TopicsRemoved(task.Value));
                            break;
                        }
                    case UpdateSubscriptionType.RECHECK:
                        {
                            _recheckTaskInQueue = false;
                            _lastRecheckTaskStartingTimestamp = DateTimeHelper.CurrentUnixTimeMillis();
                            _patternConsumer.Tell(RecheckTopicsChange.Instance);
                            break;
                        }
                    default:
                        {
                            throw new Exception("Un-support UpdateSubscriptionType");
                        }
                }
                if (_log.IsErrorEnabled)
                {
                    sub = _patternConsumer.Ask<string>(new GetSubscription()).GetAwaiter().GetResult();
                    _log.Debug($"Pattern consumer [{sub}] task finished. {task.Key} {task.Value} ");
                }
                Self.Tell(TriggerNext.Instance);
            }
            catch
            {
                var sub = _patternConsumer.Ask<string>(new GetSubscription()).GetAwaiter().GetResult();
                // Trigger next pending task.
                /// <summary>
                /// Once a updating fails, trigger a delayed new recheck task to guarantee all things is correct.
                /// - Skip if there is already a recheck task in queue.
                /// - Skip if the last recheck task has been executed after the current time.
                /// </summary>
                _log.Error($"Pattern consumer [{sub}] task finished. {task.Key} {task.Value}. But it failed");
                // Skip if there is already a recheck task in queue.
                if (_recheckTaskInQueue || _closed)
                {
                    return;
                }
                // Skip if the last recheck task has been executed after the current time.
                Timers.StartSingleTimer(LastRecheckTaskStartingTimestamp.Instance, LastRecheckTaskStartingTimestamp.Instance, TimeSpan.FromSeconds(10));

                TriggerNextTask();
            }


        }
        private async ValueTask CancelAllAndWaitForTheRunningTask()
        {
            _closed = true;
            /*if (_taskInProgress == null)
            {
                return;
            }*/
            // If the in-progress task is consumer init task, it means nothing is in-progress.
            if (_taskInProgress.Key.Equals(UpdateSubscriptionType.CONSUMER_INIT))
            {
                return;
            }
            await _taskInProgress.Value.AsTask().ContinueWith(async t =>
            {
                await t;
                return;
            });
        }
    }
    public record AppendRecheckOp
    {
        public static AppendRecheckOp Instance = new AppendRecheckOp();
    }
    public record RecheckTopicsChange
    {
        public static RecheckTopicsChange Instance { get; } = new RecheckTopicsChange();
    }
    public record CancelAllAndWaitForTheRunningTask
    {
        public static CancelAllAndWaitForTheRunningTask Instance = new CancelAllAndWaitForTheRunningTask();
    }
    public record RecheckTopicsChangeAfterReconnect
    {
        public static RecheckTopicsChangeAfterReconnect Instance { get; } = new RecheckTopicsChangeAfterReconnect();
    }

    internal record LastRecheckTaskStartingTimestamp
    {
        public static LastRecheckTaskStartingTimestamp Instance { get; } = new LastRecheckTaskStartingTimestamp();
    }
    internal record TriggerNext
    {
        public static TriggerNext Instance { get; } = new TriggerNext();
    }
}
