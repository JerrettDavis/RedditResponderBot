
namespace Bot.Service.Common.Models
{
    /// <summary>
    /// The application settings
    /// </summary>
    public class AppSettings
    {
        /// <summary>
        /// The Reddit application ID required to access the API.
        /// </summary>
        public string AppId { get; set; } = null!;
        /// <summary>
        /// The account's refresh token for the given application ID.
        /// </summary>
        public string RefreshToken { get; set; } = null!;
    }
}