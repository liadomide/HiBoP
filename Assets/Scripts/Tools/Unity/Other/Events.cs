﻿using System;
using UnityEngine;
using UnityEngine.Events;

namespace Tools.Unity
{
    [Serializable] public class BoolEvent : UnityEvent<Boolean> { }
    [Serializable] public class FloatEvent : UnityEvent<float> { }
    [Serializable] public class Vector2Event : UnityEvent<Vector2> { }
    [Serializable] public class Vector2ArrayEvent : UnityEvent<Vector2[]> {}
    [Serializable] public class StringEvent : UnityEvent<String> { }
    [Serializable] public class Texture2DEvent : UnityEvent<Texture2D> { }
}