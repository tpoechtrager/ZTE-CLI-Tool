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
using System.Text;
using System.Text.Json;
using ZTE_Cli_Tool.DTO;
using ZTE_Cli_Tool.Service.Interface;

namespace ZTE_Cli_Tool.Service;

public class ZteHttpClient : IZteHttpClient, IDisposable
{
  private readonly int HTTP_REQUEST_TIMEOUT = 5000;

  private readonly ILogger<ZteHttpClient> _logger;
  private string _routerIpAddress = "";
  private string _httpProtocol = "";
  private HttpClientHandler httpClientHandler;
  private HttpClient httpClient;

  public ZteHttpClient(ILogger<ZteHttpClient> logger)
  {
    _logger = logger;

    // Accept any certificate as self signed certificates
    // are not valid.
    httpClientHandler = new HttpClientHandler() {
      ServerCertificateCustomValidationCallback =
        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    };

    httpClient = new HttpClient(httpClientHandler);
  }

  public async Task InitializeAsync(string routerIpAddress)
  {
    _routerIpAddress = routerIpAddress;

    // Try to figure out whether the router uses https or http
    _httpProtocol = "https";

    if (!(await ApiRequestAsync("index.html")).success) {
      _httpProtocol = "http";
    }

    // Add default HttpClient headers
    httpClient.DefaultRequestHeaders.Add("Referer", $"{_httpProtocol}://{_routerIpAddress}/index.html");
    httpClient.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
  }

  /// <summary>
  /// Disposes of the HTTP client and releases associated resources.
  /// </summary>

  public void Dispose()
  {
    httpClient.Dispose();
  }

  public class ApiResult
  {
    public bool success = false;
    public string responseText = string.Empty;
    public string contentType = string.Empty;
  };

  public async Task<ApiResult> ApiRequestAsync(string request, Dictionary<string, string>? post = null)
  {
    string requestAppend = string.Format("isTest=false&_={0}", DateTimeOffset.Now.ToUnixTimeMilliseconds());

    // Add multi_data=1 if we want to receive multiple objects
    if (request.Contains(',')) {
      requestAppend += "&multi_data=1";
    }

    if (request.Contains('?')) {
      request += "&" + requestAppend;
    } else {
      request += "?" + requestAppend;
    }

    string requestUri = $"{_httpProtocol}://{_routerIpAddress}/{request}";

    CancellationTokenSource cts = new CancellationTokenSource(HTTP_REQUEST_TIMEOUT);
    HttpResponseMessage? httpResponseMessage;
    string responseText;

    try {
      if (post is not null) {
        post.Add("isTest", "false"); // Always add isTest=false
                                     // Add multi_data=1 if we want to receive multiple objects
        if (post.TryGetValue("cmd", out string? value) && value.Contains(',')) {
          post.Add("multi_data", "1");
        }
        string postUrlEncoded = await new FormUrlEncodedContent(post).ReadAsStringAsync();
        HttpContent httpContent = new StringContent(postUrlEncoded, Encoding.UTF8, "application/x-www-form-urlencoded");
        httpResponseMessage = await httpClient.PostAsync(requestUri, httpContent, cts.Token);
      } else {
        httpResponseMessage = await httpClient.GetAsync(requestUri, cts.Token);
      }

      responseText = await httpResponseMessage.Content.ReadAsStringAsync(cts.Token);
    } catch (TaskCanceledException) {
      _logger.LogError("Request Timeout");
      return new ApiResult() { success = false };
    } catch (Exception ex) {
      _logger.LogError($"Exception: {ex}");
      return new ApiResult() { success = false };
    }

    return new ApiResult() {
      success = httpResponseMessage.IsSuccessStatusCode,
      responseText = responseText,
      contentType = httpResponseMessage?.Content?.Headers?.ContentType?.ToString() ?? ""
    };
  }

  /// <summary>
  /// Sends an asynchronous API GET request with optional POST data and returns the response as a JSON string.
  /// </summary>
  /// <param name="cmd">The optional command to include in the GET request.</param>
  /// <param name="post">Optional POST data for the request.</param>
  /// <returns>The JSON response string.</returns>

  private async Task<string?> ApiGetAsJsonHelperAsync(string? cmd, Dictionary<string, string>? post = null)
  {
    var result = await ApiRequestAsync("goform/goform_get_cmd_process" + (cmd is not null ? "?cmd=" + cmd : ""), post);

    if (!result.success) {
      _logger.LogError("Request failed");
      return null;
    }

    return result.responseText;
  }

  public Task<string?> ApiGetAsJsonAsync(string cmd)
    => ApiGetAsJsonHelperAsync(cmd);

  public Task<string?> ApiGetAsJsonAsync(Dictionary<string, string> post)
  => ApiGetAsJsonHelperAsync(null, post);


  /// <summary>
  /// Sends an asynchronous API SET request with optional POST data and returns the response as an ApiResult.
  /// </summary>
  /// <param name="post">Optional POST data for the request.</param>
  /// <returns>An ApiResult containing the response information.</returns>

  private async Task<ApiResult?> ApiSetHelperAsync(Dictionary<string, string>? post)
  {
    var result = await ApiRequestAsync("/goform/goform_set_cmd_process", post);

    if (!result.success) {
      _logger.LogError("Request failed");
      return null;
    }

    return result;
  }

  public async Task<bool> ApiSetAsync(Dictionary<string, string>? post)
  {
    var result = await ApiSetHelperAsync(post);

    if (result is not null && result.success) {
      SetResult? setResult;

      try {
        setResult = JsonSerializer.Deserialize<SetResult>(result.responseText);
      } catch {
        return false;
      }

      return setResult is not null && setResult.SetSuccessful();
    }

    return false;
  }

  public async Task<string?> ApiSetAsJsonAsync(Dictionary<string, string>? post)
  {
    var result = await ApiSetHelperAsync(post);
    return result is not null && result.success ? result.responseText : null;
  }
}
