namespace OpenVASP.CSharpClient.Internals.Messages.Session
{
    public enum SessionAbortCause
    {
        AckTimeout = 1,
        SessionTimeout = 2,
        InvalidMessage = 3,
        ServiceDisruption = 4,
        Unspecified = 5
    }
}