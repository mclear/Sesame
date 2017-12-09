namespace NFCRing.UI.ViewModel.Services
{
    public interface IDialogService
    {
        /// <summary>
        /// Show question dialog.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        bool ShowQuestionDialog(string message);

        /// <summary>
        /// Show error dialog.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        bool ShowErrorDialog(string message);

        /// <summary>
        /// Show warning dialog.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        bool ShowWarningDialog(string message);

        /// <summary>
        /// Shoe image choosing dialog.
        /// </summary>
        /// <param name="imageData">The image data.</param>
        /// <returns></returns>
        bool ShowImageDialog(out ImageData imageData);
    }
}
