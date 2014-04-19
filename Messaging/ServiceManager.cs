using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.ComponentModel.Design;
using System.Threading.Tasks;
using System.Threading;
using RFC.Geometry;
using RFC.Core;
using System.Drawing;

namespace RFC.Messaging
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
            List<Tuple<Handler<T>, object>> handlers = new List<Tuple<Handler<T>,object>>();
            public void AddHandler(Handler<T> handler, object lockObject)
            {
                lock (this)
                {
                    this.handlers.Add(new Tuple<Handler<T>, object>(handler, lockObject));
                }
            }

            public override void Invoke(object message)
            {

                lock (this) {
                    foreach (Tuple<Handler<T>, object> handler in handlers)
                    {
                        new Task(() => launchThread(handler, message)).Start();
                    }
                }
            }

            private void launchThread(Tuple<Handler<T>, object> handler, object message)
            {
                if (handler.Item2 != null)
                {
                    lock (handler.Item2)
                    {
                        handler.Item1.Invoke((T)message);
                    }
                }
                else
                {
                    handler.Item1.Invoke((T)message);
                }
            }
        }


        ConcurrentDictionary<Type, HandlerHolder> handlers = new ConcurrentDictionary<Type, HandlerHolder>();

		//buffer of last messages
		ConcurrentDictionary<Type, Message> messageBuffer = new ConcurrentDictionary<Type, Message> ();

        /// <summary>
        /// Register a listener for messages
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="handler">a function that takes an argument of a subtype of message that should be called when that type of message is sent</param>
        /// <param name="lockObject">an object to lock when calling the handler, pass null if you will handle concurrency manually</param>
        public void RegisterListener<T>(Handler<T> handler, object lockObject) where T : Message
        {
            Type type = typeof(T);
            HandlerHolder holder = handlers.GetOrAdd(type, new HandlerHolder<T>());
            ((HandlerHolder<T>)holder).AddHandler(handler, lockObject);
        }

        // so that people will actually use the log messages
        public void debug(string msg)
        {
            SendMessage<LogMessage>(new LogMessage(msg));
        }
        public void db(string msg)
        {
            debug(msg);
        }

        // visual debug to field drawer
        public void vdb(RobotDestinationMessage r)
        {
            vdb(r,new Color());
        }
        public void vdb(RobotInfo r)
        {
            vdb(r,new Color());
        }
        public void vdb(Vector2 r)
        {
            vdb(r, new Color());
        }
        public void vdb(RobotDestinationMessage r, Color c)
        {
            vdb(r.Destination,c);
        }
        public void vdb(RobotInfo r, Color c)
        {
            vdb(r.Position,c);
        }
        public void vdb(Vector2 r, Color c)
        {
            SendMessage<VisualDebugMessage>(new VisualDebugMessage(r,c));
        }
        public void vdbClear()
        {
            SendMessage<VisualDebugMessage>(new VisualDebugMessage());
        }

        public void SendMessage<T>(T message) where T : Message
        {
            foreach (Type type in AllTypes(typeof(T)))
            {
                HandlerHolder holder;

                // adding message to buffer system
                messageBuffer.AddOrUpdate(type, message, (t, m) => message);

                if (handlers.TryGetValue(type, out holder))
                {
                    holder.Invoke(message);
                }

            }
        }
		
		/// <summary>
		///  returns the last message seen of the given type.
		///  if there has been no message, return null.
		///  a sent message will overwrite the buffer for all
		///  super classes.
		/// </summary>
		/// <returns>The last message of Type type.</returns>
		/// <param name="type">Type.</param>
		public T GetLastMessage<T>() where T : Message
		{
			Message m;
            if (messageBuffer.TryGetValue(typeof(T), out m))
            {
                return (T)m;
            }
            else
            {
                db("Message type not in buffer: " + typeof(T).ToString());
                return null;
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
