namespace Services {
    public interface IServiceLocator {
        T GetService<T>() where T : class;
        void RegisterService<T>(T service) where T : class;
        void UnregisterService<T>(T service) where T : class;
        void UnregisterService(System.Type type);
        bool HasService<T>() where T : class;
    }
}