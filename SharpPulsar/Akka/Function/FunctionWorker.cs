using System;
using System.Net.Http;
using Akka.Actor;
using SharpPulsar.Akka.Function.Api;

namespace SharpPulsar.Akka.Function
{
    //https://www.splunk.com/en_us/blog/it/event-processing-design-patterns-with-pulsar-functions.html
    public class FunctionWorker : ReceiveActor
    {
        private readonly PulsarFunctionsRESTAPIClient _client;

        public FunctionWorker(string server)
        {
            _client = new PulsarFunctionsRESTAPIClient(server, new HttpClient());
            Receive<InternalCommands.Function>(Handle);
        }

        protected override void Unhandled(object message)
        {

        }

        private void Handle(InternalCommands.Function function)
        {
            try
            {
                switch (function.Command)
                {
                    case FunctionCommand.DeregisterFunction:
                        var tenant = function.Arguments[0].ToString();
                        var nspace = function.Arguments[1].ToString();
                        var name =  function.Arguments[2].ToString();
                        _client.DeregisterFunctionAsync(tenant, nspace, name).GetAwaiter().GetResult();
                        function.Handler("DeregisterFunctionAsync");
                        break;
                    case FunctionCommand.GetFunctionInfo:
                        var tenant1 = function.Arguments[0].ToString();
                        var nspace1 = function.Arguments[1].ToString();
                        var name1 =  function.Arguments[2].ToString();
                        var r = _client.GetFunctionInfoAsync(tenant1, nspace1, name1).GetAwaiter().GetResult();
                        function.Handler(r);
                        break;
                    case FunctionCommand.GetFunctionInstanceStats:
                        var tenant2 = function.Arguments[0].ToString();
                        var nspace2 = function.Arguments[1].ToString();
                        var name2 =  function.Arguments[2].ToString();
                        var id = function.Arguments[3].ToString();
                        var r2 = _client.GetFunctionInstanceStatsAsync(tenant2, nspace2, name2, id).GetAwaiter().GetResult();
                        function.Handler(r2);
                        break;
                    case FunctionCommand.GetFunctionInstanceStatus:
                        var tenant3 = function.Arguments[0].ToString();
                        var nspace3 = function.Arguments[1].ToString();
                        var name3 =  function.Arguments[2].ToString();
                        var id1 = function.Arguments[3].ToString();
                        var r3 = _client.GetFunctionInstanceStatusAsync(tenant3, nspace3, name3, id1).GetAwaiter().GetResult();
                        function.Handler(r3);
                        break;
                    case FunctionCommand.GetFunctionState:
                        var tenant4 = function.Arguments[0].ToString();
                        var nspace4 = function.Arguments[1].ToString();
                        var name4 =  function.Arguments[2].ToString();
                        var key = function.Arguments[3].ToString();
                        var r4 = _client.GetFunctionStateAsync(tenant4, nspace4, name4, key).GetAwaiter().GetResult();
                        function.Handler(r4);
                        break;
                    case FunctionCommand.GetFunctionStats:
                        var tenant5 = function.Arguments[0].ToString();
                        var nspace5 = function.Arguments[1].ToString();
                        var name5 =  function.Arguments[2].ToString();
                        var r5 = _client.GetFunctionStatsAsync(tenant5, nspace5, name5).GetAwaiter().GetResult();
                        function.Handler(r5);
                        break;
                    case FunctionCommand.GetFunctionStatus:
                        var tenant6 = function.Arguments[0].ToString();
                        var nspace6 = function.Arguments[1].ToString();
                        var name6 =  function.Arguments[2].ToString();
                        var r6 = _client.GetFunctionStatusAsync(tenant6, nspace6, name6).GetAwaiter().GetResult();
                        function.Handler(r6);
                        break;
                    case FunctionCommand.ListFunctions:
                        var tenant7 = function.Arguments[0].ToString();
                        var nspace7 = function.Arguments[1].ToString();
                        var r7 = _client.ListFunctionsAsync(tenant7, nspace7).GetAwaiter().GetResult();
                        function.Handler(r7);
                        break;
                    case FunctionCommand.PutFunctionState:
                        var tenant8 = function.Arguments[0].ToString();
                        var nspace8 = function.Arguments[1].ToString();
                        var name7 = function.Arguments[2].ToString();
                        var key2 = function.Arguments[3].ToString();
                        _client.PutFunctionStateAsync(tenant8, nspace8, name7, key2).GetAwaiter().GetResult();
                        function.Handler("PutFunctionState");
                        break;
                    case FunctionCommand.RegisterFunction:
                        var config = (FunctionConfig)function.Arguments[0];
                        var pkgUrl = function.Arguments[1].ToString();
                        var file = function.Arguments[2].ToString();
                        _client.RegisterFunctionAsync(config, pkgUrl, file).GetAwaiter().GetResult();
                        function.Handler("RegisterFunction");
                        break;
                    case FunctionCommand.RestartFunction:
                        var tenant10 = function.Arguments[0].ToString();
                        var nspace10 = function.Arguments[1].ToString();
                        var name9 = function.Arguments[2].ToString();
                        _client.RestartFunctionAsync(tenant10, nspace10, name9).GetAwaiter().GetResult();
                        function.Handler("RestartFunction");
                        break;
                    case FunctionCommand.RestartInstanceFunction:
                        var tenant11 = function.Arguments[0].ToString();
                        var nspace11 = function.Arguments[1].ToString();
                        var name10 = function.Arguments[2].ToString();
                        var id3 = function.Arguments[3].ToString();
                        _client.RestartInstanceFunctionAsync(tenant11, nspace11, name10, id3).GetAwaiter().GetResult();
                        function.Handler("RestartInstanceFunction");
                        break;
                    case FunctionCommand.StartFunction:
                        var tenant12 = function.Arguments[0].ToString();
                        var nspace12 = function.Arguments[1].ToString();
                        var name11 = function.Arguments[2].ToString();
                        _client.StartFunctionAsync(tenant12, nspace12, name11).GetAwaiter().GetResult();
                        function.Handler("StartFunction");
                        break;
                    case FunctionCommand.StartInstanceFunction:
                        var tenant13 = function.Arguments[0].ToString();
                        var nspace13 = function.Arguments[1].ToString();
                        var name12 = function.Arguments[2].ToString();
                        var id4 = function.Arguments[3].ToString();
                        _client.StartInstanceFunctionAsync(tenant13, nspace13, name12, id4).GetAwaiter().GetResult();
                        function.Handler("StartInstanceFunction");
                        break;
                    case FunctionCommand.StopInstanceFunction:
                        var tenant14 = function.Arguments[0].ToString();
                        var nspace14 = function.Arguments[1].ToString();
                        var name13 = function.Arguments[2].ToString();
                        var id5 = function.Arguments[3].ToString();
                        _client.StopInstanceFunctionAsync(tenant14, nspace14, name13, id5).GetAwaiter().GetResult();
                        function.Handler("StopInstanceFunction");
                        break;
                    case FunctionCommand.StopFunction:
                        var tenant15 = function.Arguments[0].ToString();
                        var nspace15 = function.Arguments[1].ToString();
                        var name14 = function.Arguments[2].ToString();
                        _client.StopFunctionAsync(tenant15, nspace15, name14).GetAwaiter().GetResult();
                        function.Handler("StopFunction");
                        break;
                    case FunctionCommand.TriggerFunction:
                        var tenant16 = function.Arguments[0].ToString();
                        var nspace16 = function.Arguments[1].ToString();
                        var name15 = function.Arguments[2].ToString();
                        var topic = function.Arguments[3].ToString();
                        var value = function.Arguments[4].ToString();
                        var file1 = function.Arguments[5].ToString();
                        var m =_client.TriggerFunctionAsync(tenant16, nspace16, name15, topic, value, file1).GetAwaiter().GetResult();
                        function.Handler(m);
                        break;
                    case FunctionCommand.UpdateFunction:
                        var config1 = (FunctionConfig)function.Arguments[0];
                        var option = (UpdateOptions)function.Arguments[1];
                        var pkgUrl1 = function.Arguments[2].ToString();
                        var file2 = function.Arguments[3].ToString();
                        _client.UpdateFunctionAsync(config1, option, pkgUrl1, file2).GetAwaiter().GetResult();
                        function.Handler("UpdateFunction");
                        break;
                }
            }
            catch (Exception e)
            {
                function.Exception(e);
            }
        }
        public static Props Prop(string server)
        {
            return Props.Create(() => new FunctionWorker(server));
        }
    }
}
