using System;
using System.Reflection;

namespace GomokuConsole.Page
{
    public static class PageManager
    {
        private static IPage CurrentPage_ = null;

        public static void Update(float DeltaTime)
        {
            CurrentPage_?.Update(DeltaTime);
        }

        public static void Render()
        {
            CurrentPage_?.Render();
        }

        public static void OnKeyDown(ConsoleKey Key)
        {
            CurrentPage_?.KeyDown(Key);
        }

        public static void Change(IPage Page)
        {
            CurrentPage_?.Deinitialize();
            CurrentPage_ = Page;
            CurrentPage_?.Initialize();
        }

        public static void Change<T>() where T : IPage, new()
        {
            var Page = new T();
            Change(Page);
        }

        public static void Change<T>(params object[] Params) where T : IPage
        {
            var Page = (T)Activator.CreateInstance(typeof(T), Params);
            Change(Page);
        }
    }
}