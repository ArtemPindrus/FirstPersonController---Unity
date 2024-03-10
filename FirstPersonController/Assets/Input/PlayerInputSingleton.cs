using UnityEngine;

public class PlayerInputSingleton
{
    private static PlayerInput instance = null;

    public static PlayerInput Instance {
        get {
            if (instance == null) {
                instance = new PlayerInput();
                instance.Enable();
            }
            return instance;
        }
    }

    private PlayerInputSingleton() { }
}
