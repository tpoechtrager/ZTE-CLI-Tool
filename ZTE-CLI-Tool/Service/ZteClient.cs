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
using Newtonsoft.Json.Linq;
using ZTE_Cli_Tool.DTO;
using ZTE_Cli_Tool.Service.Interface;

namespace ZTE_Cli_Tool.Service;

public class ZteClient : IZteClient, IDisposable
{
  private readonly ILogger<ZteClient> _logger;
  private readonly IZteHttpClient _zteHttpClient;

  private string _routerPassword = "";

  private bool _useNewApi = false;
  private Func<string, string> _hash = Tools.ZteMd5;

  private bool _loggedIn = false;
  private int _successfulLogins = 0;

  public DeviceInfo DeviceInfo { get; private set; } = new();

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

    var json = await _zteHttpClient.ApiGetAsJsonAsync("wa_inner_version");

    if (json is null) {
      return;
    }

    var waInnerVersion = Tools.Deserializer<WaInnerVersion>.Deserialize(json, out _);

    if (waInnerVersion is null) {
      return;
    }

    string[] devicesWithNewApi = new[] {
      "MC888", "MC889"
    };

    foreach (var device in devicesWithNewApi) {
      if (waInnerVersion.Version.Contains(device, StringComparison.OrdinalIgnoreCase)) {
        _useNewApi = true;
        _hash = Tools.ZteSha256;
        break;
      }
    }
  }

  public void Dispose()
  {
    _zteHttpClient.Dispose(); // Needed?
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

    var nv = Tools.Deserializer<Nv>.Deserialize(json, out string errorMessage);

    if (nv is null) {
      _logger.LogError($"Could not deserialize Nv: {errorMessage}");
      return null;
    }

    return _hash(_hash(nv.WaInnerVersion + nv.CrVersion) + nv.RD);
  }

  /// <summary>
  /// Builds a set request with the specified goFormId and AD hash.
  /// </summary>
  /// <param name="goFormId">The goFormId for the request.</param>
  /// <param name="skipAd">
  /// Set to true to skip AD calculation; otherwise, 
  /// AD will be calculated and included in the request.</param>
  /// <returns>
  /// A dictionary containing the request parameters, or null if AD calculation fails.
  /// </returns>

  private async Task<Dictionary<string, string>?> BuildSetRequest(string goFormId, bool skipAd = false)
  {
    var setRequest = new Dictionary<string, string> {
      { "goformId", goFormId }
    };

    if (!skipAd) {
      string? ad = await CalculateAdAsync();

      if (ad is null) {
        return null;
      }

      setRequest.Add("AD", ad);
    }

    return setRequest;
  }

  /// <summary>
  /// Calculates the login hash.
  /// Algorithm: SHA256(SHA256("<PW>") + LD)
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

    string? hashRouterPassword = Tools.ZteSha256(_routerPassword);

    if (hashRouterPassword is null) {
      return null;
    }

    string? loginHash = Tools.ZteSha256(hashRouterPassword + nvLd);

    if (hashRouterPassword is null) {
      return null;
    }

    return loginHash;
  }

  private async Task<int?> LoginHelperAsync(bool developerLogin = false)
  {
    string? loginHash = await CalculateLoginHashAsync();

    if (loginHash is null) {
      _logger.LogError($"CalculateLoginHash() failed");
      return null;
    }

    var setRequest = await BuildSetRequest(
      developerLogin ? "DEVELOPER_OPTION_LOGIN" : "LOGIN", !developerLogin);

    if (setRequest is null) {
      return null;
    }

    setRequest.Add("password", loginHash);

    string? resultJson = await _zteHttpClient.ApiSetAsJsonAsync(setRequest);

    if (resultJson is null) {
      return null;
    }

    var loginResult = Tools.Deserializer<LoginResult>.Deserialize(resultJson, out string errorMessage);

    if (loginResult is null) {
      _logger.LogError($"Could not deserialize Json: {errorMessage}");
      return null;
    }

    if (!loginResult.ParseResult()) {
      _logger.LogError($"Could not parse login result: {loginResult.CodeAsString}");
      return null;
    }

    if (!loginResult.LoginSuccess()) {
      string loginErrorString = loginResult.GetErrorString();
      _logger.LogError("Login {0}failed: {1}", developerLogin ? "as developer " : "", loginErrorString);
      return loginResult.Code;
    }

    _loggedIn = true;

    _successfulLogins++;

    return (int)LoginResult.LoginErrorCode.LOGIN_OK;
  }

  public async Task<int?> LoginAsync()
  {
    int? result = await LoginHelperAsync();

    if (_useNewApi && result is not null &&
      result == (int)LoginResult.LoginErrorCode.LOGIN_OK) {
      // Log in as developer. Needed for certain settings.
      await LoginHelperAsync(true);
    }

    return result;
  }

  public async Task<bool> CheckLoginAsync()
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

  public async Task<bool> ConnectAsync(bool disconnect = false)
  {
    var setRequest = await BuildSetRequest(disconnect ? "DISCONNECT_NETWORK" : "CONNECT_NETWORK");

    if (setRequest is null) {
      return false;
    }

    setRequest.Add("notCallback", "true");

    return await _zteHttpClient.ApiSetAsJsonAsync(setRequest) is not null;
  }

  public async Task<bool> DisconnectAsync()
  {

    return await ConnectAsync(false);
  }


  private static class GetBandLockHelper<T>
  {
    public static async Task<string?> GetBandLockAsString(
      ILogger logger, IZteHttpClient zteHttpClient, string request)
    {
      var bandsJson = await zteHttpClient.ApiGetAsJsonAsync(request);

      if (bandsJson is null) {
        return null;
      }

      dynamic? bandLock = Tools.Deserializer<T>.Deserialize(bandsJson, out string errorMessage);

      if (bandLock is null) {
        logger.LogError($"Could not deserialize band lock Json: {errorMessage}");
        return null;
      }

      return ((string)bandLock.Bands);
    }

    public static async Task<IEnumerable<int>?> GetSetBandsAsListAsync(
      ILogger logger, IZteHttpClient zteHttpClient, string request)
    {
      var bands = await GetBandLockAsString(logger, zteHttpClient, request);

      if (bands is null) {
        return null;
      }

      bands = bands.ToLower();

      if (bands.StartsWith("0x")) {
        // Band mask
        Int64? bandMask = Tools.ParseHexAsInt64(bands);

        if (bandMask is null) {
          return null;
        }

        List<int> bandList = new();

        for (int i = 0; i < 64; i++) {
          if ((bandMask & (1L << i)) != 0) {
            bandList.Add(i + 1);
          }
        }

        return bandList;
      } else {
        // Band string (1,2,3)
        return bands.Split(',').Select(str => Tools.ParseInt(Tools.RemoveNonNumericCharacters(str), -1));
      }
    }
  }

  public async Task<bool> SetNrBandLockAsync(IEnumerable<int>? bands)
  {
    var setRequest = await BuildSetRequest("WAN_PERFORM_NR5G_BAND_LOCK");

    if (setRequest is null) {
      return false;
    }

    if (bands is null) {
      string defaultBandList = "1,2,3,5,7,8,20,28,38,41,50,51,66,70,71,74,75,76,77,78,79,80,81,82,83,84";
      bands = Tools.ConvertBandsToList(defaultBandList, ',');
    }

    setRequest.Add("nr5g_band_mask", string.Join(',', bands));

    return await _zteHttpClient.ApiSetAsync(setRequest);
  }

  public async Task<IEnumerable<int>?> GetNrBandLockAsync()
  {
    return await GetBandLockHelper<SaBandLock>.GetSetBandsAsListAsync(_logger, _zteHttpClient, "nr5g_sa_band_lock");
  }

  public async Task<bool> PerformNrBandHopAsync(string bands1, string bands2)
  {
    return await SetNrBandLockAsync(Tools.ConvertBandsToList(bands1)) &&
           await SetNrBandLockAsync(Tools.ConvertBandsToList(bands2));
  }


  public async Task<IEnumerable<int>?> GetLteBandLockAsync()
  {
    return await GetBandLockHelper<LteBandLock>.GetSetBandsAsListAsync(_logger, _zteHttpClient, "lte_band_lock");
  }

  public async Task<bool> SetLteBandLockAsync(IEnumerable<int>? bands)
  {
    var setRequest = await BuildSetRequest("BAND_SELECT");

    if (setRequest is null) {
      return false;
    }

    Int64 bandMask = 0;

    if (bands is not null) {
      foreach (int band in bands) {
        bandMask |= (1L << (band - 1));
      }
    } else {
      bandMask = Tools.ParseHexAsInt64("0xA3E2AB0908DF")!.Value;
    }

    setRequest.Add("is_gw_band", "0");
    setRequest.Add("gw_band_mask", "0");
    setRequest.Add("is_lte_band", "1");
    setRequest.Add("lte_band_mask", "0x" + bandMask.ToString("X").PadLeft(11, '0'));

    return await _zteHttpClient.ApiSetAsync(setRequest);
  }


  // This is only used for setting/getting the network preference.

  private readonly Dictionary<string, string> _networkPreference = new() {
    {"2G", "Only_GSM"},
    {"2G+3G+4G", "GSM_WCDMA_LTE"},
    {"2G+4G", "GSM_AND_LTE"},
    {"3G", "Only_WCDMA"},
    {"3G+2G", "WCDMA_AND_GSM"},
    {"3G+4G", "WCDMA_AND_LTE"},
    {"3G(TDSCDMA)+4G", "TDSCDMA_AND_LTE"},
    {"4G", "Only_LTE"},
    {"4G+5G", "LTE_AND_5G"},
    {"5G", "Only_5G"},
    {"CDMA+EVDO+4G", "CDMA_EVDO_LTE"},
    {"GWL+5G", "GWL_5G"},
    {"TD-SCDMA+WCDMA+2G+4G", "TDSCDMA_WCDMA_GSM_LTE"},
    {"TD-SCDMA+WCDMA+HDR+CDMA+2G+4G", "TDSCDMA_WCDMA_HDR_CDMA_GSM_LTE"},
    {"TDSCDMA+WCDMA", "TDSCDMA_AND_WCDMA"},
    {"3G_preferred", "WCDMA_preferred"},
    {"TCHGWL+5G", "TCHGWL_5G"},
    {"TGWL+5G", "TGWL_AND_5G"},
    {"WL+5G", "WL_AND_5G"},

    {"LTE", "Only_LTE"},
    {"NSA", "Only_5G"},
    {"SA", "Only_5G"}
  };

  private string? GetNetworkPreferenceValue(string mode, bool reverse = false)
  {
    if (!reverse) {
      // 2G -> Only_GSM
      // Split the input mode by '+' and convert to uppercase,
      // then sort the parts to check permutations
      string[] modeParts = mode.ToUpper().Split('+').OrderBy(part => part).ToArray();

      foreach (var entry in _networkPreference) {
        string[] entryParts = entry.Key.Split('+').OrderBy(part => part).ToArray();

        // Check if the sorted parts of the input mode match the sorted parts of the dictionary key
        if (modeParts.SequenceEqual(entryParts)) {
          return entry.Value;
        }
      }
    } else {
      // Only_GSM -> 2G
      var val = _networkPreference.FirstOrDefault(np => np.Value == mode);

      if (val.Key is not null) {
        return val.Key;
      }
    }

    return null;
  }

  public async Task<string?> GetNetworkModePreference()
  {
    var json = await _zteHttpClient.ApiGetAsJsonAsync("net_select");

    if (json is null) {
      return null;
    }

    var netSelect = Tools.Deserializer<NetSelect>.Deserialize(json, out var errorMessage);

    if (netSelect is null) {
      _logger.LogError($"Could not deserialize Json: {errorMessage}");
      return null;
    }

    var netModePreferance = GetNetworkPreferenceValue(netSelect.Mode, true);

    if (netModePreferance is null) {
      netModePreferance = netSelect.Mode;
    }

    return netModePreferance;
  }

  public async Task<bool> SetNetworkPreferenceAsync(string mode)
  {
    var networkMode = GetNetworkPreferenceValue(mode);

    if (networkMode is null) {
      _logger.LogError($"Invalid network mode {mode}");

      Console.WriteLine("Available modes:" + Environment.NewLine);

      foreach (var entry in _networkPreference) {
        Console.WriteLine(" " + entry.Key);
      }

      Console.WriteLine("");

      return false;
    }

    var setRequest = await BuildSetRequest("SET_BEARER_PREFERENCE");

    if (setRequest is null) {
      return false;
    }

    setRequest.Add("BearerPreference", networkMode);

    return await _zteHttpClient.ApiSetAsync(setRequest);
  }

  public async Task<bool> UpdateDeviceInfoAsync()
  {
    string? json = await _zteHttpClient
      .ApiGetAsJsonAsync(new Dictionary<string, string> { { "cmd", DEVICE_INFO_REQUEST } });

    if (json is null) {
      return false;
    }

    var deviceInfoNew = Tools.Deserializer<DeviceInfo>.Deserialize(json, out string errorMessage);

    if (deviceInfoNew is null) {
      _logger.LogError($"Could not deserialize Json: {errorMessage}");
      return false;
    }

    if (deviceInfoNew is null) {
      return false;
    }

    DeviceInfo = deviceInfoNew;

    if (string.IsNullOrEmpty(DeviceInfo.loginfo)) {
      _loggedIn = false;
      return false;
    }

    return true;
  }
};
