# 🏗️ ARCHITECTURE DES BASES DE DONNÉES - NIESPRO ERP

## 📋 **VUE D'ENSEMBLE**

Notre système utilise **8 bases de données** distinctes suivant le pattern **Database-per-Service** des microservices.

---

## 🔐 **1. NiesPro_Auth** (Authentification & Sécurité)

**Rôle** : Gestion complète de l'authentification, autorisation et sécurité

### 📊 **Tables :**
```sql
👥 Users              -- Utilisateurs du système
├── Id (Guid)         -- Identifiant unique
├── Username          -- Nom d'utilisateur
├── Email             -- Email
├── PasswordHash      -- Mot de passe hashé
├── IsActive          -- Compte actif ?
├── CreatedAt         -- Date création
└── UpdatedAt         -- Dernière modification

🎭 Roles              -- Rôles (Admin, Manager, Vendeur, etc.)
├── Id (Guid)
├── Name              -- Nom du rôle
├── Description       -- Description
└── Permissions       -- Permissions associées

🔑 Permissions        -- Permissions système
├── Id (Guid)
├── Name              -- CREATE_PRODUCT, DELETE_ORDER, etc.
├── Resource          -- Sur quelle ressource
└── Action            -- Quelle action

📱 Devices            -- Appareils connectés
├── Id (Guid)
├── UserId            -- Propriétaire
├── DeviceId          -- ID unique appareil
├── DeviceName        -- Nom (iPhone de Jean)
├── LastSeen          -- Dernière connexion
└── IsActive          -- Appareil autorisé ?

🔒 UserSessions       -- Sessions actives
├── Id (Guid)
├── UserId            -- Utilisateur
├── DeviceId          -- Appareil
├── Token             -- Token JWT
├── ExpiresAt         -- Expiration
└── IsActive          -- Session active ?

📝 AuditLogs          -- Journal d'audit
├── Id (Guid)
├── UserId            -- Qui ?
├── Action            -- Quoi ?
├── Resource          -- Sur quoi ?
├── Timestamp         -- Quand ?
└── Details           -- Détails JSON
```

---

## 📦 **2. NiesPro_Product** (Catalogue Produits)

**Rôle** : Gestion complète du catalogue produits

### 📊 **Tables :**
```sql
🛍️ Products           -- Produits
├── Id (Guid)
├── Name              -- Nom produit
├── Description       -- Description
├── SKU               -- Code produit unique
├── Barcode           -- Code-barres
├── CategoryId        -- Catégorie
├── SupplierId        -- Fournisseur
├── PurchasePrice     -- Prix d'achat
├── SalePrice         -- Prix de vente
├── MinStock          -- Stock minimum
├── IsActive          -- Produit actif ?
└── Timestamps

📂 Categories         -- Catégories
├── Id (Guid)
├── Name              -- Boissons, Plats, Desserts
├── ParentId          -- Catégorie parent
├── Description
└── IsActive

🏭 Suppliers          -- Fournisseurs
├── Id (Guid)
├── Name              -- Nom fournisseur
├── Contact           -- Contact
├── Address           -- Adresse
├── Email
├── Phone
└── PaymentTerms      -- Conditions paiement

🏷️ ProductVariants    -- Variantes (Taille, Couleur)
├── Id (Guid)
├── ProductId         -- Produit parent
├── Name              -- "Grande taille"
├── SKU               -- SKU spécifique
├── PriceAdjustment   -- Ajustement prix
└── StockQuantity

💰 ProductPrices      -- Historique des prix
├── Id (Guid)
├── ProductId
├── Price             -- Prix
├── ValidFrom         -- Valide à partir de
├── ValidTo           -- Valide jusqu'à
└── Reason            -- Raison changement
```

---

## 📦 **3. NiesPro_Stock** (Gestion Stock & Inventaire)

**Rôle** : Suivi des stocks, mouvements et inventaires

