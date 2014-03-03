﻿/*
 *   Coinium - Crypto Currency Pool Software - https://github.com/CoiniumServ/coinium
 *   Copyright (C) 2013 - 2014, Coinium Project - http://www.coinium.org
 *
 *   This program is free software: you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation, either version 3 of the License, or
 *   (at your option) any later version.
 *
 *   This program is distributed in the hope that it will be useful,
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *   GNU General Public License for more details.
 *
 *   You should have received a copy of the GNU General Public License
 *   along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Globalization;
using System.Reflection;
using System.Threading;
using Coinium.Common.Console;
using Coinium.Common.Platform;
using Coinium.Core.Getwork;
using Coinium.Core.Stratum;
using Coinium.Core.Wallet;
using Coinium.Net.Http;
using Serilog;

namespace Coinium
{
    class Program
    {
        static void Main(string[] args)
        {
            #if !DEBUG
                AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionHandler; // Catch any unhandled exceptions.
            #endif

            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture; // Use invariant culture - we have to set it explicitly for every thread we create to prevent any file-reading problems (mostly because of number formats).

            Console.ForegroundColor = ConsoleColor.Yellow;
            ConsoleWindow.PrintBanner();
            ConsoleWindow.PrintLicense();
            Console.ResetColor();

            InitLogging();
            Log.Information("Coinium {0} warming-up..", Assembly.GetAssembly(typeof(Program)).GetName().Version);
            Log.Information(string.Format("Running over {0} {1}.", PlatformManager.Framework, PlatformManager.FrameworkVersion));

            // coind rpc client.
            var client = new WalletClient("http://127.0.0.1:9333", "devel", "develpass");
            client.GetInfo();

            // stratum server.
            var stratumServer = new StratumServer();
            stratumServer.Listen("0.0.0.0", 3333);

            // getwork server.
            var getworkServer = new GetworkServer(8332);
            getworkServer.Listen();


            // web-server.
            //var webServer = new WebServer(81);

            Console.ReadLine();
        }

        #region logging facility

        private static void InitLogging()
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.ColoredConsole()
                .WriteTo.RollingFile("Debug.log")
                .MinimumLevel.Verbose()
                .CreateLogger();
        }

        #endregion

        #region unhandled exception emitter

        /// <summary>
        /// Unhandled exception emitter.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject as Exception;

            if (exception == null) // if we can't get the exception, whine about it.
                throw new ArgumentNullException("e");

            Console.WriteLine(
                    e.IsTerminating ? "Terminating because of unhandled exception: {0}" : "Caught unhandled exception: {0}",
                    exception);

            Console.ReadLine();
        }

        #endregion
    }
}
