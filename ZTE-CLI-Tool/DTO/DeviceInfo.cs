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
  [JsonPropertyName("loginfo")]
  public string LogInfo { get; set; } = string.Empty;

  [JsonPropertyName("wan_active_band")]
  public string WanActiveBand { get; set; } = string.Empty;

  [JsonPropertyName("wan_active_channel")]
  public string WanActiveChannel { get; set; } = string.Empty;

  [JsonPropertyName("wan_lte_ca")]
  public string WanLteCa { get; set; } = string.Empty;

  [JsonPropertyName("wan_apn")]
  public string WanApn { get; set; } = string.Empty;

  [JsonPropertyName("wan_ipaddr")]
  public string WanIpAddr { get; set; } = string.Empty;

  [JsonPropertyName("cell_id")]
  public string CellId { get; set; } = string.Empty;

  [JsonPropertyName("dns_mode")]
  public string DnsMode { get; set; } = string.Empty;

  [JsonPropertyName("prefer_dns_manual")]
  public string PreferDnsManual { get; set; } = string.Empty;

  [JsonPropertyName("standby_dns_manual")]
  public string StandbyDnsManual { get; set; } = string.Empty;

  [JsonPropertyName("network_type")]
  public string NetworkType { get; set; } = string.Empty;

  [JsonPropertyName("network_provider_fullname")]
  public string NetworkProviderFullName { get; set; } = string.Empty;

  [JsonPropertyName("ppp_connect_time")]
  public string PppConnectTime { get; set; } = string.Empty;

  [JsonPropertyName("rmcc")]
  public string Rmcc { get; set; } = string.Empty;

  [JsonPropertyName("rmnc")]
  public string Rmnc { get; set; } = string.Empty;

  [JsonPropertyName("ip_passthrough_enabled")]
  public string IpPassthroughEnabled { get; set; } = string.Empty;

  [JsonPropertyName("bandwidth")]
  public string Bandwidth { get; set; } = string.Empty;

  [JsonPropertyName("tx_power")]
  public string TxPower { get; set; } = string.Empty;

  [JsonPropertyName("lte_multi_ca_scell_info")]
  public string LteMultiCaScellInfo { get; set; } = string.Empty;

  [JsonPropertyName("lte_multi_ca_scell_sig_info")]
  public string LteMultiCaSellSigInfo { get; set; } = string.Empty;

  [JsonPropertyName("lte_band")]
  public string LteBand { get; set; } = string.Empty;

  [JsonPropertyName("lte_rsrp")]
  public string LteRsrp { get; set; } = string.Empty;

  [JsonPropertyName("lte_rsrq")]
  public string LteRsrq { get; set; } = string.Empty;

  [JsonPropertyName("lte_rssi")]
  public string LteRssi { get; set; } = string.Empty;

  [JsonPropertyName("lte_snr")]
  public string LteSnr { get; set; } = string.Empty;

  [JsonPropertyName("lte_ca_pcell_band")]
  public string LteCaPcellBand { get; set; } = string.Empty;

  [JsonPropertyName("lte_ca_pcell_freq")]
  public string LteCaPcellFreq { get; set; } = string.Empty;

  [JsonPropertyName("lte_ca_pcell_bandwidth")]
  public string LteCaPcellBandwidth { get; set; } = string.Empty;

  [JsonPropertyName("lte_ca_scell_band")]
  public string LteCaScellBand { get; set; } = string.Empty;

  [JsonPropertyName("lte_ca_scell_bandwidth")]
  public string LteCaScellBandwidth { get; set; } = string.Empty;

  [JsonPropertyName("lte_rsrp_1")]
  public string LteRsrp1 { get; set; } = string.Empty;

  [JsonPropertyName("lte_rsrp_2")]
  public string LteRsrp2 { get; set; } = string.Empty;

  [JsonPropertyName("lte_rsrp_3")]
  public string LteRsrp3 { get; set; } = string.Empty;

  [JsonPropertyName("lte_rsrp_4")]
  public string LteRsrp4 { get; set; } = string.Empty;

  [JsonPropertyName("lte_snr_1")]
  public string LteSnr1 { get; set; } = string.Empty;

  [JsonPropertyName("lte_snr_2")]
  public string LteSnr2 { get; set; } = string.Empty;

  [JsonPropertyName("lte_snr_3")]
  public string LteSnr3 { get; set; } = string.Empty;

  [JsonPropertyName("lte_snr_4")]
  public string LteSnr4 { get; set; } = string.Empty;

  [JsonPropertyName("lte_pci")]
  public string LtePci { get; set; } = string.Empty;

  [JsonPropertyName("lte_pci_lock")]
  public string LtePciLock { get; set; } = string.Empty;

  [JsonPropertyName("lte_earfcn_lock")]
  public string LteEarfcnLock { get; set; } = string.Empty;

  [JsonPropertyName("5g_rx0_rsrp")]
  public string _5gRx0Rsrp { get; set; } = string.Empty;

  [JsonPropertyName("5g_rx1_rsrp")]
  public string _5gRx1Rsrp { get; set; } = string.Empty;

  [JsonPropertyName("Z5g_rsrp")]
  public string Z5gRsrp { get; set; } = string.Empty;

  [JsonPropertyName("Z5g_rsrq")]
  public string Z5gRsrq { get; set; } = string.Empty;

  [JsonPropertyName("Z5g_SINR")]
  public string Z5gSinr { get; set; } = string.Empty;

  [JsonPropertyName("nr5g_cell_id")]
  public string Nr5gCellId { get; set; } = string.Empty;

  [JsonPropertyName("nr5g_pci")]
  public string Nr5gPci { get; set; } = string.Empty;

  [JsonPropertyName("nr5g_action_channel")]
  public string Nr5gActionChannel { get; set; } = string.Empty;

  [JsonPropertyName("nr5g_action_band")]
  public string Nr5gActionBand { get; set; } = string.Empty;

  [JsonPropertyName("nr5g_action_nsa_band")]
  public string Nr5gActionNsaBand { get; set; } = string.Empty;

  [JsonPropertyName("nr_ca_pcell_band")]
  public string NrCaPcellBand { get; set; } = string.Empty;

  [JsonPropertyName("nr_ca_pcell_freq")]
  public string NrCaPcellFreq { get; set; } = string.Empty;

  [JsonPropertyName("nr_multi_ca_scell_info")]
  public string NrMultiCaScellInfo { get; set; } = string.Empty;

  [JsonPropertyName("nr5g_sa_band_lock")]
  public string Nr5gSaBandLock { get; set; } = string.Empty;

  [JsonPropertyName("nr5g_nsa_band_lock")]
  public string Nr5gNsaBandLock { get; set; } = string.Empty;

  [JsonPropertyName("pm_sensor_ambient")]
  public string PmSensorAmbient { get; set; } = string.Empty;

  [JsonPropertyName("pm_sensor_mdm")]
  public string PmSensorMdm { get; set; } = string.Empty;

  [JsonPropertyName("pm_sensor_5g")]
  public string PmSensor5g { get; set; } = string.Empty;

  [JsonPropertyName("pm_sensor_pa1")]
  public string PmSensorPa1 { get; set; } = string.Empty;

  [JsonPropertyName("wifi_chip_temp")]
  public string WifiChipTemp { get; set; } = string.Empty;

  [JsonPropertyName("monthly_drop_rx_packets")]
  public string MonthlyDropRxPackets { get; set; } = string.Empty;

  [JsonPropertyName("monthly_rx_bytes")]
  public string MonthlyRxBytes { get; set; } = string.Empty;

  [JsonPropertyName("monthly_rx_packets")]
  public string MonthlyRxPackets { get; set; } = string.Empty;

  [JsonPropertyName("peak_rx_bytes")]
  public string PeakRxBytes { get; set; } = string.Empty;

  [JsonPropertyName("realtime_drop_rx_packets")]
  public string RealtimeDropRxPackets { get; set; } = string.Empty;

  [JsonPropertyName("realtime_rx_bytes")]
  public string RealtimeRxBytes { get; set; } = string.Empty;

  [JsonPropertyName("realtime_rx_packets")]
  public string RealtimeRxPackets { get; set; } = string.Empty;

  [JsonPropertyName("realtime_rx_thrpt")]
  public string RealtimeRxThrpt { get; set; } = string.Empty;

  [JsonPropertyName("total_drop_rx_packets")]
  public string TotalDropRxPackets { get; set; } = string.Empty;

  [JsonPropertyName("total_rx_bytes")]
  public string TotalRxBytes { get; set; } = string.Empty;

  [JsonPropertyName("total_rx_packets")]
  public string TotalRxPackets { get; set; } = string.Empty;

  [JsonPropertyName("traffic_month_home_drop_rx_packets")]
  public string TrafficMonthHomeDropRxPackets { get; set; } = string.Empty;

  [JsonPropertyName("traffic_month_home_rx_packets")]
  public string TrafficMonthHomeRxPackets { get; set; } = string.Empty;

  [JsonPropertyName("traffic_month_internal_drop_rx_packets")]
  public string TrafficMonthInternalDropRxPackets { get; set; } = string.Empty;

  [JsonPropertyName("traffic_month_internal_rx_packets")]
  public string TrafficMonthInternalRxPackets { get; set; } = string.Empty;

  [JsonPropertyName("traffic_month_roam_drop_rx_packets")]
  public string TrafficMonthRoamDropRxPackets { get; set; } = string.Empty;

  [JsonPropertyName("traffic_month_roam_rx_packets")]
  public string TrafficMonthRoamRxPackets { get; set; } = string.Empty;

  [JsonPropertyName("traffic_total_home_drop_rx_packets")]
  public string TrafficTotalHomeDropRxPackets { get; set; } = string.Empty;

  [JsonPropertyName("traffic_total_home_rx_packets")]
  public string TrafficTotalHomeRxPackets { get; set; } = string.Empty;

  [JsonPropertyName("traffic_total_internal_drop_rx_packets")]
  public string TrafficTotalInternalDropRxPackets { get; set; } = string.Empty;

  [JsonPropertyName("traffic_total_roam_drop_rx_packets")]
  public string TrafficTotalRoamDropRxPackets { get; set; } = string.Empty;

  [JsonPropertyName("traffic_total_roam_rx_packets")]
  public string TrafficTotalRoamRxPackets { get; set; } = string.Empty;

  [JsonPropertyName("wan_curr_rx_bytes")]
  public string WanCurrRxBytes { get; set; } = string.Empty;
}

