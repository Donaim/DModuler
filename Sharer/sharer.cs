using System;
using System.Collections.Generic;

namespace Sharer
{
    public class Sharer {
        public Sharer() {

        }

        private readonly Dictionary<string, object> dict = new Dictionary<string, object>();

        public bool AddInterface<T>(T o) {
            return false;
        }
    }    
}

