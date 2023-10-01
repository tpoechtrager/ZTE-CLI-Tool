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

public class Cell : CellIdentifier
{
  public Value<int> Band = new();
  public Value<float> Bandwidth = new();
  public Value<bool> Scell = new();

  public bool IsPrimaryCell => Scell == false;
  public bool IsSecondaryCell => Scell == false;

  public static bool operator ==(Cell a, Cell b)
  {
    return a.Pci == b.Pci && a.Freq == b.Freq;
  }

  public static bool operator !=(Cell a, Cell b)
  {
    return !(a == b);
  }

  public override int GetHashCode()
  {
    return Pci.GetHashCode() ^ Freq.GetHashCode();
  }

  public override bool Equals(object? inVal)
  {
    if (inVal is null) {
      return false;
    }

    if (inVal is Cell inCell) {
      return Pci == inCell.Pci && Freq == inCell.Freq;
    }

    return false;
  }
}