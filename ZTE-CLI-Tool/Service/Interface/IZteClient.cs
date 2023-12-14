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
  /// Initializes the ZteClient service.
  /// </summary>
  /// <param name="routerIpAddress">The IP address of the router.</param>
  /// <param name="routerPassword">The password for the router.</param>
  /// <param name="loginRetryWait">Login throttling in milliseconds</param>
  Task InitializeServiceAsync(string routerIpAddress, string routerPassword, int loginRetryWait);

  /// <summary>
  /// Attempts to log in and returns a LoginErrorCode.
  /// </summary>
  /// <returns>The LoginErrorCode if the login is successful; otherwise, null.</returns>
  Task<int?> LoginAsync();

  /// <summary>
  /// Checks if the user is logged in.
  /// </summary>
  /// <returns>True if logged in; otherwise, false.</returns>
  Task<bool> CheckLoginAsync();

  /// <summary>
  /// Prevents automatic logout by performing a dummy request.
  /// Ensures that only one request is made per minute to avoid frequent API calls.
  /// </summary>
  Task PreventAutoLogoutAsync();

  /// <summary>
  /// Attempts to connect to the network.
  /// </summary>
  /// <returns>
  /// True if the connection attempt was successful; otherwise, false.
  /// </returns>
  Task<bool> ConnectAsync(bool disconnect = false);

  /// <summary>
  /// Attempts to disconnect from the network.
  /// </summary>
  /// <returns>
  /// True if the disconnection attempt was successful; otherwise, false.
  /// </returns>
  Task<bool> DisconnectAsync();

  /// <summary>
  /// Retrieves the NR (New Radio) bands settings from the API.
  /// </summary>
  /// <returns>An IEnumerable of integers representing NR bands settings, or null if the operation fails.</returns>
  Task<IEnumerable<int>?> GetNrBandLockAsync();

  /// <summary>
  /// Sets the New Radio (NR) bands for the ZTE device. Use null to set all bands for automatic selection.
  /// </summary>
  /// <param name="bands">An optional collection of NR bands to set as integers or null to set all bands.</param>
  /// <returns>True if the operation was successful, otherwise false.</returns>
  Task<bool> SetNrBandLockAsync(IEnumerable<int>? bands);

  /// <summary>
  /// Performs NR band hopping by setting two sets of NR bands.
  /// </summary>
  /// <param name="bands1">The first set of NR bands to set.</param>
  /// <param name="bands2">The second set of NR bands to set.</param>
  /// <returns>True if both sets of NR bands are set successfully, false otherwise.</returns>
  Task<bool> PerformNrBandHopAsync(string bands1, string bands2);

  /// <summary>
  /// Sets the LTE bands for the ZTE device. Use null to set all bands for automatic selection.
  /// </summary>
  /// <param name="bands">An optional collection of LTE bands to set as integers or null to set all bands.</param>
  /// <returns>True if the operation was successful, otherwise false.</returns>
  Task<bool> SetLteBandLockAsync(IEnumerable<int>? bands);

  /// <summary>
  /// Retrieves the LTE bands settings from the API.
  /// </summary>
  /// <returns>An IEnumerable of integers representing LTE bands settings, or null if the operation fails.</returns>
  Task<IEnumerable<int>?> GetLteBandLockAsync();

  /// <summary>
  /// This method retrieves the network mode preference.
  /// </summary>
  /// <returns>
  /// A string representing the network mode preference, or null if there was an error.
  /// </returns>
  Task<string?> GetNetworkModePreference();

  /// <summary>
  /// Sets the network mode.
  /// </summary>
  /// <param name="mode">The network mode to set.</param>
  /// <returns>True if the network mode was set successfully; otherwise, false.</returns>
  Task<bool> SetNetworkPreferenceAsync(string mode);

  /// <summary>
  /// Updates device information asynchronously.
  /// </summary>
  /// <returns>True if the update was successful; otherwise, false.</returns>
  Task<bool> UpdateDeviceInfoAsync();
}
