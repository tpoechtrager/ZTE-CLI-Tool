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

  public static int? ParseHexAsInt(string str)
  {
    try {
      return Convert.ToInt32(str, 16);
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
}
