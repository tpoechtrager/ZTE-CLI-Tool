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

using System.Text.Json.Serialization;

namespace ZTE_Cli_Tool.DTO;

public class LoginResult
{
  [JsonPropertyName("result")]
  public string CodeAsString { get; set; } = string.Empty;
  public int Code { get; private set; } = -1;

  public bool ParseResult()
  {
    if (CodeAsString == "failure") {
      Code = (int)LoginErrorCode.LOGIN_FAILURE;
      return true;
    }

    var parsedCode = Tools.ParseInt(CodeAsString);

    if (parsedCode is null) {
      return false;
    }

    Code = parsedCode.Value;
    return true;
  }

  public bool LoginSuccess()
  {
    return Code == (int)LoginErrorCode.LOGIN_OK;
  }

  public string GetErrorString()
  {
    if (Code < 0 || Code >= LOGIN_ERROR_STRINGS.Length) {
      return string.Format("Unknown error code: {0}", Code);
    }

    return LOGIN_ERROR_STRINGS[Code];
  }

  public enum LoginErrorCode
  {
    LOGIN_OK,
    LOGIN_ERROR_TRY_AGAIN_LATER,
    LOGIN_ERROR_DUPLICATE_USER,
    LOGIN_ERROR_WRONG_PASSWORD,
    LOGIN_FAILURE
  }

  private readonly string[] LOGIN_ERROR_STRINGS = new string[] {
    "Login Success",
    "Try again later",
    "Duplicate User",
    "Wrong Password",
    "Login Failure"
  };
}
