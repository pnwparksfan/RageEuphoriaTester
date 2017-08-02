using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Windows.Forms;
using System.Drawing;
using Rage;
using Rage.Euphoria;
using RAGENativeUI;
using RAGENativeUI.Elements;

namespace RageEuphoriaTester
{
    internal static class Menus
    {
        public static UIMenu MainMenu = new UIMenu("RAGE Euphoria Tester", "");
        public static UIMenu MessageMenu = new UIMenu("RAGE Euphoria Tester", "Euphoria Message List");
        public static UIMenuListItem Targets = new UIMenuListItem("Select Ped Target", "Select (press enter) to change selected ped.", new string[] { "Player", "Nearest Ped", "Selected Ped" });
        public static UIMenuItem MessageMenuSelector = new UIMenuItem("Configure Messages", "");
        public static UIMenuCheckboxItem TargetHumansOnly = new UIMenuCheckboxItem("Humans Only", true);
        public static UIMenuCheckboxItem LiveOnly = new UIMenuCheckboxItem("Live Peds Only", false);
        public static MenuPool menuPool = new MenuPool();

        public static Ped TargetPed { get; private set; } = Game.LocalPlayer.Character;

        internal static void CreateMenu()
        {
            MainMenu.BindMenuToItem(MessageMenu, MessageMenuSelector);
            MainMenu.AddItem(MessageMenuSelector);
            MainMenu.AddItem(TargetHumansOnly);
            MainMenu.AddItem(LiveOnly);
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
                menu.MouseControlsEnabled = false;
                menu.AllowCameraMovement = true;
                // menu.ControlDisablingEnabled = false;
            }

            GameFiber.StartNew(delegate 
            {
                while(true)
                {
                    if (Game.IsKeyDown(Keys.F11) && !menuPool.IsAnyMenuOpen())
                    {
                        MainMenu.Visible = !MainMenu.Visible;
                    }
                    HighlightSelectedPed();
                    menuPool.ProcessMenus();
                    GameFiber.Yield();
                }
                
            });
        }

        internal static void SelectPed()
        {
            TargetPed = null;

            while(Game.IsKeyDownRightNow(Keys.Enter)) { GameFiber.Sleep(100); }

            while(!Game.IsKeyDown(Keys.Enter))
            {
                foreach (UIMenu menu in menuPool)
                {
                    menu.Visible = false;
                }

                Game.DisplayHelp("~g~Enter~w~ to use the selected ped.\n~b~Right Arrow~w~ to go to the next ped.\n~b~Left Arrow~w~ to go to previous ped.");
                

                bool changed = false;
                int d = 0;
                
                if(Game.IsKeyDown(Keys.Right))
                {
                    d = 1;
                    changed = true;
                } else if (Game.IsKeyDown(Keys.Left))
                {
                    d = -1;
                    changed = true;
                }

                if(changed)
                {
                    var allPeds = World.GetAllPeds().Where(p => p && p != Game.LocalPlayer.Character && !(TargetHumansOnly.Checked && !p.IsHuman) && !(LiveOnly.Checked && p.IsDead)).OrderBy(p => p.DistanceTo(Game.LocalPlayer.Character)).ToList();
                    int i = allPeds.IndexOf(TargetPed) + d;

                    if (i > allPeds.Count - 1)
                    {
                        i = 0;
                    } else if (i < 0)
                    {
                        i = allPeds.Count - 1;
                    }

                    if (TargetPed)
                        TargetPed.IsPositionFrozen = false;

                    TargetPed = allPeds[i];
                    Game.DisplaySubtitle("Changed target ped to ~o~" + TargetPed.Model.Name, 6000);
                    if (TargetPed)
                        TargetPed.IsPositionFrozen = true;
                }
                GameFiber.Yield();
            }
            // Game.IsPaused = false;
            Game.HideHelp();
            if (TargetPed)
            {
                TargetPed.IsPositionFrozen = false;
                TargetPed.Tasks.Clear();
                TargetPed.Tasks.StandStill(-1);
            }

            MainMenu.Visible = true;
                
        }

        internal static void HighlightSelectedPed()
        {
            if(Targets.Selected && TargetPed)
            {
                Debug.DrawSphere(TargetPed.Position, TargetPed.Height * 0.55f, Color.FromArgb(100, Color.Yellow));
            }
        }

        private static void On_TargetChanged(UIMenu sender, UIMenuItem selectedItem)
        {
            if(selectedItem == Targets)
            {
                switch((string)Targets.SelectedValue)
                {
                    default:
                    case "Player":
                        TargetPed = Game.LocalPlayer.Character;
                        break;
                    case "Nearest Ped":
                        TargetPed = World.GetAllPeds().Where(p => p && p != Game.LocalPlayer.Character).OrderBy(p => p.DistanceTo(Game.LocalPlayer.Character)).FirstOrDefault();
                        if(!TargetPed)
                            Game.DisplayNotification("No peds found");
                        break;
                    case "Selected Ped":
                        GameFiber.StartNew(delegate { SelectPed(); });
                        break;
                }
            }
            // Game.DisplayNotification("~y~Sorry!~w~ Ped selection isn't working yet, all effects will be applied to the player.");
        }
    }
}
