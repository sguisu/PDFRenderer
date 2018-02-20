using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Actors.Client;
using PDFRendererActorSupervisorService.Interfaces;
using PDFRendererActorProcessorService.Interfaces;
using PDFRendererModels;

namespace PDFRendererActorSupervisorService
{
    [ActorService(Name = "PDFRendererActorSupervisorService")]
    /// <remarks>
    /// This class represents an actor.
    /// Every ActorID maps to an instance of this class.
    /// The StatePersistence attribute determines persistence and replication of actor state:
    ///  - Persisted: State is written to disk and replicated.
    ///  - Volatile: State is kept in memory only and replicated.
    ///  - None: State is kept in memory only and not replicated.
    /// </remarks>
    [StatePersistence(StatePersistence.Persisted)]
    internal class PDFRendererActorSupervisorService : Actor, IPDFRendererActorSupervisorService
    {
        private const int ActorsLimit = 20;
        private readonly Uri _processorActorUri = new Uri("fabric:/PDFRenderer/PDFRendererActorProcessorService");

        /// <summary>
        /// Initializes a new instance of PDFRendererActorSupervisorService
        /// </summary>
        /// <param name="actorService">The Microsoft.ServiceFabric.Actors.Runtime.ActorService that will host this actor instance.</param>
        /// <param name="actorId">The Microsoft.ServiceFabric.Actors.ActorId for this actor instance.</param>
        public PDFRendererActorSupervisorService(ActorService actorService, ActorId actorId)
            : base(actorService, actorId)
        {
        }

        public async Task StartProcessingAsync(Message<string[]> message)
        {
            try
            {
                var result = await StateManager.TryGetStateAsync<CancellationTokenSource>(message.Id.ToString());
                if (result.HasValue)
                {
                    ActorEventSource.Current.Message($"SupervisorActor=[{Id}] is already processing MessageId=[{message.Id}].");
                    return;
                }
                var cancellationTokenSource = new CancellationTokenSource();
                await StateManager.TryAddStateAsync(message.Id.ToString(), cancellationTokenSource, cancellationTokenSource.Token);
                foreach (var chunkedCollection in message.Payload.Chunk(ActorsLimit))
                {
                    var processingActorProxy = ActorProxy.Create<IPDFRendererActorProcessorService>(ActorId.CreateRandom(), _processorActorUri);
                    await processingActorProxy.ProcessAsync(Id.ToString(), new Message<string[]>
                    {
                        Id = Guid.NewGuid(),
                        Payload = chunkedCollection.ToArray()
                    }, cancellationTokenSource.Token);
                }
            }
            catch (Exception exception)
            {
                ActorEventSource.Current.Message($"Error occured in SupervisorActor=[{Id}]: {exception}");
            }
        }
    }
}
