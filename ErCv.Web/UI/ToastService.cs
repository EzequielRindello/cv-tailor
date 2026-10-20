namespace ErCv.Web.UI
{
    public enum ToastKind
    {
        Success,
        Error,
        Info
    }

    public record ToastMessage(Guid Id, string Text, ToastKind Kind, string? ActionLabel, Func<Task>? Action);

    /// <summary>Transient messages shown by <c>ToastHost</c>. One instance per Blazor circuit.</summary>
    public class ToastService
    {
        private static readonly TimeSpan AutoDismiss = TimeSpan.FromSeconds(4);

        private readonly List<ToastMessage> _toasts = new();

        public event Action? Changed;

        public IReadOnlyList<ToastMessage> Toasts => _toasts;

        public void Success(string text) => Show(text, ToastKind.Success);

        public void Info(string text) => Show(text, ToastKind.Info);

        /// <summary>Errors stay until dismissed, so the retry action does not vanish while the user reads them.</summary>
        public void Error(string text, string? actionLabel = null, Func<Task>? action = null) =>
            Show(text, ToastKind.Error, actionLabel, action);

        public void Dismiss(Guid id)
        {
            if (_toasts.RemoveAll(t => t.Id == id) > 0)
                Changed?.Invoke();
        }

        private void Show(string text, ToastKind kind, string? actionLabel = null, Func<Task>? action = null)
        {
            var toast = new ToastMessage(Guid.NewGuid(), text, kind, actionLabel, action);
            _toasts.Add(toast);
            Changed?.Invoke();

            if (kind != ToastKind.Error)
                _ = DismissLaterAsync(toast.Id);
        }

        private async Task DismissLaterAsync(Guid id)
        {
            await Task.Delay(AutoDismiss);
            Dismiss(id);
        }
    }
}
