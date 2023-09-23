﻿/*
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

using ZTE_Cli_Tool.DTO;

namespace ZTE_Cli_Tool;

public class NrCellContainer : CellContainer<NrCell>
{
  public void Update(NetworkType networkType, in DeviceInfo deviceInfo)
  {
    if (networkType.IsNrNsa && !networkType.IsNrNsaActive) {
      // Base station is capable of 5G NSA
      // but we don't have any reception of the NSA band.
      cells.Clear();
      RemoveOrphanedCells();
      return;
    }

    string NrSigVal(string nsaVal, string saVal)
    {
      return networkType.IsNrNsa ? nsaVal : saVal;
    }

    /*
     * There's apparently no better fix for this.
     * The API does not reset its memory correctly after switching from
     * 5G CA to 5G without CA.
     */
    var is_ca = deviceInfo.nr5g_action_channel == deviceInfo.nr_ca_pcell_freq;

    // Main cell

    var _5g_rx0_rsrp = deviceInfo._5g_rx0_rsrp;

    if (string.IsNullOrEmpty(_5g_rx0_rsrp))
      _5g_rx0_rsrp = deviceInfo.Z5g_rsrp;

    Value<int> pci = new();
    Value<int> freq = new();

    pci.Set(deviceInfo.nr5g_pci, SetFlag.InputIsHex);
    freq.Set(deviceInfo.nr5g_action_channel);

    var cell = GetCell(pci, freq) ?? new NrCell();

    cell.scell.Set(false);
    cell.pci = pci;
    cell.freq = freq;
    cell.band.Set(
        is_ca
            ? deviceInfo.nr_ca_pcell_band
            : NrSigVal(
                deviceInfo.nr5g_action_nsa_band,
                deviceInfo.nr5g_action_band
            ),
        SetFlag.StripNonNumericCharacters
    );
    cell.bandwidth.Set(
      // There is a good change that deviceInfo.bandwidth
      // contains a wrong bandwidth value for NSA.
      // Therefore don't use it for NSA.
      NrSigVal("-1", deviceInfo.bandwidth),
      SetFlag.StripNonNumericCharacters
    );
    cell.rsrp1.Update(_5g_rx0_rsrp);
    cell.rsrp2.Update(deviceInfo._5g_rx1_rsrp);
    cell.rsrq.Update(deviceInfo.Z5g_rsrq);
    cell.sinr.Update(deviceInfo.Z5g_SINR, new ValueRemove("-20.0", "-3276.8"));

    AddOrUpdateCell(cell);

    // SCells

    if (!is_ca || string.IsNullOrEmpty(deviceInfo.nr_multi_ca_scell_info)) {
      RemoveOrphanedCells();
      return;
    }

    var allowedBands =
      NrSigVal(deviceInfo.nr5g_nsa_band_lock, deviceInfo.nr5g_sa_band_lock)
        .Split(",");

    var scellInfos =
      deviceInfo.nr_multi_ca_scell_info.Split(';')
        .Where(s => !string.IsNullOrEmpty(s)).ToList();

    foreach (var scellInfo in scellInfos) {
      var scellData = scellInfo.Split(',');

      if (scellData.Length < 10)
        continue;

      var band = Tools.RemoveNonNumericCharacters(scellData[3]);

      /*
       * Try to detect false data. See comment above.
       */
      if (!allowedBands.Contains(band)) {
        continue;
      }

      pci = new();
      freq = new();

      pci.Set(scellData[1]);
      freq.Set(scellData[4]);

      var scell = GetCell(pci, freq) ?? new NrCell();

      scell.scell.Set(true);
      scell.pci = pci;
      scell.freq = freq;
      scell.band.Set(band);
      scell.bandwidth.Set(scellData[5], SetFlag.StripNonNumericCharacters);
      scell.rsrp1.Update(scellData[7], new ValueRemove("0.0"));
      scell.rsrq.Update(scellData[8], new ValueRemove("0.0"));
      scell.sinr.Update(scellData[9], new ValueRemove("0.0", "-20.0", "-3276.8"));

      AddOrUpdateCell(scell);
    }

    RemoveOrphanedCells();
  }
}