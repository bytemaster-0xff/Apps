using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.Media.SpeechRecognition;

namespace LagoVista.ManCaveController.Models
{
    public class ParsedVoiceCommandResult
    {
        public string CommandName { get; set; }
        public VoiceCommandMode CommandMode { get; set; }
        public string InterpretedText { get; set; }
        public SpeechRecognitionResult RawResult { get; set; }
        public Dictionary<string, List<string>> SemanticInterpretations { get; set; }

        public ParsedVoiceCommandResult(IActivatedEventArgs args)
        {
            SemanticInterpretations = new Dictionary<string, List<string>>();

            var commandArgs = args as VoiceCommandActivatedEventArgs;
            RawResult = commandArgs.Result;

            GetAllSemanticInterpretations();

            // get the command elements we need
            CommandMode = DetermineVoiceCommandMode(SemanticInterpretation("commandMode"));
            CommandName = RawResult.RulePath[0];
            InterpretedText = RawResult.Text;
        }

        private void GetAllSemanticInterpretations()
        {
            foreach (var key in RawResult.SemanticInterpretation.Properties.Keys)
            {
                SemanticInterpretations.Add(key, RawResult.SemanticInterpretation.Properties[key].ToList());
            }
        }

        private string SemanticInterpretation(string key)
        {
            var result = new StringBuilder();

            if (SemanticInterpretations.ContainsKey(key))
            {
                var values = SemanticInterpretations.First(f => f.Key == key).Value;
                foreach (var value in values)
                {
                    if (value != values.First()) { result.Append(","); }
                    result.Append(value);
                }
            }
            else
            {
                result.Append("UNKNOWN");
            }

            return result.ToString();
        }

        private VoiceCommandMode DetermineVoiceCommandMode(string mode)
        {
            var knownVoiceLabels = new[] { "VOICE" };
            if (knownVoiceLabels.Contains(mode.ToUpperInvariant()))
            {
                return VoiceCommandMode.Voice;
            }
            return VoiceCommandMode.Text;
        }
    }

    public enum VoiceCommandMode
    {
        Voice = 0,
        Text = 1
    }
}
