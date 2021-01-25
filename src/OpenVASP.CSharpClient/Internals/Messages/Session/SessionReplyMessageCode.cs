namespace OpenVASP.CSharpClient.Internals.Messages.Session
{
    public enum SessionReplyMessageCode
    {
        SessionAccepted = 1,
        SessionDeclinedRequestNotValid = 2,
        SessionDeclinedOriginatorVaspDeclined = 3,
        SessionDeclinedTemporaryDisruptionOfService = 3
    }
}