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
using System.Globalization;
using ZTE_Cli_Tool.Service.Interface;

namespace ZTE_Cli_Tool.Program;

public class ZteCliTool
{
  private readonly ILogger<ZteCliTool> _logger;
  private readonly IZteClient _zteClient;
  private readonly Command _command;

  public ZteCliTool(ILogger<ZteCliTool> logger, IZteClient zteClient, Command command)
  {
    _logger = logger;
    _zteClient = zteClient;
    _command = command;
  }

  public class CommandLineArgs
  {
    public string RouterIp { get; set; } = "192.168.0.10";
    public string RouterPassword { get; set; } = "admin1";

    public string? DebugCmd { get; set; } = null;

    // NR
    public bool PrintSetNrBands { get; set; } = false;
    public string? SetNrBandLock { get; set; } = null;
    public string? PerformNrBandHop { get; set; }

    // LTE
    public bool PrintLteBandLock { get; set; } = false;
    public string? SetLteBandLock { get; set; } = null;

    // Network
    public bool Connect { get; set; } = false;
    public bool Disconnect { get; set; } = false;
    public bool PrintNetworkPreference { get; set; } = false;
    public string? SetNetworkPreference { get; set; } = null;
  }

  private CommandLineArgs? ParseCommandLineArgs(string[] args)
  {
    var parsedArgs = new CommandLineArgs();

    if (args.Length == 0) {
      return parsedArgs;
    }

    int startIndex = args[0].StartsWith('/') || args[0].StartsWith("./") ? 1 : 0;

    for (int i = startIndex; i < args.Length; i++) {
      string arg = args[i];
      string getNextArg()
      {
        return i < args.Length - 1 ? args[++i] : "";
      }

      switch (arg) {
        case "--router":
        case "--router-ip":
        case "--ip":
          parsedArgs.RouterIp = getNextArg();
          break;
        case "--router-password":
        case "--pass":
        case "--password":
          parsedArgs.RouterPassword = getNextArg();
          break;

        case "--debug-cmd":
          parsedArgs.DebugCmd = getNextArg();
          break;

        // NR

        case "--print-nr-band-lock":
          parsedArgs.PrintSetNrBands = true;
          break;
        case "--set-nr-band-lock":
          parsedArgs.SetNrBandLock = getNextArg();
          break;
        case "--perform-nr-band-hop":
          parsedArgs.PerformNrBandHop = getNextArg();
          break;

        // LTE

        case "--print-lte-band-lock":
          parsedArgs.PrintLteBandLock = true;
          break;
        case "--set-lte-band-lock":
          parsedArgs.SetLteBandLock = getNextArg();
          break;

        // Network

        case "--connect":
          parsedArgs.Connect = true;
          break;

        case "--disconnect":
          parsedArgs.Disconnect = true;
          break;

        case "--print-network-preference":
          parsedArgs.PrintNetworkPreference = true;
          break;

        case "--set-network-preference":
          parsedArgs.SetNetworkPreference = getNextArg();
          break;

        default:
          _logger.LogError($"Unknown command line argument: {arg}");
          return null;
      }
    }

    return parsedArgs;
  }

  /// <summary>
  /// Executes a command based on the parsed command-line arguments.
  /// </summary>
  /// <param name="zteClient">The ZteClient instance to use for the command execution.</param>
  /// <param name="parsedArgs">The parsed command-line arguments.</param>
  /// <returns>
  /// True if the command was executed successfully, false if the command failed,
  /// or null if no command has been executed.
  /// </returns>

  private async Task<bool?> ExecuteCommandAsync(IZteClient zteClient, CommandLineArgs parsedArgs)
  {
    if (parsedArgs.DebugCmd is not null) {
      return await _command.DebugCmdAsync(parsedArgs.DebugCmd);
    }

    // NR

    else if (parsedArgs.PrintSetNrBands) {
      return await _command.PrintNrBandLockAsync();
    } else if (parsedArgs.SetNrBandLock is not null) {
      return await _command.SetNrBandLockAsync(parsedArgs.SetNrBandLock);
    } else if (parsedArgs.PerformNrBandHop is not null) {
      return await _command.PerformNrBandHopAsync(parsedArgs.PerformNrBandHop);
    }

    // LTE

    else if (parsedArgs.PrintLteBandLock) {
      return await _command.PrintLteBandLockAsync();
    } else if (parsedArgs.SetLteBandLock is not null) {
      return await _command.SetLteBandLockAsync(parsedArgs.SetLteBandLock);
    }

    // Network

    else if (parsedArgs.Connect) {
      return await _command.ConnectAsync();
    } else if (parsedArgs.Disconnect) {
      return await _command.DisconnectAsync();
    } else if (parsedArgs.PrintNetworkPreference) {
      return await _command.PrintNetworkPreferenceAsync();
    } else if (parsedArgs.SetNetworkPreference is not null) {
      return await _command.SetNetworkPreferenceAsync(parsedArgs.SetNetworkPreference);
    }

    return null;
  }

  public async Task<int> ExecuteAsync(string[] args)
  {
    Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US", false);

    // Parse command line arguments

    CommandLineArgs? parsedArgs = ParseCommandLineArgs(args);

    if (parsedArgs is null) {
      return 1;
    }

    // Create zteClient instance

    await _zteClient.InitializeServiceAsync(parsedArgs.RouterIp, parsedArgs.RouterPassword);

    // Perform login

    if (!await _zteClient.CheckLoginAsync()) {
      return 1;
    }

    // Execute commands

    bool? commandResult = await ExecuteCommandAsync(_zteClient, parsedArgs);

    if (commandResult.HasValue) {
      return commandResult.Value ? 0 : 1;
    }

    // Otherwise show signal info

    while (true) {
      if (!await _zteClient.CheckLoginAsync()) {
        break;
      }

      await _zteClient.PreventAutoLogoutAsync();

      if (!await _zteClient.UpdateDeviceInfoAsync()) {
        Thread.Sleep(1000);
        continue;
      }

      Console.Clear();

      _zteClient.SignalInfo.PrintSignalInfoAsync();

      Thread.Sleep(1000);
    }

    return 1;
  }
}
