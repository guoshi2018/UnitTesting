
using Android.Widget;
using System;

#nullable enable
namespace IsolatedItemViewAdapterUnitTesting
{
    internal class HelloUIButler : BaseUIButler
    {
        private bool _enabled;
        private Button? _btnEnable, _btnCall;
        private string _textEnable, _textDisable;
        private Action? _callback;
        private EventHandler __handler;

        public Button? BtnEnable { set { _btnEnable = value; } }
        public Button? BtnCall { set { _btnCall = value; } }
        public string TextEnable { set { _textEnable = value; } }
        public string TextDisable { set { _textDisable = value; } }

        public Action? Callback
        {
            set
            {
                _callback = value;
            }
        }

        public HelloUIButler(Button? btnSwitch = null, Button? btnCall = null,
            string textOpen = "", string textClose = "")
        {
            _btnEnable = btnSwitch;
            _btnCall = btnCall;
            _textEnable = textOpen;
            _textDisable = textClose;
            _enabled = false;
            __handler = delegate
            {
                _callback?.Invoke();
            };
        }

        private void __SwitchEnable(Object sender, EventArgs e)
        {
            Action action = _enabled switch
            {
                true => __Disable,
                false => __Enable,
            };
            action();
            _RefreshUI();
        }


        private void __Disable()
        {
            _enabled = false;
        }
        private void __Enable()
        {
            _enabled = true;
        }

        private void __ThrowIfAnyNullButton()
        {
            if (_btnEnable == null || _btnCall == null)
                throw new ArgumentNullException($"At least one button not assigned.");
        }

        protected override void _RefreshUI()
        {
            __ThrowIfAnyNullButton();
            (_btnEnable!.Text, _btnCall!.Enabled) = _enabled switch
            {
                true => (_textDisable, true),
                false => (_textEnable, false),
            };
        }

        protected override void _AddListeners()
        {
            __ThrowIfAnyNullButton();
            _btnEnable!.Click += __SwitchEnable;
            _btnCall!.Click += __handler;
        }

        protected override void _RemoveListeners()
        {
            __ThrowIfAnyNullButton();
            _btnEnable!.Click -= __SwitchEnable;
            _btnCall!.Click -= __handler;
        }

        protected override void _ReleaseResources()
        {
        }

        protected override void _InitStates()
        {
            _enabled = false;
        }
    }

    internal abstract class BaseUIButler : IDisposable
    {
        protected bool _isListenersReady;
        protected bool _isWorkerReady;

        /// <summary>
        /// 开工
        /// </summary>
        public void Work()
        {
            if (!_isWorkerReady)
            {
                _InitStates();
                _isWorkerReady = true;
            }
            if (!_isListenersReady)
            {
                _AddListeners();
                _isListenersReady = true;
            }
            _RefreshUI();
        }
        /// <summary>
        /// 休息
        /// </summary>
        public void Rest()
        {
            _isWorkerReady = false;
            if (_isListenersReady)
            {
                _RemoveListeners();
                _isListenersReady = false;
            }
            _RefreshUI();
        }
        /// <summary>
        /// 重新添加事件监听
        /// </summary>
        public void AddListenersAgain()
        {
            _RemoveListeners();
            _AddListeners();
        }

        protected void _Noop() { }

        protected abstract void _RefreshUI();

        protected abstract void _AddListeners();

        protected abstract void _RemoveListeners();
        protected abstract void _ReleaseResources();

        protected abstract void _InitStates();
        /// <summary>
        /// 释放
        /// </summary>
        public void Dispose()
        {
            _ReleaseResources();
        }
    }
}