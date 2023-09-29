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

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ZTE_Cli_Tool.Service;
using ZTE_Cli_Tool.Service.Interface;

namespace ZTE_Cli_Tool.Program;

public static class Program
{
  public static async Task<int> Main(string[] args)
  {
    // Using DI

    var services = new ServiceCollection();

    services.AddLogging(builder => {
      builder.AddConsole();
    });

    services.AddSingleton<IZteHttpClient, ZteHttpClient>();
    services.AddSingleton<IZteClient, ZteClient>();
    services.AddSingleton<Command>();

    services.AddTransient<ZteCliTool>();

    var serviceProvider = services.BuildServiceProvider();

    var zteCliTool = serviceProvider.GetService<ZteCliTool>();
    int result = await zteCliTool!.ExecuteAsync(args);

    serviceProvider.Dispose();

    return result;
  }
}
