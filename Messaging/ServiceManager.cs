using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Design;

namespace Messaging
{
    public class ServiceManager
    {
        static ServiceManager manager = new ServiceManager();

        public static ServiceManager getServiceManager()
        {
            return manager;
        }

        public delegate void Handler<T>(T item);

        // private constructor so no one can make another instance
        private ServiceManager()
        {

        }

        abstract class HandlerHolder
        {
            public abstract void Invoke(object message);
        }
        class HandlerHolder<T> : HandlerHolder
        {
            Handler<T> handler;
            public void AddHandler(Handler<T> handler)
            {
                this.handler += handler;
            }

            public override void Invoke(object message)
            {
                handler.Invoke((T)message);
            }
        }

        Dictionary<Type, HandlerHolder> handlers = new Dictionary<Type, HandlerHolder>();

        // the argument here is just a function that takes an argument of a subtype of message
        public void RegisterListener<T>(Handler<T> handler) where T : Message
        {
            Type type = typeof(T);
            if (handlers.ContainsKey(type))
            {
                HandlerHolder<T> holder = (HandlerHolder<T>)handlers[type];
                holder.AddHandler(handler);
            }
            else
            {
                HandlerHolder<T> holder = new HandlerHolder<T>();
                holder.AddHandler(handler);
                handlers.Add(type, holder);
            }
        }

        public void SendMessage<T>(T message) where T : Message
        {
            foreach (Type type in AllTypes(typeof(T)))
            {
                if (handlers.ContainsKey(type))
                    handlers[type].Invoke(message);
            }
        }

        // gives all the parent types of a type, used to send to listeners of a parent type
        static IEnumerable<Type> AllTypes(Type type)
        {
            yield return type;

            foreach (Type i in type.GetInterfaces())
            {
                yield return i;
            }

            Type t = type.BaseType;
            while(t != null)
            {
                yield return t;
                t = t.BaseType;
            }
        }
    }
}
