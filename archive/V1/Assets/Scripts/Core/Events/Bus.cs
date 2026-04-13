using System;
using System.Collections.Generic;

namespace Erelia.Core.Event
{
	public abstract class GenericEvent
	{
	}

	public static class Bus
	{
		private static readonly Dictionary<Type, Delegate> handlers = new Dictionary<Type, Delegate>();

		public static void Subscribe<TEventType>(Action<TEventType> handler) where TEventType : GenericEvent
		{
			if (handler == null)
			{
				return;
			}

			Type eventType = typeof(TEventType);

			if (handlers.TryGetValue(eventType, out var existing))
			{
				handlers[eventType] = (Action<TEventType>)existing + handler;
				return;
			}

			handlers[eventType] = handler;
		}

		public static void Unsubscribe<TEventType>(Action<TEventType> handler) where TEventType : GenericEvent
		{
			if (handler == null)
			{
				return;
			}

			Type eventType = typeof(TEventType);
			if (!handlers.TryGetValue(eventType, out var existing))
			{
				return;
			}

			var updated = (Action<TEventType>)existing - handler;

			if (updated == null)
			{
				handlers.Remove(eventType);
				return;
			}

			handlers[eventType] = updated;
		}

		public static void Emit<TEventType>(TEventType evt) where TEventType : GenericEvent
		{
			if (evt == null)
			{
				return;
			}

			if (handlers.TryGetValue(typeof(TEventType), out var existing))
			{
				((Action<TEventType>)existing)?.Invoke(evt);
			}
		}
	}
}