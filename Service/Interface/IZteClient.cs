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

namespace ZTE_Cli_Tool.Service.Interface;

using global::ZTE_Cli_Tool.DTO;

using System;
using System.Threading.Tasks;

public interface IZteClient : IDisposable
{
  /// <summary>
  /// Gets information about the device.
  /// </summary>
  DeviceInfo DeviceInfo { get; }

  /// <summary>
  /// Gets signal information.
  /// </summary>
  SignalInfo.SignalInfo SignalInfo { get; }

  /// <summary>
  /// Initializes the ZteClient service.
  /// </summary>
  /// <param name="routerIpAddress">The IP address of the router.</param>
  /// <param name="routerPassword">The password for the router.</param>
  Task InitializeServiceAsync(string routerIpAddress, string routerPassword);

  /// <summary>
  /// Attempts to log in and returns a LoginErrorCode.
  /// </summary>
  /// <returns>The LoginErrorCode if the login is successful; otherwise, null.</returns>
  Task<int?> LoginAsync();

  /// <summary>
  /// Checks if the user is logged in.
  /// </summary>
  /// <returns>True if logged in; otherwise, false.</returns>
  Task<bool> CheckLogin();

  /// <summary>
  /// Prevents automatic logout by performing a dummy request.
  /// Ensures that only one request is made per minute to avoid frequent API calls.
  /// </summary>
  Task PreventAutoLogoutAsync();

  /// <summary>
  /// Sets the New Radio (NR) bands for the ZTE device.
  /// </summary>
  /// <param name="bands">A +-separated string of NR bands to set. Use "auto" for automatic selection.</param>
  /// <returns>True if the operation was successful, otherwise false.</returns>
  Task<bool> SetNrBandsAsync(string bands = "auto");

  /// <summary>
  /// Retrieves the NR bands settings as JSON from the API.
  /// </summary>
  /// <returns>The JSON representation of NR bands settings, or null if the operation fails.</returns>
  Task<string?> GetSetNrBandsAsync();

  /// <summary>
  /// Performs NR band hopping by setting two sets of NR bands.
  /// </summary>
  /// <param name="bands1">The first set of NR bands to set.</param>
  /// <param name="bands2">The second set of NR bands to set.</param>
  /// <returns>True if both sets of NR bands are set successfully, false otherwise.</returns>
  Task<bool> PerformNrBandHop(string bands1, string bands2);

  /// <summary>
  /// Updates device information asynchronously.
  /// </summary>
  /// <returns>True if the update was successful; otherwise, false.</returns>
  Task<bool> UpdateDeviceInfoAsync();
}
