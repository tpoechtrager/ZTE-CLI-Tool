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

using Microsoft.Extensions.Logging;
using ZTE_Cli_Tool.DTO;
using ZTE_Cli_Tool.Service.Interface;
using ZTE_Cli_Tool.TrafficThroughput;

namespace ZTE_Cli_Tool.Program;

public class Command
{
  private readonly ILogger<Command> _logger;
  private readonly IZteClient _zteClient;
  private readonly IZteHttpClient _zteHttpClient;

  public Command(ILogger<Command> logger, IZteClient zteClient, IZteHttpClient zteHttpClient)
  {
    _logger = logger;
    _zteClient = zteClient;
    _zteHttpClient = zteHttpClient;
  }

  /// <summary>
  /// Executes a command and prints the result.
  /// </summary>
  /// <param name="cmd">The debug command to execute.</param>
  /// <returns>
  /// True if the debug command was executed and printed successfully; otherwise, false.
  /// </returns>

  public async Task<bool> ExecuteCmdAsync(string cmd)
  {
    var result = await _zteHttpClient.ApiGetAsJsonAsync(cmd);

    if (result is not null) {
      Console.WriteLine(result);
    } else {
      Console.Error.WriteLine("Request failed!");
    }

    return true;
  }

  /// <summary>
  /// Attempts to establish a network connection and provides status feedback.
  /// </summary>
  /// <returns>
  /// True if the connection attempt was successful; otherwise, false.
  /// </returns>

  public async Task<bool> ConnectAsync()
  {
    if (await _zteClient.ConnectAsync()) {
      Console.WriteLine("Connected!");
    } else {
      Console.Error.WriteLine("Could not connect!");
    }

    return true;
  }

  /// <summary>
  /// Attempts to disconnect from the network and provides status feedback.
  /// </summary>
  /// <returns>
  /// True if the disconnection attempt was successful; otherwise, false.
  /// </returns>

  public async Task<bool> DisconnectAsync()
  {
    if (await _zteClient.DisconnectAsync()) {
      Console.WriteLine("Disconnected!");
    } else {
      Console.Error.WriteLine("Could not disconnect!");
    }

    return true;
  }


  /// <summary>
  /// Retrieves and prints the set bands on a ZTE router.
  /// </summary>
  /// <remarks>
  /// The bands are printed as a string joined by '+' characters.
  /// </remarks>
  /// <param name="getBandsAsync">A function that retrieves the bands to be printed.</param>
  /// <returns>
  /// True if the bands are successfully retrieved and printed, false otherwise.
  /// </returns>

  private async Task<bool> PrintSetBandsHelperAsync(Func<Task<IEnumerable<int>?>> getBandsAsync)
  {
    var bands = await getBandsAsync();

    if (bands is null) {
      Console.Error.WriteLine("Could not retrieve set bands!");
      return false;
    }

    Console.WriteLine(string.Join('+', bands));

    return true;
  }

  public async Task<bool> PrintNrBandLockAsync()
  {
    return await PrintSetBandsHelperAsync(_zteClient.GetNrBandLockAsync);
  }

  public async Task<bool> PrintLteBandLockAsync()
  {
    return await PrintSetBandsHelperAsync(_zteClient.GetLteBandLockAsync);
  }

  /// <summary>
  /// Sets bands on a ZTE router using the provided setBandsAsync function.
  /// </summary>
  /// <param name="setBandLockAsync">A function that sets the bands on the router.</param>
  /// <param name="bands">A string containing the bands to be set.</param>
  /// <returns>
  /// True if the bands are successfully set, false otherwise.
  /// </returns>
  /// 

  private async Task<bool> SetBandLockHelperAsync(
    Func<IEnumerable<int>?, Task<bool>> setBandLockAsync, string bands, string type)
  {
    IEnumerable<int> bandList = Tools.ConvertBandsToList(bands);

    if (await setBandLockAsync(bandList.Any() ? bandList : null)) {
      Console.WriteLine($"Set {type} band lock to {bands}!");
      return true;
    } else {
      Console.Error.WriteLine($"Could not set {type} band lock to {bands}!");
      return false;
    }
  }

