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
          deviceInfo.PppConnectTime,
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
  public NetworkType NetworkType { get; } = new();
  public ConnectionTime ConnectionTime { get; } = new();

  public LteCellContainer Lte { get; } = new();
  public NrCellContainer Nr { get; } = new();

  public bool Update(DeviceInfo deviceInfo)
  {
    NetworkType.Update(deviceInfo);
    ConnectionTime.Update(deviceInfo);

    if (NetworkType.IsLte) {
      Lte.Update(NetworkType, deviceInfo);
    }

    if (NetworkType.IsNr) {
      Nr.Update(NetworkType, deviceInfo);
    }

    return true;
  }

  public void PrintSignalInfo()
  {
    Console.WriteLine("Signal Info:");
    Console.WriteLine();
    Console.WriteLine("Network Type: {0}  Time Connected: {1}",
        NetworkType.AsString, ConnectionTime.timeConnected);

    string bandsString = string.Join(" + ", GetBands());
    float totalBandwidth = GetTotalBandwidth();
    string totalBandwidthString = totalBandwidth > -1 ? $"  Total Bandwidth: {totalBandwidth} MHz" : "";

    Console.WriteLine("Bands: {0}{1}", bandsString, totalBandwidthString);
    Console.WriteLine();

    if (NetworkType.IsLte) {
      Console.WriteLine("LTE Signal:");
      Console.WriteLine();

      foreach (var cell in Lte.Cells) {
        string cellPrefix = cell.IsPrimaryCell ? "P" : "S";
        string bandwidthString = cell.Bandwidth.Get() > -1 ? $" ({cell.Bandwidth.Get()} MHz)" : "";

        Console.WriteLine("-- {0}Cell: B{1} - {2} / {3}{4} --",
            cellPrefix, cell.Band.Get(), cell.Pci.Get(), cell.Earfcn.Get(), bandwidthString);

        PrintSignalValue(cell.IsPrimaryCell ? "RSRP1" : "RSRP", cell.Rsrp1, "dBm");

        if (cell.IsPrimaryCell) {
          PrintSignalValue("RSRP2", cell.Rsrp2, "dBm");
          PrintSignalValue("RSRP3", cell.Rsrp3, "dBm");
          PrintSignalValue("RSRP4", cell.Rsrp4, "dBm");
        }

        PrintSignalValue("RSRQ", cell.Rsrq, "dB");
        PrintSignalValue(cell.IsPrimaryCell ? "SINR1" : "SINR", cell.Sinr1, "dB");

        if (cell.IsPrimaryCell) {
          PrintSignalValue("SINR2", cell.Sinr2, "dB");
          PrintSignalValue("SINR3", cell.Sinr3, "dB");
          PrintSignalValue("SINR4", cell.Sinr4, "dB");
        }

        Console.WriteLine();
      }
    }

    if (NetworkType.IsNr) {
      Console.WriteLine("NR Signal:");
      Console.WriteLine();

      foreach (var cell in Nr.Cells) {
        string cellPrefix = NetworkType.IsNrNsa ? "S" : (cell.IsPrimaryCell ? "P" : "S");
        string bandwidthString = cell.Bandwidth.Get() > -1 ? $" ({cell.Bandwidth.Get()} MHz)" : "";

        Console.WriteLine("-- {0}Cell: n{1} - {2} / {3}{4} --",
            cellPrefix, cell.Band.Get(), cell.Pci.Get(), cell.Arfcn.Get(), bandwidthString);

        PrintSignalValue(cell.Rsrp2.Ok ? "RSRP1" : "RSRP", cell.Rsrp1, "dBm");
        if (cell.Rsrp2.Ok)
          PrintSignalValue("RSRP2", cell.Rsrp2, "dBm");
        PrintSignalValue("SINR", cell.Sinr, "dB");

        Console.WriteLine();
      }
    }

    if (NetworkType.IsUmts) {
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

    if (NetworkType.IsLte) {
      foreach (var cell in Lte.Cells) {
        float bandwidth = cell.Bandwidth.Get();
        if (bandwidth == -1) {
          hasUndefinedBandwidth = true;
        } else {
          totalBandwidth += bandwidth;
        }
      }
    }

    if (NetworkType.IsNr) {
      foreach (var cell in Nr.Cells) {
        float bandwidth = cell.Bandwidth.Get();
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
    if (NetworkType.IsLte) {
      foreach (var cell in Lte.Cells) {
        float bandwidth = cell.Bandwidth.Get();
        string bandString = string.Format("B{0}", cell.Band.Val);
        if (bandwidth > -1) {
          bandString += string.Format(" ({0} MHz)", bandwidth);
        }
        yield return bandString;
      }
    }

    if (NetworkType.IsNr) {
      foreach (var cell in Nr.Cells) {
        float bandwidth = cell.Bandwidth.Get();
        string bandString = string.Format("n{0}", cell.Band.Val);
        if (bandwidth > -1) {
          bandString += string.Format(" ({0} MHz)", bandwidth);
        }
        yield return bandString;
      }
    }
  }
}