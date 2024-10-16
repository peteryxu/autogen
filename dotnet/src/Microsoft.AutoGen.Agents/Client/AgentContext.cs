using System.Diagnostics;
using Microsoft.AutoGen.Agents.Abstractions;
using Microsoft.Extensions.Logging;

namespace Microsoft.AutoGen.Agents.Client;

internal sealed class AgentContext(AgentId agentId, AgentWorkerRuntime runtime, ILogger<AgentBase> logger, DistributedContextPropagator distributedContextPropagator) : IAgentContext
{
    private readonly AgentWorkerRuntime _runtime = runtime;

    public AgentId AgentId { get; } = agentId;
    public ILogger Logger { get; } = logger;
    public AgentBase? AgentInstance { get; set; }
    public DistributedContextPropagator DistributedContextPropagator { get; } = distributedContextPropagator;

    public async ValueTask SendResponseAsync(RpcRequest request, RpcResponse response)
    {
        response.RequestId = request.RequestId;
        await _runtime.SendResponse(response);
    }

    public async ValueTask SendRequestAsync(AgentBase agent, RpcRequest request)
    {
        await _runtime.SendRequest(agent, request);
    }

    public async ValueTask PublishEventAsync(CloudEvent @event)
    {
        await _runtime.PublishEvent(@event);
    }
}