  public async Task<bool> SetNrBandLockAsync(string bands)
  {
    return await SetBandLockHelperAsync(_zteClient.SetNrBandLockAsync, bands, "NR");
  }

  public async Task<bool> SetLteBandLockAsync(string bands)
  {
    return await SetBandLockHelperAsync(_zteClient.SetLteBandLockAsync, bands, "LTE");
  }

  /// <summary>
  /// Performs NR band hopping as a workaround for a bug in the ZTE routers that causes upload issues.
  /// This method is used when the upload speed drops to a few kbit/s. Performing the band hop
  /// helps to resolve the problem temporarily. It does not cause a disconnect,
  /// only a brief increase in ping for half a second or so.
  /// </summary>
  /// <param name="bands">A string containing two sets of NR bands separated by '|'.</param>
  /// <returns>
  /// True if the NR band hopping workaround is successful, false otherwise.
  /// </returns>

  public async Task<bool> PerformNrBandHopAsync(string bands)
  {
    string[] bandsArgs = bands.Split('|');

    if (bandsArgs.Length != 2) {
      Console.Error.WriteLine("Wrong argument to --perform-nr-band-hop.");
      return false;
    }

    return await _zteClient.PerformNrBandHopAsync(bandsArgs[0], bandsArgs[1]);
  }

  /// <summary>
  /// Retrieves and prints the network mode preference.
  /// </summary>
  /// <returns>
  /// True if the network mode preference was retrieved and printed successfully; otherwise, false.
  /// </returns>

  public async Task<bool> PrintNetworkPreferenceAsync()
  {
    var networkPreference = await _zteClient.GetNetworkModePreference();

    if (networkPreference is not null) {
      Console.WriteLine(networkPreference);
      return true;
    } else {
      Console.Error.WriteLine($"Retrieving network preference failed!");
      return false;
    }
  }

  /// <summary>
  /// Sets the network mode and handles the result.
  /// </summary>
  /// <param name="mode">The network mode to set.</param>
  /// <returns>True if the network mode was set successfully; otherwise, false.</returns>

  public async Task<bool> SetNetworkPreferenceAsync(string mode)
  {
    if (await _zteClient.SetNetworkPreferenceAsync(mode)) {
      Console.WriteLine($"Set network mode to {mode}!");
      return true;
    } else {
      Console.Error.WriteLine($"Setting network mode to ${mode} failed!");
      return false;
    }
  }

  /// <summary>
  /// Runs the device statistics update and display process.
  /// </summary>
  /// <param name="updateStats">
  ///   A function that updates statistics based on device information.
  /// </param>
  /// <param name="printStats">
  ///   An action to print the updated statistics to the console.
  /// </param>
  /// <param name="interval">
  ///   The time interval in milliseconds between updates. 
  ///   Use -1 to run the update process once without continuous updates;
  ///   otherwise, specify the desired interval (default is 1000ms).
  /// </param>
  /// <returns>
  ///   Returns false if an update fails; returns true if the interval is set to -1
  ///   and the update and displaying succeed; otherwise, this method may never return.
  /// </returns>

  private async Task<bool> ShowStatsHelperAsync(
    Func<DeviceInfo, bool>? updateStats, Action<DeviceInfo> printStats, int interval = 1000)
  {
    while (true) {
      if (!await _zteClient.CheckLoginAsync()) {
        return false;
      }

      await _zteClient.PreventAutoLogoutAsync();

      if (!await _zteClient.UpdateDeviceInfoAsync()) {
        Thread.Sleep(interval);
        continue;
      }

      Console.Clear();

      if (updateStats is not null && !updateStats(_zteClient.DeviceInfo)) {
        Console.Error.WriteLine("Updating stats failed!");
        return false;
      }

      printStats(_zteClient.DeviceInfo);

      if (interval == -1) {
        return true;
      }

      Thread.Sleep(interval);
    }
  }

  /// <summary>
  /// Continuously displays real-time signal information.
  /// </summary>
  /// <returns>
  /// Returns false if an error occurs; otherwise, this method may never return.
  /// </returns>

