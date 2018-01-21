using System;
using System.Collections.Generic;

namespace SharerSpace
{
    public class Sharer {
        private readonly Dictionary<string, Type> types = new Dictionary<string, Type>();
        private readonly Dictionary<string, ISharable> dict = new Dictionary<string, ISharable>();

        public Sharer() { }

        public IEnumerable<string> INames => dict.Keys;
        public IEnumerable<string> TNames => types.Keys;
        public bool ContainsI(string name) => dict.ContainsKey(name);
        public bool ContainsT(string name) => types.ContainsKey(name);

        public bool AddType(Type t) => AddType(t, t.Name);
        public bool AddType(Type t, string name) {
            if(t.GetInterface(nameof(ISharable)) == null) { return false; }
  
            try { types.Add(t.Name, t); return true; }
            catch { return false; }
        }
        public T CreateInterface<T>(string name) => CreateInterface<T>(types[name]);
        private T CreateInterface<T>(Type t) => (T) Activator.CreateInstance(t);
        public bool TryCreateInterface<T>(string name, out T o) {
            o = default(T);
            
            bool get = types.TryGetValue(name, out var ot);
            if(!get) { return false; }

            object instance;
            try { instance = Activator.CreateInstance(ot); }
            catch { return false; }

            if(instance is T converted) { 
                o = converted;
                return true; 
            }
            else { return false; }
        }

        public void AddInterface(ISharable o) {
            try { dict.Add(o.Name, o); }
            catch (Exception ex) {
                throw new Exception($"Sharer already contains \"{o.Name}\" !", ex);
            }
        }
        public bool TryAddInterface(ISharable o) {
            try {
                dict.Add(o.Name, o);
                return true;
            }
            catch { return false; }
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

