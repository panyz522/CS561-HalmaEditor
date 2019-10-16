using System;

namespace HalmaEditor.Data
{
    public class TitleData
    {
        public event EventHandler TitleUpdated;

        public string Title
        {
            get
            {
                return this.title;
            }
            set
            {
                this.title = value;
                TitleUpdated?.Invoke(this, EventArgs.Empty);
            }
        }

        private string title = "Halma Board Editor";
    }
}
