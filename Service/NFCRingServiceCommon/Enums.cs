namespace NFCRing.Service.Common
{
    public enum ServiceState
    {
        Starting,
        Running,
        Stopping,
        Stopped
    }
    public enum MessageType
    {
        GetToken,
        RegisterToken,
        Token,
        AssociatePluginToToken,
        CancelRegistration,
        UserCredential,
        GetState,
        State,
        Message,
        Delete,
        RegisterAll,
        UpdateFriendlyName
    }
}
