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

namespace ZTE_Cli_Tool;

using ZTE_Cli_Tool.DTO;
using static ZTE_Cli_Tool.Tools;

public class LteCellContainer : CellContainer<LteCell>
{
  public SignalValue<int> tx_power { get; } = new();

  public void Update(NetworkType _, in DeviceInfo deviceInfo)
  {
    tx_power.Update(deviceInfo.tx_power);

    /*
     * Main Cell
     */

    Value<int> pci = new();
    Value<int> freq = new();

    pci.Set(deviceInfo.lte_pci, SetFlag.InputIsHex);
    freq.Set(OneOf(deviceInfo.wan_active_band, deviceInfo.lte_ca_pcell_freq));

    var cell = GetCell(pci, freq) ?? new();

    cell.pci = pci;
    cell.freq = freq;
    cell.band.Set(OneOf(deviceInfo.lte_ca_pcell_band, deviceInfo.lte_band));

    cell.bandwidth
      .Set(OneOf(deviceInfo.lte_ca_pcell_bandwidth, deviceInfo.bandwidth),
           new ValueRemove("MHz"));

    cell.rssi.Update(deviceInfo.lte_rssi);
    cell.rsrp1.Update(deviceInfo.lte_rsrp_1);
    cell.rsrp2.Update(deviceInfo.lte_rsrp_2);
    cell.rsrp3.Update(deviceInfo.lte_rsrp_3);
    cell.rsrp4.Update(deviceInfo.lte_rsrp_4);
    cell.rsrq.Update(deviceInfo.lte_rsrq);
    cell.sinr1.Update(deviceInfo.lte_snr_1);
    cell.sinr2.Update(deviceInfo.lte_snr_2);
    cell.sinr3.Update(deviceInfo.lte_snr_3);
    cell.sinr4.Update(deviceInfo.lte_snr_4);

    AddOrUpdateCell(cell);

    /*
     * SCells
     */

    if (string.IsNullOrEmpty(deviceInfo.lte_multi_ca_scell_info)) {
      RemoveOrphanedCells();
      return;
    }

    var scellInfos = deviceInfo.lte_multi_ca_scell_info.Split(';')
      .Where(s => !string.IsNullOrEmpty(s)).ToList();

    var scellSigInfos = deviceInfo.lte_multi_ca_scell_sig_info.Split(';')
      .Where(s => !string.IsNullOrEmpty(s)).ToList();

    for (int i = 0; i < scellInfos.Count; i++) {
      var scellInfo = scellInfos[i].Split(',');

      if (scellInfo.Length < 6)
        continue;

      pci = new();
      freq = new();

      pci.Set(scellInfo[1]);
      freq.Set(scellInfo[4]);

      var scell = GetCell(pci, freq) ?? new LteCell();

      scell.scell.Set(true);
      scell.pci = pci;
      scell.freq = freq;

      scell.band.Set(scellInfo[3]);
      scell.bandwidth.Set(scellInfo[5]);

      if (i < scellSigInfos.Count) {
        var scellSigInfo = scellSigInfos[i].Split(',');

        if (scellSigInfo.Length >= 3) {
          scell.rsrp1.Update(scellSigInfo[0], new ValueRemove("0.0", "-44.0"));
          scell.rsrq.Update(scellSigInfo[1], new ValueRemove("0.0"));
          // In theory, 0.0 could be a valid SINR value
          scell.sinr1.Update(scellSigInfo[2], new ValueRemove("0.0"));
        }
      }

      AddOrUpdateCell(scell);
    }

    RemoveOrphanedCells();
  }
}
