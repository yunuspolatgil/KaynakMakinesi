using System;

namespace KaynakMakinesi.Core.Abstractions
{
    public interface ISettingsStore<T>
    {
        T Load();
        void Save(T settings);
        event EventHandler SettingsChanged;
    }
}