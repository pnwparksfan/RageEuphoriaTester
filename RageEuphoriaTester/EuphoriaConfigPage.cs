using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Drawing;
using RAGENativeUI;
using RAGENativeUI.Elements;
using Rage;
using Rage.Euphoria;
using Rage.Native;

namespace RageEuphoriaTester
{
    internal class EuphoriaConfigPage
    {
        public EuphoriaMessage Message { get; private set; }

        public UIMenu MenuPage { get; private set; }
        public UIMenuItem SendTo { get; private set; }
        public UIMenuItem Reset { get; private set; }
        public UIMenuItem OpenMenu { get; private set; }

        public EuphoriaConfigPage(EuphoriaMessage message)
        {
            Message = message;
            _setUpMenu();
        }

        private Dictionary<UIMenuItem, PropertyInfo> Items = new Dictionary<UIMenuItem, PropertyInfo>();

        private void _setUpMenu()
        {
            if (Message == null) return;

            Game.LogTrivial("Setting up menu for " + Message.Name);
            
            /*
            string desc = "";
            var descAttr = Message.GetType().GetCustomAttributes(typeof(EuphoriaDetailAttribute), false).Cast<EuphoriaDetailAttribute>().FirstOrDefault();
            if (descAttr != null)
                desc = descAttr.Description;
                */

            MenuPage = new UIMenu("RAGE Euphoria Tester", Message.Name);

            Game.LogTrivial("  Adding SendTo and Reset items");
            SendTo = new UIMenuItem("Send To...", "Send to the selected ped");
            Reset = new UIMenuItem("Reset", "Resets the message");
            MenuPage.AddItem(SendTo);
            MenuPage.AddItem(Reset);
            SendTo.Activated += On_SendTo;
            Reset.Activated += On_Reset;
            SendTo.BackColor = Color.DarkGreen;
            SendTo.ForeColor = Color.White;
            SendTo.HighlightedBackColor = Color.LightGreen;
            SendTo.HighlightedForeColor = Color.Black;
            Reset.BackColor = Color.DarkOliveGreen;
            Reset.ForeColor = Color.White;
            Reset.HighlightedBackColor = Color.LightGreen;
            Reset.HighlightedForeColor = Color.Black;

            var properties = Message.GetType().GetProperties();

            foreach (var property in properties)
            {
                UIMenuItem item = null;

                Game.LogTrivial("  Checking property " + property.Name);
                Game.LogTrivial("    Type: " + property.PropertyType.Name);

                string desc = "Configure " + property.Name;
                var attribute = property.GetCustomAttributes(typeof(EuphoriaDetailAttribute), false).Cast<EuphoriaDetailAttribute>().FirstOrDefault();
                if (attribute != null)
                {
                    desc = attribute.Description;
                }

                Game.LogTrivial("    Description: " + desc);

                if (property.PropertyType == typeof(bool))
                {
                    item = new UIMenuCheckBoxValueItem(property.Name, (bool)property.GetValue(Message), desc);
                } else if(property.PropertyType == typeof(int))
                {
                    item = new UIMenuIntSelector(property.Name, (int)property.GetValue(Message), desc);
                } else if(property.PropertyType == typeof(float))
                {
                    item = new UIMenuFloatSelector(property.Name, (float)property.GetValue(Message), desc);
                } else if(property.PropertyType == typeof(string))
                {
                    item = new UIMenuStringSelector(property.Name, (string)property.GetValue(Message), desc);
                } else if(property.PropertyType == typeof(Vector3))
                {
                    item = new UIMenuVector3Selector(property.Name, (Vector3)property.GetValue(Message), desc);
                } else
                {
                    Game.LogTrivial("    Could not find a control for type");
                }

                if(item is IMenuValueItem)
                {
                    IMenuValueItem valItem = item as IMenuValueItem;
                    Game.LogTrivial("    Registered item " + item.Text);
                    item.Activated += PropertyChanged;
                    MenuPage.AddItem(item);
                    Items.Add(item, property);
                    
                } else
                {
                    Game.LogTrivial("    Could not register item " + item.Text);
                }   
            }



            OpenMenu = new UIMenuItem(Message.Name);
            Menus.MessageMenu.AddItem(OpenMenu);
            Menus.MessageMenu.BindMenuToItem(MenuPage, OpenMenu);
            Menus.menuPool.Add(MenuPage);

            MenuPage.RefreshIndex();
        }

        private void On_Reset(UIMenu sender, UIMenuItem selectedItem)
        {
            Message.Reset();
        }

        private void On_SendTo(UIMenu sender, UIMenuItem selectedItem)
        {
            if(Menus.TargetPed)
            {
                Game.DisplaySubtitle("Sending message to ped", 6000);
                Game.LogTrivial("Sending " + Message.Name + " to ped " + Menus.TargetPed.Model.Name);
                foreach (PropertyInfo prop in Message.GetType().GetProperties())
                {
                    Game.LogTrivial("Property " + prop.Name + " is set to " + prop.GetValue(Message).ToString());
                }
                NativeFunction.Natives.SET_PED_TO_RAGDOLL(Menus.TargetPed, 4000, 5000, 1, 1, 1, 0);
                Message.SendTo(Menus.TargetPed);
            } else
            {
                Game.DisplaySubtitle("No ped is selected", 6000);
            }
            
        }

        private void PropertyChanged(UIMenu sender, UIMenuItem selectedItem)
        {
            PropertyInfo prop = null;
            bool success = Items.TryGetValue(selectedItem, out prop);

            if(!success)
            {
                Game.LogTrivial("Could not change value of " + selectedItem.Text);
                return;
            }

            if(!(selectedItem is IMenuValueItem))
            {
                Game.LogTrivial(selectedItem.Text + " is not an IMenuValueItem");
                return;
            }

            var valueItem = selectedItem as IMenuValueItem;
            prop.SetValue(Message, valueItem.ItemValue);

            Game.LogTrivial("Set " + selectedItem.Text + " to " + valueItem.ItemValue.ToString());
        }
    }
}
