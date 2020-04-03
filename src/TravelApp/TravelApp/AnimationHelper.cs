using System;
using System.Collections.Generic;
using System.Text;

namespace TravelApp
{

    public enum State
    {
        Collapsed,
        Expanded
    }

    public enum AnimationKey
    {
        Cell,
        Expand,
        FlyUp,
        Circle,
    }


    public class AnimationValues
    {
        public double Current { get; set; }
        public Dictionary<State, double> States { get; set; } = new Dictionary<State, double>();
    }


    public class AnimationHelper : Dictionary<AnimationKey, AnimationValues>
    {
        public double GetStateValue(AnimationKey key, State state)
        {
            return this[key].States[state];
        }

        public void SetStateValue(AnimationKey key, State state, double value)
        {
            AnimationValues stateValue = new AnimationValues();
            stateValue.States = new Dictionary<State, double>();
            stateValue.States[state] = value;

            if (!this.ContainsKey(key))
                this.Add(key, stateValue);
            else
                this[key].States[state] = value;

        }

    }
}
