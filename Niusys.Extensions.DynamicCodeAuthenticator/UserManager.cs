using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Niusys.Extensions.DynamicCodeAuthenticator
{
    public interface IGaSetting
    {
        string Issuer { get; }
    }
    public class UserManager<TUser, TGaSetting> : IUserManager<TUser>
        where TUser : IDynamicCodeUser
        where TGaSetting : class, IGaSetting, new()
    {
        private readonly ILogger<UserManager<TUser, TGaSetting>> _logger;
        private readonly IUserRepository<TUser> _userRepository;
        private readonly IUserDynamicCodeTokenProvider<TUser> _twoFactorTokenProvider;
        private readonly UrlEncoder _urlEncoder;
        private readonly TGaSetting _gaSetting;
        private static readonly RandomNumberGenerator _rng = RandomNumberGenerator.Create();

        private const string AuthenticatorUriFormat = "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6";

        public UserManager(ILogger<UserManager<TUser, TGaSetting>> logger,
                             IUserRepository<TUser> userRepository,
                             IUserDynamicCodeTokenProvider<TUser> twoFactorTokenProvider,
                             UrlEncoder urlEncoder,
                             IOptions<TGaSetting> gaSettingOptions)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _twoFactorTokenProvider = twoFactorTokenProvider ?? throw new ArgumentNullException(nameof(twoFactorTokenProvider));
            _urlEncoder = urlEncoder ?? throw new ArgumentNullException(nameof(urlEncoder));
            _gaSetting = gaSettingOptions?.Value ?? throw new ArgumentNullException(nameof(gaSettingOptions));
        }
        public async Task<byte[]> CreateSecurityTokenAsync(TUser user)
        {
            return Encoding.Unicode.GetBytes(await GetSecurityStampAsync(user));
        }

        public Task<string> GetUserNameAsync(TUser user)
        {
            return Task.FromResult(user.UserName);
        }

        /// <summary>
        /// Get the security stamp for the specified <paramref name="user" />.
        /// </summary>
        /// <param name="user">The user whose security stamp should be set.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the security stamp for the specified <paramref name="user"/>.</returns>
        public virtual async Task<string> GetSecurityStampAsync(TUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            var stamp = user.DynamicCodeSecret;
            if (string.IsNullOrWhiteSpace(stamp))
            {
                _logger.LogWarning("GetSecurityStampAsync for user {userId} failed because stamp was null.", await GetUserNameAsync(user));
                throw new InvalidOperationException("NullSecurityStamp");
            }
            return stamp;
        }

        /// <summary>
        /// Resets the authenticator key for the user.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>Whether the user was successfully updated.</returns>
        public async Task ResetAuthenticatorKeyAsync(TUser user)
        {
            user.DynamicCodeSecret = GenerateNewAuthenticatorKey();
            await _userRepository.ResetUserDynamicCodeSecret(user, user.DynamicCodeSecret);
        }

        public virtual string GenerateNewAuthenticatorKey() => NewSecurityStamp();

        private static string NewSecurityStamp()
        {
            byte[] bytes = new byte[20];
            _rng.GetBytes(bytes);
            return Base32.ToBase32(bytes);
        }

        public Task<string> GetAuthenticatorKeyAsync(IDynamicCodeUser user) => Task.FromResult(user.DynamicCodeSecret);

        /// <summary>
        /// Verifies the specified two factor authentication <paramref name="token" /> against the <paramref name="user"/>.
        /// </summary>
        /// <param name="user">The user the token is supposed to be for.</param>
        /// <param name="tokenProvider">The provider which will verify the token.</param>
        /// <param name="token">The token to verify.</param>
        /// <returns>
        /// The <see cref="Task"/> that represents result of the asynchronous operation, true if the token is valid,
        /// otherwise false.
        /// </returns>
        public virtual async Task<bool> VerifyDynamicCodeTokenAsync(TUser user, string token)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            // Make sure the token is valid
            var result = await _twoFactorTokenProvider.ValidateAsync("GoogleAuthenticator", token, this, user);
            if (!result)
            {
                _logger.LogWarning($"{nameof(VerifyDynamicCodeTokenAsync)}() failed for user {await GetUserNameAsync(user)}.");
            }
            return result;
        }

        public async Task SetGaEnabledAsync(TUser user, bool enabled)
        {
            if (enabled)
            {
                user.IsDynamicCodeEnabled = true;
                await _userRepository.EnableUserDynamicCodeStatus(user);
            }
            else
            {
                user.IsDynamicCodeEnabled = false;
                await _userRepository.DisableUserDynamicCodeStatus(user);
            }
        }

        public async Task<AuthenticatorModel> GenerateSharedKeyAndQrCodeUriAsync(TUser wrapperUser)
        {
            var unformattedKey = await GetAuthenticatorKeyAsync(wrapperUser);
            if (string.IsNullOrEmpty(unformattedKey))
            {
                await ResetAuthenticatorKeyAsync(wrapperUser);
                unformattedKey = await GetAuthenticatorKeyAsync(wrapperUser);
            }

            var model = new AuthenticatorModel();
            model.SharedKey = FormatKey(unformattedKey);
            model.AuthenticatorUri = GenerateQrCodeUri(await GetUserNameAsync(wrapperUser), unformattedKey);
            return model;
        }

        #region Helpers
        private string FormatKey(string unformattedKey)
        {
            var result = new StringBuilder();
            int currentPosition = 0;
            while (currentPosition + 4 < unformattedKey.Length)
            {
                result.Append(unformattedKey.Substring(currentPosition, 4)).Append(" ");
                currentPosition += 4;
            }
            if (currentPosition < unformattedKey.Length)
            {
                result.Append(unformattedKey.Substring(currentPosition));
            }

            return result.ToString().ToLowerInvariant();
        }

        private string GenerateQrCodeUri(string email, string unformattedKey)
        {
            return string.Format(
                AuthenticatorUriFormat,
                _urlEncoder.Encode(_gaSetting.Issuer),
                _urlEncoder.Encode(email),
                unformattedKey);
        }
        #endregion
    }
}
