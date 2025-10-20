using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace SharpPulsar.Messages.Requests
{
    public record ReconnectLater
    {
        public Exception Exception { get; }
        public ReconnectLater(Exception exception)
        {
            Exception = exception;
        }
    }
    //HandleCommandWatchTopicUpdate 
    public record HandleWatchTopicUpdate
    {
        public CommandWatchTopicUpdate Update { get; }
        public HandleWatchTopicUpdate(CommandWatchTopicUpdate update)
        {
            Update = update;
        }
    }
    public record TopicsRemoved
    {
        public ImmutableList<string> RemovedTopics { get; }
        public TopicsRemoved(ICollection<string> removedTopics) 
        { 
            RemovedTopics = removedTopics.ToImmutableList();
        }
    }
    public record AppendTopicsRemovedOp
    {
        public ImmutableList<string> DeletedTopics { get; }
        public AppendTopicsRemovedOp(ICollection<string> deletedTopics)
        {
            DeletedTopics = deletedTopics.ToImmutableList();
        }
    }
    public record AppendTopicsAddedOp
    {
        public ImmutableList<string> NewTopics { get; }
        public AppendTopicsAddedOp(ICollection<string> newTopics)
        {
            NewTopics = newTopics.ToImmutableList();
        }
    }
    public record TopicsAdded
    {
        public ImmutableList<string> AddedTopics { get; }
        public TopicsAdded(ICollection<string> addedTopics)
        {
            AddedTopics = addedTopics.ToImmutableList();  
        }
    }
    public record ResetBackoff
    {
        public static ResetBackoff Instance = new ResetBackoff();
    }
    public record LastConnectionClosedTimestamp
    {
        public static LastConnectionClosedTimestamp Instance = new LastConnectionClosedTimestamp();
    }
    public record LastConnectionClosedTimestampResponse
    {
        public long TimeStamp { get; }
        public LastConnectionClosedTimestampResponse(long timeStamp)
        {
            TimeStamp = timeStamp;
        }
    }

    public record GetCnx
    {
        public static GetCnx Instance = new GetCnx();
    }
    public record SetCnx
    {
        public IActorRef ClientCnx { get; }
        public SetCnx(IActorRef clientCnx)
        {
            ClientCnx = clientCnx;
        }
    }
    public record GetEpoch
    {
        public static GetEpoch Instance = new GetEpoch();
    }
    public record SwitchClientCnx
    {
        public IActorRef ClientCnx { get; }
        public SwitchClientCnx(IActorRef clientCnx)
        {
            ClientCnx = clientCnx;
        }
    }

    public record GetEpochResponse
    {
        public long Epoch { get; }
        public GetEpochResponse(long epoch)
        {
            Epoch = epoch;
        }
    }
    public record GrabCnx
    {
        public static GrabCnx Instance = new GrabCnx();    
        public string Message { get; }
        public GrabCnx(string message)
        {
            Message = message;
        }
    }
    public record Grab
    {
        public static Grab Instance = new Grab();
    }
}
