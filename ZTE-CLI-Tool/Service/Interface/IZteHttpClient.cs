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

using static ZTE_Cli_Tool.Service.ZteHttpClient;

namespace ZTE_Cli_Tool.Service.Interface;

public interface IZteHttpClient : IDisposable
{
  /// <summary>
  /// Initializes the HTTP client and determines the HTTP protocol (HTTP or HTTPS) to use.
  /// </summary>
  /// <returns>A task representing the initialization process.</returns>
  Task InitializeAsync(string routerIpAddress);

  /// <summary>
  /// Sends an asynchronous API request to the router.
  /// </summary>
  /// <param name="request">The API request path.</param>
  /// <param name="post">Optional POST data for the request.</param>
  /// <returns>An <see cref="ApiResult"/> containing the response information.</returns>
  Task<ApiResult> ApiRequestAsync(string request, Dictionary<string, string>? post = null);

  /// <summary>
  /// Sends an asynchronous API GET request and returns the response as a JSON string.
  /// </summary>
  /// <param name="cmd">The command to include in the GET request.</param>
  /// <returns>The JSON response string.</returns>
  Task<string?> ApiGetAsJsonAsync(string cmd);

  /// <summary>
  /// Sends an asynchronous API GET request with POST data and returns the response as a JSON string.
  /// </summary>
  /// <param name="post">The POST data for the request.</param>
  /// <returns>The JSON response string.</returns>
  Task<string?> ApiGetAsJsonAsync(Dictionary<string, string> post);

  /// <summary>
  /// Sends an asynchronous API SET request and returns whether the request was successful.
  /// </summary>
  /// <param name="post">The POST data for the request.</param>
  /// <returns>True if the request was successful, otherwise false.</returns>
  Task<bool> ApiSetAsync(Dictionary<string, string>? post);

  /// <summary>
  /// Sends an asynchronous API SET request and returns the response as a JSON string.
  /// </summary>
  /// <param name="post">The POST data for the request.</param>
  /// <returns>The JSON response string.</returns>
  Task<string?> ApiSetAsJsonAsync(Dictionary<string, string>? post);
}