  public async Task<bool> ShowSignalInfoAsync()
  {
    SignalInfo.SignalInfo SignalInfo = new();
    return await ShowStatsHelperAsync(SignalInfo.Update, SignalInfo.PrintSignalInfo);
  }

  /// <summary>
  /// Displays total traffic statistics for the current month.
  /// </summary>
  /// <returns>
  /// Returns true if the operation succeeds; otherwise, false.
  /// </returns>

  public async Task<bool> ShowTotalTrafficStatsAsync()
  {
    static void printTotalTrafficStats(DeviceInfo deviceInfo)
    {
      DateTime start = DateTime.Now.AddSeconds(-Tools.ParseInt(deviceInfo.TotalTime, 0));

      Console.WriteLine("-- Total traffic statistics for current month --" + Environment.NewLine);

      Console.WriteLine("Start: " + start.ToString("yyyy-MM-dd HH:mm") + Environment.NewLine);

      Console.WriteLine("DL: {0}", Tools.FormatBytes(deviceInfo.TotalRxBytes));
      Console.WriteLine("UL: {0}", Tools.FormatBytes(deviceInfo.TotalTxBytes));

      Console.WriteLine("");

      Console.WriteLine("DL Packets: {0}", deviceInfo.TotalRxPackets);
      Console.WriteLine("UL Packets: {0}", deviceInfo.TotalTxPackets);
    }

    return await ShowStatsHelperAsync(null, printTotalTrafficStats, -1);
  }

  /// <summary>
  /// Displays traffic statistics for the current month.
  /// </summary>
  /// <returns>
  /// Returns true if the operation succeeds; otherwise, false.
  /// </returns>

  public async Task<bool> ShowMonthlyTrafficStatsAsync()
  {
    static void printMonthlyTrafficStats(DeviceInfo deviceInfo)
    {
      DateTime start = DateTime.Now.AddSeconds(-Tools.ParseInt(deviceInfo.MonthlyTime, 0));

      Console.WriteLine("-- Traffic statistics for current month --" + Environment.NewLine);

      Console.WriteLine("Start: " + start.ToString("yyyy-MM-dd HH:mm") + Environment.NewLine);

      Console.WriteLine("DL: {0}", Tools.FormatBytes(deviceInfo.MonthlyRxBytes));
      Console.WriteLine("UL: {0}", Tools.FormatBytes(deviceInfo.MonthlyTxBytes));

      Console.WriteLine("");

      Console.WriteLine("DL Packets: {0}", deviceInfo.MonthlyRxPackets);
      Console.WriteLine("UL Packets: {0}", deviceInfo.MonthlyTxPackets);
    }

    return await ShowStatsHelperAsync(null, printMonthlyTrafficStats, -1);
  }

  /// <summary>
  /// Continuously displays live traffic statistics.
  /// </summary>
  /// <returns>
  /// Returns false if an error occurs; otherwise, this method may never return.
  /// </returns>

  public async Task<bool> ShowLiveTrafficStatsAsync()
  {
    ThroughputCalculator rxPackets = new();
    ThroughputCalculator txPackets = new();

    bool updateTrafficStats(DeviceInfo deviceInfoNew)
    {
      rxPackets.Update(deviceInfoNew.RealtimeRxPackets);
      txPackets.Update(deviceInfoNew.RealtimeTxPackets);
      return true;
    }

    void printTrafficStats(DeviceInfo deviceInfo)
    {
      Console.Write("DL: {0:0.00} Mbit/s  UL: {1:0.00} Mbit/s",
        Tools.BytesToMbits(deviceInfo.RealtimeRxThrpt),
        Tools.BytesToMbits(deviceInfo.RealtimeTxThrpt));

      if (rxPackets.Updates < 2) {
        Console.WriteLine("");
        return;
      }

      Console.WriteLine("  |  DL: {0:0} p/s  UL: {1:0} p/s",
        rxPackets.Throughput, txPackets.Throughput);
    }

    return await ShowStatsHelperAsync(updateTrafficStats, printTrafficStats, 2000);
  }


}