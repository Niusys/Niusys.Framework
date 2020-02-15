namespace Niusys.Extensions.AspNetCore.Sessions
{
    public interface IUserSession<TUser, TUserType>
        where TUser : class, IUser
        where TUserType : struct
    {
        TUser CurrentUser { get; }
        string SessionKey { get; }
        string UserId { get; }
        string UserName { get; }
        TUserType UserType { get; }
    }
}