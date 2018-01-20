using System;
using System.Collections.Generic;

namespace SharerSpace
{
    public class Sharer {
        private readonly Dictionary<string, ISharable> dict = new Dictionary<string, ISharable>();

        public Sharer() {

        }

        public IEnumerable<string> Names => dict.Keys;

        public void AddInterface(ISharable o) {
            try {
                dict.Add(o.Name, o);
            }
            catch (Exception ex) {
                throw new Exception($"Sharer already contains \"{o.Name}\" !", ex);
            }
        }
        public T GetInterface<T>(string name) => (T)dict[name];
        public bool TryGetInterface<T>(string name, out T face) {
            face = default(T);
            
            var get = dict.TryGetValue(name, out var o);
            if(!get) { return false; }

            if(o is T ot) { 
                face = ot;
                return true; 
            }
            else {
                // throw new Exception($"{name} is not of type {face.GetType().Name}");
                return false;
            }
        }
    }
}

