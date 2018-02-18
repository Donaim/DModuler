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
                if(dm.LoadLibrary(args[0], out var re, false, true, true )) {
                    Console.WriteLine("Everything went ok");
                } else {
                    Error.WriteLine($"ERROR: {re.Err.Message}");
                }

                var log = re.GetLog();
                if(log.Count() > 0) {
                    Error.WriteLine($"LOGS:");
                    foreach(var l in re.GetLog()) {
                        Error.WriteLine($"{l.Message}");
                    }
                }
            }
            else { throw new Exception("Expected assembly name in arguments!"); }
        }
    }
}