using System;

namespace UI.Services
{
    /// <summary>
    /// A single toast notification. Immutable — <see cref="INotificationService"/>
    /// creates one per call and adds/removes it from its observable collection.
    /// </summary>
    public sealed class ToastMessage
    {
        public ToastMessage(string message, ToastType type)
        {
            Id = Guid.NewGuid();
            Message = message;
            Type = type;
            CreatedAt = DateTime.Now;
        }

        public Guid Id { get; }

        public string Message { get; }

        public ToastType Type { get; }

        public DateTime CreatedAt { get; }
    }
}
