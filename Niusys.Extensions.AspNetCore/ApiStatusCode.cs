namespace Niusys.Extensions.AspNetCore
{
    public enum ApiStatusCode : int
    {
        Success = 200,

        ModelValidateError = 300,

        [StatusCodeDescription("Token未传或者传入的Token无效", "会话已超时，请重新登录")]
        TokenNotExists = 401,
        [StatusCodeDescription("Token已被踢", "会话已超时，请重新登录")]
        TokenBanned = 402,
        [StatusCodeDescription("Token不在有效范围", "会话已超时，请重新登录")]
        TokenExpired = 403,
        [StatusCodeDescription("Token不在有效范围", "会话已超时，请重新登录")]
        TokenIdxIssue = 404,
        [StatusCodeDescription("Token其他问题", "会话已超时，请重新登录")]
        TokenIssue = 405,
        [StatusCodeDescription("验证码校验失败", "验证码错误，请重新输入")]
        ValidCodeVerfifyFail = 406,
        [StatusCodeDescription("用户IP不在白名单列表", "网络连接错误")]
        IpUnauthorized = 407,
        [StatusCodeDescription("用户密码错误", "用户名或密码错误")]
        UserPasswordWrong = 408,
        [StatusCodeDescription("用户不存在", "用户密码错误")]
        UserNotExists = 409,
        [StatusCodeDescription("用户已锁定", "当前账户已被管理员锁定")]
        UserLocked = 410,
        [StatusCodeDescription("访问被禁止(无对应接口所要求的Permission)", "当前账户暂无权访问")]
        AccessForbidden = 414,
        [StatusCodeDescription("当前请求接口已明确忽略Token认证，如需认证请从忽略名单中移除", "当前账户暂无权访问")]
        AuthNotRequired = 415,
        InterfaceNameCannotIdentity = 416,

        RequestCancelled = 418,

        DatabaseOperationFail = 420,

        GeneralError = 500
    }
}
