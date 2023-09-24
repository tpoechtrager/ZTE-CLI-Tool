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
    public string? PerformNrBandHop { get; set; }
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
        case "--router-ip":
        case "--ip":
          parsedArgs.RouterIp = getNextArg();
          break;
        case "--pass":
        case "--password":
          parsedArgs.RouterPassword = getNextArg();
          break;
        case "--perform-nr-band-hop":
          parsedArgs.PerformNrBandHop = getNextArg();
          break;
        default:
          _logger.LogError("Unknown command line argument: " + arg);
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

  private async Task<bool?> ExecuteCommand(IZteClient zteClient, CommandLineArgs parsedArgs)
  {
    if (parsedArgs.PerformNrBandHop is not null) {
      return await _command.PerformNrBandHop(zteClient, parsedArgs.PerformNrBandHop);
    }

    return null;
  }

  public async Task<int> Execute(string[] args)
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

    if (!await _zteClient.CheckLogin()) {
      return 1;
    }

    // Execute commands

    bool? commandResult = await ExecuteCommand(_zteClient, parsedArgs);

    if (commandResult.HasValue) {
      return commandResult.Value ? 0 : 1;
    }

    // Otherwise show signal info

    while (true) {
      if (!await _zteClient.CheckLogin()) {
        break;
      }

      await _zteClient.PreventAutoLogoutAsync();

      if (!await _zteClient.UpdateDeviceInfoAsync()) {
        Thread.Sleep(1000);
        continue;
      }

      await _zteClient.GetSetNrBandsAsync();

      Console.Clear();

      _zteClient.SignalInfo.PrintSignalInfo();

      Thread.Sleep(1000);
    }

    return 1;
  }
}
