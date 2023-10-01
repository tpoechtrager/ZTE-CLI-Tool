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

using ZTE_Cli_Tool.DTO;

namespace ZTE_Cli_Tool;

public class NetworkType
{
  public enum Type
  {
    UNKNOWN,
    UMTS,
    LTE,
    LTE_PLUS,
    NR_NSA_ACTIVE,
    NR_NSA_PASSIVE, // NR available but only LTE receipton
    NR_SA
  };

  private static readonly string[] TYPE_NAMES = new string[] {
    "Unknown",
    "UMTS",
    "LTE",
    "LTE+",
    "NR-NSA",
    "NR-NSA (LTE-only)",
    "NR-SA"
  };

  private static readonly HashSet<string> UMTS_NAMES = new() {
    "HSPA", "HSDPA", "HSUPA", "HSPA+", "DC-HSPA+", "UMTS", "CDMA",
    "CDMA_EVDO", "EVDO_EHRPD", "TDSCDMA"
  };

  private static readonly HashSet<string> LTE_NAMES = new() {
    "LTE"
  };

  private static readonly HashSet<string> NR_NSA_NAMES = new() {
    "ENDC", "EN-DC", "LTE-NSA"
  };

  private static readonly HashSet<string> NR_SA_NAMES = new() {
    "SA"
  };

  private Type _type = Type.UNKNOWN;

  public string AsString => TYPE_NAMES[(int)_type];

  public bool IsUmts =>
    _type == Type.UMTS;

  public bool IsLte =>
    _type == Type.LTE || _type == Type.LTE_PLUS || IsNrNsa;

  public bool IsNr =>
  IsNrNsa || IsNrSa;

  public bool IsNrNsa =>
    _type == Type.NR_NSA_PASSIVE || _type == Type.NR_NSA_ACTIVE;

  public bool IsNrNsaActive =>
    _type == Type.NR_NSA_ACTIVE;

  public bool IsNrSa =>
    _type == Type.NR_SA;


  public void Update(DeviceInfo deviceInfo)
  {
    var network_type = deviceInfo.NetworkType;

    if (UMTS_NAMES.Contains(network_type)) {
      _type = Type.UMTS;
      return;
    }

    if (LTE_NAMES.Contains(network_type)) {
      if (!string.IsNullOrEmpty(deviceInfo.WanLteCa) &&
        (deviceInfo.WanLteCa == "ca_activated" || deviceInfo.WanLteCa == "ca_deactivated")) {
        _type = Type.LTE_PLUS;
      } else {
        _type = Type.LTE;
      }

      return;
    }

    if (NR_NSA_NAMES.Contains(network_type)) {
      if (network_type == "LTE-NSA") {
        _type = Type.NR_NSA_PASSIVE;
      } else {
        _type = Type.NR_NSA_ACTIVE;
      }

      return;
    }

    if (NR_SA_NAMES.Contains(network_type)) {
      _type = Type.NR_SA;
      return;
    }

    _type = Type.UNKNOWN;
  }
};