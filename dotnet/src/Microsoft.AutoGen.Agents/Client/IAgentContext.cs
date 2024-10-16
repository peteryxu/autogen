using System.Diagnostics;
using Microsoft.AutoGen.Agents.Abstractions;
using Microsoft.Extensions.Logging;

namespace Microsoft.AutoGen.Agents.Client;

public interface IAgentContext
{
    AgentId AgentId { get; }
    AgentBase? AgentInstance { get; set; }
    DistributedContextPropagator DistributedContextPropagator { get; }
    ILogger Logger { get; }
    ValueTask SendResponseAsync(RpcRequest request, RpcResponse response);
    ValueTask SendRequestAsync(AgentBase agent, RpcRequest request);
    ValueTask PublishEventAsync(CloudEvent @event);
}
