namespace NFCRing.UI.ViewModel.Services
{
    public interface IDialogService
    {
        /// <summary>
        /// Show question dialog.
        /// </summary>
        /// <param name="questionMessage"></param>
        /// <returns></returns>
        bool ShowQuestionDialog(string questionMessage);

        /// <summary>
        /// Show error dialog.
        /// </summary>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        bool ShowErrorDialog(string errorMessage);
    }
}
