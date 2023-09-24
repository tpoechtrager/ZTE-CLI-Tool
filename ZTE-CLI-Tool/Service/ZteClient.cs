﻿/*
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
using Newtonsoft.Json.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ZTE_Cli_Tool.DTO;
using ZTE_Cli_Tool.Service.Interface;

namespace ZTE_Cli_Tool.Service;

public class ZteClient : IZteClient, IDisposable
{
  private readonly ILogger<ZteClient> _logger;
  private readonly IZteHttpClient _zteHttpClient;
  private readonly SHA256 sha256 = SHA256.Create();

  private string _routerPassword = "";
  private bool _loggedIn = false;
  private int _successfulLogins = 0;

  public DeviceInfo DeviceInfo { get; private set; } = new();
  public SignalInfo.SignalInfo SignalInfo { get; private set; } = new();

  private static readonly string DEVICE_INFO_REQUEST =
    "loginfo," +

    "wan_active_band,wan_active_channel,wan_lte_ca,wan_apn,wan_ipaddr," +
    "cell_id,dns_mode,prefer_dns_manual,standby_dns_manual,network_type," +
    "network_provider_fullname," +

    "ppp_connect_time," +

    "rmcc,rmnc,nas_rrc_state" +

    "ip_passthrough_enabled," +

    "bandwidth," +
    "tx_power," +

    "rscp_1,ecio_1,rscp_2,ecio_2,rscp_3,ecio_3,rscp_4,ecio_4," +

    "ngbr_cell_info," +
    "lte_multi_ca_scell_info,lte_multi_ca_scell_sig_info," +
    "lte_band,lte_rsrp,lte_rsrq," +
    "lte_rsrq,lte_rssi,lte_rsrp,lte_snr," +
    "lte_ca_pcell_band,lte_ca_pcell_freq,lte_ca_pcell_bandwidth," +
    "lte_ca_scell_band,lte_ca_scell_bandwidth," +
    "lte_rsrp_1,lte_rsrp_2,lte_rsrp_3,lte_rsrp_4," +
    "lte_snr_1,lte_snr_2,lte_snr_3,lte_snr_4," +
    "lte_pci,lte_pci_lock,lte_earfcn_lock," +

    "5g_rx0_rsrp,5g_rx1_rsrp,Z5g_rsrp,Z5g_rsrq,Z5g_SINR," +
    "nr5g_cell_id,nr5g_pci," +
    "nr5g_action_channel,nr5g_action_band," +
    "nr5g_action_nsa_band," +
    "nr_ca_pcell_band,nr_ca_pcell_freq," +
    "nr_multi_ca_scell_info," +
    "nr5g_sa_band_lock,nr5g_nsa_band_lock," +

    "pm_sensor_ambient,pm_sensor_mdm,pm_sensor_5g,pm_sensor_pa1,wifi_chip_temp";

  public ZteClient(ILogger<ZteClient> logger, IZteHttpClient zteHttpClient)
  {
    _logger = logger;
    _zteHttpClient = zteHttpClient;
  }

  public async Task InitializeServiceAsync(string routerIpAddress, string routerPassword)
  {
    _routerPassword = routerPassword;
    await _zteHttpClient.InitializeAsync(routerIpAddress);
  }

  public void Dispose()
  {
    _zteHttpClient.Dispose(); // Needed?
    sha256.Dispose();
  }

  /// <summary>
  /// Computes and returns an upper-cased SHA256 hash of the input text.
  /// ZTE's API compares SHA256 hashes in upper-case.
  /// </summary>
  /// <param name="text">The text to hash.</param>
  /// <returns>
  /// The upper-cased SHA256 hash as a string.
  /// </returns>

  private string ZteSha256(string text)
  {
    byte[] result = sha256.ComputeHash(Encoding.UTF8.GetBytes(text));
    return Convert.ToHexString(result).ToUpper();
  }

  /// <summary>
  /// Retrieves an NV.
  /// </summary>
  /// <param name="nv">The NV value to retrieve.</param>
  /// <returns>
  /// The retrieved value as a string, or null if the operation fails.
  /// </returns>

  private async Task<string?> GetNvAsync(string nv)
  {
    var result = await _zteHttpClient.ApiRequestAsync($"goform/goform_get_cmd_process?cmd={nv}");

    if (!result.success) {
      _logger.LogError("Could not get nv!");
      return null;
    }

    JObject jsonObject;

    try {
      jsonObject = JObject.Parse(result.responseText);
    } catch (Exception ex) {
      _logger.LogError($"Exception: {ex}");
      return null;
    }

    if (!jsonObject.TryGetValue(nv, out var value)) {
      _logger.LogError($"Key {nv} does not exist!");
      return null;
    }

    string valueAsString = value.ToString();

    if (string.IsNullOrEmpty(valueAsString)) {
      _logger.LogError($"{nv} == \"\"");
      return null;
    }

    return valueAsString;
  }

  /// <summary>
  /// Calculate required Set Api Hash
  /// </summary>
  /// <returns></returns>

  private async Task<string?> CalculateAdAsync()
  {
    var json =
      await _zteHttpClient.ApiGetAsJsonAsync("wa_inner_version,cr_version,RD");

    if (json is null) {
      return null;
    }

    Nv? nv = null;
    bool error = false;

    try {
      nv = JsonSerializer.Deserialize<Nv>(json);
    } catch {
      error = true;
    }

    if (error || nv is null) {
      _logger.LogError($"Could not deserialize Nv");
      return null;
    }

    return ZteSha256(ZteSha256(nv.WaInnerVersion + nv.CrVersion) + nv.RD);
  }

  /// <summary>
  /// Builds a set request with the specified goFormId and AD hash.
  /// </summary>
  /// <param name="goFormId">The goFormId for the request.</param>
  /// <returns>
  /// A dictionary containing the request parameters, or null if AD calculation fails.
  /// </returns>

  private async Task<Dictionary<string, string>?> BuildSetRequest(string goFormId)
  {
    string? ad = await CalculateAdAsync();

    if (ad is null) {
      return null;
    }

    return new Dictionary<string, string> {
      { "goformId", goFormId },
      { "AD", ad }
    };
  }

  /// <summary>
  /// Calculates the login hash.
  /// Algorithm: ZTE_SHA256(ZTE_SHA256("<PW>") + LD)
  /// </summary>
  /// <returns>
  /// The calculated login hash if successful; otherwise, null.
  /// </returns>

  private async Task<string?> CalculateLoginHashAsync()
  {
    string? nvLd = await GetNvAsync("LD");

    if (nvLd is null) {
      return null;
    }

    string? hashRouterPassword = ZteSha256(_routerPassword);

    if (hashRouterPassword is null) {
      return null;
    }

    string? loginHash = ZteSha256(hashRouterPassword + nvLd);

    if (hashRouterPassword is null) {
      return null;
    }

    return loginHash;
  }

  public async Task<int?> LoginAsync()
  {
    string? loginHash = await CalculateLoginHashAsync();

    if (loginHash is null) {
      _logger.LogError($"CalculateLoginHash() failed");
      return null;
    }

    string? resultJson = await _zteHttpClient
      .ApiSetAsJsonAsync(new Dictionary<string, string> {
        { "goformId", "LOGIN" },
        { "password", loginHash }
      });

    if (resultJson is null) {
      return null;
    }

    LoginResult? loginResult;

    try {
      loginResult = JsonSerializer.Deserialize<LoginResult>(resultJson);
      if (loginResult is null) {
        _logger.LogError($"Could not deserialize Json: {resultJson}");
        return null;
      }
      loginResult.ParseResult();
    } catch (Exception ex) {
      _logger.LogError($"Could not deserialize Json: {ex}");
      return null;
    }

    if (!loginResult.LoginSuccess()) {
      string loginErrorString = loginResult.GetErrorString();
      _logger.LogError($"Login failed: {loginErrorString}");
      return loginResult.Code;
    }

    _loggedIn = true;
    _successfulLogins++;

    return (int)LoginResult.LoginErrorCode.LOGIN_OK;
  }

  public async Task<bool> CheckLogin()
  {
    while (!_loggedIn) {
      // For debugging purposes
      if (_successfulLogins >= 1) {
        _logger.LogInformation("Waiting 20 seconds before the next login attempt ...");
        Thread.Sleep(20000);
      }

      int? loginResult;

      if ((loginResult = await LoginAsync()) is not null) {
        // Do not re-attempt if the password is wrong
        if (loginResult == (int)LoginResult.LoginErrorCode.LOGIN_ERROR_WRONG_PASSWORD) {
          _logger.LogError("Wrong Router Password!");
          Console.ReadKey();
          return false;
        }
        break;
      }

      Thread.Sleep(1000);
    }

    return true;
  }

  private DateTime lastAutoLogoutRequestTime = DateTime.MinValue;
  private readonly TimeSpan autoLogoutInterval = TimeSpan.FromMinutes(1);

  public async Task PreventAutoLogoutAsync()
  {
    DateTime currentTime = DateTime.Now;

    if (currentTime - lastAutoLogoutRequestTime >= autoLogoutInterval) {
      _ = await _zteHttpClient.ApiRequestAsync("tmpl/network/apn_setting.html");
      lastAutoLogoutRequestTime = currentTime;
    }
  }

  public async Task<bool> SetNrBandsAsync(string bands = "auto")
  {
    bands = bands.Replace('+', ',');

    var setRequest = await BuildSetRequest("WAN_PERFORM_NR5G_BAND_LOCK");

    if (setRequest is null) {
      return false;
    }

    if (bands == "auto") {
      bands = "1,2,3,5,7,8,20,28,38,41,50,51,66,70,71,74,75,76,77,78,79,80,81,82,83,84";
    }

    setRequest.Add("nr5g_band_mask", bands);

    return await _zteHttpClient.ApiSetAsync(setRequest);
  }

  public async Task<string?> GetSetNrBandsAsync()
  {
    return await _zteHttpClient.ApiGetAsJsonAsync("nr5g_sa_band_lock");
  }

  public async Task<bool> PerformNrBandHop(string bands1, string bands2)
  {
    return await SetNrBandsAsync(bands1) && await SetNrBandsAsync(bands2);
  }

  public async Task<bool> UpdateDeviceInfoAsync()
  {
    string? json = await _zteHttpClient
      .ApiGetAsJsonAsync(new Dictionary<string, string> { { "cmd", DEVICE_INFO_REQUEST } });

    if (json is null) {
      return false;
    }

    DeviceInfo? DeviceInfoNew;

    try {
      DeviceInfoNew = JsonSerializer.Deserialize<DeviceInfo>(json);
    } catch (Exception ex) {
      _logger.LogError($"Exception: {ex}");
      return false;
    }

    if (DeviceInfoNew is null) {
      return false;
    }

    DeviceInfo = DeviceInfoNew;

    SignalInfo.Update(DeviceInfo);

    // FIXME move
    // Assume we are logged-out if this value is not set
    if (string.IsNullOrEmpty(DeviceInfo.loginfo)) {
      _loggedIn = false;
      return false;
    }

    return true;
  }
};