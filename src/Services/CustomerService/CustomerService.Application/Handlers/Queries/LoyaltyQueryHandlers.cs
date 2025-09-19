using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using CustomerService.Domain.Entities;
using CustomerService.Domain.Repositories;
using CustomerService.Application.DTOs.Loyalty;
using CustomerService.Application.Queries.Loyalty;
using CustomerService.Domain.ValueObjects;
using CustomerService.Domain.Enums;

namespace CustomerService.Application.Handlers.Queries
{
    // ========================================================================================
    // LOYALTY QUERY HANDLERS - RÉCUPÉRATION SOPHISTIQUÉE DES DONNÉES DE FIDÉLITÉ
    // ========================================================================================

    /// <summary>
    /// Handler pour récupérer tous les programmes de fidélité
    /// </summary>
    public class GetLoyaltyProgramsQueryHandler : IRequestHandler<GetLoyaltyProgramsQuery, PagedResult<LoyaltyProgramDto>>
    {
        private readonly ILoyaltyProgramRepository _loyaltyProgramRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetLoyaltyProgramsQueryHandler> _logger;
        private readonly IValidator<GetLoyaltyProgramsQuery> _validator;

        public GetLoyaltyProgramsQueryHandler(
            ILoyaltyProgramRepository loyaltyProgramRepository,
            IMapper mapper,
            ILogger<GetLoyaltyProgramsQueryHandler> logger,
            IValidator<GetLoyaltyProgramsQuery> validator)
        {
            _loyaltyProgramRepository = loyaltyProgramRepository;
            _mapper = mapper;
            _logger = logger;
            _validator = validator;
        }

        public async Task<PagedResult<LoyaltyProgramDto>> Handle(GetLoyaltyProgramsQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Récupération des programmes de fidélité - Actifs seulement : {ActiveOnly}", request.ActiveOnly);

            // Validation
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                _logger.LogError("Échec de validation pour GetLoyaltyPrograms : {Errors}", errors);
                throw new ValidationException($"Erreurs de validation : {errors}");
            }

            // Récupération des programmes
            var (programs, totalCount) = await _loyaltyProgramRepository.GetAllAsync(
                request.ActiveOnly,
                request.PageNumber,
                request.PageSize,
                request.SortBy,
                request.SortDescending);

            var programDtos = _mapper.Map<List<LoyaltyProgramDto>>(programs);

            // Enrichissement avec statistiques si demandé
            if (request.IncludeStatistics)
            {
                foreach (var dto in programDtos)
                {
                    var program = programs.First(p => p.Id == dto.Id);
                    dto.Statistics = await GetProgramStatistics(program.Id);
                }
            }

            var result = new PagedResult<LoyaltyProgramDto>
            {
                Items = programDtos,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
            };

            _logger.LogInformation("Programmes de fidélité récupérés : {Count} sur {Total}", programDtos.Count, totalCount);

            return result;
        }

