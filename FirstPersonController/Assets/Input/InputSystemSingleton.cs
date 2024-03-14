using System;
using UnityEngine.InputSystem;

public partial class @InputSystem : IInputActionCollection2, IDisposable {
    private static @InputSystem instance = null;

    public static @InputSystem Instance {
        get {
            if (instance == null) {
                instance = new @InputSystem();
                instance.Enable();
            }
            return instance;
        }
    }
}
