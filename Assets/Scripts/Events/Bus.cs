using System;
using System.Collections.Generic;

namespace Erelia.Event
{
	public abstract class GenericEvent
	{
	}

	public static class Bus
	{
		private static readonly Dictionary<Type, Delegate> handlers = new Dictionary<Type, Delegate>();

		public static void Subscribe<T>(Action<T> handler) where T : GenericEvent
		{
			if (handler == null)
			{
				return;
			}

			Type eventType = typeof(T);
			if (handlers.TryGetValue(eventType, out var existing))
			{
				handlers[eventType] = (Action<T>)existing + handler;
				return;
			}

			handlers[eventType] = handler;
		}

		public static void Unsubscribe<T>(Action<T> handler) where T : GenericEvent
		{
			if (handler == null)
			{
				return;
			}

			Type eventType = typeof(T);
			if (!handlers.TryGetValue(eventType, out var existing))
			{
				return;
			}

			var updated = (Action<T>)existing - handler;
			if (updated == null)
			{
				handlers.Remove(eventType);
				return;
			}

			handlers[eventType] = updated;
		}

		public static void Emit<T>(T evt) where T : GenericEvent
		{
			if (evt == null)
			{
				return;
			}

			if (handlers.TryGetValue(typeof(T), out var existing))
			{
				((Action<T>)existing)?.Invoke(evt);
			}
		}
	}
}
