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

using System.Globalization;
using ZTE_Cli_Tool.DTO;

namespace ZTE_Cli_Tool.SignalInfo;

public class ConnectionTime
{
  private DateTime? connectTime = null;

  public string timeConnected => GetTimeDifferenceFromNow();
  public int timeConnectedInSeconds => GetTimeDifferenceInSeconds();

  private int GetTimeDifferenceInSeconds()
  {
    if (connectTime is null) {
      return -1;
    }

    TimeSpan timeDifference = DateTime.Now - connectTime.Value;
    return (int)timeDifference.TotalSeconds;
  }

  private string GetTimeDifferenceFromNow()
  {
    if (connectTime is null) {
      return "N/A";
    }

    TimeSpan timeDifference = DateTime.Now - connectTime.Value;

    return string.Format(
      "{0:00}:{1:00}:{2:00}",
      (int)timeDifference.TotalHours,
      timeDifference.Minutes,
      timeDifference.Seconds
    );
  }

  public void Update(DeviceInfo deviceInfo)
  {
    try {
      connectTime = DateTime.ParseExact(
          deviceInfo.ppp_connect_time,
          "yyyy-MM-dd'T'HH:mm:ss'Z'",
          CultureInfo.InvariantCulture
      );
    } catch {
      connectTime = null;
    }
  }
}

public class SignalInfo
{
  public NetworkType networkType { get; } = new();
  public ConnectionTime connectionTime { get; } = new();

  public LteCellContainer lte { get; } = new();
  public NrCellContainer nr { get; } = new();

  public bool Update(DeviceInfo deviceInfo)
  {
    networkType.Update(deviceInfo);
    connectionTime.Update(deviceInfo);

    if (networkType.IsLte) {
      lte.Update(networkType, deviceInfo);
    }

    if (networkType.IsNr) {
      nr.Update(networkType, deviceInfo);
    }

    return true;
  }

  public void PrintSignalInfo()
  {
    Console.WriteLine("Signal Info:");
    Console.WriteLine();
    Console.WriteLine("Network Type: {0}  Time Connected: {1}",
        networkType.AsString, connectionTime.timeConnected);

    string bandsString = string.Join(" + ", GetBands());
    float totalBandwidth = GetTotalBandwidth();
    string totalBandwidthString = totalBandwidth > -1 ? $"  Total Bandwidth: {totalBandwidth} MHz" : "";

    Console.WriteLine("Bands: {0}{1}", bandsString, totalBandwidthString);
    Console.WriteLine();

    if (networkType.IsLte) {
      Console.WriteLine("LTE Signal:");
      Console.WriteLine();

      foreach (var cell in lte.cells) {
        string cellPrefix = cell.IsPrimaryCell ? "P" : "S";
        string bandwidthString = cell.bandwidth.Get() > -1 ? $" ({cell.bandwidth.Get()} MHz)" : "";

        Console.WriteLine("-- {0}Cell: B{1} - {2} / {3}{4} --",
            cellPrefix, cell.band.Get(), cell.pci.Get(), cell.earfcn.Get(), bandwidthString);

        PrintSignalValue(cell.IsPrimaryCell ? "RSRP1" : "RSRP", cell.rsrp1, "dBm");

        if (cell.IsPrimaryCell) {
          PrintSignalValue("RSRP2", cell.rsrp2, "dBm");
          PrintSignalValue("RSRP3", cell.rsrp3, "dBm");
          PrintSignalValue("RSRP4", cell.rsrp4, "dBm");
        }

        PrintSignalValue("RSRQ", cell.rsrq, "dB");
        PrintSignalValue(cell.IsPrimaryCell ? "SINR1" : "SINR", cell.sinr1, "dB");

        if (cell.IsPrimaryCell) {
          PrintSignalValue("SINR2", cell.sinr2, "dB");
          PrintSignalValue("SINR3", cell.sinr3, "dB");
          PrintSignalValue("SINR4", cell.sinr4, "dB");
        }

        Console.WriteLine();
      }
    }

    if (networkType.IsNr) {
      Console.WriteLine("NR Signal:");
      Console.WriteLine();

      foreach (var cell in nr.cells) {
        string cellPrefix = networkType.IsNrNsa ? "S" : (cell.IsPrimaryCell ? "P" : "S");
        string bandwidthString = cell.bandwidth.Get() > -1 ? $" ({cell.bandwidth.Get()} MHz)" : "";

        Console.WriteLine("-- {0}Cell: n{1} - {2} / {3}{4} --",
            cellPrefix, cell.band.Get(), cell.pci.Get(), cell.arfcn.Get(), bandwidthString);

        PrintSignalValue(cell.rsrp2.Ok ? "RSRP1" : "RSRP", cell.rsrp1, "dBm");
        if (cell.rsrp2.Ok)
          PrintSignalValue("RSRP2", cell.rsrp2, "dBm");
        PrintSignalValue("SINR", cell.sinr, "dB");

        Console.WriteLine();
      }
    }

    if (networkType.IsUmts) {
      Console.WriteLine("UMTS Signal:");
      Console.WriteLine("- Not implemented -");
    }

    Console.WriteLine();
  }

  private static void PrintSignalValue<T>(string valueName, SignalValue<T> signalValue, string valueSuffix)
      where T : struct, IConvertible
  {
    Console.WriteLine("{0}: {1} {2} (Min: {3} {2}, Max: {4} {2}, Avg: {5} {2})",
        valueName, signalValue.Current, valueSuffix, signalValue.Min,
        signalValue.Max, Math.Round(signalValue.Average, 2));
  }

  private float GetTotalBandwidth()
  {
    float totalBandwidth = 0;
    bool hasUndefinedBandwidth = false;

    if (networkType.IsLte) {
      foreach (var cell in lte.cells) {
        float bandwidth = cell.bandwidth.Get();
        if (bandwidth == -1) {
          hasUndefinedBandwidth = true;
        } else {
          totalBandwidth += bandwidth;
        }
      }
    }

    if (networkType.IsNr) {
      foreach (var cell in nr.cells) {
        float bandwidth = cell.bandwidth.Get();
        if (bandwidth == -1) {
          hasUndefinedBandwidth = true;
        } else {
          totalBandwidth += bandwidth;
        }
      }
    }

    return hasUndefinedBandwidth ? -1 : totalBandwidth;
  }

  private IEnumerable<string> GetBands()
  {
    if (networkType.IsLte) {
      foreach (var cell in lte.cells) {
        float bandwidth = cell.bandwidth.Get();
        string bandString = string.Format("B{0}", cell.band.Val);
        if (bandwidth > -1) {
          bandString += string.Format(" ({0} MHz)", bandwidth);
        }
        yield return bandString;
      }
    }

    if (networkType.IsNr) {
      foreach (var cell in nr.cells) {
        float bandwidth = cell.bandwidth.Get();
        string bandString = string.Format("n{0}", cell.band.Val);
        if (bandwidth > -1) {
          bandString += string.Format(" ({0} MHz)", bandwidth);
        }
        yield return bandString;
      }
    }
  }
}