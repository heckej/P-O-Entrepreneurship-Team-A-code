﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.QnA;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using PenoBot.CognitiveModels;
using ClusterClient;
using ClusterClient.Models;

namespace PenoBot.Dialogs
{
	public class MainDialog : ComponentDialog
	{
		protected readonly ILogger Logger;
		private readonly IBotServices _botServices;
		public static Connector conchatbot = Globals.connector;
		protected readonly BotState UserState;
		public static string userid = Globals.userID;



		public MainDialog(String id/**ContactRecognizer contactRecognizer**/ /**ILogger<LuisContactDialog> logger*/, IBotServices botServices) :
base(id)
		{

			_botServices = botServices; 


			AddDialog(new TextPrompt(nameof(TextPrompt)));
			AddDialog(new LuisContactDialog(nameof(LuisContactDialog)));
			AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
			{
				IntroStepAsync,
				DispatchStepAsync, 
				FinalStepAsync
			}));

			InitialDialogId = nameof(WaterfallDialog);

		}

		private async Task<DialogTurnResult> IntroStepAsync(WaterfallStepContext stepContext,
			System.Threading.CancellationToken cancellationToken)
		{
			if ((string)stepContext.Options == "firstTime")
			{
				List<string> randomList = new List<string>(new String[] { "What can I do for you?",
					"What question do you have for me?", "What can I help you with?",
					"How may I help you?", "How can I be of service to you?"});
				Random r = new Random();
				var question = randomList[r.Next(randomList.Count)];
				var questionMsg = MessageFactory.Text(question, question, InputHints.ExpectingInput);
				return await stepContext.PromptAsync(nameof(TextPrompt),
					new PromptOptions() { Prompt = questionMsg }, cancellationToken);
			}
			else
			{
				List<string> randomList = new List<string>(new String[] { "What else can I do for you?",
					"Is there anything else I can help with?", "Do you have another question?", "What else can I help you with?", 
					"Can I help you with something else?"});
				Random r = new Random();
				var question = randomList[r.Next(randomList.Count)];
				var questionMsg = MessageFactory.Text(question, question, InputHints.ExpectingInput);
				return await stepContext.PromptAsync(nameof(TextPrompt),
					new PromptOptions() { Prompt = questionMsg }, cancellationToken);
			}
		}

		private async Task<DialogTurnResult> DispatchStepAsync(WaterfallStepContext stepContext,
			CancellationToken cancellationToken)
		{

			// Call LUIS and gather any potential booking details. (Note the TurnContext has the response to the prompt.)
			Debug.WriteLine(stepContext.Context);
			Debug.WriteLine(cancellationToken);
			var message = stepContext.Context;
			var qnaResult = await _botServices.QnAMakerService.GetAnswersAsync(message);

			var luisResult = await _botServices.LuisService.RecognizeAsync<LuisContactModel>(message, cancellationToken);

			var thresholdScore = 0.70;

			// Check if score is too low, then it is not understood.
			if ((luisResult.TopIntent().score < thresholdScore || (luisResult.TopIntent().score > thresholdScore && luisResult.TopIntent().intent == LuisContactModel.Intent.None)) &&
				(qnaResult.FirstOrDefault()?.Score*2 ?? 0) < thresholdScore)
			{
				var notUnderstood = "Going to send this to the forum, you will receive an answer later!";
				//var notUnderstoodMessage = MessageFactory.Text(notUnderstood, notUnderstood, InputHints.ExpectingInput);
				
				//sending to server
				try
				{
					
					ServerAnswer answer = await conchatbot.SendQuestionAsync(Globals.userID, message.Activity.Text);
					if (answer == null)
					{
						var askAgain = "Please ask your question again later, it was not possible to process it right now.";
						await stepContext.Context.SendActivityAsync(MessageFactory.Text(askAgain), cancellationToken);
						return await stepContext.NextAsync(null, cancellationToken);
					}
					else if (answer.answer_id < 0 || answer.answer == "")
					{
						await stepContext.Context.SendActivityAsync(MessageFactory.Text(notUnderstood), cancellationToken);
						return await stepContext.NextAsync(null, cancellationToken);
					}
					else
					{
						await stepContext.Context.SendActivityAsync(MessageFactory.Text(answer.answer), cancellationToken);
						return await stepContext.NextAsync(null, cancellationToken);
					}

				} catch(Exception e) {
					await stepContext.Context.SendActivityAsync(MessageFactory.Text(e.Message), cancellationToken);
					return await stepContext.NextAsync(null, cancellationToken);
				}

				//dit mag ook weg denk ik?
				await stepContext.Context.SendActivityAsync(MessageFactory.Text(notUnderstood), cancellationToken);
				return await stepContext.NextAsync(null, cancellationToken);
				//return await stepContext.PromptAsync(nameof(TextPrompt),
				//new PromptOptions() { Prompt = notUnderstoodMessage }, cancellationToken);
			}

			// Check on scores between Luis and Qna.
			if (luisResult.TopIntent().score >= (qnaResult.FirstOrDefault()?.Score ?? 0))
			{

				// Start the Luis Weather dialog.
				return await stepContext.BeginDialogAsync(nameof(LuisContactDialog), luisResult, cancellationToken);
			}

		
			else {
			// Show a Qna message.
			var qnaMessage = MessageFactory.Text(qnaResult.First().Answer, qnaResult.First().Answer,
				InputHints.ExpectingInput);

			await stepContext.Context.SendActivityAsync(qnaMessage, cancellationToken);
			return await stepContext.NextAsync(null, cancellationToken);

				
			}		
		}

		private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext,
			CancellationToken cancellationToken)
		{
			var msg = "What else can I do for you?";
			return await stepContext.ReplaceDialogAsync(InitialDialogId, msg, cancellationToken); 
		}

	}
}
