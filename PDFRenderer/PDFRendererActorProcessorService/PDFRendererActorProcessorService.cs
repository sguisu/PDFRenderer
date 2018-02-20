using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Actors.Client;
using PDFRendererActorProcessorService.Interfaces;
using PDFRendererModels;
using PDFRendererHelpers;

namespace PDFRendererActorProcessorService
{
    [ActorService(Name = "PDFRendererActorProcessorService")]
    /// <remarks>
    /// This class represents an actor.
    /// Every ActorID maps to an instance of this class.
    /// The StatePersistence attribute determines persistence and replication of actor state:
    ///  - Persisted: State is written to disk and replicated.
    ///  - Volatile: State is kept in memory only and replicated.
    ///  - None: State is kept in memory only and not replicated.
    /// </remarks>
    [StatePersistence(StatePersistence.Volatile)]
    internal class PDFRendererActorProcessorService : Actor, IPDFRendererActorProcessorService, IRemindable
    {
        private const string RedisKey = "Test";
        private const string RedisConnectionString = "127.0.0.1:6379";

        /// <summary>
        /// Initializes a new instance of PDFRendererActorProcessorService
        /// </summary>
        /// <param name="actorService">The Microsoft.ServiceFabric.Actors.Runtime.ActorService that will host this actor instance.</param>
        /// <param name="actorId">The Microsoft.ServiceFabric.Actors.ActorId for this actor instance.</param>
        public PDFRendererActorProcessorService(ActorService actorService, ActorId actorId)
            : base(actorService, actorId)
        {
        }

        protected override async Task OnActivateAsync()
        {
            await base.OnActivateAsync();
            ActorEventSource.Current.ActorMessage(this, "Actor activated.");
        }

        protected override async Task OnDeactivateAsync()
        {
            await base.OnDeactivateAsync();
        }

        public async Task ProcessAsync(string supervisorId, Message<string[]> message, CancellationToken cancellationToken)
        {
            try
            {
                await StateManager.TryAddStateAsync(message.Id.ToString(), message, cancellationToken);
                await StateManager.TryAddStateAsync(Id.ToString(), cancellationToken, cancellationToken);
                await RegisterReminderAsync(message.Id.ToString(), null, TimeSpan.Zero, TimeSpan.FromMilliseconds(-1));
            }
            catch (Exception exception)
            {
                ActorEventSource.Current.ActorHostInitializationFailed(exception.ToString());
            }
        }

        public async Task ReceiveReminderAsync(string reminderName, byte[] state, TimeSpan dueTime, TimeSpan period)
        {
            try
            {
                GetReminder(reminderName);
                var reminder = GetReminder(reminderName);
                if (reminder != null)
                {
                    await UnregisterReminderAsync(reminder);
                }

                var message = await StateManager.TryGetStateAsync<Message<string[]>>(reminderName);
                if (message.HasValue)
                {
                    foreach (var value in message.Value.Payload)
                    {
                        var stream = Helpers.LoadHTMLStreamFromUrl(value);
                        var pdf = Helpers.CreatePDFFromStream(stream, string.Empty);
                        var result = Helpers.SavePDFFile(pdf, string.Format(@"C:\Temp\PDFRenderer\{0}", value.ToString()));
                    }
                }
                else
                {
                    ActorEventSource.Current.ActorMessage(this, "Received message has no value");
                }
            }
            catch (Exception exception)
            {
                ActorEventSource.Current.ActorHostInitializationFailed($"Error occured in ProcessorActor=[{Id}]: {exception}");
            }
        }
    }
}
