using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using CustomerService.Domain.Entities;
using CustomerService.Domain.Repositories;
using CustomerService.Application.DTOs.PreferenceAnalytics;
using CustomerService.Application.Queries.PreferenceAnalytics;
using CustomerService.Domain.ValueObjects;
using CustomerService.Domain.Enums;

namespace CustomerService.Application.Handlers.Queries
{
    // ========================================================================================
    // PREFERENCE ANALYTICS QUERY HANDLERS - RÉCUPÉRATION SOPHISTIQUÉE DES DONNÉES ANALYTIQUES
    // ========================================================================================

    /// <summary>
    /// Handler pour récupérer l'analyse comportementale d'un client
    /// </summary>
    public class GetBehaviorAnalysisQueryHandler : IRequestHandler<GetBehaviorAnalysisQuery, BehaviorAnalysisResultDto>
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly ICustomerPreferenceRepository _preferenceRepository;
        private readonly ICustomerInteractionRepository _interactionRepository;
        private readonly ICustomerAnalyticsRepository _analyticsRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetBehaviorAnalysisQueryHandler> _logger;
        private readonly IValidator<GetBehaviorAnalysisQuery> _validator;

        public GetBehaviorAnalysisQueryHandler(
            ICustomerRepository customerRepository,
            ICustomerPreferenceRepository preferenceRepository,
            ICustomerInteractionRepository interactionRepository,
            ICustomerAnalyticsRepository analyticsRepository,
            IMapper mapper,
            ILogger<GetBehaviorAnalysisQueryHandler> logger,
            IValidator<GetBehaviorAnalysisQuery> validator)
        {
            _customerRepository = customerRepository;
            _preferenceRepository = preferenceRepository;
            _interactionRepository = interactionRepository;
            _analyticsRepository = analyticsRepository;
            _mapper = mapper;
            _logger = logger;
            _validator = validator;
        }

        public async Task<BehaviorAnalysisResultDto> Handle(GetBehaviorAnalysisQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Récupération de l'analyse comportementale pour le client : {CustomerId}", request.CustomerId);

            // Validation
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                _logger.LogError("Échec de validation pour GetBehaviorAnalysis : {Errors}", errors);
                throw new ValidationException($"Erreurs de validation : {errors}");
            }

            // Récupération du client
            var customer = await _customerRepository.GetByIdWithDetailsAsync(request.CustomerId);
            if (customer == null)
            {
                _logger.LogWarning("Client non trouvé : {CustomerId}", request.CustomerId);
                return null;
            }

            // Récupération des données pour l'analyse
            var preferences = await _preferenceRepository.GetByCustomerIdAsync(request.CustomerId);
            var interactions = await _interactionRepository.GetRecentByCustomerIdAsync(
                request.CustomerId, DateTime.UtcNow.AddDays(-request.AnalysisPeriodDays));

            // Récupération de l'analyse existante ou calcul d'une nouvelle
            var existingAnalysis = await _analyticsRepository.GetLatestBehaviorAnalysisAsync(request.CustomerId);
            
            BehaviorAnalysisResultDto result;

            if (existingAnalysis != null && 
                existingAnalysis.AnalysisDate > DateTime.UtcNow.AddHours(-request.RefreshThresholdHours))
            {
                // Utilisation de l'analyse existante récente
                result = _mapper.Map<BehaviorAnalysisResultDto>(existingAnalysis);
                result.IsFromCache = true;
            }
            else
            {
                // Calcul d'une nouvelle analyse
                result = await PerformDetailedBehaviorAnalysis(customer, preferences, interactions, request);
                result.IsFromCache = false;

                // Sauvegarde de la nouvelle analyse
                await _analyticsRepository.SaveBehaviorAnalysisAsync(request.CustomerId, result);
            }

            // Enrichissement avec données temps réel si demandé
            if (request.IncludeRealTimeMetrics)
            {
                result.RealTimeMetrics = await GetRealTimeMetrics(request.CustomerId);
            }

            // Prédictions avancées si demandées
            if (request.IncludeAdvancedPredictions)
            {
                result.AdvancedPredictions = await GenerateAdvancedPredictions(customer, result);
            }

            _logger.LogInformation("Analyse comportementale récupérée pour le client : {CustomerId} - Score de churn : {ChurnRisk}", 
                request.CustomerId, result.ChurnRiskScore);

