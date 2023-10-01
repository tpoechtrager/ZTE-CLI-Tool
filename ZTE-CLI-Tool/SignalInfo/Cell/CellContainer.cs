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

public class CellContainer<T> where T : Cell
{
  public List<T> Cells { get; private set; } = new();
  private List<CellIdentifier> _nonOrphanedCells = new();

  public T? GetCell(Value<int> pci, Value<int> freq)
  {
    int index = Cells.FindIndex(c => (c as Cell)!.Pci == pci &&
                                     (c as Cell)!.Freq == freq);

    if (index == -1) {
      return default;
    }

    T cell = Cells[index];
    _nonOrphanedCells.Add(cell);
    return cell;
  }

  public void AddOrUpdateCell(T cell)
  {
    int index = Cells.FindIndex(c => (c as Cell)!.Pci == (cell as Cell)!.Pci &&
                                     (c as Cell)!.Freq == (cell as Cell)!.Freq);

    _nonOrphanedCells.Add(cell);

    if (index != -1) {
      Cells[index] = cell;
      return;
    }

    Cells.Add(cell);
  }

  // Removes orphaned cells
  public void RemoveOrphanedCells()
  {
    for (int i = Cells.Count - 1; i >= 0; i--) {
      var cell = Cells[i];
      if (!_nonOrphanedCells.Contains(cell)) {
        Cells.Remove(cell);
      }
    }

    _nonOrphanedCells.Clear();
  }
}