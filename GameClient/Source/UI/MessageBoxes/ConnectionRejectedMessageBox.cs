namespace GameClient.UI;

/// <summary>
/// Represents a message box that is displayed
/// when the connection to the server is rejected.
/// </summary>
/// <param name="rejectReasonKey">The reason why the connection was rejected.</param>
internal class ConnectionRejectedMessageBox(string rejectReasonKey) : ConnectionFailedMessageBox(
        "MessageBoxLabels.ConnectionRejected",
        new LocalizedString(
                $"ConnectionRejectedReason.{char.ToUpper(rejectReasonKey[0]) + rejectReasonKey[1..]}"))
{
}
