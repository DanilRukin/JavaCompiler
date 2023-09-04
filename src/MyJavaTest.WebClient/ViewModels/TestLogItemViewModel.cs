using MyJavaTest.WebClient.Models;

namespace MyJavaTest.WebClient.ViewModels
{
    public class TestLogItemViewModel
    {
        public string Value { get; set; } = string.Empty;
        public TestLogItemState State { get; set; } = TestLogItemState.Success;

        public TestLogItemViewModel(string value, TestLogItemState state)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
            State = state;
        }

        public override string ToString() => Value;
    }
}
