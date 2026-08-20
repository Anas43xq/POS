namespace POS.Contracts.Printing;

public interface IPrinter
{
    string PrinterType { get; }

    Task PrintAsync(object content, bool showDialog, CancellationToken cancellationToken = default);
}