### 📊 **Tables :**
```sql
📊 StockLevels        -- Niveaux de stock actuels
├── Id (Guid)
├── ProductId         -- Produit
├── WarehouseId       -- Entrepôt/Local
├── Quantity          -- Quantité actuelle
├── ReservedQuantity  -- Quantité réservée
├── MinLevel          -- Niveau minimum
├── MaxLevel          -- Niveau maximum
└── LastUpdated

🏢 Warehouses         -- Entrepôts/Locaux
├── Id (Guid)
├── Name              -- "Magasin Principal"
├── Address           -- Adresse
├── Type              -- STORE, WAREHOUSE, KITCHEN
├── IsActive
└── ManagerId         -- Responsable

📋 StockMovements     -- Mouvements de stock
├── Id (Guid)
├── ProductId         -- Produit
├── WarehouseId       -- Lieu
├── MovementType      -- IN, OUT, TRANSFER, ADJUSTMENT
├── Quantity          -- Quantité (+/-)
├── Reason            -- Raison (Vente, Réception, Perte)
├── ReferenceId       -- Référence (OrderId, etc.)
├── UserId            -- Qui a fait le mouvement
└── Timestamp

🔍 StockAdjustments   -- Ajustements d'inventaire
├── Id (Guid)
├── ProductId
├── WarehouseId
├── QuantityBefore    -- Quantité avant
├── QuantityAfter     -- Quantité après
├── Difference        -- Différence
├── Reason            -- Raison ajustement
├── UserId            -- Qui
└── AdjustmentDate

⚠️ StockAlerts        -- Alertes stock
├── Id (Guid)
├── ProductId
├── WarehouseId
├── AlertType         -- LOW_STOCK, OUT_OF_STOCK, OVERSTOCK
├── Threshold         -- Seuil déclenché
├── CurrentLevel      -- Niveau actuel
├── IsResolved        -- Alerte résolue ?
└── CreatedAt
```

---

## 🛒 **4. NiesPro_Order** (Commandes & Factures)

**Rôle** : Gestion des commandes clients et fournisseurs

### 📊 **Tables :**
```sql
📝 Orders             -- Commandes
├── Id (Guid)
├── OrderNumber       -- Numéro commande unique
├── CustomerId        -- Client (peut être null pour walk-in)
├── OrderType         -- SALE, PURCHASE, RETURN
├── Status            -- PENDING, CONFIRMED, SHIPPED, DELIVERED, CANCELLED
├── OrderDate         -- Date commande
├── DeliveryDate      -- Date livraison prévue
├── SubTotal          -- Sous-total
├── TaxAmount         -- Montant taxes
├── DiscountAmount    -- Remise
├── TotalAmount       -- Total final
├── PaymentStatus     -- PENDING, PARTIAL, PAID, REFUNDED
├── UserId            -- Vendeur/Responsable
├── Notes             -- Notes
└── Timestamps

📋 OrderItems         -- Lignes de commande
├── Id (Guid)
├── OrderId           -- Commande parent
├── ProductId         -- Produit
├── ProductName       -- Nom (snapshot)
├── Quantity          -- Quantité
├── UnitPrice         -- Prix unitaire (snapshot)
├── DiscountPercent   -- Remise ligne
├── LineTotal         -- Total ligne
└── Notes

🧾 Invoices           -- Factures
├── Id (Guid)
├── OrderId           -- Commande liée
├── InvoiceNumber     -- Numéro facture
├── InvoiceDate       -- Date facture
├── DueDate           -- Date échéance
├── Status            -- DRAFT, SENT, PAID, OVERDUE, CANCELLED
├── TotalAmount       -- Montant total
├── PaidAmount        -- Montant payé
├── RemainingAmount   -- Solde
└── PaymentTerms

🔄 OrderStatusHistory -- Historique statuts
├── Id (Guid)
├── OrderId
├── PreviousStatus    -- Ancien statut
├── NewStatus         -- Nouveau statut
├── ChangedBy         -- Qui a changé
├── ChangeReason      -- Raison
├── Timestamp
└── Notes
```

---

## 💳 **5. NiesPro_Payment** (Gestion Paiements)

**Rôle** : Traitement des paiements et transactions financières

