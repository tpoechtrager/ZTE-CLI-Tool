/*
 * This file is part of ZTE-Cli-Tool.
 *
 * ZTE-Cli-Tool is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 2 of the License, or
 * (at your option) any later version.
 *
 * ZTE-Cli-Tool is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU General Public License for more details.
 *
 * Author: Thomas Pöchtrager (email: t.poechtrager@gmail.com)
 * Year: 2023
 */

using System.Text.Json.Serialization;

namespace ZTE_Cli_Tool.DTO;

public class DeviceInfo
{
  public string loginfo { get; set; } = string.Empty;
  public string wan_active_band { get; set; } = string.Empty;
  public string wan_active_channel { get; set; } = string.Empty;
  public string wan_lte_ca { get; set; } = string.Empty;
  public string wan_apn { get; set; } = string.Empty;
  public string wan_ipaddr { get; set; } = string.Empty;
  public string cell_id { get; set; } = string.Empty;
  public string dns_mode { get; set; } = string.Empty;
  public string prefer_dns_manual { get; set; } = string.Empty;
  public string standby_dns_manual { get; set; } = string.Empty;
  public string network_type { get; set; } = string.Empty;
  public string network_provider_fullname { get; set; } = string.Empty;
  public string ppp_connect_time { get; set; } = string.Empty;
  public string rmcc { get; set; } = string.Empty;
  public string rmnc { get; set; } = string.Empty;
  public string ip_passthrough_enabled { get; set; } = string.Empty;
  public string bandwidth { get; set; } = string.Empty;
  public string tx_power { get; set; } = string.Empty;
  public string rscp_1 { get; set; } = string.Empty;
  public string ecio_1 { get; set; } = string.Empty;
  public string rscp_2 { get; set; } = string.Empty;
  public string ecio_2 { get; set; } = string.Empty;
  public string rscp_3 { get; set; } = string.Empty;
  public string ecio_3 { get; set; } = string.Empty;
  public string rscp_4 { get; set; } = string.Empty;
  public string ecio_4 { get; set; } = string.Empty;
  public string ngbr_cell_info { get; set; } = string.Empty;
  public string lte_multi_ca_scell_info { get; set; } = string.Empty;
  public string lte_multi_ca_scell_sig_info { get; set; } = string.Empty;
  public string lte_band { get; set; } = string.Empty;
  public string lte_rsrp { get; set; } = string.Empty;
  public string lte_rsrq { get; set; } = string.Empty;
  public string lte_rssi { get; set; } = string.Empty;
  public string lte_snr { get; set; } = string.Empty;
  public string lte_ca_pcell_band { get; set; } = string.Empty;
  public string lte_ca_pcell_freq { get; set; } = string.Empty;
  public string lte_ca_pcell_bandwidth { get; set; } = string.Empty;
  public string lte_ca_scell_band { get; set; } = string.Empty;
  public string lte_ca_scell_bandwidth { get; set; } = string.Empty;
  public string lte_rsrp_1 { get; set; } = string.Empty;
  public string lte_rsrp_2 { get; set; } = string.Empty;
  public string lte_rsrp_3 { get; set; } = string.Empty;
  public string lte_rsrp_4 { get; set; } = string.Empty;
  public string lte_snr_1 { get; set; } = string.Empty;
  public string lte_snr_2 { get; set; } = string.Empty;
  public string lte_snr_3 { get; set; } = string.Empty;
  public string lte_snr_4 { get; set; } = string.Empty;
  public string lte_pci { get; set; } = string.Empty;
  public string lte_pci_lock { get; set; } = string.Empty;
  public string lte_earfcn_lock { get; set; } = string.Empty;
  [JsonPropertyName("5g_rx0_rsrp")]
  public string _5g_rx0_rsrp { get; set; } = string.Empty;
  [JsonPropertyName("5g_rx1_rsrp")]
  public string _5g_rx1_rsrp { get; set; } = string.Empty;
  public string Z5g_rsrp { get; set; } = string.Empty;
  public string Z5g_rsrq { get; set; } = string.Empty;
  public string Z5g_SINR { get; set; } = string.Empty;
  public string nr5g_cell_id { get; set; } = string.Empty;
  public string nr5g_pci { get; set; } = string.Empty;
  public string nr5g_action_channel { get; set; } = string.Empty;
  public string nr5g_action_band { get; set; } = string.Empty;
  public string nr5g_action_nsa_band { get; set; } = string.Empty;
  public string nr_ca_pcell_band { get; set; } = string.Empty;
  public string nr_ca_pcell_freq { get; set; } = string.Empty;
  public string nr_multi_ca_scell_info { get; set; } = string.Empty;
  public string nr5g_sa_band_lock { get; set; } = string.Empty;
  public string nr5g_nsa_band_lock { get; set; } = string.Empty;
  public string pm_sensor_ambient { get; set; } = string.Empty;
  public string pm_sensor_mdm { get; set; } = string.Empty;
  public string pm_sensor_5g { get; set; } = string.Empty;
  public string pm_sensor_pa1 { get; set; } = string.Empty;
  public string wifi_chip_temp { get; set; } = string.Empty;
};
