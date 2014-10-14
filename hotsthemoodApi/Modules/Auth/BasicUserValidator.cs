using System.Collections.Generic;
using Nancy.Authentication.Basic;
using Nancy.Security;

namespace hotsthemoodApi.Modules.Auth
{
    public class BasicUserValidator : IUserValidator
    {
        public IUserIdentity Validate(string username, string password)
        {
            if (username == "hots" && password == "happ")
            {
                return new AdminUserIdentitiy();
            }
            return null;
        }
    }

    public class AdminUserIdentitiy : IUserIdentity
    {
        public string UserName { get; private set; }
        public IEnumerable<string> Claims { get; private set; }

        public AdminUserIdentitiy()
        {
            UserName = "Admin";
            Claims = new[] { "administrator" };
        }
    }
}