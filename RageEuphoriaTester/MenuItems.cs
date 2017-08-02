﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAGENativeUI.Elements
{
    using Rage;
    using Rage.Native;
    using RAGENativeUI;

    internal interface IMenuValueItem
    {
        object ItemValue { get; }
    }

    internal abstract class UIMenuValueEntrySelector<T> : UIMenuItem, IMenuValueItem
    {
        public UIMenuValueEntrySelector(string text, T value) : base(text)
        {
            this.Value = value;
            this.Activated += ActivatedHandler;
        }

        public UIMenuValueEntrySelector(string text, T value, string description) : base(text, description)
        {
            this.Value = value;
            this.Activated += ActivatedHandler;
        }

        private T _value;
        public T Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
            }
        }
        public object ItemValue => Value;
        protected virtual int MaxInputLength { get; } = 1000;
        protected abstract bool ValidateInput(string input, ref T value);
        protected virtual string DisplayMenu => Value.ToString();
        protected virtual string DisplayInputBox => Value.ToString();
        public override string RightLabel => DisplayMenu;

        protected virtual void ActivatedHandler(UIMenu sender, UIMenuItem selectedItem)
        {
            string input = GetUserInput(this.Text, DisplayInputBox, this.MaxInputLength);
            bool valid = input != null && ValidateInput(input, ref _value);
            if(!valid)
            {
                Game.DisplaySubtitle("The value ~b~" + input + "~w~ is ~r~invalid~w~ for property ~b~" + Text, 6000);
            }
        }

        private static string GetUserInput(string windowTitle, string defaultText, int maxLength)
        {
            NativeFunction.Natives.DISABLE_ALL_CONTROL_ACTIONS(2);

            NativeFunction.Natives.DISPLAY_ONSCREEN_KEYBOARD(true, windowTitle, 0, defaultText, 0, 0, 0, maxLength + 1);

            while (NativeFunction.Natives.UPDATE_ONSCREEN_KEYBOARD<int>() == 0)
            {
                GameFiber.Yield();
            }

            NativeFunction.Natives.ENABLE_ALL_CONTROL_ACTIONS(2);

            return NativeFunction.Natives.GET_ONSCREEN_KEYBOARD_RESULT<string>();
        }
    }

    internal class UIMenuStringSelector : UIMenuValueEntrySelector<string>
    {
        public UIMenuStringSelector(string text, string value) : base(text, value) { }
        public UIMenuStringSelector(string text, string value, string description) : base(text, value, description) { }

        protected override bool ValidateInput(string input, ref string value)
        {
            value = input;
            return true;
        }
    }

    internal class UIMenuIntSelector : UIMenuValueEntrySelector<int>
    {
        public UIMenuIntSelector(string text, int value) : base(text, value) { }
        public UIMenuIntSelector(string text, int value, string description) : base(text, value, description) { }

        protected override bool ValidateInput(string input, ref int value) => int.TryParse(input, out value);
    }

    internal class UIMenuFloatSelector : UIMenuValueEntrySelector<float>
    {
        public UIMenuFloatSelector(string text, float value) : base(text, value) { }
        public UIMenuFloatSelector(string text, float value, string description) : base(text, value, description) { }

        protected override bool ValidateInput(string input, ref float value) => float.TryParse(input, out value);
    }

    internal class UIMenuVector3Selector : UIMenuValueEntrySelector<Vector3>
    {
        public UIMenuVector3Selector(string text, Vector3 value) : base(text, value) { }
        public UIMenuVector3Selector(string text, Vector3 value, string description) : base(text, value, description) { }

        protected override string DisplayInputBox => string.Format("{0},{1},{2}", Value.X, Value.Y, Value.Z);

        protected override bool ValidateInput(string input, ref Vector3 value)
        {
            string[] inputs = input.Split(',');
            if (input.Length != 3) return false;

            float[] outputs = new float[3];
            bool success = false;
            for (int i = 0; i < inputs.Length; i++)
            {
                success = success && float.TryParse(inputs[i], out outputs[i]);
            }

            if(success)
            {
                value = new Vector3(outputs[0], outputs[1], outputs[2]);
            }

            return success;
        }

        
    }

    internal class UIMenuCheckBoxValueItem : UIMenuCheckboxItem, IMenuValueItem
    {
        public object ItemValue => this.Checked;

        public UIMenuCheckBoxValueItem(string text, bool check) : base(text, check) {}
        public UIMenuCheckBoxValueItem(string text, bool check, string description) : base(text, check, description) { }
    }
}
