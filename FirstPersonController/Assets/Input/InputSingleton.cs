namespace Input {
    public partial class InputAsset {
        private static @InputAsset _instance;

        public static @InputAsset Instance {
            get {
                if (_instance == null) {
                    _instance = new @InputAsset();
                    _instance.Enable();
                }

                return _instance;
            }
        }
    }
}