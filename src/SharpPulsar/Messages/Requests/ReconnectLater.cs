using Akka.Actor;
using SharpPulsar.Protocol.Proto;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace SharpPulsar.Messages.Requests
{
    public readonly record struct ReconnectLater
    {
        public Exception Exception { get; }
        public ReconnectLater(Exception exception)
        {
            Exception = exception;
        }
    }
    //HandleCommandWatchTopicUpdate 
    public readonly record struct HandleWatchTopicUpdate
    {
        public CommandWatchTopicUpdate Update { get; }
        public HandleWatchTopicUpdate(CommandWatchTopicUpdate update)
        {
            Update = update;
        }
    }
    public readonly record struct TopicsRemoved
    {
        public ImmutableList<string> RemovedTopics { get; }
        public TopicsRemoved(ICollection<string> removedTopics) 
        { 
            RemovedTopics = removedTopics.ToImmutableList();
        }
    }
    public readonly record struct TopicsAdded
    {
        public ImmutableList<string> AddedTopics { get; }
        public TopicsAdded(ICollection<string> addedTopics)
        {
            AddedTopics = addedTopics.ToImmutableList();  
        }
    }
    public readonly record struct ResetBackoff
    {
        public static ResetBackoff Instance = new ResetBackoff();
    }
    public readonly record struct LastConnectionClosedTimestamp
    {
        public static LastConnectionClosedTimestamp Instance = new LastConnectionClosedTimestamp();
    }
    public readonly record struct LastConnectionClosedTimestampResponse
    {
        public long TimeStamp { get; }
        public LastConnectionClosedTimestampResponse(long timeStamp)
        {
            TimeStamp = timeStamp;
        }
    }

    public readonly record struct GetCnx
    {
        public static GetCnx Instance = new GetCnx();
    }
    public readonly record struct SetCnx
    {
        public IActorRef ClientCnx { get; }
        public SetCnx(IActorRef clientCnx)
        {
            ClientCnx = clientCnx;
        }
    }
    public readonly record struct GetEpoch
    {
        public static GetEpoch Instance = new GetEpoch();
    }
    public readonly record struct SwitchClientCnx
    {
        public IActorRef ClientCnx { get; }
        public SwitchClientCnx(IActorRef clientCnx)
        {
            ClientCnx = clientCnx;
        }
    }

    public readonly record struct GetEpochResponse
    {
        public long Epoch { get; }
        public GetEpochResponse(long epoch)
        {
            Epoch = epoch;
        }
    }
    public readonly record struct GrabCnx
    {
        public string Message { get; }
        public GrabCnx(string message)
        {
            Message = message;
        }
    }
    public readonly record struct Grab
    {
        public static Grab Instance = new Grab();
    }
}
