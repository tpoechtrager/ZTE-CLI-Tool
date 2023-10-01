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
  public string Find;
  public string Replacement;

  public ValueReplace(string find, string replacement)
  {
    Find = find;
    Replacement = replacement;
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
  public int Updates { get; private set; } = 0;
  private T _val = default!;

  public T Get() => _val;
  public T Val => _val;
  public bool Ok => Updates > 0;

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
    return a._val.Equals(b);
  }

  public static bool operator !=(Value<T> a, object b)
  {
    return !(a == b);
  }

  public static bool operator ==(Value<T> a, Value<T> b)
  {
    return a._val.Equals(b._val);
  }

  public static bool operator !=(Value<T> a, Value<T> b)
  {
    return !(a == b);
  }

  public override bool Equals(object? inVal)
  {
    if (inVal is null || _val.GetType() != inVal.GetType()) {
      return false;
    }

    return _val.Equals(inVal);
  }

  public override int GetHashCode()
  {
    return _val.GetHashCode();
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
          replaceValue.Find, replaceValue.Replacement);
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

    if (_val is float) {
      if (float.TryParse(strValue, out var floatValue)) {
        _val = (T)(object)floatValue;
        success = true;
      }
    } else if (_val is int) {
      var result =
        haveFlag(SetFlag.InputIsHex)
          ? Tools.ParseHexAsInt(strValue)
          : Tools.ParseInt(strValue);
      if (result is not null) {
        _val = (T)(object)result;
        success = true;
      }
    } else if (strValue is T sVal) {
      _val = sVal;
      success = true;
    } else {
      throw new NotImplementedException();
    }

    if (success) {
      Updates++;
      return true;
    }

    return false;
  }

  public void Set(bool inVal)
  {
    if (inVal is T v) {
      _val = v;
      Updates++;
    } else {
      throw new InvalidDataException();
    }
  }
}