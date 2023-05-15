namespace WavePlayer.GUI
{
    public class ComboBoxSelectionItem<VALUE_T>
    {
        public ComboBoxSelectionItem(string text, VALUE_T value)
        {
            Text = text;
            Value = value;
        }

        public string Text { get; }
        public VALUE_T Value { get; }
    }
}
