using System;
using System.Collections.Generic;

namespace GomokuBase.Event
{
    public abstract class GameEvent
    {
    }

    public static class EventManager
    {
        private abstract class EventTriggerBase
        {
            public abstract void Trigger(GameEvent Msg);
        }

        private class EventTrigger<T> : EventTriggerBase where T : GameEvent
        {
            public event Action<T> OnEvent = null;

            public override void Trigger(GameEvent Msg)
            {
                if (OnEvent != null)
                {
                    OnEvent((T)Msg);
                }
            }
        }

        private static readonly Dictionary<string, EventTriggerBase> EventList_ = new Dictionary<string, EventTriggerBase>();

        public static void Register<T>(Action<T> Callback) where T : GameEvent
        {
            var FullName = typeof(T).FullName;
            if (!EventList_.ContainsKey(FullName))
            {
                EventList_.Add(FullName, new EventTrigger<T>());
            }

            ((EventTrigger<T>)EventList_[FullName]).OnEvent += Callback;
        }

        public static void UnRegister<T>(Action<T> Callback) where T : GameEvent
        {
            var FullName = typeof(T).FullName;
            if (EventList_.ContainsKey(FullName))
            {
                ((EventTrigger<T>)EventList_[FullName]).OnEvent -= Callback;
            }
        }

        public static void Send<T>(T Msg) where T : GameEvent
        {
            var FullName = typeof(T).FullName;
            if (EventList_.ContainsKey(FullName))
            {
                ((EventTrigger<T>)EventList_[FullName]).Trigger(Msg);
            }
        }

        public static void Send<T>() where T : GameEvent, new()
        {
            var Event = new T();
            Send(Event);
        }
    }
}
