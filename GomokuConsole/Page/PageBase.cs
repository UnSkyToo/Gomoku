using System;

namespace GomokuConsole.Page
{
    public abstract class PageBase : IPage
    {
        private bool IsRepaint_;

        protected PageBase()
        {
            Console.Title = "Gomoku Game";
            IsRepaint_ = true;
        }

        public void Initialize()
        {
            OnInitialize();
        }

        public void Deinitialize()
        {
            OnDeinitalize();
        }

        public void Update(float DeltaTime)
        {
            OnUpdate(DeltaTime);
        }

        public void Render()
        {
            if (!IsRepaint_)
            {
                return;
            }

            IsRepaint_ = false;
            Console.Clear();
            OnRender();
        }

        public void KeyDown(ConsoleKey Key)
        {
            OnKeyDown(Key);
        }

        public void Repaint()
        {
            IsRepaint_ = true;
        }

        protected abstract void OnInitialize();

        protected abstract void OnDeinitalize();

        protected abstract void OnUpdate(float DeltaTime);

        protected abstract void OnRender();

        protected abstract void OnKeyDown(ConsoleKey Key);
    }
}
