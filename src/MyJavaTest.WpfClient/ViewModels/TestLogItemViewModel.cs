using MyJavaTest.WpfClient.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyJavaTest.WpfClient.ViewModels
{
    public class TestLogItemViewModel : ViewModel
    {
        private string _value = string.Empty;
        public string Value { get => _value; set => Set(ref _value, value); }

        private TestLogItemState _state = TestLogItemState.Success;
        public TestLogItemState State { get => _state; set => Set(ref _state, value); }

        public TestLogItemViewModel(string value, TestLogItemState state)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
            State = state;
        }

        public override string ToString() => Value;
    }

    public enum TestLogItemState
    {
        Success, Error
    }
}
