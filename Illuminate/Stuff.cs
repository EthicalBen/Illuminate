using System.Collections.Generic;
namespace Sandbox
{
    class Server
    {

        List<Users> users;
        List<Role> roles;

        int number;

        public Server(List<Users>? users = null)
        {
            this.users = users ?? new List<Users>();
        }
        #region Not Today
        void Button1(object sender)
        {
            var i = 0;

        }

        void Button2(object sender)
        {

        }

        void Button3(object sender)
        {

        }

        void Button4(object sender)
        {

        }
        #endregion
    }

    class Users 
    {
        void test()
        {
            new Server();
        }
    }
    class Role { }
}