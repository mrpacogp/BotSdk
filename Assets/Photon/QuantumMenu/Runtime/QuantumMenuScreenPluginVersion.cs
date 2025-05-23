namespace Quantum.Menu {
  using System.Collections.Generic;
#if QUANTUM_ENABLE_TEXTMESHPRO
  using Text = TMPro.TMP_Text;
#else 
  using Text = UnityEngine.UI.Text;
#endif
  using UnityEngine;

  /// <summary>
  /// The version plugin is used to format informational parts of the <see cref="IQuantumMenuConnectArgs"/> or <see cref="IQuantumMenuConnection"/>.
  /// </summary>
  public class QuantumMenuScreenPluginVersion : QuantumMenuScreenPlugin {
    /// <summary>
    /// The text field to write version information into.
    /// </summary>
    [InlineHelp, SerializeField] protected Text _textField;

    /// <summary>
    /// The parent screen is shown. Use the connection object (is available) or the connection args to format a version string.
    /// </summary>
    /// <param name="screen"></param>
    public override void Show(QuantumMenuUIScreen screen) {
      if (_textField != null) {
        if (screen.Connection != null && screen.Connection.IsConnected) {
          _textField.text = GetInformationalVersion(screen.Connection);
        } else {
          _textField.text = GetInformationalVersion(screen.ConnectionArgs);
        }
      }
    }

    /// <summary>
    /// Format a version string from the <see cref="IQuantumMenuConnectArgs"/>.
    /// </summary>
    /// <param name="connectionArgs">Connection args.</param>
    /// <returns>Informational version string</returns>
    public virtual string GetInformationalVersion(IQuantumMenuConnectArgs connectionArgs) {
      if (connectionArgs == null) {
        return string.Empty;
      }
      return CreateInformationVersion(string.IsNullOrEmpty(connectionArgs.Region) ? connectionArgs.PreferredRegion : connectionArgs.Region, connectionArgs.AppVersion);
    }

    /// <summary>
    /// Format a version string from the <see cref="IQuantumMenuConnection"/>.
    /// </summary>
    /// <param name="connection">Connection object.</param>
    /// <returns>Informational version string</returns>
    public virtual string GetInformationalVersion(IQuantumMenuConnection connection) {
      if (connection == null) {
        return string.Empty;
      }
      return CreateInformationVersion(connection.Region, connection.AppVersion);
    }

    /// <summary>
    /// Construct the informational version string.
    /// </summary>
    /// <param name="region">Region</param>
    /// <param name="appVersion">AppVersion</param>
    /// <returns>Informational version string</returns>
    public virtual string CreateInformationVersion(string region, string appVersion) {
      var list = new List<string>();
      if (string.IsNullOrEmpty(region) == false) {
        list.Add(region);

      } else {
        list.Add("Best Region");
      }
      if (string.IsNullOrEmpty(appVersion) == false) {
        list.Add(appVersion);
      }
      if (list.Count == 0) {
        return null;
      }
      return string.Join(" | ", list);
    }
  }
}
