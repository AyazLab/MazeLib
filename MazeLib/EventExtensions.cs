using System;
using System.Threading;

namespace MazeLib
{
    public static class EventExtensions
    {
        public static void Raise(this EventHandler eventHandler, SynchronizationContext synchronizationContext, object sender, EventArgs eventArgs)
        {
            if (eventHandler != null)
            {
                if (synchronizationContext != null)
                {
                    synchronizationContext.Post(new SendOrPostCallback(o => eventHandler(sender, eventArgs)), null);
                }
                else
                {
                    eventHandler(sender, eventArgs);
                }
            }
        }

        public static void Raise<TEventArgs>(this EventHandler<TEventArgs> eventHandler, SynchronizationContext synchronizationContext, object sender, TEventArgs eventArgs)
            where TEventArgs : EventArgs
        {
            if (eventHandler != null)
            {
                if (synchronizationContext != null)
                {
                    synchronizationContext.Post(new SendOrPostCallback(o => eventHandler(sender, eventArgs)), null);
                }
                else
                {
                    eventHandler(sender, eventArgs);
                }
            }
        }
    }
}
