using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Index_lib;

namespace ID_lib
{
    class IDHandler
    {
        static void CreateUserFolder()
        {
            string test = System.IO.Path.GetDirectoryName(Application.ExecutablePath);
            if ()
            {

            }
            //System.IO.Path.GetDirectoryName(Application.ExecutablePath)
            HiddenFolder.Add();
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
