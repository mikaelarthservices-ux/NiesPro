using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using CustomerService.Domain.Entities;
using CustomerService.Domain.Repositories;
using CustomerService.Application.DTOs.SegmentInteraction;
using CustomerService.Application.Queries.SegmentInteraction;
using CustomerService.Domain.ValueObjects;
using CustomerService.Domain.Enums;

namespace CustomerService.Application.Handlers.Queries
{
    // ========================================================================================
    // SEGMENT QUERY HANDLERS - RÉCUPÉRATION SOPHISTIQUÉE DES DONNÉES DE SEGMENTATION
    // ========================================================================================

    /// <summary>
    /// Handler pour évaluer l'appartenance d'un client à des segments
    /// </summary>
    public class EvaluateCustomerSegmentsQueryHandler : IRequestHandler<EvaluateCustomerSegmentsQuery, List<SegmentEvaluationDto>>
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly ICustomerSegmentRepository _segmentRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<EvaluateCustomerSegmentsQueryHandler> _logger;
        private readonly IValidator<EvaluateCustomerSegmentsQuery> _validator;

        public EvaluateCustomerSegmentsQueryHandler(
            ICustomerRepository customerRepository,
            ICustomerSegmentRepository segmentRepository,
            IMapper mapper,
            ILogger<EvaluateCustomerSegmentsQueryHandler> logger,
            IValidator<EvaluateCustomerSegmentsQuery> validator)
        {
            _customerRepository = customerRepository;
            _segmentRepository = segmentRepository;
            _mapper = mapper;
            _logger = logger;
            _validator = validator;
        }

        public async Task<List<SegmentEvaluationDto>> Handle(EvaluateCustomerSegmentsQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Évaluation des segments pour le client : {CustomerId}", request.CustomerId);

            // Validation
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                _logger.LogError("Échec de validation pour EvaluateCustomerSegments : {Errors}", errors);
                throw new ValidationException($"Erreurs de validation : {errors}");
            }

            // Récupération du client
            var customer = await _customerRepository.GetByIdWithDetailsAsync(request.CustomerId);
            if (customer == null)
            {
                _logger.LogWarning("Client non trouvé : {CustomerId}", request.CustomerId);
                return new List<SegmentEvaluationDto>();
            }

            // Récupération des segments actifs
            var segments = await _segmentRepository.GetActiveSegmentsAsync();

            var evaluations = new List<SegmentEvaluationDto>();

            foreach (var segment in segments)
            {
                var evaluation = new SegmentEvaluationDto
                {
                    SegmentId = segment.Id,
                    SegmentName = segment.Name,
                    SegmentDescription = segment.Description,
                    CustomerId = customer.Id,
                    EvaluationDate = DateTime.UtcNow
                };

                try
                {
                    // Évaluation des critères
                    var meetsaCriteria = segment.EvaluateCriteria(customer);
                    var currentlyInSegment = customer.IsInSegment(segment.Id);

                    evaluation.MeetsCriteria = meetsaCriteria;
                    evaluation.CurrentlyInSegment = currentlyInSegment;
                    evaluation.RecommendedAction = DetermineRecommendedAction(meetsaCriteria, currentlyInSegment);
                    evaluation.ConfidenceScore = CalculateConfidenceScore(segment, customer);
                    evaluation.CriteriaDetails = EvaluateCriteriaDetails(segment, customer);

                    // Calcul du score de correspondance
                    evaluation.MatchScore = CalculateMatchScore(segment, customer);

                    // Prédictions d'évolution
                    if (request.IncludePredictions)
                    {
                        evaluation.PredictedStability = PredictSegmentStability(segment, customer);
                        evaluation.RiskOfLeaving = CalculateRiskOfLeaving(segment, customer);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erreur lors de l'évaluation du segment {SegmentId} pour le client {CustomerId}", 
                        segment.Id, customer.Id);
                    
                    evaluation.MeetsCriteria = false;
                    evaluation.ErrorMessage = ex.Message;
                }

                evaluations.Add(evaluation);
            }

            // Tri par score de correspondance
            evaluations = evaluations.OrderByDescending(e => e.MatchScore).ToList();

            _logger.LogInformation("Évaluation des segments terminée - {Count} segments évalués pour le client {CustomerId}", 
                evaluations.Count, customer.Id);

