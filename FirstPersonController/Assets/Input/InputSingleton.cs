namespace Input {
    public partial class MyInput {
        private static MyInput _instance;

        public static MyInput Instance {
            get {
                if (_instance == null) {
                    _instance = new MyInput();
                    _instance.Enable();
                }

                return _instance;
            }
        }
    }
}