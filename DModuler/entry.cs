using System;
using System.Linq;
using static System.Console;

namespace DModulerSpace
{
    public static class Entry {
        public static void Main(string [] args) {
            if(args.Length > 0) 
            { 
                var dm = new DModuler(new SharerSpace.Sharer());
                if(dm.LoadLibrary(args[0], out var err, false, true, true )) {
                    Console.WriteLine("Everything went ok");
                } else {
                    Error.WriteLine($"ERROR: {err.Err.Message}");
                }

                var log = err.GetLog();
                if(log.Count() > 0) {
                    Error.WriteLine($"LOGS:");
                    foreach(var l in err.GetLog()) {
                        Error.WriteLine($"{l.Message}");
                    }
                }
            }
            else { throw new Exception("Expected assembly name in arguments!"); }
        }
    }
}