            return result;
        }

        private async Task<BehaviorAnalysisResultDto> PerformDetailedBehaviorAnalysis(
            Customer customer, 
            IEnumerable<CustomerPreference> preferences, 
            IEnumerable<CustomerInteraction> interactions,
            GetBehaviorAnalysisQuery request)
        {
            var preferenceList = preferences.ToList();
            var interactionList = interactions.ToList();

            // Analyse comportementale détaillée
            var behaviorProfile = AnalyzeBehaviorPatterns(customer, preferenceList, interactionList);
            var churnRisk = CalculateAdvancedChurnRisk(customer, interactionList, behaviorProfile);
            var lifetimeValue = CalculateDetailedLifetimeValue(customer, behaviorProfile, interactionList);
            var personalityInsights = GeneratePersonalityInsights(preferenceList, interactionList);
            var recommendations = await GeneratePersonalizedRecommendations(customer, behaviorProfile);

            return new BehaviorAnalysisResultDto
            {
                CustomerId = customer.Id,
                AnalysisDate = DateTime.UtcNow,
                AnalysisType = request.AnalysisType,
                BehaviorProfile = behaviorProfile,
                ChurnRiskScore = churnRisk,
                LifetimeValueEstimate = lifetimeValue,
                PersonalityInsights = personalityInsights,
                PersonalizedRecommendations = recommendations,
                KeyInsights = ExtractKeyBehaviorInsights(behaviorProfile, churnRisk, lifetimeValue),
                RecommendedActions = GenerateActionableRecommendations(churnRisk, lifetimeValue, behaviorProfile),
                ConfidenceScore = CalculateAnalysisConfidence(preferenceList.Count, interactionList.Count),
                DataQualityScore = AssessDataQuality(customer, preferenceList, interactionList),
                TrendAnalysis = AnalyzeBehaviorTrends(customer, interactionList)
            };
        }

        private BehaviorProfileDto AnalyzeBehaviorPatterns(Customer customer, List<CustomerPreference> preferences, List<CustomerInteraction> interactions)
        {
            // Analyse sophistiquée des patterns comportementaux
            var communicationPatterns = AnalyzeCommunicationPatterns(interactions);
            var engagementPatterns = AnalyzeEngagementPatterns(interactions);
            var preferencePatterns = AnalyzePreferencePatterns(preferences);
            var temporalPatterns = AnalyzeTemporalPatterns(interactions);

            return new BehaviorProfileDto
            {
                CustomerId = customer.Id,
                AnalysisDate = DateTime.UtcNow,
                CommunicationStyle = communicationPatterns.PrimaryStyle,
                EngagementLevel = engagementPatterns.Level,
                PreferenceStability = preferencePatterns.StabilityScore,
                ResponsePatterns = communicationPatterns.ResponsePatterns,
                ActivityPatterns = temporalPatterns.ActivityPatterns,
                InteractionPreferences = communicationPatterns.ChannelPreferences,
                BehaviorScores = CalculateBehaviorScores(customer, preferences, interactions),
                PersonalityTraits = InferPersonalityTraits(preferences, interactions),
                RiskFactors = IdentifyBehaviorRiskFactors(customer, interactions),
                OpportunityAreas = IdentifyOpportunityAreas(customer, preferences, interactions)
            };
        }

        private double CalculateAdvancedChurnRisk(Customer customer, List<CustomerInteraction> interactions, BehaviorProfileDto behaviorProfile)
        {
            var riskFactors = new Dictionary<string, double>();

            // Facteur d'inactivité
            var daysSinceLastInteraction = (DateTime.UtcNow - (interactions.LastOrDefault()?.InteractionDate ?? customer.RegistrationDate)).TotalDays;
            riskFactors["inactivity"] = Math.Min(1.0, daysSinceLastInteraction / 180.0) * 0.25;

            // Facteur de satisfaction
            var avgSatisfaction = interactions.Where(i => i.Rating.HasValue).Average(i => i.Rating) ?? 5.0;
            riskFactors["satisfaction"] = Math.Max(0, (5.0 - avgSatisfaction) / 5.0) * 0.3;

            // Facteur d'engagement
            var engagementScore = behaviorProfile.BehaviorScores.GetValueOrDefault("engagement", 0.5);
            riskFactors["engagement"] = (1.0 - engagementScore) * 0.2;

            // Facteur de récurrence des problèmes
            var complaintRatio = interactions.Count(i => i.InteractionType == InteractionType.Complaint) / (double)Math.Max(1, interactions.Count);
            riskFactors["complaints"] = complaintRatio * 0.15;

            // Facteur de stabilité des préférences
            riskFactors["preference_instability"] = (1.0 - behaviorProfile.PreferenceStability) * 0.1;

            var totalRisk = riskFactors.Values.Sum();
            return Math.Min(1.0, totalRisk);
        }

        private decimal CalculateDetailedLifetimeValue(Customer customer, BehaviorProfileDto behaviorProfile, List<CustomerInteraction> interactions)
        {
            // Calcul sophistiqué de la CLV
            var baseValue = customer.LoyaltyStats.TotalPointsEarned * 0.01m; // Conversion points -> valeur

            // Multiplicateurs basés sur l'analyse comportementale
            var engagementMultiplier = (decimal)behaviorProfile.BehaviorScores.GetValueOrDefault("engagement", 0.5) * 2;
            var loyaltyMultiplier = GetLoyaltyMultiplier(customer.LoyaltyStats.CurrentTier);
            var satisfactionMultiplier = CalculateSatisfactionMultiplier(interactions);
            var frequencyMultiplier = CalculateFrequencyMultiplier(interactions);

            // Prédiction de durée de vie
            var predictedLifetimeMonths = PredictCustomerLifetime(customer, behaviorProfile, interactions);

            // Calcul de la valeur mensuelle moyenne
            var monthlyValue = baseValue * engagementMultiplier * loyaltyMultiplier * satisfactionMultiplier * frequencyMultiplier / 12;

            return monthlyValue * predictedLifetimeMonths;
        }

        private PersonalityInsightsDto GeneratePersonalityInsights(List<CustomerPreference> preferences, List<CustomerInteraction> interactions)
        {
            return new PersonalityInsightsDto
            {
                CommunicationStyle = InferCommunicationStyle(preferences, interactions),
                DecisionMakingStyle = InferDecisionMakingStyle(interactions),
                ServiceExpectations = InferServiceExpectations(interactions),
                PersonalityTraits = InferDetailedPersonalityTraits(preferences, interactions),
                PreferredApproach = DeterminePreferredApproach(preferences, interactions),
                MotivationFactors = IdentifyMotivationFactors(preferences, interactions),
                CommunicationTips = GenerateCommunicationTips(preferences, interactions)
            };
        }

        private async Task<List<PersonalizedRecommendationDto>> GeneratePersonalizedRecommendations(Customer customer, BehaviorProfileDto behaviorProfile)
        {
            var recommendations = new List<PersonalizedRecommendationDto>();

            // Recommandations basées sur l'engagement
            if (behaviorProfile.EngagementLevel == "Low")
            {
                recommendations.Add(new PersonalizedRecommendationDto
                {
                    Type = "engagement",
                    Title = "Programme de réengagement personnalisé",
                    Description = "Mettre en place un parcours de réengagement adapté au profil comportemental",
                    Priority = "High",
                    ConfidenceScore = 0.85,
                    ExpectedImpact = "Medium",
                    RecommendedActions = new List<string>
                    {
                        "Proposer une offre exclusive basée sur les préférences",
                        "Utiliser le canal de communication préféré",
                        "Programmer un contact personnel"
                    },
                    EstimatedROI = CalculateEstimatedROI("engagement", customer, behaviorProfile)
                });
            }

            // Recommandations basées sur les préférences
            if (behaviorProfile.CommunicationStyle == "Digital")
            {
                recommendations.Add(new PersonalizedRecommendationDto
                {
                    Type = "communication",
                    Title = "Optimisation communication digitale",
                    Description = "Adapter toute communication aux préférences digitales du client",
                    Priority = "Medium",
                    ConfidenceScore = 0.92,
                    ExpectedImpact = "High",
                    RecommendedActions = new List<string>
                    {
                        "Privilégier emails et SMS",
                        "Proposer l'application mobile",
                        "Éviter les appels téléphoniques non sollicités"
                    },
                    EstimatedROI = CalculateEstimatedROI("communication", customer, behaviorProfile)
                });
            }

            // Recommandations de fidélisation
            if (customer.LoyaltyStats.CurrentTier == "Gold" || customer.LoyaltyStats.CurrentTier == "Platinum")
            {
                recommendations.Add(new PersonalizedRecommendationDto
                {
                    Type = "loyalty",
                    Title = "Programme VIP exclusif",
                    Description = "Proposer des avantages exclusifs pour maintenir la fidélité",
                    Priority = "High",
                    ConfidenceScore = 0.88,
                    ExpectedImpact = "High",
                    RecommendedActions = new List<string>
                    {
                        "Accès prioritaire aux nouveautés",
                        "Service client dédié",
                        "Événements exclusifs"
                    },
                    EstimatedROI = CalculateEstimatedROI("loyalty", customer, behaviorProfile)
                });
            }

            return recommendations.OrderByDescending(r => r.ConfidenceScore * (double)r.EstimatedROI).ToList();
        }

        private async Task<RealTimeMetricsDto> GetRealTimeMetrics(Guid customerId)
        {
            // Métriques en temps réel
            return new RealTimeMetricsDto
            {
                LastActivityDate = await _customerRepository.GetLastActivityDateAsync(customerId),
                CurrentEngagementScore = await _analyticsRepository.GetCurrentEngagementScoreAsync(customerId),
                RecentBehaviorChanges = await _analyticsRepository.GetRecentBehaviorChangesAsync(customerId),
                ActiveCampaigns = await _analyticsRepository.GetActiveCampaignsAsync(customerId),
                PendingActions = await _analyticsRepository.GetPendingActionsAsync(customerId),
                LiveInteractions = await _interactionRepository.GetLiveInteractionsAsync(customerId)
            };
        }

        private async Task<AdvancedPredictionsDto> GenerateAdvancedPredictions(Customer customer, BehaviorAnalysisResultDto analysis)
        {
            return new AdvancedPredictionsDto
            {
                ChurnProbabilityByTimeframe = await PredictChurnByTimeframe(customer, analysis),
                ValueEvolutionPrediction = await PredictValueEvolution(customer, analysis),
                BehaviorEvolutionPrediction = await PredictBehaviorEvolution(customer, analysis),
                OptimalContactStrategy = await DetermineOptimalContactStrategy(customer, analysis),
                RecommendedNextActions = await GenerateNextActionsPredictions(customer, analysis),
                RiskMitigationStrategies = await GenerateRiskMitigationStrategies(customer, analysis)
            };
        }

        // Méthodes utilitaires pour l'analyse comportementale
        private CommunicationPatterns AnalyzeCommunicationPatterns(List<CustomerInteraction> interactions)
        {
            var channelUsage = interactions.GroupBy(i => i.Channel)
                .ToDictionary(g => g.Key.ToString(), g => g.Count());

            var responsePatterns = AnalyzeResponsePatterns(interactions);
            var primaryStyle = DeterminePrimaryCommunicationStyle(interactions);

            return new CommunicationPatterns
            {
                PrimaryStyle = primaryStyle,
                ChannelPreferences = channelUsage,
                ResponsePatterns = responsePatterns
            };
        }

        private EngagementPatterns AnalyzeEngagementPatterns(List<CustomerInteraction> interactions)
        {
            var frequency = CalculateInteractionFrequency(interactions);
            var quality = CalculateInteractionQuality(interactions);
            var consistency = CalculateEngagementConsistency(interactions);

            var level = (frequency + quality + consistency) / 3.0 switch
            {
                > 0.7 => "High",
                > 0.4 => "Medium",
                _ => "Low"
            };

            return new EngagementPatterns
            {
                Level = level,
                Frequency = frequency,
                Quality = quality,
                Consistency = consistency
            };
        }

        private PreferencePatterns AnalyzePreferencePatterns(List<CustomerPreference> preferences)
        {
            var stability = CalculatePreferenceStability(preferences);
            var diversity = CalculatePreferenceDiversity(preferences);
            var strength = CalculatePreferenceStrength(preferences);

            return new PreferencePatterns
            {
                StabilityScore = stability,
                DiversityScore = diversity,
                StrengthScore = strength
            };
        }

        private TemporalPatterns AnalyzeTemporalPatterns(List<CustomerInteraction> interactions)
        {
            var hourlyDistribution = interactions.GroupBy(i => i.InteractionDate.Hour)
                .ToDictionary(g => g.Key, g => g.Count());

            var dailyDistribution = interactions.GroupBy(i => i.InteractionDate.DayOfWeek)
                .ToDictionary(g => g.Key.ToString(), g => g.Count());

            return new TemporalPatterns
            {
                ActivityPatterns = new Dictionary<string, object>
                {
                    ["hourly"] = hourlyDistribution,
                    ["daily"] = dailyDistribution
                }
            };
        }

        private Dictionary<string, double> CalculateBehaviorScores(Customer customer, List<CustomerPreference> preferences, List<CustomerInteraction> interactions)
        {
            return new Dictionary<string, double>
            {
                ["engagement"] = CalculateEngagementScore(interactions),
                ["satisfaction"] = CalculateSatisfactionScore(interactions),
                ["loyalty"] = CalculateLoyaltyScore(customer),
                ["responsiveness"] = CalculateResponsivenessScore(interactions),
                ["consistency"] = CalculateConsistencyScore(interactions),
                ["preference_clarity"] = CalculatePreferenceClarityScore(preferences)
            };
        }

        private List<string> InferPersonalityTraits(List<CustomerPreference> preferences, List<CustomerInteraction> interactions)
        {
            var traits = new List<string>();

            // Analyse basée sur les interactions
            if (interactions.Average(i => i.Rating ?? 4.0) > 4.5) traits.Add("Positive");
            if (interactions.Count(i => i.InteractionType == InteractionType.Complaint) == 0) traits.Add("Patient");
            if (interactions.Count(i => i.Channel == InteractionChannel.Digital) > interactions.Count * 0.7) traits.Add("Tech-Savvy");

            // Analyse basée sur les préférences
            if (preferences.Any(p => p.Category.Contains("Premium"))) traits.Add("Quality-Oriented");
            if (preferences.Count(p => p.PreferenceType == PreferenceType.Communication) > 3) traits.Add("Communication-Conscious");

            return traits;
        }

        private List<string> IdentifyBehaviorRiskFactors(Customer customer, List<CustomerInteraction> interactions)
        {
            var risks = new List<string>();

            var daysSinceLastActivity = (DateTime.UtcNow - (interactions.LastOrDefault()?.InteractionDate ?? customer.RegistrationDate)).TotalDays;
            if (daysSinceLastActivity > 90) risks.Add("Inactivité prolongée");

            var avgSatisfaction = interactions.Where(i => i.Rating.HasValue).Average(i => i.Rating) ?? 5.0;
            if (avgSatisfaction < 3.5) risks.Add("Satisfaction faible");

            var complaintRatio = interactions.Count(i => i.InteractionType == InteractionType.Complaint) / (double)Math.Max(1, interactions.Count);
            if (complaintRatio > 0.3) risks.Add("Fréquence élevée de plaintes");

            return risks;
        }

        private List<string> IdentifyOpportunityAreas(Customer customer, List<CustomerPreference> preferences, List<CustomerInteraction> interactions)
        {
            var opportunities = new List<string>();

            if (customer.LoyaltyStats.TotalPoints > 1000) opportunities.Add("Optimisation du programme de fidélité");
            if (interactions.Count > 10) opportunities.Add("Client engagé - potentiel d'ambassadeur");
            if (preferences.Any(p => p.Category.Contains("Premium"))) opportunities.Add("Opportunité d'upselling");

            return opportunities;
        }

        // Méthodes de calcul spécialisées
        private decimal GetLoyaltyMultiplier(string tier) => tier switch
        {
            "Bronze" => 1.0m,
            "Silver" => 1.2m,
            "Gold" => 1.5m,
            "Platinum" => 2.0m,
            _ => 1.0m
        };

        private decimal CalculateSatisfactionMultiplier(List<CustomerInteraction> interactions)
        {
            var avgSatisfaction = interactions.Where(i => i.Rating.HasValue).Average(i => i.Rating) ?? 5.0;
            return (decimal)(avgSatisfaction / 5.0);
        }

        private decimal CalculateFrequencyMultiplier(List<CustomerInteraction> interactions)
        {
            var frequency = interactions.Count / Math.Max(1.0, (DateTime.UtcNow - interactions.Min(i => i.InteractionDate)).TotalDays / 30);
            return Math.Min(2.0m, (decimal)(1.0 + frequency / 10.0));
        }

        private decimal PredictCustomerLifetime(Customer customer, BehaviorProfileDto behaviorProfile, List<CustomerInteraction> interactions)
        {
            var baseLifetime = 24m; // 24 mois par défaut
            var engagementFactor = (decimal)behaviorProfile.BehaviorScores.GetValueOrDefault("engagement", 0.5);
            var loyaltyFactor = GetLoyaltyMultiplier(customer.LoyaltyStats.CurrentTier) / 2;
            
            return baseLifetime * (1 + engagementFactor + loyaltyFactor);
        }

        private decimal CalculateEstimatedROI(string recommendationType, Customer customer, BehaviorProfileDto behaviorProfile)
        {
            // Calcul simplifié du ROI estimé
            var baseValue = customer.LoyaltyStats.TotalPointsEarned * 0.01m;
            var multiplier = recommendationType switch
            {
                "engagement" => 1.5m,
                "communication" => 1.2m,
                "loyalty" => 2.0m,
                _ => 1.0m
            };

            return baseValue * multiplier * 0.1m; // 10% d'amélioration estimée
        }

        private double CalculateEngagementScore(List<CustomerInteraction> interactions)
        {
            if (!interactions.Any()) return 0.0;
            
            var frequency = interactions.Count / Math.Max(1.0, (DateTime.UtcNow - interactions.Min(i => i.InteractionDate)).TotalDays / 30);
            var quality = interactions.Where(i => i.Rating.HasValue).Average(i => i.Rating) ?? 0.0;
            
            return Math.Min(1.0, (frequency / 10.0 + quality / 5.0) / 2.0);
        }

        private double CalculateSatisfactionScore(List<CustomerInteraction> interactions)
        {
            return interactions.Where(i => i.Rating.HasValue).Average(i => i.Rating) ?? 0.0 / 5.0;
        }

        private double CalculateLoyaltyScore(Customer customer)
        {
            var daysSinceRegistration = (DateTime.UtcNow - customer.RegistrationDate).TotalDays;
            var loyaltyPoints = customer.LoyaltyStats.TotalPoints;
            
            return Math.Min(1.0, (daysSinceRegistration / 365.0 + loyaltyPoints / 1000.0) / 2.0);
        }

        private double CalculateResponsivenessScore(List<CustomerInteraction> interactions)
        {
            var responseRate = interactions.Count(i => i.Outcome == InteractionOutcome.Positive) / (double)Math.Max(1, interactions.Count);
            return responseRate;
        }

        private double CalculateConsistencyScore(List<CustomerInteraction> interactions)
        {
            if (interactions.Count < 2) return 1.0;
            
            var ratings = interactions.Where(i => i.Rating.HasValue).Select(i => i.Rating.Value).ToList();
            if (ratings.Count < 2) return 1.0;
            
            var variance = ratings.Aggregate(0.0, (acc, val) => acc + Math.Pow(val - ratings.Average(), 2)) / ratings.Count;
            return Math.Max(0.0, 1.0 - variance / 25.0); // Normalisation sur une échelle de 1-5
        }

        private double CalculatePreferenceClarityScore(List<CustomerPreference> preferences)
        {
            if (!preferences.Any()) return 0.0;
            
            var avgConfidence = preferences.Average(p => p.Confidence);
            return avgConfidence;
        }

        // Méthodes prédictives avancées
        private async Task<Dictionary<string, double>> PredictChurnByTimeframe(Customer customer, BehaviorAnalysisResultDto analysis)
        {
            var baseChurnRisk = analysis.ChurnRiskScore;
            
            return new Dictionary<string, double>
            {
                ["30_days"] = baseChurnRisk * 0.3,
                ["90_days"] = baseChurnRisk * 0.6,
                ["180_days"] = baseChurnRisk * 0.8,
                ["365_days"] = baseChurnRisk
            };
        }

        private async Task<List<ValuePredictionDto>> PredictValueEvolution(Customer customer, BehaviorAnalysisResultDto analysis)
        {
            var currentValue = analysis.LifetimeValueEstimate;
            var growthFactor = (decimal)(1.0 - analysis.ChurnRiskScore * 0.5); // Facteur de croissance basé sur le risque de churn
            
            return new List<ValuePredictionDto>
            {
                new() { Month = 1, PredictedValue = currentValue * growthFactor },
                new() { Month = 3, PredictedValue = currentValue * growthFactor * 1.05m },
                new() { Month = 6, PredictedValue = currentValue * growthFactor * 1.1m },
                new() { Month = 12, PredictedValue = currentValue * growthFactor * 1.2m }
            };
        }

        private async Task<BehaviorEvolutionDto> PredictBehaviorEvolution(Customer customer, BehaviorAnalysisResultDto analysis)
        {
            return new BehaviorEvolutionDto
            {
                PredictedEngagementTrend = analysis.BehaviorProfile.BehaviorScores.GetValueOrDefault("engagement", 0.5) > 0.7 ? "Stable" : "Declining",
                PredictedSatisfactionTrend = analysis.ChurnRiskScore < 0.3 ? "Improving" : "At Risk",
                PredictedCommunicationEvolution = "Consistent with current patterns",
                KeyBehaviorChanges = GenerateKeyBehaviorChangePredictions(analysis)
            };
        }

        private async Task<OptimalContactStrategyDto> DetermineOptimalContactStrategy(Customer customer, BehaviorAnalysisResultDto analysis)
        {
            return new OptimalContactStrategyDto
            {
                PreferredChannel = analysis.BehaviorProfile.InteractionPreferences.OrderByDescending(kv => kv.Value).First().Key,
                OptimalTiming = DetermineOptimalContactTiming(analysis),
                RecommendedFrequency = analysis.BehaviorProfile.EngagementLevel switch
                {
                    "High" => "Weekly",
                    "Medium" => "Bi-weekly",
                    "Low" => "Monthly",
                    _ => "Monthly"
                },
                MessageTone = analysis.BehaviorProfile.PersonalityTraits.Contains("Positive") ? "Friendly" : "Professional",
                ContentRecommendations = GenerateContentRecommendations(analysis)
            };
        }

        private async Task<List<string>> GenerateNextActionsPredictions(Customer customer, BehaviorAnalysisResultDto analysis)
        {
            var actions = new List<string>();
            
            if (analysis.ChurnRiskScore > 0.7)
                actions.Add("Intervention de rétention immédiate");
            
            if (analysis.LifetimeValueEstimate > 1000)
                actions.Add("Proposition de programme VIP");
            
            if (analysis.BehaviorProfile.EngagementLevel == "Low")
                actions.Add("Campagne de réengagement personnalisée");
            
            return actions;
        }

        private async Task<List<RiskMitigationDto>> GenerateRiskMitigationStrategies(Customer customer, BehaviorAnalysisResultDto analysis)
        {
            var strategies = new List<RiskMitigationDto>();
            
            foreach (var riskFactor in analysis.BehaviorProfile.RiskFactors)
            {
                strategies.Add(new RiskMitigationDto
                {
                    RiskFactor = riskFactor,
                    MitigationStrategy = GenerateMitigationStrategy(riskFactor),
                    Priority = DetermineMitigationPriority(riskFactor, analysis.ChurnRiskScore),
                    EstimatedEffectiveness = CalculateStrategyEffectiveness(riskFactor)
                });
            }
            
            return strategies.OrderByDescending(s => s.Priority).ToList();
        }

        // Méthodes utilitaires pour les prédictions
        private List<string> GenerateKeyBehaviorChangePredictions(BehaviorAnalysisResultDto analysis)
        {
            var predictions = new List<string>();
            
            if (analysis.BehaviorProfile.BehaviorScores.GetValueOrDefault("engagement", 0.5) < 0.3)
                predictions.Add("Risque de diminution de l'engagement");
            
            if (analysis.ChurnRiskScore > 0.5)
                predictions.Add("Possible évolution vers un comportement de désengagement");
            
            return predictions;
        }

        private string DetermineOptimalContactTiming(BehaviorAnalysisResultDto analysis)
        {
            // Analyse des patterns temporels pour déterminer le meilleur moment de contact
            var activityPatterns = analysis.BehaviorProfile.ActivityPatterns;
            
            // Logique simplifiée - dans un vrai système, analyserait les patterns réels
            return "Matinée (9h-11h) en semaine";
        }

        private List<string> GenerateContentRecommendations(BehaviorAnalysisResultDto analysis)
        {
            var recommendations = new List<string>();
            
            if (analysis.BehaviorProfile.PersonalityTraits.Contains("Tech-Savvy"))
                recommendations.Add("Contenu digital interactif");
            
            if (analysis.BehaviorProfile.PersonalityTraits.Contains("Quality-Oriented"))
                recommendations.Add("Focus sur la qualité et l'excellence");
            
            return recommendations;
        }

        private string GenerateMitigationStrategy(string riskFactor)
        {
            return riskFactor switch
            {
                "Inactivité prolongée" => "Campagne de réactivation avec offre personnalisée",
                "Satisfaction faible" => "Contact proactif du service client pour résolution",
                "Fréquence élevée de plaintes" => "Analyse des causes racines et plan d'amélioration",
                _ => "Monitoring renforcé et suivi personnalisé"
            };
        }

        private string DetermineMitigationPriority(string riskFactor, double churnRisk)
        {
            if (churnRisk > 0.7) return "Critical";
            if (churnRisk > 0.4) return "High";
            return "Medium";
        }

        private double CalculateStrategyEffectiveness(string riskFactor)
        {
            // Efficacité estimée basée sur l'historique (simulée)
            return riskFactor switch
            {
                "Inactivité prolongée" => 0.75,
                "Satisfaction faible" => 0.85,
                "Fréquence élevée de plaintes" => 0.70,
                _ => 0.60
            };
        }

        // Méthodes d'inférence de personnalité
        private string InferCommunicationStyle(List<CustomerPreference> preferences, List<CustomerInteraction> interactions)
        {
            var digitalChannels = new[] { InteractionChannel.Email, InteractionChannel.SMS, InteractionChannel.Website };
            var digitalRatio = interactions.Count(i => digitalChannels.Contains(i.Channel)) / (double)Math.Max(1, interactions.Count);
            
            return digitalRatio > 0.6 ? "Digital" : "Traditional";
        }

        private string InferDecisionMakingStyle(List<CustomerInteraction> interactions)
        {
            var avgDuration = interactions.Where(i => i.Duration.HasValue).Average(i => i.Duration.Value);
            
            return avgDuration switch
            {
                > 30 => "Analytical",
                > 15 => "Considerate",
                _ => "Quick"
            };
        }

        private string InferServiceExpectations(List<CustomerInteraction> interactions)
        {
            var avgRating = interactions.Where(i => i.Rating.HasValue).Average(i => i.Rating) ?? 4.0;
            var hasComplaints = interactions.Any(i => i.InteractionType == InteractionType.Complaint);
            
            if (avgRating > 4.5 && !hasComplaints) return "High standards but reasonable";
            if (avgRating < 3.0 || hasComplaints) return "Demanding with specific expectations";
            return "Standard service expectations";
        }

        private List<string> InferDetailedPersonalityTraits(List<CustomerPreference> preferences, List<CustomerInteraction> interactions)
        {
            var traits = InferPersonalityTraits(preferences, interactions);
            
            // Ajout de traits plus détaillés
            if (interactions.Count > 15) traits.Add("Highly Engaged");
            if (preferences.Count > 10) traits.Add("Preference-Specific");
            
            var timeSpan = interactions.Max(i => i.InteractionDate) - interactions.Min(i => i.InteractionDate);
            if (timeSpan.TotalDays > 365) traits.Add("Long-term Customer");
            
            return traits.Distinct().ToList();
        }

        private string DeterminePreferredApproach(List<CustomerPreference> preferences, List<CustomerInteraction> interactions)
        {
            var hasServiceInteractions = interactions.Any(i => i.InteractionType == InteractionType.Support);
            var avgSatisfaction = interactions.Where(i => i.Rating.HasValue).Average(i => i.Rating) ?? 4.0;
            
            if (hasServiceInteractions && avgSatisfaction > 4.0) return "Proactive service approach";
            if (avgSatisfaction < 3.5) return "Problem-solving focused approach";
            return "Relationship-building approach";
        }

        private List<string> IdentifyMotivationFactors(List<CustomerPreference> preferences, List<CustomerInteraction> interactions)
        {
            var factors = new List<string>();
            
            if (preferences.Any(p => p.Category.Contains("Discount"))) factors.Add("Value-driven");
            if (preferences.Any(p => p.Category.Contains("Premium"))) factors.Add("Quality-focused");
            if (interactions.Count > 10) factors.Add("Relationship-oriented");
            
            return factors;
        }

        private List<string> GenerateCommunicationTips(List<CustomerPreference> preferences, List<CustomerInteraction> interactions)
        {
            var tips = new List<string>();
            
            var digitalRatio = interactions.Count(i => i.Channel == InteractionChannel.Email || i.Channel == InteractionChannel.SMS) / (double)Math.Max(1, interactions.Count);
            if (digitalRatio > 0.7) tips.Add("Privilégier les communications digitales");
            
            var avgResponseTime = interactions.Where(i => i.Duration.HasValue).Average(i => i.Duration.Value);
            if (avgResponseTime < 10) tips.Add("Être concis et direct");
            
            return tips;
        }

        // Méthodes d'analyse de patterns
        private string AnalyzeResponsePatterns(List<CustomerInteraction> interactions)
        {
            var positiveResponses = interactions.Count(i => i.Outcome == InteractionOutcome.Positive);
            var responseRate = positiveResponses / (double)Math.Max(1, interactions.Count);
            
            return responseRate switch
            {
                > 0.8 => "Highly responsive",
                > 0.6 => "Generally responsive",
                > 0.4 => "Moderately responsive",
                _ => "Low responsiveness"
            };
        }

        private string DeterminePrimaryCommunicationStyle(List<CustomerInteraction> interactions)
        {
            var channelGroups = interactions.GroupBy(i => i.Channel).ToDictionary(g => g.Key, g => g.Count());
            var primaryChannel = channelGroups.OrderByDescending(kv => kv.Value).FirstOrDefault().Key;
            
            return primaryChannel switch
            {
                InteractionChannel.Email or InteractionChannel.SMS or InteractionChannel.Website => "Digital",
                InteractionChannel.Phone => "Voice",
                InteractionChannel.InPerson => "Face-to-face",
                _ => "Multi-channel"
            };
        }

        private double CalculateInteractionFrequency(List<CustomerInteraction> interactions)
        {
            if (!interactions.Any()) return 0.0;
            
            var timeSpan = (DateTime.UtcNow - interactions.Min(i => i.InteractionDate)).TotalDays;
            return interactions.Count / Math.Max(1.0, timeSpan / 30.0); // Interactions par mois
        }

        private double CalculateInteractionQuality(List<CustomerInteraction> interactions)
        {
            return interactions.Where(i => i.Rating.HasValue).Average(i => i.Rating) ?? 0.0 / 5.0;
        }

        private double CalculateEngagementConsistency(List<CustomerInteraction> interactions)
        {
            if (interactions.Count < 3) return 1.0;
            
            // Calcul de la régularité des interactions
            var intervals = new List<double>();
            for (int i = 1; i < interactions.Count; i++)
            {
                intervals.Add((interactions[i].InteractionDate - interactions[i-1].InteractionDate).TotalDays);
            }
            
            var avgInterval = intervals.Average();
            var variance = intervals.Sum(x => Math.Pow(x - avgInterval, 2)) / intervals.Count;
            var consistency = 1.0 / (1.0 + Math.Sqrt(variance) / avgInterval);
            
            return Math.Min(1.0, consistency);
        }

        private double CalculatePreferenceStability(List<CustomerPreference> preferences)
        {
            if (!preferences.Any()) return 0.0;
            
            var recentChanges = preferences.Count(p => p.LastUpdated > DateTime.UtcNow.AddDays(-30));
            return 1.0 - (recentChanges / (double)preferences.Count);
        }

        private double CalculatePreferenceDiversity(List<CustomerPreference> preferences)
        {
            var categories = preferences.Select(p => p.Category).Distinct().Count();
            return Math.Min(1.0, categories / 10.0); // Normalisation sur 10 catégories max
        }

        private double CalculatePreferenceStrength(List<CustomerPreference> preferences)
        {
            return preferences.Average(p => p.Confidence);
        }

        private List<string> ExtractKeyBehaviorInsights(BehaviorProfileDto behaviorProfile, double churnRisk, decimal lifetimeValue)
        {
            var insights = new List<string>();
            
            if (churnRisk > 0.7) insights.Add("Client à risque élevé de churn - intervention urgente recommandée");
            if (lifetimeValue > 1000) insights.Add("Client à haute valeur - stratégie de rétention premium");
            if (behaviorProfile.EngagementLevel == "High") insights.Add("Client très engagé - potentiel d'ambassadeur de marque");
            if (behaviorProfile.PreferenceStability > 0.8) insights.Add("Préférences stables - personnalisation efficace possible");
            
            return insights;
        }

        private List<string> GenerateActionableRecommendations(double churnRisk, decimal lifetimeValue, BehaviorProfileDto behaviorProfile)
        {
            var actions = new List<string>();
            
            if (churnRisk > 0.5) actions.Add("Implémenter un programme de rétention ciblé");
            if (lifetimeValue > 500) actions.Add("Proposer des services et produits premium");
            if (behaviorProfile.EngagementLevel == "Low") actions.Add("Lancer une campagne de réengagement personnalisée");
            if (behaviorProfile.BehaviorScores.GetValueOrDefault("satisfaction", 0.5) < 0.6) actions.Add("Améliorer l'expérience client sur les points de friction identifiés");
            
            return actions;
        }

        private double CalculateAnalysisConfidence(int preferenceCount, int interactionCount)
        {
            var dataPoints = preferenceCount + interactionCount;
            return Math.Min(1.0, dataPoints / 50.0); // Confiance maximale avec 50+ points de données
        }

        private double AssessDataQuality(Customer customer, List<CustomerPreference> preferences, List<CustomerInteraction> interactions)
        {
            var completenessScore = CalculateDataCompleteness(customer, preferences, interactions);
            var recentnessScore = CalculateDataRecentness(preferences, interactions);
            var consistencyScore = CalculateDataConsistency(preferences, interactions);
            
            return (completenessScore + recentnessScore + consistencyScore) / 3.0;
        }

        private BehaviorTrendsDto AnalyzeBehaviorTrends(Customer customer, List<CustomerInteraction> interactions)
        {
            var recentInteractions = interactions.Where(i => i.InteractionDate > DateTime.UtcNow.AddDays(-30)).ToList();
            var olderInteractions = interactions.Where(i => i.InteractionDate <= DateTime.UtcNow.AddDays(-30) && i.InteractionDate > DateTime.UtcNow.AddDays(-60)).ToList();
            
            return new BehaviorTrendsDto
            {
                EngagementTrend = CalculateTrendDirection(recentInteractions.Count, olderInteractions.Count),
                SatisfactionTrend = CalculateTrendDirection(
                    recentInteractions.Where(i => i.Rating.HasValue).Average(i => i.Rating) ?? 0,
                    olderInteractions.Where(i => i.Rating.HasValue).Average(i => i.Rating) ?? 0),
                FrequencyTrend = CalculateTrendDirection(recentInteractions.Count, olderInteractions.Count),
                ChannelEvolution = AnalyzeChannelEvolution(recentInteractions, olderInteractions)
            };
        }

        private double CalculateDataCompleteness(Customer customer, List<CustomerPreference> preferences, List<CustomerInteraction> interactions)
        {
            var score = 0.0;
            var maxScore = 5.0;
            
            if (!string.IsNullOrEmpty(customer.PersonalInfo.FirstName)) score += 1.0;
            if (customer.PersonalInfo.DateOfBirth.HasValue) score += 1.0;
            if (!string.IsNullOrEmpty(customer.ContactInfo.Email)) score += 1.0;
            if (preferences.Any()) score += 1.0;
            if (interactions.Any()) score += 1.0;
            
            return score / maxScore;
        }

        private double CalculateDataRecentness(List<CustomerPreference> preferences, List<CustomerInteraction> interactions)
        {
            var recentPreferences = preferences.Count(p => p.LastUpdated > DateTime.UtcNow.AddDays(-90));
            var recentInteractions = interactions.Count(i => i.InteractionDate > DateTime.UtcNow.AddDays(-90));
            
            var totalRecent = recentPreferences + recentInteractions;
            var totalData = preferences.Count + interactions.Count;
            
            return totalData > 0 ? (double)totalRecent / totalData : 0.0;
        }

        private double CalculateDataConsistency(List<CustomerPreference> preferences, List<CustomerInteraction> interactions)
        {
            // Vérification de la cohérence entre préférences déclarées et comportement observé
            var digitalPreference = preferences.Any(p => p.Category.Contains("Digital"));
            var digitalBehavior = interactions.Count(i => i.Channel == InteractionChannel.Email || i.Channel == InteractionChannel.SMS) > interactions.Count / 2;
            
            var consistencyScore = digitalPreference == digitalBehavior ? 1.0 : 0.5;
            
            return consistencyScore;
        }

        private string CalculateTrendDirection(double recent, double older)
        {
            if (older == 0) return recent > 0 ? "Improving" : "Stable";
            
            var change = (recent - older) / older;
            return change switch
            {
                > 0.1 => "Improving",
                < -0.1 => "Declining",
                _ => "Stable"
            };
        }

        private Dictionary<string, string> AnalyzeChannelEvolution(List<CustomerInteraction> recent, List<CustomerInteraction> older)
        {
            var recentChannels = recent.GroupBy(i => i.Channel).ToDictionary(g => g.Key.ToString(), g => g.Count());
            var olderChannels = older.GroupBy(i => i.Channel).ToDictionary(g => g.Key.ToString(), g => g.Count());
            
            var evolution = new Dictionary<string, string>();
            
            foreach (var channel in recentChannels.Keys.Union(olderChannels.Keys))
            {
                var recentCount = recentChannels.GetValueOrDefault(channel, 0);
                var olderCount = olderChannels.GetValueOrDefault(channel, 0);
                
                evolution[channel] = CalculateTrendDirection(recentCount, olderCount);
            }
            
            return evolution;
        }
    }

    // Classes internes pour l'organisation des données d'analyse
    internal class CommunicationPatterns
    {
        public string PrimaryStyle { get; set; }
        public Dictionary<string, int> ChannelPreferences { get; set; } = new();
        public string ResponsePatterns { get; set; }
    }

    internal class EngagementPatterns
    {
        public string Level { get; set; }
        public double Frequency { get; set; }
        public double Quality { get; set; }
        public double Consistency { get; set; }
    }

    internal class PreferencePatterns
    {
        public double StabilityScore { get; set; }
        public double DiversityScore { get; set; }
        public double StrengthScore { get; set; }
    }

    internal class TemporalPatterns
    {
        public Dictionary<string, object> ActivityPatterns { get; set; } = new();
    }
}