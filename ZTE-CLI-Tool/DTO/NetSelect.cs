namespace ZTE_Cli_Tool.DTO;

using System.Text.Json.Serialization;

public class NetSelect
{
  [JsonPropertyName("net_select")]
  public string Mode { get; set; } = "";
}
