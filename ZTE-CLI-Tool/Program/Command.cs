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
using ZTE_Cli_Tool.Service.Interface;

namespace ZTE_Cli_Tool.Program;

public class Command
{
  private readonly ILogger<Command> _logger;

  public Command(ILogger<Command> logger)
  {
    _logger = logger;
  }

  /// <summary>
  /// Performs NR band hopping as a workaround for a bug in the ZTE routers that causes upload issues.
  /// This method is used when the upload speed drops to a few kbit/s. Performing the band hop
  /// helps to resolve the problem temporarily. It does not cause a disconnect,
  /// only a brief increase in ping for half a second or so.
  /// </summary>
  /// <param name="zte_Client">The ZteClient instance to use for the operation.</param>
  /// <param name="bands">A string containing two sets of NR bands separated by '|'.</param>
  /// <returns>
  /// True if the NR band hopping workaround is successful, false otherwise.
  /// </returns>

  public async Task<bool> PerformNrBandHopAsync(IZteClient zte_Client, string bands)
  {
    string[] bandsArgs = bands.Split('|');

    if (bandsArgs.Length != 2) {
      _logger.LogError("Wrong argument to --perform-nr-band-hop.");
      return false;
    }

    return await zte_Client.PerformNrBandHopAsync(bandsArgs[0], bandsArgs[1]);
  }

  /// <summary>
  /// Retrieves and prints the NR bands set on a ZTE router.
  /// </summary>
  /// <param name="zte_Client">The ZteClient instance to use for the operation.</param>
  /// <remarks>
  /// The NR bands are printed as a string joined by '+' characters.
  /// </remarks>
  /// <returns>
  /// True if the NR bands are successfully retrieved and printed, false otherwise.
  /// </returns>

  public async Task<bool> PrintSetNrBandsAsync(IZteClient zte_Client)
  {
    var bands = await zte_Client.GetSetNrBandsAsync();

    if (bands is null) {
      _logger.LogError("Could not retrieve set NR bands!");
      return false;
    }

    Console.WriteLine(string.Join('+', bands));

    return true;
  }
}