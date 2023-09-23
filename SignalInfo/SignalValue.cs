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

public class SignalValue<T> where T : struct, IConvertible
{
  private const int MAX_HISTORY = 100;

  public int updates => val.updates;

  private Value<T> val = new();
  private T min;
  private T max;

  private int historyOldestIndex = 0;
  private List<T> history = new(MAX_HISTORY);

  public bool Ok => updates > 0;
  public T Current => val.Get();
  public double Average => CalculateAverage();
  public T Min => min;
  public T Max => max;

  private double CalculateAverage()
  {
    if (!history.Any()) {
      return 0.0;
    }

    double sum = 0.0;

    foreach (T value in history) {
      sum += Convert.ToDouble(value);
    }

    return sum / history.Count;
  }

  public bool Update(string strValue, params object[] options)
  {
    if (!val.Set(strValue, options)) {
      return false;
    }

    // This should be more performant than using RemoveAt(0)
    if (history.Count >= MAX_HISTORY) {
      // Overwrite the oldest value
      history[historyOldestIndex] = val.Get();
      // Update the oldest index
      historyOldestIndex = (historyOldestIndex + 1) % MAX_HISTORY;
    } else {
      history.Add(val.Get());
    }

    if (updates == 1 || val > max) {
      max = val.Get();
    }

    if (updates == 1 || val < min) {
      min = val.Get();
    }

    return true;
  }
}
