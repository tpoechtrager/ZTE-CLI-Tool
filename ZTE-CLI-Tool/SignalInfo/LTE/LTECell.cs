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

namespace ZTE_Cli_Tool;

public class LteCell : Cell
{
  public Value<int> Earfcn => Freq;

  public SignalValue<int> Rssi { get; set; } = new();
  public SignalValue<float> Rsrp1 { get; set; } = new();
  public SignalValue<float> Rsrp2 { get; set; } = new();
  public SignalValue<float> Rsrp3 { get; set; } = new();
  public SignalValue<float> Rsrp4 { get; set; } = new();
  public SignalValue<float> Rsrq { get; set; } = new();
  public SignalValue<float> Sinr1 { get; set; } = new();
  public SignalValue<float> Sinr2 { get; set; } = new();
  public SignalValue<float> Sinr3 { get; set; } = new();
  public SignalValue<float> Sinr4 { get; set; } = new();
};