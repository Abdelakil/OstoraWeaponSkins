using SwiftlyS2.Shared.Events;

namespace SwiftlyS2.Core.Events;


internal class OnClientVoiceEvent : IOnClientVoiceEvent
{
    public int PlayerId { get; set; }
}
