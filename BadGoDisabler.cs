
using System;
using UnityEngine;

namespace VRCPlusPet
{
    internal class BadGoDisabler : MonoBehaviour
    {
        public BadGoDisabler(IntPtr obj0) : base(obj0)
        {
        }

        void OnEnable() => this.gameObject.SetActive(false);
    }
}
