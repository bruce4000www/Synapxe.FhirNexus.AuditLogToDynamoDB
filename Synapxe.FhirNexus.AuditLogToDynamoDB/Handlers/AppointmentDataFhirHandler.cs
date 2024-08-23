// -------------------------------------------------------------------------------------------------
// Copyright (c) Integrated Health Information Systems Pte Ltd. All rights reserved.
// -------------------------------------------------------------------------------------------------

using Hl7.Fhir.Model;
using Ihis.FhirEngine.Core;
using Ihis.FhirEngine.Core.Data;
using Ihis.FhirEngine.Core.Exceptions;
using Ihis.FhirEngine.Core.Models;
using Ihis.FhirEngine.Core.Search;
using Task = System.Threading.Tasks.Task;

namespace Synapxe.FhirNexus.AuditLogToDynamoDB.Handlers
{
    [FhirHandlerClass(AcceptedType = nameof(Appointment))]
    public class AppointmentDataFhirHandler
    {
        private readonly IDataService dataStore;
        private readonly ISearchService searchService;
        private readonly ILogger<AppointmentDataFhirHandler> logger; // Added ILogger field

        public AppointmentDataFhirHandler(
            IDataService<Appointment> dataStore,
            ISearchService<Appointment> searchService,
            ILogger<AppointmentDataFhirHandler> logger) // Added ILogger parameter
        {
            this.dataStore = dataStore;
            this.searchService = searchService;
            this.logger = logger; // Assigned logger parameter to field
        }

        [FhirHandler(FhirInteractionType.OperationInstance, CustomOperation = "cancel")]
        public async Task<Appointment> CancelAsync(
            ResourceKey resourceKey,
            CodeableConcept? cancellationReason = default,
            CancellationToken cancellationToken = default)
        {
            var appointment = await dataStore.GetAsync<Appointment>(resourceKey, cancellationToken);

            if (appointment != null)
            {
                appointment.CancellationReason = cancellationReason;
                appointment.Status = Appointment.AppointmentStatus.Cancelled;
                appointment.Meta.LastUpdated = DateTimeOffset.UtcNow;
                await dataStore.UpsertAsync(appointment, WeakETag.FromVersionId(appointment.VersionId), false, true, false, cancellationToken);
                return appointment;
            }
            else
            {
                throw new ResourceNotFoundException(resourceKey);
            }
        }

        [FhirHandler("ValidateNoAppointmentConflictOnCreate", HandlerCategory.PreCRUD, FhirInteractionType.Create)]
        public async Task ValidateNoAppointmentConflictAsync(Appointment appointment, CancellationToken cancellationToken)
        {
            logger.LogInformation("Validating appointment conflict");
            // Check that all participants have no other appointments for the same time
            var participantIds = appointment.Participant.Select(x => x.Actor.Reference).ToList();

            var searchParams = new List<(string, string)>
            {
                ("_summary", "count"), // we fetch count only
                ("date", appointment.StartElement.ToString()!),
                ("actor", string.Join(',', participantIds)),
            };

            var result = await searchService.SearchAsync("Appointment", searchParams.ToArray(), false, cancellationToken);
            if (result.TotalCount > 0)
            {
                throw new ResourceNotValidException("Appointment participant has another appointment for that date.");
            }
        }
    }
}
