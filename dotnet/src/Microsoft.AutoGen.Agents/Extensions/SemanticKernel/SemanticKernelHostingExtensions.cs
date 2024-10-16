// Copyright (c) Microsoft. All rights reserved.

using System.Text.Json;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using Microsoft.SemanticKernel.Memory;

namespace Microsoft.AutoGen.Agents.Extensions.SemanticKernel;
public static class SemanticKernelHostingExtensions
{
    public static IHostApplicationBuilder ConfigureSemanticKernel(this IHostApplicationBuilder builder)
    {
        builder.Services.Configure<OpenAIOptions>(o =>
        {
            o.EmbeddingsEndpoint = o.ImageEndpoint = o.ChatEndpoint = builder.Configuration["OpenAI:Endpoint"] ?? throw new InvalidOperationException("Ensure that OpenAI:Endpoint is set in configuration");
            o.EmbeddingsApiKey = o.ImageApiKey = o.ChatApiKey = builder.Configuration["OpenAI:Key"]!;
            o.EmbeddingsDeploymentOrModelId = "text-embedding-3-large";
            o.ImageDeploymentOrModelId = "dall-e-3";
            o.ChatDeploymentOrModelId = "gpt-4o";
        });

        builder.Services.Configure<OpenAIClientOptions>(o =>
        {
            o.Retry.NetworkTimeout = TimeSpan.FromMinutes(5);
        });

        builder.Services.AddOptions<QdrantOptions>().Bind(builder.Configuration.GetSection("Qdrant"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        builder.Services.Configure<JsonSerializerOptions>(options =>
        {
            options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        });

        builder.Services.AddTransient(CreateKernel);
        builder.Services.AddTransient(CreateMemory);
        return builder;
    }

    private static ISemanticTextMemory CreateMemory(IServiceProvider provider)
    {
        var qdrantConfig = provider.GetRequiredService<IOptions<QdrantOptions>>().Value;
        var openAiConfig = provider.GetRequiredService<IOptions<OpenAIOptions>>().Value;
        var qdrantHttpClient = new HttpClient();
        if (!string.IsNullOrEmpty(qdrantConfig.ApiKey))
        {
            qdrantHttpClient.DefaultRequestHeaders.Add("api-key", qdrantConfig.ApiKey);
        }
        var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
        var memoryBuilder = new MemoryBuilder();
        return memoryBuilder.WithLoggerFactory(loggerFactory)
                    .WithQdrantMemoryStore(qdrantHttpClient, qdrantConfig.VectorSize, qdrantConfig.Endpoint)
                     .WithAzureOpenAITextEmbeddingGeneration(openAiConfig.EmbeddingsDeploymentOrModelId, openAiConfig.EmbeddingsEndpoint, openAiConfig.EmbeddingsApiKey)
                     .Build();
    }

    private static Kernel CreateKernel(IServiceProvider provider)
    {
        OpenAIOptions openAiConfig = provider.GetRequiredService<IOptions<OpenAIOptions>>().Value;
        var builder = Kernel.CreateBuilder();

        // Chat
        if (openAiConfig.ChatEndpoint.Contains(".azure", StringComparison.OrdinalIgnoreCase))
        {
            var openAIClient = new OpenAIClient(new Uri(openAiConfig.ChatEndpoint), new Azure.AzureKeyCredential(openAiConfig.ChatApiKey));
            builder.Services.AddAzureOpenAIChatCompletion(openAiConfig.ChatDeploymentOrModelId, openAIClient);
        }
        else
        {
            var openAIClient = new OpenAIClient(openAiConfig.ChatApiKey);
            builder.Services.AddOpenAIChatCompletion(openAiConfig.ChatDeploymentOrModelId, openAIClient);
        }

        // Text to Image
        if (openAiConfig.ImageEndpoint.Contains(".azure", StringComparison.OrdinalIgnoreCase))
        {
            ArgumentException.ThrowIfNullOrEmpty(openAiConfig.ImageDeploymentOrModelId);
            var openAIClient = new OpenAIClient(new Uri(openAiConfig.ImageEndpoint), new Azure.AzureKeyCredential(openAiConfig.ImageApiKey));
            builder.Services.AddAzureOpenAITextToImage(openAiConfig.ImageDeploymentOrModelId, openAIClient);
        }
        else
        {
            builder.Services.AddOpenAITextToImage(openAiConfig.ImageApiKey, modelId: openAiConfig.ImageDeploymentOrModelId);
        }

        // Embeddings
        if (openAiConfig.EmbeddingsEndpoint.Contains(".azure", StringComparison.OrdinalIgnoreCase))
        {
            var openAIClient = new OpenAIClient(new Uri(openAiConfig.EmbeddingsEndpoint), new Azure.AzureKeyCredential(openAiConfig.EmbeddingsApiKey));
            builder.Services.AddAzureOpenAITextEmbeddingGeneration(openAiConfig.EmbeddingsDeploymentOrModelId, openAIClient);
        }
        else
        {
            var openAIClient = new OpenAIClient(openAiConfig.EmbeddingsApiKey);
            builder.Services.AddOpenAITextEmbeddingGeneration(openAiConfig.EmbeddingsDeploymentOrModelId, openAIClient);
        }

        return builder.Build();
    }
}
