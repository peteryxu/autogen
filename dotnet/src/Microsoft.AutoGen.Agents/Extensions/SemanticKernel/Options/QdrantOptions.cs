using System.ComponentModel.DataAnnotations;

namespace Microsoft.AutoGen.Agents.Extensions.SemanticKernel;
public class QdrantOptions
{
    [Required]
    public required string Endpoint { get; set; }
    [Required]
    public required int VectorSize { get; set; }
    public string ApiKey { get; set; } = "";
}