        private async Task<LoyaltyProgramStatisticsDto> GetProgramStatistics(Guid programId)
        {
            var stats = await _loyaltyProgramRepository.GetProgramStatisticsAsync(programId);
            return new LoyaltyProgramStatisticsDto
            {
                TotalMembers = stats.TotalMembers,
                ActiveMembers = stats.ActiveMembers,
                TotalPointsIssued = stats.TotalPointsIssued,
                TotalPointsRedeemed = stats.TotalPointsRedeemed,
                AveragePointsPerMember = stats.AveragePointsPerMember,
                TopTierDistribution = stats.TierDistribution
            };
        }
    }

    /// <summary>
    /// Handler pour récupérer les statistiques de fidélité d'un client
    /// </summary>
    public class GetCustomerLoyaltyStatsQueryHandler : IRequestHandler<GetCustomerLoyaltyStatsQuery, CustomerLoyaltyStatsDto>
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly ILoyaltyProgramRepository _loyaltyProgramRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetCustomerLoyaltyStatsQueryHandler> _logger;
        private readonly IValidator<GetCustomerLoyaltyStatsQuery> _validator;

        public GetCustomerLoyaltyStatsQueryHandler(
            ICustomerRepository customerRepository,
            ILoyaltyProgramRepository loyaltyProgramRepository,
            IMapper mapper,
            ILogger<GetCustomerLoyaltyStatsQueryHandler> logger,
            IValidator<GetCustomerLoyaltyStatsQuery> validator)
        {
            _customerRepository = customerRepository;
            _loyaltyProgramRepository = loyaltyProgramRepository;
            _mapper = mapper;
            _logger = logger;
            _validator = validator;
        }

        public async Task<CustomerLoyaltyStatsDto> Handle(GetCustomerLoyaltyStatsQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Récupération des statistiques de fidélité pour le client : {CustomerId}", request.CustomerId);

            // Validation
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                _logger.LogError("Échec de validation pour GetCustomerLoyaltyStats : {Errors}", errors);
                throw new ValidationException($"Erreurs de validation : {errors}");
            }

            // Récupération du client
            var customer = await _customerRepository.GetByIdAsync(request.CustomerId);
            if (customer == null)
            {
                _logger.LogWarning("Client non trouvé : {CustomerId}", request.CustomerId);
                return null;
            }

            // Récupération du programme de fidélité principal
            var primaryProgram = await _loyaltyProgramRepository.GetPrimaryProgramAsync();

            // Construction des statistiques
            var loyaltyStats = new CustomerLoyaltyStatsDto
            {
                CustomerId = customer.Id,
                CurrentPoints = customer.LoyaltyStats.TotalPoints,
                LifetimePointsEarned = customer.LoyaltyStats.TotalPointsEarned,
                LifetimePointsRedeemed = customer.LoyaltyStats.TotalPointsRedeemed,
                CurrentTier = customer.LoyaltyStats.CurrentTier,
                TierSince = customer.LoyaltyStats.TierAchievedDate,
                NextTier = GetNextTier(customer.LoyaltyStats.CurrentTier, primaryProgram),
                PointsToNextTier = CalculatePointsToNextTier(customer, primaryProgram),
                TransactionHistory = await GetLoyaltyTransactionHistory(request.CustomerId, request.IncludeTransactionHistory),
                TierBenefits = GetTierBenefits(customer.LoyaltyStats.CurrentTier, primaryProgram),
                ExpiringPoints = await GetExpiringPoints(request.CustomerId),
                RedemptionRecommendations = await GetRedemptionRecommendations(customer, primaryProgram)
            };

            // Calculs de prédiction
            if (request.IncludePredictions)
            {
                loyaltyStats.Predictions = await CalculateLoyaltyPredictions(customer, primaryProgram);
            }

            _logger.LogInformation("Statistiques de fidélité récupérées pour le client : {CustomerId}", request.CustomerId);

            return loyaltyStats;
        }

        private string GetNextTier(string currentTier, LoyaltyProgram program)
        {
            var tiers = program.TierThresholds.OrderBy(t => t.Value).Select(t => t.Key).ToList();
            var currentIndex = tiers.IndexOf(currentTier);
            return currentIndex >= 0 && currentIndex < tiers.Count - 1 ? tiers[currentIndex + 1] : null;
        }

        private int CalculatePointsToNextTier(Customer customer, LoyaltyProgram program)
        {
            var nextTier = GetNextTier(customer.LoyaltyStats.CurrentTier, program);
            if (nextTier == null) return 0;

            var nextTierThreshold = program.TierThresholds[nextTier];
            return Math.Max(0, nextTierThreshold - customer.LoyaltyStats.TotalPoints);
        }

        private async Task<List<LoyaltyTransactionDto>> GetLoyaltyTransactionHistory(Guid customerId, bool includeHistory)
        {
            if (!includeHistory) return new List<LoyaltyTransactionDto>();

            var transactions = await _customerRepository.GetLoyaltyTransactionsAsync(customerId);
            return _mapper.Map<List<LoyaltyTransactionDto>>(transactions);
        }

        private List<TierBenefitDto> GetTierBenefits(string tier, LoyaltyProgram program)
        {
            // Récupération des avantages associés au niveau
            return program.GetTierBenefits(tier).Select(benefit => new TierBenefitDto
            {
                Name = benefit.Name,
                Description = benefit.Description,
                Value = benefit.Value,
                IsActive = benefit.IsActive
            }).ToList();
        }

        private async Task<List<ExpiringPointsDto>> GetExpiringPoints(Guid customerId)
        {
            var expiringPoints = await _customerRepository.GetExpiringPointsAsync(customerId, DateTime.UtcNow.AddDays(90));
            return _mapper.Map<List<ExpiringPointsDto>>(expiringPoints);
        }

        private async Task<List<RedemptionRecommendationDto>> GetRedemptionRecommendations(Customer customer, LoyaltyProgram program)
        {
            var availableRewards = await _loyaltyProgramRepository.GetAvailableRewardsAsync(program.Id, customer.LoyaltyStats.TotalPoints);
            
            return availableRewards.Select(reward => new RedemptionRecommendationDto
            {
                RewardId = reward.Id,
                RewardName = reward.Name,
                PointsCost = reward.PointsCost,
                Value = reward.Value,
                RecommendationReason = GenerateRecommendationReason(reward, customer),
                Priority = CalculateRecommendationPriority(reward, customer)
            }).OrderByDescending(r => r.Priority).Take(5).ToList();
        }

        private async Task<LoyaltyPredictionsDto> CalculateLoyaltyPredictions(Customer customer, LoyaltyProgram program)
        {
            // Calculs prédictifs sophistiqués
            return new LoyaltyPredictionsDto
            {
                TierProgressionPrediction = await PredictTierProgression(customer, program),
                ChurnRiskScore = await CalculateChurnRisk(customer),
                LifetimeValuePrediction = await PredictLifetimeValue(customer),
                RecommendedActions = await GenerateLoyaltyRecommendations(customer)
            };
        }

        private string GenerateRecommendationReason(LoyaltyReward reward, Customer customer)
        {
            if (reward.RewardType == RewardType.Discount)
                return "Économisez sur votre prochaine commande";
            if (reward.RewardType == RewardType.FreeItem)
                return "Profitez d'un produit gratuit";
            return "Récompense exclusive pour votre fidélité";
        }

        private int CalculateRecommendationPriority(LoyaltyReward reward, Customer customer)
        {
            var priority = 1;
            
            // Priorité basée sur la valeur
            if (reward.Value > 20) priority += 3;
            else if (reward.Value > 10) priority += 2;
            else priority += 1;

            // Priorité basée sur les préférences client
            if (customer.HasPreferenceFor(reward.Category)) priority += 2;

            return priority;
        }

        private async Task<TierProgressionPredictionDto> PredictTierProgression(Customer customer, LoyaltyProgram program)
        {
            // Logique de prédiction basée sur l'historique
            var averageMonthlyEarnings = customer.CalculateAverageMonthlyPointEarnings();
            var nextTier = GetNextTier(customer.LoyaltyStats.CurrentTier, program);
            var pointsNeeded = CalculatePointsToNextTier(customer, program);

            if (nextTier == null || averageMonthlyEarnings <= 0)
                return null;

            var monthsToNextTier = pointsNeeded / averageMonthlyEarnings;

            return new TierProgressionPredictionDto
            {
                NextTier = nextTier,
                EstimatedMonthsToAchieve = Math.Ceiling(monthsToNextTier),
                Confidence = CalculatePredictionConfidence(customer),
                RecommendedActions = GenerateTierProgressionActions(monthsToNextTier)
            };
        }

        private async Task<double> CalculateChurnRisk(Customer customer)
        {
            // Algorithme de calcul du risque de churn basé sur l'activité
            var daysSinceLastActivity = customer.GetDaysSinceLastInteraction();
            var pointRedemptionFrequency = customer.CalculateRedemptionFrequency();
            var engagementScore = customer.CalculateEngagementScore();

            var churnRisk = 0.0;
            if (daysSinceLastActivity > 90) churnRisk += 0.4;
            if (pointRedemptionFrequency < 0.1) churnRisk += 0.3;
            if (engagementScore < 0.3) churnRisk += 0.3;

            return Math.Min(1.0, churnRisk);
        }

        private async Task<decimal> PredictLifetimeValue(Customer customer)
        {
            // Prédiction de valeur vie basée sur les patterns actuels
            var averageMonthlySpend = customer.CalculateAverageMonthlySpend();
            var loyaltyMultiplier = customer.GetLoyaltyMultiplier();
            var expectedLifetimeMonths = customer.CalculateExpectedLifetime();

            return averageMonthlySpend * loyaltyMultiplier * expectedLifetimeMonths;
        }

        private async Task<List<string>> GenerateLoyaltyRecommendations(Customer customer)
        {
            var recommendations = new List<string>();

            if (customer.LoyaltyStats.TotalPoints > 1000)
                recommendations.Add("Utilisez vos points avant qu'ils n'expirent");

            if (customer.GetDaysSinceLastRedemption() > 180)
                recommendations.Add("Découvrez nos nouvelles récompenses disponibles");

            return recommendations;
        }

        private double CalculatePredictionConfidence(Customer customer)
        {
            var transactionCount = customer.GetTransactionCount();
            return Math.Min(1.0, transactionCount / 20.0); // Confiance max avec 20+ transactions
        }

        private List<string> GenerateTierProgressionActions(double monthsToNextTier)
        {
            var actions = new List<string>();

            if (monthsToNextTier <= 2)
                actions.Add("Vous êtes proche du niveau supérieur - continuez vos achats");
            else if (monthsToNextTier <= 6)
                actions.Add("Participez aux événements spéciaux pour gagner des points bonus");
            else
                actions.Add("Explorez nos offres partenaires pour gagner des points supplémentaires");

            return actions;
        }
    }

    /// <summary>
    /// Handler pour récupérer les récompenses disponibles
    /// </summary>
    public class GetAvailableRewardsQueryHandler : IRequestHandler<GetAvailableRewardsQuery, PagedResult<LoyaltyRewardDto>>
    {
        private readonly ILoyaltyRewardRepository _rewardRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetAvailableRewardsQueryHandler> _logger;
        private readonly IValidator<GetAvailableRewardsQuery> _validator;

        public GetAvailableRewardsQueryHandler(
            ILoyaltyRewardRepository rewardRepository,
            ICustomerRepository customerRepository,
            IMapper mapper,
            ILogger<GetAvailableRewardsQueryHandler> logger,
            IValidator<GetAvailableRewardsQuery> validator)
        {
            _rewardRepository = rewardRepository;
            _customerRepository = customerRepository;
            _mapper = mapper;
            _logger = logger;
            _validator = validator;
        }

        public async Task<PagedResult<LoyaltyRewardDto>> Handle(GetAvailableRewardsQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Récupération des récompenses disponibles pour le client : {CustomerId}", request.CustomerId);

            // Validation
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                _logger.LogError("Échec de validation pour GetAvailableRewards : {Errors}", errors);
                throw new ValidationException($"Erreurs de validation : {errors}");
            }

            // Récupération du client si spécifié
            Customer customer = null;
            if (request.CustomerId.HasValue)
            {
                customer = await _customerRepository.GetByIdAsync(request.CustomerId.Value);
                if (customer == null)
                {
                    _logger.LogWarning("Client non trouvé : {CustomerId}", request.CustomerId);
                    return new PagedResult<LoyaltyRewardDto> { Items = new List<LoyaltyRewardDto>() };
                }
            }

            // Récupération des récompenses
            var (rewards, totalCount) = await _rewardRepository.GetAvailableRewardsAsync(
                request.ProgramId,
                customer?.LoyaltyStats.TotalPoints,
                customer?.LoyaltyStats.CurrentTier,
                request.RewardType,
                request.PageNumber,
                request.PageSize,
                request.SortBy,
                request.SortDescending);

            var rewardDtos = _mapper.Map<List<LoyaltyRewardDto>>(rewards);

            // Enrichissement avec informations d'éligibilité
            if (customer != null)
            {
                foreach (var dto in rewardDtos)
                {
                    var reward = rewards.First(r => r.Id == dto.Id);
                    dto.IsEligible = customer.CanRedeemReward(reward);
                    dto.EligibilityReason = GetEligibilityReason(customer, reward);
                    dto.PersonalizationScore = CalculatePersonalizationScore(customer, reward);
                }

                // Tri par score de personnalisation si demandé
                if (request.SortByPersonalization)
                {
                    rewardDtos = rewardDtos.OrderByDescending(r => r.PersonalizationScore).ToList();
                }
            }

            var result = new PagedResult<LoyaltyRewardDto>
            {
                Items = rewardDtos,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
            };

            _logger.LogInformation("Récompenses disponibles récupérées : {Count} sur {Total}", rewardDtos.Count, totalCount);

            return result;
        }

        private string GetEligibilityReason(Customer customer, LoyaltyReward reward)
        {
            if (customer.LoyaltyStats.TotalPoints < reward.PointsCost)
                return $"Points insuffisants (besoin de {reward.PointsCost - customer.LoyaltyStats.TotalPoints} points supplémentaires)";

            if (!string.IsNullOrEmpty(reward.MinimumTierRequired) && 
                !customer.MeetsTierRequirement(reward.MinimumTierRequired))
                return $"Niveau minimum requis : {reward.MinimumTierRequired}";

            if (reward.MaxRedemptionsPerCustomer.HasValue)
            {
                var customerRedemptions = customer.GetRedemptionCount(reward.Id);
                if (customerRedemptions >= reward.MaxRedemptionsPerCustomer.Value)
                    return "Limite de rachat atteinte pour ce client";
            }

            return "Éligible";
        }

        private double CalculatePersonalizationScore(Customer customer, LoyaltyReward reward)
        {
            var score = 0.0;

            // Score basé sur les préférences
            if (customer.HasPreferenceFor(reward.Category)) score += 0.4;

            // Score basé sur l'historique de rachat
            if (customer.HasRedeemedSimilarReward(reward.RewardType)) score += 0.3;

            // Score basé sur la valeur relative
            var valueRatio = (double)reward.Value / reward.PointsCost;
            score += Math.Min(0.3, valueRatio * 0.1);

            return Math.Min(1.0, score);
        }
    }

    /// <summary>
    /// Handler pour récupérer l'historique des transactions de fidélité
    /// </summary>
    public class GetLoyaltyTransactionHistoryQueryHandler : IRequestHandler<GetLoyaltyTransactionHistoryQuery, PagedResult<LoyaltyTransactionDto>>
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetLoyaltyTransactionHistoryQueryHandler> _logger;
        private readonly IValidator<GetLoyaltyTransactionHistoryQuery> _validator;

        public GetLoyaltyTransactionHistoryQueryHandler(
            ICustomerRepository customerRepository,
            IMapper mapper,
            ILogger<GetLoyaltyTransactionHistoryQueryHandler> logger,
            IValidator<GetLoyaltyTransactionHistoryQuery> validator)
        {
            _customerRepository = customerRepository;
            _mapper = mapper;
            _logger = logger;
            _validator = validator;
        }

        public async Task<PagedResult<LoyaltyTransactionDto>> Handle(GetLoyaltyTransactionHistoryQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Récupération de l'historique des transactions de fidélité - Client : {CustomerId}", request.CustomerId);

            // Validation
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                _logger.LogError("Échec de validation pour GetLoyaltyTransactionHistory : {Errors}", errors);
                throw new ValidationException($"Erreurs de validation : {errors}");
            }

            // Récupération des transactions
            var (transactions, totalCount) = await _customerRepository.GetLoyaltyTransactionHistoryAsync(
                request.CustomerId,
                request.StartDate,
                request.EndDate,
                request.TransactionType,
                request.PageNumber,
                request.PageSize,
                request.SortBy,
                request.SortDescending);

            var transactionDtos = _mapper.Map<List<LoyaltyTransactionDto>>(transactions);

            // Calcul des statistiques si demandé
            var summary = request.IncludeSummary ? await CalculateTransactionSummary(transactions) : null;

            var result = new PagedResult<LoyaltyTransactionDto>
            {
                Items = transactionDtos,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize),
                Summary = summary
            };

            _logger.LogInformation("Historique des transactions récupéré : {Count} sur {Total}", transactionDtos.Count, totalCount);

            return result;
        }

        private async Task<object> CalculateTransactionSummary(IEnumerable<LoyaltyTransaction> transactions)
        {
            return new
            {
                TotalTransactions = transactions.Count(),
                TotalPointsEarned = transactions.Where(t => t.Type == "Earned").Sum(t => t.Points),
                TotalPointsRedeemed = transactions.Where(t => t.Type == "Redeemed").Sum(t => t.Points),
                AverageTransactionValue = transactions.Average(t => t.Points),
                MostFrequentTransactionType = transactions.GroupBy(t => t.Type)
                    .OrderByDescending(g => g.Count())
                    .FirstOrDefault()?.Key
            };
        }
    }

    /// <summary>
    /// Handler pour récupérer les statistiques globales de fidélité
    /// </summary>
    public class GetLoyaltyProgramStatsQueryHandler : IRequestHandler<GetLoyaltyProgramStatsQuery, LoyaltyProgramStatsDto>
    {
        private readonly ILoyaltyProgramRepository _loyaltyProgramRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetLoyaltyProgramStatsQueryHandler> _logger;
        private readonly IValidator<GetLoyaltyProgramStatsQuery> _validator;

        public GetLoyaltyProgramStatsQueryHandler(
            ILoyaltyProgramRepository loyaltyProgramRepository,
            ICustomerRepository customerRepository,
            IMapper mapper,
            ILogger<GetLoyaltyProgramStatsQueryHandler> logger,
            IValidator<GetLoyaltyProgramStatsQuery> validator)
        {
            _loyaltyProgramRepository = loyaltyProgramRepository;
            _customerRepository = customerRepository;
            _mapper = mapper;
            _logger = logger;
            _validator = validator;
        }

        public async Task<LoyaltyProgramStatsDto> Handle(GetLoyaltyProgramStatsQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Calcul des statistiques du programme de fidélité : {ProgramId}", request.ProgramId);

            // Validation
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                _logger.LogError("Échec de validation pour GetLoyaltyProgramStats : {Errors}", errors);
                throw new ValidationException($"Erreurs de validation : {errors}");
            }

            // Récupération du programme
            var program = await _loyaltyProgramRepository.GetByIdAsync(request.ProgramId);
            if (program == null)
            {
                _logger.LogWarning("Programme de fidélité non trouvé : {ProgramId}", request.ProgramId);
                return null;
            }

            // Calcul des statistiques
            var stats = new LoyaltyProgramStatsDto
            {
                ProgramId = program.Id,
                ProgramName = program.Name,
                GeneratedAt = DateTime.UtcNow,
                PeriodStart = request.StartDate,
                PeriodEnd = request.EndDate,
                
                // Statistiques de membres
                TotalMembers = await _customerRepository.GetProgramMemberCountAsync(request.ProgramId),
                ActiveMembers = await _customerRepository.GetActiveProgramMemberCountAsync(request.ProgramId, request.StartDate),
                NewMembersInPeriod = await _customerRepository.GetNewMembersInPeriodAsync(request.ProgramId, request.StartDate, request.EndDate),
                
                // Statistiques de points
                TotalPointsIssued = await _loyaltyProgramRepository.GetTotalPointsIssuedAsync(request.ProgramId, request.StartDate, request.EndDate),
                TotalPointsRedeemed = await _loyaltyProgramRepository.GetTotalPointsRedeemedAsync(request.ProgramId, request.StartDate, request.EndDate),
                OutstandingPoints = await _loyaltyProgramRepository.GetOutstandingPointsAsync(request.ProgramId),
                
                // Distribution par niveau
                TierDistribution = await _customerRepository.GetTierDistributionAsync(request.ProgramId),
                
                // Statistiques de récompenses
                TopRewards = await _loyaltyProgramRepository.GetTopRedeemedRewardsAsync(request.ProgramId, request.StartDate, request.EndDate, 10),
                RedemptionRate = await CalculateRedemptionRate(request.ProgramId, request.StartDate, request.EndDate),
                
                // Métriques d'engagement
                AverageTransactionsPerMember = await CalculateAverageTransactionsPerMember(request.ProgramId, request.StartDate, request.EndDate),
                MemberRetentionRate = await CalculateMemberRetentionRate(request.ProgramId, request.StartDate, request.EndDate)
            };

            // Calcul des tendances si demandé
            if (request.IncludeTrends)
            {
                stats.Trends = await CalculateLoyaltyTrends(request.ProgramId, request.StartDate, request.EndDate);
            }

            // Analyse prédictive si demandée
            if (request.IncludePredictions)
            {
                stats.Predictions = await CalculateLoyaltyPredictions(request.ProgramId);
            }

            _logger.LogInformation("Statistiques du programme calculées - Membres : {Members}, Points émis : {PointsIssued}", 
                stats.TotalMembers, stats.TotalPointsIssued);

            return stats;
        }

        private async Task<double> CalculateRedemptionRate(Guid programId, DateTime startDate, DateTime endDate)
        {
            var totalIssued = await _loyaltyProgramRepository.GetTotalPointsIssuedAsync(programId, startDate, endDate);
            var totalRedeemed = await _loyaltyProgramRepository.GetTotalPointsRedeemedAsync(programId, startDate, endDate);
            
            return totalIssued > 0 ? (double)totalRedeemed / totalIssued * 100 : 0;
        }

        private async Task<double> CalculateAverageTransactionsPerMember(Guid programId, DateTime startDate, DateTime endDate)
        {
            var totalTransactions = await _loyaltyProgramRepository.GetTotalTransactionsAsync(programId, startDate, endDate);
            var activeMembers = await _customerRepository.GetActiveProgramMemberCountAsync(programId, startDate);
            
            return activeMembers > 0 ? (double)totalTransactions / activeMembers : 0;
        }

        private async Task<double> CalculateMemberRetentionRate(Guid programId, DateTime startDate, DateTime endDate)
        {
            var startMembers = await _customerRepository.GetProgramMemberCountAtDateAsync(programId, startDate);
            var endMembers = await _customerRepository.GetProgramMemberCountAtDateAsync(programId, endDate);
            var newMembers = await _customerRepository.GetNewMembersInPeriodAsync(programId, startDate, endDate);
            
            var retainedMembers = endMembers - newMembers;
            return startMembers > 0 ? (double)retainedMembers / startMembers * 100 : 0;
        }

        private async Task<LoyaltyTrendsDto> CalculateLoyaltyTrends(Guid programId, DateTime startDate, DateTime endDate)
        {
            var previousPeriodStart = startDate.AddDays(-(endDate - startDate).TotalDays);
            
            var currentStats = await _loyaltyProgramRepository.GetPeriodStatsAsync(programId, startDate, endDate);
            var previousStats = await _loyaltyProgramRepository.GetPeriodStatsAsync(programId, previousPeriodStart, startDate);

            return new LoyaltyTrendsDto
            {
                MemberGrowthRate = CalculateGrowthRate(currentStats.NewMembers, previousStats.NewMembers),
                PointIssuanceTrend = CalculateGrowthRate(currentStats.PointsIssued, previousStats.PointsIssued),
                RedemptionTrend = CalculateGrowthRate(currentStats.PointsRedeemed, previousStats.PointsRedeemed),
                EngagementTrend = CalculateGrowthRate(currentStats.AvgTransactionsPerMember, previousStats.AvgTransactionsPerMember)
            };
        }

        private async Task<LoyaltyPredictionsDto> CalculateLoyaltyPredictions(Guid programId)
        {
            // Calculs prédictifs sophistiqués
            return new LoyaltyPredictionsDto
            {
                PredictedMemberGrowth = await PredictMemberGrowth(programId),
                PredictedPointLiability = await PredictPointLiability(programId),
                PredictedRedemptionVolume = await PredictRedemptionVolume(programId),
                RiskFactors = await IdentifyRiskFactors(programId),
                RecommendedActions = await GenerateProgramRecommendations(programId)
            };
        }

        private double CalculateGrowthRate(double current, double previous)
        {
            if (previous == 0) return current > 0 ? 100 : 0;
            return ((current - previous) / previous) * 100;
        }

        private async Task<double> PredictMemberGrowth(Guid programId)
        {
            // Analyse de tendance basée sur les données historiques
            var historicalGrowth = await _loyaltyProgramRepository.GetHistoricalGrowthRateAsync(programId);
            return historicalGrowth; // Simplification - dans un vrai système, utiliseriez ML
        }

        private async Task<decimal> PredictPointLiability(Guid programId)
        {
            // Prédiction de la dette de points basée sur les patterns actuels
            var outstandingPoints = await _loyaltyProgramRepository.GetOutstandingPointsAsync(programId);
            var redemptionRate = await _loyaltyProgramRepository.GetAverageRedemptionRateAsync(programId);
            
            return outstandingPoints * (1 - (decimal)redemptionRate);
        }

        private async Task<double> PredictRedemptionVolume(Guid programId)
        {
            // Prédiction basée sur les tendances saisonnières et les patterns historiques
            var historicalVolume = await _loyaltyProgramRepository.GetHistoricalRedemptionVolumeAsync(programId);
            return historicalVolume; // Simplification
        }

        private async Task<List<string>> IdentifyRiskFactors(Guid programId)
        {
            var risks = new List<string>();
            
            var redemptionRate = await _loyaltyProgramRepository.GetAverageRedemptionRateAsync(programId);
            if (redemptionRate < 0.2) risks.Add("Taux de rachat faible - risque de désengagement");
            
            var pointLiability = await _loyaltyProgramRepository.GetOutstandingPointsAsync(programId);
            if (pointLiability > 1000000) risks.Add("Dette de points élevée - risque financier");
            
            return risks;
        }

        private async Task<List<string>> GenerateProgramRecommendations(Guid programId)
        {
            var recommendations = new List<string>();
            
            var redemptionRate = await _loyaltyProgramRepository.GetAverageRedemptionRateAsync(programId);
            if (redemptionRate < 0.3)
            {
                recommendations.Add("Améliorer le catalogue de récompenses");
                recommendations.Add("Simplifier le processus de rachat");
            }
            
            var memberGrowth = await _loyaltyProgramRepository.GetRecentMemberGrowthAsync(programId);
            if (memberGrowth < 0.05)
            {
                recommendations.Add("Lancer une campagne d'acquisition");
                recommendations.Add("Améliorer les avantages d'inscription");
            }
            
            return recommendations;
        }
    }

    /// <summary>
    /// Handler pour vérifier l'éligibilité à une récompense
    /// </summary>
    public class CheckRewardEligibilityQueryHandler : IRequestHandler<CheckRewardEligibilityQuery, RewardEligibilityDto>
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly ILoyaltyRewardRepository _rewardRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<CheckRewardEligibilityQueryHandler> _logger;
        private readonly IValidator<CheckRewardEligibilityQuery> _validator;

        public CheckRewardEligibilityQueryHandler(
            ICustomerRepository customerRepository,
            ILoyaltyRewardRepository rewardRepository,
            IMapper mapper,
            ILogger<CheckRewardEligibilityQueryHandler> logger,
            IValidator<CheckRewardEligibilityQuery> validator)
        {
            _customerRepository = customerRepository;
            _rewardRepository = rewardRepository;
            _mapper = mapper;
            _logger = logger;
            _validator = validator;
        }

        public async Task<RewardEligibilityDto> Handle(CheckRewardEligibilityQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Vérification de l'éligibilité à la récompense {RewardId} pour le client {CustomerId}", 
                request.RewardId, request.CustomerId);

            // Validation
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                _logger.LogError("Échec de validation pour CheckRewardEligibility : {Errors}", errors);
                throw new ValidationException($"Erreurs de validation : {errors}");
            }

            // Récupération du client et de la récompense
            var customer = await _customerRepository.GetByIdAsync(request.CustomerId);
            var reward = await _rewardRepository.GetByIdAsync(request.RewardId);

            if (customer == null || reward == null)
            {
                return new RewardEligibilityDto
                {
                    IsEligible = false,
                    Reason = "Client ou récompense non trouvé"
                };
            }

            // Vérification de l'éligibilité
            var eligibility = new RewardEligibilityDto
            {
                CustomerId = customer.Id,
                RewardId = reward.Id,
                IsEligible = customer.CanRedeemReward(reward),
                CustomerPoints = customer.LoyaltyStats.TotalPoints,
                RequiredPoints = reward.PointsCost,
                CustomerTier = customer.LoyaltyStats.CurrentTier,
                RequiredTier = reward.MinimumTierRequired
            };

            // Détail des raisons d'inéligibilité
            if (!eligibility.IsEligible)
            {
                eligibility.Reason = GetDetailedEligibilityReason(customer, reward);
                eligibility.SuggestedActions = GetSuggestedActions(customer, reward);
            }
            else
            {
                eligibility.Reason = "Éligible pour cette récompense";
            }

            _logger.LogInformation("Éligibilité vérifiée - Client {CustomerId}, Récompense {RewardId}, Éligible : {IsEligible}", 
                request.CustomerId, request.RewardId, eligibility.IsEligible);

            return eligibility;
        }

        private string GetDetailedEligibilityReason(Customer customer, LoyaltyReward reward)
        {
            var reasons = new List<string>();

            if (customer.LoyaltyStats.TotalPoints < reward.PointsCost)
                reasons.Add($"Points insuffisants (manque {reward.PointsCost - customer.LoyaltyStats.TotalPoints} points)");

            if (!string.IsNullOrEmpty(reward.MinimumTierRequired) && 
                !customer.MeetsTierRequirement(reward.MinimumTierRequired))
                reasons.Add($"Niveau minimum requis : {reward.MinimumTierRequired}");

            if (!reward.IsActive)
                reasons.Add("Récompense actuellement indisponible");

            if (reward.ValidFrom > DateTime.UtcNow)
                reasons.Add($"Récompense disponible à partir du {reward.ValidFrom:dd/MM/yyyy}");

            if (reward.ValidUntil < DateTime.UtcNow)
                reasons.Add("Récompense expirée");

            return string.Join("; ", reasons);
        }

        private List<string> GetSuggestedActions(Customer customer, LoyaltyReward reward)
        {
            var actions = new List<string>();

            if (customer.LoyaltyStats.TotalPoints < reward.PointsCost)
            {
                var pointsNeeded = reward.PointsCost - customer.LoyaltyStats.TotalPoints;
                actions.Add($"Gagnez {pointsNeeded} points supplémentaires");
                
                var estimatedPurchase = pointsNeeded / 10; // Approximation 10 points = 1€
                actions.Add($"Effectuez environ {estimatedPurchase:F0}€ d'achats supplémentaires");
            }

            if (!string.IsNullOrEmpty(reward.MinimumTierRequired) && 
                !customer.MeetsTierRequirement(reward.MinimumTierRequired))
            {
                actions.Add($"Atteignez le niveau {reward.MinimumTierRequired}");
            }

            return actions;
        }
    }
}