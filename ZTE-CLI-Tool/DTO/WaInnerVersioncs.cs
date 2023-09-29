using System.Text.Json.Serialization;

namespace ZTE_Cli_Tool.DTO;

public class WaInnerVersion
{
  [JsonPropertyName("wa_inner_version")]
  public string Version { get; set; } = string.Empty;
}
