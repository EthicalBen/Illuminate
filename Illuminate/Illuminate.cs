﻿namespace Illuminate {
	using System;
	using System.Threading.Tasks;

#if DEBUG
	using static Debug;
#endif

	internal class Illuminate {
        private static async Task<int> Main(string[] args) {
            //---[Entrypoint]---
#if DEBUG
            _("DEBUG Build!");
            await AsyncBreakfast.AsyncBreakfast.Main(args);
#endif

            //... [do work] ...


            //---[Exitpoint]---
            while(Console.KeyAvailable) Console.ReadKey(true);
            Console.WriteLine("\n[Press any key to exit...]");
            Console.ReadKey(true);
            return 0;
        }
    }
}