using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Windows.Forms;

using Rage;
using Rage.Euphoria;
using RAGENativeUI;
using RAGENativeUI.Elements;

namespace RageEuphoriaTester
{
    internal static class Menus
    {
        public static UIMenu MainMenu = new UIMenu("RAGE Euphoria Tester", "");
        public static UIMenu MessageMenu = new UIMenu("Euphoria Messages", "");
        public static UIMenuListItem Targets = new UIMenuListItem("Select Ped Target", "Select (press enter) to change selected ped.", new string[] { "Player", "Nearest Ped", "Selected Ped" });
        public static UIMenuItem MessageMenuSelector = new UIMenuItem("Configure Messages", "");
        public static MenuPool menuPool = new MenuPool();

        public static Ped TargetPed => Game.LocalPlayer.Character;

        internal static void CreateMenu()
        {
            MainMenu.BindMenuToItem(MessageMenu, MessageMenuSelector);
            MainMenu.AddItem(MessageMenuSelector);
            MainMenu.AddItem(Targets);

            Targets.Activated += On_TargetChanged;

            // var allMessages = Assembly.GetCallingAssembly().GetTypes().Where(t => t.IsAssignableFrom(typeof(EuphoriaMessage)) && !t.IsAbstract).ToArray();
            var allMessages = (from domainAssembly in AppDomain.CurrentDomain.GetAssemblies()
                            from assemblyType in domainAssembly.GetTypes()
                            where assemblyType.IsSubclassOf(typeof(EuphoriaMessage))
                            select assemblyType).Where(t => t.GetConstructors()
                                .Any(c => c.GetParameters().Length == 1 && c.GetParameters()[0].ParameterType == typeof(bool))).ToArray();

            Game.LogTrivial("Assembly: " + Assembly.GetCallingAssembly().FullName);
            Game.LogTrivial("Total message types found: " + allMessages.Length);

            foreach (var T in allMessages)
            {
                Game.LogTrivial("Trying to load: " + T.Name);
                var instance = Activator.CreateInstance(T, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, new object[] { true }, null);
                new EuphoriaConfigPage((EuphoriaMessage)instance);
            }

            menuPool.Add(MainMenu);
            menuPool.Add(MessageMenu);

            foreach (UIMenu menu in menuPool)
            {
                menu.SetMenuWidthOffset(100);
            }

            GameFiber.StartNew(delegate 
            {
                while(true)
                {
                    if (Game.IsKeyDown(Keys.F11) && !menuPool.IsAnyMenuOpen())
                    {
                        MainMenu.Visible = !MainMenu.Visible;
                    }
                    menuPool.ProcessMenus();
                    GameFiber.Yield();
                }
                
            });
        }

        private static void On_TargetChanged(UIMenu sender, UIMenuItem selectedItem)
        {
            Game.DisplayNotification("~y~Sorry!~w~ Ped selection isn't working yet, all effects will be applied to the player.");
        }
    }
}
