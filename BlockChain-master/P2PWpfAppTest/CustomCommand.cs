using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace P2PWpfAppTest
{
    public static class CustomCommands
    {
        public static readonly RoutedUICommand StartMine = new RoutedUICommand(
                        "StartMine",
                        "StartMine",
                        typeof(CustomCommands));
        public static readonly RoutedUICommand StopMine = new RoutedUICommand(
                "StopMine",
                "StopMine",
                typeof(CustomCommands));
        public static readonly RoutedUICommand SignIn = new RoutedUICommand(
        "SignIn",
        "SignIn",
        typeof(CustomCommands));

        public static readonly RoutedUICommand SignOut = new RoutedUICommand(
        "SignOut",
        "SignOut",
        typeof(CustomCommands));
    }
}
