using Microsoft.CognitiveServices.Speech;

namespace Wobble.Services;

public class SpeechService
{
    private readonly SpeechConfig _speechConfig;

    public SpeechService(string key, string region, string voiceName = "")
    {
        _speechConfig = SpeechConfig.FromSubscription(key, region);

        _speechConfig.SpeechSynthesisVoiceName = 
            string.IsNullOrWhiteSpace(voiceName) 
                ? "en-US-JennyNeural" 
                : voiceName;
    }

    public async void SpeakAsync(string message)
    {
        using var speechSynthesizer = new SpeechSynthesizer(_speechConfig);
        var speechSynthesisResult = await speechSynthesizer.SpeakTextAsync(message);
    }
}