### 📊 **Tables :**
```sql
💰 Payments           -- Paiements
├── Id (Guid)
├── OrderId           -- Commande liée (optionnel)
├── InvoiceId         -- Facture liée (optionnel)
├── PaymentNumber     -- Numéro paiement unique
├── Amount            -- Montant
├── PaymentMethod     -- CASH, CARD, TRANSFER, CHECK, MOBILE
├── PaymentDate       -- Date paiement
├── Status            -- PENDING, COMPLETED, FAILED, REFUNDED
├── Reference         -- Référence externe (transaction bancaire)
├── ProcessedBy       -- Qui a traité
├── Notes
└── Timestamps

💳 PaymentMethods     -- Méthodes de paiement configurées
├── Id (Guid)
├── Name              -- "Visa", "Espèces", "Virement"
├── Type              -- CASH, CARD, ELECTRONIC
├── Provider          -- "Stripe", "PayPal", etc.
├── IsActive          -- Méthode active ?
├── Configuration     -- Config JSON (clés API, etc.)
└── Fees              -- Frais associés

🔄 PaymentRefunds     -- Remboursements
├── Id (Guid)
├── OriginalPaymentId -- Paiement original
├── RefundAmount      -- Montant remboursé
├── Reason            -- Raison remboursement
├── RefundDate        -- Date remboursement
├── ProcessedBy       -- Qui a traité
├── Status            -- PENDING, COMPLETED, FAILED
└── Reference

📊 DailyTransactions  -- Transactions journalières (cache)
├── Id (Guid)
├── Date              -- Date
├── TotalSales        -- Total ventes
├── TotalRefunds      -- Total remboursements
├── NetAmount         -- Montant net
├── TransactionCount  -- Nombre transactions
├── CashAmount        -- Montant espèces
├── CardAmount        -- Montant cartes
└── LastCalculated    -- Dernière maj
```

---

## 👥 **6. NiesPro_Customer** (Gestion Clientèle)

**Rôle** : CRM - Gestion des clients et relations

### 📊 **Tables :**
```sql
👤 Customers          -- Clients
├── Id (Guid)
├── CustomerNumber    -- Numéro client unique
├── FirstName         -- Prénom
├── LastName          -- Nom
├── Email             -- Email
├── Phone             -- Téléphone
├── DateOfBirth       -- Date naissance
├── CustomerType      -- INDIVIDUAL, BUSINESS, VIP
├── Status            -- ACTIVE, INACTIVE, BLOCKED
├── TotalSpent        -- Total dépensé (cache)
├── LastOrderDate     -- Dernière commande
├── LoyaltyPoints     -- Points fidélité
├── PreferredContact  -- Email, Phone, SMS
├── Notes
└── Timestamps

🏠 CustomerAddresses  -- Adresses clients
├── Id (Guid)
├── CustomerId        -- Client
├── Type              -- BILLING, SHIPPING, HOME, WORK
├── Street            -- Rue
├── City              -- Ville
├── PostalCode        -- Code postal
├── Country           -- Pays
├── IsDefault         -- Adresse par défaut ?
└── IsActive

🎁 LoyaltyProgram     -- Programme fidélité
├── Id (Guid)
├── CustomerId        -- Client
├── PointsEarned      -- Points gagnés
├── PointsUsed        -- Points utilisés
├── PointsBalance     -- Solde points
├── TierLevel         -- Niveau (Bronze, Silver, Gold)
├── LastEarned        -- Derniers points gagnés
└── LastUsed          -- Derniers points utilisés

📞 CustomerInteractions -- Interactions client
├── Id (Guid)
├── CustomerId        -- Client
├── InteractionType   -- CALL, EMAIL, VISIT, COMPLAINT, SUPPORT
├── Subject           -- Sujet
├── Description       -- Description
├── Status            -- OPEN, IN_PROGRESS, RESOLVED, CLOSED
├── Priority          -- LOW, MEDIUM, HIGH, URGENT
├── AssignedTo        -- Assigné à (UserId)
├── CreatedBy         -- Créé par
└── Timestamps
```

---

## 🍽️ **7. NiesPro_Restaurant** (Spécifique Restaurant)

**Rôle** : Fonctionnalités spécifiques à la restauration

