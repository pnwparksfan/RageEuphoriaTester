using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[assembly: Rage.Attributes.Plugin("RAGE Euphoria Tester", Description = "Test tool for viewing Euphoria effects", Author = "PNWParksFan")]

namespace RageEuphoriaTester
{
    using Rage;
    using Rage.Native;
    using Rage.Euphoria;


    public class EntryPoint
    {
        private static void Main()
        {
            Menus.CreateMenu();
            GameFiber.Hibernate();
        }
    }
}
