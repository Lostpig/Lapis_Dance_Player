using System;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine;

namespace Oz.Timeline
{
    [Serializable]
    public class EventMarker : Marker, INotification, INotificationOptionProvider
    {
        // Fields
        private bool m_Retroactive; // 0x28
        private bool m_EmitOnce; // 0x29
        private string m_EventName; // 0x30
        private EventData m_EventData; // 0x38

        // Properties
        public bool Retroactive { get => m_Retroactive; set => m_Retroactive = value; }
        public bool EmitOnce { get => m_EmitOnce; set => m_EmitOnce = value; }
        public string EventName { get => m_EventName; set => m_EventName = value; }
        public EventData EventData { get => m_EventData; set => m_EventData = value; }
        public PropertyName id { get; }
        public NotificationFlags flags { get; }
    }

    [Serializable]
    public class EventData
    {
        // Fields
        public string StringValue; // 0x10
        public float FloatValue; // 0x18
        public int IntValue; // 0x1c
        private ExposedReference<UnityEngine.Object> ObjectValue; // 0x20
        public UnityEngine.Object ResolvedObjectValue; // 0x30

        // Properties

        // Methods
        // RVA: 0x13d750c VA: 0x6e8992750c
        public void ResolveEventArgs(IExposedPropertyTable resolver) { }
    }
}
