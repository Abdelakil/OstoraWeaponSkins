namespace SwiftlyS2.Shared.Misc;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class ThreadUnsafeAttribute : Attribute
{
    public ThreadUnsafeAttribute()
    {
    }
}