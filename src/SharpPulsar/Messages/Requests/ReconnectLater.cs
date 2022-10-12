using Akka.Actor;
using SharpPulsar.Protocol.Proto;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace SharpPulsar.Messages.Requests
{
    public sealed class ReconnectLater
    {
        public Exception Exception { get; }
        public ReconnectLater(Exception exception)
        {
            Exception = exception;
        }
    }
    //HandleCommandWatchTopicUpdate 
    public sealed class HandleWatchTopicUpdate
    {
        public CommandWatchTopicUpdate Update { get; }
        public HandleWatchTopicUpdate(CommandWatchTopicUpdate update)
        {
            Update = update;
        }
    }
    public sealed class TopicsRemoved
    {
        public ImmutableList<string> RemovedTopics { get; }
        public TopicsRemoved(ICollection<string> removedTopics) 
        { 
            RemovedTopics = removedTopics.ToImmutableList();
        }
    }
    public sealed class TopicsAdded
    {
        public ImmutableList<string> AddedTopics { get; }
        public TopicsAdded(ICollection<string> addedTopics)
        {
            AddedTopics = addedTopics.ToImmutableList();  
        }
    }
    public sealed class ResetBackoff
    {
        public static ResetBackoff Instance = new ResetBackoff();
    }
    public sealed class LastConnectionClosedTimestamp
    {
        public static LastConnectionClosedTimestamp Instance = new LastConnectionClosedTimestamp();
    }
    public sealed class LastConnectionClosedTimestampResponse
    {
        public long TimeStamp { get; }
        public LastConnectionClosedTimestampResponse(long timeStamp)
        {
            TimeStamp = timeStamp;
        }
    }

    public sealed class GetCnx
    {
        public static GetCnx Instance = new GetCnx();
    }
    public sealed class SetCnx
    {
        public IActorRef ClientCnx { get; }
        public SetCnx(IActorRef clientCnx)
        {
            ClientCnx = clientCnx;
        }
    }
    public sealed class GetEpoch
    {
        public static GetEpoch Instance = new GetEpoch();
    }
    public sealed class GetEpochResponse
    {
        public long Epoch { get; }
        public GetEpochResponse(long epoch)
        {
            Epoch = epoch;
        }
    }
    public sealed class GrabCnx
    {
        public string Message { get; }
        public GrabCnx(string message)
        {
            Message = message;
        }
    }
    public sealed class Grab
    {
        public static Grab Instance = new Grab();
    }
}
