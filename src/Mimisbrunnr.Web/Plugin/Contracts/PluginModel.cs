using System.ComponentModel.DataAnnotations;
using Mimisbrunnr.Integration.User;

/// <summary>
/// Represents a plugin in the Mimisbrunnr system.
/// </summary>
public class PluginModel
{
    /// <summary>
    /// Unique identifier of the plugin instance.
    /// </summary>
    public string Id { get; set; }
    /// <summary>
    /// Unique identifier of the plugin package or assembly.
    /// </summary>
    [Required]
    [MaxLength(255)]
    public string PluginIdentifier { get; set; }
    /// <summary>
    /// Display name of the plugin shown in the UI.
    /// </summary>
    [Required]
    [MaxLength(255)]
    public string Name { get; set; }
    /// <summary>
    /// Version number of the plugin.
    /// </summary>
    [Required]
    [MaxLength(64)]
    public string Version { get; set; }
    /// <summary>
    /// User who installed this plugin.
    /// </summary>
    public UserModel InstalledBy { get; internal set; }
    /// <summary>
    /// Date and time when the plugin was installed.
    /// </summary>
    public DateTime Installation { get; internal set; }
    /// <summary>
    /// Collection of macros provided by this plugin.
    /// </summary>
    public MacroModel[] Macros { get; set; } = [];
}
