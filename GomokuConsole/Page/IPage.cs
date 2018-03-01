using System;

namespace GomokuConsole.Page
{
    public interface IPage
    {
        void Initialize();

        void Deinitialize();

        void Update(float DeltaTime);

        void Render();

        void KeyDown(ConsoleKey Key);
    }
}
