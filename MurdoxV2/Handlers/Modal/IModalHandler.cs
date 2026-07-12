using DSharpPlus.EventArgs;

namespace MurdoxV2.Handlers.Modal;

public interface IModalHandler
{
    string CustomId { get; }
    Task HandleAsync(ModalSubmittedEventArgs e);
}
