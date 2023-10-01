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

  public int Updates => _val.Updates;

  private readonly Value<T> _val = new();
  private T _min;
  private T _max;

  private int _historyOldestIndex = 0;
  private List<T> _history = new(MAX_HISTORY);

  public bool Ok => Updates > 0;
  public T Current => _val.Get();
  public double Average => CalculateAverage();
  public T Min => _min;
  public T Max => _max;

  private double CalculateAverage()
  {
    if (!_history.Any()) {
      return 0.0;
    }

    double sum = 0.0;

    foreach (T value in _history) {
      sum += Convert.ToDouble(value);
    }

    return sum / _history.Count;
  }

  public bool Update(string strValue, params object[] options)
  {
    if (!_val.Set(strValue, options)) {
      return false;
    }

    // This should be more performant than using RemoveAt(0)
    if (_history.Count >= MAX_HISTORY) {
      // Overwrite the oldest value
      _history[_historyOldestIndex] = _val.Get();
      // Update the oldest index
      _historyOldestIndex = (_historyOldestIndex + 1) % MAX_HISTORY;
    } else {
      _history.Add(_val.Get());
    }

    if (Updates == 1 || _val > _max) {
      _max = _val.Get();
    }

    if (Updates == 1 || _val < _min) {
      _min = _val.Get();
    }

    return true;
  }
}
