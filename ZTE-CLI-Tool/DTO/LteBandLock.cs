using System.Text.Json.Serialization;

namespace ZTE_Cli_Tool.DTO;

public class LteBandLock
{
  // Band mask
  [JsonPropertyName("lte_band_lock")]
  public string Bands { get; set; } = string.Empty;
}
