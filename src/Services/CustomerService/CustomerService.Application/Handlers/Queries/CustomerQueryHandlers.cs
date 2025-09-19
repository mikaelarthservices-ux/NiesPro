using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using CustomerService.Domain.Entities;
using CustomerService.Domain.Repositories;
using CustomerService.Application.DTOs.Customer;
using CustomerService.Application.Queries.Customer;
using CustomerService.Domain.ValueObjects;
using CustomerService.Domain.Enums;

namespace CustomerService.Application.Handlers.Queries
{
    // ========================================================================================
    // CUSTOMER QUERY HANDLERS - RÉCUPÉRATION SOPHISTIQUÉE DES DONNÉES CLIENT
    // ========================================================================================

    /// <summary>
    /// Handler pour la récupération d'un client par son ID
    /// </summary>
    public class GetCustomerByIdQueryHandler : IRequestHandler<GetCustomerByIdQuery, CustomerDetailDto>
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetCustomerByIdQueryHandler> _logger;
        private readonly IValidator<GetCustomerByIdQuery> _validator;

        public GetCustomerByIdQueryHandler(
            ICustomerRepository customerRepository,
            IMapper mapper,
            ILogger<GetCustomerByIdQueryHandler> logger,
            IValidator<GetCustomerByIdQuery> validator)
        {
            _customerRepository = customerRepository;
            _mapper = mapper;
            _logger = logger;
            _validator = validator;
        }

        public async Task<CustomerDetailDto> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Récupération du client par ID : {CustomerId}", request.CustomerId);

