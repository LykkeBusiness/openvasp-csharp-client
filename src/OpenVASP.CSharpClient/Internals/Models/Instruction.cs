namespace OpenVASP.CSharpClient.Internals
{
    public enum Instruction : byte
    {
        /// <summary>
        /// Acknowledge Envelope receipt (ACK)
        /// </summary>
        Ack = 0,

        /// <summary>
        /// Inviting for a Connection (INVITE)
        /// </summary>
        Invite = 1,

        /// <summary>
        /// Accepting a Connection invitation (ACCEPT)
        /// </summary>
        Accept = 2,

        /// <summary>
        /// Denying a Connection invitation (DENY)
        /// </summary>
        Deny = 3,

        /// <summary>
        /// Sending an Envelope over a Connection (UPDATE)
        /// </summary>
        Update = 4,

        /// <summary>
        /// Closing a Connection (CLOSE)
        /// </summary>
        Close = 5
    }
}