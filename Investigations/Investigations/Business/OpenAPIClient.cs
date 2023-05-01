using OpenAI.GPT3;
using OpenAI.GPT3.Managers;
using OpenAI.GPT3.ObjectModels.RequestModels;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Investigations.Business
{
	public class OpenAPIClient
	{
		private OpenAIService _client;
		public OpenAPIClient()
		{
			_client = new OpenAIService(
				new OpenAiOptions()
				{
					ApiKey = "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"
				});
		}

		public async Task<string> CreateCompletions(string prompt)
		{
			var result = await _client.Completions.CreateCompletion(new CompletionCreateRequest() { Prompt = prompt, Model = OpenAI.GPT3.ObjectModels.Models.TextDavinciV3 });

			if (result.Successful)
			{
				return result.Choices.Select(choice => choice.Text).FirstOrDefault(string.Empty);
			}
			else
			{
				return "Error";
			}
		}

		public string[] EmbeddedResources { get; } = typeof(MainPage).Assembly.GetManifestResourceNames();
		public async Task<string> TranscribeAudio(string filename = "audio-sample-1.mp3")
		{
			var assembly = GetType().Assembly;
			// Get sample image
			string audioFile = EmbeddedResources.First(item => item.EndsWith(filename));
			var audioFileStream = assembly.GetManifestResourceStream(audioFile);
			if(audioFileStream == null)
			{
				return "Error no audio";
			}
			var audioFileBytes = await audioFileStream.ReadBytesAsync();
			var audioTranscriptionRequest = new AudioCreateTranscriptionRequest() 
			{
				FileName = filename,
				File = audioFileBytes,
				Model = OpenAI.GPT3.ObjectModels.Models.WhisperV1,
				ResponseFormat = OpenAI.GPT3.ObjectModels.StaticValues.AudioStatics.ResponseFormat.Text
			};
			var audioResult = await _client.Audio.CreateTranscription(audioTranscriptionRequest);

			if (audioResult.Successful)
			{
				return string.Join("\n", audioResult.Text);
			}
			else
			{
				return "Error";
			}
		}
	}
}
