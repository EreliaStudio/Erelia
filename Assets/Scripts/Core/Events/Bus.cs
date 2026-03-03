using System;
using System.Collections.Generic;

namespace Erelia.Core.Event
{
	/// <summary>
	/// Base type for all events dispatched through <see cref="Bus"/>.
	/// </summary>
	/// <remarks>
	/// This class acts as a common constraint so the event bus can enforce that only
	/// event objects (and not arbitrary types) are published.
	/// </remarks>
	public abstract class GenericEvent
	{
		// Marker base class for events.
	}

	/// <summary>
	/// Simple in-process event bus used to publish and subscribe to <see cref="GenericEvent"/> messages.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This bus maps an event runtime <see cref="Type"/> to a multicast delegate of handlers.
	/// </para>
	/// <para>
	/// Characteristics:
	/// <list type="bullet">
	/// <item><description>Type-based routing: handlers are keyed by the concrete event type <c>TEventType</c>.</description></item>
	/// <item><description>Synchronous dispatch: <see cref="Emit{TEventType}"/> invokes handlers immediately on the calling thread.</description></item>
	/// <item><description>No inheritance dispatch: emitting a derived event does not notify handlers subscribed to a base type.</description></item>
	/// <item><description>Not thread-safe: concurrent Subscribe/Unsubscribe/Emit calls must be externally synchronized if needed.</description></item>
	/// </list>
	/// </para>
	/// </remarks>
	public static class Bus
	{
		/// <summary>
		/// Stores handlers by event type as multicast delegates.
		/// </summary>
		/// <remarks>
		/// The value is stored as <see cref="Delegate"/> but is always an <see cref="Action{TEventType}"/>
		/// for the corresponding key type.
		/// </remarks>
		private static readonly Dictionary<Type, Delegate> handlers = new Dictionary<Type, Delegate>();

		/// <summary>
		/// Subscribes a handler to events of type <typeparamref name="TEventType"/>.
		/// </summary>
		/// <typeparam name="TEventType">Concrete event type.</typeparam>
		/// <param name="handler">Callback invoked whenever an event of type <typeparamref name="TEventType"/> is emitted.</param>
		/// <remarks>
		/// If the same handler is subscribed multiple times, it will be invoked multiple times (multicast delegate behavior).
		/// Passing <c>null</c> does nothing.
		/// </remarks>
		public static void Subscribe<TEventType>(Action<TEventType> handler) where TEventType : GenericEvent
		{
			// Ignore null handlers to keep subscription logic simple.
			if (handler == null)
			{
				return;
			}

			// Use the concrete event type as the routing key.
			Type eventType = typeof(TEventType);

			// If handlers already exist for this type, append the new handler to the multicast delegate.
			if (handlers.TryGetValue(eventType, out var existing))
			{
				handlers[eventType] = (Action<TEventType>)existing + handler;
				return;
			}

			// First subscriber for this type: store the handler directly.
			handlers[eventType] = handler;
		}

		/// <summary>
		/// Unsubscribes a handler from events of type <typeparamref name="TEventType"/>.
		/// </summary>
		/// <typeparam name="TEventType">Concrete event type.</typeparam>
		/// <param name="handler">The handler previously passed to <see cref="Subscribe{TEventType}"/>.</param>
		/// <remarks>
		/// Passing <c>null</c> does nothing. If the handler was not subscribed, this method has no effect.
		/// </remarks>
		public static void Unsubscribe<TEventType>(Action<TEventType> handler) where TEventType : GenericEvent
		{
			// Ignore null handlers.
			if (handler == null)
			{
				return;
			}

			// Locate the delegate list for the concrete event type.
			Type eventType = typeof(TEventType);
			if (!handlers.TryGetValue(eventType, out var existing))
			{
				return;
			}

			// Remove the handler from the multicast delegate.
			var updated = (Action<TEventType>)existing - handler;

			// If no handlers remain, remove the dictionary entry.
			if (updated == null)
			{
				handlers.Remove(eventType);
				return;
			}

			// Otherwise, store the updated multicast delegate.
			handlers[eventType] = updated;
		}

		/// <summary>
		/// Emits (publishes) an event instance to all subscribers of its concrete type <typeparamref name="TEventType"/>.
		/// </summary>
		/// <typeparam name="TEventType">Concrete event type.</typeparam>
		/// <param name="evt">Event instance to dispatch.</param>
		/// <remarks>
		/// Dispatch is synchronous and happens immediately on the calling thread.
		/// If <paramref name="evt"/> is <c>null</c>, nothing happens.
		/// </remarks>
		public static void Emit<TEventType>(TEventType evt) where TEventType : GenericEvent
		{
			// Ignore null events.
			if (evt == null)
			{
				return;
			}

			// Look up handlers registered for this exact event type and invoke them.
			if (handlers.TryGetValue(typeof(TEventType), out var existing))
			{
				((Action<TEventType>)existing)?.Invoke(evt);
			}
		}
	}
}