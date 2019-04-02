using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ID_lib
{
    class IDHandler
    {
        static void CreateUserFolder()
        {
            /*string test = System.IO.Path.GetDirectoryName(Application.ExecutablePath);*/
            if (true)
            {

            }
            //System.IO.Path.GetDirectoryName(Application.ExecutablePath)
        }

        static void CreateUser(string password)
        {
            //Generate UID
            //
        }

        static bool IsUserPresent()
        {
            //Test for validators in hidden subfolder
            if (true)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        static bool ValidateUser(string uid, string password)
        {
            //find matching UID file
            //compare password hash
            if (true)
            {
                return true;
            }
            else
            {
                return false;
            }
        }



    }
}
