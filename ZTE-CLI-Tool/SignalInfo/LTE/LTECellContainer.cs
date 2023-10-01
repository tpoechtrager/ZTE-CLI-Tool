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
  public SignalValue<int> TxPower { get; } = new();

  public void Update(NetworkType _, in DeviceInfo deviceInfo)
  {
    TxPower.Update(deviceInfo.TxPower);

    /*
     * Main Cell
     */

    Value<int> pci = new();
    Value<int> freq = new();

    pci.Set(deviceInfo.LtePci, SetFlag.InputIsHex);
    freq.Set(OneOf(deviceInfo.WanActiveBand, deviceInfo.LteCaPcellFreq));

    var cell = GetCell(pci, freq) ?? new();

    cell.Pci = pci;
    cell.Freq = freq;
    cell.Band.Set(OneOf(deviceInfo.LteCaPcellBand, deviceInfo.LteBand));

    cell.Bandwidth
      .Set(OneOf(deviceInfo.LteCaPcellBandwidth, deviceInfo.Bandwidth),
           new ValueRemove("MHz"));

    cell.Rssi.Update(deviceInfo.LteRssi);
    cell.Rsrp1.Update(deviceInfo.LteRsrp1);
    cell.Rsrp2.Update(deviceInfo.LteRsrp2);
    cell.Rsrp3.Update(deviceInfo.LteRsrp3);
    cell.Rsrp4.Update(deviceInfo.LteRsrp4);
    cell.Rsrq.Update(deviceInfo.LteRsrq);
    cell.Sinr1.Update(deviceInfo.LteSnr1);
    cell.Sinr2.Update(deviceInfo.LteSnr2);
    cell.Sinr3.Update(deviceInfo.LteSnr3);
    cell.Sinr4.Update(deviceInfo.LteSnr4);

    AddOrUpdateCell(cell);

    /*
     * SCells
     */

    if (string.IsNullOrEmpty(deviceInfo.LteMultiCaScellInfo)) {
      RemoveOrphanedCells();
      return;
    }

    var scellInfos = deviceInfo.LteMultiCaScellInfo.Split(';')
      .Where(s => !string.IsNullOrEmpty(s)).ToList();

    var scellSigInfos = deviceInfo.LteMultiCaSellSigInfo.Split(';')
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

      scell.Scell.Set(true);
      scell.Pci = pci;
      scell.Freq = freq;

      scell.Band.Set(scellInfo[3]);
      scell.Bandwidth.Set(scellInfo[5]);

      if (i < scellSigInfos.Count) {
        var scellSigInfo = scellSigInfos[i].Split(',');

        if (scellSigInfo.Length >= 3) {
          scell.Rsrp1.Update(scellSigInfo[0], new ValueRemove("0.0", "-44.0"));
          scell.Rsrq.Update(scellSigInfo[1], new ValueRemove("0.0"));
          // In theory, 0.0 could be a valid SINR value
          scell.Sinr1.Update(scellSigInfo[2], new ValueRemove("0.0"));
        }
      }

      AddOrUpdateCell(scell);
    }

    RemoveOrphanedCells();
  }
}
