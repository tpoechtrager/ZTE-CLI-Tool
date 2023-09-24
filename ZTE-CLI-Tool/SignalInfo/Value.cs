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

namespace ZTE_Cli_Tool;

public enum SetFlag
{
  None = 0x00,
  InputIsHex = 0x01,
  StripNonNumericCharacters = 0x02
}

class ValueReplace
{
  public string find;
  public string replacement;

  public ValueReplace(string find, string replacement)
  {
    this.find = find;
    this.replacement = replacement;
  }
}

class ValueRemove
{
  public string[] strsToRemove;

  public ValueRemove(params string[] strs)
  {
    strsToRemove = strs;
  }
}

public class Value<T> where T : notnull
{
  public int updates { get; private set; } = 0;
  private T val = default!;

  public T Get() => val;
  public T Val => val;
  public bool Ok => updates > 0;

  public static bool operator >(Value<T> a, object b)
  {
    var valA = a.Get();

    Comparer<int>.Default.Compare(0, 1);

    if (valA is T value && (b is int || b is float)) {
      return Comparer<T>.Default.Compare(value, (T)b) > 0;
    }

    throw new NotImplementedException(
      $"Comparison not supported for types {typeof(T)} and {b?.GetType()}");
  }

  public static bool operator <(Value<T> a, object b)
  {
    var valA = a.Get();

    if (valA is T value && (b is int || b is float)) {
      return Comparer<T>.Default.Compare(value, (T)b) < 0;
    }

    throw new NotImplementedException(
      $"Comparison not supported for types {typeof(T)} and {b.GetType()}");
  }

  public static bool operator ==(Value<T> a, object b)
  {
    return a.val.Equals(b);
  }

  public static bool operator !=(Value<T> a, object b)
  {
    return !(a == b);
  }

  public static bool operator ==(Value<T> a, Value<T> b)
  {
    return a.val.Equals(b.val);
  }

  public static bool operator !=(Value<T> a, Value<T> b)
  {
    return !(a == b);
  }

  public override bool Equals(object? inVal)
  {
    if (inVal is null || val.GetType() != inVal.GetType()) {
      return false;
    }

    return val.Equals(inVal);
  }

  public override int GetHashCode()
  {
    return val.GetHashCode();
  }

  public bool Set(string strValue, params object[] options)
  {
    bool success = false;

    SetFlag flags = SetFlag.None;

    foreach (var option in options) {
      if (option is ValueRemove setRemove) {
        foreach (string strToRemove in setRemove.strsToRemove) {
          // Ensure not to break numbers
          if (Tools.IsNumber(strToRemove)) {
            if (strValue.StartsWith(strToRemove)) {
              strValue = strValue[..strToRemove.Length];
            } else {
              strValue = strValue.Replace(" " + strToRemove, string.Empty);
            }
          } else {
            strValue = strValue.Replace(strToRemove, string.Empty);
          }
        }
      } else if (option is ValueReplace replaceValue) {
        strValue = strValue.Replace(
          replaceValue.find, replaceValue.replacement);
      } else if (option is SetFlag setFlags) {
        flags |= setFlags;
      }
    }

    bool haveFlag(SetFlag flag)
    {
      return (flags & flag) != 0;
    }

    if (haveFlag(SetFlag.StripNonNumericCharacters)) {
      strValue = Tools.RemoveNonNumericCharacters(strValue);
    }

    if (val is float) {
      if (float.TryParse(strValue, out var floatValue)) {
        val = (T)(object)floatValue;
        success = true;
      }
    } else if (val is int) {
      var result =
        haveFlag(SetFlag.InputIsHex)
          ? Tools.ParseHexAsInt(strValue)
          : Tools.ParseInt(strValue);
      if (result is not null) {
        val = (T)(object)result;
        success = true;
      }
    } else if (strValue is T sVal) {
      val = sVal;
      success = true;
    } else {
      throw new NotImplementedException();
    }

    if (success) {
      updates++;
      return true;
    }

    return false;
  }

  public void Set(bool inVal)
  {
    if (inVal is T v) {
      val = v;
      updates++;
    } else {
      throw new InvalidDataException();
    }
  }
}