### 📊 **Tables :**
```sql
🪑 Tables              -- Tables du restaurant
├── Id (Guid)
├── TableNumber       -- Numéro table
├── Capacity          -- Nombre de places
├── Status            -- AVAILABLE, OCCUPIED, RESERVED, OUT_OF_ORDER
├── ZoneId            -- Zone (terrasse, salle, bar)
├── Location          -- Position (x,y pour plan)
├── CurrentOrderId    -- Commande en cours
└── LastCleaned       -- Dernière désinfection

🏠 Zones               -- Zones du restaurant
├── Id (Guid)
├── Name              -- "Terrasse", "Salle principale"
├── Description       -- Description
├── Capacity          -- Capacité totale
├── IsActive          -- Zone active ?
└── ServerId          -- Serveur assigné

📅 Reservations       -- Réservations
├── Id (Guid)
├── CustomerId        -- Client (optionnel)
├── CustomerName      -- Nom si pas dans base
├── CustomerPhone     -- Téléphone
├── TableId           -- Table réservée
├── ReservationDate   -- Date/heure réservation
├── PartySize         -- Nombre de personnes
├── Status            -- CONFIRMED, SEATED, COMPLETED, CANCELLED, NO_SHOW
├── SpecialRequests   -- Demandes spéciales
├── CreatedBy         -- Qui a pris la réservation
└── Timestamps

🍽️ MenuCategories     -- Catégories menu
├── Id (Guid)
├── Name              -- "Entrées", "Plats", "Desserts"
├── DisplayOrder      -- Ordre affichage
├── IsActive          -- Catégorie active ?
├── IconUrl           -- Icône
└── Description

👨‍🍳 Kitchen           -- Gestion cuisine
├── Id (Guid)
├── OrderId           -- Commande
├── OrderItemId       -- Ligne spécifique
├── Status            -- PENDING, PREPARING, READY, SERVED
├── CookId            -- Cuisinier assigné
├── EstimatedTime     -- Temps préparation estimé
├── ActualTime        -- Temps réel
├── StartedAt         -- Début préparation
├── CompletedAt       -- Fin préparation
└── Notes             -- Notes cuisine

🕐 ServiceHistory     -- Historique service
├── Id (Guid)
├── TableId           -- Table
├── OrderId           -- Commande
├── ServerId          -- Serveur
├── ServiceStart      -- Début service
├── ServiceEnd        -- Fin service
├── CustomerRating    -- Note client
├── Tips              -- Pourboires
└── Notes
```

---

## 📊 **8. NiesPro_Log** (Logs & Audit Global)

**Rôle** : Centralisation des logs système et business

### 📊 **Tables :**
```sql
📝 SystemLogs         -- Logs système
├── Id (Guid)
├── Level             -- DEBUG, INFO, WARN, ERROR, FATAL
├── Logger            -- Source du log
├── Message           -- Message
├── Exception         -- Exception (si erreur)
├── Properties        -- Propriétés additionnelles (JSON)
├── Timestamp         -- Horodatage
├── MachineName       -- Machine source
├── UserId            -- Utilisateur (si applicable)
└── RequestId         -- ID requête (traçabilité)

🔍 BusinessAudit      -- Audit métier
├── Id (Guid)
├── EntityType        -- Type entité (Order, Product, etc.)
├── EntityId          -- ID entité
├── Action            -- CREATE, UPDATE, DELETE, VIEW
├── UserId            -- Qui
├── Timestamp         -- Quand
├── OldValues         -- Anciennes valeurs (JSON)
├── NewValues         -- Nouvelles valeurs (JSON)
├── ChangeReason      -- Raison changement
└── IpAddress         -- Adresse IP

⚡ PerformanceLogs    -- Logs performance
├── Id (Guid)
├── OperationType     -- Type opération
├── OperationName     -- Nom opération
├── Duration          -- Durée (ms)
├── Success           -- Succès ?
├── ErrorMessage      -- Message erreur
├── Parameters        -- Paramètres (JSON)
├── ResultSize        -- Taille résultat
├── Timestamp         -- Horodatage
└── UserId            -- Utilisateur

🔄 IntegrationLogs    -- Logs intégrations
├── Id (Guid)
├── ServiceName       -- Service appelé
├── Operation         -- Opération
├── Request           -- Requête (JSON)
├── Response          -- Réponse (JSON)
├── Duration          -- Durée
├── Success           -- Succès ?
├── ErrorCode         -- Code erreur
├── Timestamp         -- Horodatage
└── CorrelationId     -- ID corrélation
```

---

## 🔗 **RELATIONS INTER-BASES**

### 🔄 **Communications entre Microservices :**

```
Auth ←→ Toutes les bases   (Authentification)
Order ←→ Product          (Validation produits)
Order ←→ Customer         (Données client)  
Order ←→ Stock            (Réservation stock)
Order ←→ Payment          (Traitement paiement)
Restaurant ←→ Order       (Commandes restaurant)
Restaurant ←→ Customer    (Réservations)
Log ←← Toutes les bases   (Centralisation logs)
```

### 📡 **Événements Distribués :**
- **OrderCreated** → Stock (réservation), Payment (traitement)
- **PaymentCompleted** → Order (mise à jour statut), Stock (confirmation)
- **ProductUpdated** → Stock (ajustement niveaux)
- **CustomerRegistered** → Loyalty (création compte fidélité)

Cette architecture garantit :
- ✅ **Isolation** des données par domaine métier
- ✅ **Scalabilité** indépendante par service
- ✅ **Sécurité** avec contrôle d'accès granulaire
- ✅ **Traçabilité** complète des opérations
- ✅ **Performance** avec bases spécialisées