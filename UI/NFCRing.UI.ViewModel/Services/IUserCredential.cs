namespace NFCRing.UI.ViewModel.Services
{
    public interface IUserCredentials
    {
        int MaxTokensCount { get; }
        string GetName();
        bool IsValidCredentials(string username, string password);
    }
}