            return evaluations;
        }

        private string DetermineRecommendedAction(bool meetsCriteria, bool currentlyInSegment)
        {
            return (meetsCriteria, currentlyInSegment) switch
            {
                (true, false) => "Ajouter au segment",
                (false, true) => "Retirer du segment",
                (true, true) => "Maintenir dans le segment",
                (false, false) => "Aucune action requise"
            };
        }

        private double CalculateConfidenceScore(CustomerSegment segment, Customer customer)
        {
            // Score de confiance basé sur la qualité des données et la stabilité des critères
            var dataQualityScore = customer.GetDataQualityScore();
            var criteriaStabilityScore = segment.GetCriteriaStabilityScore();
            
            return (dataQualityScore + criteriaStabilityScore) / 2.0;
        }

        private List<CriteriaEvaluationDto> EvaluateCriteriaDetails(CustomerSegment segment, Customer customer)
        {
            var details = new List<CriteriaEvaluationDto>();

            foreach (var criterion in segment.GetIndividualCriteria())
            {
                var result = criterion.Evaluate(customer);
                details.Add(new CriteriaEvaluationDto
                {
                    CriterionName = criterion.Name,
                    CriterionDescription = criterion.Description,
                    IsMet = result.IsMet,
                    ActualValue = result.ActualValue?.ToString(),
                    ExpectedValue = result.ExpectedValue?.ToString(),
                    Weight = criterion.Weight,
                    Impact = result.IsMet ? "Positive" : "Negative"
                });
            }

            return details;
        }

        private double CalculateMatchScore(CustomerSegment segment, Customer customer)
        {
            var criteriaDetails = EvaluateCriteriaDetails(segment, customer);
            if (!criteriaDetails.Any()) return 0.0;

            var weightedScore = criteriaDetails
                .Sum(c => (c.IsMet ? 1.0 : 0.0) * c.Weight) / 
                criteriaDetails.Sum(c => c.Weight);

            return Math.Min(1.0, weightedScore);
        }

        private double PredictSegmentStability(CustomerSegment segment, Customer customer)
        {
            // Prédiction de la stabilité de l'appartenance au segment
            var historicalStability = customer.GetSegmentStabilityHistory(segment.Id);
            var trendScore = customer.GetBehaviorTrendScore();
            
            return (historicalStability + trendScore) / 2.0;
        }

        private double CalculateRiskOfLeaving(CustomerSegment segment, Customer customer)
        {
            // Calcul du risque de sortie du segment
            if (!customer.IsInSegment(segment.Id)) return 0.0;

            var criteriaStrength = CalculateMatchScore(segment, customer);
            var behaviorStability = customer.GetBehaviorStabilityScore();
            
            return 1.0 - ((criteriaStrength + behaviorStability) / 2.0);
        }
    }

    /// <summary>
    /// Handler pour récupérer les clients d'un segment
    /// </summary>
    public class GetSegmentCustomersQueryHandler : IRequestHandler<GetSegmentCustomersQuery, PagedResult<CustomerSummaryDto>>
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly ICustomerSegmentRepository _segmentRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetSegmentCustomersQueryHandler> _logger;
        private readonly IValidator<GetSegmentCustomersQuery> _validator;

        public GetSegmentCustomersQueryHandler(
            ICustomerRepository customerRepository,
            ICustomerSegmentRepository segmentRepository,
            IMapper mapper,
            ILogger<GetSegmentCustomersQueryHandler> logger,
            IValidator<GetSegmentCustomersQuery> validator)
        {
            _customerRepository = customerRepository;
            _segmentRepository = segmentRepository;
            _mapper = mapper;
            _logger = logger;
            _validator = validator;
        }

        public async Task<PagedResult<CustomerSummaryDto>> Handle(GetSegmentCustomersQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Récupération des clients du segment : {SegmentId}", request.SegmentId);

            // Validation
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                _logger.LogError("Échec de validation pour GetSegmentCustomers : {Errors}", errors);
                throw new ValidationException($"Erreurs de validation : {errors}");
            }

            // Vérification de l'existence du segment
            var segment = await _segmentRepository.GetByIdAsync(request.SegmentId);
            if (segment == null)
            {
                _logger.LogWarning("Segment non trouvé : {SegmentId}", request.SegmentId);
                return new PagedResult<CustomerSummaryDto> { Items = new List<CustomerSummaryDto>() };
            }

            // Récupération des clients du segment
            var (customers, totalCount) = await _customerRepository.GetSegmentCustomersAsync(
                request.SegmentId,
                request.PageNumber,
                request.PageSize,
                request.SortBy,
                request.SortDescending);

            var customerDtos = _mapper.Map<List<CustomerSummaryDto>>(customers);

            // Enrichissement avec informations de segment
            if (request.IncludeSegmentDetails)
            {
                foreach (var dto in customerDtos)
                {
                    var customer = customers.First(c => c.Id == dto.Id);
                    dto.SegmentMembership = GetCustomerSegmentDetails(customer, segment);
                }
            }

            // Calcul des métriques de segment si demandé
            var segmentMetrics = request.IncludeMetrics ? await CalculateSegmentMetrics(request.SegmentId, customers) : null;

            var result = new PagedResult<CustomerSummaryDto>
            {
                Items = customerDtos,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize),
                Metadata = segmentMetrics != null ? new Dictionary<string, object> { ["SegmentMetrics"] = segmentMetrics } : null
            };

            _logger.LogInformation("Clients du segment récupérés : {Count} sur {Total}", customerDtos.Count, totalCount);

            return result;
        }

        private CustomerSegmentMembershipDto GetCustomerSegmentDetails(Customer customer, CustomerSegment segment)
        {
            var membership = customer.GetSegmentMembership(segment.Id);
            return new CustomerSegmentMembershipDto
            {
                SegmentId = segment.Id,
                SegmentName = segment.Name,
                JoinedDate = membership?.JoinedDate ?? DateTime.UtcNow,
                AssignmentReason = membership?.Reason ?? "Critères automatiques",
                IsAutoAssigned = segment.AutoAssign,
                MatchScore = segment.EvaluateMatchScore(customer)
            };
        }

        private async Task<SegmentMetricsDto> CalculateSegmentMetrics(Guid segmentId, IEnumerable<Customer> customers)
        {
            return new SegmentMetricsDto
            {
                TotalMembers = customers.Count(),
                AverageAge = customers.Where(c => c.PersonalInfo.DateOfBirth.HasValue)
                    .Average(c => (DateTime.Now - c.PersonalInfo.DateOfBirth.Value).TotalDays / 365.25),
                GenderDistribution = customers.Where(c => c.PersonalInfo.Gender.HasValue)
                    .GroupBy(c => c.PersonalInfo.Gender.Value)
                    .ToDictionary(g => g.Key.ToString(), g => g.Count()),
                AverageLoyaltyPoints = customers.Average(c => c.LoyaltyStats.TotalPoints),
                LoyaltyTierDistribution = customers.GroupBy(c => c.LoyaltyStats.CurrentTier)
                    .ToDictionary(g => g.Key, g => g.Count()),
                AverageCustomerValue = customers.Average(c => c.CalculateLifetimeValue()),
                TopCities = customers.Where(c => !string.IsNullOrEmpty(c.ContactInfo.City))
                    .GroupBy(c => c.ContactInfo.City)
                    .OrderByDescending(g => g.Count())
                    .Take(5)
                    .ToDictionary(g => g.Key, g => g.Count())
            };
        }
    }

    /// <summary>
    /// Handler pour rafraîchir les segments
    /// </summary>
    public class RefreshSegmentQueryHandler : IRequestHandler<RefreshSegmentQuery, SegmentRefreshResultDto>
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly ICustomerSegmentRepository _segmentRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<RefreshSegmentQueryHandler> _logger;
        private readonly IValidator<RefreshSegmentQuery> _validator;

        public RefreshSegmentQueryHandler(
            ICustomerRepository customerRepository,
            ICustomerSegmentRepository segmentRepository,
            IMapper mapper,
            ILogger<RefreshSegmentQueryHandler> logger,
            IValidator<RefreshSegmentQuery> validator)
        {
            _customerRepository = customerRepository;
            _segmentRepository = segmentRepository;
            _mapper = mapper;
            _logger = logger;
            _validator = validator;
        }

        public async Task<SegmentRefreshResultDto> Handle(RefreshSegmentQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Rafraîchissement du segment : {SegmentId}", request.SegmentId);

            // Validation
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                _logger.LogError("Échec de validation pour RefreshSegment : {Errors}", errors);
                throw new ValidationException($"Erreurs de validation : {errors}");
            }

            // Récupération du segment
            var segment = await _segmentRepository.GetByIdAsync(request.SegmentId);
            if (segment == null)
            {
                _logger.LogWarning("Segment non trouvé : {SegmentId}", request.SegmentId);
                return null;
            }

            var result = new SegmentRefreshResultDto
            {
                SegmentId = segment.Id,
                SegmentName = segment.Name,
                StartTime = DateTime.UtcNow,
                Success = true
            };

            try
            {
                // Analyse en mode simulation d'abord
                var allCustomers = await _customerRepository.GetAllActiveAsync();
                var currentMembers = await _customerRepository.GetSegmentCustomersAsync(request.SegmentId);
                
                var analysisResult = AnalyzeSegmentChanges(segment, allCustomers.ToList(), currentMembers.customers.ToList());

                result.CurrentMemberCount = currentMembers.totalCount;
                result.EligibleCustomersCount = analysisResult.EligibleCustomers.Count;
                result.CustomersToAdd = analysisResult.CustomersToAdd.Count;
                result.CustomersToRemove = analysisResult.CustomersToRemove.Count;
                result.Changes = analysisResult.Changes;

                // Calcul de l'impact
                result.ImpactAssessment = CalculateImpactAssessment(analysisResult);

                // Si ce n'est qu'une simulation, on s'arrête ici
                if (request.SimulationOnly)
                {
                    result.SimulationOnly = true;
                    result.EndTime = DateTime.UtcNow;
                    
                    _logger.LogInformation("Simulation de rafraîchissement terminée - Segment : {SegmentId}, Ajouts : {ToAdd}, Suppressions : {ToRemove}", 
                        request.SegmentId, result.CustomersToAdd, result.CustomersToRemove);
                    
                    return result;
                }

                // Application des changements réels si demandé
                if (result.CustomersToAdd > 0 || result.CustomersToRemove > 0)
                {
                    await ApplySegmentChanges(segment, analysisResult);
                    
                    // Mise à jour des statistiques du segment
                    segment.UpdateStatistics(result.CustomersToAdd, result.CustomersToRemove);
                    segment.MarkAsRefreshed();
                    await _segmentRepository.UpdateAsync(segment);
                }

                result.EndTime = DateTime.UtcNow;

                _logger.LogInformation("Rafraîchissement du segment terminé - Segment : {SegmentId}, Succès : {Success}", 
                    request.SegmentId, result.Success);
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = ex.Message;
                result.EndTime = DateTime.UtcNow;

                _logger.LogError(ex, "Erreur lors du rafraîchissement du segment : {SegmentId}", request.SegmentId);
            }

            return result;
        }

        private SegmentAnalysisResult AnalyzeSegmentChanges(CustomerSegment segment, List<Customer> allCustomers, List<Customer> currentMembers)
        {
            var result = new SegmentAnalysisResult
            {
                Changes = new List<SegmentChangeDto>()
            };

            var currentMemberIds = currentMembers.Select(c => c.Id).ToHashSet();

            foreach (var customer in allCustomers)
            {
                var meetsaCriteria = segment.EvaluateCriteria(customer);
                var isCurrentMember = currentMemberIds.Contains(customer.Id);

                if (meetsaCriteria && !isCurrentMember)
                {
                    result.CustomersToAdd.Add(customer);
                    result.Changes.Add(new SegmentChangeDto
                    {
                        CustomerId = customer.Id,
                        CustomerName = $"{customer.PersonalInfo.FirstName} {customer.PersonalInfo.LastName}",
                        ChangeType = "Add",
                        Reason = "Critères maintenant remplis",
                        MatchScore = segment.EvaluateMatchScore(customer),
                        ImpactLevel = "Medium"
                    });
                }
                else if (!meetsaCriteria && isCurrentMember)
                {
                    result.CustomersToRemove.Add(customer);
                    result.Changes.Add(new SegmentChangeDto
                    {
                        CustomerId = customer.Id,
                        CustomerName = $"{customer.PersonalInfo.FirstName} {customer.PersonalInfo.LastName}",
                        ChangeType = "Remove",
                        Reason = "Critères non remplis",
                        MatchScore = segment.EvaluateMatchScore(customer),
                        ImpactLevel = DetermineImpactLevel(customer)
                    });
                }
                else if (meetsaCriteria)
                {
                    result.EligibleCustomers.Add(customer);
                }
            }

            return result;
        }

        private string DetermineImpactLevel(Customer customer)
        {
            var customerValue = customer.CalculateLifetimeValue();
            return customerValue switch
            {
                > 1000 => "High",
                > 500 => "Medium",
                _ => "Low"
            };
        }

        private SegmentImpactAssessmentDto CalculateImpactAssessment(SegmentAnalysisResult analysisResult)
        {
            return new SegmentImpactAssessmentDto
            {
                TotalChanges = analysisResult.Changes.Count,
                HighImpactChanges = analysisResult.Changes.Count(c => c.ImpactLevel == "High"),
                EstimatedValueImpact = analysisResult.Changes
                    .Where(c => c.ChangeType == "Add")
                    .Sum(c => c.EstimatedValue ?? 0),
                RecommendedCommunication = GenerateCommunicationRecommendations(analysisResult),
                RiskAssessment = AssessRefreshRisks(analysisResult)
            };
        }

        private async Task ApplySegmentChanges(CustomerSegment segment, SegmentAnalysisResult analysisResult)
        {
            // Application des ajouts
            foreach (var customer in analysisResult.CustomersToAdd)
            {
                customer.AssignToSegment(segment.Id, segment.Name, "Assignation automatique - critères remplis");
                await _customerRepository.UpdateAsync(customer);
            }

            // Application des suppressions
            foreach (var customer in analysisResult.CustomersToRemove)
            {
                customer.RemoveFromSegment(segment.Id, "Suppression automatique - critères non remplis");
                await _customerRepository.UpdateAsync(customer);
            }
        }

        private List<string> GenerateCommunicationRecommendations(SegmentAnalysisResult analysisResult)
        {
            var recommendations = new List<string>();

            if (analysisResult.CustomersToAdd.Count > 0)
                recommendations.Add($"Envoyer un message de bienvenue aux {analysisResult.CustomersToAdd.Count} nouveaux membres");

            if (analysisResult.CustomersToRemove.Count > 0)
                recommendations.Add($"Campagne de rétention pour les {analysisResult.CustomersToRemove.Count} clients qui quittent le segment");

            return recommendations;
        }

        private List<string> AssessRefreshRisks(SegmentAnalysisResult analysisResult)
        {
            var risks = new List<string>();

            var highValueRemovals = analysisResult.Changes
                .Where(c => c.ChangeType == "Remove" && c.ImpactLevel == "High")
                .Count();

            if (highValueRemovals > 0)
                risks.Add($"Perte de {highValueRemovals} clients à haute valeur");

            var majorChanges = (double)(analysisResult.CustomersToAdd.Count + analysisResult.CustomersToRemove.Count) / 
                             Math.Max(1, analysisResult.EligibleCustomers.Count + analysisResult.CustomersToAdd.Count);

            if (majorChanges > 0.2)
                risks.Add("Changements majeurs (>20%) dans la composition du segment");

            return risks;
        }
    }

    // ========================================================================================
    // INTERACTION QUERY HANDLERS - RÉCUPÉRATION DES DONNÉES D'INTERACTION CLIENT
    // ========================================================================================

    /// <summary>
    /// Handler pour récupérer l'historique des interactions d'un client
    /// </summary>
    public class GetInteractionHistoryQueryHandler : IRequestHandler<GetInteractionHistoryQuery, PagedResult<CustomerInteractionDto>>
    {
        private readonly ICustomerInteractionRepository _interactionRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetInteractionHistoryQueryHandler> _logger;
        private readonly IValidator<GetInteractionHistoryQuery> _validator;

        public GetInteractionHistoryQueryHandler(
            ICustomerInteractionRepository interactionRepository,
            IMapper mapper,
            ILogger<GetInteractionHistoryQueryHandler> logger,
            IValidator<GetInteractionHistoryQuery> validator)
        {
            _interactionRepository = interactionRepository;
            _mapper = mapper;
            _logger = logger;
            _validator = validator;
        }

        public async Task<PagedResult<CustomerInteractionDto>> Handle(GetInteractionHistoryQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Récupération de l'historique des interactions - Client : {CustomerId}", request.CustomerId);

            // Validation
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                _logger.LogError("Échec de validation pour GetInteractionHistory : {Errors}", errors);
                throw new ValidationException($"Erreurs de validation : {errors}");
            }

            // Récupération des interactions
            var (interactions, totalCount) = await _interactionRepository.GetByCustomerIdAsync(
                request.CustomerId,
                request.StartDate,
                request.EndDate,
                request.InteractionTypes,
                request.Channels,
                request.PageNumber,
                request.PageSize,
                request.SortBy,
                request.SortDescending);

            var interactionDtos = _mapper.Map<List<CustomerInteractionDto>>(interactions);

            // Calcul des statistiques si demandé
            var summary = request.IncludeSummary ? await CalculateInteractionSummary(interactions, request.CustomerId) : null;

            var result = new PagedResult<CustomerInteractionDto>
            {
                Items = interactionDtos,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize),
                Summary = summary
            };

            _logger.LogInformation("Historique des interactions récupéré : {Count} sur {Total}", interactionDtos.Count, totalCount);

            return result;
        }

        private async Task<InteractionSummaryDto> CalculateInteractionSummary(IEnumerable<CustomerInteraction> interactions, Guid customerId)
        {
            var interactionList = interactions.ToList();
            
            return new InteractionSummaryDto
            {
                CustomerId = customerId,
                TotalInteractions = interactionList.Count,
                InteractionsByType = interactionList.GroupBy(i => i.InteractionType)
                    .ToDictionary(g => g.Key.ToString(), g => g.Count()),
                InteractionsByChannel = interactionList.GroupBy(i => i.Channel)
                    .ToDictionary(g => g.Key.ToString(), g => g.Count()),
                InteractionsByOutcome = interactionList.GroupBy(i => i.Outcome)
                    .ToDictionary(g => g.Key.ToString(), g => g.Count()),
                AverageRating = interactionList.Where(i => i.Rating.HasValue).Average(i => i.Rating) ?? 0,
                AverageDuration = interactionList.Where(i => i.Duration.HasValue).Average(i => i.Duration.Value),
                FirstInteractionDate = interactionList.Min(i => i.InteractionDate),
                LastInteractionDate = interactionList.Max(i => i.InteractionDate),
                TrendAnalysis = CalculateInteractionTrends(interactionList)
            };
        }

        private InteractionTrendsDto CalculateInteractionTrends(List<CustomerInteraction> interactions)
        {
            var now = DateTime.UtcNow;
            var currentMonth = interactions.Where(i => i.InteractionDate >= now.AddMonths(-1));
            var previousMonth = interactions.Where(i => i.InteractionDate >= now.AddMonths(-2) && i.InteractionDate < now.AddMonths(-1));

            return new InteractionTrendsDto
            {
                FrequencyTrend = CalculateGrowthRate(currentMonth.Count(), previousMonth.Count()),
                SatisfactionTrend = CalculateGrowthRate(
                    currentMonth.Where(i => i.Rating.HasValue).Average(i => i.Rating) ?? 0,
                    previousMonth.Where(i => i.Rating.HasValue).Average(i => i.Rating) ?? 0)
            };
        }

        private double CalculateGrowthRate(double current, double previous)
        {
            if (previous == 0) return current > 0 ? 100 : 0;
            return ((current - previous) / previous) * 100;
        }
    }

    /// <summary>
    /// Handler pour rechercher des interactions
    /// </summary>
    public class SearchInteractionsQueryHandler : IRequestHandler<SearchInteractionsQuery, PagedResult<CustomerInteractionDto>>
    {
        private readonly ICustomerInteractionRepository _interactionRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<SearchInteractionsQueryHandler> _logger;
        private readonly IValidator<SearchInteractionsQuery> _validator;

        public SearchInteractionsQueryHandler(
            ICustomerInteractionRepository interactionRepository,
            IMapper mapper,
            ILogger<SearchInteractionsQueryHandler> logger,
            IValidator<SearchInteractionsQuery> validator)
        {
            _interactionRepository = interactionRepository;
            _mapper = mapper;
            _logger = logger;
            _validator = validator;
        }

        public async Task<PagedResult<CustomerInteractionDto>> Handle(SearchInteractionsQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Recherche d'interactions - Terme : '{SearchTerm}'", request.SearchTerm);

            // Validation
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                _logger.LogError("Échec de validation pour SearchInteractions : {Errors}", errors);
                throw new ValidationException($"Erreurs de validation : {errors}");
            }

            // Construction des critères de recherche
            var searchCriteria = new InteractionSearchCriteria
            {
                SearchTerm = request.SearchTerm,
                CustomerId = request.CustomerId,
                InteractionType = request.InteractionType,
                Channel = request.Channel,
                Outcome = request.Outcome,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                MinRating = request.MinRating,
                MaxRating = request.MaxRating,
                AgentId = request.AgentId,
                Tags = request.Tags,
                HasFollowUp = request.HasFollowUp
            };

            // Exécution de la recherche
            var (interactions, totalCount) = await _interactionRepository.SearchAsync(
                searchCriteria,
                request.PageNumber,
                request.PageSize,
                request.SortBy,
                request.SortDescending);

            var interactionDtos = _mapper.Map<List<CustomerInteractionDto>>(interactions);

            var result = new PagedResult<CustomerInteractionDto>
            {
                Items = interactionDtos,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
            };

            _logger.LogInformation("Recherche d'interactions terminée : {Count} sur {Total}", interactionDtos.Count, totalCount);

            return result;
        }
    }

    /// <summary>
    /// Handler pour récupérer les interactions nécessitant un suivi
    /// </summary>
    public class GetPendingFollowUpsQueryHandler : IRequestHandler<GetPendingFollowUpsQuery, PagedResult<CustomerInteractionDto>>
    {
        private readonly ICustomerInteractionRepository _interactionRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetPendingFollowUpsQueryHandler> _logger;
        private readonly IValidator<GetPendingFollowUpsQuery> _validator;

        public GetPendingFollowUpsQueryHandler(
            ICustomerInteractionRepository interactionRepository,
            IMapper mapper,
            ILogger<GetPendingFollowUpsQueryHandler> logger,
            IValidator<GetPendingFollowUpsQuery> validator)
        {
            _interactionRepository = interactionRepository;
            _mapper = mapper;
            _logger = logger;
            _validator = validator;
        }

        public async Task<PagedResult<CustomerInteractionDto>> Handle(GetPendingFollowUpsQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Récupération des suivis en attente - Agent : {AgentId}", request.AgentId);

            // Validation
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                throw new ValidationException($"Erreurs de validation : {string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))}");
            }

            // Récupération des suivis en attente
            var (interactions, totalCount) = await _interactionRepository.GetPendingFollowUpsAsync(
                request.AgentId,
                request.DueBefore,
                request.Priority,
                request.PageNumber,
                request.PageSize,
                request.SortBy,
                request.SortDescending);

            var interactionDtos = _mapper.Map<List<CustomerInteractionDto>>(interactions);

            // Enrichissement avec priorités calculées
            foreach (var dto in interactionDtos)
            {
                var interaction = interactions.First(i => i.Id == dto.Id);
                dto.CalculatedPriority = CalculateFollowUpPriority(interaction);
                dto.UrgencyLevel = DetermineUrgencyLevel(interaction);
                dto.SuggestedActions = GenerateFollowUpSuggestions(interaction);
            }

            // Tri par priorité calculée
            interactionDtos = interactionDtos.OrderByDescending(i => i.CalculatedPriority).ToList();

            var result = new PagedResult<CustomerInteractionDto>
            {
                Items = interactionDtos,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
            };

            _logger.LogInformation("Suivis en attente récupérés : {Count} sur {Total}", interactionDtos.Count, totalCount);

            return result;
        }

        private int CalculateFollowUpPriority(CustomerInteraction interaction)
        {
            var priority = 0;

            // Urgence basée sur la date d'échéance
            if (interaction.FollowUpDate.HasValue)
            {
                var daysUntilDue = (interaction.FollowUpDate.Value - DateTime.UtcNow).TotalDays;
                if (daysUntilDue < 0) priority += 10; // En retard
                else if (daysUntilDue < 1) priority += 8; // Aujourd'hui
                else if (daysUntilDue < 3) priority += 5; // Dans 3 jours
            }

            // Priorité basée sur le type d'interaction
            priority += interaction.InteractionType switch
            {
                InteractionType.Complaint => 7,
                InteractionType.Support => 5,
                InteractionType.Sales => 3,
                _ => 1
            };

            // Priorité basée sur la satisfaction
            if (interaction.Rating.HasValue && interaction.Rating < 3)
                priority += 5;

            return priority;
        }

        private string DetermineUrgencyLevel(CustomerInteraction interaction)
        {
            var priority = CalculateFollowUpPriority(interaction);
            
            return priority switch
            {
                >= 15 => "Critical",
                >= 10 => "High",
                >= 5 => "Medium",
                _ => "Low"
            };
        }

        private List<string> GenerateFollowUpSuggestions(CustomerInteraction interaction)
        {
            var suggestions = new List<string>();

            if (interaction.InteractionType == InteractionType.Complaint)
            {
                suggestions.Add("Vérifier la résolution du problème");
                suggestions.Add("S'assurer de la satisfaction du client");
            }

            if (interaction.Rating.HasValue && interaction.Rating < 3)
            {
                suggestions.Add("Proposer un geste commercial");
                suggestions.Add("Escalader vers un superviseur");
            }

            if (interaction.FollowUpDate.HasValue && interaction.FollowUpDate < DateTime.UtcNow)
            {
                suggestions.Add("Contact immédiat requis - suivi en retard");
            }

            return suggestions;
        }
    }

    /// <summary>
    /// Handler pour les statistiques d'interactions
    /// </summary>
    public class GetInteractionStatsQueryHandler : IRequestHandler<GetInteractionStatsQuery, InteractionStatsDto>
    {
        private readonly ICustomerInteractionRepository _interactionRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetInteractionStatsQueryHandler> _logger;
        private readonly IValidator<GetInteractionStatsQuery> _validator;

        public GetInteractionStatsQueryHandler(
            ICustomerInteractionRepository interactionRepository,
            IMapper mapper,
            ILogger<GetInteractionStatsQueryHandler> logger,
            IValidator<GetInteractionStatsQuery> validator)
        {
            _interactionRepository = interactionRepository;
            _mapper = mapper;
            _logger = logger;
            _validator = validator;
        }

        public async Task<InteractionStatsDto> Handle(GetInteractionStatsQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Calcul des statistiques d'interactions - Période : {StartDate} à {EndDate}", 
                request.StartDate, request.EndDate);

            // Validation
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                _logger.LogError("Échec de validation pour GetInteractionStats : {Errors}", errors);
                throw new ValidationException($"Erreurs de validation : {errors}");
            }

            // Calcul des statistiques
            var stats = new InteractionStatsDto
            {
                GeneratedAt = DateTime.UtcNow,
                PeriodStart = request.StartDate,
                PeriodEnd = request.EndDate,
                
                // Statistiques de base
                TotalInteractions = await _interactionRepository.GetTotalCountAsync(request.StartDate, request.EndDate),
                InteractionsByType = await _interactionRepository.GetInteractionsByTypeAsync(request.StartDate, request.EndDate),
                InteractionsByChannel = await _interactionRepository.GetInteractionsByChannelAsync(request.StartDate, request.EndDate),
                InteractionsByOutcome = await _interactionRepository.GetInteractionsByOutcomeAsync(request.StartDate, request.EndDate),
                
                // Métriques de qualité
                AverageSatisfactionRating = await _interactionRepository.GetAverageSatisfactionAsync(request.StartDate, request.EndDate),
                AverageResolutionTime = await _interactionRepository.GetAverageResolutionTimeAsync(request.StartDate, request.EndDate),
                FirstContactResolutionRate = await _interactionRepository.GetFirstContactResolutionRateAsync(request.StartDate, request.EndDate),
                
                // Métriques d'agent
                TopPerformingAgents = await _interactionRepository.GetTopPerformingAgentsAsync(request.StartDate, request.EndDate, 10),
                AverageHandlingTime = await _interactionRepository.GetAverageHandlingTimeAsync(request.StartDate, request.EndDate)
            };

            // Calcul des tendances si demandé
            if (request.IncludeTrends)
            {
                stats.Trends = await CalculateInteractionTrends(request.StartDate, request.EndDate);
            }

            // Analyse comparative si demandée
            if (request.IncludeComparison)
            {
                stats.Comparison = await CalculateComparisonMetrics(request.StartDate, request.EndDate);
            }

            _logger.LogInformation("Statistiques d'interactions calculées - Total : {Total}, Satisfaction moyenne : {Satisfaction}", 
                stats.TotalInteractions, stats.AverageSatisfactionRating);

            return stats;
        }

        private async Task<InteractionTrendsDto> CalculateInteractionTrends(DateTime startDate, DateTime endDate)
        {
            var previousPeriodStart = startDate.AddDays(-(endDate - startDate).TotalDays);
            
            var currentStats = await _interactionRepository.GetPeriodStatsAsync(startDate, endDate);
            var previousStats = await _interactionRepository.GetPeriodStatsAsync(previousPeriodStart, startDate);

            return new InteractionTrendsDto
            {
                VolumeTrend = CalculateGrowthRate(currentStats.TotalInteractions, previousStats.TotalInteractions),
                SatisfactionTrend = CalculateGrowthRate(currentStats.AverageSatisfaction, previousStats.AverageSatisfaction),
                ResolutionTimeTrend = CalculateGrowthRate(currentStats.AverageResolutionTime, previousStats.AverageResolutionTime)
            };
        }

        private async Task<InteractionComparisonDto> CalculateComparisonMetrics(DateTime startDate, DateTime endDate)
        {
            // Comparaison avec la même période l'année précédente
            var yearAgoStart = startDate.AddYears(-1);
            var yearAgoEnd = endDate.AddYears(-1);
            
            var currentStats = await _interactionRepository.GetPeriodStatsAsync(startDate, endDate);
            var yearAgoStats = await _interactionRepository.GetPeriodStatsAsync(yearAgoStart, yearAgoEnd);

            return new InteractionComparisonDto
            {
                VolumeComparison = CalculateGrowthRate(currentStats.TotalInteractions, yearAgoStats.TotalInteractions),
                SatisfactionComparison = CalculateGrowthRate(currentStats.AverageSatisfaction, yearAgoStats.AverageSatisfaction),
                EfficiencyComparison = CalculateGrowthRate(currentStats.AverageResolutionTime, yearAgoStats.AverageResolutionTime)
            };
        }

        private double CalculateGrowthRate(double current, double previous)
        {
            if (previous == 0) return current > 0 ? 100 : 0;
            return ((current - previous) / previous) * 100;
        }
    }

    // Classes internes pour l'analyse
    internal class SegmentAnalysisResult
    {
        public List<Customer> EligibleCustomers { get; set; } = new();
        public List<Customer> CustomersToAdd { get; set; } = new();
        public List<Customer> CustomersToRemove { get; set; } = new();
        public List<SegmentChangeDto> Changes { get; set; } = new();
    }
}