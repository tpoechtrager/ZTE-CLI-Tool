using System.Text.Json.Serialization;

namespace ZTE_Cli_Tool.DTO;

public class SaBandLock
{
  [JsonPropertyName("nr5g_sa_band_lock")]
  public string Bands { get; set; } = string.Empty;
}
