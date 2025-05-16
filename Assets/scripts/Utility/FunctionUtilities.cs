using System;
using System.Collections.Generic;
using UnityEngine;

namespace Utility
{
    public class PersistenFunctionCollection <TKey>
    {
        private Dictionary<TKey, Action> functions_ = new();

        public void Add(TKey key, Action value)
        {
            functions_.Add(key, value);
        }
        public void Remove(TKey key)
        {
            functions_.Remove(key);
        }
        public void Execute()
        {
            foreach (var func in functions_)
            {

                func.Value.Invoke();
            }
        }
    }
    public class FunctionQueue
    {
        private Queue<Action> queue_ = new Queue<Action>();

        public void Add(Action action)
        {
            queue_.Enqueue(action);
        }
        public void Execute()
        {
            while (queue_.Count > 0)
            {

                var func = queue_.Dequeue();
                func.Invoke();

            }
        }
    }

    public class TimedFunction
    {
        private float startTime_;
        private float duration_;
        private Action function_;

        public Action Function { get => function_; }

        public TimedFunction(float duration, Action function )
        {
            startTime_ = Time.time;
            duration_ = duration;
            function_ = function;
        }
        public bool isExpired()
        {
            return startTime_ + duration_ < Time.time;
        }
        
    }

    public class TimedFunctionCollection 
    {
        Queue<TimedFunction> functions_ = new Queue<TimedFunction>();

        public void Add(TimedFunction function)
        {
            functions_.Enqueue(function);
        }

        public void Execute()
        {
            var initialSize = functions_.Count;
            for (int i = 0; i < initialSize; i++) { 
                var function = functions_.Dequeue();
                if(!function.isExpired()) {
                    function.Function();
                    functions_.Enqueue(function);
                }

            }
            
        }

    }
}
