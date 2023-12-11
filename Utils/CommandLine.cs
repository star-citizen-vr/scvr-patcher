namespace SCVRPatcher.Utils {

    public class CommandLineParser {
        private readonly List<string> _args;

        public CommandLineParser(string[] args) {
            _args = args.ToList();
        }

        public string? GetStringArgument(string key, char? shortKey = null) {
            var index = _args.IndexOf("--" + key);

            if (index >= 0 && _args.Count > index) {
                return _args[index + 1];
            }

            if (shortKey != null) {
                index = _args.IndexOf("-" + shortKey);

                if (index >= 0 && _args.Count > index) {
                    return _args[index + 1];
                }
            }

            return null;
        }

        public bool GetSwitchArgument(string value, char? shortKey = null) {
            return _args.Contains("--" + value) || _args.Contains("-" + shortKey);
        }
    }
}