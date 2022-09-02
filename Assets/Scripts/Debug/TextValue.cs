using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DebugElements
{
    public class TextValue : MonoBehaviour
    {
        [TextArea(1,60)]
        public string Value;
    }
}


