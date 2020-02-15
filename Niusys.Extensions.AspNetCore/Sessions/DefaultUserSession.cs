﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Security.Claims;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using Niusys.Extensions.Storage.Mongo;

namespace Niusys.Extensions.AspNetCore.Sessions
{
    public class DefaultUserSession<TUser, TUserType, TRole>
        where TUser : class, IUser
        where TUserType : struct
        where TRole : struct
    {
        private readonly IdentityOptions _options;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUserStore<TUser> _userRep;
        private readonly ClaimsIdentity _userIdentity;

        public DefaultUserSession(IHttpContextAccessor httpContextAccessor,
            IUserStore<TUser> userRep,
            IOptions<IdentityOptions> optionsAccessor)
        {
            if (optionsAccessor == null || optionsAccessor.Value == null)
            {
                throw new ArgumentNullException(nameof(optionsAccessor));
            }

            _options = optionsAccessor.Value;

            this._httpContextAccessor = httpContextAccessor;
            this._userRep = userRep;
            _userIdentity = httpContextAccessor.HttpContext.User.Identity as ClaimsIdentity;
        }

        private TUser _user;
        public TUser CurrentUser
        {
            get
            {
                if (!_userIdentity.IsAuthenticated)
                {
                    throw new Exception("未登录");
                }

                if (_user == null)
                {
                    lock (_httpContextAccessor.HttpContext)
                    {
                        if (_user == null)
                        {
                            _user = _userRep.GetByPropertyAsync(x => x.Sysid, UserId.SafeToObjectId()).Result;
                        }
                    }
                }

                // 第二次检查，如果还是为null, 则抛出异常(说明数据库用户发生变化)
                if (_user == null)
                    throw new Exception($"数据库中找不到当前已认证的用户#{UserId}");
                return _user;
            }
        }

        public string UserId => _userIdentity.Claims.FirstOrDefault(x => x.Type == _options.ClaimsIdentity.UserIdClaimType)?.Value;
        public string UserName => _userIdentity.Claims.FirstOrDefault(x => x.Type == _options.ClaimsIdentity.UserNameClaimType)?.Value;
        public TUserType UserType
        {
            get
            {
                if (Enum.TryParse<TUserType>(_userIdentity.Claims.FirstOrDefault(x => x.Type == "UserType")?.Value, out var userType))
                {
                    return userType;
                }
                throw new InvalidCastException($"UserType获取失败");
            }
        }

        private List<TRole> _userRoles;
        public IReadOnlyList<TRole> UserRoles
        {
            get
            {
                if (_userRoles == null)
                {
                    _userRoles = new List<TRole>();
                    var roles = _userIdentity.Claims.Where(x => x.Type == "UserRole").Select(x => x.Value);

                    foreach (var item in roles)
                    {
                        if (Enum.TryParse<TRole>(item, out var role))
                        {
                            _userRoles.Add(role);
                        }
                    }
                }
                return _userRoles.AsReadOnly();
            }
        }

        public string SessionKey => _userIdentity.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti)?.Value;
    }
}