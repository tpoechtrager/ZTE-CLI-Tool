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

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace ZTE_Cli_Tool;

public static class Tools
{
  /// <summary>
  /// Computes and returns an upper-cased SHA256 hash of the input text.
  /// </summary>
  /// <param name="text">The text to hash.</param>
  /// <returns>
  /// The upper-cased SHA256 hash as a string.
  /// </returns>

  public static string ZteSha256(string text)
  {
    byte[] result = SHA256.HashData(Encoding.UTF8.GetBytes(text));
    return Convert.ToHexString(result).ToUpper();
  }

  /// <summary>
  /// Computes and returns an upper-cased MD5 hash of the input text.
  /// </summary>
  /// <param name="text">The text to hash.</param>
  /// <returns>
  /// The upper-cased MD5 hash as a string.
  /// </returns>

  public static string ZteMd5(string text)
  {
    byte[] result = MD5.HashData(Encoding.UTF8.GetBytes(text));
    return Convert.ToHexString(result).ToUpper();
  }

  public static bool IsNumber(string str)
  {
    return double.TryParse(str, out _);
  }

  public static int? ParseInt(string str)
  {
    if (int.TryParse(str, out int val)) {
      return val;
    }

    return null;
  }

  public static Int64 ParseInt64(string str, Int64 errorValue)
  {
    if (Int64.TryParse(str, out Int64 val)) {
      return val;
    }

    return errorValue;
  }


  public static Int64? ParseInt64(string str)
  {
    if (Int64.TryParse(str, out Int64 val)) {
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

  public static string CalculateDate(int secondsToSubtract)
  {
    try {
      // Get the current date and time
      DateTime currentDate = DateTime.Now;

      // Subtract the specified number of seconds
      DateTime calculatedDate = currentDate.AddSeconds(-secondsToSubtract);

      // Convert the calculated date to a string
      string dateStr = calculatedDate.ToString("yyyy-MM-dd HH:mm:ss");

      return dateStr;
    } catch {
      return "?";
    }
  }

  public static string FormatBytes(Int64 bytes, int decimals = 2)
  {
    string[] suffixes = { "Bytes", "KB", "MB", "GB", "TB", "PB" };
    int index = 0;
    double size = bytes;

    while (size >= 1024 && index < suffixes.Length - 1) {
      size /= 1024;
      index++;
    }

    if (index >= suffixes.Length) {
      return "Input is too large to format.";
    }

    return String.Format("{0:F" + decimals + "} {1}", size, suffixes[index]);
  }

  public static string FormatBytes(string bytesStr, int decimals = 2)
  => FormatBytes(ParseInt64(bytesStr, -1));

  public static double BytesToMbits(Int64 bytes)
  {
    // Convert bytes to bits
    Int64 bits = bytes * 8;
    // Convert bits to megabits
    return bits / (1024.0 * 1024.0);
  }

  public static double BytesToMbits(string bytesStr)
    => BytesToMbits(ParseInt64(bytesStr, -1));
}