            // Validation
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                _logger.LogError("Échec de validation pour GetCustomerById : {Errors}", errors);
                throw new ValidationException($"Erreurs de validation : {errors}");
            }

            // Récupération du client avec tous les détails
            var customer = await _customerRepository.GetByIdWithDetailsAsync(request.CustomerId);
            if (customer == null)
            {
                _logger.LogWarning("Client non trouvé : {CustomerId}", request.CustomerId);
                return null;
            }

            // Mappage vers DTO avec enrichissement
            var customerDto = _mapper.Map<CustomerDetailDto>(customer);

            // Enrichissement avec données calculées
            if (request.IncludeAnalytics)
            {
                customerDto.Analytics = await GetCustomerAnalytics(customer);
            }

            if (request.IncludeInteractions)
            {
                customerDto.RecentInteractions = await GetRecentInteractions(customer.Id);
            }

            if (request.IncludePreferences)
            {
                customerDto.Preferences = await GetCustomerPreferences(customer.Id);
            }

            _logger.LogInformation("Client récupéré avec succès : {CustomerId}", request.CustomerId);

            return customerDto;
        }

        private async Task<CustomerAnalyticsDto> GetCustomerAnalytics(Customer customer)
        {
            // Calculs analytiques en temps réel
            return new CustomerAnalyticsDto
            {
                CustomerLifetimeValue = customer.CalculateLifetimeValue(),
                ChurnRiskScore = await customer.CalculateChurnRiskAsync(),
                EngagementScore = customer.CalculateEngagementScore(),
                SatisfactionScore = customer.CalculateAverageSatisfaction(),
                TotalInteractions = customer.GetTotalInteractionCount(),
                LastInteractionDate = customer.GetLastInteractionDate(),
                PreferredChannels = customer.GetPreferredChannels()
            };
        }

        private async Task<List<CustomerInteractionSummaryDto>> GetRecentInteractions(Guid customerId)
        {
            // Récupération des interactions récentes (30 derniers jours)
            var interactions = await _customerRepository.GetRecentInteractionsAsync(customerId, 30);
            return _mapper.Map<List<CustomerInteractionSummaryDto>>(interactions);
        }

        private async Task<List<CustomerPreferenceSummaryDto>> GetCustomerPreferences(Guid customerId)
        {
            // Récupération des préférences actives
            var preferences = await _customerRepository.GetActivePreferencesAsync(customerId);
            return _mapper.Map<List<CustomerPreferenceSummaryDto>>(preferences);
        }
    }

    /// <summary>
    /// Handler pour la récupération d'un client par email
    /// </summary>
    public class GetCustomerByEmailQueryHandler : IRequestHandler<GetCustomerByEmailQuery, CustomerDetailDto>
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetCustomerByEmailQueryHandler> _logger;
        private readonly IValidator<GetCustomerByEmailQuery> _validator;

        public GetCustomerByEmailQueryHandler(
            ICustomerRepository customerRepository,
            IMapper mapper,
            ILogger<GetCustomerByEmailQueryHandler> logger,
            IValidator<GetCustomerByEmailQuery> validator)
        {
            _customerRepository = customerRepository;
            _mapper = mapper;
            _logger = logger;
            _validator = validator;
        }

        public async Task<CustomerDetailDto> Handle(GetCustomerByEmailQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Récupération du client par email : {Email}", request.Email);

            // Validation
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                _logger.LogError("Échec de validation pour GetCustomerByEmail : {Errors}", errors);
                throw new ValidationException($"Erreurs de validation : {errors}");
            }

            // Récupération par email
            var customer = await _customerRepository.GetByEmailAsync(request.Email);
            if (customer == null)
            {
                _logger.LogWarning("Client non trouvé pour l'email : {Email}", request.Email);
                return null;
            }

            var customerDto = _mapper.Map<CustomerDetailDto>(customer);

            _logger.LogInformation("Client récupéré par email avec succès : {CustomerId}", customer.Id);

            return customerDto;
        }
    }

    /// <summary>
    /// Handler pour la recherche avancée de clients
    /// </summary>
    public class SearchCustomersQueryHandler : IRequestHandler<SearchCustomersQuery, PagedResult<CustomerSummaryDto>>
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<SearchCustomersQueryHandler> _logger;
        private readonly IValidator<SearchCustomersQuery> _validator;

        public SearchCustomersQueryHandler(
            ICustomerRepository customerRepository,
            IMapper mapper,
            ILogger<SearchCustomersQueryHandler> logger,
            IValidator<SearchCustomersQuery> validator)
        {
            _customerRepository = customerRepository;
            _mapper = mapper;
            _logger = logger;
            _validator = validator;
        }

        public async Task<PagedResult<CustomerSummaryDto>> Handle(SearchCustomersQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Recherche de clients - Terme : '{SearchTerm}', Page : {Page}, Taille : {PageSize}", 
                request.SearchTerm, request.PageNumber, request.PageSize);

            // Validation
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                _logger.LogError("Échec de validation pour la recherche de clients : {Errors}", errors);
                throw new ValidationException($"Erreurs de validation : {errors}");
            }

            // Construction des critères de recherche
            var searchCriteria = new CustomerSearchCriteria
            {
                SearchTerm = request.SearchTerm,
                Status = request.Status,
                RegistrationDateFrom = request.RegistrationDateFrom,
                RegistrationDateTo = request.RegistrationDateTo,
                MinLoyaltyPoints = request.MinLoyaltyPoints,
                MaxLoyaltyPoints = request.MaxLoyaltyPoints,
                LoyaltyTier = request.LoyaltyTier,
                SegmentIds = request.SegmentIds,
                City = request.City,
                Country = request.Country,
                PreferredLanguage = request.PreferredLanguage,
                HasInteractionsSince = request.HasInteractionsSince,
                MinSatisfactionRating = request.MinSatisfactionRating,
                Tags = request.Tags
            };

            // Exécution de la recherche
            var (customers, totalCount) = await _customerRepository.SearchAsync(
                searchCriteria,
                request.PageNumber,
                request.PageSize,
                request.SortBy,
                request.SortDescending);

            // Mappage vers DTOs
            var customerDtos = _mapper.Map<List<CustomerSummaryDto>>(customers);

            // Enrichissement avec métriques si demandé
            if (request.IncludeMetrics)
            {
                foreach (var dto in customerDtos)
                {
                    var customer = customers.First(c => c.Id == dto.Id);
                    dto.Metrics = new CustomerMetricsDto
                    {
                        TotalInteractions = customer.GetTotalInteractionCount(),
                        AverageSatisfaction = customer.CalculateAverageSatisfaction(),
                        DaysSinceLastInteraction = customer.GetDaysSinceLastInteraction(),
                        EngagementLevel = customer.GetEngagementLevel()
                    };
                }
            }

            var result = new PagedResult<CustomerSummaryDto>
            {
                Items = customerDtos,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
            };

            _logger.LogInformation("Recherche terminée - {Count} clients trouvés sur {Total} total", 
                customerDtos.Count, totalCount);

            return result;
        }
    }

    /// <summary>
    /// Handler pour les statistiques globales des clients
    /// </summary>
    public class GetCustomerStatsQueryHandler : IRequestHandler<GetCustomerStatsQuery, CustomerStatsDto>
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly ICustomerAnalyticsRepository _analyticsRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetCustomerStatsQueryHandler> _logger;
        private readonly IValidator<GetCustomerStatsQuery> _validator;

        public GetCustomerStatsQueryHandler(
            ICustomerRepository customerRepository,
            ICustomerAnalyticsRepository analyticsRepository,
            IMapper mapper,
            ILogger<GetCustomerStatsQueryHandler> logger,
            IValidator<GetCustomerStatsQuery> validator)
        {
            _customerRepository = customerRepository;
            _analyticsRepository = analyticsRepository;
            _mapper = mapper;
            _logger = logger;
            _validator = validator;
        }

        public async Task<CustomerStatsDto> Handle(GetCustomerStatsQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Calcul des statistiques clients - Période : {StartDate} à {EndDate}", 
                request.StartDate, request.EndDate);

            // Validation
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                _logger.LogError("Échec de validation pour les statistiques clients : {Errors}", errors);
                throw new ValidationException($"Erreurs de validation : {errors}");
            }

            // Calcul des statistiques de base
            var totalCustomers = await _customerRepository.GetTotalCountAsync();
            var activeCustomers = await _customerRepository.GetActiveCountAsync();
            var newCustomersInPeriod = await _customerRepository.GetNewCustomersCountAsync(request.StartDate, request.EndDate);

            // Statistiques de fidélité
            var loyaltyStats = await _customerRepository.GetLoyaltyStatsAsync();

            // Statistiques par statut
            var statusDistribution = await _customerRepository.GetStatusDistributionAsync();

            // Statistiques géographiques
            var geographicDistribution = await _customerRepository.GetGeographicDistributionAsync(request.TopCountries);

            // Statistiques d'engagement
            var engagementStats = await _analyticsRepository.GetEngagementStatsAsync(request.StartDate, request.EndDate);

            // Tendances
            var trends = request.IncludeTrends ? await CalculateTrends(request.StartDate, request.EndDate) : null;

            var statsDto = new CustomerStatsDto
            {
                GeneratedAt = DateTime.UtcNow,
                PeriodStart = request.StartDate,
                PeriodEnd = request.EndDate,
                TotalCustomers = totalCustomers,
                ActiveCustomers = activeCustomers,
                InactiveCustomers = totalCustomers - activeCustomers,
                NewCustomersInPeriod = newCustomersInPeriod,
                CustomerGrowthRate = totalCustomers > 0 ? (newCustomersInPeriod / (double)totalCustomers) * 100 : 0,
                LoyaltyStats = loyaltyStats,
                StatusDistribution = statusDistribution,
                GeographicDistribution = geographicDistribution,
                EngagementStats = engagementStats,
                Trends = trends
            };

            _logger.LogInformation("Statistiques calculées - Total : {Total}, Actifs : {Active}, Nouveaux : {New}", 
                totalCustomers, activeCustomers, newCustomersInPeriod);

            return statsDto;
        }

        private async Task<CustomerTrendsDto> CalculateTrends(DateTime startDate, DateTime endDate)
        {
            var previousPeriodStart = startDate.AddDays(-(endDate - startDate).TotalDays);
            
            var currentPeriodStats = await _analyticsRepository.GetPeriodStatsAsync(startDate, endDate);
            var previousPeriodStats = await _analyticsRepository.GetPeriodStatsAsync(previousPeriodStart, startDate);

            return new CustomerTrendsDto
            {
                NewCustomersTrend = CalculateGrowthRate(currentPeriodStats.NewCustomers, previousPeriodStats.NewCustomers),
                EngagementTrend = CalculateGrowthRate(currentPeriodStats.AvgEngagement, previousPeriodStats.AvgEngagement),
                SatisfactionTrend = CalculateGrowthRate(currentPeriodStats.AvgSatisfaction, previousPeriodStats.AvgSatisfaction),
                RetentionTrend = CalculateGrowthRate(currentPeriodStats.RetentionRate, previousPeriodStats.RetentionRate)
            };
        }

        private double CalculateGrowthRate(double current, double previous)
        {
            if (previous == 0) return current > 0 ? 100 : 0;
            return ((current - previous) / previous) * 100;
        }
    }

    /// <summary>
    /// Handler pour récupérer le profil complet d'un client
    /// </summary>
    public class GetCustomerProfileQueryHandler : IRequestHandler<GetCustomerProfileQuery, CustomerProfileDto>
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly ICustomerPreferenceRepository _preferenceRepository;
        private readonly ICustomerInteractionRepository _interactionRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetCustomerProfileQueryHandler> _logger;
        private readonly IValidator<GetCustomerProfileQuery> _validator;

        public GetCustomerProfileQueryHandler(
            ICustomerRepository customerRepository,
            ICustomerPreferenceRepository preferenceRepository,
            ICustomerInteractionRepository interactionRepository,
            IMapper mapper,
            ILogger<GetCustomerProfileQueryHandler> logger,
            IValidator<GetCustomerProfileQuery> validator)
        {
            _customerRepository = customerRepository;
            _preferenceRepository = preferenceRepository;
            _interactionRepository = interactionRepository;
            _mapper = mapper;
            _logger = logger;
            _validator = validator;
        }

        public async Task<CustomerProfileDto> Handle(GetCustomerProfileQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Récupération du profil client : {CustomerId}", request.CustomerId);

            // Validation
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                _logger.LogError("Échec de validation pour GetCustomerProfile : {Errors}", errors);
                throw new ValidationException($"Erreurs de validation : {errors}");
            }

            // Récupération du client
            var customer = await _customerRepository.GetByIdWithDetailsAsync(request.CustomerId);
            if (customer == null)
            {
                _logger.LogWarning("Client non trouvé pour le profil : {CustomerId}", request.CustomerId);
                return null;
            }

            // Construction du profil complet
            var profile = _mapper.Map<CustomerProfileDto>(customer);

            // Enrichissement avec données détaillées
            profile.Preferences = await GetDetailedPreferences(request.CustomerId);
            profile.InteractionHistory = await GetInteractionHistory(request.CustomerId, request.IncludeFullHistory);
            profile.SegmentMemberships = await GetSegmentMemberships(request.CustomerId);
            profile.LoyaltyDetails = await GetLoyaltyDetails(request.CustomerId);
            profile.BehaviorInsights = await GetBehaviorInsights(request.CustomerId);

            _logger.LogInformation("Profil client récupéré avec succès : {CustomerId}", request.CustomerId);

            return profile;
        }

        private async Task<List<CustomerPreferenceDetailDto>> GetDetailedPreferences(Guid customerId)
        {
            var preferences = await _preferenceRepository.GetByCustomerIdAsync(customerId);
            return _mapper.Map<List<CustomerPreferenceDetailDto>>(preferences);
        }

        private async Task<CustomerInteractionHistoryDto> GetInteractionHistory(Guid customerId, bool includeFullHistory)
        {
            var interactions = includeFullHistory 
                ? await _interactionRepository.GetByCustomerIdAsync(customerId)
                : await _interactionRepository.GetRecentByCustomerIdAsync(customerId, DateTime.UtcNow.AddMonths(-6));

            return new CustomerInteractionHistoryDto
            {
                TotalInteractions = interactions.Count(),
                Interactions = _mapper.Map<List<CustomerInteractionDetailDto>>(interactions),
                InteractionsByType = interactions.GroupBy(i => i.InteractionType)
                    .ToDictionary(g => g.Key.ToString(), g => g.Count()),
                InteractionsByChannel = interactions.GroupBy(i => i.Channel)
                    .ToDictionary(g => g.Key.ToString(), g => g.Count()),
                AverageRating = interactions.Where(i => i.Rating.HasValue).Average(i => i.Rating) ?? 0
            };
        }

        private async Task<List<CustomerSegmentMembershipDto>> GetSegmentMemberships(Guid customerId)
        {
            var memberships = await _customerRepository.GetSegmentMembershipsAsync(customerId);
            return _mapper.Map<List<CustomerSegmentMembershipDto>>(memberships);
        }

        private async Task<CustomerLoyaltyDetailsDto> GetLoyaltyDetails(Guid customerId)
        {
            var loyaltyDetails = await _customerRepository.GetLoyaltyDetailsAsync(customerId);
            return _mapper.Map<CustomerLoyaltyDetailsDto>(loyaltyDetails);
        }

        private async Task<CustomerBehaviorInsightsDto> GetBehaviorInsights(Guid customerId)
        {
            var insights = await _customerRepository.GetBehaviorInsightsAsync(customerId);
            return _mapper.Map<CustomerBehaviorInsightsDto>(insights);
        }
    }

    /// <summary>
    /// Handler pour récupérer les clients ayant des anniversaires
    /// </summary>
    public class GetCustomerBirthdaysQueryHandler : IRequestHandler<GetCustomerBirthdaysQuery, List<CustomerBirthdayDto>>
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetCustomerBirthdaysQueryHandler> _logger;
        private readonly IValidator<GetCustomerBirthdaysQuery> _validator;

        public GetCustomerBirthdaysQueryHandler(
            ICustomerRepository customerRepository,
            IMapper mapper,
            ILogger<GetCustomerBirthdaysQueryHandler> logger,
            IValidator<GetCustomerBirthdaysQuery> validator)
        {
            _customerRepository = customerRepository;
            _mapper = mapper;
            _logger = logger;
            _validator = validator;
        }

        public async Task<List<CustomerBirthdayDto>> Handle(GetCustomerBirthdaysQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Récupération des anniversaires - Date de début : {StartDate}, Fin : {EndDate}", 
                request.StartDate, request.EndDate);

            // Validation
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                _logger.LogError("Échec de validation pour les anniversaires : {Errors}", errors);
                throw new ValidationException($"Erreurs de validation : {errors}");
            }

            // Récupération des clients avec anniversaires dans la période
            var customers = await _customerRepository.GetCustomersWithBirthdaysAsync(request.StartDate, request.EndDate);

            var birthdayDtos = _mapper.Map<List<CustomerBirthdayDto>>(customers);

            // Enrichissement avec informations personnalisées
            foreach (var dto in birthdayDtos)
            {
                var customer = customers.First(c => c.Id == dto.CustomerId);
                dto.Age = CalculateAge(customer.PersonalInfo.DateOfBirth.Value);
                dto.LoyaltyTier = customer.LoyaltyStats.CurrentTier;
                dto.PreferredCommunicationChannel = customer.GetPreferredCommunicationChannel();
                dto.SuggestedOffers = await GenerateBirthdayOffers(customer);
            }

            _logger.LogInformation("Anniversaires récupérés : {Count} clients", birthdayDtos.Count);

            return birthdayDtos;
        }

        private int CalculateAge(DateTime birthDate)
        {
            var today = DateTime.Today;
            var age = today.Year - birthDate.Year;
            if (birthDate.Date > today.AddYears(-age)) age--;
            return age;
        }

        private async Task<List<string>> GenerateBirthdayOffers(Customer customer)
        {
            // Génération d'offres personnalisées basées sur le profil client
            var offers = new List<string>();

            if (customer.LoyaltyStats.CurrentTier == "Gold" || customer.LoyaltyStats.CurrentTier == "Platinum")
            {
                offers.Add("Réduction de 20% sur votre prochaine commande");
                offers.Add("Dessert gratuit offert");
            }
            else
            {
                offers.Add("Réduction de 15% sur votre prochaine commande");
                offers.Add("Boisson gratuite offerte");
            }

            offers.Add("Points de fidélité doublés pendant 1 semaine");

            return offers;
        }
    }

    /// <summary>
    /// Handler pour récupérer les clients inactifs
    /// </summary>
    public class GetInactiveCustomersQueryHandler : IRequestHandler<GetInactiveCustomersQuery, PagedResult<CustomerSummaryDto>>
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetInactiveCustomersQueryHandler> _logger;
        private readonly IValidator<GetInactiveCustomersQuery> _validator;

        public GetInactiveCustomersQueryHandler(
            ICustomerRepository customerRepository,
            IMapper mapper,
            ILogger<GetInactiveCustomersQueryHandler> logger,
            IValidator<GetInactiveCustomersQuery> validator)
        {
            _customerRepository = customerRepository;
            _mapper = mapper;
            _logger = logger;
            _validator = validator;
        }

        public async Task<PagedResult<CustomerSummaryDto>> Handle(GetInactiveCustomersQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Récupération des clients inactifs - Seuil : {Days} jours", request.InactiveDays);

            // Validation
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                _logger.LogError("Échec de validation pour les clients inactifs : {Errors}", errors);
                throw new ValidationException($"Erreurs de validation : {errors}");
            }

            // Calcul de la date limite d'inactivité
            var inactiveThreshold = DateTime.UtcNow.AddDays(-request.InactiveDays);

            // Récupération des clients inactifs
            var (customers, totalCount) = await _customerRepository.GetInactiveCustomersAsync(
                inactiveThreshold,
                request.PageNumber,
                request.PageSize,
                request.SortBy,
                request.SortDescending);

            var customerDtos = _mapper.Map<List<CustomerSummaryDto>>(customers);

            // Enrichissement avec informations d'inactivité
            foreach (var dto in customerDtos)
            {
                var customer = customers.First(c => c.Id == dto.Id);
                dto.DaysSinceLastActivity = customer.GetDaysSinceLastInteraction();
                dto.ChurnRiskLevel = customer.GetChurnRiskLevel();
                dto.RecommendedActions = GenerateReactivationActions(customer);
            }

            var result = new PagedResult<CustomerSummaryDto>
            {
                Items = customerDtos,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
            };

            _logger.LogInformation("Clients inactifs récupérés : {Count} sur {Total}", customerDtos.Count, totalCount);

            return result;
        }

        private List<string> GenerateReactivationActions(Customer customer)
        {
            var actions = new List<string>();

            var daysSinceLastActivity = customer.GetDaysSinceLastInteraction();

            if (daysSinceLastActivity > 180)
            {
                actions.Add("Campagne de réactivation intensive");
                actions.Add("Offre de retour avec réduction significative");
            }
            else if (daysSinceLastActivity > 90)
            {
                actions.Add("Email de réengagement personnalisé");
                actions.Add("Proposition d'offre exclusive");
            }
            else
            {
                actions.Add("Rappel de points de fidélité disponibles");
                actions.Add("Invitation à découvrir les nouveautés");
            }

            return actions;
        }
    }

    /// <summary>
    /// Handler pour vérifier l'unicité d'un email
    /// </summary>
    public class CheckEmailUniquenessQueryHandler : IRequestHandler<CheckEmailUniquenessQuery, bool>
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly ILogger<CheckEmailUniquenessQueryHandler> _logger;
        private readonly IValidator<CheckEmailUniquenessQuery> _validator;

        public CheckEmailUniquenessQueryHandler(
            ICustomerRepository customerRepository,
            ILogger<CheckEmailUniquenessQueryHandler> logger,
            IValidator<CheckEmailUniquenessQuery> validator)
        {
            _customerRepository = customerRepository;
            _logger = logger;
            _validator = validator;
        }

        public async Task<bool> Handle(CheckEmailUniquenessQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Vérification de l'unicité de l'email : {Email}", request.Email);

            // Validation
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                _logger.LogError("Échec de validation pour CheckEmailUniqueness : {Errors}", errors);
                throw new ValidationException($"Erreurs de validation : {errors}");
            }

            // Vérification de l'unicité
            var isUnique = await _customerRepository.IsEmailUniqueAsync(request.Email, request.ExcludeCustomerId);

            _logger.LogInformation("Unicité de l'email {Email} : {IsUnique}", request.Email, isUnique);

            return isUnique;
        }
    }
}