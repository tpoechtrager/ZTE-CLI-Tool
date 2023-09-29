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

using System.Text.Json;
using System.Text.RegularExpressions;

namespace ZTE_Cli_Tool;

public static class Tools
{
  public static bool IsNumber(string str)
  {
    return float.TryParse(str, out _);
  }

  public static int? ParseInt(string str)
  {
    if (int.TryParse(str, out int val)) {
      return val;
    }

    return null;
  }

  public static int ParseInt(string str, int errorValue)
  {
    if (int.TryParse(str, out int val)) {
      return val;
    }

    return errorValue;
  }

  public static int? ParseHexAsInt(string str)
  {
    try {
      return Convert.ToInt32(str, 16);
    } catch (Exception) {
      return null;
    }
  }

  public static Int64? ParseHexAsInt64(string str)
  {
    try {
      return Convert.ToInt64(str, 16);
    } catch (Exception) {
      return null;
    }
  }

  public static void RemoveCharacterFromString(ref string str, char ch)
  {
    int index;
    while ((index = str.IndexOf(ch)) != -1) {
      str = str.Remove(index, 1);
    }
  }

  public static string RemoveNonNumericCharacters(string str)
  {
    // Find the first numeric number in the string
    Match match = Regex.Match(str, @"-?\d+(\.\d+)?");

    if (match.Success) {
      return match.Value;
    }

    return string.Empty;
  }

  public static string OneOf(params string[] values)
  {
    foreach (string value in values) {
      if (!string.IsNullOrEmpty(value)) {
        return value;
      }
    }

    return string.Empty;
  }

  public static class Deserializer<T>
  {
    public static T? Deserialize(string json, out string errorMessage)
    {
      try {
        var obj = JsonSerializer.Deserialize<T>(json);

        if (obj is null) {
          errorMessage = "JsonSerializer.Deserialize() returned null";
        } else {
          errorMessage = String.Empty;
        }

        return obj;
      } catch (Exception e) {
        errorMessage = e.Message;
        return default;
      }
    }
  }

  public static IEnumerable<int> ConvertBandsToList(string bands, char separator = '+')
  {
    List<int> bandList = new();

    foreach (string b in bands.Split(separator)) {
      int band = Tools.ParseInt(Tools.RemoveNonNumericCharacters(b), -1);
      if (band != -1) {
        bandList.Add(band);
      }
    }

    return bandList;
  }
}
