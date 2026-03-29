using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CassMach.Application.Common.Interfaces
{
    public class ParseResult
    {
        public string Brand { get; set; }
        public string Model { get; set; }
        public string ErrorCode { get; set; }
        public string Symptom { get; set; }
        public bool IsValid { get; set; }
    }

    public class StreamResult
    {
        public string Text { get; set; }
        public bool IsDone { get; set; }
        public int InputTokens { get; set; }
        public int OutputTokens { get; set; }
    }

    public interface IClaudeService
    {
        Task<ParseResult> ParseUserQuestion(string question, CancellationToken cancellationToken = default);

        IAsyncEnumerable<StreamResult> StreamSolution(
            string question, string brand, string model, string errorCode, string symptom,
            CancellationToken cancellationToken = default);

        IAsyncEnumerable<StreamResult> StreamRetrySolution(
            string question, string brand, string model, string errorCode, string symptom,
            List<string> previousResponses,
            CancellationToken cancellationToken = default);
    }
}
