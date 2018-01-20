using System;
using System.Collections.Generic;

namespace DModulerSpace {
    public class OutputEx {
        Exception ex;
        public bool OK => ex == null;

        readonly Stack<Exception> log = new Stack<Exception>();
        public IEnumerable<Exception> GetLog() => log;
        public void Log(Exception ignored_error) => log.Push(ignored_error);
        public void Log(string message, Exception inner = null) => Log(new Exception(message, inner));

        public OutputEx Throw(Exception err) {
            ex = err;
            return this;
        }
        public OutputEx Throw(string message, Exception inner = null) => Throw(new Exception(message, inner));
        public OutputEx ThrowLast() {
            if(log.Count > 0) { Throw(log.Pop()); }
            return this;
        } 

        public OutputEx(Exception _ex) {
            ex = _ex;
        }
        public OutputEx() : this(null) {}

        public static implicit operator bool (OutputEx me) { return me.OK; }
        public static implicit operator OutputEx (bool b) 
        {
            if(b) {
                return new OutputEx(null);
            } 
            else {
                return new OutputEx(new Exception("Unknown error!")); 
            }
        }
        public static implicit operator OutputEx (Exception ex) { return new OutputEx(ex); }
        public static implicit operator OutputEx (string message) { return new OutputEx(new Exception(message)); }
    }
}