namespace ConsoleMenu
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    public class ConsoleMenu<T> where T : struct, IConvertible, IComparable, IFormattable
    {
        public Stream OutputStream { get; set; }

        public Stream InputStream { get; set; }

        public string Header { get; set; }

        public string Delimiter { get; set; }

        public T? SelectedMenuEntry { get; private set; }

        private readonly Dictionary<T, Action> actions;

        public ConsoleMenu(Stream outputStream, Stream inputStream)
        {
            if (!typeof(T).IsEnum)
            {
                throw new TypeAccessException("Type '" + typeof(T).Name + "' must be of type Enum!");
            }

            this.OutputStream = outputStream;
            this.InputStream = inputStream;

            this.Header = "Please choose an option:";
            this.Delimiter = "-";

            this.actions = new Dictionary<T, Action>();
        }

        public void SetAction(T menuEntry, Action action)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action", "Parameter action must not be null!");
            }

            if (this.actions.ContainsKey(menuEntry))
            {
                this.actions[menuEntry] = action;
                return;
            }

            this.actions.Add(menuEntry, action);
        }

        public void ShowMenu(bool parseUnderlineToWhitespace = true)
        {
            this.CheckStreams();

            this.ShowMenuUntilValidSelectionIsMade(parseUnderlineToWhitespace);

            this.PerformActionForSelectedEntry();
        }

        private void PerformActionForSelectedEntry()
        {
            if (this.SelectedMenuEntry != null)
            {
                if (this.actions.ContainsKey(this.SelectedMenuEntry.Value))
                {
                    this.actions[this.SelectedMenuEntry.Value]();
                }
            }
        }

        private void ShowMenuUntilValidSelectionIsMade(bool parseUnderlineToWhitespace)
        {
            var failed = true;

            var streamWriter = new StreamWriter(this.OutputStream) { AutoFlush = true };

            do
            {
                failed = !this.TryShowMenu(parseUnderlineToWhitespace, streamWriter);

                if (failed)
                {
                    this.DisplayErrorMessage(streamWriter);
                }
            }
            while (failed);            
        }

        private bool TryShowMenu(bool parseUnderlineToWhitespace, StreamWriter streamWriter)
        {
            try
            {
                this.DisplayMenu(streamWriter, parseUnderlineToWhitespace);

                this.SelectedMenuEntry = this.GetInput();
                streamWriter.WriteLine();

                return true;
            }
            catch (InvalidDataException)
            {
                return false;
            }
        }

        private void DisplayErrorMessage(StreamWriter streamWriter)
        {
            streamWriter.WriteLine();
            streamWriter.WriteLine("No valid selection! Please try again: ");
            streamWriter.WriteLine();
        }

        private T? GetInput()
        {
            var streamReader = new StreamReader(this.InputStream);
            var input = streamReader.ReadLine();
            var selectedMenuEntry = -1;

            if (input.ToLower().Equals("x"))
            {
                return null;
            }

            if (int.TryParse(input, out selectedMenuEntry) && Enum.IsDefined(typeof(T), selectedMenuEntry))
            {
                return (T)Enum.Parse(typeof(T), selectedMenuEntry.ToString());
            }

            throw new InvalidDataException("No valid selection!");
        }

        private void DisplayMenu(StreamWriter streamWriter, bool parseUnderlineToWhitespace = true)
        {
            var memberInfos =
                typeof(T).GetMembers(BindingFlags.Public | BindingFlags.Static)
                         .OrderBy(info => (int)Enum.Parse(typeof(T), info.Name));

            streamWriter.WriteLine();
            streamWriter.WriteLine(this.Header);
            streamWriter.WriteLine();

            foreach (var memberInfo in memberInfos)
            {
                var menuEntryText = memberInfo.Name;
                var menuEntryNumber = (int)Enum.Parse(typeof(T), memberInfo.Name);

                if (parseUnderlineToWhitespace)
                {
                    menuEntryText = menuEntryText.Replace('_', ' ');
                }

                streamWriter.WriteLine("{0} {1} {2}", menuEntryNumber, this.Delimiter, menuEntryText);
            }

            streamWriter.WriteLine("X " + this.Delimiter + " Exit");
            streamWriter.WriteLine();
        }

        private void CheckStreams()
        {
            if (this.InputStream == null)
            {
                throw new NullReferenceException("InputStream must be a valid stream!");
            }

            if (this.OutputStream == null)
            {
                throw new NullReferenceException("OutputStream must be a valid stream!");
            }
        }
    }
}
