using System;
using UnityEngine;

namespace SeroJob.UiSystem
{
    public class UIWindowTrack
    {
        private Action _reverseAction;

        public UIWindowTrack(Action reverseAction)
        {
            _reverseAction = reverseAction;
        }

        public void Reverse()
        {
            _reverseAction();
        }
    }
}